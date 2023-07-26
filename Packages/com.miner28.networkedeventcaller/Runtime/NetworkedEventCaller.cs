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
            _0xFFFF = 0xFFFF;

        #endregion

        #region StorageVariables

        private bool _tmpBool;
        private byte _byteTmp;
        private sbyte _sbyteValue;
        private short _int16Value;
        private ushort _ushortLength;
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
        [UdonSynced] private int _sentOutMethods;
        [UdonSynced, NonSerialized] public int methodTarget;
        [UdonSynced] private int _scriptTarget;

        [UdonSynced] private byte[] _buffer = new byte[0];
        [UdonSynced] private Types[] _types = new Types[0];
        [UdonSynced] private ushort[] _lengths = new ushort[0];

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
                Log($"PreSerialization - {methodTarget} - {_buffer.Length} - {_types.Length} - {_lengths.Length}");
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            if (!result.success)
            {
                LogWarning($"Failed Serialization - {result.byteCount}");
            }
        }
        
        internal void _PrepareSend(SyncTarget target, string method, int scriptTarget, DataToken[] data)
        {
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
            
            if (target != SyncTarget.Others)
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
            int requiredLength = 0;
            var sIndex = Array.IndexOf(sceneInterfacesIds, scriptTarget);
            if (sIndex == -1)
            {
                LogError($"Invalid script target: {scriptTarget} - {method} can't send method if target is invalid");
                return;
            }

            if (_types.Length != data.Length) _types = new Types[data.Length];
            if (_lengths.Length != data.Length) _lengths = new ushort[data.Length];
            for (int y = 0; y < data.Length; y++)
            {
                var type = data[y].TokenType;
                int typeId;
                if (type != TokenType.Reference)
                {
                    typeId = Array.IndexOf(_tokenMap, type);
                }
                else
                {
                    var reference = data[y].Reference;
                    typeId = Array.IndexOf(_typeMap, reference.GetType());
                }

                Types enumType;
                if (typeId == -1)
                {
                    enumType = Types.Null;
                }
                else
                {
                    enumType = (Types) typeId;
                }

                _types[y] = enumType;

                ushort length;
                switch (enumType)
                {
                    case Types.Boolean:
                    case Types.Byte:
                    case Types.SByte:
                        requiredLength += 1;
                        break;
                    case Types.Int16:
                    case Types.UInt16:
                        requiredLength += 2;
                        break;
                    case Types.Int32:
                    case Types.UInt32:
                        requiredLength += 4;
                        break;
                    case Types.Int64:
                    case Types.UInt64:
                        requiredLength += 8;
                        break;
                    case Types.Single:
                        requiredLength += 4;
                        break;
                    case Types.Double:
                        requiredLength += 8;
                        break;
                    case Types.Decimal:
                        requiredLength += 16;
                        break;
                    case Types.String:
                        length = (ushort) BitConverter.GetStringSizeInBytes(data[y].String);
                        requiredLength += length;
                        _lengths[y] = length;
                        break;
                    case Types.VRCPlayerApi:
                        requiredLength += 4;
                        break;
                    case Types.Color:
                        requiredLength += 16;
                        break;
                    case Types.Color32:
                        requiredLength += 4;
                        break;
                    case Types.Vector2:
                    case Types.Vector2Int:
                        requiredLength += 8;
                        break;
                    case Types.Vector3:
                    case Types.Vector3Int:
                        requiredLength += 12;
                        break;
                    case Types.Vector4:
                    case Types.Quaternion:
                        requiredLength += 16;
                        break;
                    case Types.DateTime:
                        requiredLength += 8;
                        break;
                    //Arrays
                    case Types.BooleanA:
                        length = (ushort) ((bool[]) data[y].Reference).Length;
                        requiredLength += length;
                        _lengths[y] = length;
                        break;
                    case Types.ByteA:
                        length = (ushort) ((byte[]) data[y].Reference).Length;
                        requiredLength += length;
                        _lengths[y] = length;
                        break;
                    case Types.SByteA:
                        length = (ushort) ((sbyte[]) data[y].Reference).Length;
                        requiredLength += length;
                        _lengths[y] = length;
                        break;
                    case Types.Int16A:
                        length = (ushort) ((short[]) data[y].Reference).Length;
                        requiredLength += length * 2;
                        _lengths[y] = length;
                        break;
                    case Types.UInt16A:
                        length = (ushort) ((ushort[]) data[y].Reference).Length;
                        requiredLength += length * 2;
                        _lengths[y] = length;
                        break;
                    case Types.Int32A:
                        length = (ushort) ((int[]) data[y].Reference).Length;
                        requiredLength += length * 4;
                        _lengths[y] = length;
                        break;
                    case Types.UInt32A:
                        length = (ushort) ((uint[]) data[y].Reference).Length;
                        requiredLength += length * 4;
                        _lengths[y] = length;
                        break;
                    case Types.Int64A:
                        length = (ushort) ((long[]) data[y].Reference).Length;
                        requiredLength += length * 8;
                        _lengths[y] = length;
                        break;
                    case Types.UInt64A:
                        length = (ushort) ((ulong[]) data[y].Reference).Length;
                        requiredLength += length * 8;
                        _lengths[y] = length;
                        break;
                    case Types.SingleA:
                        length = (ushort) ((float[]) data[y].Reference).Length;
                        requiredLength += length * 4;
                        _lengths[y] = length;
                        break;
                    case Types.DoubleA:
                        length = (ushort) ((double[]) data[y].Reference).Length;
                        requiredLength += length * 8;
                        _lengths[y] = length;
                        break;
                    case Types.DecimalA:
                        length = (ushort) ((decimal[]) data[y].Reference).Length;
                        requiredLength += length * 16;
                        _lengths[y] = length;
                        break;
                    case Types.StringA:
                    {
                        _stringA = (string[]) data[y].Reference;
                        length = (ushort) (_stringA.Length - 1);
                        for (int i = 0; i < _stringA.Length; i++)
                        {
                            length += (ushort) BitConverter.GetStringSizeInBytes(_stringA[i]);
                        }

                        requiredLength += length;
                        _lengths[y] = length;
                        break;
                    }
                    case Types.VRCPlayerApiA:
                        length = (ushort) ((VRCPlayerApi[]) data[y].Reference).Length;
                        requiredLength += length * 4;
                        _lengths[y] = length;
                        break;
                    case Types.ColorA:
                        length = (ushort) ((Color[]) data[y].Reference).Length;
                        requiredLength += length * 16;
                        _lengths[y] = length;
                        break;
                    case Types.Color32A:
                        length = (ushort) ((Color32[]) data[y].Reference).Length;
                        requiredLength += length * 4;
                        _lengths[y] = length;
                        break;
                    case Types.Vector2A:
                        length = (ushort) ((Vector2[]) data[y].Reference).Length;
                        requiredLength += length * 8;
                        _lengths[y] = length;
                        break;
                    case Types.Vector2IntA:
                        length = (ushort) ((Vector2Int[]) data[y].Reference).Length;
                        requiredLength += length * 8;
                        _lengths[y] = length;
                        break;
                    case Types.Vector3A:
                        length = (ushort) ((Vector3[]) data[y].Reference).Length;
                        requiredLength += length * 12;
                        _lengths[y] = length;
                        break;
                    case Types.Vector3IntA:
                        length = (ushort) ((Vector3Int[]) data[y].Reference).Length;
                        requiredLength += length * 12;
                        _lengths[y] = length;
                        break;
                    case Types.Vector4A:
                        length = (ushort) ((Vector4[]) data[y].Reference).Length;
                        requiredLength += length * 16;
                        _lengths[y] = length;
                        break;
                    case Types.QuaternionA:
                        length = (ushort) ((Quaternion[]) data[y].Reference).Length;
                        requiredLength += length * 16;
                        _lengths[y] = length;
                        break;
                }
            }

            if (_buffer.Length != requiredLength) _buffer = new byte[requiredLength];

            _bufferOffset = 0;

            for (_iter = 0; _iter < data.Length; _iter++)
            {
                var enumType = _types[_iter];

                switch (enumType)
                {
                    case Types.Boolean:
                        _buffer[_bufferOffset] = data[_iter].Boolean ? BYTE_ONE : BYTE_ZERO;
                        _bufferOffset++;
                        break;
                    case Types.Byte:
                        _buffer[_bufferOffset] = data[_iter].Byte;
                        _bufferOffset++;
                        break;
                    case Types.SByte:
                        _sbyteValue = data[_iter].SByte;
                        _buffer[_bufferOffset] = (byte) (_sbyteValue < 0 ? (_sbyteValue + _0xFF) : _sbyteValue);
                        _bufferOffset++;
                        break;
                    case Types.Int16:
                        _int16Value = data[_iter].Short;
                        _int32TMP = _int16Value < 0 ? (_int16Value + 0xFFFF) : _int16Value;
                        _buffer[_bufferOffset] = (byte) (_int32TMP >> Bit8);
                        _buffer[_bufferOffset + 1] = (byte) (_int32TMP & 0xFF);
                        _bufferOffset += 2;
                        break;
                    case Types.UInt16:
                        _uint16Value = data[_iter].UShort;
                        _buffer[_bufferOffset] = (byte) ((_uint16Value >> Bit8));
                        _buffer[_bufferOffset + 1] = (byte) (_uint16Value & 0xFF);
                        _bufferOffset += 2;
                        break;
                    case Types.Int32:
                        _int32TMP2 = data[_iter].Int;
                        _buffer[_bufferOffset] = (byte) ((_int32TMP2 >> Bit24) & 0xFF);
                        _buffer[_bufferOffset + 1] = (byte) ((_int32TMP2 >> Bit16) & 0xFF);
                        _buffer[_bufferOffset + 2] = (byte) ((_int32TMP2 >> Bit8) & 0xFF);
                        _buffer[_bufferOffset + 3] = (byte) (_int32TMP2 & 0xFF);
                        _bufferOffset += 4;
                        break;
                    case Types.UInt32:
                        _uint32Value = data[_iter].UInt;
                        _buffer[_bufferOffset] = (byte) ((_uint32Value >> Bit24) & 255u);
                        _buffer[_bufferOffset + 1] = (byte) ((_uint32Value >> Bit16) & 255u);
                        _buffer[_bufferOffset + 2] = (byte) ((_uint32Value >> Bit8) & 255u);
                        _buffer[_bufferOffset + 3] = (byte) (_uint32Value & 255u);
                        _bufferOffset += 4;
                        break;
                    case Types.Int64:
                        _int64Value = data[_iter].Long;
                        _buffer[_bufferOffset] = (byte) ((_int64Value >> Bit56) & 0xFF);
                        _buffer[_bufferOffset + 1] = (byte) ((_int64Value >> Bit48) & 0xFF);
                        _buffer[_bufferOffset + 2] = (byte) ((_int64Value >> Bit40) & 0xFF);
                        _buffer[_bufferOffset + 3] = (byte) ((_int64Value >> Bit32) & 0xFF);
                        _buffer[_bufferOffset + 4] = (byte) ((_int64Value >> Bit24) & 0xFF);
                        _buffer[_bufferOffset + 5] = (byte) ((_int64Value >> Bit16) & 0xFF);
                        _buffer[_bufferOffset + 6] = (byte) ((_int64Value >> Bit8) & 0xFF);
                        _buffer[_bufferOffset + 7] = (byte) (_int64Value & 0xFF);
                        _bufferOffset += 8;
                        break;
                    case Types.UInt64:
                        _uint64Value = data[_iter].ULong;
                        _buffer[_bufferOffset] = (byte) ((_uint64Value >> Bit56) & 255ul);
                        _buffer[_bufferOffset + 1] = (byte) ((_uint64Value >> Bit48) & 255ul);
                        _buffer[_bufferOffset + 2] = (byte) ((_uint64Value >> Bit40) & 255ul);
                        _buffer[_bufferOffset + 3] = (byte) ((_uint64Value >> Bit32) & 255ul);
                        _buffer[_bufferOffset + 4] = (byte) ((_uint64Value >> Bit24) & 255ul);
                        _buffer[_bufferOffset + 5] = (byte) ((_uint64Value >> Bit16) & 255ul);
                        _buffer[_bufferOffset + 6] = (byte) ((_uint64Value >> Bit8) & 255ul);
                        _buffer[_bufferOffset + 7] = (byte) (_uint64Value & 255ul);
                        _bufferOffset += 8;
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

                        _buffer[_bufferOffset] = (byte) ((_uintTmp >> Bit24) & 255u);
                        _buffer[_bufferOffset + 1] = (byte) ((_uintTmp >> Bit16) & 255u);
                        _buffer[_bufferOffset + 2] = (byte) ((_uintTmp >> Bit8) & 255u);
                        _buffer[_bufferOffset + 3] = (byte) (_uintTmp & 255u);
                        _bufferOffset += 4;
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

                        _buffer[_bufferOffset] = (byte) ((doubleTmp >> Bit56) & 255ul);
                        _buffer[_bufferOffset + 1] = (byte) ((doubleTmp >> Bit48) & 255ul);
                        _buffer[_bufferOffset + 2] = (byte) ((doubleTmp >> Bit40) & 255ul);
                        _buffer[_bufferOffset + 3] = (byte) ((doubleTmp >> Bit32) & 255ul);
                        _buffer[_bufferOffset + 4] = (byte) ((doubleTmp >> Bit24) & 255ul);
                        _buffer[_bufferOffset + 5] = (byte) ((doubleTmp >> Bit16) & 255ul);
                        _buffer[_bufferOffset + 6] = (byte) ((doubleTmp >> Bit8) & 255ul);
                        _buffer[_bufferOffset + 7] = (byte) (doubleTmp & 255ul);
                        _bufferOffset += 8;
                        break;
                    }
                    case Types.Decimal:
                        _int32A = Decimal.GetBits((decimal) data[_iter].Reference);

                        _buffer[_bufferOffset] = (byte) ((_tempBytes[0] >> Bit24) & _0xFF);
                        _buffer[_bufferOffset + 1] = (byte) ((_tempBytes[0] >> Bit16) & _0xFF);
                        _buffer[_bufferOffset + 2] = (byte) ((_tempBytes[0] >> Bit8) & _0xFF);
                        _buffer[_bufferOffset + 3] = (byte) (_tempBytes[0] & _0xFF);

                        _buffer[_bufferOffset + 4] = (byte) ((_tempBytes[1] >> Bit24) & _0xFF);
                        _buffer[_bufferOffset + 5] = (byte) ((_tempBytes[1] >> Bit16) & _0xFF);
                        _buffer[_bufferOffset + 6] = (byte) ((_tempBytes[1] >> Bit8) & _0xFF);
                        _buffer[_bufferOffset + 7] = (byte) (_tempBytes[1] & _0xFF);

                        _buffer[_bufferOffset + 8] = (byte) ((_tempBytes[2] >> Bit24) & _0xFF);
                        _buffer[_bufferOffset + 9] = (byte) ((_tempBytes[2] >> Bit16) & _0xFF);
                        _buffer[_bufferOffset + 10] = (byte) ((_tempBytes[2] >> Bit8) & _0xFF);
                        _buffer[_bufferOffset + 11] = (byte) (_tempBytes[2] & _0xFF);

                        _buffer[_bufferOffset + 12] = (byte) ((_tempBytes[3] >> Bit24) & _0xFF);
                        _buffer[_bufferOffset + 13] = (byte) ((_tempBytes[3] >> Bit16) & _0xFF);
                        _buffer[_bufferOffset + 14] = (byte) ((_tempBytes[3] >> Bit8) & _0xFF);
                        _buffer[_bufferOffset + 15] = (byte) (_tempBytes[3] & _0xFF);
                        _bufferOffset += 16;
                        break;
                    case Types.String:
                    {
                        _stringData = data[_iter].String;
                        _int32TMP = _stringData.Length;

                        for (int i = 0; i < _int32TMP; i++)
                        {
                            int value = char.ConvertToUtf32(_stringData, i);
                            if (value < 0x80)
                            {
                                _buffer[_bufferOffset++] = (byte) value;
                            }
                            else if (value < 0x0800)
                            {
                                _buffer[_bufferOffset++] = (byte) (value >> 6 | 0xC0);
                                _buffer[_bufferOffset++] = (byte) (value & 0x3F | 0x80);
                            }
                            else if (value < 0x010000)
                            {
                                _buffer[_bufferOffset++] = (byte) (value >> 12 | 0xE0);
                                _buffer[_bufferOffset++] = (byte) ((value >> 6) & 0x3F | 0x80);
                                _buffer[_bufferOffset++] = (byte) (value & 0x3F | 0x80);
                            }
                            else
                            {
                                _buffer[_bufferOffset++] = (byte) (value >> 18 | 0xF0);
                                _buffer[_bufferOffset++] = (byte) ((value >> 12) & 0x3F | 0x80);
                                _buffer[_bufferOffset++] = (byte) ((value >> 6) & 0x3F | 0x80);
                                _buffer[_bufferOffset++] = (byte) (value & 0x3F | 0x80);
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
                            _bufferOffset += 4;
                        }
                        else
                        {
                            _int32TMP = _player.playerId;
                            _buffer[_bufferOffset] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            _buffer[_bufferOffset + 1] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            _buffer[_bufferOffset + 2] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            _buffer[_bufferOffset + 3] = (byte) (_int32TMP & _0xFF);
                            _bufferOffset += 4;
                        }

                        break;
                    }
                    case Types.Color:
                        _color = (Color) data[_iter].Reference;
                        _tempBytes = BitConverter.GetBytes(_color.r);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_color.g);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 4, 4);
                        _tempBytes = BitConverter.GetBytes(_color.b);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 8, 4);
                        _tempBytes = BitConverter.GetBytes(_color.a);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 12, 4);
                        _bufferOffset += 16;
                        break;
                    case Types.Color32:
                        _color32 = (Color32) data[_iter].Reference;
                        _buffer[_bufferOffset] = _color32.r;
                        _buffer[_bufferOffset + 1] = _color32.g;
                        _buffer[_bufferOffset + 2] = _color32.b;
                        _buffer[_bufferOffset + 3] = _color32.a;
                        _bufferOffset += 4;
                        break;
                    case Types.Vector2:
                        _vector2 = (Vector2) data[_iter].Reference;
                        _tempBytes = BitConverter.GetBytes(_vector2.x);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_vector2.y);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 4, 4);
                        _bufferOffset += 8;
                        break;
                    case Types.Vector2Int:
                        _vector2Int = (Vector2Int) data[_iter].Reference;
                        _tempBytes = BitConverter.GetBytes(_vector2Int.x);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_vector2Int.y);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 4, 4);
                        _bufferOffset += 8;
                        break;
                    case Types.Vector3:
                        _vector3 = (Vector3) data[_iter].Reference;

                        _tempBytes = BitConverter.GetBytes(_vector3.x);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_vector3.y);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 4, 4);
                        _tempBytes = BitConverter.GetBytes(_vector3.z);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 8, 4);
                        _bufferOffset += 12;
                        break;
                    case Types.Vector3Int:
                        _vector3Int = (Vector3Int) data[_iter].Reference;
                        _tempBytes = BitConverter.GetBytes(_vector3Int.x);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_vector3Int.y);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 4, 4);
                        _tempBytes = BitConverter.GetBytes(_vector3Int.z);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 8, 4);
                        _bufferOffset += 12;
                        break;
                    case Types.Vector4:
                        _vector4 = (Vector4) data[_iter].Reference;
                        _tempBytes = BitConverter.GetBytes(_vector4.x);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_vector4.y);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 4, 4);
                        _tempBytes = BitConverter.GetBytes(_vector4.z);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 8, 4);
                        _tempBytes = BitConverter.GetBytes(_vector4.w);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 12, 4);
                        _bufferOffset += 16;
                        break;
                    case Types.Quaternion:
                        _quaternion = (Quaternion) data[_iter].Reference;
                        _tempBytes = BitConverter.GetBytes(_quaternion.x);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_quaternion.y);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 4, 4);
                        _tempBytes = BitConverter.GetBytes(_quaternion.z);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 8, 4);
                        _tempBytes = BitConverter.GetBytes(_quaternion.w);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 12, 4);
                        _bufferOffset += 16;
                        break;
                    case Types.DateTime:
                        _int64Value = ((DateTime) data[_iter].Reference).ToBinary();
                        _tempBytes = BitConverter.GetBytes(_int64Value);
                        Array.Copy(_tempBytes, 0, _buffer, _bufferOffset, 8);
                        _bufferOffset += 8;
                        break;
                    //Arrays
                    case Types.BooleanA:
                    {
                        _boolA = (bool[]) data[_iter].Reference;
                        for (int j = 0; j < _boolA.Length; j++)
                        {
                            _buffer[_bufferOffset + j] = _boolA[j] ? BYTE_ONE : BYTE_ZERO;
                        }

                        _bufferOffset += _boolA.Length;
                        break;
                    }
                    case Types.ByteA:
                        _byteA = (byte[]) data[_iter].Reference;
                        Array.Copy(_byteA, 0, _buffer, _bufferOffset, _byteA.Length);
                        _bufferOffset += _byteA.Length;
                        break;
                    case Types.SByteA:
                    {
                        _sbyteA = (sbyte[]) data[_iter].Reference;
                        for (int j = 0; j < _sbyteA.Length; j++)
                        {
                            _sbyteValue = _sbyteA[j];
                            _buffer[_bufferOffset + j] =
                                (byte) (_sbyteValue < 0 ? (_sbyteValue + 0xFFFF) : _sbyteValue);
                        }

                        _bufferOffset += _sbyteA.Length;
                        break;
                    }
                    case Types.Int16A:
                    {
                        _int16A = (short[]) data[_iter].Reference;
                        for (int j = 0; j < _int16A.Length; j++)
                        {
                            _int32TMP = _int16A[j] < 0 ? (_int16A[j] + 0xFFFF) : _int16A[j];
                            _buffer[_bufferOffset] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            _buffer[_bufferOffset + 1] = (byte) (_int32TMP & _0xFF);
                            _bufferOffset += 2;
                        }

                        break;
                    }
                    case Types.UInt16A:
                    {
                        _uint16A = (ushort[]) data[_iter].Reference;
                        for (int j = 0; j < _uint16A.Length; j++)
                        {
                            _uint16Value = _uint16A[j];
                            _buffer[_bufferOffset] = (byte) ((_uint16Value >> Bit8) & 255u);
                            _buffer[_bufferOffset + 1] = (byte) (_uint16Value & 255u);
                            _bufferOffset += 2;
                        }

                        break;
                    }
                    case Types.Int32A:
                    {
                        _int32A = (int[]) data[_iter].Reference;
                        for (int j = 0; j < _int32A.Length; j++)
                        {
                            _int32TMP2 = _int32A[j];
                            _buffer[_bufferOffset] = (byte) ((_int32TMP2 >> Bit24) & _0xFF);
                            _buffer[_bufferOffset + 1] = (byte) ((_int32TMP2 >> Bit16) & _0xFF);
                            _buffer[_bufferOffset + 2] = (byte) ((_int32TMP2 >> Bit8) & _0xFF);
                            _buffer[_bufferOffset + 3] = (byte) (_int32TMP2 & _0xFF);
                            _bufferOffset += 4;
                        }

                        break;
                    }
                    case Types.UInt32A:
                    {
                        _uint32A = (uint[]) data[_iter].Reference;
                        for (int j = 0; j < _uint32A.Length; j++)
                        {
                            _uint32Value = _uint32A[j];
                            _buffer[_bufferOffset] = (byte) ((_uint32Value >> Bit24) & 255u);
                            _buffer[_bufferOffset + 1] = (byte) ((_uint32Value >> Bit16) & 255u);
                            _buffer[_bufferOffset + 2] = (byte) ((_uint32Value >> Bit8) & 255u);
                            _buffer[_bufferOffset + 3] = (byte) (_uint32Value & 255u);
                            _bufferOffset += 4;
                        }

                        break;
                    }
                    case Types.Int64A:
                    {
                        _int64A = (long[]) data[_iter].Reference;
                        for (int j = 0; j < _int64A.Length; j++)
                        {
                            _int64Value = _int64A[j];
                            _buffer[_bufferOffset] = (byte) ((_int64Value >> Bit56) & _0xFF);
                            _buffer[_bufferOffset + 1] = (byte) ((_int64Value >> Bit48) & _0xFF);
                            _buffer[_bufferOffset + 2] = (byte) ((_int64Value >> Bit40) & _0xFF);
                            _buffer[_bufferOffset + 3] = (byte) ((_int64Value >> Bit32) & _0xFF);
                            _buffer[_bufferOffset + 4] = (byte) ((_int64Value >> Bit24) & _0xFF);
                            _buffer[_bufferOffset + 5] = (byte) ((_int64Value >> Bit16) & _0xFF);
                            _buffer[_bufferOffset + 6] = (byte) ((_int64Value >> Bit8) & _0xFF);
                            _buffer[_bufferOffset + 7] = (byte) (_int64Value & _0xFF);
                            _bufferOffset += 8;
                        }

                        break;
                    }
                    case Types.UInt64A:
                    {
                        _uint64A = (ulong[]) data[_iter].Reference;
                        for (int j = 0; j < _uint64A.Length; j++)
                        {
                            _uint64Value = _uint64A[j];
                            _buffer[_bufferOffset] = (byte) ((_uint64Value >> Bit56) & 255ul);
                            _buffer[_bufferOffset + 1] = (byte) ((_uint64Value >> Bit48) & 255ul);
                            _buffer[_bufferOffset + 2] = (byte) ((_uint64Value >> Bit40) & 255ul);
                            _buffer[_bufferOffset + 3] = (byte) ((_uint64Value >> Bit32) & 255ul);
                            _buffer[_bufferOffset + 4] = (byte) ((_uint64Value >> Bit24) & 255ul);
                            _buffer[_bufferOffset + 5] = (byte) ((_uint64Value >> Bit16) & 255ul);
                            _buffer[_bufferOffset + 6] = (byte) ((_uint64Value >> Bit8) & 255ul);
                            _buffer[_bufferOffset + 7] = (byte) (_uint64Value & 255ul);
                            _bufferOffset += 8;
                        }

                        break;
                    }
                    case Types.SingleA:
                    {
                        _singleA = (float[]) data[_iter].Reference;
                        for (int j = 0; j < _singleA.Length; j++)
                        {
                            _tempBytes = BitConverter.GetBytes(_singleA[j]);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + j * 4, 4);
                        }

                        _bufferOffset += _singleA.Length * 4;
                        break;
                    }
                    case Types.DoubleA:
                    {
                        _doubleA = (double[]) data[_iter].Reference;
                        for (int j = 0; j < _doubleA.Length; j++)
                        {
                            _tempBytes = BitConverter.GetBytes(_doubleA[j]);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + j * 8, 8);
                        }

                        _bufferOffset += _doubleA.Length * 8;
                        break;
                    }
                    case Types.DecimalA:
                        break;
                    case Types.StringA:
                    {
                        _stringA = (string[]) data[_iter].Reference;
                        for (int j = 0; j < _stringA.Length; j++)
                        {
                            _tempBytes = BitConverter.GetBytes(_stringA[j]);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset, _tempBytes.Length);
                            _bufferOffset += _tempBytes.Length;
                            if (j < _stringA.Length - 1)
                            {
                                _buffer[_bufferOffset] = BYTE_ZERO;
                                _bufferOffset++;
                            }
                        }

                        break;
                    }
                    case Types.VRCPlayerApiA:
                    {
                        _vrcPlayerApiA = (VRCPlayerApi[]) data[_iter].Reference;
                        for (int j = 0; j < _vrcPlayerApiA.Length; j++)
                        {
                            _int32TMP = _vrcPlayerApiA[j].playerId;
                            _buffer[_bufferOffset] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            _buffer[_bufferOffset + 1] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            _buffer[_bufferOffset + 2] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            _buffer[_bufferOffset + 3] = (byte) (_int32TMP & _0xFF);
                            _bufferOffset += 4;
                        }

                        break;
                    }
                    case Types.ColorA:
                    {
                        _colorA = (Color[]) data[_iter].Reference;
                        for (int j = 0; j < _colorA.Length; j++)
                        {
                            _color = _colorA[j];
                            _tempBytes = BitConverter.GetBytes(_color.r);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset, 4);
                            _tempBytes = BitConverter.GetBytes(_color.g);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 4, 4);
                            _tempBytes = BitConverter.GetBytes(_color.b);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 8, 4);
                            _tempBytes = BitConverter.GetBytes(_color.a);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 12, 4);
                            _bufferOffset += 16;
                        }

                        break;
                    }
                    case Types.Color32A:
                    {
                        _color32A = (Color32[]) data[_iter].Reference;
                        for (int j = 0; j < _color32A.Length; j++)
                        {
                            _color32 = _color32A[j];
                            _buffer[_bufferOffset] = _color32.r;
                            _buffer[_bufferOffset + 1] = _color32.g;
                            _buffer[_bufferOffset + 2] = _color32.b;
                            _buffer[_bufferOffset + 3] = _color32.a;
                            _bufferOffset += 4;
                        }

                        _bufferOffset += _color32A.Length * 4;
                        break;
                    }
                    case Types.Vector2A:
                    {
                        _vector2A = (Vector2[]) data[_iter].Reference;
                        for (int j = 0; j < _vector2A.Length; j++)
                        {
                            _vector2 = _vector2A[j];
                            _tempBytes = BitConverter.GetBytes(_vector2.x);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset, 4);
                            _tempBytes = BitConverter.GetBytes(_vector2.y);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 4, 4);
                            _bufferOffset += 8;
                        }

                        _bufferOffset += _vector2A.Length * 8;
                        break;
                    }
                    case Types.Vector2IntA:
                    {
                        _vector2IntA = (Vector2Int[]) data[_iter].Reference;
                        for (int j = 0; j < _vector2IntA.Length; j++)
                        {
                            _vector2Int = _vector2IntA[j];
                            _int32TMP = _vector2Int.x;
                            _buffer[_bufferOffset] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            _buffer[_bufferOffset + 1] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            _buffer[_bufferOffset + 2] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            _buffer[_bufferOffset + 3] = (byte) (_int32TMP & _0xFF);
                            _int32TMP = _vector2Int.y;
                            _buffer[_bufferOffset + 4] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            _buffer[_bufferOffset + 5] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            _buffer[_bufferOffset + 6] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            _buffer[_bufferOffset + 7] = (byte) (_int32TMP & _0xFF);
                            _bufferOffset += 8;
                        }

                        break;
                    }
                    case Types.Vector3A:
                    {
                        _vector3A = (Vector3[]) data[_iter].Reference;
                        for (int j = 0; j < _vector3A.Length; j++)
                        {
                            _vector3 = _vector3A[j];
                            _tempBytes = BitConverter.GetBytes(_vector3.x);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + j * 12, 4);
                            _tempBytes = BitConverter.GetBytes(_vector3.y);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + j * 12 + 4, 4);
                            _tempBytes = BitConverter.GetBytes(_vector3.z);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + j * 12 + 8, 4);
                        }

                        _bufferOffset += _vector3A.Length * 12;
                        break;
                    }
                    case Types.Vector3IntA:
                    {
                        _vector3IntA = (Vector3Int[]) data[_iter].Reference;
                        for (int j = 0; j < _vector3IntA.Length; j++)
                        {
                            _vector3Int = _vector3IntA[j];
                            _int32TMP = _vector3Int.x;
                            _buffer[_bufferOffset] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            _buffer[_bufferOffset + 1] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            _buffer[_bufferOffset + 2] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            _buffer[_bufferOffset + 3] = (byte) (_int32TMP & _0xFF);
                            _int32TMP = _vector3Int.y;
                            _buffer[_bufferOffset + 4] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            _buffer[_bufferOffset + 5] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            _buffer[_bufferOffset + 6] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            _buffer[_bufferOffset + 7] = (byte) (_int32TMP & _0xFF);
                            _int32TMP = _vector3Int.z;
                            _buffer[_bufferOffset + 8] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            _buffer[_bufferOffset + 9] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            _buffer[_bufferOffset + 10] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            _buffer[_bufferOffset + 11] = (byte) (_int32TMP & _0xFF);
                            _bufferOffset += 12;
                        }

                        break;
                    }
                    case Types.Vector4A:
                    {
                        _vector4A = (Vector4[]) data[_iter].Reference;
                        for (int j = 0; j < _vector4A.Length; j++)
                        {
                            _vector4 = _vector4A[j];
                            _tempBytes = BitConverter.GetBytes(_vector4.x);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset, 4);
                            _tempBytes = BitConverter.GetBytes(_vector4.y);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 4, 4);
                            _tempBytes = BitConverter.GetBytes(_vector4.z);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 8, 4);
                            _tempBytes = BitConverter.GetBytes(_vector4.w);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + 12, 4);
                            _bufferOffset += 16;
                        }

                        break;
                    }
                    case Types.QuaternionA:
                    {
                        _quaternionA = (Quaternion[]) data[_iter].Reference;
                        for (int j = 0; j < _quaternionA.Length; j++)
                        {
                            _quaternion = _quaternionA[j];
                            _tempBytes = BitConverter.GetBytes(_quaternion.x);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + j * 16, 4);
                            _tempBytes = BitConverter.GetBytes(_quaternion.y);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + j * 16 + 4, 4);
                            _tempBytes = BitConverter.GetBytes(_quaternion.z);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + j * 16 + 8, 4);
                            _tempBytes = BitConverter.GetBytes(_quaternion.w);
                            Array.Copy(_tempBytes, 0, _buffer, _bufferOffset + j * 16 + 12, 4);
                        }

                        _bufferOffset += _quaternionA.Length * 16;
                        break;
                    }
                }
            }

            if (_debug)
            {
                Log("Sending " + method + " to " + scriptTarget + " with " + data.Length + " parameters");
            }


            methodTarget = method;
            this._scriptTarget = scriptTarget;
            _sentOutMethods++;
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
            if (_debug)
                Log($"Deserialization - {methodTarget} - {_buffer.Length} - {_types.Length} - {_lengths.Length}");

            if (methodTarget == -1) return;

            if (_localSentOut >= _sentOutMethods && _localSentOut != 0)
            {
                if (_debug)
                    LogWarning(
                        $"Ignoring deserialization, already sent out Local: {_localSentOut} - Global: {_sentOutMethods}");
                _localSentOut = _sentOutMethods;
                return;
            }

            _localSentOut = _sentOutMethods;


            var sIndex = Array.IndexOf(sceneInterfacesIds, _scriptTarget);
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
            if (_parameters.Length != _types.Length)
            {
                _parameters = new DataToken[_types.Length];
            }

            _bufferOffset = 0;

            for (_iter = 0; _iter < _parameters.Length; _iter++)
            {
                _ushortLength = _lengths[_iter];
                string[] chars;
                switch (_types[_iter])
                {
                    case Types.Boolean:
                        _parameters[_iter] = _buffer[_bufferOffset] == BYTE_ONE;
                        _bufferOffset++;
                        break;
                    case Types.Byte:
                        _parameters[_iter] = _buffer[_bufferOffset];
                        _bufferOffset++;
                        break;
                    case Types.SByte:
                    {
                        _int32TMP = _buffer[_bufferOffset];
                        if (_int32TMP >= 0x80) _int32TMP -= _0xFF;
                        _parameters[_iter] = Convert.ToSByte(_int32TMP);
                        _bufferOffset++;
                        break;
                    }
                    case Types.Int16:
                    {
                        _int32TMP = (_buffer[_bufferOffset] << Bit8) | _buffer[_bufferOffset + 1];
                        if (_int32TMP >= 0x8000) _int32TMP -= _0xFFFF;
                        _parameters[_iter] = Convert.ToInt16(_int32TMP);
                        _bufferOffset += 2;
                        break;
                    }
                    case Types.UInt16:
                        _int32TMP = (_buffer[_bufferOffset] << Bit8) | _buffer[_bufferOffset + 1];
                        _parameters[_iter] = Convert.ToUInt16(_int32TMP);
                        _bufferOffset += 2;
                        break;
                    case Types.Int32:
                        _parameters[_iter] = (_buffer[_bufferOffset] << Bit24) | (_buffer[_bufferOffset + 1] << Bit16) |
                                             (_buffer[_bufferOffset + 2] << Bit8) | _buffer[_bufferOffset + 3];
                        _bufferOffset += 4;
                        break;
                    case Types.UInt32:
                        _parameters[_iter] = (uint) ((_buffer[_bufferOffset] << Bit24) |
                                                     (_buffer[_bufferOffset + 1] << Bit16) |
                                                     (_buffer[_bufferOffset + 2] << Bit8) | _buffer[_bufferOffset + 3]);
                        _bufferOffset += 4;
                        break;
                    case Types.Int64:
                        _parameters[_iter] = ((long) _buffer[_bufferOffset] << Bit56) |
                                             ((long) _buffer[_bufferOffset + 1] << Bit48) |
                                             ((long) _buffer[_bufferOffset + 2] << Bit40) |
                                             ((long) _buffer[_bufferOffset + 3] << Bit32) |
                                             ((long) _buffer[_bufferOffset + 4] << Bit24) |
                                             ((long) _buffer[_bufferOffset + 5] << Bit16) |
                                             ((long) _buffer[_bufferOffset + 6] << Bit8) |
                                             _buffer[_bufferOffset + 7];
                        _bufferOffset += 8;
                        break;
                    case Types.UInt64:
                        _parameters[_iter] = ((ulong) _buffer[_bufferOffset] << Bit56) |
                                             ((ulong) _buffer[_bufferOffset + 1] << Bit48) |
                                             ((ulong) _buffer[_bufferOffset + 2] << Bit40) |
                                             ((ulong) _buffer[_bufferOffset + 3] << Bit32) |
                                             ((ulong) _buffer[_bufferOffset + 4] << Bit24) |
                                             ((ulong) _buffer[_bufferOffset + 5] << Bit16) |
                                             ((ulong) _buffer[_bufferOffset + 6] << Bit8) |
                                             _buffer[_bufferOffset + 7];

                        _bufferOffset += 8;
                        break;
                    case Types.Single:
                    {
                        _uint32Value = ((uint) _buffer[_bufferOffset] << Bit24) |
                                       ((uint) _buffer[_bufferOffset + 1] << Bit16) |
                                       ((uint) _buffer[_bufferOffset + 2] << Bit8) | _buffer[_bufferOffset + 3];
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
                        _uint64Value = ((ulong) _buffer[_bufferOffset] << Bit56) |
                                       ((ulong) _buffer[_bufferOffset + 1] << Bit48) |
                                       ((ulong) _buffer[_bufferOffset + 2] << Bit40) |
                                       ((ulong) _buffer[_bufferOffset + 3] << Bit32) |
                                       ((ulong) _buffer[_bufferOffset + 4] << Bit24) |
                                       ((ulong) _buffer[_bufferOffset + 5] << Bit16) |
                                       ((ulong) _buffer[_bufferOffset + 6] << Bit8) | _buffer[_bufferOffset + 7];

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
                            _buffer[_bufferOffset] << Bit24 | _buffer[_bufferOffset + 1] << Bit16 |
                            _buffer[_bufferOffset + 2] << Bit8 | _buffer[_bufferOffset + 3],

                            _buffer[_bufferOffset + 4] << Bit24 | _buffer[_bufferOffset + 5] << Bit16 |
                            _buffer[_bufferOffset + 6] << Bit8 | _buffer[_bufferOffset + 7],

                            _buffer[_bufferOffset + 8] << Bit24 | _buffer[_bufferOffset + 9] << Bit16 |
                            _buffer[_bufferOffset + 10] << Bit8 | _buffer[_bufferOffset + 11],

                            _buffer[_bufferOffset + 12] << Bit24 | _buffer[_bufferOffset + 13] << Bit16 |
                            _buffer[_bufferOffset + 14] << Bit8 | _buffer[_bufferOffset + 15]
                        }));
                        _bufferOffset += 16;
                        break;
                    case Types.String:
                    {
                        _int32TMP = 0;
                        _int32TMP2 = 0;
                        _int32TMP3 = 0;
                        _int32TMP4 = _lengths[_iter];
                        chars = new string[_int32TMP4];

                        _int32TMP4 += _bufferOffset;


                        for (var i = _bufferOffset; i < _int32TMP4; i++)
                        {
                            _byteTmp = _buffer[i];
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
                        _bufferOffset += _lengths[_iter];
                        break;
                    }
                    case Types.VRCPlayerApi:
                        _parameters[_iter] = new DataToken(VRCPlayerApi.GetPlayerById(
                            _buffer[_bufferOffset] << Bit24 |
                            _buffer[_bufferOffset + 1] << Bit16 |
                            _buffer[_bufferOffset + 2] << Bit8 |
                            _buffer[_bufferOffset + 3]));
                        _bufferOffset += 4;
                        break;
                    case Types.Color:
                        _tempBytes = new byte[4];
                        Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                        _singleValue = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(_buffer, _bufferOffset + 4, _tempBytes, 0, 4);
                        _singleValue2 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(_buffer, _bufferOffset + 8, _tempBytes, 0, 4);
                        _singleValue3 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(_buffer, _bufferOffset + 12, _tempBytes, 0, 4);
                        _singleValue4 = BitConverter.ToSingle(_tempBytes);
                        _parameters[_iter] =
                            new DataToken(new Color(_singleValue, _singleValue2, _singleValue3, _singleValue4));
                        _bufferOffset += 16;
                        break;
                    case Types.Color32:
                        _parameters[_iter] = new DataToken(new Color32(_buffer[_bufferOffset],
                            _buffer[_bufferOffset + 1],
                            _buffer[_bufferOffset + 2], _buffer[_bufferOffset + 3]));
                        _bufferOffset += 4;
                        break;
                    case Types.Vector2:
                        _tempBytes = new byte[4];
                        Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                        _singleValue = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(_buffer, _bufferOffset + 4, _tempBytes, 0, 4);
                        _singleValue2 = BitConverter.ToSingle(_tempBytes);
                        _parameters[_iter] = new DataToken(new Vector2(_singleValue, _singleValue2));
                        _bufferOffset += 8;
                        break;
                    case Types.Vector2Int:
                        _parameters[_iter] = new DataToken(new Vector2Int(
                            _buffer[_bufferOffset] << Bit24 |
                            _buffer[_bufferOffset + 1] << Bit16 |
                            _buffer[_bufferOffset + 2] << Bit8 |
                            _buffer[_bufferOffset + 3],
                            _buffer[_bufferOffset + 4] << Bit24 |
                            _buffer[_bufferOffset + 5] << Bit16 |
                            _buffer[_bufferOffset + 6] << Bit8 |
                            _buffer[_bufferOffset + 7]));
                        _bufferOffset += 8;
                        break;
                    case Types.Vector3:
                        _tempBytes = new byte[4];
                        Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                        _singleValue = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(_buffer, _bufferOffset + 4, _tempBytes, 0, 4);
                        _singleValue2 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(_buffer, _bufferOffset + 8, _tempBytes, 0, 4);
                        _singleValue3 = BitConverter.ToSingle(_tempBytes);
                        _parameters[_iter] = new DataToken(new Vector3(_singleValue, _singleValue2, _singleValue3));
                        _bufferOffset += 12;
                        break;
                    case Types.Vector3Int:
                        _parameters[_iter] = new DataToken(new Vector3Int(
                            _buffer[_bufferOffset] << Bit24 |
                            _buffer[_bufferOffset + 1] << Bit16 |
                            _buffer[_bufferOffset + 2] << Bit8 |
                            _buffer[_bufferOffset + 3],
                            _buffer[_bufferOffset + 4] << Bit24 |
                            _buffer[_bufferOffset + 5] << Bit16 |
                            _buffer[_bufferOffset + 6] << Bit8 |
                            _buffer[_bufferOffset + 7],
                            _buffer[_bufferOffset + 8] << Bit24 |
                            _buffer[_bufferOffset + 9] << Bit16 |
                            _buffer[_bufferOffset + 10] << Bit8 |
                            _buffer[_bufferOffset + 11]));
                        _bufferOffset += 12;
                        break;
                    case Types.Vector4:
                        _tempBytes = new byte[4];
                        Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                        _singleValue = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(_buffer, _bufferOffset + 4, _tempBytes, 0, 4);
                        _singleValue2 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(_buffer, _bufferOffset + 8, _tempBytes, 0, 4);
                        _singleValue3 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(_buffer, _bufferOffset + 12, _tempBytes, 0, 4);
                        _singleValue4 = BitConverter.ToSingle(_tempBytes);
                        _parameters[_iter] =
                            new DataToken(new Vector4(_singleValue, _singleValue2, _singleValue3, _singleValue4));
                        _bufferOffset += 16;
                        break;
                    case Types.Quaternion:
                        _tempBytes = new byte[4];
                        Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                        _singleValue = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(_buffer, _bufferOffset + 4, _tempBytes, 0, 4);
                        _singleValue2 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(_buffer, _bufferOffset + 8, _tempBytes, 0, 4);
                        _singleValue3 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(_buffer, _bufferOffset + 12, _tempBytes, 0, 4);
                        _singleValue4 = BitConverter.ToSingle(_tempBytes);
                        _parameters[_iter] =
                            new DataToken(new Quaternion(_singleValue, _singleValue2, _singleValue3, _singleValue4));
                        _bufferOffset += 16;
                        break;
                    case Types.DateTime:
                        _parameters[_iter] = new DataToken(DateTime.FromBinary(
                            (long) _buffer[_bufferOffset] << Bit56 |
                            (long) _buffer[_bufferOffset + 1] << Bit48 |
                            (long) _buffer[_bufferOffset + 2] << Bit40 |
                            (long) _buffer[_bufferOffset + 3] << Bit32 |
                            (long) _buffer[_bufferOffset + 4] << Bit24 |
                            (long) _buffer[_bufferOffset + 5] << Bit16 |
                            (long) _buffer[_bufferOffset + 6] << Bit8 |
                            _buffer[_bufferOffset + 7]));
                        _bufferOffset += 8;
                        break;
                    //Arrays
                    case Types.BooleanA:
                    {
                        _ushortLength = _lengths[_iter];
                        _boolA = new bool[_ushortLength];

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _boolA[i] = _buffer[_bufferOffset] == BYTE_ONE;
                            _bufferOffset++;
                        }

                        _parameters[_iter] = new DataToken(_boolA);
                        break;
                    }
                    case Types.ByteA:
                    {
                        _ushortLength = _lengths[_iter];
                        _byteA = new byte[_ushortLength];

                        Array.Copy(_buffer, _bufferOffset, _byteA, 0, _byteA.Length);
                        _parameters[_iter] = new DataToken(_byteA);
                        _bufferOffset += _byteA.Length;
                        break;
                    }
                    case Types.SByteA:
                    {
                        _ushortLength = _lengths[_iter];
                        _sbyteA = new sbyte[_ushortLength];

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _int32TMP = _buffer[_bufferOffset];
                            if (_int32TMP >= 0x80) _int32TMP -= _0xFF;
                            _sbyteA[i] = Convert.ToSByte(_int32TMP);
                            _bufferOffset++;
                        }

                        _parameters[_iter] = new DataToken(_sbyteA);
                        break;
                    }
                    case Types.Int16A:
                    {
                        _ushortLength = _lengths[_iter];
                        _int16A = new short[_ushortLength];

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _int32TMP = (_buffer[_bufferOffset] << Bit8) | _buffer[_bufferOffset + 1];
                            if (_int32TMP >= 0x8000) _int32TMP -= _0xFFFF;
                            _int16A[i] = Convert.ToInt16(_int32TMP);
                            _bufferOffset += 2;
                        }

                        _parameters[_iter] = new DataToken(_int16A);
                        break;
                    }
                    case Types.UInt16A:
                    {
                        _ushortLength = _lengths[_iter];
                        _uint16A = new ushort[_ushortLength];

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _uint16A[i] = (ushort) ((ushort) (_buffer[_bufferOffset] << Bit8) |
                                                    _buffer[_bufferOffset + 1]);
                            _bufferOffset += 2;
                        }

                        _parameters[_iter] = new DataToken(_uint16A);
                        break;
                    }
                    case Types.Int32A:
                    {
                        _ushortLength = _lengths[_iter];
                        _int32A = new int[_ushortLength];


                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _int32A[i] = (_buffer[_bufferOffset] << Bit24) | (_buffer[_bufferOffset + 1] << Bit16) |
                                         (_buffer[_bufferOffset + 2] << Bit8) | _buffer[_bufferOffset + 3];
                            _bufferOffset += 4;
                        }

                        _parameters[_iter] = new DataToken(_int32A);
                        break;
                    }
                    case Types.UInt32A:
                    {
                        _ushortLength = _lengths[_iter];
                        _uint32A = new uint[_ushortLength];


                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _uint32A[i] = (uint) (_buffer[_bufferOffset] << Bit24) |
                                          (uint) (_buffer[_bufferOffset + 1] << Bit16) |
                                          (uint) (_buffer[_bufferOffset + 2] << Bit8) | _buffer[_bufferOffset + 3];
                            _bufferOffset += 4;
                        }

                        _parameters[_iter] = new DataToken(_uint32A);
                        break;
                    }
                    case Types.Int64A:
                    {
                        _ushortLength = _lengths[_iter];
                        _int64A = new long[_ushortLength];

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _int64A[i] = ((long) _buffer[_bufferOffset] << Bit56) |
                                         ((long) _buffer[_bufferOffset + 1] << Bit48) |
                                         ((long) _buffer[_bufferOffset + 2] << Bit40) |
                                         ((long) _buffer[_bufferOffset + 3] << Bit32) |
                                         ((long) _buffer[_bufferOffset + 4] << Bit24) |
                                         ((long) _buffer[_bufferOffset + 5] << Bit16) |
                                         ((long) _buffer[_bufferOffset + 6] << Bit8) | _buffer[_bufferOffset + 7];
                            _bufferOffset += 8;
                        }

                        _parameters[_iter] = new DataToken(_int64A);
                        break;
                    }
                    case Types.UInt64A:
                    {
                        _ushortLength = _lengths[_iter];
                        _uint64A = new ulong[_ushortLength];

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _uint64A[i] = ((ulong) _buffer[_bufferOffset] << Bit56) |
                                          ((ulong) _buffer[_bufferOffset + 1] << Bit48) |
                                          ((ulong) _buffer[_bufferOffset + 2] << Bit40) |
                                          ((ulong) _buffer[_bufferOffset + 3] << Bit32) |
                                          ((ulong) _buffer[_bufferOffset + 4] << Bit24) |
                                          ((ulong) _buffer[_bufferOffset + 5] << Bit16) |
                                          ((ulong) _buffer[_bufferOffset + 6] << Bit8) | _buffer[_bufferOffset + 7];
                            _bufferOffset += 8;
                        }

                        _parameters[_iter] = new DataToken(_uint64A);
                        break;
                    }
                    case Types.SingleA:
                    {
                        _ushortLength = _lengths[_iter];
                        _singleA = new float[_ushortLength];

                        _tempBytes = new byte[4];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleA[i] = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                        }

                        _parameters[_iter] = new DataToken(_singleA);
                        break;
                    }
                    case Types.DoubleA:
                    {
                        _ushortLength = _lengths[_iter];
                        _doubleA = new double[_ushortLength];

                        _tempBytes = new byte[8];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 8);
                            _doubleA[i] = BitConverter.ToDouble(_tempBytes);
                            _bufferOffset += 8;
                        }

                        _parameters[_iter] = new DataToken(_doubleA);
                        break;
                    }
                    case Types.DecimalA:
                    {
                        _ushortLength = _lengths[_iter];
                        _decimalA = new decimal[_ushortLength];


                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _decimalA[i] = new decimal(new int[]
                            {
                                _buffer[_bufferOffset] << Bit24 | _buffer[_bufferOffset + 1] << Bit16 |
                                _buffer[_bufferOffset + 2] << Bit8 | _buffer[_bufferOffset + 3],
                                _buffer[_bufferOffset + 4] << Bit24 | _buffer[_bufferOffset + 5] << Bit16 |
                                _buffer[_bufferOffset + 6] << Bit8 | _buffer[_bufferOffset + 7],
                                _buffer[_bufferOffset + 8] << Bit24 | _buffer[_bufferOffset + 9] << Bit16 |
                                _buffer[_bufferOffset + 10] << Bit8 | _buffer[_bufferOffset + 11],
                                _buffer[_bufferOffset + 12] << Bit24 | _buffer[_bufferOffset + 13] << Bit16 |
                                _buffer[_bufferOffset + 14] << Bit8 | _buffer[_bufferOffset + 15]
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
                        _int32TMP4 = _lengths[_iter];
                        chars = new string[_int32TMP4];

                        _int32TMP4 += _bufferOffset;


                        for (var i = _bufferOffset; i < _int32TMP4; i++)
                        {
                            _byteTmp = _buffer[i];
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

                        _parameters[_iter] = new DataToken(string.Concat(chars).Split('\0'));
                        _bufferOffset += _lengths[_iter];
                        break;
                    }
                    case Types.VRCPlayerApiA:
                    {
                        _ushortLength = _lengths[_iter];
                        _vrcPlayerApiA = new VRCPlayerApi[_ushortLength];

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _int32TMP = (_buffer[_bufferOffset] << Bit24) | (_buffer[_bufferOffset + 1] << Bit16) |
                                        (_buffer[_bufferOffset + 2] << Bit8) | _buffer[_bufferOffset + 3];
                            _vrcPlayerApiA[i] = VRCPlayerApi.GetPlayerById(_int32TMP);
                            _bufferOffset += 4;
                        }

                        _parameters[_iter] = new DataToken(_vrcPlayerApiA);
                        break;
                    }
                    case Types.ColorA:
                    {
                        _ushortLength = _lengths[_iter];
                        _colorA = new Color[_ushortLength];

                        _tempBytes = new byte[4];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue2 = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue3 = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue4 = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            _colorA[i] = new Color(_singleValue, _singleValue2, _singleValue3, _singleValue4);
                        }

                        _parameters[_iter] = new DataToken(_colorA);
                        break;
                    }
                    case Types.Color32A:
                    {
                        _ushortLength = _lengths[_iter];
                        _color32A = new Color32[_ushortLength];

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _color32A[i] = new Color32(_buffer[_bufferOffset], _buffer[_bufferOffset + 1],
                                _buffer[_bufferOffset + 2], _buffer[_bufferOffset + 3]);
                            _bufferOffset += 4;
                        }

                        _parameters[_iter] = new DataToken(_color32A);
                        break;
                    }
                    case Types.Vector2A:
                    {
                        _ushortLength = _lengths[_iter];
                        _vector2A = new Vector2[_ushortLength];

                        _tempBytes = new byte[4];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue2 = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            _vector2A[i] = new Vector2(_singleValue, _singleValue2);
                        }

                        _parameters[_iter] = new DataToken(_vector2A);
                        break;
                    }
                    case Types.Vector2IntA:
                    {
                        _ushortLength = _lengths[_iter];
                        _vector2IntA = new Vector2Int[_ushortLength];

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _vector2IntA[i] = new Vector2Int(
                                _buffer[_bufferOffset] << Bit24 | _buffer[_bufferOffset + 1] << Bit16 |
                                _buffer[_bufferOffset + 2] << Bit8 | _buffer[_bufferOffset + 3],
                                _buffer[_bufferOffset + 4] << Bit24 | _buffer[_bufferOffset + 5] << Bit16 |
                                _buffer[_bufferOffset + 6] << Bit8 | _buffer[_bufferOffset + 7]);
                            _bufferOffset += 8;
                        }

                        _parameters[_iter] = new DataToken(_vector2IntA);
                        break;
                    }
                    case Types.Vector3A:
                    {
                        _ushortLength = _lengths[_iter];
                        _vector3A = new Vector3[_ushortLength];

                        _tempBytes = new byte[4];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue2 = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue3 = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            _vector3A[i] = new Vector3(_singleValue, _singleValue2, _singleValue3);
                        }

                        _parameters[_iter] = new DataToken(_vector3A);
                        break;
                    }
                    case Types.Vector3IntA:
                    {
                        _ushortLength = _lengths[_iter];
                        _vector3IntA = new Vector3Int[_ushortLength];

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _vector3IntA[i] = new Vector3Int(
                                _buffer[_bufferOffset] << Bit24 | _buffer[_bufferOffset + 1] << Bit16 |
                                _buffer[_bufferOffset + 2] << Bit8 | _buffer[_bufferOffset + 3],
                                _buffer[_bufferOffset + 4] << Bit24 | _buffer[_bufferOffset + 5] << Bit16 |
                                _buffer[_bufferOffset + 6] << Bit8 | _buffer[_bufferOffset + 7],
                                _buffer[_bufferOffset + 8] << Bit24 | _buffer[_bufferOffset + 9] << Bit16 |
                                _buffer[_bufferOffset + 10] << Bit8 | _buffer[_bufferOffset + 11]);
                            _bufferOffset += 12;
                        }

                        _parameters[_iter] = new DataToken(_vector3IntA);
                        break;
                    }
                    case Types.Vector4A:
                    {
                        _ushortLength = _lengths[_iter];
                        _vector4A = new Vector4[_ushortLength];

                        _tempBytes = new byte[4];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue2 = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue3 = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue4 = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            _vector4A[i] = new Vector4(_singleValue, _singleValue2, _singleValue3, _singleValue4);
                        }

                        _parameters[_iter] = new DataToken(_vector4A);
                        break;
                    }
                    case Types.QuaternionA:
                    {
                        _ushortLength = _lengths[_iter];
                        _quaternionA = new Quaternion[_ushortLength];

                        _tempBytes = new byte[4];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue2 = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue3 = BitConverter.ToSingle(_tempBytes);
                            _bufferOffset += 4;
                            Array.Copy(_buffer, _bufferOffset, _tempBytes, 0, 4);
                            _singleValue4 = BitConverter.ToSingle(_tempBytes);
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