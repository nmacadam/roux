using NUnit.Framework;
using Roux;

namespace RouxTests
{
    class TestErrorReporter : IErrorReporter
    {
        public bool HadError => false;

        public void Error(string message)
        {
            Report(-1, "", message);
        }

        public void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private void Report(int line, string where, string message)
        {
            throw new AssertionException($"[line {line}] Error {where}: {message}");
        }
    }
}
