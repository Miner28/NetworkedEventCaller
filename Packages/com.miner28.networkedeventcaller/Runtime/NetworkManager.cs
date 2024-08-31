using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Miner28.UdonUtils.Network
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class NetworkManager : UdonSharpBehaviour
    {
        const int EventProcessingSpeed = 25;
        
        private NetworkedEventCaller _myCaller;
        [HideInInspector] public string methodInfosJson;
        internal DataDictionary methodInfos;
        internal DataList methodInfosKeys;
        internal DataList methodInfosValues;


        #region PoolManager

        [HideInInspector] public NetworkInterface[] sceneInterfaces;
        [HideInInspector] public uint[] sceneInterfacesIds;
        [HideInInspector] public NetworkedEventCaller[] sceneCallers;

        [HideInInspector] public GameObject[] pool;
        [HideInInspector] public bool debug;

        [UdonSynced] private int[] poolOwners = new int[100];
        [UdonSynced] private int[] toClean = new int[0];

        public SyncChannel syncChannel = SyncChannel.Channel1;
        DataList _bufferQueue = new DataList();
        [NonSerialized] public bool networkingActive = true;
        [NonSerialized] bool _shouldVoidEvents = false;
        
        bool _runOnce;
        


        void OnEnable()
        {
            if (_runOnce) return;

            poolOwners = new int[pool.Length];
            _runOnce = true;
            if (string.IsNullOrEmpty(methodInfosJson))
            {
                Log("MethodInfosJson is empty");
                methodInfos = new DataDictionary();
            }
            else
            {
                VRCJson.TryDeserializeFromJson(methodInfosJson, out var json);
                methodInfos = json.DataDictionary;
            }

            methodInfosKeys = methodInfos.GetKeys();
            methodInfosKeys.Sort();


            foreach (var @interface in sceneInterfaces)
            {
                @interface.SetupInterface();
            }

            foreach (var caller in sceneCallers)
            {
                caller.SetupCaller();
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer.isMaster)
            {
                if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

                GameObject obj = null;

                for (int i = 0; i < poolOwners.Length; i++)
                {
                    if (poolOwners[i] == 0)
                    {
                        obj = pool[i];
                    }
                }

                _Cleanup();

                if (obj == null)
                {
                    Log("No free objects in pool - Unable to assign Caller");
                    return;
                }

                var oId = Array.IndexOf(pool, obj);
                poolOwners[oId] = player.playerId;
                obj.SetActive(true);

                RequestSerialization();

                if (player.isLocal)
                {
                    _myCaller = sceneCallers[oId];
                    Networking.SetOwner(Networking.LocalPlayer, _myCaller.gameObject);
                    Log("I have got myself Caller - Master");
                    OnCallerAssigned();
                }
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(Networking.LocalPlayer)) return;
            int userId = player.playerId;

            var index = Array.IndexOf(poolOwners, userId);
            if (index == -1) return;

            toClean = toClean.Add(index);

            if (Networking.LocalPlayer.isMaster)
            {
                if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

                poolOwners[index] = 0;
                pool[index].SetActive(false);
                Networking.SetOwner(Networking.LocalPlayer, pool[index]);

                RequestSerialization();

                SendCustomEventDelayedFrames(nameof(_Cleanup), 5);
            }
        }

        public override void OnDeserialization()
        {
            if (!Utilities.IsValid(_myCaller))
            {
                int myId = Networking.LocalPlayer.playerId;

                for (var i = 0; i < poolOwners.Length; i++)
                {
                    if (myId == poolOwners[i])
                    {
                        _myCaller = sceneCallers[i];

                        sceneCallers[i].gameObject.SetActive(true);

                        Networking.SetOwner(Networking.LocalPlayer, _myCaller.gameObject);
                        Log($"I have got myself Caller - NonMaster {_myCaller == null}");
                        OnCallerAssigned();
                    }
                }
            }

            for (int i = 0; i < poolOwners.Length; i++)
            {
                pool[i].SetActive(poolOwners[i] != 0);
            }
        }

        public void _Cleanup()
        {
            if (toClean.Length == 0) return;

            for (var index = 0; index < toClean.Length; index++)
            {
                var i = toClean[index];

                var o = pool[i];
                var networkedEventCaller = sceneCallers[i];
                if (networkedEventCaller.syncBuffer.Length == 0)
                {
                    Log($"Returning {i}");
                    poolOwners[i] = 0;
                    pool[i].SetActive(false);
                    toClean = toClean.Remove(index);
                    index--;
                }
                else
                {
                    Log($"Cleaning up {i}");
                    networkedEventCaller.syncBuffer = new byte[0];

                    Networking.SetOwner(Networking.LocalPlayer, o);
                    networkedEventCaller.RequestSerialization();
                }
            }

            SendCustomEventDelayedFrames(nameof(_Cleanup), 5);
        }
        
        /// <summary>
        /// Toggles the state of Receiving Events, if voidEvents is true, it will void any received events
        /// NOTE: VRCPlayerAPI may behave weirdly if voidEvents is false. If player has left by the time the event is processed, the VRCPlayerAPI will be invalid and will cause networking inconsistencies
        /// </summary>
        /// <param name="state">Networking state</param>
        /// <param name="voidEvents">Void Events Received</param>
        public void ToggleNetworking(bool state, bool voidEvents)
        {
            _shouldVoidEvents = voidEvents;
            networkingActive = state;
            
            if (state)
            {
                _HandleNetworkResumed();
            }
        }

        /// <summary>
        /// Handles events when networking is paused
        /// </summary>
        internal void HandlePaused(VRCPlayerApi sender, byte[] data)
        {
            if (_shouldVoidEvents) return;
            
            byte[] buffer = new byte[data.Length];
            Array.Copy(data, buffer, data.Length);
            var bufferData = new DataDictionary();
            bufferData["sender"] = new DataToken(sender);
            bufferData["data"] = new DataToken(buffer);
            _bufferQueue.Add(bufferData);
        }

        public void _HandleNetworkResumed()
        {
            if (_bufferQueue.Count == 0) return;
            
            for (int i = 0; i < EventProcessingSpeed; i++)
            {
                if (_bufferQueue.Count == 0) break;
                var data = _bufferQueue[0].DataDictionary;
                _bufferQueue.RemoveAt(0);
                var sender = (VRCPlayerApi)data["sender"].Reference;
                var buffer = (byte[])data["data"].Reference;
                _myCaller.HandleDeserialization(sender, buffer);
            }
            
            if (_bufferQueue.Count > 0)
            {
                SendCustomEventDelayedFrames(nameof(_HandleNetworkResumed), 1);
            }
        }

        private void Log(object log)
        {
            if (debug) Debug.Log($"<color=#FFFF00>PoolManager</color> {log}");
        }

        private void Log(string log)
        {
            if (debug) Debug.Log($"<color=#FFFF00>PoolManager</color> {log}");
        }

        private void OnCallerAssigned()
        {
            foreach (var @interface in sceneInterfaces)
            {
                Log($"Assigning Interface {@interface.networkID}");
                @interface._caller = _myCaller;
                @interface.OnCallerAssigned();
            }
        }

        #endregion
    }
}