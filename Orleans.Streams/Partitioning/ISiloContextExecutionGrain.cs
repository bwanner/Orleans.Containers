using System;
using System.Threading.Tasks;

namespace Orleans.Streams.Partitioning
{
    public interface ISiloContextExecutionGrain : IGrainWithGuidKey
    {
        Task Execute(Action<IGrainFactory> action);

        Task<object> ExecuteFunc(Func<IGrainFactory, object> func);

        Task<object> ExecuteFunc(Func<IGrainFactory, Task<object>> func);

        Task<object> ExecuteFunc(Func<IGrainFactory, object, Task<object>> func, object state);

        Task Execute(Action<IGrainFactory, object> action, object state);

        Task<string> GetSiloIdentity();
    }
}