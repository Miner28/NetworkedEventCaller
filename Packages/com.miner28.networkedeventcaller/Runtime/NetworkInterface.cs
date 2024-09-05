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
        public VRCPlayerApi eventSenderPlayer;
        VRCPlayerApi _netInterfaceLocalPlayer;

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

            eventSenderPlayer = _netInterfaceLocalPlayer;
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

            if (!Utilities.IsValid(target))
            {
                Debug.LogError($"Invalid target unable to send method - {methodName}");
                return;
            }

            eventSenderPlayer = _netInterfaceLocalPlayer;
            _caller._PrepareSend(Convert.ToUInt32(target.playerId + 100), $"{_udonClassName}.{methodName}", networkID,
                paramTokens);
        }

        public void SendMethodNetworked(string methodName, SyncChannel channel, params DataToken[] paramTokens) {
            if (!_callerAssigned) {
                if (_caller == null) {
                    Debug.LogError($"Caller not assigned unable to send method - {methodName}");
                    return;
                }

                _callerAssigned = true;
            }

            eventSenderPlayer = _netInterfaceLocalPlayer;
            _caller._PrepareSend(Convert.ToUInt32(channel), $"{_udonClassName}.{methodName}", networkID, paramTokens);
        }


        internal void SetupInterface()
        {
            _udonClassName = GetUdonTypeName();
            _netInterfaceLocalPlayer = Networking.LocalPlayer;
        }


        public virtual void OnCallerAssigned()
        {
        }
    }
}