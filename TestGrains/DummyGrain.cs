using System.Threading.Tasks;
using Orleans;

namespace TestGrains
{
    public class DummyGrain : Grain, IDummyGrain
    {
        public Task<DummyInt> DoNotCallMe1()
        {
            throw new System.NotImplementedException();
        }

        public Task<DummyIntWithPayload> DoNotCallMe2()
        {
            throw new System.NotImplementedException();
        }
    }
}