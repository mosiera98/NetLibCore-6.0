﻿using System;

namespace NetLib.SIP.Stack
{
    /// <summary>
    /// This class represents REFER dialog. Defined in RFC 3515.
    /// </summary>
    public class SIP_Dialog_Refer : SIP_Dialog
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        internal SIP_Dialog_Refer()
        {
        }


        private void CreateNotify(string statusLine)
        {
            // TODO: Block for UAC ? because UAS can generate NOTIFY requests only.
        }


        /// <summary>
        /// Processes specified request through this dialog.
        /// </summary>
        /// <param name="e">Method arguments.</param>
        /// <returns>Returns true if this dialog processed specified request, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>e</b> is null reference.</exception>
        protected internal override bool ProcessRequest(SIP_RequestReceivedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (base.ProcessRequest(e))
            {
                return true;
            }

            if (e.Request.RequestLine.Method == SIP_Methods.NOTIFY)
            {
                OnNotify(e);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Is raised when NOTIFY request received.
        /// </summary>
        public event EventHandler<SIP_RequestReceivedEventArgs> Notify = null;

        /// <summary>
        /// Raises <b>Notify</b> event.
        /// </summary>
        /// <param name="e">Event args.</param>
        private void OnNotify(SIP_RequestReceivedEventArgs e)
        {
            if (Notify != null)
                Notify(this, e);
        }
    }
}
