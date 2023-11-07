#define NETCALLER_USE_VARIABLE_SERIALIZATION
#define NETCALLER_DEBUG
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace Miner28.UdonUtils.Network
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class NetworkedEventCaller : UdonSharpBehaviour
    {
        [HideInInspector] public NetworkManager networkManager;
        [HideInInspector] public NetworkInterface[] sceneInterfaces;
        [HideInInspector] public int[] sceneInterfacesIds;

        private DataDictionary _methodInfos;
        private DataList _methodInfosKeys;


        private bool _debug;
        private bool _startRun;


        #region Constants

        private readonly Type[] _typeMap =
        {
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(string),
            typeof(decimal),
            typeof(VRCPlayerApi),
            typeof(Color),
            typeof(Color32),
            typeof(Vector2),
            typeof(Vector2Int),
            typeof(Vector3),
            typeof(Vector3Int),
            typeof(Vector4),
            typeof(Quaternion),
            typeof(DateTime),
            //Arrays
            typeof(bool[]),
            typeof(byte[]),
            typeof(sbyte[]),
            typeof(short[]),
            typeof(ushort[]),
            typeof(int[]),
            typeof(uint[]),
            typeof(long[]),
            typeof(ulong[]),
            typeof(float[]),
            typeof(double[]),
            typeof(decimal[]),
            typeof(string[]),
            typeof(VRCPlayerApi[]),
            typeof(Color[]),
            typeof(Color32[]),
            typeof(Vector2[]),
            typeof(Vector2Int[]),
            typeof(Vector3[]),
            typeof(Vector3Int[]),
            typeof(Vector4[]),
            typeof(Quaternion[]),
        };

        private readonly TokenType[] _tokenMap =
        {
            TokenType.Boolean,
            TokenType.Byte,
            TokenType.SByte,
            TokenType.Short,
            TokenType.UShort,
            TokenType.Int,
            TokenType.UInt,
            TokenType.Long,
            TokenType.ULong,
            TokenType.Float,
            TokenType.Double,
            TokenType.String,
        };

        private const int
            Bit8 = 8,
            Bit16 = 16,
            Bit24 = 24,
            Bit32 = 32,
            Bit40 = 40,
            Bit48 = 48,
            Bit56 = 56;

        private const uint
            FloatSignBit = 0x80000000,
            FloatExpMask = 0x7F800000,
            FloatFracMask = 0x007FFFFF;

        private const ulong
            DoubleSignBit = 0x8000000000000000,
            DoubleExpMask = 0x7FF0000000000000,
            DoubleFracMask = 0x000FFFFFFFFFFFFF;

        private const byte
            _0x80 = 0x80,
            _0xC0 = 0xC0,
            _0x3F = 0x3F,
            _0xF0 = 0xF0,
            _0xF8 = 0xF8,
            _0x07 = 0x07,
            _0x1F = 0x1F,
            _0x0F = 0x0F,
            _0xE0 = 0xE0,
            _0xFF = 0xFF,
            BYTE_ONE = 1,
            BYTE_ZERO = 0;

        private const int
            _0xFFFF = 0xFFFF,
            _0x7FFF = 0x7FFF;

        private const byte
            Int16V = (byte) Types.Int16V,
            UInt16V = (byte) Types.UInt16V,
            Int32V = (byte) Types.Int32V,
            UInt32V = (byte) Types.UInt32V,
            Int64V = (byte) Types.Int64V,
            UInt64V = (byte) Types.UInt64V;
        #endregion

        #region StorageVariables

        private bool _tmpBool;
        private byte _byteTmp;
        private sbyte _sbyteValue;
        private short _int16Value;
        private ushort _uint16Value;
        private int _int32TMP;
        private int _int32TMP2;
        private int _int32TMP3;
        private int _int32TMP4;
        private int _iter;
        private uint _uint32Value;
        private uint _uintTmp;
        private long _int64Value;
        private ulong _uint64Value;
        private long _exp;
        private long _doubleFracMask;
        private float _singleValue;
        private float _singleValue2;
        private float _singleValue3;
        private float _singleValue4;
        private double _doubleValue;
        private string _stringData;
        private Vector2 _vector2;
        private Vector2Int _vector2Int;
        private Vector3 _vector3;
        private Vector3Int _vector3Int;
        private Vector4 _vector4;
        private Quaternion _quaternion;
        private Color _color;
        private Color32 _color32;
        private VRCPlayerApi _player;

        private bool[] _boolA = new bool[0];
        private byte[] _tempBytes;
        private byte[] _byteA = new byte[0];
        private sbyte[] _sbyteA = new sbyte[0];
        private short[] _int16A = new short[0];
        private ushort[] _uint16A = new ushort[0];
        private int[] _int32A = new int[0];
        private uint[] _uint32A = new uint[0];
        private long[] _int64A = new long[0];
        private ulong[] _uint64A = new ulong[0];
        private float[] _singleA = new float[0];
        private double[] _doubleA = new double[0];
        private decimal[] _decimalA = new decimal[0];
        private string[] _stringA = new string[0];
        private Vector2[] _vector2A = new Vector2[0];
        private Vector2Int[] _vector2IntA = new Vector2Int[0];
        private Vector3[] _vector3A = new Vector3[0];
        private Vector3Int[] _vector3IntA = new Vector3Int[0];
        private Vector4[] _vector4A = new Vector4[0];
        private Quaternion[] _quaternionA = new Quaternion[0];
        private Color[] _colorA = new Color[0];
        private Color32[] _color32A = new Color32[0];
        private VRCPlayerApi[] _vrcPlayerApiA = new VRCPlayerApi[0];

        #endregion

        #region InternalVariables

        private DataToken[] _parameters = new DataToken[0];
        private int _bufferOffset;
        private int _localSentOut;
        
        [UdonSynced] [NonSerialized] public byte[] syncBuffer = new byte[0];
        
        private DataList syncBufferBuilder = new DataList();

        private DataList _methodQueue = new DataList();
        private DataList _targetQueue = new DataList();
        private DataList _dataQueue = new DataList();
        private bool _queueRunning;
        private float _lastSendTime;
        private NetworkInterface _targetScript;

        #endregion


        internal void SetupCaller()
        {
            _methodInfos = networkManager.methodInfos;
            _methodInfosKeys = networkManager.methodInfosKeys;
        }

        public override void OnPreSerialization()
        {
            if (_debug)
            {
                syncBuffer.ReadVariableInt(0, out int target);
                Log($"PreSerialization - {target} - {syncBuffer.Length}");
            }
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            if (!result.success)
            {
                LogWarning($"Failed Serialization - {result.byteCount}");
            }
        }
        
        internal void _PrepareSend(int intTarget, string method, int scriptTarget, DataToken[] data)
        {
            var target = (SyncTarget) intTarget;
            if (intTarget > 100) intTarget -= 100;

            var sIndex = Array.IndexOf(sceneInterfacesIds, scriptTarget);
            if (sIndex == -1)
            {
                LogError($"Invalid script target: {scriptTarget} - {method} can't send method if target is invalid");
                return;
            }

            var methodId = _methodInfosKeys.IndexOf(method);
            if (methodId == -1)
            {
                LogError(
                    $"<color=#FF0000>Invalid method: {method}</color> - {method} can't send method if method is invalid, check if method is public and marked as [NetworkedMethod]");
                return;
            }

            if (_debug)
            {
                Log($"Preparing Send - {method} - {methodId} - {scriptTarget} - {sIndex} - {target}");
            }
            

            //Limit sending method every 0.125 seconds - 8 times per second 
            if (Time.realtimeSinceStartup - _lastSendTime < 0.125f)
            {
                _methodQueue.Add(methodId);
                _targetQueue.Add(scriptTarget);
                DataToken[] dataTokens = new DataToken[data.Length];
                Array.Copy(data, dataTokens, data.Length);
                _dataQueue.Add(new DataToken(dataTokens));
                if (!_queueRunning)
                {
                    _queueRunning = true;
                    SendCustomEventDelayedSeconds(nameof(_SendQueue), 0.125f - (Time.realtimeSinceStartup - _lastSendTime));
                }
            }
            else
            {
                _lastSendTime = Time.realtimeSinceStartup;
                SendData(methodId, scriptTarget, data);
            }
            
            if (target == SyncTarget.All || target == SyncTarget.Local)
            {
                var methodKey = _methodInfosKeys[methodId];
                var methodInfo = _methodInfos[methodKey].DataDictionary;

                _targetScript = sceneInterfaces[sIndex];

                if (methodInfo.TryGetValue("parameters", out var parametersToken))
                {
                    if (!parametersToken.IsNull)
                    {
                        var parameters = parametersToken.DataList;
                        for (int i = 0; i < data.Length; i++)
                        {
                            SetUdonVariable(parameters[i].String, data[i]);
                        }
                    }
                }

                _targetScript.SendCustomEvent(methodInfo["methodName"].String);
            }
            
            
        }

        private void SendData(int method, int scriptTarget, DataToken[] data)
        {
            var sIndex = Array.IndexOf(sceneInterfacesIds, scriptTarget);
            if (sIndex == -1)
            {
                LogError($"Invalid script target: {scriptTarget} - {method} can't send method if target is invalid");
                return;
            }
            
            syncBufferBuilder.Clear();
            
            _localSentOut++;
            syncBufferBuilder.AddVariableInt(Convert.ToUInt32(data.Length));
            syncBufferBuilder.AddVariableInt(method);
            syncBufferBuilder.AddVariableInt(scriptTarget);
            syncBufferBuilder.AddVariableInt(_localSentOut);
            
            
            for (_iter = 0; _iter < data.Length; _iter++)
            {
                LogWarning($"Sending {data[_iter].TokenType} - Iter: {_iter}");
                
                var type = data[_iter].TokenType;
                int typeId;
                if (type != TokenType.Reference)
                {
                    typeId = Array.IndexOf(_tokenMap, type);
                }
                else
                {
                    var reference = data[_iter].Reference;
                    typeId = Array.IndexOf(_typeMap, reference.GetType());
                    LogWarning($"Reference: {reference} - Type: {reference.GetType()} - TypeId: {typeId}");
                }

                LogWarning($"Type: {type} - TypeId: {typeId}");
                Types enumType;
                if (typeId == -1)
                {
                    enumType = Types.Null;
                    typeId = (int) Types.Null;
                }
                else
                {
                    enumType = (Types) typeId;
                }
                
                LogWarning($"EnumType: {enumType} - TypeId: {typeId}");


                if (typeId < (int) Types.Int16 || typeId > (int) Types.UInt64)
                {
                    syncBufferBuilder.Add((byte) typeId);
                }


                switch (enumType)
                {
                    case Types.Boolean:
                        syncBufferBuilder.Add(data[_iter].Boolean ? BYTE_ONE : BYTE_ZERO);
                        break;
                    case Types.Byte:
                        syncBufferBuilder.Add(data[_iter].Byte);
                        break;
                    case Types.SByte:
                        _sbyteValue = data[_iter].SByte;
                        syncBufferBuilder.Add((byte) (_sbyteValue < 0 ? (_sbyteValue + _0xFF) : _sbyteValue));
                        break;
                    case Types.Int16:
                        _int16Value = data[_iter].Short;
                        _int32TMP = _int16Value < 0 ? (_int16Value + 0xFFFF) : _int16Value;

                        if (_int32TMP == 0)
                        {
                            syncBufferBuilder.Add(Int16V);
                        }
                        else if (_int32TMP < _0xFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(Int16V + 1));
                            syncBufferBuilder.Add(Convert.ToByte( _int32TMP));
                        }
                        else
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt16V+2));
                            syncBufferBuilder.Add(Convert.ToByte((_int32TMP >> Bit8) & _0xFF));
                            syncBufferBuilder.Add(Convert.ToByte(_int32TMP & _0xFF));
                        }
                        break;
                    case Types.UInt16:
                        _uint16Value = data[_iter].UShort;
                        
                        if (_uint16Value < _0xFF)
                        {
                            syncBufferBuilder.Add(UInt16V);
                            syncBufferBuilder.Add((byte) _uint16Value);
                        }
                        else
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt16V+1));
                            syncBufferBuilder.Add((byte) ((_uint16Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint16Value & _0xFF));
                        }
                        break;
                    case Types.Int32:
                        _int32TMP2 = data[_iter].Int;
                        
                        if (_int32TMP2 < _0xFF)
                        {
                            syncBufferBuilder.Add(Int32V);
                            syncBufferBuilder.Add((byte) _int32TMP2);
                        }
                        else if (_int32TMP2 < _0xFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(Int32V+1));
                            syncBufferBuilder.Add((byte) ((_int32TMP2 >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_int32TMP2 & _0xFF));
                        }
                        else if (_int32TMP2 < 0xFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(Int32V+2));
                            syncBufferBuilder.Add((byte) ((_int32TMP2 >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int32TMP2 >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_int32TMP2 & _0xFF));
                        }
                        else
                        {
                            syncBufferBuilder.Add(Convert.ToByte(Int32V+3));
                            syncBufferBuilder.Add((byte) ((_int32TMP2 >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int32TMP2 >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int32TMP2 >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_int32TMP2 & _0xFF));
                        }
                        
                        break;
                    case Types.UInt32:
                        _uint32Value = data[_iter].UInt;
                        
                        if (_uint32Value < _0xFF)
                        {
                            syncBufferBuilder.Add(UInt32V);
                            syncBufferBuilder.Add((byte) _uint32Value);
                        }
                        else if (_uint32Value < _0xFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt32V+1));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint32Value & _0xFF));
                        }
                        else if (_uint32Value < 0xFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt32V+2));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint32Value & _0xFF));
                        }
                        else
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt32V+3));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint32Value & _0xFF));
                        }
                        break;
                    case Types.Int64:
                        _int64Value = data[_iter].Long;

                        if (_int64Value < _0xFF)
                        {
                            syncBufferBuilder.Add(Int64V);
                            syncBufferBuilder.Add((byte) _int64Value);
                        }
                        else if (_int64Value < _0xFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(Int64V+1));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                        }
                        else if (_int64Value < 0xFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(Int64V+2));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                        }
                        else if (_int64Value < 0xFFFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(Int64V+3));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                        }
                        else if (_int64Value < 0xFFFFFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(Int64V+4));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit32) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                        }
                        else if (_int64Value < 0xFFFFFFFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(Int64V+5));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit40) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit32) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                        }
                        else if (_int64Value < 0xFFFFFFFFFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(Int64V+6));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit48) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit40) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit32) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                        }
                        else
                        {
                            syncBufferBuilder.Add(Convert.ToByte(Int64V+7));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit56) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit48) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit40) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit32) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                        }
                        break;
                    case Types.UInt64:
                        _uint64Value = data[_iter].ULong;
                        if (_uint64Value < _0xFF)
                        {
                            syncBufferBuilder.Add(UInt64V);
                            syncBufferBuilder.Add((byte) _uint64Value);
                        }
                        else if (_uint64Value < _0xFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V+1));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint64Value & _0xFF));
                        }
                        else if (_uint64Value < 0xFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V+2));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint64Value & _0xFF));
                        }
                        else if (_uint64Value < 0xFFFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V + 3));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint64Value & _0xFF));
                        }
                        else if (_uint64Value < 0xFFFFFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V+4));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit32) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint64Value & _0xFF));
                        }
                        else if (_uint64Value < 0xFFFFFFFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V + 5));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit40) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit32) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint64Value & _0xFF));
                        }
                        else if (_uint64Value < 0xFFFFFFFFFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V + 6));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit48) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit40) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit32) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint64Value & _0xFF));
                        }
                        else
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V + 7));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit56) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit48) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit40) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit32) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint64Value & _0xFF));
                        }

                        break;
                    case Types.Single:
                    {
                        _singleValue = data[_iter].Float;
                        _uintTmp = 0;
                        if (float.IsNaN(_singleValue))
                        {
                            _uintTmp = FloatExpMask | FloatFracMask;
                        }
                        else if (float.IsInfinity(_singleValue))
                        {
                            _uintTmp = FloatExpMask;
                            if (float.IsNegativeInfinity(_singleValue)) _uintTmp |= FloatSignBit;
                        }
                        else if (_singleValue != 0f)
                        {
                            if (_singleValue < 0f)
                            {
                                _singleValue = -_singleValue;
                                _uintTmp |= FloatSignBit;
                            }

                            int exp = 0;
                            bool normal = true;
                            while (_singleValue >= 2f)
                            {
                                _singleValue *= 0.5f;
                                exp++;
                            }

                            while (_singleValue < 1f)
                            {
                                if (exp == -126)
                                {
                                    normal = false;
                                    break;
                                }

                                _singleValue *= 2f;
                                exp--;
                            }

                            if (normal)
                            {
                                _singleValue -= 1f;
                                exp += 127;
                            }
                            else exp = 0;

                            _uintTmp |= Convert.ToUInt32(exp << 23) & FloatExpMask;
                            _uintTmp |= Convert.ToUInt32(_singleValue * 0x800000) & FloatFracMask;
                        }

                        
                        syncBufferBuilder.Add((byte) ((_uintTmp >> Bit24) & 255u));
                        syncBufferBuilder.Add((byte) ((_uintTmp >> Bit16) & 255u));
                        syncBufferBuilder.Add((byte) ((_uintTmp >> Bit8) & 255u));
                        syncBufferBuilder.Add((byte) (_uintTmp & 255u));
                        
                        break;
                    }
                    case Types.Double:
                    {
                        _doubleValue = data[_iter].Double;
                        ulong doubleTmp = 0;
                        if (double.IsNaN(_doubleValue))
                        {
                            doubleTmp = DoubleExpMask | DoubleFracMask;
                        }
                        else if (double.IsInfinity(_doubleValue))
                        {
                            doubleTmp = DoubleExpMask;
                            if (double.IsNegativeInfinity(_doubleValue)) doubleTmp |= DoubleSignBit;
                        }
                        else if (_doubleValue != 0.0)
                        {
                            if (_doubleValue < 0.0)
                            {
                                _doubleValue = -_doubleValue;
                                doubleTmp |= DoubleSignBit;
                            }

                            long exp = 0;
                            while (_doubleValue >= 2.0)
                            {
                                _doubleValue *= 0.5;
                                ++exp;
                            }

                            bool normal = true;
                            while (_doubleValue < 1.0)
                            {
                                if (exp == -1022)
                                {
                                    normal = false;
                                    break;
                                }

                                _doubleValue *= 2.0;
                                --exp;
                            }

                            if (normal)
                            {
                                _doubleValue -= 1.0;
                                exp += 1023;
                            }
                            else exp = 0;

                            doubleTmp |= Convert.ToUInt64(exp << 52) & DoubleExpMask;
                            doubleTmp |= Convert.ToUInt64(_doubleValue * 0x10000000000000) & DoubleFracMask;
                        }
                        
                        syncBufferBuilder.Add((byte) ((doubleTmp >> Bit56) & 255ul));
                        syncBufferBuilder.Add((byte) ((doubleTmp >> Bit48) & 255ul));
                        syncBufferBuilder.Add((byte) ((doubleTmp >> Bit40) & 255ul));
                        syncBufferBuilder.Add((byte) ((doubleTmp >> Bit32) & 255ul));
                        syncBufferBuilder.Add((byte) ((doubleTmp >> Bit24) & 255ul));
                        syncBufferBuilder.Add((byte) ((doubleTmp >> Bit16) & 255ul));
                        syncBufferBuilder.Add((byte) ((doubleTmp >> Bit8) & 255ul));
                        syncBufferBuilder.Add((byte) (doubleTmp & 255ul));
                        break;
                    }
                    case Types.Decimal:
                        _int32A = Decimal.GetBits((decimal) data[_iter].Reference);
                        
                        syncBufferBuilder.Add((byte) ((_int32A[0] >> Bit24) & 255u));
                        syncBufferBuilder.Add((byte) ((_int32A[0] >> Bit16) & 255u));
                        syncBufferBuilder.Add((byte) ((_int32A[0] >> Bit8) & 255u));
                        syncBufferBuilder.Add((byte) (_int32A[0] & 255u));
                        
                        syncBufferBuilder.Add((byte) ((_int32A[1] >> Bit24) & 255u));
                        syncBufferBuilder.Add((byte) ((_int32A[1] >> Bit16) & 255u));
                        syncBufferBuilder.Add((byte) ((_int32A[1] >> Bit8) & 255u));
                        syncBufferBuilder.Add((byte) (_int32A[1] & 255u));
                        
                        syncBufferBuilder.Add((byte) ((_int32A[2] >> Bit24) & 255u));
                        syncBufferBuilder.Add((byte) ((_int32A[2] >> Bit16) & 255u));
                        syncBufferBuilder.Add((byte) ((_int32A[2] >> Bit8) & 255u));
                        syncBufferBuilder.Add((byte) (_int32A[2] & 255u));
                        
                        syncBufferBuilder.Add((byte) ((_int32A[3] >> Bit24) & 255u));
                        syncBufferBuilder.Add((byte) ((_int32A[3] >> Bit16) & 255u));
                        syncBufferBuilder.Add((byte) ((_int32A[3] >> Bit8) & 255u));
                        syncBufferBuilder.Add((byte) (_int32A[3] & 255u));
                        break;
                    case Types.String:
                    {
                        _stringData = data[_iter].String;
                        _int32TMP = _stringData.Length;
                        _int32TMP2 = BitConverter.GetStringSizeInBytes(_stringData);
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        syncBufferBuilder.AddVariableInt(_int32TMP2);
                        
                        

                        for (int i = 0; i < _int32TMP; i++)
                        {
                            int value = char.ConvertToUtf32(_stringData, i);
                            if (value < 0x80)
                            {
                                syncBufferBuilder.Add((byte) value);
                            }
                            else if (value < 0x0800)
                            {
                                syncBufferBuilder.Add((byte) (value >> 6 | 0xC0));
                                syncBufferBuilder.Add((byte) (value & 0x3F | 0x80));
                            }
                            else if (value < 0x010000)
                            {
                                syncBufferBuilder.Add((byte) (value >> 12 | 0xE0));
                                syncBufferBuilder.Add((byte) ((value >> 6) & 0x3F | 0x80));
                                syncBufferBuilder.Add((byte) (value & 0x3F | 0x80));
                            }
                            else
                            {
                               
                                syncBufferBuilder.Add((byte) (value >> 18 | 0xF0));
                                syncBufferBuilder.Add((byte) ((value >> 12) & 0x3F | 0x80));
                                syncBufferBuilder.Add((byte) ((value >> 6) & 0x3F | 0x80));
                                syncBufferBuilder.Add((byte) (value & 0x3F | 0x80));
                            }

                            if (char.IsSurrogate(_stringData, i)) i++;
                        }

                        break;
                    }
                    case Types.VRCPlayerApi:
                    {
                        _player = (VRCPlayerApi) data[_iter].Reference;
                        if (_player == null)
                        {
                            syncBufferBuilder.AddVariableInt(0);
                        }
                        else
                        {
                            syncBufferBuilder.AddVariableInt(_player.playerId);
                        }
                        break;
                    }
                    case Types.Color:
                        _color = (Color) data[_iter].Reference;
                        syncBufferBuilder.AddBytes(_color.r);
                        syncBufferBuilder.AddBytes(_color.g);
                        syncBufferBuilder.AddBytes(_color.b);
                        syncBufferBuilder.AddBytes(_color.a);
                        break;
                    case Types.Color32:
                        _color32 = (Color32) data[_iter].Reference;
                        
                        syncBufferBuilder.Add(_color32.r);
                        syncBufferBuilder.Add(_color32.g);
                        syncBufferBuilder.Add(_color32.b);
                        syncBufferBuilder.Add(_color32.a);
                        break;
                    case Types.Vector2:
                        _vector2 = (Vector2) data[_iter].Reference;
                        syncBufferBuilder.AddBytes(_vector2.x);
                        syncBufferBuilder.AddBytes(_vector2.y);
                        break;
                    case Types.Vector2Int:
                        _vector2Int = (Vector2Int) data[_iter].Reference;
                        syncBufferBuilder.AddVariableInt(_vector2Int.x);
                        syncBufferBuilder.AddVariableInt(_vector2Int.y);
                        break;
                    case Types.Vector3:
                        _vector3 = (Vector3) data[_iter].Reference;
                        syncBufferBuilder.AddBytes(_vector3.x);
                        syncBufferBuilder.AddBytes(_vector3.y);
                        syncBufferBuilder.AddBytes(_vector3.z);
                        break;
                    case Types.Vector3Int:
                        _vector3Int = (Vector3Int) data[_iter].Reference;
                        
                        syncBufferBuilder.AddVariableInt(_vector3Int.x);
                        syncBufferBuilder.AddVariableInt(_vector3Int.y);   
                        syncBufferBuilder.AddVariableInt(_vector3Int.z);
                        break;
                    case Types.Vector4:
                        _vector4 = (Vector4) data[_iter].Reference;
                        
                        syncBufferBuilder.AddBytes(_vector4.x);
                        syncBufferBuilder.AddBytes(_vector4.y);
                        syncBufferBuilder.AddBytes(_vector4.z);
                        syncBufferBuilder.AddBytes(_vector4.w);
                        break;
                    case Types.Quaternion:
                        _quaternion = (Quaternion) data[_iter].Reference;
                        
                        syncBufferBuilder.AddBytes(_quaternion.x);
                        syncBufferBuilder.AddBytes(_quaternion.y);
                        syncBufferBuilder.AddBytes(_quaternion.z);
                        syncBufferBuilder.AddBytes(_quaternion.w);
                        break;
                    case Types.DateTime:
                        _int64Value = ((DateTime) data[_iter].Reference).ToBinary();
                        
                        syncBufferBuilder.Add((byte) ((_int64Value >> Bit56) & _0xFF));
                        syncBufferBuilder.Add((byte) ((_int64Value >> Bit48) & _0xFF));
                        syncBufferBuilder.Add((byte) ((_int64Value >> Bit40) & _0xFF));
                        syncBufferBuilder.Add((byte) ((_int64Value >> Bit32) & _0xFF));
                        syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                        syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                        syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                        syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                        
                        break;
                    //Arrays
                    case Types.BooleanA:
                    {
                        _boolA = (bool[]) data[_iter].Reference;
                        _int32TMP = _boolA.Length; 
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        
                        //bit packing
                        int bitOffset = 0;
                        byte currentByte = 0;
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            if (_boolA[_int32TMP2])
                            {
                                currentByte |= (byte) (1 << bitOffset);
                            }

                            bitOffset++;
                            if (bitOffset == 8)
                            {
                                syncBufferBuilder.Add(currentByte);
                                currentByte = 0;
                                bitOffset = 0;
                            }
                        }
                        
                        if (bitOffset != 0)
                        {
                            syncBufferBuilder.Add(currentByte);
                        }
                        break;
                    }
                    case Types.ByteA:
                        _byteA = (byte[]) data[_iter].Reference;
                        _int32TMP = _byteA.Length;
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            syncBufferBuilder.Add(_byteA[_int32TMP2]);
                        }
                        break;
                    case Types.SByteA:
                    {
                        _sbyteA = (sbyte[]) data[_iter].Reference;
                        _int32TMP = _sbyteA.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            syncBufferBuilder.Add((byte) (_sbyteValue < 0 ? (_sbyteValue + 0xFF) : _sbyteValue));
                        }
                        break;
                    }
                    case Types.Int16A:
                    {
                        _int16A = (short[]) data[_iter].Reference;
                        _int32TMP = _int16A.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            _int32TMP = _int16A[_int32TMP2];

#if NETCALLER_USE_VARIABLE_SERIALIZATION
                            syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
#else
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_int32TMP & 255u));
#endif                       
                        }

                        break;
                    }
                    case Types.UInt16A:
                    {
                        _uint16A = (ushort[]) data[_iter].Reference;
                        _int32TMP = _uint16A.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            _uint16Value = _uint16A[_int32TMP2];
#if NETCALLER_USE_VARIABLE_SERIALIZATION
                            syncBufferBuilder.AddVariableInt(_uint16Value);
#else
                            syncBufferBuilder.Add((byte) ((_uint16Value >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_uint16Value & 255u));
#endif
                        }

                        break;
                    }
                    case Types.Int32A:
                    {
                        _int32A = (int[]) data[_iter].Reference;
                        _int32TMP = _int32A.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            _int32TMP = _int32A[_int32TMP2];
#if NETCALLER_USE_VARIABLE_SERIALIZATION
                            syncBufferBuilder.AddVariableInt(_int32TMP);
#else
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit24) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit16) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_int32TMP & 255u));
#endif
                        }
                        break;
                    }
                    case Types.UInt32A:
                    {
                        _uint32A = (uint[]) data[_iter].Reference;
                        _int32TMP = _uint32A.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            _uint32Value = _uint32A[_int32TMP2];
#if NETCALLER_USE_VARIABLE_SERIALIZATION
                            syncBufferBuilder.AddVariableInt(_uint32Value);
#else
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit24) & 255u));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit16) & 255u));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_uint32Value & 255u));
#endif
                        }
                        break;
                    }
                    case Types.Int64A:
                    {
                        _int64A = (long[]) data[_iter].Reference;
                        _int32TMP = _int64A.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            _int64Value = _int64A[_int32TMP2];
#if NETCALLER_USE_VARIABLE_SERIALIZATION
                            syncBufferBuilder.AddVariableInt(_int64Value);
#else
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit56) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit48) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit40) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit32) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
#endif
                        }
                        break;
                    }
                    case Types.UInt64A:
                    {
                        _uint64A = (ulong[]) data[_iter].Reference;
                        _int32TMP = _uint64A.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            _uint64Value = _uint64A[_int32TMP2];
#if NETCALLER_USE_VARIABLE_SERIALIZATION
                            syncBufferBuilder.AddVariableInt(_uint64Value);
#else
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit56) & 255ul));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit48) & 255ul));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit40) & 255ul));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit32) & 255ul));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit24) & 255ul));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit16) & 255ul));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit8) & 255ul));
                            syncBufferBuilder.Add((byte) (_uint64Value & 255ul));
#endif
                        }
                        break;
                    }
                    case Types.SingleA:
                    {
                        _singleA = (float[]) data[_iter].Reference;
                        _int32TMP = _singleA.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            _singleValue = _singleA[_int32TMP2];
                            _uintTmp = 0;
                            if (float.IsNaN(_singleValue))
                            {
                                _uintTmp = FloatExpMask | FloatFracMask;
                            }
                            else if (float.IsInfinity(_singleValue))
                            {
                                _uintTmp = FloatExpMask;
                                if (float.IsNegativeInfinity(_singleValue)) _uintTmp |= FloatSignBit;
                            }
                            else if (_singleValue != 0f)
                            {
                                if (_singleValue < 0f)
                                {
                                    _singleValue = -_singleValue;
                                    _uintTmp |= FloatSignBit;
                                }

                                int exp = 0;
                                bool normal = true;
                                while (_singleValue >= 2f)
                                {
                                    _singleValue *= 0.5f;
                                    exp++;
                                }

                                while (_singleValue < 1f)
                                {
                                    if (exp == -126)
                                    {
                                        normal = false;
                                        break;
                                    }

                                    _singleValue *= 2f;
                                    exp--;
                                }

                                if (normal)
                                {
                                    _singleValue -= 1f;
                                    exp += 127;
                                }
                                else exp = 0;

                                _uintTmp |= Convert.ToUInt32(exp << 23) & FloatExpMask;
                                _uintTmp |= Convert.ToUInt32(_singleValue * 0x800000) & FloatFracMask;
                            }

                            
                            syncBufferBuilder.Add((byte) ((_uintTmp >> Bit24) & 255u));
                            syncBufferBuilder.Add((byte) ((_uintTmp >> Bit16) & 255u));
                            syncBufferBuilder.Add((byte) ((_uintTmp >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_uintTmp & 255u));
                        }
                        break;
                    }
                    case Types.DoubleA:
                    {
                        _doubleA = (double[]) data[_iter].Reference;
                        _int32TMP = _doubleA.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            _doubleValue = _doubleA[_int32TMP2];
                            ulong doubleTmp = 0;
                            if (double.IsNaN(_doubleValue))
                            {
                                doubleTmp = DoubleExpMask | DoubleFracMask;
                            }
                            else if (double.IsInfinity(_doubleValue))
                            {
                                doubleTmp = DoubleExpMask;
                                if (double.IsNegativeInfinity(_doubleValue)) doubleTmp |= DoubleSignBit;
                            }
                            else if (_doubleValue != 0.0)
                            {
                                if (_doubleValue < 0.0)
                                {
                                    _doubleValue = -_doubleValue;
                                    doubleTmp |= DoubleSignBit;
                                }

                                long exp = 0;
                                while (_doubleValue >= 2.0)
                                {
                                    _doubleValue *= 0.5;
                                    ++exp;
                                }

                                bool normal = true;
                                while (_doubleValue < 1.0)
                                {
                                    if (exp == -1022)
                                    {
                                        normal = false;
                                        break;
                                    }

                                    _doubleValue *= 2.0;
                                    --exp;
                                }

                                if (normal)
                                {
                                    _doubleValue -= 1.0;
                                    exp += 1023;
                                }
                                else exp = 0;

                                doubleTmp |= Convert.ToUInt64(exp << 52) & DoubleExpMask;
                                doubleTmp |= Convert.ToUInt64(_doubleValue * 0x10000000000000) & DoubleFracMask;
                            }

                            syncBufferBuilder.Add((byte) ((doubleTmp >> Bit56) & 255ul));
                            syncBufferBuilder.Add((byte) ((doubleTmp >> Bit48) & 255ul));
                            syncBufferBuilder.Add((byte) ((doubleTmp >> Bit40) & 255ul));
                            syncBufferBuilder.Add((byte) ((doubleTmp >> Bit32) & 255ul));
                            syncBufferBuilder.Add((byte) ((doubleTmp >> Bit24) & 255ul));
                            syncBufferBuilder.Add((byte) ((doubleTmp >> Bit16) & 255ul));
                            syncBufferBuilder.Add((byte) ((doubleTmp >> Bit8) & 255ul));
                            syncBufferBuilder.Add((byte) (doubleTmp & 255ul));
                        }

                        break;
                    }
                    case Types.DecimalA:
                    {
                        _decimalA = (decimal[]) data[_iter].Reference;
                        _int32TMP = _decimalA.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            _int32A = Decimal.GetBits(_decimalA[_int32TMP2]);
                            
                            syncBufferBuilder.Add((byte) ((_int32A[0] >> Bit24) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32A[0] >> Bit16) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32A[0] >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_int32A[0] & 255u));
                            
                            syncBufferBuilder.Add((byte) ((_int32A[1] >> Bit24) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32A[1] >> Bit16) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32A[1] >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_int32A[1] & 255u));
                            
                            syncBufferBuilder.Add((byte) ((_int32A[2] >> Bit24) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32A[2] >> Bit16) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32A[2] >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_int32A[2] & 255u));
                            
                            syncBufferBuilder.Add((byte) ((_int32A[3] >> Bit24) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32A[3] >> Bit16) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32A[3] >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_int32A[3] & 255u));
                        }
                        break;
                    }
                    case Types.StringA:
                    {
                        _stringA = (string[]) data[_iter].Reference;
                        _int32TMP = _stringA.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            _stringData = _stringA[_int32TMP2];
                            _int32TMP = _stringData.Length;
                            _int32TMP2 = BitConverter.GetStringSizeInBytes(_stringData);
                            
                            syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                            syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP2));
                            
                            for (_int32TMP3 = 0; _int32TMP3 < _int32TMP; _int32TMP3++)
                            {
                                int value = char.ConvertToUtf32(_stringData, _int32TMP3);
                                if (value < 0x80)
                                {
                                    syncBufferBuilder.Add((byte) value);
                                }
                                else if (value < 0x0800)
                                {
                                    syncBufferBuilder.Add((byte) (value >> 6 | 0xC0));
                                    syncBufferBuilder.Add((byte) (value & 0x3F | 0x80));
                                }
                                else if (value < 0x010000)
                                {
                                    syncBufferBuilder.Add((byte) (value >> 12 | 0xE0));
                                    syncBufferBuilder.Add((byte) ((value >> 6) & 0x3F | 0x80));
                                    syncBufferBuilder.Add((byte) (value & 0x3F | 0x80));
                                }
                                else
                                {
                                    syncBufferBuilder.Add((byte) (value >> 18 | 0xF0));
                                    syncBufferBuilder.Add((byte) ((value >> 12) & 0x3F | 0x80));
                                    syncBufferBuilder.Add((byte) ((value >> 6) & 0x3F | 0x80));
                                    syncBufferBuilder.Add((byte) (value & 0x3F | 0x80));
                                }

                                if (char.IsSurrogate(_stringData, _int32TMP3)) _int32TMP3++;
                            }
                        }
                        break;
                    }
                    case Types.VRCPlayerApiA:
                    {
                        _vrcPlayerApiA = (VRCPlayerApi[]) data[_iter].Reference;
                        _int32TMP = _vrcPlayerApiA.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++) 
                            syncBufferBuilder.AddVariableInt(_vrcPlayerApiA[_int32TMP2] == null ? 0 : _vrcPlayerApiA[_int32TMP2].playerId);

                        break;
                    }
                    case Types.ColorA:
                    {
                        _colorA = (Color[]) data[_iter].Reference;
                        _int32TMP = _colorA.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            _color = _colorA[_int32TMP2];
                            syncBufferBuilder.AddBytes(_color.r);
                            syncBufferBuilder.AddBytes(_color.g);
                            syncBufferBuilder.AddBytes(_color.b);
                            syncBufferBuilder.AddBytes(_color.a);
                        }
                        break;
                    }
                    case Types.Color32A:
                    {
                        _color32A = (Color32[]) data[_iter].Reference;
                        _int32TMP = _color32A.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            _color32 = _color32A[_int32TMP2];
                            syncBufferBuilder.Add(_color32.r);
                            syncBufferBuilder.Add(_color32.g);
                            syncBufferBuilder.Add(_color32.b);
                            syncBufferBuilder.Add(_color32.a);
                        }
                        break;
                    }
                    case Types.Vector2A:
                    {
                        _vector2A = (Vector2[]) data[_iter].Reference;
                        _int32TMP = _vector2A.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++) 
                        {
                            _vector2 = _vector2A[_int32TMP2];
                            syncBufferBuilder.AddBytes(_vector2.x);
                            syncBufferBuilder.AddBytes(_vector2.y);
                        }
                        break;
                    }
                    case Types.Vector2IntA:
                    {
                        _vector2IntA = (Vector2Int[]) data[_iter].Reference;
                        _int32TMP = _vector2IntA.Length;

                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++)
                        {
                            _vector2Int = _vector2IntA[_int32TMP2];
#if NETCALLER_USE_VARIABLE_SERIALIZATION
                            syncBufferBuilder.AddVariableInt(_vector2Int.x);
                            syncBufferBuilder.AddVariableInt(_vector2Int.y);
#else
                            _int32TMP = _vector2Int.x;
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit24) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit16) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_int32TMP & 255u));
                            _int32TMP = _vector2Int.y;
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit24) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit16) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_int32TMP & 255u));
#endif
                        }

                        break;
                    }
                    case Types.Vector3A:
                    {
                        _vector3A = (Vector3[]) data[_iter].Reference;
                        _int32TMP = _vector3A.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++) 
                        {
                            _vector3 = _vector3A[_int32TMP2];
                            syncBufferBuilder.AddBytes(_vector3.x);
                            syncBufferBuilder.AddBytes(_vector3.y);
                            syncBufferBuilder.AddBytes(_vector3.z);
                        }
                        break;
                    }
                    case Types.Vector3IntA:
                    {
                        _vector3IntA = (Vector3Int[]) data[_iter].Reference;
                        _int32TMP = _vector3IntA.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++) 
                        {
                            _vector3Int = _vector3IntA[_int32TMP2];
#if NETCALLER_USE_VARIABLE_SERIALIZATION
                            syncBufferBuilder.AddVariableInt(_vector3Int.x);
                            syncBufferBuilder.AddVariableInt(_vector3Int.y);
                            syncBufferBuilder.AddVariableInt(_vector3Int.z);
#else
                            _int32TMP = _vector3Int.x;
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit24) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit16) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_int32TMP & 255u));
                            _int32TMP = _vector3Int.y;
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit24) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit16) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_int32TMP & 255u));
                            _int32TMP = _vector3Int.z;
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit24) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit16) & 255u));
                            syncBufferBuilder.Add((byte) ((_int32TMP >> Bit8) & 255u));
                            syncBufferBuilder.Add((byte) (_int32TMP & 255u));
#endif
                        }
                        break;
                    }
                    case Types.Vector4A:
                    {
                        _vector4A = (Vector4[]) data[_iter].Reference;
                        _int32TMP = _vector4A.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; _int32TMP2++) 
                        {
                            _vector4 = _vector4A[_int32TMP2];
                            syncBufferBuilder.AddBytes(_vector4.x);
                            syncBufferBuilder.AddBytes(_vector4.y);
                            syncBufferBuilder.AddBytes(_vector4.z);
                            syncBufferBuilder.AddBytes(_vector4.w);
                        }

                        break;
                    }
                    case Types.QuaternionA:
                    {
                        _quaternionA = (Quaternion[]) data[_iter].Reference;
                        _int32TMP = _quaternionA.Length;
                        
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP));
                        for (_int32TMP2 = 0; _int32TMP2 < _int32TMP; ++_int32TMP2)
                        {
                            _quaternion = _quaternionA[_int32TMP2];
                            syncBufferBuilder.AddBytes(_quaternion.x);
                            syncBufferBuilder.AddBytes(_quaternion.y);
                            syncBufferBuilder.AddBytes(_quaternion.z);
                            syncBufferBuilder.AddBytes(_quaternion.w);
                        }
                        break;
                    }
                }
            }

            if (syncBuffer.Length != syncBufferBuilder.Count)
            {
                syncBuffer = new byte[syncBufferBuilder.Count];
            }
            for (int i = 0; i < syncBufferBuilder.Count; i++)
            {
                Debug.Log($"SyncBufferBuilder {i} - {syncBufferBuilder[i].Byte}");
                syncBuffer[i] = syncBufferBuilder[i].Byte;
            }
            
            syncBufferBuilder.Clear();
            

            if (_debug)
            {
                Log(
                    $"Sending {method} to {scriptTarget} with {data.Length} parameters totaling {syncBuffer.Length} bytes");
            }
            


            RequestSerialization();
        }

        public void _SendQueue()
        {
            if (_methodQueue.Count == 0)
            {
                _queueRunning = false;
                return;
            }

            Log($"Handling queue {_methodQueue.Count}");

            var method = _methodQueue[0].Int;
            var target = _targetQueue[0].Int;
            var data = (DataToken[]) _dataQueue[0].Reference;

            _methodQueue.RemoveAt(0);
            _targetQueue.RemoveAt(0);
            _dataQueue.RemoveAt(0);
            SendData(method, target, data);
            _lastSendTime = Time.realtimeSinceStartup;

            SendCustomEventDelayedSeconds(nameof(_SendQueue), 0.125f);
        }

        public override void OnDeserialization()
        {
            int offset = 0;
            offset += syncBuffer.ReadVariableInt(offset, out int methodTarget);
            if (_debug)
            {
                Log($"Deserialization - {methodTarget} - Size {syncBuffer.Length}");
            }

            if (methodTarget == -1) return;
            
            offset += syncBuffer.ReadVariableInt(offset, out int scriptTarget);
            syncBuffer.ReadVariableInt(offset, out int sentOutMethods);
            if (_localSentOut >= sentOutMethods && _localSentOut != 0)
            {
                if (_debug)
                    LogWarning(
                        $"Ignoring deserialization, already sent out Local: {_localSentOut} - Global: {sentOutMethods}");
                _localSentOut = sentOutMethods;
                return;
            }

            _localSentOut = sentOutMethods;


            var sIndex = Array.IndexOf(sceneInterfacesIds, scriptTarget);
            if (sIndex == -1)
            {
                LogError("Script target not found unable to receive and process data");
                return;
            }

            ReceiveData(); //Convert data from buffer to parameters

            _targetScript = sceneInterfaces[sIndex];
            var methodKey = _methodInfosKeys[methodTarget];
            var methodInfo = _methodInfos[methodKey].DataDictionary;

            if (methodInfo.TryGetValue("parameters", out var parametersToken))
            {
                if (!parametersToken.IsNull)
                {
                    var parameters = parametersToken.DataList;
                    for (int i = 0; i < _parameters.Length; i++)
                    {
                        SetUdonVariable(parameters[i].String, _parameters[i]);
                    }
                }
            }

            _targetScript.SendCustomEvent(methodInfo["methodName"].String);
        }

        private void ReceiveData()
        {
            _bufferOffset = 0;

            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out uint length);
            if (_parameters.Length != length)
            {
                _parameters = new DataToken[length];
            }

            
            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out int method);
            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out int scriptTarget);
            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out int sentOutMethods);

            for (_iter = 0; _iter < _parameters.Length; _iter++)
            {
                string[] chars;

                Types type = Types.Null;
                length = 0;
                int typeByte = syncBuffer[_bufferOffset++];
                if (typeByte < 100)
                {
                    if (typeByte >= (int) Types.BooleanA)
                    {
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out length);
                    }
                    type = (Types) typeByte;
                }
                else
                {
                    if (typeByte < Int16V + 10)
                    {
                        length = Convert.ToUInt32(typeByte - Int16V);
                        type = Types.Int16V;
                    }
                    else if (typeByte < UInt16V + 10)
                    {
                        length = Convert.ToUInt32(typeByte - UInt16V);
                        type = Types.UInt16V;
                    }
                    else if (typeByte < Int32V + 10)
                    {
                        length = Convert.ToUInt32(typeByte - Int32V);
                        type = Types.Int32V;
                    }
                    else if (typeByte < UInt32V + 10)
                    {
                        length = Convert.ToUInt32(typeByte - UInt32V);
                        type = Types.UInt32V;
                    }
                    else if (typeByte < Int64V + 10)
                    {
                        length = Convert.ToUInt32(typeByte - Int64V);
                        type = Types.Int64V;
                    }
                    else if (typeByte < UInt64V + 10)
                    {
                        length = Convert.ToUInt32(typeByte - UInt64V);
                        type = Types.UInt64V;
                    }
                    
                }
                
                
                
                switch (type)
                {
                    case Types.Boolean:
                        _parameters[_iter] = syncBuffer[_bufferOffset] == BYTE_ONE;
                        _bufferOffset++;
                        break;
                    case Types.Byte:
                        _parameters[_iter] = syncBuffer[_bufferOffset];
                        _bufferOffset++;
                        break;
                    case Types.SByte:
                        _int32TMP = syncBuffer[_bufferOffset];
                        if (_int32TMP >= 0x80) _int32TMP -= _0xFF;
                        _parameters[_iter] = Convert.ToSByte(_int32TMP);
                        _bufferOffset++;
                        break;
                    case Types.Int16V:
                        if (length == 0)
                        {
                            _parameters[_iter] = Convert.ToInt16(0);
                            break;
                        }

                        if (length == 1)
                        {
                            _parameters[_iter] = Convert.ToInt16(syncBuffer[_bufferOffset]);
                            _bufferOffset++;
                            break;
                        }

                        if (length == 2)
                        {
                            _parameters[_iter] = Convert.ToInt16((syncBuffer[_bufferOffset] << Bit8) |
                                                                 syncBuffer[_bufferOffset + 1]);
                            _bufferOffset += 2;
                            break;
                        }
                        break;
                    case Types.UInt16:
                        if (length == 0)
                        {
                            _parameters[_iter] = Convert.ToUInt16(0);
                            break;
                        }
                        
                        if (length == 1)
                        {
                            _parameters[_iter] = Convert.ToUInt16(syncBuffer[_bufferOffset]);
                            _bufferOffset++;
                            break;
                        }
                        
                        if (length == 2)
                        {
                            _parameters[_iter] = Convert.ToUInt16((syncBuffer[_bufferOffset] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 1]);
                            _bufferOffset += 2;
                            break;
                        }
                        break;
                    case Types.Int32:
                        if (length == 0)
                        {
                            _parameters[_iter] = Convert.ToInt32(0);
                            break;
                        }
                        
                        if (length == 1)
                        {
                            _parameters[_iter] = Convert.ToInt32(syncBuffer[_bufferOffset]);
                            _bufferOffset++;
                            break;
                        }
                        
                        if (length == 2)
                        {
                            _parameters[_iter] = Convert.ToInt32((syncBuffer[_bufferOffset] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 1]);
                            _bufferOffset += 2;
                            break;
                        }
                        
                        if (length == 3)
                        {
                            _parameters[_iter] = Convert.ToInt32((syncBuffer[_bufferOffset] << Bit16) |
                                                                  (syncBuffer[_bufferOffset + 1] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 2]);
                            _bufferOffset += 3;
                            break;
                        }
                        
                        if (length == 4)
                        {
                            _parameters[_iter] = Convert.ToInt32((syncBuffer[_bufferOffset] << Bit24) |
                                                                  (syncBuffer[_bufferOffset + 1] << Bit16) |
                                                                  (syncBuffer[_bufferOffset + 2] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 3]);
                            _bufferOffset += 4;
                            break;
                        }
                        break;
                    case Types.UInt32:
                        if (length == 0)
                        {
                            _parameters[_iter] = Convert.ToUInt32(0);
                            break;
                        }
                        
                        if (length == 1)
                        {
                            _parameters[_iter] = Convert.ToUInt32(syncBuffer[_bufferOffset]);
                            _bufferOffset++;
                            break;
                        }
                        
                        if (length == 2)
                        {
                            _parameters[_iter] = Convert.ToUInt32((syncBuffer[_bufferOffset] << Bit8) |
                                                                   syncBuffer[_bufferOffset + 1]);
                            _bufferOffset += 2;
                            break;
                        }
                        
                        if (length == 3)
                        {
                            _parameters[_iter] = Convert.ToUInt32((syncBuffer[_bufferOffset] << Bit16) |
                                                                   (syncBuffer[_bufferOffset + 1] << Bit8) |
                                                                   syncBuffer[_bufferOffset + 2]);
                            _bufferOffset += 3;
                            break;
                        }
                        
                        if (length == 4)
                        {
                            _parameters[_iter] = Convert.ToUInt32((syncBuffer[_bufferOffset] << Bit24) |
                                                                   (syncBuffer[_bufferOffset + 1] << Bit16) |
                                                                   (syncBuffer[_bufferOffset + 2] << Bit8) |
                                                                   syncBuffer[_bufferOffset + 3]);
                            _bufferOffset += 4;
                            break;
                        }
                        break;
                    case Types.Int64:
                        if (length == 0)
                        {
                            _parameters[_iter] = Convert.ToInt64(0);
                            break;
                        }
                        
                        if (length == 1)
                        {
                            _parameters[_iter] = Convert.ToInt64(syncBuffer[_bufferOffset]);
                            _bufferOffset++;
                            break;
                        }
                        
                        if (length == 2)
                        {
                            _parameters[_iter] = Convert.ToInt64((syncBuffer[_bufferOffset] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 1]);
                            _bufferOffset += 2;
                            break;
                        }
                        
                        if (length == 3)
                        {
                            _parameters[_iter] = Convert.ToInt64((syncBuffer[_bufferOffset] << Bit16) |
                                                                  (syncBuffer[_bufferOffset + 1] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 2]);
                            _bufferOffset += 3;
                            break;
                        }
                        
                        if (length == 4)
                        {
                            _parameters[_iter] = Convert.ToInt64((syncBuffer[_bufferOffset] << Bit24) |
                                                                  (syncBuffer[_bufferOffset + 1] << Bit16) |
                                                                  (syncBuffer[_bufferOffset + 2] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 3]);
                            _bufferOffset += 4;
                            break;
                        }
                        
                        if (length == 5)
                        {
                            _parameters[_iter] = Convert.ToInt64((syncBuffer[_bufferOffset] << Bit32) |
                                                                  (syncBuffer[_bufferOffset + 1] << Bit24) |
                                                                  (syncBuffer[_bufferOffset + 2] << Bit16) |
                                                                  (syncBuffer[_bufferOffset + 3] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 4]);
                            _bufferOffset += 5;
                            break;
                        }
                        
                        if (length == 6)
                        {
                            _parameters[_iter] = Convert.ToInt64((syncBuffer[_bufferOffset] << Bit40) |
                                                                  (syncBuffer[_bufferOffset + 1] << Bit32) |
                                                                  (syncBuffer[_bufferOffset + 2] << Bit24) |
                                                                  (syncBuffer[_bufferOffset + 3] << Bit16) |
                                                                  (syncBuffer[_bufferOffset + 4] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 5]);
                            _bufferOffset += 6;
                            break;
                        }
                        
                        if (length == 7)
                        {
                            _parameters[_iter] = Convert.ToInt64((syncBuffer[_bufferOffset] << Bit48) |
                                                                  (syncBuffer[_bufferOffset + 1] << Bit40) |
                                                                  (syncBuffer[_bufferOffset + 2] << Bit32) |
                                                                  (syncBuffer[_bufferOffset + 3] << Bit24) |
                                                                  (syncBuffer[_bufferOffset + 4] << Bit16) |
                                                                  (syncBuffer[_bufferOffset + 5] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 6]);
                            _bufferOffset += 7;
                            break;
                        }
                        
                        if (length == 8)
                        {
                            _parameters[_iter] = Convert.ToInt64((syncBuffer[_bufferOffset] << Bit56) |
                                                                  (syncBuffer[_bufferOffset + 1] << Bit48) |
                                                                  (syncBuffer[_bufferOffset + 2] << Bit40) |
                                                                  (syncBuffer[_bufferOffset + 3] << Bit32) |
                                                                  (syncBuffer[_bufferOffset + 4] << Bit24) |
                                                                  (syncBuffer[_bufferOffset + 5] << Bit16) |
                                                                  (syncBuffer[_bufferOffset + 6] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 7]);
                            _bufferOffset += 8;
                            break;
                        }
                        break;
                    case Types.UInt64:
                        if (length == 0)
                        {
                            _parameters[_iter] = Convert.ToUInt64(0);
                            break;
                        }
                        
                        if (length == 1)
                        {
                            _parameters[_iter] = Convert.ToUInt64(syncBuffer[_bufferOffset]);
                            _bufferOffset++;
                            break;
                        }
                        
                        if (length == 2)
                        {
                            _parameters[_iter] = Convert.ToUInt64((syncBuffer[_bufferOffset] << Bit8) |
                                                                   syncBuffer[_bufferOffset + 1]);
                            _bufferOffset += 2;
                            break;
                        }
                        
                        if (length == 3)
                        {
                            _parameters[_iter] = Convert.ToUInt64((syncBuffer[_bufferOffset] << Bit16) |
                                                                   (syncBuffer[_bufferOffset + 1] << Bit8) |
                                                                   syncBuffer[_bufferOffset + 2]);
                            _bufferOffset += 3;
                            break;
                        }
                        
                        if (length == 4)
                        {
                            _parameters[_iter] = Convert.ToUInt64((syncBuffer[_bufferOffset] << Bit24) |
                                                                   (syncBuffer[_bufferOffset + 1] << Bit16) |
                                                                   (syncBuffer[_bufferOffset + 2] << Bit8) |
                                                                   syncBuffer[_bufferOffset + 3]);
                            _bufferOffset += 4;
                            break;
                        }
                        
                        if (length == 5)
                        {
                            _parameters[_iter] = Convert.ToUInt64((syncBuffer[_bufferOffset] << Bit32) |
                                                                   (syncBuffer[_bufferOffset + 1] << Bit24) |
                                                                   (syncBuffer[_bufferOffset + 2] << Bit16) |
                                                                   (syncBuffer[_bufferOffset + 3] << Bit8) |
                                                                   syncBuffer[_bufferOffset + 4]);
                            _bufferOffset += 5;
                            break;
                        }
                        
                        if (length == 6)
                        {
                            _parameters[_iter] = Convert.ToUInt64((syncBuffer[_bufferOffset] << Bit40) |
                                                                   (syncBuffer[_bufferOffset + 1] << Bit32) |
                                                                   (syncBuffer[_bufferOffset + 2] << Bit24) |
                                                                   (syncBuffer[_bufferOffset + 3] << Bit16) |
                                                                   (syncBuffer[_bufferOffset + 4] << Bit8) |
                                                                   syncBuffer[_bufferOffset + 5]);
                            _bufferOffset += 6;
                            break;
                        }
                        
                        if (length == 7)
                        {
                            _parameters[_iter] = Convert.ToUInt64((syncBuffer[_bufferOffset] << Bit48) |
                                                                   (syncBuffer[_bufferOffset + 1] << Bit40) |
                                                                   (syncBuffer[_bufferOffset + 2] << Bit32) |
                                                                   (syncBuffer[_bufferOffset + 3] << Bit24) |
                                                                   (syncBuffer[_bufferOffset + 4] << Bit16) |
                                                                   (syncBuffer[_bufferOffset + 5] << Bit8) |
                                                                   syncBuffer[_bufferOffset + 6]);
                            _bufferOffset += 7;
                            break;
                        }
                        
                        if (length == 8)
                        {
                            _parameters[_iter] = Convert.ToUInt64((syncBuffer[_bufferOffset] << Bit56) |
                                                                   (syncBuffer[_bufferOffset + 1] << Bit48) |
                                                                   (syncBuffer[_bufferOffset + 2] << Bit40) |
                                                                   (syncBuffer[_bufferOffset + 3] << Bit32) |
                                                                   (syncBuffer[_bufferOffset + 4] << Bit24) |
                                                                   (syncBuffer[_bufferOffset + 5] << Bit16) |
                                                                   (syncBuffer[_bufferOffset + 6] << Bit8) |
                                                                   syncBuffer[_bufferOffset + 7]);
                            _bufferOffset += 8;
                            break;
                        }
                        break;
                    case Types.Single:
                    {
                        _uint32Value = ((uint) syncBuffer[_bufferOffset] << Bit24) |
                                       ((uint) syncBuffer[_bufferOffset + 1] << Bit16) |
                                       ((uint) syncBuffer[_bufferOffset + 2] << Bit8) | syncBuffer[_bufferOffset + 3];
                        _bufferOffset += 4;
                        if (_uint32Value == 0 || _uint32Value == FloatSignBit)
                        {
                            _parameters[_iter] = 0f;
                            continue;
                        }

                        _exp = (int) ((_uint32Value & FloatExpMask) >> 23);
                        _doubleFracMask = (int) (_uint32Value & FloatFracMask);
                        if (_exp == 0xFF)
                        {
                            if (_doubleFracMask == 0)
                            {
                                _parameters[_iter] = (_uint32Value & FloatSignBit) == FloatSignBit
                                    ? float.NegativeInfinity
                                    : float.PositiveInfinity;
                                continue;
                            }

                            _parameters[_iter] = float.NaN;
                            continue;
                        }


                        _tmpBool = _exp != 0x00;
                        if (_tmpBool) _exp -= 127;
                        else _exp = -126;

                        _singleValue = _doubleFracMask / 8388608F;
                        if (_tmpBool) _singleValue += 1f;

                        _singleValue *= Mathf.Pow(2, _exp);

                        _tmpBool = (_uint32Value & FloatSignBit) == FloatSignBit;
                        if (_tmpBool) _singleValue = -_singleValue;
                        _parameters[_iter] = _singleValue;
                        break;
                    }
                    case Types.Double:
                    {
                        _uint64Value = ((ulong) syncBuffer[_bufferOffset] << Bit56) |
                                       ((ulong) syncBuffer[_bufferOffset + 1] << Bit48) |
                                       ((ulong) syncBuffer[_bufferOffset + 2] << Bit40) |
                                       ((ulong) syncBuffer[_bufferOffset + 3] << Bit32) |
                                       ((ulong) syncBuffer[_bufferOffset + 4] << Bit24) |
                                       ((ulong) syncBuffer[_bufferOffset + 5] << Bit16) |
                                       ((ulong) syncBuffer[_bufferOffset + 6] << Bit8) | syncBuffer[_bufferOffset + 7];

                        _bufferOffset += 8;

                        if (_uint64Value == 0.0 || _uint64Value == DoubleSignBit)
                        {
                            _parameters[_iter] = 0.0;
                            continue;
                        }


                        _exp = (long) ((_uint64Value & DoubleExpMask) >> 52);
                        _doubleFracMask = (long) (_uint64Value & DoubleFracMask);

                        if (_exp == 0x7FF)
                        {
                            if (_doubleFracMask == 0)
                            {
                                _parameters[_iter] = (_uint64Value & DoubleSignBit) == DoubleSignBit
                                    ? double.NegativeInfinity
                                    : double.PositiveInfinity;
                                continue;
                            }

                            _parameters[_iter] = double.NaN;
                            continue;
                        }

                        _tmpBool = _exp != 0x000;
                        if (_tmpBool) _exp -= 1023;
                        else _exp = -1022;

                        _doubleValue = (double) _doubleFracMask / 0x10000000000000UL;
                        if (_tmpBool) _doubleValue += 1.0;

                        _doubleValue *= Math.Pow(2, _exp);

                        _tmpBool = (_uint64Value & DoubleSignBit) == DoubleSignBit;
                        if (_tmpBool) _doubleValue = -_doubleValue;
                        _parameters[_iter] = _doubleValue;
                        break;
                    }
                    case Types.Decimal:
                        _parameters[_iter] = new DataToken(new decimal(new[]
                        {
                            syncBuffer[_bufferOffset] << Bit24 | syncBuffer[_bufferOffset + 1] << Bit16 |
                            syncBuffer[_bufferOffset + 2] << Bit8 | syncBuffer[_bufferOffset + 3],

                            syncBuffer[_bufferOffset + 4] << Bit24 | syncBuffer[_bufferOffset + 5] << Bit16 |
                            syncBuffer[_bufferOffset + 6] << Bit8 | syncBuffer[_bufferOffset + 7],

                            syncBuffer[_bufferOffset + 8] << Bit24 | syncBuffer[_bufferOffset + 9] << Bit16 |
                            syncBuffer[_bufferOffset + 10] << Bit8 | syncBuffer[_bufferOffset + 11],

                            syncBuffer[_bufferOffset + 12] << Bit24 | syncBuffer[_bufferOffset + 13] << Bit16 |
                            syncBuffer[_bufferOffset + 14] << Bit8 | syncBuffer[_bufferOffset + 15]
                        }));
                        _bufferOffset += 16;
                        break;
                    case Types.String:
                    {
                        _int32TMP = 0;
                        _int32TMP2 = 0;
                        _int32TMP3 = 0;
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP4); // String length
                        
                        chars = new string[_int32TMP4];
                        
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP4); // String length in bytes

                        _int32TMP4 += _bufferOffset;


                        for (var i = _bufferOffset; i < _int32TMP4; i++)
                        {
                            _byteTmp = syncBuffer[i];
                            if ((_byteTmp & _0x80) == 0)
                            {
                                chars[_int32TMP++] = ((char) _byteTmp).ToString();
                            }
                            else if ((_byteTmp & _0xC0) == _0x80)
                            {
                                if (_int32TMP3 > 0)
                                {
                                    _int32TMP2 = _int32TMP2 << 6 | (_byteTmp & _0x3F);
                                    _int32TMP3--;
                                    if (_int32TMP3 == 0)
                                    {
                                        chars[_int32TMP++] = char.ConvertFromUtf32(_int32TMP2);
                                    }
                                }
                            }
                            else if ((_byteTmp & _0xE0) == _0xC0)
                            {
                                _int32TMP3 = 1;
                                _int32TMP2 = _byteTmp & _0x1F;
                            }
                            else if ((_byteTmp & _0xF0) == _0xE0)
                            {
                                _int32TMP3 = 2;
                                _int32TMP2 = _byteTmp & _0x0F;
                            }
                            else if ((_byteTmp & _0xF8) == _0xF0)
                            {
                                _int32TMP3 = 3;
                                _int32TMP2 = _byteTmp & _0x07;
                            }
                        }

                        _parameters[_iter] = string.Concat(chars);
                        _bufferOffset += _int32TMP4 - _bufferOffset;
                        break;
                    }
                    case Types.VRCPlayerApi:
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP);
                        _parameters[_iter] = new DataToken(VRCPlayerApi.GetPlayerById(_int32TMP));
                        break;
                    case Types.Color:
                        _singleValue = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _singleValue2 = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _singleValue3 = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _singleValue4 = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _parameters[_iter] =
                            new DataToken(new Color(_singleValue, _singleValue2, _singleValue3, _singleValue4));
                        break;
                    case Types.Color32:
                        _parameters[_iter] = new DataToken(new Color32(syncBuffer[_bufferOffset],
                            syncBuffer[_bufferOffset + 1],
                            syncBuffer[_bufferOffset + 2], syncBuffer[_bufferOffset + 3]));
                        _bufferOffset += 4;
                        break;
                    case Types.Vector2:
                        _singleValue = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _singleValue2 = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _parameters[_iter] = new DataToken(new Vector2(_singleValue, _singleValue2));
                        break;
                    case Types.Vector2Int:
#if NETCALLER_USE_VARIABLE_SERIALIZATION
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP);
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP2);
                        _parameters[_iter] = new DataToken(new Vector2Int(_int32TMP, _int32TMP2));
#else
                        _parameters[_iter] = new DataToken(new Vector2Int(
                            syncBuffer[_bufferOffset] << Bit24 |
                            syncBuffer[_bufferOffset + 1] << Bit16 |
                            syncBuffer[_bufferOffset + 2] << Bit8 |
                            syncBuffer[_bufferOffset + 3],
                            syncBuffer[_bufferOffset + 4] << Bit24 |
                            syncBuffer[_bufferOffset + 5] << Bit16 |
                            syncBuffer[_bufferOffset + 6] << Bit8 |
                            syncBuffer[_bufferOffset + 7]));
#endif
                        break;
                    case Types.Vector3:
                        _singleValue = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _singleValue2 = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _singleValue3 = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _parameters[_iter] = new DataToken(new Vector3(_singleValue, _singleValue2, _singleValue3));
                        break;
                    case Types.Vector3Int:
#if NETCALLER_USE_VARIABLE_SERIALIZATION
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP);
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP2);
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP3);
                        _parameters[_iter] = new DataToken(new Vector3Int(_int32TMP, _int32TMP2, _int32TMP3));
#else
                        _parameters[_iter] = new DataToken(new Vector3Int(
                            syncBuffer[_bufferOffset] << Bit24 |
                            syncBuffer[_bufferOffset + 1] << Bit16 |
                            syncBuffer[_bufferOffset + 2] << Bit8 |
                            syncBuffer[_bufferOffset + 3],
                            syncBuffer[_bufferOffset + 4] << Bit24 |
                            syncBuffer[_bufferOffset + 5] << Bit16 |
                            syncBuffer[_bufferOffset + 6] << Bit8 |
                            syncBuffer[_bufferOffset + 7],
                            syncBuffer[_bufferOffset + 8] << Bit24 |
                            syncBuffer[_bufferOffset + 9] << Bit16 |
                            syncBuffer[_bufferOffset + 10] << Bit8 |
                            syncBuffer[_bufferOffset + 11])
                        );
                        
                        _bufferOffset += 12;
                        #endif
                        break;
                    case Types.Vector4:
                        _singleValue = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _singleValue2 = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _singleValue3 = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _singleValue4 = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _parameters[_iter] =
                            new DataToken(new Vector4(_singleValue, _singleValue2, _singleValue3, _singleValue4));
                        break;
                    case Types.Quaternion:
                        _singleValue = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _singleValue2 = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _singleValue3 = syncBuffer.ReadFloat(_bufferOffset);
                        _bufferOffset += 4;
                        _singleValue4 = syncBuffer.ReadFloat(_bufferOffset);
                        _parameters[_iter] =
                            new DataToken(new Quaternion(_singleValue, _singleValue2, _singleValue3, _singleValue4));
                        break;
                    case Types.DateTime:
                        _parameters[_iter] = new DataToken(DateTime.FromBinary(
                            (long) syncBuffer[_bufferOffset] << Bit56 |
                            (long) syncBuffer[_bufferOffset + 1] << Bit48 |
                            (long) syncBuffer[_bufferOffset + 2] << Bit40 |
                            (long) syncBuffer[_bufferOffset + 3] << Bit32 |
                            (long) syncBuffer[_bufferOffset + 4] << Bit24 |
                            (long) syncBuffer[_bufferOffset + 5] << Bit16 |
                            (long) syncBuffer[_bufferOffset + 6] << Bit8 |
                            syncBuffer[_bufferOffset + 7]));
                        _bufferOffset += 8;
                        break;
                    //Arrays
                    case Types.BooleanA:
                    {
                        _boolA = new bool[length];
                        
                        //Bit unpacking
                        for (var i = 0; i < length; i++)
                        {
                            _boolA[i] = (syncBuffer[_bufferOffset] & (1 << i)) != 0;
                            
                            if (i % 8 == 7) _bufferOffset++;
                        }

                        _parameters[_iter] = new DataToken(_boolA);
                        break;
                    }
                    case Types.ByteA:
                    {
                        _byteA = new byte[length];
                        

                        Array.Copy(syncBuffer, _bufferOffset, _byteA, 0, _byteA.Length);
                        _parameters[_iter] = new DataToken(_byteA);
                        _bufferOffset += _byteA.Length;
                        break;
                    }
                    case Types.SByteA:
                    {
                        _sbyteA = new sbyte[length];

                        for (var i = 0; i < length; i++)
                        {
                            _int32TMP = syncBuffer[_bufferOffset];
                            if (_int32TMP >= 0x80) _int32TMP -= _0xFF;
                            _sbyteA[i] = Convert.ToSByte(_int32TMP);
                            _bufferOffset++;
                        }

                        _parameters[_iter] = new DataToken(_sbyteA);
                        break;
                    }
                    case Types.Int16A:
                    {
                        _int16A = new short[length];

                        for (var i = 0; i < length; i++)
                        {
#if NETCALLER_USE_VARIABLE_SERIALIZATION
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int16Value);
                            _int16A[i] = _int16Value;
#else
                            _int16A[i] = Convert.ToInt16((syncBuffer[_bufferOffset] << Bit8) | syncBuffer[_bufferOffset + 1]);
                            _bufferOffset += 2;
#endif
                        }

                        _parameters[_iter] = new DataToken(_int16A);
                        break;
                    }
                    case Types.UInt16A:
                    {
                        _uint16A = new ushort[length];

                        for (var i = 0; i < length; i++)
                        {
                            #if NETCALLER_USE_VARIABLE_SERIALIZATION
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _uint16Value);
                            _uint16A[i] = _uint16Value;
                            #else
                            _uint16A[i] = Convert.ToUInt16((syncBuffer[_bufferOffset] << Bit8) | syncBuffer[_bufferOffset + 1]);
                            #endif
                        }

                        _parameters[_iter] = new DataToken(_uint16A);
                        break;
                    }
                    case Types.Int32A:
                    {
                        _int32A = new int[length];
                        
                        for (var i = 0; i < length; i++)
                        {
                            #if NETCALLER_USE_VARIABLE_SERIALIZATION
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP);
                            _int32A[i] = _int32TMP;
                            #else
                            
                            _int32A[i] = (syncBuffer[_bufferOffset] << Bit24) | (syncBuffer[_bufferOffset + 1] << Bit16) |
                                         (syncBuffer[_bufferOffset + 2] << Bit8) | syncBuffer[_bufferOffset + 3];
                            _bufferOffset += 4;
                            #endif
                        }

                        _parameters[_iter] = new DataToken(_int32A);
                        break;
                    }
                    case Types.UInt32A:
                    {
                        _uint32A = new uint[length];


                        for (var i = 0; i < length; i++)
                        {
                            #if NETCALLER_USE_VARIABLE_SERIALIZATION
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _uint32Value);
                            _uint32A[i] = Convert.ToUInt32(_int32TMP);
                            #else
                            _uint32A[i] = (uint) (syncBuffer[_bufferOffset] << Bit24) |
                                          (uint) (syncBuffer[_bufferOffset + 1] << Bit16) |
                                          (uint) (syncBuffer[_bufferOffset + 2] << Bit8) | syncBuffer[_bufferOffset + 3];
                            _bufferOffset += 4;
                            #endif
                        }

                        _parameters[_iter] = new DataToken(_uint32A);
                        break;
                    }
                    case Types.Int64A:
                    {
                        _int64A = new long[length];

                        for (var i = 0; i < length; i++)
                        {
                            #if NETCALLER_USE_VARIABLE_SERIALIZATION
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int64Value);
                            _int64A[i] = _int64Value;
                            #else
                            _int64A[i] = ((long) syncBuffer[_bufferOffset] << Bit56) |
                                         ((long) syncBuffer[_bufferOffset + 1] << Bit48) |
                                         ((long) syncBuffer[_bufferOffset + 2] << Bit40) |
                                         ((long) syncBuffer[_bufferOffset + 3] << Bit32) |
                                         ((long) syncBuffer[_bufferOffset + 4] << Bit24) |
                                         ((long) syncBuffer[_bufferOffset + 5] << Bit16) |
                                         ((long) syncBuffer[_bufferOffset + 6] << Bit8) | syncBuffer[_bufferOffset + 7];
                            _bufferOffset += 8;
                            #endif

                        }

                        _parameters[_iter] = new DataToken(_int64A);
                        break;
                    }
                    case Types.UInt64A:
                    {
                        _uint64A = new ulong[length];

                        for (var i = 0; i < length; i++)
                        {
                            #if NETCALLER_USE_VARIABLE_SERIALIZATION
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _uint64Value);
                            _uint64A[i] = _uint64Value;
                            #else
                            _uint64A[i] = ((ulong) syncBuffer[_bufferOffset] << Bit56) |
                                          ((ulong) syncBuffer[_bufferOffset + 1] << Bit48) |
                                          ((ulong) syncBuffer[_bufferOffset + 2] << Bit40) |
                                          ((ulong) syncBuffer[_bufferOffset + 3] << Bit32) |
                                          ((ulong) syncBuffer[_bufferOffset + 4] << Bit24) |
                                          ((ulong) syncBuffer[_bufferOffset + 5] << Bit16) |
                                          ((ulong) syncBuffer[_bufferOffset + 6] << Bit8) | syncBuffer[_bufferOffset + 7];
                            _bufferOffset += 8;
                            #endif
                        }

                        _parameters[_iter] = new DataToken(_uint64A);
                        break;
                    }
                    case Types.SingleA:
                    {
                        _singleA = new float[length];
                        
                        for (var i = 0; i < length; i++)
                        {
                            _singleA[i] = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                        }

                        _parameters[_iter] = new DataToken(_singleA);
                        break;
                    }
                    case Types.DoubleA:
                    {
                        _doubleA = new double[length];
                        
                        for (var i = 0; i < length; i++)
                        {
                            _doubleA[i] = syncBuffer.ReadDouble(_bufferOffset);
                            _bufferOffset += 8;
                        }

                        _parameters[_iter] = new DataToken(_doubleA);
                        break;
                    }
                    case Types.DecimalA:
                    {
                        _decimalA = new decimal[length];


                        for (var i = 0; i < length; i++)
                        {
                            _decimalA[i] = new decimal(new int[]
                            {
                                syncBuffer[_bufferOffset] << Bit24 | syncBuffer[_bufferOffset + 1] << Bit16 |
                                syncBuffer[_bufferOffset + 2] << Bit8 | syncBuffer[_bufferOffset + 3],
                                syncBuffer[_bufferOffset + 4] << Bit24 | syncBuffer[_bufferOffset + 5] << Bit16 |
                                syncBuffer[_bufferOffset + 6] << Bit8 | syncBuffer[_bufferOffset + 7],
                                syncBuffer[_bufferOffset + 8] << Bit24 | syncBuffer[_bufferOffset + 9] << Bit16 |
                                syncBuffer[_bufferOffset + 10] << Bit8 | syncBuffer[_bufferOffset + 11],
                                syncBuffer[_bufferOffset + 12] << Bit24 | syncBuffer[_bufferOffset + 13] << Bit16 |
                                syncBuffer[_bufferOffset + 14] << Bit8 | syncBuffer[_bufferOffset + 15]
                            });
                            _bufferOffset += 16;
                        }

                        _parameters[_iter] = new DataToken(_decimalA);
                        break;
                    }
                    case Types.StringA:
                    {
                        _int32TMP = 0;
                        _int32TMP2 = 0;
                        _int32TMP3 = 0;
                        _stringA = new string[length];

                        _int32TMP4 = _bufferOffset;

                        for (int i = 0; i < length; i++)
                        {
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _uint32Value); // String length
                            chars = new string[_uint32Value];
                            
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _uint32Value); // String length in bytes
                            
                            _int32TMP4 = Convert.ToInt32(_bufferOffset + _uint32Value);
                            _int32TMP = 0;
                            _int32TMP2 = 0;
                            _int32TMP3 = 0;
                            for (var j = _bufferOffset; j < _int32TMP4; j++)
                            {
                                _byteTmp = syncBuffer[j];
                                if ((_byteTmp & _0x80) == 0)
                                {
                                    chars[_int32TMP++] = ((char) _byteTmp).ToString();
                                }
                                else if ((_byteTmp & _0xC0) == _0x80)
                                {
                                    if (_int32TMP3 > 0)
                                    {
                                        _int32TMP2 = _int32TMP2 << 6 | (_byteTmp & _0x3F);
                                        _int32TMP3--;
                                        if (_int32TMP3 == 0)
                                        {
                                            chars[_int32TMP++] = char.ConvertFromUtf32(_int32TMP2);
                                        }
                                    }
                                }
                                else if ((_byteTmp & _0xE0) == _0xC0)
                                {
                                    _int32TMP3 = 1;
                                    _int32TMP2 = _byteTmp & _0x1F;
                                }
                                else if ((_byteTmp & _0xF0) == _0xE0)
                                {
                                    _int32TMP3 = 2;
                                    _int32TMP2 = _byteTmp & _0x0F;
                                }
                                else if ((_byteTmp & _0xF8) == _0xF0)
                                {
                                    _int32TMP3 = 3;
                                    _int32TMP2 = _byteTmp & _0x07;
                                }
                            }

                            _stringA[i] = string.Concat(chars);
                            _bufferOffset += _int32TMP4 - _bufferOffset;
                        }
                        
                        break;
                    }
                    case Types.VRCPlayerApiA:
                    {
                        _vrcPlayerApiA = new VRCPlayerApi[length];

                        for (var i = 0; i < length; i++)
                        {
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _uint32Value);
                            _vrcPlayerApiA[i] = VRCPlayerApi.GetPlayerById(Convert.ToInt32(_uint32Value));
                        }

                        _parameters[_iter] = new DataToken(_vrcPlayerApiA);
                        break;
                    }
                    case Types.ColorA:
                    {
                        _colorA = new Color[length];

                        for (var i = 0; i < length; i++)
                        {
                            _singleValue = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _singleValue2 = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _singleValue3 = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _singleValue4 = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _colorA[i] = new Color(_singleValue, _singleValue2, _singleValue3, _singleValue4);
                        }

                        _parameters[_iter] = new DataToken(_colorA);
                        break;
                    }
                    case Types.Color32A:
                    {
                        _color32A = new Color32[length];

                        for (var i = 0; i < length; i++)
                        {
                            _color32A[i] = new Color32(syncBuffer[_bufferOffset], syncBuffer[_bufferOffset + 1],
                                syncBuffer[_bufferOffset + 2], syncBuffer[_bufferOffset + 3]);
                            _bufferOffset += 4;
                        }

                        _parameters[_iter] = new DataToken(_color32A);
                        break;
                    }
                    case Types.Vector2A:
                    {
                        _vector2A = new Vector2[length];

                        for (var i = 0; i < length; i++)
                        {
                            _singleValue = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _singleValue2 = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _vector2A[i] = new Vector2(_singleValue, _singleValue2);
                        }

                        _parameters[_iter] = new DataToken(_vector2A);
                        break;
                    }
                    case Types.Vector2IntA:
                    {
                        _vector2IntA = new Vector2Int[length];

                        for (var i = 0; i < length; i++)
                        {
                            #if NETCALLER_USE_VARIABLE_SERIALIZATION
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP);
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP2);
                            _vector2IntA[i] = new Vector2Int(_int32TMP, _int32TMP2);
                            #else
                            _vector2IntA[i] = new Vector2Int(
                                syncBuffer[_bufferOffset] << Bit24 | syncBuffer[_bufferOffset + 1] << Bit16 |
                                syncBuffer[_bufferOffset + 2] << Bit8 | syncBuffer[_bufferOffset + 3],
                                syncBuffer[_bufferOffset + 4] << Bit24 | syncBuffer[_bufferOffset + 5] << Bit16 |
                                syncBuffer[_bufferOffset + 6] << Bit8 | syncBuffer[_bufferOffset + 7]);
                            _bufferOffset += 8;
                            #endif
                        }

                        _parameters[_iter] = new DataToken(_vector2IntA);
                        break;
                    }
                    case Types.Vector3A:
                    {
                        _vector3A = new Vector3[length];

                        for (var i = 0; i < length; i++)
                        {
                            _singleValue = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _singleValue2 = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _singleValue3 = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _vector3A[i] = new Vector3(_singleValue, _singleValue2, _singleValue3);
                        }

                        _parameters[_iter] = new DataToken(_vector3A);
                        break;
                    }
                    case Types.Vector3IntA:
                    {
                        _vector3IntA = new Vector3Int[length];

                        for (var i = 0; i < length; i++)
                        {
                            #if NETCALLER_USE_VARIABLE_SERIALIZATION
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP);
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP2);
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP3);
                            _vector3IntA[i] = new Vector3Int(_int32TMP, _int32TMP2, _int32TMP3);
                            #else
                            _vector3IntA[i] = new Vector3Int(
                                syncBuffer[_bufferOffset] << Bit24 | syncBuffer[_bufferOffset + 1] << Bit16 |
                                syncBuffer[_bufferOffset + 2] << Bit8 | syncBuffer[_bufferOffset + 3],
                                syncBuffer[_bufferOffset + 4] << Bit24 | syncBuffer[_bufferOffset + 5] << Bit16 |
                                syncBuffer[_bufferOffset + 6] << Bit8 | syncBuffer[_bufferOffset + 7],
                                syncBuffer[_bufferOffset + 8] << Bit24 | syncBuffer[_bufferOffset + 9] << Bit16 |
                                syncBuffer[_bufferOffset + 10] << Bit8 | syncBuffer[_bufferOffset + 11]);
                            _bufferOffset += 12;
                            #endif
                        }

                        _parameters[_iter] = new DataToken(_vector3IntA);
                        break;
                    }
                    case Types.Vector4A:
                    {
                        _vector4A = new Vector4[length];

                        for (var i = 0; i < length; i++)
                        {
                            _singleValue = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _singleValue2 = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _singleValue3 = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _singleValue4 = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _vector4A[i] = new Vector4(_singleValue, _singleValue2, _singleValue3, _singleValue4);
                        }

                        _parameters[_iter] = new DataToken(_vector4A);
                        break;
                    }
                    case Types.QuaternionA:
                    {
                        _quaternionA = new Quaternion[length];

                        for (var i = 0; i < length; i++)
                        {
                            _singleValue = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _singleValue2 = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _singleValue3 = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _singleValue4 = syncBuffer.ReadFloat(_bufferOffset);
                            _bufferOffset += 4;
                            _quaternionA[i] = new Quaternion(_singleValue, _singleValue2, _singleValue3, _singleValue4);
                        }

                        _parameters[_iter] = new DataToken(_quaternionA);
                        break;
                    }
                    case Types.Null:
                        _parameters[_iter] = new DataToken();
                        break;
                }
            }
        }

        private void SetUdonVariable(string variable, DataToken token)
        {
            var type = token.TokenType;
            int typeId;
            if (type != TokenType.Reference)
            {
                typeId = Array.IndexOf(_tokenMap, type);
            }
            else
            {
                var reference = token.Reference;
                typeId = Array.IndexOf(_typeMap, reference.GetType());
            }

            Types enumType;
            if (typeId == -1)
                enumType = Types.Null;
            else
                enumType = (Types) typeId;
            
            Log($"Setting {variable} to {token} ({enumType})");

            switch (enumType)
            {
                case Types.Boolean:
                    _targetScript.SetProgramVariable(variable, token.Boolean);
                    break;
                case Types.Byte:
                    _targetScript.SetProgramVariable(variable, token.Byte);
                    break;
                case Types.SByte:
                    _targetScript.SetProgramVariable(variable, token.SByte);
                    break;
                case Types.Int16:
                    _targetScript.SetProgramVariable(variable, token.Short);
                    break;
                case Types.UInt16:
                    _targetScript.SetProgramVariable(variable, token.UShort);
                    break;
                case Types.Int32:
                    _targetScript.SetProgramVariable(variable, token.Int);
                    break;
                case Types.UInt32:
                    _targetScript.SetProgramVariable(variable, token.UInt);
                    break;
                case Types.Int64:
                    _targetScript.SetProgramVariable(variable, token.Long);
                    break;
                case Types.UInt64:
                    _targetScript.SetProgramVariable(variable, token.ULong);
                    break;
                case Types.Single:
                    _targetScript.SetProgramVariable(variable, token.Float);
                    break;
                case Types.Double:
                    _targetScript.SetProgramVariable(variable, token.Double);
                    break;
                case Types.String:
                    _targetScript.SetProgramVariable(variable, token.String);
                    break;
                case Types.Decimal:
                    _targetScript.SetProgramVariable(variable, (decimal) token.Reference);
                    break;
                case Types.VRCPlayerApi:
                    _targetScript.SetProgramVariable(variable, (VRCPlayerApi) token.Reference);
                    break;
                case Types.Color:
                    _targetScript.SetProgramVariable(variable, (Color) token.Reference);
                    break;
                case Types.Color32:
                    _targetScript.SetProgramVariable(variable, (Color32) token.Reference);
                    break;
                case Types.Vector2:
                    _targetScript.SetProgramVariable(variable, (Vector2) token.Reference);
                    break;
                case Types.Vector2Int:
                    _targetScript.SetProgramVariable(variable, (Vector2Int) token.Reference);
                    break;
                case Types.Vector3:
                    _targetScript.SetProgramVariable(variable, (Vector3) token.Reference);
                    break;
                case Types.Vector3Int:
                    _targetScript.SetProgramVariable(variable, (Vector3Int) token.Reference);
                    break;
                case Types.Vector4:
                    _targetScript.SetProgramVariable(variable, (Vector4) token.Reference);
                    break;
                case Types.Quaternion:
                    _targetScript.SetProgramVariable(variable, (Quaternion) token.Reference);
                    break;
                case Types.DateTime:
                    _targetScript.SetProgramVariable(variable, (DateTime) token.Reference);
                    break;
                case Types.BooleanA:
                    _targetScript.SetProgramVariable(variable, (bool[]) token.Reference);
                    break;
                case Types.ByteA:
                    _targetScript.SetProgramVariable(variable, (byte[]) token.Reference);
                    break;
                case Types.SByteA:
                    _targetScript.SetProgramVariable(variable, (sbyte[]) token.Reference);
                    break;
                case Types.Int16A:
                    _targetScript.SetProgramVariable(variable, (short[]) token.Reference);
                    break;
                case Types.UInt16A:
                    _targetScript.SetProgramVariable(variable, (ushort[]) token.Reference);
                    break;
                case Types.Int32A:
                    _targetScript.SetProgramVariable(variable, (int[]) token.Reference);
                    break;
                case Types.UInt32A:
                    _targetScript.SetProgramVariable(variable, (uint[]) token.Reference);
                    break;
                case Types.Int64A:
                    _targetScript.SetProgramVariable(variable, (long[]) token.Reference);
                    break;
                case Types.UInt64A:
                    _targetScript.SetProgramVariable(variable, (ulong[]) token.Reference);
                    break;
                case Types.SingleA:
                    _targetScript.SetProgramVariable(variable, (float[]) token.Reference);
                    break;
                case Types.DoubleA:
                    _targetScript.SetProgramVariable(variable, (double[]) token.Reference);
                    break;
                case Types.DecimalA:
                    _targetScript.SetProgramVariable(variable, (decimal[]) token.Reference);
                    break;
                case Types.StringA:
                    _targetScript.SetProgramVariable(variable, (string[]) token.Reference);
                    break;
                case Types.VRCPlayerApiA:
                    _targetScript.SetProgramVariable(variable, (VRCPlayerApi[]) token.Reference);
                    break;
                case Types.ColorA:
                    _targetScript.SetProgramVariable(variable, (Color[]) token.Reference);
                    break;
                case Types.Color32A:
                    _targetScript.SetProgramVariable(variable, (Color32[]) token.Reference);
                    break;
                case Types.Vector2A:
                    _targetScript.SetProgramVariable(variable, (Vector2[]) token.Reference);
                    break;
                case Types.Vector2IntA:
                    _targetScript.SetProgramVariable(variable, (Vector2Int[]) token.Reference);
                    break;
                case Types.Vector3A:
                    _targetScript.SetProgramVariable(variable, (Vector3[]) token.Reference);
                    break;
                case Types.Vector3IntA:
                    _targetScript.SetProgramVariable(variable, (Vector3Int[]) token.Reference);
                    break;
                case Types.Vector4A:
                    _targetScript.SetProgramVariable(variable, (Vector4[]) token.Reference);
                    break;
                case Types.QuaternionA:
                    _targetScript.SetProgramVariable(variable, (Quaternion[]) token.Reference);
                    break;
                case Types.Null:
                    _targetScript.SetProgramVariable(variable, null);
                    break;
            }
        }


        private void OnEnable()
        {
            if (_startRun) return;
            _startRun = true;
            _debug = networkManager.debug;
        }


        private void Log(object log)
        {
            if (_debug) Debug.Log($"<color=#00FFFF>[NetCaller]</color> {log}");
        }

        private void LogWarning(object log)
        {
            if (_debug) Debug.LogWarning($"<color=#FF8000>[WARN]</color> <color=#00FFFF>[NetCaller]</color> {log}");
        }

        private void LogError(object log)
        {
            Debug.LogError($"<color=#FF0000>[WARN]</color> <color=#00FFFF>[NetCaller]</color> {log}");
        }
    }
}