using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Miner28.UdonUtils.Network
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)][DefaultExecutionOrder(Int32.MinValue + 1000000)]
    public class NetworkManager : UdonSharpBehaviour
    {
        const int EventProcessingSpeed = 25;

        NetworkedEventCaller _myCaller;
        public DataDictionary methodInfos;
        internal DataList methodInfosKeys;
        internal DataList methodInfosValues;


        #region PoolManager

        [HideInInspector] public NetworkInterface[] sceneInterfaces;
        [HideInInspector] public uint[] sceneInterfacesIds;

        [HideInInspector] public GameObject[] pool;
        [HideInInspector] public bool debug;

        public SyncChannel syncChannel = SyncChannel.Channel1;
        readonly DataList _bufferQueue = new DataList();
        [NonSerialized] public bool networkingActive = true;
        [NonSerialized] bool _shouldVoidEvents;


        void Start()
        {
            if (methodInfos == null)
            {
                Log("FATAL: MethodInfos is empty, this should never happen. Please report this!");
                methodInfos = new DataDictionary();
            }

            methodInfosKeys = methodInfos.GetKeys();
            methodInfosKeys.Sort();


            foreach (var @interface in sceneInterfaces) @interface.SetupInterface();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            var playerCaller = player.GetPlayerObjectOfType<NetworkedEventCaller>();
            if (playerCaller != null) playerCaller.SetupCaller();

            if (player.isLocal)
            {
                var netCaller = player.GetPlayerObjectOfType<NetworkedEventCaller>();
                if (netCaller != null && _myCaller == null)
                {
                    _myCaller = netCaller;
                    OnCallerAssigned();
                }
            }
        }
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            var playerCaller = player.GetPlayerObjectOfType<NetworkedEventCaller>();
            if (playerCaller != null) playerCaller.SetupCaller();

            if (player.isLocal)
            {
                var netCaller = player.GetPlayerObjectOfType<NetworkedEventCaller>();
                if (netCaller != null && _myCaller == null)
                {
                    _myCaller = netCaller;
                    OnCallerAssigned();
                }
            }
        }
        
        public void BackwardsRegister(NetworkedEventCaller caller)
        {
            if (Networking.GetOwner(caller.gameObject).isLocal && _myCaller == null)
            {
                _myCaller = caller;
                OnCallerAssigned();
            }
        }
        
        /// <summary>
        ///     Toggles the state of Receiving Events, if voidEvents is true, it will void any received events
        ///     NOTE: VRCPlayerAPI may behave weirdly if voidEvents is false. If player has left by the time the event is
        ///     processed, the VRCPlayerAPI will be invalid and will cause networking inconsistencies
        /// </summary>
        /// <param name="state">Networking state</param>
        /// <param name="voidEvents">Void Events Received</param>
        public void ToggleNetworking(bool state, bool voidEvents)
        {
            _shouldVoidEvents = voidEvents;
            networkingActive = state;

            if (state) _HandleNetworkResumed();
        }

        /// <summary>
        ///     Handles events when networking is paused
        /// </summary>
        internal void HandlePaused(VRCPlayerApi sender, byte[] data)
        {
            if (_shouldVoidEvents) return;

            var buffer = new byte[data.Length];
            Array.Copy(data, buffer, data.Length);
            var bufferData = new DataDictionary();
            bufferData["sender"] = new DataToken(sender);
            bufferData["data"] = new DataToken(buffer);
            _bufferQueue.Add(bufferData);
        }

        public void _HandleNetworkResumed()
        {
            if (_bufferQueue.Count == 0) return;

            for (var i = 0; i < EventProcessingSpeed; i++)
            {
                if (_bufferQueue.Count == 0) break;
                var data = _bufferQueue[0].DataDictionary;
                _bufferQueue.RemoveAt(0);
                var sender = (VRCPlayerApi)data["sender"].Reference;
                var buffer = (byte[])data["data"].Reference;
                _myCaller.HandleDeserialization(sender, buffer);
            }

            if (_bufferQueue.Count > 0) SendCustomEventDelayedFrames(nameof(_HandleNetworkResumed), 1);
        }


        void Log(object log)
        {
            if (debug) Debug.Log($"<color=#FFFF00>PoolManager</color> {log}");
        }

        void Log(string log)
        {
            if (debug) Debug.Log($"<color=#FFFF00>PoolManager</color> {log}");
        }

        void OnCallerAssigned()
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