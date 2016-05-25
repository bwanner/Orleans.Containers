using System;
using System.Threading.Tasks;

namespace Orleans.Streams.Partitioning
{
    public interface ISiloContextExecutionGrain : IGrainWithGuidKey
    {
        Task Execute(Action action);

        Task Execute(Action<object> action, object state);

        Task<string> GetSiloIdentity();
    }
}