using System;
using System.Threading.Tasks;
using Orleans.Collections;

namespace TestGrains
{
    [Serializable]
    public class SimpleCommunicator
    {
        private ContainerElementReference<SimpleCommunicator> _listener;
        public int ReceiveCount { get; private set; } = 0;

        public void SetListener(ContainerElementReference<SimpleCommunicator> listener)
        {
            _listener = listener;
        }

        public async Task SendNotification()
        {
            if (_listener != null)
            {
                //var action = new ContainerAction<SimpleCommunicator>((communicator, o) => communicator.ReceiveNotification(), _listener);
                //await action.Execute();
                // Task ExecuteLambda< TState > (Action < T, TState > action, TState state = default(TState));
                //await _listener.Executor.ExecuteLambda((communicator, state) => communicator.ReceiveNotification(), null, _listener);
            }
        }

        public void ReceiveNotification()
        {
            ReceiveCount++;
        }
    }
}