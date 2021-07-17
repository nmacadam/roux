using System;
using System.Collections.Generic;
using System.Text;

namespace Roux
{
    internal class EnvironmentException : Exception
    {
        public readonly Token Token;

        public EnvironmentException(Token token, string message)
            : base(message)
        {
            Token = token;
        }
    }

    internal class Environment
    {
        public readonly Environment Enclosing;
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public Environment()
        {
            Enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            Enclosing = enclosing;
        }

        public object Get(Token name)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                return _values[name.Lexeme];
            }
            if (Enclosing != null)
            {
                return Enclosing.Get(name);
            }

            // Note: only throws error if an undefined reference is evaluated
            throw new EnvironmentException(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public void Assign(Token name, object value)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                _values[name.Lexeme] = value;
                return;
            }
            if (Enclosing != null)
            {
                Enclosing.Assign(name, value);
                return;
            }

            throw new EnvironmentException(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public void Define(string name, object value)
        {
            // Note: this means roux can redefine a variable that already exists!
            _values[name] = value;
        }
    }
}
