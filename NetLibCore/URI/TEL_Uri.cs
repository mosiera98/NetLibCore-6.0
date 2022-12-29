using System;
using System.Collections.Generic;
using System.Text;
using NetLib.SIP.Message;
using NetLib.URI;

namespace NetLib.URI
{
    /// <summary>
    /// Implements TEL URI. Defined in RFC 2806.
    /// </summary>
    public class TEL_Uri : AbsoluteUri
    {
        private string m_number = "";


        /// <summary>
        /// Default constructor.
        /// </summary>
        public TEL_Uri()
        {
        }


        #region Properties implementation

        public bool IsGlobal
        {
            get { return false; }
        }

        public string PhoneNmber
        {
            get { return ""; }
        }


        public override string Scheme
        {
            get
            {
                return "tel";
            }
        }
        #endregion


        protected override void ParseInternal(string value)
        {
            // Syntax: sip:/sips: username@host:port *[;parameter] [?header *[&header]]
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            value = Uri.UnescapeDataString(value);

            if (!(value.ToLower().StartsWith("tel:")))
            {
                throw new SIP_ParseException("Specified value is invalid Tel-URI !");
            }

            StringReader r = new StringReader(value);

            var splited = value.Split(new[] { ':' });
            // Get username
            if (splited.Any() && splited.Count() > 1)
            {
                this.m_number = splited[1];
            }


        }

        public override string ToString()
        {
            // Syntax: sip:/sips: username@host *[;parameter] [?header *[&header]]

            StringBuilder retVal = new StringBuilder();
            retVal.Append("tel:");

            if (this.m_number != null)
            {
                retVal.Append(this.m_number);
            }

            return retVal.ToString();
        }

    }
}
