using System;
using System.Reflection;
using System.Threading;

namespace Dorado.Core.Threading
{
    public class AsyncResultNoReturn : IAsyncResult
    {
        private const int c_StatePending = 0;
        private const int c_StateCompletedSynchronously = 1;
        private const int c_StateCompletedAsynchronously = 2;
        private readonly AsyncCallback m_AsyncCallback;
        private readonly object m_AsyncState;
        private int m_CompletedState;
        private ManualResetEvent m_AsyncWaitHandle;
        private Exception m_exception;
        private static AsyncCallback s_AsyncCallbackHelper = AsyncCallbackCompleteOpHelperNoReturnValue;
        private static WaitCallback s_WaitCallbackHelper = WaitCallbackCompleteOpHelperNoReturnValue;

        public object AsyncState
        {
            get
            {
                return m_AsyncState;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return m_CompletedState == 1;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (m_AsyncWaitHandle == null)
                {
                    bool isCompleted = IsCompleted;
                    ManualResetEvent manualResetEvent = new ManualResetEvent(isCompleted);
                    if (Interlocked.CompareExchange<ManualResetEvent>(ref m_AsyncWaitHandle, manualResetEvent, null) != null)
                    {
                        manualResetEvent.Close();
                    }
                    else
                    {
                        if (!isCompleted && IsCompleted)
                        {
                            m_AsyncWaitHandle.Set();
                        }
                    }
                }
                return m_AsyncWaitHandle;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return m_CompletedState != 0;
            }
        }

        public AsyncResultNoReturn(AsyncCallback asyncCallback, object state)
        {
            m_AsyncCallback = asyncCallback;
            m_AsyncState = state;
        }

        public void SetAsCompleted(Exception exception, bool completedSynchronously)
        {
            m_exception = exception;
            int num = Interlocked.Exchange(ref m_CompletedState, completedSynchronously ? 1 : 2);
            if (num != 0)
            {
                throw new InvalidOperationException("You can set a result only once");
            }
            if (m_AsyncWaitHandle != null)
            {
                m_AsyncWaitHandle.Set();
            }
            if (m_AsyncCallback != null)
            {
                m_AsyncCallback(this);
            }
        }

        public void EndInvoke()
        {
            if (!IsCompleted)
            {
                AsyncWaitHandle.WaitOne();
                AsyncWaitHandle.Close();
                m_AsyncWaitHandle = null;
            }
            if (m_exception != null)
            {
                throw m_exception;
            }
        }

        protected static AsyncCallback GetAsyncCallbackHelper()
        {
            return s_AsyncCallbackHelper;
        }

        protected IAsyncResult BeginInvokeOnWorkerThread()
        {
            ThreadPool.QueueUserWorkItem(s_WaitCallbackHelper, this);
            return this;
        }

        private static void AsyncCallbackCompleteOpHelperNoReturnValue(IAsyncResult otherAsyncResult)
        {
            AsyncResultNoReturn asyncResultNoReturn = (AsyncResultNoReturn)otherAsyncResult.AsyncState;
            asyncResultNoReturn.CompleteOpHelper(otherAsyncResult);
        }

        private static void WaitCallbackCompleteOpHelperNoReturnValue(object o)
        {
            AsyncResultNoReturn asyncResultNoReturn = (AsyncResultNoReturn)o;
            asyncResultNoReturn.CompleteOpHelper(null);
        }

        private void CompleteOpHelper(IAsyncResult ar)
        {
            Exception exception = null;
            try
            {
                OnCompleteOperation(ar);
            }
            catch (TargetInvocationException ex)
            {
                exception = ex.InnerException;
            }
            catch (Exception ex2)
            {
                exception = ex2;
            }
            finally
            {
                SetAsCompleted(exception, false);
            }
        }

        protected virtual void OnCompleteOperation(IAsyncResult ar)
        {
        }
    }
}