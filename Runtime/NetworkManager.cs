using System;
using Miner28.UdonUtils;
using Miner28.UdonUtils.Network;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace Miner28.UdonUtils.Network
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class NetworkManager : UdonSharpBehaviour
    {
        private NetworkedEventCaller _myCaller;

        #region PoolManager

        [HideInInspector] public NetworkInterface[] sceneInterfaces;
        [HideInInspector] public int[] sceneInterfacesIds;

        [HideInInspector] public VRCObjectPool pool;
        [HideInInspector] public bool debug;

        [UdonSynced] private int[] poolOwners = new int[100];
        [UdonSynced] private int[] toClean = new int[0];

        private bool runOnce;

        private void OnEnable()
        {
            if (runOnce) return;

            poolOwners = new int[pool.Pool.Length];
            runOnce = true;
        }


        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer.isMaster)
            {
                if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

                var obj = pool.TryToSpawn();

                _Cleanup();

                poolOwners[Array.IndexOf(pool.Pool, obj)] = player.playerId;

                RequestSerialization();

                if (player.isLocal)
                {
                    _myCaller = obj.GetComponent<NetworkedEventCaller>();
                    Networking.SetOwner(Networking.LocalPlayer, _myCaller.gameObject);
                    Log("I have got myself Caller - Master");
                    OnCallerAssigned();
                }
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            int userId = player.playerId;

            var index = Array.IndexOf(poolOwners, userId);
            if (index == -1) return;

            toClean = toClean.Add(index);

            if (Networking.LocalPlayer.isMaster)
            {
                if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

                poolOwners[index] = 0;
                Networking.SetOwner(Networking.LocalPlayer, pool.Pool[index]);

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
                        _myCaller = pool.Pool[i].GetComponent<NetworkedEventCaller>();
                        Networking.SetOwner(Networking.LocalPlayer, _myCaller.gameObject);
                        Log($"I have got myself Caller - NonMaster {_myCaller == null}");
                        OnCallerAssigned();
                    }
                }
            }
        }


        public void _Cleanup()
        {
            if (toClean.Length == 0) return;

            for (var index = 0; index < toClean.Length; index++)
            {
                var i = toClean[index];

                var o = pool.Pool[i];
                var networkedEventCaller = o.GetComponent<NetworkedEventCaller>();
                if (networkedEventCaller.methodTarget == "")
                {
                    Log($"Returning {i}");
                    pool.Return(o);
                    toClean = toClean.Remove(index);
                    index--;
                }
                else
                {
                    Log($"Cleaning up {i}");
                    networkedEventCaller.methodTarget = "";

                    Networking.SetOwner(Networking.LocalPlayer, o);
                    networkedEventCaller.RequestSerialization();
                }
            }

            SendCustomEventDelayedFrames(nameof(_Cleanup), 5);
        }

        private static void Log(string log)
        {
            Debug.Log($"<color=#FFFF00>PoolManager</color> {log}");
        }

        private void OnCallerAssigned()
        {
            foreach (var @interface in sceneInterfaces)
            {
                Debug.Log(@interface.networkID);
                @interface.networkManagerInternal = this;
                @interface.caller = _myCaller;
                @interface.OnCallerAssigned();
            }
        }

        #endregion



    }
}