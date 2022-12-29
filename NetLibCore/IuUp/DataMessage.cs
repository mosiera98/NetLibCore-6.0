using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetLib.IuUp
{
    public class DataMessage:IuUpMessage
    {
        // 2 Bit
        public byte FQC;
        // 6 Bit
        public byte RFCI;
        // 6 Bit
        public byte HeaderCRC;
        // 10 Bit
        public byte[] PayloadCrc;
        public byte[] PayloadData;
        // 4 Bit
        public byte FrameNumber;

        // 4.75/ 5.15 / 5.9 / 6.7 / 7.4 / 7.95 / 10.2 / 12.2  kbit/s 
        private readonly int[] if2FrameBytes = new[] { 13, 14, 16, 18, 19, 21, 26, 31 };
        public int IFtype = 8;
        // 1 Byte indicates AMR Frame Header
        public byte AmrFrameHeader;

        public DataMessage()
        {
            PduType = PduType.Data;
        }

        public void Parse(BinaryReader reader)
        {
            byte readByte = reader.ReadByte();
            FrameNumber = (byte)(readByte & 0x0F);
            
            readByte = reader.ReadByte();
            FQC = (byte)(readByte & 0xC0);
            RFCI = (byte)(readByte & 0x3f);
            
            readByte = reader.ReadByte();
            HeaderCRC = (byte)(readByte >> 2);
            PayloadCrc = new[]{(byte)(readByte & 0x03), reader.ReadByte()};

            var payload = new List<byte>();
            while (!reader.IsEos())
                payload.Add(reader.ReadByte());
            PayloadData = payload.ToArray();

            GetAmrHeader();
        }

        public byte[] DecodeAsAmr()
        {
            var result = new List<byte> {AmrFrameHeader};
            result.AddRange(PayloadData);
            return result.ToArray();
        }

        public byte[] ToBytes()
        {
            var result = new List<byte>
                         {
                             ((byte) PduType).ToBitArray().Concat(FrameNumber.ToBitArray(), 4, 4).ToBytes()[0],
                             FQC.ToBitArray().Concat(RFCI.ToBitArray(), 2, 6).ToBytes()[0], 
                             HeaderCRC.ToBitArray().Concat(PayloadCrc[0].ToBitArray(), 6, 2).ToBytes()[0], PayloadCrc[1]
                         };

            result.AddRange(PayloadData);
            return result.ToArray();
        }

        private void GetAmrHeader()
        {
            for (int k = 0; k < if2FrameBytes.Length; k++)
            {
                if (PayloadData.Length == if2FrameBytes[k])
                {
                    IFtype = k;
                    break;
                }
            }

            if (IFtype == 8)
                return;
            AmrFrameHeader = (byte)((IFtype << 3) | 0x04);
        }

        public static IEnumerable<DataMessage> ExtractIuupDataFromAmr12K(string amrFileName)
        {
            var amrFileWithoutHeader = File.ReadAllBytes(amrFileName).Skip(6).ToArray();
            var reader = new BinaryReader(new MemoryStream(amrFileWithoutHeader));
            var buffer = new byte[32];
            var result = new List<DataMessage>();
            int frameNumber = 0;
            var readCount = reader.Read(buffer, 0, buffer.Length);
            while (readCount != 0)
            {
                var data = new DataMessage
                           {
                               FrameNumber = (byte) frameNumber++, 
                               FQC = 0x00, 
                               AmrFrameHeader = buffer[0], 
                               HeaderCRC = 0x38, 
                               IFtype = 7, 
                               PayloadCrc = new byte[] {0x00, 0x19}, 
                               PduType = 0x00, 
                               RFCI = 0x00, 
                               PayloadData = buffer.Skip(1).ToArray()
                           };
                result.Add(data);
                readCount = reader.Read(buffer, 0, buffer.Length);
            }
            return result;
        } 
    }
}