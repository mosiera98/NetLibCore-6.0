using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace NetLib.IuUp
{
    public class ControlProcedureMessage:IuUpMessage
    {
        // 2 Bit
        public byte AckNack;
        // 4 Bit
        public byte ModeVersion;
        // 4 Bit
        public byte Procedure;
        // 6 Bit
        public byte HeaderCRC;
        // 6 Bit
        public byte[] PayloadCrc;
        // 2 Bit
        public byte HeaderSpare;
        // 3 Bit
        public byte Spare;
        // 1 Bit
        public byte TI;
        // 2 Bit
        public byte FrameNumber;
        // 3 Bit
        public byte SubFlows;
        // 1 Bit
        public byte ChainIndicator;
        public List<RFCI> Rfcis;
        // 16 Bit
        public BitArray VersionsSupported;
        // 4 Bit
        public byte RfciDataPduType;
        // 4 Bit spare for Initialize

        public ControlProcedureMessage()
        {
            PduType = PduType.ControlProcedure;
            Rfcis = new List<RFCI>();
        }

        public void Parse(BinaryReader reader)
        {
            var bitArray = new BitArray(new[] { reader.ReadByte() });
            AckNack = bitArray.GetByteValue(2, 1, 0)[0];
            FrameNumber = bitArray.GetByteValue(2, 1, 2)[0];

            bitArray = new BitArray(new[] { reader.ReadByte() });
            ModeVersion = bitArray.GetByteValue(4, 1, 4)[0];
            Procedure = bitArray.GetByteValue(4, 1, 0)[0];

            bitArray = new BitArray(new[] { reader.ReadByte() });
            HeaderCRC = bitArray.GetByteValue(6, 1, 2)[0];
            PayloadCrc = new byte[2];
            PayloadCrc[0] = bitArray.GetByteValue(2, 1, 0)[0];

            bitArray = new BitArray(new[] { reader.ReadByte() });
            PayloadCrc[1] = bitArray.GetByteValue(8, 1, 0)[0];

            bitArray = new BitArray(new[] { reader.ReadByte() });
            Spare = bitArray.GetByteValue(3, 1, 5)[0];
            TI = bitArray.GetByteValue(1, 1, 4)[0];
            SubFlows = bitArray.GetByteValue(3, 1, 1)[0];
            ChainIndicator = bitArray.GetByteValue(1, 1, 0)[0];

            while (true)
            {
                var rfci = new RFCI();
                rfci.Parse(reader, SubFlows);
                Rfcis.Add(rfci);
                if(rfci.LRI)
                    break;
            }

            VersionsSupported = new BitArray(new[] {reader.ReadByte(), reader.ReadByte()});
            
            bitArray = new BitArray(new[] { reader.ReadByte() });
            RfciDataPduType = bitArray.GetByteValue(4, 1, 4)[0];
        }

        public byte[] ToBytes()
        {
            var result = new List<byte>
                         {
                             ((byte) PduType).ToBitArray().Concat(AckNack.ToBitArray(), 4, 2).Concat(FrameNumber.ToBitArray(), 6, 2).ToBytes()[0], 
                             ModeVersion.ToBitArray().Concat(Procedure.ToBitArray(), 4, 4).ToBytes()[0],
                         };
            // Ack
            if (AckNack == 0x01)
            {
                result.Add(HeaderCRC.ToBitArray().Concat(HeaderSpare.ToBitArray(), 6, 2).ToBytes()[0]);
                result.Add(Spare);
            }
            // Initialize
            else
            {
                result.Add(HeaderCRC.ToBitArray().Concat(PayloadCrc[0].ToBitArray(), 6, 2).ToBytes()[0]);
                result.Add(PayloadCrc[1]);
                result.Add(Spare.ToBitArray().Concat(TI.ToBitArray(), 3, 1).Concat(SubFlows.ToBitArray(), 4, 3).Concat(ChainIndicator.ToBitArray(), 7, 1).ToBytes()[0]);
                foreach (var rfci in Rfcis)
                    result.AddRange(rfci.ToBytes());
                result.AddRange(VersionsSupported.ToBytes());
                // RfciDataPduType + (0000 as spare)
                result.Add(RfciDataPduType.ToBitArray().Concat(new BitArray(new[]{false, false, false, false}), 4, 4).ToBytes()[0]);
            }

            return result.ToArray();
        }

        public static ControlProcedureMessage CreateAck()
        {
            var ack = new ControlProcedureMessage
            {
                AckNack = 0x01,
                FrameNumber = 0x00,
                ModeVersion = 0x01,
                Procedure = 0x00,
                HeaderCRC = 0x3D,
                HeaderSpare = 0x00,
                Spare = 0x00
            };
            return ack;
        }
    }
}