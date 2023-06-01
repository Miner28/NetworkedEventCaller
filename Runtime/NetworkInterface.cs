using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Miner28.UdonUtils.Network
{
    public class NetworkInterface : UdonSharpBehaviour
    {
        [Header("Network Interface ID")] public int networkID = 0;

        private int _paramOffset;
        
        [NonSerialized] public DataToken[] localTokens;

        [HideInInspector] public NetworkManager networkManager;
        [HideInInspector] public NetworkedEventCaller caller;

        private bool _callerAssigned;

        protected void SendMethodNetworked(string methodName, SyncTarget target, params DataToken[] paramTokens)
        {
            if (!_callerAssigned)
            {
                if (caller == null)
                {
                    Debug.LogError($"Caller not assigned unable to send method - {methodName}");
                    return;
                }

                _callerAssigned = true;
            }

            caller._SendMethod(target, methodName, networkID, paramTokens);
        }

        public void OnMethodReceived(string method)
        {
            _paramOffset = 0;
            SendCustomEvent(method);
        }

        #region DeSerialization
        protected bool GetBool() => localTokens[_paramOffset++].Boolean;
        protected byte GetByte() => localTokens[_paramOffset++].Byte;
        protected sbyte GetSByte() => localTokens[_paramOffset++].SByte;
        protected short GetShort() => localTokens[_paramOffset++].Short;
        protected ushort GetUShort() => localTokens[_paramOffset++].UShort;
        protected int GetInt() => localTokens[_paramOffset++].Int;
        protected uint GetUInt() => localTokens[_paramOffset++].UInt;
        protected long GetLong() => localTokens[_paramOffset++].Long;
        protected ulong GetULong() => localTokens[_paramOffset++].ULong;
        protected float GetFloat() => localTokens[_paramOffset++].Float;
        protected double GetDouble() => localTokens[_paramOffset++].Double;
        protected string GetString() => localTokens[_paramOffset++].String;
        protected decimal GetDecimal() => (decimal) localTokens[_paramOffset++].Reference;
        protected VRCPlayerApi GetPlayer() => (VRCPlayerApi) localTokens[_paramOffset++].Reference;
        protected Color GetColor() => (Color) localTokens[_paramOffset++].Reference;
        protected Color32 GetColor32() => (Color32) localTokens[_paramOffset++].Reference;
        protected Vector2 GetVector2() => (Vector2) localTokens[_paramOffset++].Reference;
        protected Vector2Int GetVector2Int() => (Vector2Int) localTokens[_paramOffset++].Reference;
        protected Vector3 GetVector3() => (Vector3) localTokens[_paramOffset++].Reference;
        protected Vector3Int GetVector3Int() => (Vector3Int) localTokens[_paramOffset++].Reference;
        protected Vector4 GetVector4() => (Vector4) localTokens[_paramOffset++].Reference;
        protected Quaternion GetQuaternion() => (Quaternion) localTokens[_paramOffset++].Reference;
        protected DateTime GetDateTime() => (DateTime) localTokens[_paramOffset++].Reference;
        
        protected bool[] GetBoolArray() => (bool[]) localTokens[_paramOffset++].Reference;
        protected byte[] GetByteArray() => (byte[]) localTokens[_paramOffset++].Reference;
        protected sbyte[] GetSByteArray() => (sbyte[]) localTokens[_paramOffset++].Reference;
        protected short[] GetShortArray() => (short[]) localTokens[_paramOffset++].Reference;
        protected ushort[] GetUShortArray() => (ushort[]) localTokens[_paramOffset++].Reference;
        protected int[] GetIntArray() => (int[]) localTokens[_paramOffset++].Reference;
        protected uint[] GetUIntArray() => (uint[]) localTokens[_paramOffset++].Reference;
        protected long[] GetLongArray() => (long[]) localTokens[_paramOffset++].Reference;
        protected ulong[] GetULongArray() => (ulong[]) localTokens[_paramOffset++].Reference;
        protected float[] GetFloatArray() => (float[]) localTokens[_paramOffset++].Reference;
        protected double[] GetDoubleArray() => (double[]) localTokens[_paramOffset++].Reference;
        protected string[] GetStringArray() => (string[]) localTokens[_paramOffset++].Reference;
        protected decimal[] GetDecimalArray() => (decimal[]) localTokens[_paramOffset++].Reference;
        protected VRCPlayerApi[] GetPlayerArray() => (VRCPlayerApi[]) localTokens[_paramOffset++].Reference;
        protected Color[] GetColorArray() => (Color[]) localTokens[_paramOffset++].Reference;
        protected Color32[] GetColor32Array() => (Color32[]) localTokens[_paramOffset++].Reference;
        protected Vector2[] GetVector2Array() => (Vector2[]) localTokens[_paramOffset++].Reference;
        protected Vector2Int[] GetVector2IntArray() => (Vector2Int[]) localTokens[_paramOffset++].Reference;
        protected Vector3[] GetVector3Array() => (Vector3[]) localTokens[_paramOffset++].Reference;
        protected Vector3Int[] GetVector3IntArray() => (Vector3Int[]) localTokens[_paramOffset++].Reference;
        protected Vector4[] GetVector4Array() => (Vector4[]) localTokens[_paramOffset++].Reference;
        protected Quaternion[] GetQuaternionArray() => (Quaternion[]) localTokens[_paramOffset++].Reference;
        
        



        #endregion

        public virtual void OnCallerAssigned()
        {
        }
        
    }
}