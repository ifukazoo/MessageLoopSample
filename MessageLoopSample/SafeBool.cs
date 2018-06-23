using System.Threading;

namespace MessageLoopSample
{
    class SafeBool
    {
        ReaderWriterLock rwlock = new ReaderWriterLock();
        private bool val;

        public void Set(bool b)
        {
            try
            {
                rwlock.AcquireWriterLock(Timeout.Infinite);
                val = b;
            }
            finally
            {
                rwlock.ReleaseWriterLock();
            }
        }
        public bool Get()
        {
            try
            {
                rwlock.AcquireReaderLock(Timeout.Infinite);
                return val;
            }
            finally
            {
                rwlock.ReleaseReaderLock();
            }
        }
    }
}
