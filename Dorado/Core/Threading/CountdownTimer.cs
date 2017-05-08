using System;
using System.Threading;

namespace Dorado.Core.Threading
{
    public sealed class CountdownTimer
    {
        private sealed class CountdownAsyncResult : AsyncResultNoReturn
        {
            private Timer m_Timer;

            public CountdownAsyncResult(int ms, AsyncCallback ac, object state)
                : base(ac, state)
            {
                m_Timer = new Timer(CountdownDone, null, ms, -1);
            }

            private void CountdownDone(object state)
            {
                SetAsCompleted(null, false);
                m_Timer.Dispose();
                m_Timer = null;
            }
        }

        public IAsyncResult BeginCountdown(int ms, AsyncCallback ac, object state)
        {
            return new CountdownAsyncResult(ms, ac, state);
        }

        public void EndCountdown(IAsyncResult ar)
        {
        }
    }
}