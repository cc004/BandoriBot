using BandoriBot.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    public class BlockingDelegate<T, TResult> where TResult : Task
    {
        private readonly Func<T, TResult> cmd;
        private Task task;

        public async Task<TResult> Run(T args)
        {
            Task<TResult> mytask;
            lock (this)
            {
                mytask = task.ContinueWith(_ => cmd(args));
                task = mytask;
            }
            return await mytask;
        }

        public BlockingDelegate(Func<T, TResult> command)
        {
            task = Task.CompletedTask;
            cmd = command;
        }
    }
}
