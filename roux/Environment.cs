using System;
using System.Collections.Generic;
using System.Text;

namespace Roux
{
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

        public object GetAt(int distance, string name)
        {
            return Ancestor(distance)._values[name];
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
        
        public void Assign(string lexeme, object value)
        {
            if (_values.ContainsKey(lexeme))
            {
                _values[lexeme] = value;
                return;
            }
            if (Enclosing != null)
            {
                Enclosing.Assign(lexeme, value);
                return;
            }

            // throw new EnvironmentException(name, $"Undefined variable '{name.Lexeme}'.");
            throw new Exception($"Undefined variable '{lexeme}'.");
        }

        public void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance)._values[name.Lexeme] = value;
        }

        public void Define(string name, object value)
        {
            // Note: this means roux can redefine a variable that already exists!
            _values[name] = value;
        }

        internal object Fetch(string lexeme, bool checkEnclosing)
        {
            if (_values.ContainsKey(lexeme))
            {
                return _values[lexeme];
            }
            if (checkEnclosing && Enclosing != null)
            {
                return Enclosing.Fetch(lexeme, true);
            }

            // Note: only throws error if an undefined reference is evaluated
            //throw new EnvironmentException(name, $"Undefined variable '{name.Lexeme}'.");
            throw new Exception($"Undefined variable '{lexeme}'.");
        }

        private Environment Ancestor(int distance)
        {
            Environment environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment.Enclosing;
            }
            return environment;
        }
    }
}
