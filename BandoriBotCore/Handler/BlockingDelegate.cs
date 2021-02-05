using BandoriBot.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    public sealed class BlockingDelegate<T, TResult> : IDisposable
    {
        private readonly SemaphoreSlim sema = new SemaphoreSlim(1, 1);
        private readonly Func<T, Task<TResult>> cmd;

        public async Task<TResult> Run(T args)
        {
            try
            {
                await sema.WaitAsync();
                return await cmd(args);
            }
            finally
            {
                sema.Release();
            }
        }

        public void Dispose()
        {
            sema.Dispose();
        }

        public BlockingDelegate(Func<T, Task<TResult>> command)
        {
            cmd = command;
        }
    }
    public sealed class BlockingDelegate<T> : IDisposable
    {
        private readonly SemaphoreSlim sema = new SemaphoreSlim(1, 1);
        private readonly Func<T, Task> cmd;

        public async Task Run(T args)
        {
            try
            {
                await sema.WaitAsync();
                await cmd(args);
            }
            finally
            {
                sema.Release();
            }
        }

        public void Dispose()
        {
            sema.Dispose();
        }

        public BlockingDelegate(Func<T, Task> command)
        {
            cmd = command;
        }
    }
}
