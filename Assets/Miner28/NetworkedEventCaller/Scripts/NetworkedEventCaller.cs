using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class NetworkedEventCaller : UdonSharpBehaviour
{
    [HideInInspector] public NetworkReceiver networkReceiver;

    private static void Log(string log)
    {
        Debug.Log($"<color=#FFFF00>[NetCaller]</color> {log}");
    }
    private static void LogWarn(string log)
    {
        Debug.Log($"<color=#FF0000>[NetCaller]</color> <color=#0000FF>{log}</color>");
    }

    #region ArraySyncCaller

    [UdonSynced] private string[] arrayParameters = new string[0];
    [UdonSynced] private Types[] paramTypes = new Types[0];
    [NonSerialized][UdonSynced] public string methodName = "";

    private object[] parameters;
    private byte paramCounter;
    private object[][] queue = new object[0][];
    private float lastSent;
    private bool isQueueRunning;

    private Type[] _types = new []
    {
        typeof(byte),
        typeof(sbyte),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(short),
        typeof(ushort),
        typeof(float),
        typeof(double),
        typeof(string),
        typeof(bool),
        typeof(VRCPlayerApi),
        typeof(Color),
        typeof(Color32),
        typeof(Vector3),
        typeof(Quaternion),
        typeof(byte[]),
        typeof(sbyte[]),
        typeof(int[]),
        typeof(uint[]),
        typeof(long[]),
        typeof(ulong[]),
        typeof(short[]),
        typeof(ushort[]),
        typeof(float[]),
        typeof(double[]),
        typeof(string[]),
        typeof(bool[]),
        typeof(VRCPlayerApi[]),

    };


    public void _SendMethod(SyncTarget target, string method, object[] paramsObj)
    {
        var arrayParametersLocal = new string[paramsObj.Length];
        var paramTypesLocal = new Types[paramsObj.Length];

        for (int i = 0; i < paramsObj.Length; i++)
        {
            var type = paramsObj[i].GetType();

            var typeId = Array.IndexOf(_types, type);
            if (typeId == -1) return;
            
            var enumType = (Types) typeId;

            paramTypesLocal[i] = enumType;
            
            string temp;
            switch (enumType)
            {
                case Types.Byte:
                    arrayParametersLocal[i] = paramsObj[i].ToString();
                    break;
                case Types.SByte:
                    arrayParametersLocal[i] = paramsObj[i].ToString();
                    break;
                case Types.Int32:
                    arrayParametersLocal[i] = paramsObj[i].ToString();
                    break;
                case Types.UInt32:
                    arrayParametersLocal[i] = paramsObj[i].ToString();
                    break;
                case Types.Int64:
                    arrayParametersLocal[i] = paramsObj[i].ToString();
                    break;
                case Types.UInt64:
                    arrayParametersLocal[i] = paramsObj[i].ToString();
                    break;
                case Types.Int16:
                    arrayParametersLocal[i] = paramsObj[i].ToString();
                    break;
                case Types.UInt16:
                    arrayParametersLocal[i] = paramsObj[i].ToString();
                    break;
                case Types.Single:
                    arrayParametersLocal[i] = paramsObj[i].ToString();
                    break;
                case Types.Double:
                    arrayParametersLocal[i] = paramsObj[i].ToString();
                    break;
                case Types.String:
                    arrayParametersLocal[i] = paramsObj[i].ToString();
                    break;
                case Types.Boolean:
                    arrayParametersLocal[i] = (bool) paramsObj[i] ? "T" : "F";
                    break;
                case Types.VRCPlayerApi:
                {
                    arrayParametersLocal[i] = ((VRCPlayerApi) paramsObj[i]).playerId.ToString();
                    break;
                }
                case Types.Color:
                {
                    Color color = (Color) paramsObj[i];
                    arrayParametersLocal[i] = $"{color.r}{color.g}{color.b}{color.a}";
                    break;
                }
                case Types.Vector3:
                {
                    Vector3 vector = (Vector3) paramsObj[i];
                    arrayParametersLocal[i] = $"{vector.x}{vector.y}{vector.z}";
                    break;
                }
                case Types.Quaternion:
                {
                    Quaternion quaternion = (Quaternion) paramsObj[i];
                    arrayParametersLocal[i] = $"{quaternion.x}{quaternion.y}{quaternion.z}{quaternion.w}";
                    break;
                }
                case Types.Color32:
                {
                    Color32 color32 = (Color32) paramsObj[i];
                    arrayParametersLocal[i] = $"{color32.r}{color32.g}{color32.b}{color32.a}";
                    break;
                }
                case Types.ByteA:
                {
                    temp = "";
                    var param = (byte[]) paramsObj[i];
                    if (param.Length != 0)
                    {
                        foreach (var b in param)
                        {
                            temp += $"{b}";
                        }

                        temp = temp.Remove(temp.Length - 1);
                    }

                    arrayParametersLocal[i] = temp;
                    break;
                }
                case Types.SByteA:
                {
                    temp = "";
                    var param = (sbyte[]) paramsObj[i];
                    if (param.Length != 0)
                    {
                        foreach (var b in param)
                        {
                            temp += $"{b}";
                        }

                        temp = temp.Remove(temp.Length - 1);
                    }

                    arrayParametersLocal[i] = temp;
                    break;
                }
                case Types.Int32A:
                {
                    temp = "";
                    var param = (int[]) paramsObj[i];
                    if (param.Length != 0)
                    {
                        foreach (var b in param)
                        {
                            temp += $"{b}";
                        }

                        temp = temp.Remove(temp.Length - 1);
                    }

                    arrayParametersLocal[i] = temp;
                    break;
                }
                case Types.UInt32A:
                {
                    temp = "";
                    var param = (uint[]) paramsObj[i];
                    if (param.Length != 0)
                    {
                        foreach (var b in param)
                        {
                            temp += $"{b}";
                        }

                        temp = temp.Remove(temp.Length - 1);
                    }

                    arrayParametersLocal[i] = temp;
                    break;
                }
                case Types.Int64A:
                {
                    temp = "";
                    var param = (long[]) paramsObj[i];
                    if (param.Length != 0)
                    {
                        foreach (var b in param)
                        {
                            temp += $"{b}";
                        }

                        temp = temp.Remove(temp.Length - 1);
                    }

                    arrayParametersLocal[i] = temp;
                    break;
                }
                case Types.UInt64A:
                {
                    temp = "";
                    var param = (ulong[]) paramsObj[i];
                    if (param.Length != 0)
                    {
                        foreach (var b in param)
                        {
                            temp += $"{b}";
                        }

                        temp = temp.Remove(temp.Length - 1);
                    }

                    arrayParametersLocal[i] = temp;
                    break;
                }
                case Types.Int16A:
                {
                    temp = "";
                    var param = (short[]) paramsObj[i];
                    if (param.Length != 0)
                    {
                        foreach (var b in param)
                        {
                            temp += $"{b}";
                        }

                        temp = temp.Remove(temp.Length - 1);
                    }

                    arrayParametersLocal[i] = temp;
                    break;
                }
                case Types.UInt16A:
                {
                    temp = "";
                    var param = (ushort[]) paramsObj[i];
                    if (param.Length != 0)
                    {
                        foreach (var b in param)
                        {
                            temp += $"{b}";
                        }

                        temp = temp.Remove(temp.Length - 1);
                    }

                    arrayParametersLocal[i] = temp;
                    break;
                }
                case Types.SingleA:
                {
                    temp = "";
                    var param = (float[]) paramsObj[i];
                    if (param.Length != 0)
                    {
                        foreach (var b in param)
                        {
                            temp += $"{b}";
                        }

                        temp = temp.Remove(temp.Length - 1);
                    }

                    arrayParametersLocal[i] = temp;
                    break;
                }
                case Types.DoubleA:
                {
                    temp = "";
                    var param = (double[]) paramsObj[i];
                    if (param.Length != 0)
                    {
                        foreach (var b in param)
                        {
                            temp += $"{b}";
                        }

                        temp = temp.Remove(temp.Length - 1);
                    }

                    arrayParametersLocal[i] = temp;
                    break;
                }
                case Types.StringA:
                {
                    temp = "";
                    var param = (string[]) paramsObj[i];
                    if (param.Length != 0)
                    {
                        foreach (var b in param)
                        {
                            temp += $"{b}";
                        }

                        temp = temp.Remove(temp.Length - 1);
                    }

                    arrayParametersLocal[i] = temp;
                    break;
                }
                case Types.BooleanA:
                {
                    temp = "";
                    var param = (bool[]) paramsObj[i];
                    if (param.Length != 0)
                    {
                        foreach (var b in param)
                        {
                            temp += $"{(b ? 'T' : 'F')}";
                        }

                        temp = temp.Remove(temp.Length - 1);
                    }

                    arrayParametersLocal[i] = temp;
                    break;
                }
                case Types.VRCPlayerApiA:
                {
                    temp = "";
                    var param = (VRCPlayerApi[]) paramsObj[i];
                    if (param.Length != 0)
                    {
                        foreach (var b in param)
                        {
                            temp += $"{b.playerId.ToString()}";
                        }

                        temp = temp.Remove(temp.Length - 1);
                    }

                    arrayParametersLocal[i] = temp;
                    break;
                }
            }
        }

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
    }

    private void _MethodReceived(string method, string[] stringParams, Types[] typesArray)
    {
        if (method == "") return;

        paramCounter = 0;
        parameters = new object[stringParams.Length];

        for (int i = 0; i < stringParams.Length; i++)
        {
            SetType(typesArray[i], stringParams[i]);
        }

        networkReceiver.parameters = parameters;
        networkReceiver.SendCustomEvent(method);
    }

    private void SetType(Types type, string value)
    {
        string[] split;
        switch (type)
        {
            case Types.Byte:
                parameters[paramCounter] = Convert.ToByte(value);
                break;
            case Types.SByte:
                parameters[paramCounter] = Convert.ToSByte(value);
                break;
            case Types.Int32:
                parameters[paramCounter] = Convert.ToInt32(value);
                break;
            case Types.UInt32:
                parameters[paramCounter] = Convert.ToUInt32(value);
                break;
            case Types.Int64:
                parameters[paramCounter] = Convert.ToInt64(value);
                break;
            case Types.UInt64:
                parameters[paramCounter] = Convert.ToUInt64(value);
                break;
            case Types.Int16:
                parameters[paramCounter] = Convert.ToInt16(value);
                break;
            case Types.UInt16:
                parameters[paramCounter] = Convert.ToUInt16(value);
                break;
            case Types.Single:
                parameters[paramCounter] = Convert.ToSingle(value);
                break;
            case Types.Double:
                parameters[paramCounter] = Convert.ToDouble(value);
                break;
            case Types.String:
                parameters[paramCounter] = value;
                break;
            case Types.Boolean:
                parameters[paramCounter] = value == "T";
                break;
            case Types.VRCPlayerApi:
                parameters[paramCounter] = VRCPlayerApi.GetPlayerById(Convert.ToInt32(value));
                break;
            case Types.Color:
                split = value.Split('');
                parameters[paramCounter] = new Color(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]),
                    Convert.ToSingle(split[2]), Convert.ToSingle(split[3]));
                break;
            case Types.Color32:
                split = value.Split('');
                parameters[paramCounter] = new Color32(Convert.ToByte(split[0]), Convert.ToByte(split[1]),
                    Convert.ToByte(split[2]), Convert.ToByte(split[3]));
                break;
            case Types.Vector3:
                split = value.Split('');
                parameters[paramCounter] = new Vector3(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]),
                    Convert.ToSingle(split[2]));
                break;
            case Types.Quaternion:
                split = value.Split('');
                parameters[paramCounter] = new Quaternion(Convert.ToSingle(split[0]), Convert.ToSingle(split[1]),
                    Convert.ToSingle(split[2]), Convert.ToSingle(split[3]));
                break;
            case Types.ByteA:
                split = value.Split('');
                byte[] byteOut = new byte[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    byteOut[i] = Convert.ToByte(split[i]);
                }

                parameters[paramCounter] = byteOut;
                break;
            case Types.SByteA:
                split = value.Split('');
                sbyte[] sbyteOut = new sbyte[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    sbyteOut[i] = Convert.ToSByte(split[i]);
                }

                parameters[paramCounter] = sbyteOut;
                break;
            case Types.Int32A:
                split = value.Split('');
                int[] intOut = new int[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    intOut[i] = Convert.ToInt32(split[i]);
                }

                parameters[paramCounter] = intOut;
                break;
            case Types.UInt32A:
                split = value.Split('');
                uint[] uintOut = new uint[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    uintOut[i] = Convert.ToUInt32(split[i]);
                }

                parameters[paramCounter] = uintOut;
                break;
            case Types.Int64A:
                split = value.Split('');
                long[] longOut = new long[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    longOut[i] = Convert.ToInt64(split[i]);
                }

                parameters[paramCounter] = longOut;
                break;
            case Types.UInt64A:
                split = value.Split('');
                ulong[] ulongOut = new ulong[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    ulongOut[i] = Convert.ToUInt64(split[i]);
                }

                parameters[paramCounter] = ulongOut;
                break;
            case Types.Int16A:
                split = value.Split('');
                short[] shortOut = new short[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    shortOut[i] = Convert.ToInt16(split[i]);
                }

                parameters[paramCounter] = shortOut;
                break;
            case Types.UInt16A:
                split = value.Split('');
                ushort[] ushortOut = new ushort[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    ushortOut[i] = Convert.ToUInt16(split[i]);
                }

                parameters[paramCounter] = ushortOut;
                break;
            case Types.SingleA:
                split = value.Split('');
                float[] floatOut = new float[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    floatOut[i] = Convert.ToSingle(split[i]);
                }

                parameters[paramCounter] = floatOut;
                break;
            case Types.DoubleA:
                split = value.Split('');
                double[] doubleOut = new Double[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    doubleOut[i] = Convert.ToDouble(split[i]);
                }

                parameters[paramCounter] = doubleOut;
                break;
            case Types.StringA:
                split = value.Split('');
                parameters[paramCounter] = split;
                break;
            case Types.BooleanA:
                split = value.Split('');
                bool[] boolOut = new bool[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    boolOut[i] = split[i] == "T";
                }

                parameters[paramCounter] = boolOut;
                break;
            case Types.VRCPlayerApiA:
                split = value.Split('');
                VRCPlayerApi[] apiOut = new VRCPlayerApi[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    apiOut[i] = VRCPlayerApi.GetPlayerById(Convert.ToInt32(split[i]));
                }

                parameters[paramCounter] = apiOut;
                break;
        }

        paramCounter++;
    }


    //TODO [UdonSynced] private int largestKnown;
    private int localSentOut;
    [UdonSynced] private int sentOutMethods;

    public override void OnPreSerialization()
    {
        Log(
            $"PreSerialization - {Networking.GetOwner(gameObject).displayName} - {methodName} - {localSentOut} - {sentOutMethods}");
    }


    public override void OnDeserialization()
    {
        Log(
            $"Deserialization - {Networking.GetOwner(gameObject).displayName} - {methodName} - {localSentOut} - {sentOutMethods}");

        if (localSentOut >= sentOutMethods && localSentOut != 0)
        {
            LogWarn("Ignoring - Local more than global");
            localSentOut = sentOutMethods;
            return;
        }

        localSentOut = sentOutMethods;

        /* TODO
        if (largestKnown < Networking.LocalPlayer.playerId)
        {
            Log("LargestKnown less than My ID");
            return;
        }*/

        _MethodReceived(methodName, arrayParameters, paramTypes);
    }

    public override void OnPostSerialization(SerializationResult result)
    {
        if (!result.success)
        {
            LogWarn("<color=#FF0000>FAILED SERIALIZATION</color>");
        }
    }


    [NonSerialized] private bool startRun;

    private void OnEnable()
    {
        if (startRun) return;
        startRun = true;
        parameters = new object[0];
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

public static class NetCallerExtensions
{
    public static bool Contains<T>(this T[] array, T item) => Array.IndexOf(array, item) != -1;

    public static T[] Add<T>(this T[] array, T item)
    {
        T[] newArray = new T[array.Length + 1];
        Array.Copy(array, newArray, array.Length);
        newArray[array.Length] = item;
        return newArray;
    }

    public static T[] Remove<T>(this T[] array, int index)
    {
        T[] newArray = new T[array.Length - 1];
        Array.Copy(array, newArray, index);
        Array.Copy(array, index + 1, newArray, index, newArray.Length - index);
        return newArray;
    }
}