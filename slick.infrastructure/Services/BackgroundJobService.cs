using Hangfire;
using slick.Application.Services.Interfaces;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace slick.Infrastructure.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        public void Enqueue<T>(Expression<Action<T>> methodCall)
        {
            BackgroundJob.Enqueue(methodCall);
        }

        public void Enqueue<T>(Expression<Func<T, Task>> methodCall)
        {
            BackgroundJob.Enqueue(methodCall);
        }
    }
}
