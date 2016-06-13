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
        public static bool BusyWaiting(int iterations = 2000000)
        {
            int i = 0;
            do
            {
                i++;
            } while (i < iterations);

            return true;
        }

        public static bool BusyWaiting2Mio()
        {
            int i = 0;
            do
            {
                i++;
            } while (i < 2000000);

            return true;
        }

        public static bool BusyWaiting4Mio()
        {
            int i = 0;
            do
            {
                i++;
            } while (i < 4000000);

            return true;
        }

        public static bool BusyWaiting500k()
        {
            int i = 0;
            do
            {
                i++;
            } while (i < 500000);

            return true;
        }
    }
}