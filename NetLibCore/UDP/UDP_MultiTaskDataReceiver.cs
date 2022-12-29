﻿namespace NetLib.UDP
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using RTP;

    /// <summary>
    /// 
    /// </summary>
    public class UDP_MultiTaskDataReceiver
    {
        private bool m_IsDisposed;
        private bool m_IsRunning;
        private Socket m_pSocket;
        private byte[] m_pBuffer;
        private int m_BufferSize = 1400;
        private SocketAsyncEventArgs m_pSocketArgs;
        private UDP_e_PacketReceived m_pEventArgs;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="socket">UDP socket.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>socket</b> is null reference.</exception>
        public UDP_MultiTaskDataReceiver(Socket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }

            m_pSocket = socket;
        }

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }
            m_IsDisposed = true;

            m_pSocket = null;
            m_pBuffer = null;
            if (m_pSocketArgs != null)
            {
                m_pSocketArgs.Dispose();
                m_pSocketArgs = null;
            }
            m_pEventArgs = null;

            PacketReceived = null;
            Error = null;
        }

        /// <summary>
        /// Starts receiving data.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this calss is disposed and this method is accessed.</exception>
        public void Start()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            if (m_IsRunning)
            {
                return;
            }
            m_IsRunning = true;

            bool isIoCompletionSupported = Net_Utils.IsSocketAsyncSupported();

            m_pEventArgs = new UDP_e_PacketReceived();
            m_pBuffer = new byte[m_BufferSize];

            if (isIoCompletionSupported)
            {
                m_pSocketArgs = new SocketAsyncEventArgs();
                m_pSocketArgs.SetBuffer(m_pBuffer, 0, m_BufferSize);
                m_pSocketArgs.RemoteEndPoint = new IPEndPoint(m_pSocket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
                m_pSocketArgs.Completed += delegate
                                           {
                                               if (m_IsDisposed)
                                               {
                                                   return;
                                               }

                                               try
                                               {
                                                   if (m_pSocketArgs.SocketError == SocketError.Success)
                                                   {
                                                       OnPacketReceived(m_pBuffer, m_pSocketArgs.BytesTransferred, (IPEndPoint) m_pSocketArgs.RemoteEndPoint);
                                                   }
                                                   else
                                                   {
                                                       OnError(new Exception("Socket error '" + m_pSocketArgs.SocketError + "'."));
                                                   }

                                                   IOCompletionReceive();
                                               }
                                               catch (Exception x)
                                               {
                                                   OnError(x);
                                               }
                                           };
            }

            // Move processing to thread pool.
            Task.Factory.StartNew(delegate
                                  {
                                      if (m_IsDisposed)
                                      {
                                          return;
                                      }

                                      try
                                      {
                                          if (isIoCompletionSupported)
                                          {
                                              IOCompletionReceive();
                                          }
                                          else
                                          {
                                              EndPoint rtpRemoteEP = new IPEndPoint(m_pSocket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
                                              m_pSocket.BeginReceiveFrom(
                                                  m_pBuffer,
                                                  0,
                                                  m_BufferSize,
                                                  SocketFlags.None,
                                                  ref rtpRemoteEP,
                                                  AsyncSocketReceive,
                                                  null
                                                  );
                                          }
                                      }
                                      catch (Exception x)
                                      {
                                          OnError(x);
                                      }
                                  });
        }

        /// <summary>
        /// Receives synchornously(if packet(s) available now) or starts waiting UDP packet asynchronously if no packets at moment.
        /// </summary>
        private void IOCompletionReceive()
        {
            try
            {
                // Use active worker thread as long as ReceiveFromAsync completes synchronously.
                // (With this approach we don't have thread context switches while ReceiveFromAsync completes synchronously)
                while (!m_IsDisposed && !m_pSocket.ReceiveFromAsync(m_pSocketArgs))
                {
                    if (m_pSocketArgs.SocketError == SocketError.Success)
                    {
                        try
                        {
                            OnPacketReceived(m_pBuffer, m_pSocketArgs.BytesTransferred, (IPEndPoint) m_pSocketArgs.RemoteEndPoint);
                        }
                        catch (Exception x)
                        {
                            OnError(x);
                        }
                    }
                    else
                    {
                        OnError(new Exception("Socket error '" + m_pSocketArgs.SocketError + "'."));
                    }

                    // Reset remote end point.
                    m_pSocketArgs.RemoteEndPoint = new IPEndPoint(m_pSocket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
                }
            }
            catch (Exception x)
            {
                OnError(x);
            }
        }

        /// <summary>
        /// Is called BeginReceiveFrom has completed.
        /// </summary>
        /// <param name="ar">The result of the asynchronous operation.</param>
        private void AsyncSocketReceive(IAsyncResult ar)
        {
            if (m_IsDisposed)
            {
                return;
            }

            try
            {
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                int count = m_pSocket.EndReceiveFrom(ar, ref remoteEP);

                OnPacketReceived(m_pBuffer, count, (IPEndPoint)remoteEP);
            }
            catch (Exception x)
            {
                OnError(x);
            }

            try
            {
                // Start receiving new packet.
                EndPoint rtpRemoteEP = new IPEndPoint(m_pSocket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
                m_pSocket.BeginReceiveFrom(
                    m_pBuffer,
                    0,
                    m_BufferSize,
                    SocketFlags.None,
                    ref rtpRemoteEP,
                    new AsyncCallback(this.AsyncSocketReceive),
                    null
                );
            }
            catch (Exception x)
            {
                OnError(x);
            }
        }

        /// <summary>
        /// Is raised when when new UDP packet is available.
        /// </summary>
        public event EventHandler<UDP_e_PacketReceived> PacketReceived = null;

        /// <summary>
        /// Raises <b>PacketReceived</b> event.
        /// </summary>
        /// <param name="buffer">Data buffer.</param>
        /// <param name="count">Number of bytes stored in <b>buffer</b></param>
        /// <param name="remoteEP">Remote IP end point from where data was received.</param>
        private void OnPacketReceived(byte[] buffer, int count, IPEndPoint remoteEP)
        {
            if (PacketReceived != null)
            {
                m_pEventArgs.Reuse(m_pSocket, buffer, count, remoteEP);

                PacketReceived(this, m_pEventArgs);
            }
        }

        /// <summary>
        /// Is raised when unhandled error happens.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> Error = null;

        /// <summary>
        /// Raises <b>Error</b> event.
        /// </summary>
        /// <param name="x">Exception happened.</param>
        private void OnError(Exception x)
        {
            if (m_IsDisposed)
            {
                return;
            }

            if (Error != null)
            {
                Error(this, new ExceptionEventArgs(x));
            }
        } 
    }
}