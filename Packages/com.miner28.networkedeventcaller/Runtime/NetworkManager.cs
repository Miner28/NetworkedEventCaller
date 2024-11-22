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

        [HideInInspector] public GameObject[] pool;
        [HideInInspector] public bool debug;

        public SyncChannel syncChannel = SyncChannel.Channel1;
        DataList _bufferQueue = new DataList();
        [NonSerialized] public bool networkingActive = true;
        [NonSerialized] bool _shouldVoidEvents = false;
        
        bool _runOnce;
        


        void OnEnable()
        {
            if (_runOnce) return;
            
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
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            var playerCaller = player.GetPlayerObjectOfType<NetworkedEventCaller>();
            if (playerCaller != null)
            {
                playerCaller.SetupCaller();
            }
            
            if (player.isLocal)
            {
                var netCaller = player.GetPlayerObjectOfType<NetworkedEventCaller>();
                if (netCaller != null)
                {
                    _myCaller = netCaller;
                    OnCallerAssigned();
                }
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