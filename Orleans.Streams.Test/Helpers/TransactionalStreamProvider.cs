using System.Linq;
using System.Threading.Tasks;
using Orleans.Streams.Endpoints;

namespace Orleans.Streams.Test.Helpers
{
    public class TestTransactionalStreamConsumerAggregate<TIn> : MultiStreamListConsumer<TIn>
    { 

        public TestTransactionalStreamConsumerAggregate(IStreamProvider streamProvider) : base(streamProvider)
        {
        }

        public override async Task TearDown()
        {
            await base.TearDown();
        }

        public async Task<bool> AllConsumersTearDownCalled()
        {
            var values = await Task.WhenAll(Consumers.Select(c => c.IsTearedDown()));
            var valuesGrouped = values.GroupBy(x => x);
            return valuesGrouped.Count() == 1 && valuesGrouped.Count(group => group.Key == true) == 1;
        }
    }
}