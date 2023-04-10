using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

/*

        if (target == SyncTarget.Local)
        {
            _MethodReceived(method, arrayParametersLocal, paramTypesLocal);
            return;
        }

        if (target != SyncTarget.Others)
        {
            _MethodReceived(method, arrayParametersLocal, paramTypesLocal);
        }

        //TODO largestKnown = networkManager.largest;
        if (!isQueueRunning && Time.timeSinceLevelLoad - lastSent > 0.05f)
        {
            lastSent = Time.timeSinceLevelLoad;
            sentOutMethods++;

            methodName = method;
            arrayParameters = arrayParametersLocal;
            paramTypes = paramTypesLocal;

            EnsureOwner();
            RequestSerialization();

            Log($"Sent {methodName}");
        }
        else
        {
            EnqueueMethod(method, arrayParametersLocal, paramTypesLocal);
        }

    private void EnqueueMethod(string method, string[] stringParameters, Types[] types)
    {
        queue = queue.Add(new object[] {method, stringParameters, types});
        Log($"Adding to queue {method}");
        if (!isQueueRunning)
        {
            SendCustomEventDelayedSeconds("_QueueManager", 0.05f);
            isQueueRunning = true;
        }
    }


    public void _QueueManager()
    {
        Log($"QueueManager running: {queue.Length}");
        if (queue.Length != 0) // Check to make sure something magical didn't happen
        {
            var current = queue[0];
            queue = queue.Remove(0);

            methodName = (string) current[0];
            arrayParameters = (string[]) current[1];
            paramTypes = (Types[]) current[2];

            Log($"Handling queue for: {methodName}");

            lastSent = Time.timeSinceLevelLoad;
            sentOutMethods++;
            //TODO largestKnown = networkManager.largest;

            EnsureOwner();
            RequestSerialization();
        }

        if (queue.Length != 0)
        {
            SendCustomEventDelayedSeconds("_QueueManager", 0.05f);
        }
        else
        {
            isQueueRunning = false;
        }
    }

    private void EnsureOwner()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    #endregion
}
*/

namespace Miner28.UdonUtils.Network
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class NetworkedEventCaller : UdonSharpBehaviour
    {
        [HideInInspector] public NetworkReceiver networkReceiver;

        private bool _debug;
        private bool _startRun;

        private int _localSentOut;
        [UdonSynced] private int _sentOutMethods;
        
        #region Constants

        private Type[] _types =
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
            typeof(decimal),
            typeof(string),
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
            _byteOne = 1,
            _byteZero = 0;

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
        
        private object[] _parameters = new object[0];
        private int bufferOffset;

        [UdonSynced] public string methodTarget;
        [UdonSynced] private byte[] buffer = new byte[0];
        [UdonSynced] private Types[] types = new Types[0];
        [UdonSynced] private ushort[] lengths = new ushort[0];

        public override void OnPreSerialization()
        {
            if (_debug) Log($"PreSerialization - {methodTarget} - {buffer.Length} - {types.Length} - {lengths.Length}");
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            if (!result.success)
            {
                LogWarning($"Failed Serialization - {result.byteCount}");
            }
        }

        public override void OnDeserialization()
        {
            if (_debug) Log($"Deserialization - {methodTarget} - {buffer.Length} - {types.Length} - {lengths.Length}");

            if (string.IsNullOrEmpty(methodTarget)) return;

            if (_localSentOut >= _sentOutMethods && _localSentOut != 0)
            {
                if (_debug) LogWarning($"Ignoring deserialization, already sent out Local: {_localSentOut} - Global: {_sentOutMethods}");
                _localSentOut = _sentOutMethods;
                return;
            }
            
            _localSentOut = _sentOutMethods;

            ReceiveData(); //Convert data from buffer to parameters
            
            networkReceiver.byteCount += buffer.Length + types.Length * 4 + lengths.Length * 4; //Add to byte count
            
            networkReceiver.parameters = _parameters; //Set parameters to network receiver
            networkReceiver.SendCustomEvent(methodTarget); //Send event to network receiver
        }


        public void _SendMethod(SyncTarget target, string method, object[] data)
        {
            
            int requiredLength = 0;
            if (types.Length != data.Length) types = new Types[data.Length];
            if (lengths.Length != data.Length) lengths = new ushort[data.Length];
            for (int y = 0; y < data.Length; y++)
            {
                var type = data[y].GetType();
                var typeId = Array.IndexOf(_types, type);
                if (typeId == -1) return;

                var enumType = (Types) typeId;
                types[y] = enumType;

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
                        length = (ushort) BitConverter.GetStringSizeInBytes((string) data[y]);
                        requiredLength += length;
                        lengths[y] = length;
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
                        length = (ushort) ((bool[]) data[y]).Length;
                        requiredLength += length;
                        lengths[y] = length;
                        break;
                    case Types.ByteA:
                        length = (ushort) ((byte[]) data[y]).Length;
                        requiredLength += length;
                        lengths[y] = length;
                        break;
                    case Types.SByteA:
                        length = (ushort) ((sbyte[]) data[y]).Length;
                        requiredLength += length;
                        lengths[y] = length;
                        break;
                    case Types.Int16A:
                        length = (ushort) ((short[]) data[y]).Length;
                        requiredLength += length * 2;
                        lengths[y] = length;
                        break;
                    case Types.UInt16A:
                        length = (ushort) ((ushort[]) data[y]).Length;
                        requiredLength += length * 2;
                        lengths[y] = length;
                        break;
                    case Types.Int32A:
                        length = (ushort) ((int[]) data[y]).Length;
                        requiredLength += length * 4;
                        lengths[y] = length;
                        break;
                    case Types.UInt32A:
                        length = (ushort) ((uint[]) data[y]).Length;
                        requiredLength += length * 4;
                        lengths[y] = length;
                        break;
                    case Types.Int64A:
                        length = (ushort) ((long[]) data[y]).Length;
                        requiredLength += length * 8;
                        lengths[y] = length;
                        break;
                    case Types.UInt64A:
                        length = (ushort) ((ulong[]) data[y]).Length;
                        requiredLength += length * 8;
                        lengths[y] = length;
                        break;
                    case Types.SingleA:
                        length = (ushort) ((float[]) data[y]).Length;
                        requiredLength += length * 4;
                        lengths[y] = length;
                        break;
                    case Types.DoubleA:
                        length = (ushort) ((double[]) data[y]).Length;
                        requiredLength += length * 8;
                        lengths[y] = length;
                        break;
                    case Types.DecimalA:
                        length = (ushort) ((decimal[]) data[y]).Length;
                        requiredLength += length * 16;
                        lengths[y] = length;
                        break;
                    case Types.StringA:
                    {
                        _stringA = (string[]) data[y];
                        length = (ushort) (_stringA.Length - 1);
                        for (int i = 0; i < _stringA.Length; i++)
                        {
                            length += (ushort) BitConverter.GetStringSizeInBytes(_stringA[i]);
                        }

                        requiredLength += length;
                        lengths[y] = length;
                        break;
                    }
                    case Types.VRCPlayerApiA:
                        length = (ushort) ((VRCPlayerApi[]) data[y]).Length;
                        requiredLength += length * 4;
                        lengths[y] = length;
                        break;
                    case Types.ColorA:
                        length = (ushort) ((Color[]) data[y]).Length;
                        requiredLength += length * 16;
                        lengths[y] = length;
                        break;
                    case Types.Color32A:
                        length = (ushort) ((Color32[]) data[y]).Length;
                        requiredLength += length * 4;
                        lengths[y] = length;
                        break;
                    case Types.Vector2A:
                        length = (ushort) ((Vector2[]) data[y]).Length;
                        requiredLength += length * 8;
                        lengths[y] = length;
                        break;
                    case Types.Vector2IntA:
                        length = (ushort) ((Vector2Int[]) data[y]).Length;
                        requiredLength += length * 8;
                        lengths[y] = length;
                        break;
                    case Types.Vector3A:
                        length = (ushort) ((Vector3[]) data[y]).Length;
                        requiredLength += length * 12;
                        lengths[y] = length;
                        break;
                    case Types.Vector3IntA:
                        length = (ushort) ((Vector3Int[]) data[y]).Length;
                        requiredLength += length * 12;
                        lengths[y] = length;
                        break;
                    case Types.Vector4A:
                        length = (ushort) ((Vector4[]) data[y]).Length;
                        requiredLength += length * 16;
                        lengths[y] = length;
                        break;
                    case Types.QuaternionA:
                        length = (ushort) ((Quaternion[]) data[y]).Length;
                        requiredLength += length * 16;
                        lengths[y] = length;
                        break;
                }
            }

            if (buffer.Length != requiredLength) buffer = new byte[requiredLength];

            bufferOffset = 0;

            for (_iter = 0; _iter < data.Length; _iter++)
            {
                var enumType = types[_iter];
                
                switch (enumType)
                {
                    case Types.Boolean:
                        buffer[bufferOffset] = (bool) data[_iter] ? _byteOne : _byteZero;
                        bufferOffset++;
                        break;
                    case Types.Byte:
                        buffer[bufferOffset] = (byte) data[_iter];
                        bufferOffset++;
                        break;
                    case Types.SByte:
                        _sbyteValue = (sbyte) data[_iter];
                        buffer[bufferOffset] = (byte) (_sbyteValue < 0 ? (_sbyteValue + _0xFF) : _sbyteValue);
                        bufferOffset++;
                        break;
                    case Types.Int16:
                        _int16Value = (short) data[_iter];
                        _int32TMP = _int16Value < 0 ? (_int16Value + 0xFFFF) : _int16Value;
                        buffer[bufferOffset] = (byte) (_int32TMP >> Bit8);
                        buffer[bufferOffset + 1] = (byte) (_int32TMP & 0xFF);
                        bufferOffset += 2;
                        break;
                    case Types.UInt16:
                        _uint16Value = (ushort) data[_iter];
                        buffer[bufferOffset] = (byte) ((_uint16Value >> Bit8));
                        buffer[bufferOffset + 1] = (byte) (_uint16Value & 0xFF);
                        bufferOffset += 2;
                        break;
                    case Types.Int32:
                        _int32TMP2 = (int) data[_iter];
                        buffer[bufferOffset] = (byte) ((_int32TMP2 >> Bit24) & 0xFF);
                        buffer[bufferOffset + 1] = (byte) ((_int32TMP2 >> Bit16) & 0xFF);
                        buffer[bufferOffset + 2] = (byte) ((_int32TMP2 >> Bit8) & 0xFF);
                        buffer[bufferOffset + 3] = (byte) (_int32TMP2 & 0xFF);
                        bufferOffset += 4;
                        break;
                    case Types.UInt32:
                        _uint32Value = (uint) data[_iter];
                        buffer[bufferOffset] = (byte) ((_uint32Value >> Bit24) & 255u);
                        buffer[bufferOffset + 1] = (byte) ((_uint32Value >> Bit16) & 255u);
                        buffer[bufferOffset + 2] = (byte) ((_uint32Value >> Bit8) & 255u);
                        buffer[bufferOffset + 3] = (byte) (_uint32Value & 255u);
                        bufferOffset += 4;
                        break;
                    case Types.Int64:
                        _int64Value = (long) data[_iter];
                        buffer[bufferOffset] = (byte) ((_int64Value >> Bit56) & 0xFF);
                        buffer[bufferOffset + 1] = (byte) ((_int64Value >> Bit48) & 0xFF);
                        buffer[bufferOffset + 2] = (byte) ((_int64Value >> Bit40) & 0xFF);
                        buffer[bufferOffset + 3] = (byte) ((_int64Value >> Bit32) & 0xFF);
                        buffer[bufferOffset + 4] = (byte) ((_int64Value >> Bit24) & 0xFF);
                        buffer[bufferOffset + 5] = (byte) ((_int64Value >> Bit16) & 0xFF);
                        buffer[bufferOffset + 6] = (byte) ((_int64Value >> Bit8) & 0xFF);
                        buffer[bufferOffset + 7] = (byte) (_int64Value & 0xFF);
                        bufferOffset += 8;
                        break;
                    case Types.UInt64:
                        _uint64Value = (ulong) data[_iter];
                        buffer[bufferOffset] = (byte) ((_uint64Value >> Bit56) & 255ul);
                        buffer[bufferOffset + 1] = (byte) ((_uint64Value >> Bit48) & 255ul);
                        buffer[bufferOffset + 2] = (byte) ((_uint64Value >> Bit40) & 255ul);
                        buffer[bufferOffset + 3] = (byte) ((_uint64Value >> Bit32) & 255ul);
                        buffer[bufferOffset + 4] = (byte) ((_uint64Value >> Bit24) & 255ul);
                        buffer[bufferOffset + 5] = (byte) ((_uint64Value >> Bit16) & 255ul);
                        buffer[bufferOffset + 6] = (byte) ((_uint64Value >> Bit8) & 255ul);
                        buffer[bufferOffset + 7] = (byte) (_uint64Value & 255ul);
                        bufferOffset += 8;
                        break;
                    case Types.Single:
                    {
                        _singleValue = (float) data[_iter];
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

                        buffer[bufferOffset] = (byte) ((_uintTmp >> Bit24) & 255u);
                        buffer[bufferOffset + 1] = (byte) ((_uintTmp >> Bit16) & 255u);
                        buffer[bufferOffset + 2] = (byte) ((_uintTmp >> Bit8) & 255u);
                        buffer[bufferOffset + 3] = (byte) (_uintTmp & 255u);
                        bufferOffset += 4;
                        break;
                    }
                    case Types.Double:
                    {
                        _doubleValue = (double) data[_iter];
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

                        buffer[bufferOffset] = (byte) ((doubleTmp >> Bit56) & 255ul);
                        buffer[bufferOffset + 1] = (byte) ((doubleTmp >> Bit48) & 255ul);
                        buffer[bufferOffset + 2] = (byte) ((doubleTmp >> Bit40) & 255ul);
                        buffer[bufferOffset + 3] = (byte) ((doubleTmp >> Bit32) & 255ul);
                        buffer[bufferOffset + 4] = (byte) ((doubleTmp >> Bit24) & 255ul);
                        buffer[bufferOffset + 5] = (byte) ((doubleTmp >> Bit16) & 255ul);
                        buffer[bufferOffset + 6] = (byte) ((doubleTmp >> Bit8) & 255ul);
                        buffer[bufferOffset + 7] = (byte) (doubleTmp & 255ul);
                        bufferOffset += 8;
                        break;
                    }
                    case Types.Decimal:
                        _int32A = Decimal.GetBits((decimal) data[_iter]);

                        buffer[bufferOffset] = (byte) ((_tempBytes[0] >> Bit24) & _0xFF);
                        buffer[bufferOffset + 1] = (byte) ((_tempBytes[0] >> Bit16) & _0xFF);
                        buffer[bufferOffset + 2] = (byte) ((_tempBytes[0] >> Bit8) & _0xFF);
                        buffer[bufferOffset + 3] = (byte) (_tempBytes[0] & _0xFF);

                        buffer[bufferOffset + 4] = (byte) ((_tempBytes[1] >> Bit24) & _0xFF);
                        buffer[bufferOffset + 5] = (byte) ((_tempBytes[1] >> Bit16) & _0xFF);
                        buffer[bufferOffset + 6] = (byte) ((_tempBytes[1] >> Bit8) & _0xFF);
                        buffer[bufferOffset + 7] = (byte) (_tempBytes[1] & _0xFF);

                        buffer[bufferOffset + 8] = (byte) ((_tempBytes[2] >> Bit24) & _0xFF);
                        buffer[bufferOffset + 9] = (byte) ((_tempBytes[2] >> Bit16) & _0xFF);
                        buffer[bufferOffset + 10] = (byte) ((_tempBytes[2] >> Bit8) & _0xFF);
                        buffer[bufferOffset + 11] = (byte) (_tempBytes[2] & _0xFF);

                        buffer[bufferOffset + 12] = (byte) ((_tempBytes[3] >> Bit24) & _0xFF);
                        buffer[bufferOffset + 13] = (byte) ((_tempBytes[3] >> Bit16) & _0xFF);
                        buffer[bufferOffset + 14] = (byte) ((_tempBytes[3] >> Bit8) & _0xFF);
                        buffer[bufferOffset + 15] = (byte) (_tempBytes[3] & _0xFF);
                        bufferOffset += 16;
                        break;
                    case Types.String:
                    {
                        _stringData = (string) data[_iter];
                        _int32TMP = _stringData.Length;

                        for (int i = 0; i < _int32TMP; i++)
                        {
                            int value = char.ConvertToUtf32(_stringData, i);
                            if (value < 0x80)
                            {
                                buffer[bufferOffset++] = (byte) value;
                            }
                            else if (value < 0x0800)
                            {
                                buffer[bufferOffset++] = (byte) (value >> 6 | 0xC0);
                                buffer[bufferOffset++] = (byte) (value & 0x3F | 0x80);
                            }
                            else if (value < 0x010000)
                            {
                                buffer[bufferOffset++] = (byte) (value >> 12 | 0xE0);
                                buffer[bufferOffset++] = (byte) ((value >> 6) & 0x3F | 0x80);
                                buffer[bufferOffset++] = (byte) (value & 0x3F | 0x80);
                            }
                            else
                            {
                                buffer[bufferOffset++] = (byte) (value >> 18 | 0xF0);
                                buffer[bufferOffset++] = (byte) ((value >> 12) & 0x3F | 0x80);
                                buffer[bufferOffset++] = (byte) ((value >> 6) & 0x3F | 0x80);
                                buffer[bufferOffset++] = (byte) (value & 0x3F | 0x80);
                            }

                            if (char.IsSurrogate(_stringData, i)) i++;
                        }

                        break;
                    }
                    case Types.VRCPlayerApi:
                    {
                        _player = (VRCPlayerApi) data[_iter];
                        if (_player == null)
                        {
                            bufferOffset += 4;
                        }
                        else
                        {
                            _int32TMP = _player.playerId;
                            buffer[bufferOffset] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            buffer[bufferOffset + 1] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            buffer[bufferOffset + 2] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            buffer[bufferOffset + 3] = (byte) (_int32TMP & _0xFF);
                            bufferOffset += 4;
                        }

                        break;
                    }
                    case Types.Color:
                        _color = (Color) data[_iter];
                        _tempBytes = BitConverter.GetBytes(_color.r);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_color.g);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 4, 4);
                        _tempBytes = BitConverter.GetBytes(_color.b);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 8, 4);
                        _tempBytes = BitConverter.GetBytes(_color.a);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 12, 4);
                        bufferOffset += 16;
                        break;
                    case Types.Color32:
                        _color32 = (Color32) data[_iter];
                        buffer[bufferOffset] = _color32.r;
                        buffer[bufferOffset + 1] = _color32.g;
                        buffer[bufferOffset + 2] = _color32.b;
                        buffer[bufferOffset + 3] = _color32.a;
                        bufferOffset += 4;
                        break;
                    case Types.Vector2:
                        _vector2 = (Vector2) data[_iter];
                        _tempBytes = BitConverter.GetBytes(_vector2.x);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_vector2.y);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 4, 4);
                        bufferOffset += 8;
                        break;
                    case Types.Vector2Int:
                        _vector2Int = (Vector2Int) data[_iter];
                        _tempBytes = BitConverter.GetBytes(_vector2Int.x);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_vector2Int.y);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 4, 4);
                        bufferOffset += 8;
                        break;
                    case Types.Vector3:
                        _vector3 = (Vector3) data[_iter];

                        _tempBytes = BitConverter.GetBytes(_vector3.x);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_vector3.y);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 4, 4);
                        _tempBytes = BitConverter.GetBytes(_vector3.z);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 8, 4);
                        bufferOffset += 12;
                        break;
                    case Types.Vector3Int:
                        _vector3Int = (Vector3Int) data[_iter];
                        _tempBytes = BitConverter.GetBytes(_vector3Int.x);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_vector3Int.y);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 4, 4);
                        _tempBytes = BitConverter.GetBytes(_vector3Int.z);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 8, 4);
                        bufferOffset += 12;
                        break;
                    case Types.Vector4:
                        _vector4 = (Vector4) data[_iter];
                        _tempBytes = BitConverter.GetBytes(_vector4.x);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_vector4.y);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 4, 4);
                        _tempBytes = BitConverter.GetBytes(_vector4.z);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 8, 4);
                        _tempBytes = BitConverter.GetBytes(_vector4.w);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 12, 4);
                        bufferOffset += 16;
                        break;
                    case Types.Quaternion:
                        _quaternion = (Quaternion) data[_iter];
                        _tempBytes = BitConverter.GetBytes(_quaternion.x);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset, 4);
                        _tempBytes = BitConverter.GetBytes(_quaternion.y);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 4, 4);
                        _tempBytes = BitConverter.GetBytes(_quaternion.z);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 8, 4);
                        _tempBytes = BitConverter.GetBytes(_quaternion.w);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset + 12, 4);
                        bufferOffset += 16;
                        break;
                    case Types.DateTime:
                        _int64Value = ((DateTime) data[_iter]).ToBinary();
                        _tempBytes = BitConverter.GetBytes(_int64Value);
                        Array.Copy(_tempBytes, 0, buffer, bufferOffset, 8);
                        bufferOffset += 8;
                        break;
                    //Arrays
                    case Types.BooleanA:
                    {
                        _boolA = (bool[]) data[_iter];
                        for (int j = 0; j < _boolA.Length; j++)
                        {
                            buffer[bufferOffset + j] = _boolA[j] ? _byteOne : _byteZero;
                        }

                        bufferOffset += _boolA.Length;
                        break;
                    }
                    case Types.ByteA:
                        _byteA = (byte[]) data[_iter];
                        Array.Copy(_byteA, 0, buffer, bufferOffset, _byteA.Length);
                        bufferOffset += _byteA.Length;
                        break;
                    case Types.SByteA:
                    {
                        _sbyteA = (sbyte[]) data[_iter];
                        for (int j = 0; j < _sbyteA.Length; j++)
                        {
                            _sbyteValue = _sbyteA[j];
                            buffer[bufferOffset + j] = (byte) (_sbyteValue < 0 ? (_sbyteValue + 0xFFFF) : _sbyteValue);
                        }

                        bufferOffset += _sbyteA.Length;
                        break;
                    }
                    case Types.Int16A:
                    {
                        _int16A = (short[]) data[_iter];
                        for (int j = 0; j < _int16A.Length; j++)
                        {
                            _int32TMP = _int16A[j] < 0 ? (_int16A[j] + 0xFFFF) : _int16A[j];
                            buffer[bufferOffset] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            buffer[bufferOffset + 1] = (byte) (_int32TMP & _0xFF);
                            bufferOffset += 2;
                        }

                        bufferOffset += _int16A.Length * 2;
                        break;
                    }
                    case Types.UInt16A:
                    {
                        _uint16A = (ushort[]) data[_iter];
                        for (int j = 0; j < _uint16A.Length; j++)
                        {
                            _uint16Value = _uint16A[j];
                            buffer[bufferOffset] = (byte) ((_uint16Value >> Bit8) & 255u);
                            buffer[bufferOffset + 1] = (byte) (_uint16Value & 255u);
                            bufferOffset += 2;
                        }

                        bufferOffset += _uint16A.Length * 2;
                        break;
                    }
                    case Types.Int32A:
                    {
                        _int32A = (int[]) data[_iter];
                        for (int j = 0; j < _int32A.Length; j++)
                        {
                            _int32TMP2 = _int32A[j];
                            buffer[bufferOffset] = (byte) ((_int32TMP2 >> Bit24) & _0xFF);
                            buffer[bufferOffset + 1] = (byte) ((_int32TMP2 >> Bit16) & _0xFF);
                            buffer[bufferOffset + 2] = (byte) ((_int32TMP2 >> Bit8) & _0xFF);
                            buffer[bufferOffset + 3] = (byte) (_int32TMP2 & _0xFF);
                            bufferOffset += 4;
                        }

                        bufferOffset += _int32A.Length * 4;
                        break;
                    }
                    case Types.UInt32A:
                    {
                        _uint32A = (uint[]) data[_iter];
                        for (int j = 0; j < _uint32A.Length; j++)
                        {
                            _uint32Value = _uint32A[j];
                            buffer[bufferOffset] = (byte) ((_uint32Value >> Bit24) & 255u);
                            buffer[bufferOffset + 1] = (byte) ((_uint32Value >> Bit16) & 255u);
                            buffer[bufferOffset + 2] = (byte) ((_uint32Value >> Bit8) & 255u);
                            buffer[bufferOffset + 3] = (byte) (_uint32Value & 255u);
                            bufferOffset += 4;
                        }

                        bufferOffset += _uint32A.Length * 4;
                        break;
                    }
                    case Types.Int64A:
                    {
                        _int64A = (long[]) data[_iter];
                        for (int j = 0; j < _int64A.Length; j++)
                        {
                            _int64Value = _int64A[j];
                            buffer[bufferOffset] = (byte) ((_int64Value >> Bit56) & _0xFF);
                            buffer[bufferOffset + 1] = (byte) ((_int64Value >> Bit48) & _0xFF);
                            buffer[bufferOffset + 2] = (byte) ((_int64Value >> Bit40) & _0xFF);
                            buffer[bufferOffset + 3] = (byte) ((_int64Value >> Bit32) & _0xFF);
                            buffer[bufferOffset + 4] = (byte) ((_int64Value >> Bit24) & _0xFF);
                            buffer[bufferOffset + 5] = (byte) ((_int64Value >> Bit16) & _0xFF);
                            buffer[bufferOffset + 6] = (byte) ((_int64Value >> Bit8) & _0xFF);
                            buffer[bufferOffset + 7] = (byte) (_int64Value & _0xFF);
                            bufferOffset += 8;
                        }

                        bufferOffset += _int64A.Length * 8;
                        break;
                    }
                    case Types.UInt64A:
                    {
                        _uint64A = (ulong[]) data[_iter];
                        for (int j = 0; j < _uint64A.Length; j++)
                        {
                            _uint64Value = _uint64A[j];
                            buffer[bufferOffset] = (byte) ((_uint64Value >> Bit56) & 255ul);
                            buffer[bufferOffset + 1] = (byte) ((_uint64Value >> Bit48) & 255ul);
                            buffer[bufferOffset + 2] = (byte) ((_uint64Value >> Bit40) & 255ul);
                            buffer[bufferOffset + 3] = (byte) ((_uint64Value >> Bit32) & 255ul);
                            buffer[bufferOffset + 4] = (byte) ((_uint64Value >> Bit24) & 255ul);
                            buffer[bufferOffset + 5] = (byte) ((_uint64Value >> Bit16) & 255ul);
                            buffer[bufferOffset + 6] = (byte) ((_uint64Value >> Bit8) & 255ul);
                            buffer[bufferOffset + 7] = (byte) (_uint64Value & 255ul);
                            bufferOffset += 8;
                        }

                        bufferOffset += _uint64A.Length * 8;
                        break;
                    }
                    case Types.SingleA:
                    {
                        _singleA = (float[]) data[_iter];
                        for (int j = 0; j < _singleA.Length; j++)
                        {
                            _tempBytes = BitConverter.GetBytes(_singleA[j]);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + j * 4, 4);
                        }

                        bufferOffset += _singleA.Length * 4;
                        break;
                    }
                    case Types.DoubleA:
                    {
                        _doubleA = (double[]) data[_iter];
                        for (int j = 0; j < _doubleA.Length; j++)
                        {
                            _tempBytes = BitConverter.GetBytes(_doubleA[j]);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + j * 8, 8);
                        }

                        bufferOffset += _doubleA.Length * 8;
                        break;
                    }
                    case Types.DecimalA:
                        break;
                    case Types.StringA:
                    {
                        _stringA = (string[]) data[_iter];
                        for (int j = 0; j < _stringA.Length; j++)
                        {
                            _tempBytes = BitConverter.GetBytes(_stringA[j]);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset, _tempBytes.Length);
                            bufferOffset += _tempBytes.Length;
                            if (j < _stringA.Length - 1)
                            {
                                buffer[bufferOffset] = _byteZero;
                                bufferOffset++;
                            }
                        }

                        break;
                    }
                    case Types.VRCPlayerApiA:
                    {
                        _vrcPlayerApiA = (VRCPlayerApi[]) data[_iter];
                        for (int j = 0; j < _vrcPlayerApiA.Length; j++)
                        {
                            _int32TMP = _vrcPlayerApiA[j].playerId;
                            buffer[bufferOffset] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            buffer[bufferOffset + 1] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            buffer[bufferOffset + 2] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            buffer[bufferOffset + 3] = (byte) (_int32TMP & _0xFF);
                            bufferOffset += 4;
                        }

                        bufferOffset += _vrcPlayerApiA.Length * 4;
                        break;
                    }
                    case Types.ColorA:
                    {
                        _colorA = (Color[]) data[_iter];
                        for (int j = 0; j < _colorA.Length; j++)
                        {
                            _color = _colorA[j];
                            _tempBytes = BitConverter.GetBytes(_color.r);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset, 4);
                            _tempBytes = BitConverter.GetBytes(_color.g);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + 4, 4);
                            _tempBytes = BitConverter.GetBytes(_color.b);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + 8, 4);
                            _tempBytes = BitConverter.GetBytes(_color.a);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + 12, 4);
                            bufferOffset += 16;
                        }

                        break;
                    }
                    case Types.Color32A:
                    {
                        _color32A = (Color32[]) data[_iter];
                        for (int j = 0; j < _color32A.Length; j++)
                        {
                            _color32 = _color32A[j];
                            buffer[bufferOffset] = _color32.r;
                            buffer[bufferOffset + 1] = _color32.g;
                            buffer[bufferOffset + 2] = _color32.b;
                            buffer[bufferOffset + 3] = _color32.a;
                            bufferOffset += 4;
                        }

                        bufferOffset += _color32A.Length * 4;
                        break;
                    }
                    case Types.Vector2A:
                    {
                        _vector2A = (Vector2[]) data[_iter];
                        for (int j = 0; j < _vector2A.Length; j++)
                        {
                            _vector2 = _vector2A[j];
                            _tempBytes = BitConverter.GetBytes(_vector2.x);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset, 4);
                            _tempBytes = BitConverter.GetBytes(_vector2.y);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + 4, 4);
                            bufferOffset += 8;
                        }

                        bufferOffset += _vector2A.Length * 8;
                        break;
                    }
                    case Types.Vector2IntA:
                    {
                        _vector2IntA = (Vector2Int[]) data[_iter];
                        for (int j = 0; j < _vector2IntA.Length; j++)
                        {
                            _vector2Int = _vector2IntA[j];
                            _int32TMP = _vector2Int.x;
                            buffer[bufferOffset] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            buffer[bufferOffset + 1] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            buffer[bufferOffset + 2] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            buffer[bufferOffset + 3] = (byte) (_int32TMP & _0xFF);
                            _int32TMP = _vector2Int.y;
                            buffer[bufferOffset + 4] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            buffer[bufferOffset + 5] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            buffer[bufferOffset + 6] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            buffer[bufferOffset + 7] = (byte) (_int32TMP & _0xFF);
                            bufferOffset += 8;
                        }

                        break;
                    }
                    case Types.Vector3A:
                    {
                        _vector3A = (Vector3[]) data[_iter];
                        for (int j = 0; j < _vector3A.Length; j++)
                        {
                            _vector3 = _vector3A[j];
                            _tempBytes = BitConverter.GetBytes(_vector3.x);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + j * 12, 4);
                            _tempBytes = BitConverter.GetBytes(_vector3.y);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + j * 12 + 4, 4);
                            _tempBytes = BitConverter.GetBytes(_vector3.z);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + j * 12 + 8, 4);
                        }

                        bufferOffset += _vector3A.Length * 12;
                        break;
                    }
                    case Types.Vector3IntA:
                    {
                        _vector3IntA = (Vector3Int[]) data[_iter];
                        for (int j = 0; j < _vector3IntA.Length; j++)
                        {
                            _vector3Int = _vector3IntA[j];
                            _int32TMP = _vector3Int.x;
                            buffer[bufferOffset] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            buffer[bufferOffset + 1] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            buffer[bufferOffset + 2] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            buffer[bufferOffset + 3] = (byte) (_int32TMP & _0xFF);
                            _int32TMP = _vector3Int.y;
                            buffer[bufferOffset + 4] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            buffer[bufferOffset + 5] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            buffer[bufferOffset + 6] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            buffer[bufferOffset + 7] = (byte) (_int32TMP & _0xFF);
                            _int32TMP = _vector3Int.z;
                            buffer[bufferOffset + 8] = (byte) ((_int32TMP >> Bit24) & _0xFF);
                            buffer[bufferOffset + 9] = (byte) ((_int32TMP >> Bit16) & _0xFF);
                            buffer[bufferOffset + 10] = (byte) ((_int32TMP >> Bit8) & _0xFF);
                            buffer[bufferOffset + 11] = (byte) (_int32TMP & _0xFF);
                            bufferOffset += 12;
                        }

                        break;
                    }
                    case Types.Vector4A:
                    {
                        _vector4A = (Vector4[]) data[_iter];
                        for (int j = 0; j < _vector4A.Length; j++)
                        {
                            _vector4 = _vector4A[j];
                            _tempBytes = BitConverter.GetBytes(_vector4.x);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset, 4);
                            _tempBytes = BitConverter.GetBytes(_vector4.y);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + 4, 4);
                            _tempBytes = BitConverter.GetBytes(_vector4.z);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + 8, 4);
                            _tempBytes = BitConverter.GetBytes(_vector4.w);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + 12, 4);
                            bufferOffset += 16;
                        }

                        break;
                    }
                    case Types.QuaternionA:
                    {
                        _quaternionA = (Quaternion[]) data[_iter];
                        for (int j = 0; j < _quaternionA.Length; j++)
                        {
                            _quaternion = _quaternionA[j];
                            _tempBytes = BitConverter.GetBytes(_quaternion.x);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + j * 16, 4);
                            _tempBytes = BitConverter.GetBytes(_quaternion.y);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + j * 16 + 4, 4);
                            _tempBytes = BitConverter.GetBytes(_quaternion.z);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + j * 16 + 8, 4);
                            _tempBytes = BitConverter.GetBytes(_quaternion.w);
                            Array.Copy(_tempBytes, 0, buffer, bufferOffset + j * 16 + 12, 4);
                        }

                        bufferOffset += _quaternionA.Length * 16;
                        break;
                    }
                }
            }

            //Locally receive the method without going through conversions
            if (target != SyncTarget.Others)
            {
                networkReceiver.parameters = data;
                networkReceiver.SendCustomEvent(method);
            }

            methodTarget = method;
            _sentOutMethods++;
            RequestSerialization();
        }

        private void ReceiveData()
        {
            if (_parameters.Length != types.Length)
            {
                _parameters = new object[types.Length];
            }

            bufferOffset = 0;

            for (_iter = 0; _iter < _parameters.Length; _iter++)
            {
                _ushortLength = lengths[_iter];
                string[] chars;
                switch (types[_iter])
                {
                    case Types.Boolean:
                        _parameters[_iter] = buffer[bufferOffset] == _byteOne;
                        bufferOffset++;
                        break;
                    case Types.Byte:
                        _parameters[_iter] = buffer[bufferOffset];
                        bufferOffset++;
                        break;
                    case Types.SByte:
                    {
                        _int32TMP = buffer[bufferOffset];
                        if (_int32TMP >= 0x80) _int32TMP -= _0xFF;
                        _parameters[_iter] = Convert.ToSByte(_int32TMP);
                        bufferOffset++;
                        break;
                    }
                    case Types.Int16:
                    {
                        _int32TMP = (buffer[bufferOffset] << Bit8) | buffer[bufferOffset + 1];
                        if (_int32TMP >= 0x8000) _int32TMP -= _0xFFFF;
                        _parameters[_iter] = Convert.ToInt16(_int32TMP);
                        bufferOffset += 2;
                        break;
                    }
                    case Types.UInt16:
                        _int32TMP = (buffer[bufferOffset] << Bit8) | buffer[bufferOffset + 1];
                        _parameters[_iter] = Convert.ToUInt16(_int32TMP);
                        bufferOffset += 2;
                        break;
                    case Types.Int32:
                        _parameters[_iter] = (buffer[bufferOffset] << Bit24) | (buffer[bufferOffset + 1] << Bit16) |
                                             (buffer[bufferOffset + 2] << Bit8) | buffer[bufferOffset + 3];
                        bufferOffset += 4;
                        break;
                    case Types.UInt32:
                        _parameters[_iter] = (uint) ((buffer[bufferOffset] << Bit24) |
                                                     (buffer[bufferOffset + 1] << Bit16) |
                                                     (buffer[bufferOffset + 2] << Bit8) | buffer[bufferOffset + 3]);
                        bufferOffset += 4;
                        break;
                    case Types.Int64:
                        _parameters[_iter] = ((long) buffer[bufferOffset] << Bit56) |
                                             ((long) buffer[bufferOffset + 1] << Bit48) |
                                             ((long) buffer[bufferOffset + 2] << Bit40) |
                                             ((long) buffer[bufferOffset + 3] << Bit32) |
                                             ((long) buffer[bufferOffset + 4] << Bit24) |
                                             ((long) buffer[bufferOffset + 5] << Bit16) |
                                             ((long) buffer[bufferOffset + 6] << Bit8) |
                                             buffer[bufferOffset + 7];
                        bufferOffset += 8;
                        break;
                    case Types.UInt64:
                        _parameters[_iter] = ((ulong) buffer[bufferOffset] << Bit56) |
                                             ((ulong) buffer[bufferOffset + 1] << Bit48) |
                                             ((ulong) buffer[bufferOffset + 2] << Bit40) |
                                             ((ulong) buffer[bufferOffset + 3] << Bit32) |
                                             ((ulong) buffer[bufferOffset + 4] << Bit24) |
                                             ((ulong) buffer[bufferOffset + 5] << Bit16) |
                                             ((ulong) buffer[bufferOffset + 6] << Bit8) |
                                             buffer[bufferOffset + 7];

                        bufferOffset += 8;
                        break;
                    case Types.Single:
                    {
                        _uint32Value = ((uint) buffer[bufferOffset] << Bit24) |
                                       ((uint) buffer[bufferOffset + 1] << Bit16) |
                                       ((uint) buffer[bufferOffset + 2] << Bit8) | buffer[bufferOffset + 3];
                        bufferOffset += 4;
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
                        _uint64Value = ((ulong) buffer[bufferOffset] << Bit56) |
                                       ((ulong) buffer[bufferOffset + 1] << Bit48) |
                                       ((ulong) buffer[bufferOffset + 2] << Bit40) |
                                       ((ulong) buffer[bufferOffset + 3] << Bit32) |
                                       ((ulong) buffer[bufferOffset + 4] << Bit24) |
                                       ((ulong) buffer[bufferOffset + 5] << Bit16) |
                                       ((ulong) buffer[bufferOffset + 6] << Bit8) | buffer[bufferOffset + 7];

                        bufferOffset += 8;

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
                        _parameters[_iter] = new decimal(new[]
                        {
                            buffer[bufferOffset] << Bit24 | buffer[bufferOffset + 1] << Bit16 |
                            buffer[bufferOffset + 2] << Bit8 | buffer[bufferOffset + 3],

                            buffer[bufferOffset + 4] << Bit24 | buffer[bufferOffset + 5] << Bit16 |
                            buffer[bufferOffset + 6] << Bit8 | buffer[bufferOffset + 7],

                            buffer[bufferOffset + 8] << Bit24 | buffer[bufferOffset + 9] << Bit16 |
                            buffer[bufferOffset + 10] << Bit8 | buffer[bufferOffset + 11],

                            buffer[bufferOffset + 12] << Bit24 | buffer[bufferOffset + 13] << Bit16 |
                            buffer[bufferOffset + 14] << Bit8 | buffer[bufferOffset + 15]
                        });
                        bufferOffset += 16;
                        break;
                    case Types.String:
                    {
                        _int32TMP = 0;
                        _int32TMP2 = 0;
                        _int32TMP3 = 0;
                        _int32TMP4 = lengths[_iter];
                        chars = new string[_int32TMP4];

                        _int32TMP4 += bufferOffset;


                        for (var i = bufferOffset; i < _int32TMP4; i++)
                        {
                            _byteTmp = buffer[i];
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
                        bufferOffset += lengths[_iter];
                        break;
                    }
                    case Types.VRCPlayerApi:
                        _parameters[_iter] = VRCPlayerApi.GetPlayerById(
                            buffer[bufferOffset] << Bit24 |
                            buffer[bufferOffset + 1] << Bit16 |
                            buffer[bufferOffset + 2] << Bit8 |
                            buffer[bufferOffset + 3]);
                        bufferOffset += 4;
                        break;
                    case Types.Color:
                        _tempBytes = new byte[4];
                        Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                        _singleValue = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(buffer, bufferOffset + 4, _tempBytes, 0, 4);
                        _singleValue2 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(buffer, bufferOffset + 8, _tempBytes, 0, 4);
                        _singleValue3 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(buffer, bufferOffset + 12, _tempBytes, 0, 4);
                        _singleValue4 = BitConverter.ToSingle(_tempBytes);
                        _parameters[_iter] = new Color(_singleValue, _singleValue2, _singleValue3, _singleValue4);
                        bufferOffset += 16;
                        break;
                    case Types.Color32:
                        _parameters[_iter] = new Color32(buffer[bufferOffset], buffer[bufferOffset + 1],
                            buffer[bufferOffset + 2], buffer[bufferOffset + 3]);
                        bufferOffset += 4;
                        break;
                    case Types.Vector2:
                        _tempBytes = new byte[4];
                        Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                        _singleValue = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(buffer, bufferOffset + 4, _tempBytes, 0, 4);
                        _singleValue2 = BitConverter.ToSingle(_tempBytes);
                        _parameters[_iter] = new Vector2(_singleValue, _singleValue2);
                        bufferOffset += 8;
                        break;
                    case Types.Vector2Int:
                        _parameters[_iter] = new Vector2Int(
                            buffer[bufferOffset] << Bit24 |
                            buffer[bufferOffset + 1] << Bit16 |
                            buffer[bufferOffset + 2] << Bit8 |
                            buffer[bufferOffset + 3],
                            buffer[bufferOffset + 4] << Bit24 |
                            buffer[bufferOffset + 5] << Bit16 |
                            buffer[bufferOffset + 6] << Bit8 |
                            buffer[bufferOffset + 7]);
                        bufferOffset += 8;
                        break;
                    case Types.Vector3:
                        _tempBytes = new byte[4];
                        Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                        _singleValue = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(buffer, bufferOffset + 4, _tempBytes, 0, 4);
                        _singleValue2 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(buffer, bufferOffset + 8, _tempBytes, 0, 4);
                        _singleValue3 = BitConverter.ToSingle(_tempBytes);
                        _parameters[_iter] = new Vector3(_singleValue, _singleValue2, _singleValue3);
                        bufferOffset += 12;
                        break;
                    case Types.Vector3Int:
                        _parameters[_iter] = new Vector3Int(
                            buffer[bufferOffset] << Bit24 |
                            buffer[bufferOffset + 1] << Bit16 |
                            buffer[bufferOffset + 2] << Bit8 |
                            buffer[bufferOffset + 3],
                            buffer[bufferOffset + 4] << Bit24 |
                            buffer[bufferOffset + 5] << Bit16 |
                            buffer[bufferOffset + 6] << Bit8 |
                            buffer[bufferOffset + 7],
                            buffer[bufferOffset + 8] << Bit24 |
                            buffer[bufferOffset + 9] << Bit16 |
                            buffer[bufferOffset + 10] << Bit8 |
                            buffer[bufferOffset + 11]);
                        bufferOffset += 12;
                        break;
                    case Types.Vector4:
                        _tempBytes = new byte[4];
                        Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                        _singleValue = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(buffer, bufferOffset + 4, _tempBytes, 0, 4);
                        _singleValue2 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(buffer, bufferOffset + 8, _tempBytes, 0, 4);
                        _singleValue3 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(buffer, bufferOffset + 12, _tempBytes, 0, 4);
                        _singleValue4 = BitConverter.ToSingle(_tempBytes);
                        _parameters[_iter] = new Vector4(_singleValue, _singleValue2, _singleValue3, _singleValue4);
                        bufferOffset += 16;
                        break;
                    case Types.Quaternion:
                        _tempBytes = new byte[4];
                        Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                        _singleValue = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(buffer, bufferOffset + 4, _tempBytes, 0, 4);
                        _singleValue2 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(buffer, bufferOffset + 8, _tempBytes, 0, 4);
                        _singleValue3 = BitConverter.ToSingle(_tempBytes);
                        Array.Copy(buffer, bufferOffset + 12, _tempBytes, 0, 4);
                        _singleValue4 = BitConverter.ToSingle(_tempBytes);
                        _parameters[_iter] = new Quaternion(_singleValue, _singleValue2, _singleValue3, _singleValue4);
                        bufferOffset += 16;
                        break;
                    case Types.DateTime:
                        _parameters[_iter] = DateTime.FromBinary(
                            (long) buffer[bufferOffset] << Bit56 |
                            (long) buffer[bufferOffset + 1] << Bit48 |
                            (long) buffer[bufferOffset + 2] << Bit40 |
                            (long) buffer[bufferOffset + 3] << Bit32 |
                            (long) buffer[bufferOffset + 4] << Bit24 |
                            (long) buffer[bufferOffset + 5] << Bit16 |
                            (long) buffer[bufferOffset + 6] << Bit8 |
                            (long) buffer[bufferOffset + 7]);
                        bufferOffset += 8;
                        break;
                    //Arrays
                    case Types.BooleanA:
                    {
                        _ushortLength = lengths[_iter];
                        if (_boolA.Length != _ushortLength)
                        {
                            _boolA = new bool[_ushortLength];
                        }

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _boolA[i] = buffer[bufferOffset] == _byteOne;
                            bufferOffset++;
                        }

                        _parameters[_iter] = _boolA;
                        break;
                    }
                    case Types.ByteA:
                    {
                        _ushortLength = lengths[_iter];
                        if (_byteA.Length != _ushortLength)
                        {
                            _byteA = new byte[_ushortLength];
                        }

                        Array.Copy(buffer, bufferOffset, _byteA, 0, _byteA.Length);
                        _parameters[_iter] = _byteA;
                        bufferOffset += _byteA.Length;
                        break;
                    }
                    case Types.SByteA:
                    {
                        _ushortLength = lengths[_iter];
                        if (_sbyteA.Length != _ushortLength)
                        {
                            _sbyteA = new sbyte[_ushortLength];
                        }

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _int32TMP = buffer[bufferOffset];
                            if (_int32TMP >= 0x80) _int32TMP -= _0xFF;
                            _sbyteA[i] = Convert.ToSByte(_int32TMP);
                            bufferOffset++;
                        }

                        _parameters[_iter] = _sbyteA;
                        break;
                    }
                    case Types.Int16A:
                    {
                        _ushortLength = lengths[_iter];
                        if (_int16A.Length != _ushortLength)
                        {
                            _int16A = new short[_ushortLength];
                        }

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _int32TMP = (buffer[bufferOffset] << Bit8) | buffer[bufferOffset + 1];
                            if (_int32TMP >= 0x8000) _int32TMP -= _0xFFFF;
                            _int16A[i] = Convert.ToInt16(_int32TMP);
                            bufferOffset += 2;
                        }

                        _parameters[_iter] = _int16A;
                        break;
                    }
                    case Types.UInt16A:
                    {
                        _ushortLength = lengths[_iter];
                        if (_uint16A.Length != _ushortLength)
                        {
                            _uint16A = new ushort[_ushortLength];
                        }

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _uint16A[i] = (ushort) ((ushort) (buffer[bufferOffset] << Bit8) | buffer[bufferOffset + 1]);
                            bufferOffset += 2;
                        }

                        _parameters[_iter] = _uint16A;
                        break;
                    }
                    case Types.Int32A:
                    {
                        _ushortLength = lengths[_iter];
                        if (_int32A.Length != _ushortLength)
                        {
                            _int32A = new int[_ushortLength];
                        }

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _int32A[i] = (buffer[bufferOffset] << Bit24) | (buffer[bufferOffset + 1] << Bit16) |
                                         (buffer[bufferOffset + 2] << Bit8) | buffer[bufferOffset + 3];
                            bufferOffset += 4;
                        }

                        _parameters[_iter] = _int32A;
                        break;
                    }
                    case Types.UInt32A:
                    {
                        _ushortLength = lengths[_iter];
                        if (_uint32A.Length != _ushortLength)
                        {
                            _uint32A = new uint[_ushortLength];
                        }

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _uint32A[i] = (uint) (buffer[bufferOffset] << Bit24) |
                                          (uint) (buffer[bufferOffset + 1] << Bit16) |
                                          (uint) (buffer[bufferOffset + 2] << Bit8) | buffer[bufferOffset + 3];
                            bufferOffset += 4;
                        }

                        _parameters[_iter] = _uint32A;
                        break;
                    }
                    case Types.Int64A:
                    {
                        _ushortLength = lengths[_iter];
                        if (_int64A.Length != _ushortLength)
                        {
                            _int64A = new long[_ushortLength];
                        }

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _int64A[i] = ((long) buffer[bufferOffset] << Bit56) |
                                         ((long) buffer[bufferOffset + 1] << Bit48) |
                                         ((long) buffer[bufferOffset + 2] << Bit40) |
                                         ((long) buffer[bufferOffset + 3] << Bit32) |
                                         ((long) buffer[bufferOffset + 4] << Bit24) |
                                         ((long) buffer[bufferOffset + 5] << Bit16) |
                                         ((long) buffer[bufferOffset + 6] << Bit8) | buffer[bufferOffset + 7];
                            bufferOffset += 8;
                        }

                        _parameters[_iter] = _int64A;
                        break;
                    }
                    case Types.UInt64A:
                    {
                        _ushortLength = lengths[_iter];
                        if (_uint64A.Length != _ushortLength)
                        {
                            _uint64A = new ulong[_ushortLength];
                        }

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _uint64A[i] = ((ulong) buffer[bufferOffset] << Bit56) |
                                          ((ulong) buffer[bufferOffset + 1] << Bit48) |
                                          ((ulong) buffer[bufferOffset + 2] << Bit40) |
                                          ((ulong) buffer[bufferOffset + 3] << Bit32) |
                                          ((ulong) buffer[bufferOffset + 4] << Bit24) |
                                          ((ulong) buffer[bufferOffset + 5] << Bit16) |
                                          ((ulong) buffer[bufferOffset + 6] << Bit8) | buffer[bufferOffset + 7];
                            bufferOffset += 8;
                        }

                        _parameters[_iter] = _uint64A;
                        break;
                    }
                    case Types.SingleA:
                    {
                        _ushortLength = lengths[_iter];
                        if (_singleA.Length != _ushortLength)
                        {
                            _singleA = new float[_ushortLength];
                        }

                        _tempBytes = new byte[4];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleA[i] = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                        }

                        _parameters[_iter] = _singleA;
                        break;
                    }
                    case Types.DoubleA:
                    {
                        _ushortLength = lengths[_iter];
                        if (_doubleA.Length != _ushortLength)
                        {
                            _doubleA = new double[_ushortLength];
                        }

                        _tempBytes = new byte[8];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 8);
                            _doubleA[i] = BitConverter.ToDouble(_tempBytes);
                            bufferOffset += 8;
                        }

                        _parameters[_iter] = _doubleA;
                        break;
                    }
                    case Types.DecimalA:
                    {
                        _ushortLength = lengths[_iter];
                        if (_decimalA.Length != _ushortLength)
                        {
                            _decimalA = new decimal[_ushortLength];
                        }

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _decimalA[i] = new decimal(new int[]
                            {
                                buffer[bufferOffset] << Bit24 | buffer[bufferOffset + 1] << Bit16 |
                                buffer[bufferOffset + 2] << Bit8 | buffer[bufferOffset + 3],
                                buffer[bufferOffset + 4] << Bit24 | buffer[bufferOffset + 5] << Bit16 |
                                buffer[bufferOffset + 6] << Bit8 | buffer[bufferOffset + 7],
                                buffer[bufferOffset + 8] << Bit24 | buffer[bufferOffset + 9] << Bit16 |
                                buffer[bufferOffset + 10] << Bit8 | buffer[bufferOffset + 11],
                                buffer[bufferOffset + 12] << Bit24 | buffer[bufferOffset + 13] << Bit16 |
                                buffer[bufferOffset + 14] << Bit8 | buffer[bufferOffset + 15]
                            });
                            bufferOffset += 16;
                        }

                        _parameters[_iter] = _decimalA;
                        break;
                    }
                    case Types.StringA:
                    {
                        _int32TMP = 0;
                        _int32TMP2 = 0;
                        _int32TMP3 = 0;
                        _int32TMP4 = lengths[_iter];
                        chars = new string[_int32TMP4];

                        _int32TMP4 += bufferOffset;


                        for (var i = bufferOffset; i < _int32TMP4; i++)
                        {
                            _byteTmp = buffer[i];
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

                        _parameters[_iter] = string.Concat(chars).Split('\0');
                        bufferOffset += lengths[_iter];
                        break;
                    }
                    case Types.VRCPlayerApiA:
                    {
                        _ushortLength = lengths[_iter];
                        if (_vrcPlayerApiA.Length != _ushortLength)
                        {
                            _vrcPlayerApiA = new VRCPlayerApi[_ushortLength];
                        }

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _int32TMP = (buffer[bufferOffset] << Bit24) | (buffer[bufferOffset + 1] << Bit16) |
                                        (buffer[bufferOffset + 2] << Bit8) | buffer[bufferOffset + 3];
                            _vrcPlayerApiA[i] = VRCPlayerApi.GetPlayerById(_int32TMP);
                            bufferOffset += 4;
                        }

                        _parameters[_iter] = _vrcPlayerApiA;
                        break;
                    }
                    case Types.ColorA:
                    {
                        _ushortLength = lengths[_iter];
                        if (_colorA.Length != _ushortLength)
                        {
                            _colorA = new Color[_ushortLength];
                        }

                        _tempBytes = new byte[4];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue2 = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue3 = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue4 = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            _colorA[i] = new Color(_singleValue, _singleValue2, _singleValue3, _singleValue4);
                        }

                        _parameters[_iter] = _colorA;
                        break;
                    }
                    case Types.Color32A:
                    {
                        _ushortLength = lengths[_iter];
                        if (_color32A.Length != _ushortLength)
                        {
                            _color32A = new Color32[_ushortLength];
                        }

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _color32A[i] = new Color32(buffer[bufferOffset], buffer[bufferOffset + 1],
                                buffer[bufferOffset + 2], buffer[bufferOffset + 3]);
                            bufferOffset += 4;
                        }

                        _parameters[_iter] = _color32A;
                        break;
                    }
                    case Types.Vector2A:
                    {
                        _ushortLength = lengths[_iter];
                        if (_vector2A.Length != _ushortLength)
                        {
                            _vector2A = new Vector2[_ushortLength];
                        }

                        _tempBytes = new byte[4];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue2 = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            _vector2A[i] = new Vector2(_singleValue, _singleValue2);
                        }

                        _parameters[_iter] = _vector2A;
                        break;
                    }
                    case Types.Vector2IntA:
                    {
                        _ushortLength = lengths[_iter];
                        if (_vector2IntA.Length != _ushortLength)
                        {
                            _vector2IntA = new Vector2Int[_ushortLength];
                        }

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _vector2IntA[i] = new Vector2Int(
                                buffer[bufferOffset] << Bit24 | buffer[bufferOffset + 1] << Bit16 |
                                buffer[bufferOffset + 2] << Bit8 | buffer[bufferOffset + 3],
                                buffer[bufferOffset + 4] << Bit24 | buffer[bufferOffset + 5] << Bit16 |
                                buffer[bufferOffset + 6] << Bit8 | buffer[bufferOffset + 7]);
                            bufferOffset += 8;
                        }

                        _parameters[_iter] = _vector2IntA;
                        break;
                    }
                    case Types.Vector3A:
                    {
                        _ushortLength = lengths[_iter];
                        if (_vector3A.Length != _ushortLength)
                        {
                            _vector3A = new Vector3[_ushortLength];
                        }

                        _tempBytes = new byte[4];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue2 = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue3 = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            _vector3A[i] = new Vector3(_singleValue, _singleValue2, _singleValue3);
                        }

                        _parameters[_iter] = _vector3A;
                        break;
                    }
                    case Types.Vector3IntA:
                    {
                        _ushortLength = lengths[_iter];
                        if (_vector3IntA.Length != _ushortLength)
                        {
                            _vector3IntA = new Vector3Int[_ushortLength];
                        }

                        for (var i = 0; i < _ushortLength; i++)
                        {
                            _vector3IntA[i] = new Vector3Int(
                                buffer[bufferOffset] << Bit24 | buffer[bufferOffset + 1] << Bit16 |
                                buffer[bufferOffset + 2] << Bit8 | buffer[bufferOffset + 3],
                                buffer[bufferOffset + 4] << Bit24 | buffer[bufferOffset + 5] << Bit16 |
                                buffer[bufferOffset + 6] << Bit8 | buffer[bufferOffset + 7],
                                buffer[bufferOffset + 8] << Bit24 | buffer[bufferOffset + 9] << Bit16 |
                                buffer[bufferOffset + 10] << Bit8 | buffer[bufferOffset + 11]);
                            bufferOffset += 12;
                        }

                        _parameters[_iter] = _vector3IntA;
                        break;
                    }
                    case Types.Vector4A:
                    {
                        _ushortLength = lengths[_iter];
                        if (_vector4A.Length != _ushortLength)
                        {
                            _vector4A = new Vector4[_ushortLength];
                        }

                        _tempBytes = new byte[4];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue2 = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue3 = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue4 = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            _vector4A[i] = new Vector4(_singleValue, _singleValue2, _singleValue3, _singleValue4);
                        }

                        _parameters[_iter] = _vector4A;
                        break;
                    }
                    case Types.QuaternionA:
                    {
                        _ushortLength = lengths[_iter];
                        if (_quaternionA.Length != _ushortLength)
                        {
                            _quaternionA = new Quaternion[_ushortLength];
                        }

                        _tempBytes = new byte[4];
                        for (var i = 0; i < _ushortLength; i++)
                        {
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue2 = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue3 = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            Array.Copy(buffer, bufferOffset, _tempBytes, 0, 4);
                            _singleValue4 = BitConverter.ToSingle(_tempBytes);
                            bufferOffset += 4;
                            _quaternionA[i] = new Quaternion(_singleValue, _singleValue2, _singleValue3, _singleValue4);
                        }

                        _parameters[_iter] = _quaternionA;
                        break;
                    }
                }
            }
            
            
        }

        private void OnEnable()
        {
            if (_startRun) return;
            _startRun = true;
            _debug = networkReceiver.networkManager.debug;
        }

        private void Log(string log) => Debug.Log($"<color=#00FFFF>[NetCaller]</color> {log}");
        private void Log(object log) => Debug.Log($"<color=#00FFFF>[NetCaller]</color> {log}");
        private void LogWarning(string log) => Debug.LogWarning($"<color=#FF8000>[WARN]</color> <color=#00FFFF>[NetCaller]</color> {log}");
        private void LogWarning(object log) => Debug.LogWarning($"<color=#FF8000>[WARN]</color> <color=#00FFFF>[NetCaller]</color> {log}");
        private void LogError(string log) => Debug.LogError($"<color=#FF0000>[WARN]</color> <color=#00FFFF>[NetCaller]</color> {log}");
        private void LogError(object log) => Debug.LogError($"<color=#FF0000>[WARN]</color> <color=#00FFFF>[NetCaller]</color> {log}");
    }
}
