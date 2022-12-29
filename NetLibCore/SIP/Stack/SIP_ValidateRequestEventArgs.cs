using System;
using System.Net;

namespace NetLib.SIP.Stack
{
    /// <summary>
    /// This class provides data for SIP_Stack.ValidateRequest event.
    /// </summary>
    public class SIP_ValidateRequestEventArgs : EventArgs
    {
        private readonly SIP_Request m_pRequest;
        private readonly IPEndPoint m_pRemoteEndPoint;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="request">Incoming SIP request.</param>
        /// <param name="remoteEndpoint">IP end point what made request.</param>
        public SIP_ValidateRequestEventArgs(SIP_Request request, IPEndPoint remoteEndpoint)
        {
            m_pRequest = request;
            m_pRemoteEndPoint = remoteEndpoint;
        }

        /// <summary>
        /// Gets incoming SIP request.
        /// </summary>
        public SIP_Request Request
        {
            get { return m_pRequest; }
        }

        /// <summary>
        /// Gets IP end point what made request.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return m_pRemoteEndPoint; }
        }

        /// <summary>
        /// Gets or sets response code. Value null means SIP stack will handle it.
        /// </summary>
        public string ResponseCode { get; set; }
    }
}
