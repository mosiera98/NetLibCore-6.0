using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NetLib.Media;

namespace NetLib.IuUp
{
    class IuupTests
    {
        [Test]
        public void DecodeIuupDataMessage()
        {
            var bytes = new byte[]
                        {
                            0x01, 0x00, 0xe0, 0x19, 0xb3, 0xb5, 0x49, 0x60, 0x88, 0xbe, 0x8e, 0x37, 0x26, 0x3e, 0xf3, 0xb5
                            , 0x9a, 0x1d, 0x4d, 0xb0, 0xa5, 0x88, 0x36, 0x27, 0x32, 0xf8, 0x83, 0x4e, 0x03, 0xc0, 0xbf, 0xa7
                            , 0x8b, 0x37, 0x25, 0x37, 0xe1, 0x91, 0x06, 0x35, 0xff, 0xbb, 0xa6, 0x8b
                        };

            var iuup = (DataMessage)new IuUpMessage().Parse(bytes);
            Assert.AreEqual(iuup.AmrFrameHeader, 0);
            Assert.AreEqual(iuup.FQC, 0x00);
            Assert.AreEqual(iuup.IFtype, 8);
            Assert.AreEqual(iuup.RFCI, 0x00);
            Assert.AreEqual(iuup.PduType, PduType.Data);
            Assert.AreEqual(iuup.PayloadCrc, new byte[]{0, 25});
            Assert.AreEqual(iuup.HeaderCRC, 56);
            Assert.AreEqual(iuup.FrameNumber, 1);
        }

        [Test]
        public void ExtranctSampleVoice()
        {
            var messages = new List<DataMessage>();
            foreach (byte[] item in BiccSampleVoice.GetData())
            {
                IEnumerable<byte> buffer = item.Skip(0x36);
                byte[] buff = buffer.ToArray();

                if ((buff[0] & 0xf0) == 0x00) //PDU type = 0
                    messages.Add((DataMessage)new DataMessage().Parse(buff.ToArray()));
            }
            messages.RemoveAll(pkt => pkt.IFtype != 7);
            var result = new List<byte>();
            for (int i = 0; i < 1000; i++)
                result.AddRange(messages[i].DecodeAsAmr());
            result.InsertRange(0, new byte[] { 0x23, 0x21, 0x41, 0x4d, 0x52, 0x0a });
            File.WriteAllBytes("D:/x.amr", result.ToArray());

            var player = new AudioOut(AudioOut.Devices[0], 8000, 8, 1);
            player.Write(result.ToArray(), 0, result.Count);
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
