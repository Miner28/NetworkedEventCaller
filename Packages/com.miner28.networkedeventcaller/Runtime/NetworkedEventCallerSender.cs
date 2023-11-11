using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Miner28.UdonUtils.Network
{
    public partial class NetworkedEventCaller : UdonSharpBehaviour
    {
        private DataList SendData(uint method, uint scriptTarget, uint target, DataToken[] data)
        {
            var sIndex = Array.IndexOf(sceneInterfacesIds, scriptTarget);
            if (sIndex == -1)
            {
                LogError($"Invalid script target: {scriptTarget} - {method} can't send method if target is invalid");
                return null;
            }

            syncBufferBuilder = new DataList();

            _localSentOut++;
            syncBufferBuilder.AddVariableInt(Convert.ToUInt32(data.Length));
            syncBufferBuilder.AddVariableInt(target);
            syncBufferBuilder.AddVariableInt(method);
            syncBufferBuilder.AddVariableInt(scriptTarget);
            syncBufferBuilder.AddVariableInt(_localSentOut);


            for (_iter = 0; _iter < data.Length; _iter++)
            {
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
                }

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


                if (typeId < (int) Types.Int16 || typeId > (int) Types.UInt64)
                {
                    syncBufferBuilder.Add(Convert.ToByte(typeId));
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

                        if (_int16Value == 0)
                        {
                            syncBufferBuilder.Add(Int16V);
                        }
                        else if (_int16Value > 0)
                        {
                            if (_int16Value < _0xFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int16V + 1));
                                syncBufferBuilder.Add(Convert.ToByte(_int16Value));
                            }
                            else
                            {
                                syncBufferBuilder.Add(Convert.ToByte(UInt16V + 2));
                                syncBufferBuilder.Add(Convert.ToByte((_int16Value >> Bit8) & _0xFF));
                                syncBufferBuilder.Add(Convert.ToByte(_int16Value & _0xFF));
                            }
                        }
                        else
                        {
                            _int32TMP = -_int16Value;

                            if (_int16Value < _0xFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int16VN + 1));
                                syncBufferBuilder.Add(Convert.ToByte(_int32TMP));
                            }
                            else
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int16VN + 2));
                                syncBufferBuilder.Add(Convert.ToByte((_int32TMP >> Bit8) & _0xFF));
                                syncBufferBuilder.Add(Convert.ToByte(_int32TMP & _0xFF));
                            }
                        }

                        break;
                    case Types.UInt16:
                        _uint16Value = data[_iter].UShort;

                        if (_uint16Value == 0)
                        {
                            syncBufferBuilder.Add(UInt16V);
                        }
                        else if (_uint16Value < _0xFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt16V + 1));
                            syncBufferBuilder.Add((byte) _uint16Value);
                        }
                        else
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt16V + 2));
                            syncBufferBuilder.Add((byte) ((_uint16Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint16Value & _0xFF));
                        }

                        break;
                    case Types.Int32:
                        _int32TMP = data[_iter].Int;

                        if (_int32TMP == 0)
                        {
                            syncBufferBuilder.Add(Int32V);
                        }
                        else if (_int32TMP > 0)
                        {
                            if (_int32TMP < _0xFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int32V + 1));
                                syncBufferBuilder.Add((byte) _int32TMP);
                            }
                            else if (_int32TMP < _0xFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int32V + 2));
                                syncBufferBuilder.Add((byte) ((_int32TMP >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int32TMP & _0xFF));
                            }
                            else if (_int32TMP < 0xFFFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int32V + 3));
                                syncBufferBuilder.Add((byte) ((_int32TMP >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int32TMP >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int32TMP & _0xFF));
                            }
                            else
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int32V + 4));
                                syncBufferBuilder.Add((byte) ((_int32TMP >> Bit24) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int32TMP >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int32TMP >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int32TMP & _0xFF));
                            }
                        }
                        else
                        {
                            _int32TMP = -_int32TMP;

                            if (_int32TMP < _0xFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int32VN + 1));
                                syncBufferBuilder.Add((byte) _int32TMP);
                            }
                            else if (_int32TMP < _0xFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int32VN + 2));
                                syncBufferBuilder.Add((byte) ((_int32TMP >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int32TMP & _0xFF));
                            }
                            else if (_int32TMP < 0xFFFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int32VN + 3));
                                syncBufferBuilder.Add((byte) ((_int32TMP >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int32TMP >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int32TMP & _0xFF));
                            }
                            else
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int32VN + 4));
                                syncBufferBuilder.Add((byte) ((_int32TMP >> Bit24) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int32TMP >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int32TMP >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int32TMP & _0xFF));
                            }
                        }


                        break;
                    case Types.UInt32:
                        _uint32Value = data[_iter].UInt;

                        if (_uint32Value == 0)
                        {
                            syncBufferBuilder.Add(UInt32V);
                        }
                        else if (_uint32Value < _0xFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt32V + 1));
                            syncBufferBuilder.Add((byte) _uint32Value);
                        }
                        else if (_uint32Value < _0xFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt32V + 2));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint32Value & _0xFF));
                        }
                        else if (_uint32Value < 0xFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt32V + 3));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint32Value & _0xFF));
                        }
                        else
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt32V + 4));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint32Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint32Value & _0xFF));
                        }

                        break;
                    case Types.Int64:
                        _int64Value = data[_iter].Long;

                        if (_int64Value == 0)
                        {
                            syncBufferBuilder.Add(Int64V);
                        }
                        else if (_int64Value > 0)
                        {
                            if (_int64Value < _0xFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64V + 1));
                                syncBufferBuilder.Add((byte) _int64Value);
                            }
                            else if (_int64Value < _0xFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64V + 2));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                            }
                            else if (_int64Value < 0xFFFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64V + 3));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                            }
                            else if (_int64Value < 0xFFFFFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64V + 4));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                            }
                            else if (_int64Value < 0xFFFFFFFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64V + 5));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit32) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                            }
                            else if (_int64Value < 0xFFFFFFFFFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64V + 6));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit40) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit32) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                            }
                            else if (_int64Value < 0xFFFFFFFFFFFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64V + 7));
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
                                syncBufferBuilder.Add(Convert.ToByte(Int64V + 8));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit56) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit48) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit40) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit32) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                            }
                        }
                        else
                        {
                            _int64Value = -_int64Value;

                            if (_int64Value < _0xFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64VN + 1));
                                syncBufferBuilder.Add((byte) _int64Value);
                            }
                            else if (_int64Value < _0xFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64VN + 2));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                            }
                            else if (_int64Value < 0xFFFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64VN + 3));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                            }
                            else if (_int64Value < 0xFFFFFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64VN + 4));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                            }
                            else if (_int64Value < 0xFFFFFFFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64VN + 5));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit32) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                            }
                            else if (_int64Value < 0xFFFFFFFFFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64VN + 6));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit40) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit32) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                            }
                            else if (_int64Value < 0xFFFFFFFFFFFFFF)
                            {
                                syncBufferBuilder.Add(Convert.ToByte(Int64VN + 7));
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
                                syncBufferBuilder.Add(Convert.ToByte(Int64VN + 8));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit56) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit48) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit40) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit32) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit24) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit16) & _0xFF));
                                syncBufferBuilder.Add((byte) ((_int64Value >> Bit8) & _0xFF));
                                syncBufferBuilder.Add((byte) (_int64Value & _0xFF));
                            }
                        }

                        break;
                    case Types.UInt64:
                        _uint64Value = data[_iter].ULong;

                        if (_uint64Value == 0)
                        {
                            syncBufferBuilder.Add(UInt64V);
                        }
                        else if (_uint64Value < _0xFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V + 1));
                            syncBufferBuilder.Add((byte) _uint64Value);
                        }
                        else if (_uint64Value < _0xFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V + 2));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint64Value & _0xFF));
                        }
                        else if (_uint64Value < 0xFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V + 3));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint64Value & _0xFF));
                        }
                        else if (_uint64Value < 0xFFFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V + 4));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint64Value & _0xFF));
                        }
                        else if (_uint64Value < 0xFFFFFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V + 5));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit32) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint64Value & _0xFF));
                        }
                        else if (_uint64Value < 0xFFFFFFFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V + 6));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit40) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit32) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit24) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit16) & _0xFF));
                            syncBufferBuilder.Add((byte) ((_uint64Value >> Bit8) & _0xFF));
                            syncBufferBuilder.Add((byte) (_uint64Value & _0xFF));
                        }
                        else if (_uint64Value < 0xFFFFFFFFFFFFFF)
                        {
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V + 7));
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
                            syncBufferBuilder.Add(Convert.ToByte(UInt64V + 8));
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
                        syncBufferBuilder.AddVariableInt(Convert.ToUInt32(_int32TMP2));


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
                            syncBufferBuilder.AddVariableInt(_vrcPlayerApiA[_int32TMP2] == null
                                ? 0
                                : _vrcPlayerApiA[_int32TMP2].playerId);

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


            if (_debug)
            {
                Log(
                    $"[SyncBuffer] [Build] [End] [Size: {syncBufferBuilder.Count}] [Time: {DateTime.Now:HH:mm:ss.fff}]");
            }


            return syncBufferBuilder;
        }
    }
}