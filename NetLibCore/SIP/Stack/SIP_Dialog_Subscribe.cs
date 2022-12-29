﻿using System;

namespace NetLib.SIP.Stack
{
    /// <summary>
    /// This class represent SUBSCRIBE dialog. Defined in RFC 3265.
    /// </summary>
    public class SIP_Dialog_Subscribe
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        internal SIP_Dialog_Subscribe()
        {
        }

        /// <summary>
        /// Sends notify request to remote end point.
        /// </summary>
        /// <param name="notify">SIP NOTIFY request.</param>
        public void Notify(SIP_Request notify)
        {
            if (notify == null)
            {
                throw new ArgumentNullException("notify");
            }

            // TODO:
        }
    }
}
