using NUnit.Framework;
using Roux;

namespace RouxTests
{
    class TestErrorReporter : IErrorReporter
    {
        public bool HadError => false;
        public bool HadRuntimeError => false;

        public void Warning(string message)
        {

        }

        public void Warning(int line, string message)
        {

        }

        public void Warning(Token token, string message)
        {

        }

        public void Error(string message)
        {
            Report(-1, "", message);
        }

        public void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public void Error(Token token, string message)
        {
            Report(token.Line, "", message);
        }

        public void Reset()
        {

        }

        public void RuntimeError(Token token, string message)
        {
            throw new AssertionException($"[line {token.Line}] RuntimeError: {message}");
        }

        private void Report(int line, string where, string message)
        {
            throw new AssertionException($"[line {line}] Error {where}: {message}");
        }
    }
}
