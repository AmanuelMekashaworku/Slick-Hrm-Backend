using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace erp.Application.Services.Interfaces
{
    public interface IBackgroundJobService
    {
        void Enqueue<T>(Expression<Action<T>> methodCall);
        void Enqueue<T>(Expression<Func<T, Task>> methodCall); // Support async methods
    }
}
