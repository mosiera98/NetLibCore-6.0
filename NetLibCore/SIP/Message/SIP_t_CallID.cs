using System;

namespace NetLib.SIP.Message
{
    /// <summary>
    /// Implements SIP "callid" value. Defined in RFC 3261.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 3261 Syntax:
    ///     callid = word [ "@" word ]
    /// </code>
    /// </remarks>
    public class SIP_t_CallID : SIP_t_Value
    {
        private string m_CallID = "";

        /// <summary>
        /// Gets or sets call ID.
        /// </summary>
        public string CallID
        {
            get { return m_CallID; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property CallID value may not be null or empty !");
                }

                m_CallID = value;
            }
        }

        /// <summary>
        /// Creates new call ID value.
        /// </summary>
        /// <returns>Returns call ID value.</returns>
        public static SIP_t_CallID CreateCallID()
        {
            var callID = new SIP_t_CallID {CallID = Guid.NewGuid().ToString().Replace("-", "")};
            return callID;
        }

        /// <summary>
        /// Parses "callid" from specified value.
        /// </summary>
        /// <param name="value">SIP "callid" value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>value</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public void Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Parse(new StringReader(value));
        }

        /// <summary>
        /// Parses "callid" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            // callid = word [ "@" word ]

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Get Method
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'callid' value, callid is missing !");
            }
            m_CallID = word;
        }

        /// <summary>
        /// Converts this to valid "callid" value.
        /// </summary>
        /// <returns>Returns "callid" value.</returns>
        public override string ToStringValue()
        {
            return m_CallID;
        }
    }
}