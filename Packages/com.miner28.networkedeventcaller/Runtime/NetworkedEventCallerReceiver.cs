using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Miner28.UdonUtils.Network
{
    public partial class NetworkedEventCaller : UdonSharpBehaviour
    {
        int ReceiveData(int startIndex)
        {
            _bufferOffset = startIndex;

            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out uint length);
            if (_parameters.Length != length)
            {
                _parameters = new DataToken[length];
            }

            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out uint target);
            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out uint method);
            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out uint scriptTarget);
            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out uint sentOutMethods);

            for (_iter = 0; _iter < _parameters.Length; _iter++)
            {
                string[] chars;

                Types type = Types.Null;
                length = 0;
                byte typeByte = syncBuffer[_bufferOffset++];

                if (typeByte < 60)
                {
                    if (typeByte >= (int) Types.BooleanA && typeByte != (byte) Types.Null)
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
                    else if (typeByte < Int16VN + 10)
                    {
                        length = Convert.ToUInt32(typeByte - Int16VN);
                        type = Types.Int16VN;
                    }
                    else if (typeByte < Int32V + 10)
                    {
                        length = Convert.ToUInt32(typeByte - Int32V);
                        type = Types.Int32V;
                    }
                    else if (typeByte < Int32VN + 10)
                    {
                        length = Convert.ToUInt32(typeByte - Int32VN);
                        type = Types.Int32VN;
                    }
                    else if (typeByte < Int64V + 10)
                    {
                        length = Convert.ToUInt32(typeByte - Int64V);
                        type = Types.Int64V;
                    }
                    else if (typeByte < Int64VN + 10)
                    {
                        length = Convert.ToUInt32(typeByte - Int64VN);
                        type = Types.Int64VN;
                    }
                    else if (typeByte < UInt16V + 10)
                    {
                        length = Convert.ToUInt32(typeByte - UInt16V);
                        type = Types.UInt16V;
                    }
                    else if (typeByte < UInt32V + 10)
                    {
                        length = Convert.ToUInt32(typeByte - UInt32V);
                        type = Types.UInt32V;
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
                    case Types.Int16VN:
                        if (length == 0)
                        {
                            _parameters[_iter] = Convert.ToInt16(0);
                            break;
                        }

                        if (length == 1)
                        {
                            _int16Value = Convert.ToInt16(syncBuffer[_bufferOffset]);

                            _bufferOffset++;
                        }

                        if (length == 2)
                        {
                            _int16Value = Convert.ToInt16((syncBuffer[_bufferOffset] << Bit8) |
                                                          syncBuffer[_bufferOffset + 1]);
                            _bufferOffset += 2;
                        }

                        _parameters[_iter] = type == Types.Int16V ? _int16Value : (short) -_int16Value;
                        break;
                    case Types.UInt16V:
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
                        }

                        break;
                    case Types.Int32V:
                    case Types.Int32VN:
                        if (length == 0)
                        {
                            _parameters[_iter] = Convert.ToInt32(0);
                            break;
                        }

                        if (length == 1)
                        {
                            _int32TMP = syncBuffer[_bufferOffset];
                            _bufferOffset++;
                        }

                        if (length == 2)
                        {
                            _int32TMP = (syncBuffer[_bufferOffset] << Bit8) |
                                        syncBuffer[_bufferOffset + 1];
                            _bufferOffset += 2;
                        }

                        if (length == 3)
                        {
                            _int32TMP = (syncBuffer[_bufferOffset] << Bit16) |
                                        (syncBuffer[_bufferOffset + 1] << Bit8) |
                                        syncBuffer[_bufferOffset + 2];
                            _bufferOffset += 3;
                        }

                        if (length == 4)
                        {
                            _int32TMP = (syncBuffer[_bufferOffset] << Bit24) |
                                        (syncBuffer[_bufferOffset + 1] << Bit16) |
                                        (syncBuffer[_bufferOffset + 2] << Bit8) |
                                        syncBuffer[_bufferOffset + 3];
                            _bufferOffset += 4;
                        }

                        _parameters[_iter] = type == Types.Int32V ? _int32TMP : -_int32TMP;
                        break;
                    case Types.UInt32V:
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
                        }

                        break;
                    case Types.Int64V:
                    case Types.Int64VN:
                        if (length == 0)
                        {
                            _parameters[_iter] = Convert.ToInt64(0);
                            break;
                        }

                        if (length == 1)
                        {
                            _int64Value = Convert.ToInt64(syncBuffer[_bufferOffset]);
                            _bufferOffset++;
                        }

                        if (length == 2)
                        {
                            _int64Value = Convert.ToInt64((syncBuffer[_bufferOffset] << Bit8) |
                                                          syncBuffer[_bufferOffset + 1]);
                            _bufferOffset += 2;
                        }

                        if (length == 3)
                        {
                            _int64Value = Convert.ToInt64((syncBuffer[_bufferOffset] << Bit16) |
                                                          (syncBuffer[_bufferOffset + 1] << Bit8) |
                                                          syncBuffer[_bufferOffset + 2]);
                            _bufferOffset += 3;
                        }

                        if (length == 4)
                        {
                            _int64Value = Convert.ToInt64((syncBuffer[_bufferOffset] << Bit24) |
                                                          (syncBuffer[_bufferOffset + 1] << Bit16) |
                                                          (syncBuffer[_bufferOffset + 2] << Bit8) |
                                                          syncBuffer[_bufferOffset + 3]);
                            _bufferOffset += 4;
                        }

                        if (length == 5)
                        {
                            _int64Value = Convert.ToInt64(((long) syncBuffer[_bufferOffset] << Bit32) |
                                                          (uint) (syncBuffer[_bufferOffset + 1] << Bit24) |
                                                          (uint) (syncBuffer[_bufferOffset + 2] << Bit16) |
                                                          (uint) (syncBuffer[_bufferOffset + 3] << Bit8) |
                                                          syncBuffer[_bufferOffset + 4]);
                            _bufferOffset += 5;
                        }

                        if (length == 6)
                        {
                            _int64Value = Convert.ToInt64(((long) syncBuffer[_bufferOffset] << Bit40) |
                                                          ((long) syncBuffer[_bufferOffset + 1] << Bit32) |
                                                          ((uint) syncBuffer[_bufferOffset + 2] << Bit24) |
                                                          ((uint) syncBuffer[_bufferOffset + 3] << Bit16) |
                                                          ((uint) syncBuffer[_bufferOffset + 4] << Bit8) |
                                                          syncBuffer[_bufferOffset + 5]);
                            _bufferOffset += 6;
                        }

                        if (length == 7)
                        {
                            _int64Value = Convert.ToInt64(((long) syncBuffer[_bufferOffset] << Bit48) |
                                                          ((long) syncBuffer[_bufferOffset + 1] << Bit40) |
                                                          ((long) syncBuffer[_bufferOffset + 2] << Bit32) |
                                                          ((uint) syncBuffer[_bufferOffset + 3] << Bit24) |
                                                          ((uint) syncBuffer[_bufferOffset + 4] << Bit16) |
                                                          ((uint) syncBuffer[_bufferOffset + 5] << Bit8) |
                                                          syncBuffer[_bufferOffset + 6]);
                            _bufferOffset += 7;
                        }

                        if (length == 8)
                        {
                            _int64Value = Convert.ToInt64(((long) syncBuffer[_bufferOffset] << Bit56) |
                                                          ((long) syncBuffer[_bufferOffset + 1] << Bit48) |
                                                          ((long) syncBuffer[_bufferOffset + 2] << Bit40) |
                                                          ((long) syncBuffer[_bufferOffset + 3] << Bit32) |
                                                          ((uint) syncBuffer[_bufferOffset + 4] << Bit24) |
                                                          ((uint) syncBuffer[_bufferOffset + 5] << Bit16) |
                                                          ((uint) syncBuffer[_bufferOffset + 6] << Bit8) |
                                                          syncBuffer[_bufferOffset + 7]);
                            _bufferOffset += 8;
                        }

                        _parameters[_iter] = type == Types.Int64V ? _int64Value : -_int64Value;
                        break;
                    case Types.UInt64V:
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
                            _parameters[_iter] = Convert.ToUInt64(((ulong) syncBuffer[_bufferOffset] << Bit32) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 1] << Bit24) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 2] << Bit16) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 3] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 4]);
                            _bufferOffset += 5;
                            break;
                        }

                        if (length == 6)
                        {
                            _parameters[_iter] = Convert.ToUInt64(((ulong)syncBuffer[_bufferOffset] << Bit40) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 1] << Bit32) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 2] << Bit24) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 3] << Bit16) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 4] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 5]);
                            _bufferOffset += 6;
                            break;
                        }

                        if (length == 7)
                        {
                            _parameters[_iter] = Convert.ToUInt64(((ulong)syncBuffer[_bufferOffset] << Bit48) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 1] << Bit40) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 2] << Bit32) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 3] << Bit24) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 4] << Bit16) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 5] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 6]);
                            _bufferOffset += 7;
                            break;
                        }

                        if (length == 8)
                        {
                            _parameters[_iter] = Convert.ToUInt64(((ulong)syncBuffer[_bufferOffset] << Bit56) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 1] << Bit48) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 2] << Bit40) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 3] << Bit32) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 4] << Bit24) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 5] << Bit16) |
                                                                  ((ulong)syncBuffer[_bufferOffset + 6] << Bit8) |
                                                                  syncBuffer[_bufferOffset + 7]);
                            _bufferOffset += 8;
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
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _uint32Value); // String length

                        chars = new string[_uint32Value];

                        _bufferOffset +=
                            syncBuffer.ReadVariableInt(_bufferOffset, out _uint32Value); // String length in bytes

                        _int32TMP4 = Convert.ToInt32(_uint32Value);
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
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP);
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP2);
                        _parameters[_iter] = new DataToken(new Vector2Int(_int32TMP, _int32TMP2));

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
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP);
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP2);
                        _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP3);
                        _parameters[_iter] = new DataToken(new Vector3Int(_int32TMP, _int32TMP2, _int32TMP3));

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
                        _bufferOffset += 4;
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
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int16Value);
                            _int16A[i] = _int16Value;

                        }

                        _parameters[_iter] = new DataToken(_int16A);
                        break;
                    }
                    case Types.UInt16A:
                    {
                        _uint16A = new ushort[length];

                        for (var i = 0; i < length; i++)
                        {
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _uint16Value);
                            _uint16A[i] = _uint16Value;

                        }

                        _parameters[_iter] = new DataToken(_uint16A);
                        break;
                    }
                    case Types.Int32A:
                    {
                        _int32A = new int[length];

                        for (var i = 0; i < length; i++)
                        {
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP);
                            _int32A[i] = _int32TMP;

                        }

                        _parameters[_iter] = new DataToken(_int32A);
                        break;
                    }
                    case Types.UInt32A:
                    {
                        _uint32A = new uint[length];


                        for (var i = 0; i < length; i++)
                        {
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _uint32Value);
                            _uint32A[i] = Convert.ToUInt32(_int32TMP);

                        }

                        _parameters[_iter] = new DataToken(_uint32A);
                        break;
                    }
                    case Types.Int64A:
                    {
                        _int64A = new long[length];

                        for (var i = 0; i < length; i++)
                        {
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int64Value);
                            _int64A[i] = _int64Value;

                        }

                        _parameters[_iter] = new DataToken(_int64A);
                        break;
                    }
                    case Types.UInt64A:
                    {
                        _uint64A = new ulong[length];

                        for (var i = 0; i < length; i++)
                        {
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _uint64Value);
                            _uint64A[i] = _uint64Value;

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
                            _bufferOffset +=
                                syncBuffer.ReadVariableInt(_bufferOffset, out _uint32Value); // String length
                            chars = new string[_uint32Value];

                            _bufferOffset +=
                                syncBuffer.ReadVariableInt(_bufferOffset, out _uint32Value); // String length in bytes

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
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP);
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP2);
                            _vector2IntA[i] = new Vector2Int(_int32TMP, _int32TMP2);

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
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP);
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP2);
                            _bufferOffset += syncBuffer.ReadVariableInt(_bufferOffset, out _int32TMP3);
                            _vector3IntA[i] = new Vector3Int(_int32TMP, _int32TMP2, _int32TMP3);

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

            return _bufferOffset;
        }
    }
}