namespace NetLib.SIP.Message
{
    /// <summary>
    /// This base class for all SIP data types.
    /// </summary>
    public abstract class SIP_t_Value
    {
        #region method Parse

        /// <summary>
        /// Parses single value from specified reader.
        /// </summary>
        /// <param name="reader">Reader what contains </param>
        public abstract void Parse(StringReader reader);

        #endregion

        #region method ToStringValue

        /// <summary>
        /// Convert this to string value.
        /// </summary>
        /// <returns>Returns this as string value.</returns>
        public abstract string ToStringValue();

        #endregion
    }
}