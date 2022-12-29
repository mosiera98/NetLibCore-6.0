using NUnit.Framework;
using NetLib.IuUp;

namespace NetLib
{
    public class UnitTests
    {
        [Test]
        public void ParseIuupDataMessage()
        {
            var iuupMessage = (DataMessage)new IuUpMessage().Parse(new byte[]
                                                           {
                                                               0x01, 0x00, 0xe1, 0x8f, 0x72, 0x5e, 0x1c, 0x00, 0x06, 0xdc, 0x86, 0x07, 0x82, 0x78, 0xab, 0xe6
                                                               , 0xef, 0xa0, 0x0f, 0x6d, 0x3d, 0x03, 0x36, 0x74, 0x74, 0xe5, 0x9a, 0x29, 0x2c, 0x56, 0x88, 0x0c
                                                               , 0xcc, 0x81, 0xe0
                                                           });

            Assert.AreEqual(iuupMessage.GetType(), typeof(DataMessage));
            Assert.AreEqual(iuupMessage.PduType, PduType.Data);
            Assert.AreEqual(iuupMessage.FrameNumber, 1);
            Assert.AreEqual(iuupMessage.FQC, 0);
            Assert.AreEqual(iuupMessage.RFCI, 0);
            Assert.AreEqual(iuupMessage.HeaderCRC, 0x38);
            Assert.AreEqual(iuupMessage.PayloadCrc, new byte[] { 0x01, 0x8F });
        }

        [Test]
        public void ParseIuupControlMessage()
        {
            var controlProcedureMessage = (ControlProcedureMessage)new IuUpMessage().Parse(new byte[]
                                                      {
                                                          0xe0, 0x10, 0x0d, 0xc9, 0x06, 0x00, 0x51, 0x67, 0x3c, 0x81, 0x27, 0x00, 0x00, 0x00, 0x02, 0x00
                                                      });

            Assert.AreEqual(controlProcedureMessage.GetType(), typeof(ControlProcedureMessage));
            Assert.AreEqual(controlProcedureMessage.AckNack, 0);
            Assert.AreEqual(controlProcedureMessage.PduType, PduType.ControlProcedure);
            Assert.AreEqual(controlProcedureMessage.FrameNumber, 0);
            Assert.AreEqual(controlProcedureMessage.ModeVersion, 1);
            Assert.AreEqual(controlProcedureMessage.Procedure, 0);
            Assert.AreEqual(controlProcedureMessage.HeaderCRC, 3);
            Assert.AreEqual(controlProcedureMessage.PayloadCrc, new byte[]{0x01, 0xC9});
            Assert.AreEqual(controlProcedureMessage.Spare, 0);
            Assert.AreEqual(controlProcedureMessage.TI, 0);
            Assert.AreEqual(controlProcedureMessage.SubFlows, 3);
            Assert.AreEqual(controlProcedureMessage.ChainIndicator, 0);
            Assert.AreEqual(controlProcedureMessage.Rfcis.Count, 2);
        }

        [Test]
        public void IuupToBytes()
        {
            var stream = new byte[] {0xe0, 0x10, 0x0d, 0xc9, 0x06, 0x00, 0x51, 0x67, 0x3c, 0x81, 0x27, 0x00, 0x00, 0x00, 0x02, 0x00};
            var controlProcedureMessage = (ControlProcedureMessage)new IuUpMessage().Parse(stream);
            var bytes = controlProcedureMessage.ToBytes();
            for (int i = 0; i < bytes.Length; i++)
                Assert.AreEqual(bytes[i], stream[i]);

            var iuupMessage = new ControlProcedureMessage
            {
                AckNack = 0x01,
                FrameNumber = 0x00,
                ModeVersion = 0x01,
                Procedure = 0x00,
                HeaderCRC = 0x3D,
                HeaderSpare = 0x00,
                Spare = 0x00
            };
            Assert.AreEqual(iuupMessage.ToBytes()[0], 0xE4);
            Assert.AreEqual(iuupMessage.ToBytes()[1], 0x10);
            Assert.AreEqual(iuupMessage.ToBytes()[2], 0xF4);
            Assert.AreEqual(iuupMessage.ToBytes()[3], 0x00);

            stream = new byte[]
                     {
                         0x01, 0x00, 0xe1, 0x8f, 0x72, 0x5e, 0x1c, 0x00, 0x06, 0xdc, 0x86, 0x07, 0x82, 0x78, 0xab, 0xe6, 0xef, 0xa0, 
                         0x0f, 0x6d, 0x3d, 0x03, 0x36, 0x74, 0x74, 0xe5, 0x9a, 0x29, 0x2c, 0x56, 0x88, 0x0c, 0xcc, 0x81, 0xe0
                     };
            var dataMessage = (DataMessage)new IuUpMessage().Parse(stream);
            bytes = dataMessage.ToBytes();
            for (int i = 0; i < bytes.Length; i++)
                Assert.AreEqual(bytes[i], stream[i]);
        }
    }
}