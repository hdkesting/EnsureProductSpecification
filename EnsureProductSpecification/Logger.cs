using System;
using System.IO;

namespace EnsureProductSpecification
{
    internal sealed class Logger : IDisposable
    {
        private readonly StreamWriter logWriter;

        public Logger(string filename)
        {
            logWriter = new StreamWriter(File.Open(filename, FileMode.Append));
        }

        public void Write(string message)
        {
            Console.WriteLine(message);
            logWriter.WriteLine("{0:dd-MM-yyyy HH:mm:ss} - {1}", DateTime.Now, message);
            logWriter.Flush();
        }

        public void Dispose()
        {
            logWriter?.Close();
            logWriter?.Dispose();
        }
    }
}
