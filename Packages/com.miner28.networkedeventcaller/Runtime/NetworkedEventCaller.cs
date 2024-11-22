using System;
using System.Diagnostics;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon.Common;
using Debug = UnityEngine.Debug;

namespace Miner28.UdonUtils.Network
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public partial class NetworkedEventCaller : UdonSharpBehaviour
    {
        [HideInInspector] public NetworkManager networkManager;
        [HideInInspector] public NetworkInterface[] sceneInterfaces;
        [HideInInspector] public uint[] sceneInterfacesIds;

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
            Int16VN = (byte) Types.Int16VN,
            UInt16V = (byte) Types.UInt16V,
            Int32V = (byte) Types.Int32V,
            Int32VN = (byte) Types.Int32VN,
            UInt32V = (byte) Types.UInt32V,
            Int64V = (byte) Types.Int64V,
            Int64VN = (byte) Types.Int64VN,
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
        private uint _localSentOut;

        [UdonSynced] [NonSerialized] public byte[] syncBuffer = new byte[0];

        private DataList syncBufferBuilder = new DataList();

        private DataList _dataQueue = new DataList();
        private bool _queueRunning;
        private float _lastSendTime;
        private NetworkInterface _targetScript;

        #endregion


        internal void SetupCaller()
        {
            Log("Setting up Caller");
            _methodInfos = networkManager.methodInfos;
            _methodInfosKeys = networkManager.methodInfosKeys;
        }
        
        private void OnEnable()
        {
            if (_startRun) return;
            _startRun = true;
            _debug = networkManager.debug;
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
        
        public override void OnDeserialization(DeserializationResult result)
        {
            if (result.isFromStorage)
            {
                return;
            }
            if (syncBuffer.Length == 0) 
            {
                if (_debug) Log($"Empty buffer, (Likely caused by serialization after playerLeft)");
                return;
            }

            if (networkManager.networkingActive)
            {
                HandleDeserialization(Networking.GetOwner(gameObject), syncBuffer);
            }
            else
            {
                networkManager.HandlePaused(Networking.GetOwner(gameObject), syncBuffer);
            }
        }

        public void HandleDeserialization(VRCPlayerApi sender, byte[] dataBuffer)
        { 
            syncBuffer = dataBuffer;
            int startOffset = 0;

            while (true)
            {
                startOffset += syncBuffer.ReadVariableInt(startOffset, out uint byteLength);
                
                int preReadOffset = startOffset;
                preReadOffset += syncBuffer.ReadVariableInt(preReadOffset, out uint length);
                preReadOffset += syncBuffer.ReadVariableInt(preReadOffset, out uint playerTarget);
                
                
                bool shouldDeserialize = true;
                if (playerTarget > 100)
                {
                    if (playerTarget - 100 != Networking.LocalPlayer.playerId)
                    {
                        if (_debug)
                            Log(
                                $"Ignoring deserialization, not my player id: {playerTarget - 100} - {Networking.LocalPlayer.playerId}");
                        shouldDeserialize = false;
                    }
                }
                else
                {
                    if (playerTarget >= 8) 
                    {
                        SyncChannel channel = (SyncChannel) playerTarget;
                        if (channel != networkManager.syncChannel)
                        {
                            if (_debug) Log($"Ignoring deserialization, not in channel");
                            shouldDeserialize = false;
                        }
                    } 
                    else
                    {
                        var target = (SyncTarget) playerTarget;
                        if (target == SyncTarget.Master && !Networking.IsMaster)
                        {
                            if (_debug) Log($"Ignoring deserialization, not master");
                            shouldDeserialize = false;
                        }

                        if (target == SyncTarget.NonMaster && Networking.IsMaster)
                        {
                            if (_debug) Log($"Ignoring deserialization, not non master");
                            shouldDeserialize = false;
                        }
                    }
                }
                
                uint methodTarget = 0;
                if (shouldDeserialize)
                {
                    preReadOffset += syncBuffer.ReadVariableInt(preReadOffset, out methodTarget);
                    if (_debug)
                    {
                        Log($"Deserialization - {methodTarget} - Size {syncBuffer.Length}");
                    }
                }
                
                int sIndex = -1;
                if (shouldDeserialize)
                {
                    preReadOffset += syncBuffer.ReadVariableInt(preReadOffset, out uint scriptTarget);
                    syncBuffer.ReadVariableInt(preReadOffset, out uint sentOutMethods);
                    if (_localSentOut >= sentOutMethods && _localSentOut != 0)
                    {
                        if (_debug)
                            LogWarning(
                                $"Ignoring deserialization, already sent out Local: {_localSentOut} - Global: {sentOutMethods}");
                        _localSentOut = sentOutMethods;
                        shouldDeserialize = false;
                    }

                    _localSentOut = sentOutMethods;
                    sIndex = Array.IndexOf(sceneInterfacesIds, scriptTarget);
                    if (sIndex == -1)
                    {
                        LogError("Script target not found unable to receive and process data");
                        shouldDeserialize = false;
                    }
                }
                
                
                //Read data
                if (shouldDeserialize)
                {
                    startOffset = ReceiveData(startOffset); //Convert data from buffer to parameters
                    SendUdonMethod(sceneInterfaces[sIndex], (int) methodTarget, sender); //Send method to target script
                }
                else
                {
                    startOffset += (int) byteLength;
                }
                
                
                //Check if there is more data
                if (startOffset >= syncBuffer.Length - 1) break;
            }
        }
        
        private void SendUdonMethod(NetworkInterface target, int methodTarget, VRCPlayerApi sender)
        {
            _targetScript = target;
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
            
            _targetScript.eventSenderPlayer = sender;
            _targetScript.SendCustomEvent(methodInfo["methodName"].String);
        }


        internal void _PrepareSend(uint intTarget, string method, uint scriptTarget, DataToken[] data)
        {
            SyncTarget target = SyncTarget.All;
            SyncChannel syncChannel = (SyncChannel)(-1);
            if (intTarget <= 100)
            {
                if (intTarget < 8)
                    target = (SyncTarget)intTarget;
                else {
                    target = (SyncTarget)99;
                    syncChannel = (SyncChannel)intTarget;
                }
            }
            else
            {
                target = (SyncTarget) (-1);
            }

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

            uint methodIdUint = (uint) methodId;

            if (_debug)
            {
                Log($"Preparing Send - {method} - {methodIdUint} - {scriptTarget} - {sIndex} - {target}");
            }

            var byteList = SendData(methodIdUint, scriptTarget, intTarget, data);
            if (byteList == null)
            {
                LogError($"Failed to send data - {method} - {methodIdUint} - {scriptTarget} - {sIndex} - {target}");
                return;
            }

            //Limit sending method every 0.33 seconds - 3 times per second 
            if (Time.realtimeSinceStartup - _lastSendTime < 0.33f)
            {
                _dataQueue.Add(byteList);
                if (!_queueRunning)
                {
                    _queueRunning = true;
                    SendCustomEventDelayedSeconds(nameof(_SendQueue),
                        0.33f - (Time.realtimeSinceStartup - _lastSendTime));
                }
            }
            else
            {
                _lastSendTime = Time.realtimeSinceStartup;
                
                //Offload to Queue to handle sending multiple methods at once
                _dataQueue.Add(byteList);
                
                if (!_queueRunning)
                {
                    _queueRunning = true;
                    SendCustomEventDelayedSeconds(nameof(_SendQueue), 
                        0.33f - (Time.realtimeSinceStartup - _lastSendTime)
                        );
                }
                
            }

            if (target == SyncTarget.All ||
                syncChannel == networkManager.syncChannel ||
                target == SyncTarget.Local ||
                (target == SyncTarget.Master && Networking.IsMaster) ||
                (target == SyncTarget.NonMaster && !Networking.IsMaster) ||
                (target == (SyncTarget) (-1) && Networking.LocalPlayer.playerId == intTarget - 100))
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


        public void _SendQueue()
        {
            if (_dataQueue.Count == 0)
            {
                _queueRunning = false;
                return;
            }

            Log($"Handling queue {_dataQueue.Count}");
            
            
            int requiredSize = 0;
            for (int i = 0; i < _dataQueue.Count; i++)
            {
                requiredSize += _dataQueue[i].DataList.Count;
            }
            
            if (syncBuffer.Length != requiredSize)
            {
                syncBuffer = new byte[requiredSize];
            }
            
            int offset = 0;
            for (int i = 0; i < _dataQueue.Count; i++)
            {
                var data = _dataQueue[i].DataList;
                for (int j = 0; j < data.Count; j++)
                {
                    syncBuffer[offset] = data[j].Byte;
                    offset++;
                }
                data.Clear();
            }
            
            _dataQueue.Clear();

            _lastSendTime = Time.realtimeSinceStartup;
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
            RequestSerialization();
            SendCustomEventDelayedSeconds(nameof(_SendQueue), 0.33f);
        }



        private void SetUdonVariable(string variable, DataToken token)
        {
            var type = token.TokenType;
            int typeId = -1;
            if (type != TokenType.Reference)
            {
                typeId = Array.IndexOf(_tokenMap, type);
            }
            else
            {
                var reference = token.Reference;
                if (Utilities.IsValid(reference))
                {
                    typeId = Array.IndexOf(_typeMap, reference.GetType());
                }
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