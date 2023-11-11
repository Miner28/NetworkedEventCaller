using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Miner28.UdonUtils.Network
{
    public class NetworkInterface : UdonSharpBehaviour
    {
        [Header("Network Interface ID")] public uint networkID = 0;
        
        
        [HideInInspector] public NetworkManager networkManagerInternal;
        internal NetworkedEventCaller _caller;
        private string _udonClassName;

        private bool _callerAssigned;

        public void SendMethodNetworked(string methodName, SyncTarget target, params DataToken[] paramTokens)
        {
            if (!_callerAssigned)
            {
                if (_caller == null)
                {
                    Debug.LogError($"Caller not assigned unable to send method - {methodName}");
                    return;
                }

                _callerAssigned = true;
            }
            
            _caller._PrepareSend(Convert.ToUInt32(target), $"{_udonClassName}.{methodName}", networkID, paramTokens);
        }
        
         public void SendMethodNetworked(string methodName, VRCPlayerApi target, params DataToken[] paramTokens)
        {
            if (!_callerAssigned)
            {
                if (_caller == null)
                {
                    Debug.LogError($"Caller not assigned unable to send method - {methodName}");
                    return;
                }

                _callerAssigned = true;
            }
            
            _caller._PrepareSend(Convert.ToUInt32(target.playerId + 100), $"{_udonClassName}.{methodName}", networkID, paramTokens);
        }
        
        

        internal void SetupInterface()
        {
            _udonClassName = GetUdonTypeName();
        }
        

        public virtual void OnCallerAssigned()
        {
        }

    }
}