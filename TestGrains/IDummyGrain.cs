using System.Threading.Tasks;
using Orleans;

namespace TestGrains
{
    public interface IDummyGrain : IGrainWithGuidKey
    {
        Task<DummyInt> DoNotCallMe1();

        Task<DummyIntWithPayload> DoNotCallMe2();
    }
}