using System;

namespace TestGrains
{
    public static class TestUtil
    {
        public static bool BusyWaiting(TimeSpan timespan)
        {
            var end = DateTime.Now.Add(timespan);
            while (DateTime.Now < end)
            {
                ;
            }

            return true;
        }
    }
}