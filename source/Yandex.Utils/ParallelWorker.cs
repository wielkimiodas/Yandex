using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Yandex.Utils
{
    public class ParallelWorker
    {
        int nThreads;
        Semaphore availableThreads;

        public ParallelWorker(int nThreads)
        {
            this.nThreads = nThreads;
            availableThreads = new Semaphore(nThreads, nThreads);

            int arg1, arg2;
            ThreadPool.GetMinThreads(out arg1, out arg2);
            if (arg1 < nThreads || arg2 < nThreads)
                ThreadPool.SetMinThreads(Math.Max(arg1, nThreads), Math.Max(arg2, nThreads));
        }

        public void Queue(Action action)
        {
            availableThreads.WaitOne();
            ThreadPool.QueueUserWorkItem(delegate
            {
                action();
                availableThreads.Release();
            });
        }

        public void Wait()
        {
            for (int i = 0; i < nThreads; i++)
                availableThreads.WaitOne();

            for (int i = 0; i < nThreads; i++)
                availableThreads.Release();
        }
    }
}
