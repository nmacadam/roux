using System;

namespace Roux
{
    public class RouxException : Exception
    {
        public RouxException() {}
        public RouxException(string message)
            : base(message) {}
        
        public RouxException(string message, Exception inner)
            : base(message, inner) {}
    }
    
    internal class RuntimeException : RouxException
    {
        public readonly Token Token;

        public RuntimeException(Token op, string message)
            : base(message)
        {
            Token = op;
        }
    }

    internal class InterpreterException : RuntimeException 
    {
        public InterpreterException(Token op, string message)
            : base(op, message)
        { }
    }
    internal class EnvironmentException : RuntimeException
    {
        public EnvironmentException(Token token, string message)
            : base(token, message)
        {}
    }
}