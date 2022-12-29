using System.Collections;
using System.IO;

namespace NetLib
{
    public static class Extensions
    {
        public static bool IsEos(this BinaryReader reader)
        {
            return reader.BaseStream.Position >= reader.BaseStream.Length;
        }

        public static byte[] GetByteValue(this BitArray bitArray, int bitCount, int byteCount, int startIndex)
        {
            var tmpBytes = new byte[byteCount];
            var tmp = new BitArray(bitCount);
            int j = 0;
            for (int i = startIndex; i < (startIndex + bitCount); i++)
                tmp[j++] = bitArray[i];
            tmp.CopyTo(tmpBytes, 0);
            return tmpBytes;
        }

        public static BitArray ToBitArray(this byte aByte)
        {
            var result = new BitArray(8);
            for (int i = 7; i >= 0; i--)
            {
                result[i] = (aByte & 0x80) != 0;
                aByte *= 2;
            }
            return result;
        }

        public static byte[] ToBytes(this BitArray aBitAByte)
        {
            var length = aBitAByte.Count%8 == 0 ? aBitAByte.Count/8 : aBitAByte.Count%8 + 1;
            var result = new byte[length];
            aBitAByte.CopyTo(result, 0);
            return result;
        }

        public static BitArray Or(this BitArray bitArray1, BitArray bitArray2)
        {
            var result = new BitArray(bitArray1.Length);
            for (int i = 0; i < bitArray1.Length; i++)
                result[i] = bitArray1[i] | bitArray2[i];
            return result;
        }

        public static BitArray Concat(this BitArray bitArray1, BitArray bitArray2, int bitArray1Length, int bitArray2Length)
        {
            var length = bitArray1Length + bitArray2Length;
            var result = new BitArray(length);
            for (int i = 0; i < length; i++)
            {
                if (i < bitArray2Length)
                {
                    if(i >= bitArray2.Length)
                        result[i] = false;
                    else
                        result[i] = bitArray2[i];
                }
                else
                {
                    var index = i - bitArray2Length;
                    if (index >= bitArray1.Length)
                        result[i] = false;
                    else
                        result[i] = bitArray1[index];
                }
            }
            return result;
        }
    }
}