using System;

namespace Roux
{
    internal class RouxErrorReporter : IErrorReporter
    {
        public bool HadError { get; protected set; }
        public bool HadRuntimeError { get; protected set; }

        public void Reset()
        {
            HadError = false;
            HadRuntimeError = false;
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
            if (token.TokenType == TokenType.Eof)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, " at '" + token.Lexeme + "'", message);
            }
        }

        public void RuntimeError(Token token, string message)
        {
            Console.WriteLine($"[line {token.Line}] Runtime Error: {message}");
            HadRuntimeError = true;
        }

        private void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Parse Error {where}: {message}");
            HadError = true;
        }
    }
}