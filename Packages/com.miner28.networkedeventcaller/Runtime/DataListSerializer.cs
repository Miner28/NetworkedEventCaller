using System;
using UnityEngine;
using VRC.SDK3.Data;

namespace Miner28.UdonUtils
{
    public static class VariableSerializer
    {
        private const int Bit8 = 8;
        private const int Bit16 = 16;
        private const int Bit24 = 24;
        private const int Bit32 = 32;
        private const int Bit40 = 40;
        private const int Bit48 = 48;
        private const int Bit56 = 56;
        private const int Bit64 = 64;


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
            _0xE0 = 0xE0;


        public static void AddVariableInt(this DataList list, uint value)
        {
            if (value < 0x80)
            {
                list.Add((byte) value);
            }
            else if (value < 0x4000)
            {
                list.Add((byte) ((value >> Bit8) | _0x80));
                list.Add((byte) (value & 0xFF));
            }
            else if (value < 0x200000)
            {
                list.Add((byte) ((value >> Bit16) | _0xC0));
                list.Add((byte) ((value >> Bit8) & 0xFF));
                list.Add((byte) (value & 0xFF));
            }
            else if (value < 0x10000000)
            {
                list.Add((byte) ((value >> Bit24) | _0xE0));
                list.Add((byte) ((value >> Bit16) & 0xFF));
                list.Add((byte) ((value >> Bit8) & 0xFF));
                list.Add((byte) (value & 0xFF));
            }
            else
            {
                list.Add((byte) 0xF0);
                list.Add((byte) ((value >> Bit24) & 0xFF));
                list.Add((byte) ((value >> Bit16) & 0xFF));
                list.Add((byte) ((value >> Bit8) & 0xFF));
                list.Add((byte) (value & 0xFF));
            }
        }

        public static void AddVariableInt(this DataList list, ushort value)
        {
            if (value < 0x80)
            {
                list.Add((byte) value);
            }
            else if (value < 0x4000)
            {
                list.Add((byte) ((value >> Bit8) | _0x80));
                list.Add((byte) (value & 0xFF));
            }
            else
            {
                // Handle values greater than or equal to 0x4000 using your existing method
                list.Add((byte) ((value >> Bit16) | _0xC0));
                list.Add((byte) ((value >> Bit8) & 0xFF));
                list.Add((byte) (value & 0xFF));
            }
        }

        public static void AddVariableInt(this DataList list, ulong value)
        {
            if (value < 0x80)
            {
                list.Add((byte) value);
            }
            else if (value < 0x4000)
            {
                list.Add((byte) ((value >> Bit8) | _0x80));
                list.Add((byte) (value & 0xFF));
            }
            else if (value < 0x200000)
            {
                list.Add((byte) ((value >> Bit16) | _0xC0));
                list.Add((byte) ((value >> Bit8) & 0xFF));
                list.Add((byte) (value & 0xFF));
            }
            else if (value < 0x10000000)
            {
                list.Add((byte) ((value >> Bit24) | _0xE0));
                list.Add((byte) ((value >> Bit16) & 0xFF));
                list.Add((byte) ((value >> Bit8) & 0xFF));
                list.Add((byte) (value & 0xFF));
            }
            else if (value < 0x800000000)
            {
                list.Add((byte) 0xF0);
                list.Add((byte) ((value >> Bit24) & 0xFF));
                list.Add((byte) ((value >> Bit16) & 0xFF));
                list.Add((byte) ((value >> Bit8) & 0xFF));
                list.Add((byte) (value & 0xFF));
            }
            else if (value < 0x40000000000)
            {
                list.Add((byte) 0xF8);
                list.Add((byte) ((value >> Bit32) & 0xFF));
                list.Add((byte) ((value >> Bit24) & 0xFF));
                list.Add((byte) ((value >> Bit16) & 0xFF));
                list.Add((byte) ((value >> Bit8) & 0xFF));
                list.Add((byte) (value & 0xFF));
            }
            else if (value < 0x2000000000000)
            {
                list.Add((byte) 0xFC);
                list.Add((byte) ((value >> Bit40) & 0xFF));
                list.Add((byte) ((value >> Bit32) & 0xFF));
                list.Add((byte) ((value >> Bit24) & 0xFF));
                list.Add((byte) ((value >> Bit16) & 0xFF));
                list.Add((byte) ((value >> Bit8) & 0xFF));
                list.Add((byte) (value & 0xFF));
            }
            else if (value < 0x100000000000000)
            {
                list.Add((byte) 0xFE);
                list.Add((byte) ((value >> Bit48) & 0xFF));
                list.Add((byte) ((value >> Bit40) & 0xFF));
                list.Add((byte) ((value >> Bit32) & 0xFF));
                list.Add((byte) ((value >> Bit24) & 0xFF));
                list.Add((byte) ((value >> Bit16) & 0xFF));
                list.Add((byte) ((value >> Bit8) & 0xFF));
                list.Add((byte) (value & 0xFF));
            }
            else if (value < 0x8000000000000000)
            {
                list.Add((byte) 0xFF);
                list.Add((byte) ((value >> Bit56) & 0xFF));
                list.Add((byte) ((value >> Bit48) & 0xFF));
                list.Add((byte) ((value >> Bit40) & 0xFF));
                list.Add((byte) ((value >> Bit32) & 0xFF));
                list.Add((byte) ((value >> Bit24) & 0xFF));
                list.Add((byte) ((value >> Bit16) & 0xFF));
                list.Add((byte) ((value >> Bit8) & 0xFF));
                list.Add((byte) (value & 0xFF));
            }
        }

        public static void AddVariableInt(this DataList list, int value)
        {
            // Apply ZigZag encoding to convert the signed int to an unsigned int
            uint uValue = (uint) ((value << 1) ^ (value >> 31));

            while (uValue >= 0x80)
            {
                list.Add((byte) ((uValue & 0x7F) | 0x80));
                uValue >>= 7;
            }

            list.Add((byte) uValue);
        }

        public static int AddVariableInt(this DataList list, short value)
        {
            ushort uValue = (ushort) ((value << 1) ^ (value >> 15));

            while (uValue >= 0x80)
            {
                list.Add((byte) ((uValue & 0x7F) | 0x80));
                uValue >>= 7;
            }

            list.Add((byte) uValue);
            return list.Count;
        }

        public static int AddVariableInt(this DataList list, long value)
        {
            ulong uValue = (ulong) ((value << 1) ^ (value >> 63));

            while (uValue >= 0x80)
            {
                list.Add((byte) ((uValue & 0x7F) | 0x80));
                uValue >>= 7;
            }

            list.Add((byte) uValue);
            return list.Count;
        }


        public static void AddBytes(this DataList list, float value)
        {
            uint tmp = 0;
            if (float.IsNaN(value))
            {
                tmp = FloatExpMask | FloatFracMask;
            }
            else if (float.IsInfinity(value))
            {
                tmp = FloatExpMask;
                if (float.IsNegativeInfinity(value)) tmp |= FloatSignBit;
            }
            else if (value != 0f)
            {
                if (value < 0f)
                {
                    value = -value;
                    tmp |= FloatSignBit;
                }

                int exp = 0;
                bool normal = true;
                while (value >= 2f)
                {
                    value *= 0.5f;
                    exp++;
                }

                while (value < 1f)
                {
                    if (exp == -126)
                    {
                        normal = false;
                        break;
                    }

                    value *= 2f;
                    exp--;
                }

                if (normal)
                {
                    value -= 1f;
                    exp += 127;
                }
                else exp = 0;

                tmp |= Convert.ToUInt32(exp << 23) & FloatExpMask;
                tmp |= Convert.ToUInt32(value * 0x800000) & FloatFracMask;
            }

            //return WriteUInt32(tmp);
            list.Add((byte) ((tmp >> Bit24) & 255u));
            list.Add((byte) ((tmp >> Bit16) & 255u));
            list.Add((byte) ((tmp >> Bit8) & 255u));
            list.Add((byte) (tmp & 255u));
        }


        public static float ReadFloat(this byte[] array, int startIndex)
        {
            var _uint32Value = ((uint) array[startIndex] << Bit24) |
                               ((uint) array[startIndex + 1] << Bit16) |
                               ((uint) array[startIndex + 2] << Bit8) | array[startIndex + 3];
            if (_uint32Value == 0 || _uint32Value == FloatSignBit)
            {
                return 0f;
            }

            var _exp = (int) ((_uint32Value & FloatExpMask) >> 23);
            var _doubleFracMask = (int) (_uint32Value & FloatFracMask);
            if (_exp == 0xFF)
            {
                if (_doubleFracMask == 0)
                {
                    return (_uint32Value & FloatSignBit) == FloatSignBit
                        ? float.NegativeInfinity
                        : float.PositiveInfinity;
                }

                return float.NaN;
            }


            var _tmpBool = _exp != 0x00;
            if (_tmpBool) _exp -= 127;
            else _exp = -126;

            var _singleValue = _doubleFracMask / 8388608F;
            if (_tmpBool) _singleValue += 1f;

            _singleValue *= Mathf.Pow(2, _exp);

            _tmpBool = (_uint32Value & FloatSignBit) == FloatSignBit;
            if (_tmpBool) _singleValue = -_singleValue;
            return _singleValue;
        }

        public static double ReadDouble(this byte[] array, int startIndex)
        {
            ulong value = ((ulong) array[0] << Bit56) | ((ulong) array[1] << Bit48) |
                          ((ulong) array[2] << Bit40) | ((ulong) array[3] << Bit32) |
                          ((ulong) array[4] << Bit24) | ((ulong) array[5] << Bit16) |
                          ((ulong) array[6] << Bit8) | array[7];

            if (value == 0.0 || value == DoubleSignBit) return 0.0;

            long exp = (long) ((value & DoubleExpMask) >> 52);
            long frac = (long) (value & DoubleFracMask);
            bool negate = (value & DoubleSignBit) == DoubleSignBit;

            if (exp == 0x7FF)
            {
                if (frac == 0) return negate ? double.NegativeInfinity : double.PositiveInfinity;
                return double.NaN;
            }

            bool normal = exp != 0x000;
            if (normal) exp -= 1023;
            else exp = -1022;

            double result = (double) frac / 0x10000000000000UL;
            if (normal) result += 1.0;

            result *= Math.Pow(2, exp);
            if (negate) result = -result;

            return result;
        }


        public static int ReadVariableInt(this byte[] array, int startIndex, out short result)
        {
            uint uValue = 0;
            int bytesRead = 0;
            int shift = 0;

            do
            {
                if (startIndex >= array.Length)
                {
                    result = 0; // Handle the case where the array is too short.
                    return bytesRead;
                }

                byte b = array[startIndex];
                uValue |= (uint)(b & 0x7F) << shift;
                shift += 7;
                bytesRead++;
                startIndex++;
            }
            while ((array[startIndex - 1] & 0x80) != 0);

            // Apply ZigZag decoding to get the original signed short value
            result = (short)((uValue >> 1) ^ -(short)(uValue & 1));

            return bytesRead;
        }


        public static int ReadVariableInt(this byte[] array, int startIndex, out long result)
        {
            ulong uValue = 0;
            int bytesRead = 0;
            int shift = 0;

            do
            {
                if (startIndex >= array.Length)
                {
                    result = 0; // Handle the case where the array is too short.
                    return bytesRead;
                }

                byte b = array[startIndex];
                uValue |= (ulong)(b & 0x7F) << shift;
                shift += 7;
                bytesRead++;
                startIndex++;
            }
            while ((array[startIndex - 1] & 0x80) != 0);

            // Apply ZigZag decoding to get the original signed long value
            result = (long)((uValue >> 1) ^ (ulong) -(long)(uValue & 1));

            return bytesRead;
        }

        public static int ReadVariableInt(this byte[] array, int startIndex, out int result)
        {
            uint uValue = 0;
            int bytesRead = 0;
            int shift = 0;

            do
            {
                if (startIndex >= array.Length)
                {
                    result = 0; // Handle the case where the array is too short.
                    return bytesRead;
                }

                byte b = array[startIndex];
                uValue |= (uint)(b & 0x7F) << shift;
                shift += 7;
                bytesRead++;
                startIndex++;
            }
            while ((array[startIndex - 1] & 0x80) != 0);

            // Apply ZigZag decoding to get the original signed integer value
            result = (int)((uValue >> 1) ^ -(int)(uValue & 1));

            return bytesRead;
        }


        public static int ReadVariableInt(this byte[] array, int startIndex, out uint result)
        {
            byte b = array[startIndex];

            if ((b & _0x80) == 0)
            {
                result = b;
                return 1;
            }
            else if ((b & _0xC0) == _0x80)
            {
                result = (uint) ((b & _0x3F) << 8 | array[startIndex + 1]);
                return 2;
            }
            else if ((b & _0xE0) == _0xC0)
            {
                result = (uint) ((b & _0x1F) << 16 | array[startIndex + 1] << 8 | array[startIndex + 2]);
                return 3;
            }
            else if ((b & _0xF0) == _0xE0)
            {
                result = (uint) ((b & _0x0F) << 24 | array[startIndex + 1] << 16 | array[startIndex + 2] << 8 |
                                 array[startIndex + 3]);
                return 4;
            }
            else if (b == 0xF0)
            {
                result = (uint) (array[startIndex + 1] << 24 | array[startIndex + 2] << 16 |
                                 array[startIndex + 3] << 8 | array[startIndex + 4]);
                return 5;
            }

            result = 0;
            return 0;
        }

        public static int ReadVariableInt(this byte[] array, int startIndex, out ushort result)
        {
            byte b = array[startIndex];

            if ((b & _0x80) == 0)
            {
                result = (ushort) b;
                return 1;
            }
            else if ((b & _0xC0) == _0x80)
            {
                result = (ushort) (((b & _0x3F) << 8) | array[startIndex + 1]);
                return 2;
            }
            else
            {
                result = (ushort) (((b & _0x1F) << 16) | (array[startIndex + 1] << 8) | array[startIndex + 2]);
                return 3;
            }
        }

        public static int ReadVariableInt(this byte[] array, int startIndex, out ulong result)
        {
            byte b = array[startIndex];
            result = 0;

            if ((b & _0x80) == 0)
            {
                result = b;
                return 1;
            }
            else if ((b & _0xC0) == _0x80)
            {
                result = (ulong) ((b & _0x3F) << 8 | array[startIndex + 1]);
                return 2;
            }
            else if ((b & _0xE0) == _0xC0)
            {
                result = (ulong) ((b & _0x1F) << 16 | array[startIndex + 1] << 8 | array[startIndex + 2]);
                return 3;
            }
            else if ((b & _0xF0) == _0xE0)
            {
                result = (ulong) ((b & _0x0F) << 24 | array[startIndex + 1] << 16 | array[startIndex + 2] << 8 |
                                  array[startIndex + 3]);
                return 4;
            }
            else if (b == 0xF0)
            {
                result = (ulong) (array[startIndex + 1] << 24 | array[startIndex + 2] << 16 |
                                  array[startIndex + 3] << 8 | array[startIndex + 4]);
                return 5;
            }
            else if (b == 0xF8)
            {
                result = (ulong) (array[startIndex + 1]) << 32 | ((ulong) array[startIndex + 2]) << 24 |
                         ((ulong) array[startIndex + 3]) << 16 | ((ulong) array[startIndex + 4]) << 8 |
                         ((ulong) array[startIndex + 5]);
                return 6;
            }
            else if (b == 0xFC)
            {
                result = (ulong) (array[startIndex + 1]) << 40 | ((ulong) array[startIndex + 2]) << 32 |
                         ((ulong) array[startIndex + 3]) << 24 | ((ulong) array[startIndex + 4]) << 16 |
                         ((ulong) array[startIndex + 5]) << 8 | ((ulong) array[startIndex + 6]);
                return 7;
            }
            else if (b == 0xFE)
            {
                result = (ulong) (array[startIndex + 1]) << 48 | ((ulong) array[startIndex + 2]) << 40 |
                         ((ulong) array[startIndex + 3]) << 32 | ((ulong) array[startIndex + 4]) << 24 |
                         ((ulong) array[startIndex + 5]) << 16 | ((ulong) array[startIndex + 6]) << 8 |
                         ((ulong) array[startIndex + 7]);
                return 8;
            }
            else if (b == 0xFF)
            {
                result = (ulong) (array[startIndex + 1]) << 56 | ((ulong) array[startIndex + 2]) << 48 |
                         ((ulong) array[startIndex + 3]) << 40 | ((ulong) array[startIndex + 4]) << 32 |
                         ((ulong) array[startIndex + 5]) << 24 | ((ulong) array[startIndex + 6]) << 16 |
                         ((ulong) array[startIndex + 7]) << 8 | ((ulong) array[startIndex + 8]);
                return 9;
            }

            return 0;
        }
    }
}