using System;
using System.Collections.Generic;
using NetLib.SIP.Message;

namespace NetLib.SIP.Stack
{
    /// <summary>
    /// This is base class for SIP client and server transaction.
    /// </summary>
    public abstract class SIP_Transaction : IDisposable
    {
        private SIP_TransactionState m_State;
        private SIP_Stack m_pStack;
        private SIP_Flow m_pFlow;
        private SIP_Request m_pRequest;
        private string m_Method = "";
        private string m_ID = "";
        private string m_Key = "";
        private DateTime m_CreateTime;
        private List<SIP_Response> m_pResponses;
        private object m_pTag;
        private object m_pLock = new object();

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stack">Owner SIP stack.</param>
        /// <param name="flow">Transaction data flow.</param>
        /// <param name="request">SIP request that transaction will handle.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stack</b>,<b>flow</b> or <b>request</b> is null reference.</exception>
        public SIP_Transaction(SIP_Stack stack, SIP_Flow flow, SIP_Request request)
        {
            if (stack == null)
            {
                throw new ArgumentNullException("stack");
            }
            if (flow == null)
            {
                throw new ArgumentNullException("flow");
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            m_pStack = stack;
            m_pFlow = flow;
            m_pRequest = request;
            m_Method = request.RequestLine.Method;
            m_CreateTime = DateTime.Now;
            m_pResponses = new List<SIP_Response>();

            // Validate Via:
            SIP_t_ViaParm via = request.Via.GetTopMostValue();
            if (via == null)
            {
                throw new ArgumentException("Via: header is missing !");
            }
            if (via.Branch == null)
            {
                throw new ArgumentException("Via: header 'branch' parameter is missing !");
            }

            m_ID = via.Branch;

            if (this is SIP_ServerTransaction)
            {
                /*
                    We use branch and sent-by as indexing key for transaction, the only special what we need to 
                    do is to handle CANCEL, because it has same branch as transaction to be canceled.
                    For avoiding key collision, we add branch + '-' + 'sent-by' + CANCEL for cancel index key.
                    ACK has also same branch, but we won't do transaction for ACK, so it isn't problem.
                */
                string key = request.Via.GetTopMostValue().Branch + '-' + request.Via.GetTopMostValue().SentBy;
                if (request.RequestLine.Method == SIP_Methods.CANCEL)
                {
                    key += "-CANCEL";
                }
                m_Key = key;
            }
            else
            {
                m_Key = m_ID + "-" + request.RequestLine.Method;
            }
        }

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public virtual void Dispose()
        {
            SetState(SIP_TransactionState.Disposed);
            OnDisposed();

            m_pStack = null;
            m_pFlow = null;
            m_pRequest = null;

            StateChanged = null;
            Disposed = null;
            TimedOut = null;
            TransportError = null;
        }

        /// <summary>
        /// Cancels current transaction.
        /// </summary>
        public abstract void Cancel();

        /// <summary>
        /// Changes transaction state.
        /// </summary>
        /// <param name="state">New transaction state.</param>
        protected void SetState(SIP_TransactionState state)
        {
            // Log
            if (Stack.Logger != null)
            {
                Stack.Logger.AddText(ID,
                                     "Transaction [branch='" + ID + "';method='" + Method + "';IsServer=" +
                                     (this is SIP_ServerTransaction) + "] switched to '" + state.ToString() +
                                     "' state.");
            }

            m_State = state;

            OnStateChanged();

            if (m_State == SIP_TransactionState.Terminated)
            {
                Dispose();
            }
        }

        /// <summary>
        /// Adds specified response to transaction responses collection.
        /// </summary>
        /// <param name="response">SIP response.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>response</b> is null reference.</exception>
        protected void AddResponse(SIP_Response response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            // Don't store more than 15 responses, otherwise hacker may try todo buffer overrun with provisional responses.
            if (m_pResponses.Count < 15 || response.StatusCode >= 200)
            {
                m_pResponses.Add(response);
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the dialog.
        /// </summary>
        public object SyncRoot
        {
            get { return m_pLock; }
        }

        /// <summary>
        /// Gets if transaction is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return m_State == SIP_TransactionState.Disposed; }
        }

        /// <summary>
        /// Gets current transaction state.
        /// </summary>
        public SIP_TransactionState State
        {
            get { return m_State; }
        }

        /// <summary>
        /// Gets owner SIP stack.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public SIP_Stack Stack
        {
            get
            {
                if (State == SIP_TransactionState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pStack;
            }
        }

        /// <summary>
        /// Gets transaction data flow.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public SIP_Flow Flow
        {
            get
            {
                if (State == SIP_TransactionState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pFlow;
            }
        }

        /// <summary>
        /// Gets SIP request what caused this transaction creation.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public SIP_Request Request
        {
            get
            {
                if (State == SIP_TransactionState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pRequest;
            }
        }

        /// <summary>
        /// Gets request method that transaction handles.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public string Method
        {
            get
            {
                if (State == SIP_TransactionState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_Method;
            }
        }

        /// <summary>
        /// Gets transaction ID (Via: branch parameter value).
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public string ID
        {
            get
            {
                if (State == SIP_TransactionState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_ID;
            }
        }

        /// <summary>
        /// Gets time when this transaction was created.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public DateTime CreateTime
        {
            get
            {
                if (State == SIP_TransactionState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_CreateTime;
            }
        }

        /// <summary>
        /// Gets transaction processed responses.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public SIP_Response[] Responses
        {
            get
            {
                if (State == SIP_TransactionState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pResponses.ToArray();
            }
        }

        /// <summary>
        /// Gets transaction final(1xx) response from responses collection. Returns null if no provisional responses.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public SIP_Response LastProvisionalResponse
        {
            get
            {
                if (State == SIP_TransactionState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                for (int i = Responses.Length - 1; i > -1; i--)
                {
                    if (Responses[i].StatusCodeType == SIP_StatusCodeType.Provisional)
                    {
                        return Responses[i];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets transaction final(2xx - 699) response from responses collection. Returns null if no final responses.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public SIP_Response FinalResponse
        {
            get
            {
                if (State == SIP_TransactionState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                foreach (SIP_Response response in Responses)
                {
                    if (response.StatusCodeType != SIP_StatusCodeType.Provisional)
                    {
                        return response;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets if transaction has any provisional(1xx) in responses collection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this class is Disposed and this property is accessed.</exception>
        public bool HasProvisionalResponse
        {
            get
            {
                if (State == SIP_TransactionState.Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                foreach (SIP_Response response in m_pResponses)
                {
                    if (response.StatusCodeType == SIP_StatusCodeType.Provisional)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets transaction related SIP dialog. Returns null if no dialog available.
        /// </summary>
        public SIP_Dialog Dialog
        {
            // FIX ME:
            get { return null; }
        }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag
        {
            get { return m_pTag; }

            set { m_pTag = value; }
        }


        /// <summary>
        /// Gets transaction indexing key.
        /// </summary>
        internal string Key
        {
            get { return m_Key; }
        }

        /// <summary>
        /// Is raised when transaction state has changed.
        /// </summary>
        public event EventHandler StateChanged = null;

        /// <summary>
        /// Raises event <b>StateChanged</b>.
        /// </summary>
        private void OnStateChanged()
        {
            if (StateChanged != null)
            {
                StateChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Is raised when transaction is disposed.
        /// </summary>
        public event EventHandler Disposed = null;

        /// <summary>
        /// Raises event <b>Disposed</b>.
        /// </summary>
        protected void OnDisposed()
        {
            if (Disposed != null)
            {
                Disposed(this, new EventArgs());
            }
        }

        /// <summary>
        /// Is raised if transaction is timed out. 
        /// </summary>
        public event EventHandler TimedOut = null;

        /// <summary>
        /// Raises TimedOut event.
        /// </summary>
        protected void OnTimedOut()
        {
            if (TimedOut != null)
            {
                TimedOut(this, new EventArgs());
            }
        }

        /// <summary>
        /// Is raised when there is transport error. 
        /// </summary>
        public event EventHandler<ExceptionEventArgs> TransportError = null;

        /// <summary>
        /// Raises TimedOut event.
        /// </summary>
        /// <param name="exception">Transport exception.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>exception</b> is null reference.</exception>
        protected void OnTransportError(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            if (TransportError != null)
            {
                TransportError(this, new ExceptionEventArgs(exception));
            }
        }

        /// <summary>
        /// Is raised when there is transaction error. For example this is raised when server transaction never
        /// gets ACK.
        /// </summary>
        public event EventHandler TransactionError = null;

        /// <summary>
        /// Raises TransactionError event.
        /// </summary>
        /// <param name="errorText">Text describing error.</param>
        protected void OnTransactionError(string errorText)
        {
            if (TransactionError != null)
            {
                TransactionError(this, new EventArgs());
            }
        }
    }
}
