using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace NetLib.IuUp
{
    public class RFCI
    {
        // 1 Bit : Last RFCI Indicator
        public bool LRI;
        // 1 Bit
        public bool LI;
        // 6 Bit
        public byte Rfci;
        
        public int RfciNumber;
        // 1 Byte For Each
        public List<byte> FlowsLength;

        public int SubFlows;

        public void Parse(BinaryReader reader, int subFlows)
        {
            SubFlows = subFlows;
            FlowsLength = new List<byte>();
            var bitArray = new BitArray(new[] { reader.ReadByte() });
            Rfci = bitArray.GetByteValue(6, 1, 0)[0];
            LI = bitArray.GetByteValue(1, 1, 6)[0] != 0;
            LRI = bitArray.GetByteValue(1, 1, 7)[0] != 0;
            for (int i = 0; i < subFlows; i++)
                FlowsLength.Add(reader.ReadByte());
        }

        public IEnumerable<byte> ToBytes()
        {
            var result = new List<byte>
                         {
                             (LRI ? (byte) 0x1 : (byte) 0x0).ToBitArray().Concat((LI ? (byte) 0x1 : (byte) 0x0).ToBitArray(), 1, 1).Concat(Rfci.ToBitArray(), 2, 6).ToBytes()[0]
                         };
            result.AddRange(FlowsLength);
            return result.ToArray();
        }
    }
}