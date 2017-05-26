using System;
using System.IO;
using inRiver.Remoting;

namespace EnsureProductSpecification
{
    class Program
    {
        private const string InRiverUrl = "http://localhost:8080";
        private const string InRiverUser = "pimuser1";
        private const string InRiverPassword = "pimuser1";

        private static void Main()
        {
            var instance = RemoteManager.CreateInstance(InRiverUrl, InRiverUser, InRiverPassword);
            var logfile = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "logfile.txt");
            Console.WriteLine("Log will be written to: " + logfile);
            using (var logger = new Logger(logfile))
            {
                var processor = new Processor(logger);
                processor.Execute();
                logger.Write("DONE");
            }
            Console.Write("Press <enter> to exit >");
            Console.ReadLine();
        }
    }
}
