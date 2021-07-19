using System;
using System.Collections.Generic;
using System.Text;

namespace Roux
{
    internal class RouxInstance
    {
        private readonly RouxClass _klass;
        private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();

        public RouxInstance(RouxClass klass)
        {
            _klass = klass;
        }

        public object Get(Token name)
        {
            if (_fields.ContainsKey(name.Lexeme))
            {
                return _fields[name.Lexeme];
            }

            RouxFunction method = _klass.FindMethod(name.Lexeme);
            if (method != null)
            {
                // bind the method to this instance with a new environment for it's scope within the class
                // we're doing this to give put the 'this' keyword in the method's scope 
                return method.Bind(this);
            }

            throw new RuntimeException(name, $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(Token name, object value)
        {
            _fields[name.Lexeme] = value;
        }

        public override string ToString()
        {
            return $"{_klass.Name} instance";
        }
    }
}
