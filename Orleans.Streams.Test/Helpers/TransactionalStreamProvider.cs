using System.Linq;
using System.Threading.Tasks;
using Orleans.Streams.Endpoints;

namespace Orleans.Streams.Test.Helpers
{
    public class TestTransactionalTransactionalStreamConsumerAggregate<TIn> : TransactionalStreamListConsumer<TIn>
    { 

        public TestTransactionalTransactionalStreamConsumerAggregate(IStreamProvider streamProvider) : base(streamProvider)
        {
        }

        public override async Task TearDown()
        {
            await base.TearDown();
        }

        public async Task<bool> AllConsumersTearDownCalled()
        {
            return await MessageDispatcher.IsTearedDown();
        }
    }
}