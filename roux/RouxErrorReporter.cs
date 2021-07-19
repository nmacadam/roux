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

        public void Warning(string message)
        {
            ReportWarning(-1, "", message);
        }

        public void Warning(int line, string message)
        {
            ReportWarning(line, "", message);
        }

        public void Warning(Token token, string message)
        {
            ReportWarning(token.Line, " at '" + token.Lexeme + "'", message);
        }

        public void Error(string message)
        {
            ReportError(-1, "", message);
        }

        public void Error(int line, string message)
        {
            ReportError(line, "", message);
        }

        public void Error(Token token, string message)
        {
            if (token.TokenType == TokenType.Eof)
            {
                ReportError(token.Line, " at end", message);
            }
            else
            {
                ReportError(token.Line, " at '" + token.Lexeme + "'", message);
            }
        }

        public void RuntimeError(Token token, string message)
        {
            Console.WriteLine($"[line {token.Line}] Runtime Error: {message}");
            HadRuntimeError = true;
        }

        private void ReportWarning(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Warning {where}: {message}");
        }

        private void ReportError(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Parse Error {where}: {message}");
            HadError = true;
        }
    }
}