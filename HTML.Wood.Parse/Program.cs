using HTML.Wood.Parse.Services;
using System;

namespace HTML.Wood.Parse
{
    class Programs
    {
        private const char StopKeyChar = 's';

        static void Main()
        {
            Console.WriteLine("Press 's' for stop");

            using (var job = new WoodParserService())
            {
                job.StartJob();
                Wait();
            }
        }

        private static void Wait()
        {
            var ch = GetKeyChar();
            if (ch == StopKeyChar)
            {
                return;
            }
            Wait();
        }

        private static char GetKeyChar()
        {
            return Console.ReadKey(false).KeyChar;
        }
    }
}
