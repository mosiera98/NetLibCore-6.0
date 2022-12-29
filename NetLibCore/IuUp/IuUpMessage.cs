using System.Globalization;
using System.IO;

namespace NetLib.IuUp
{
    public class IuUpMessage
    {
        // 4 bit
        public PduType PduType;

        public IuUpMessage Parse(byte[] stream)
        {
            var reader = new BinaryReader(new MemoryStream(stream));
            var r = stream[0].ToString("X2")[0];
            PduType = (PduType)(int.Parse(r.ToString(), NumberStyles.HexNumber));
            
            if(PduType == PduType.ControlProcedure)
            {
                var message = new ControlProcedureMessage();
                message.Parse(reader);
                return message;
            }
            if (PduType == PduType.Data )
            {
                var message = new DataMessage();
                message.Parse(reader);
                return message;
            }
            return null;
        }
    }
}