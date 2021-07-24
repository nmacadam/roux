using System;
using System.Collections.Generic;
using System.Text;

namespace Roux
{
    public class RouxInstance
    {
        private RouxClass _class;
        private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();

        public RouxClass Class { get => _class; protected set => _class = value; }

        internal RouxInstance(RouxClass @class)
        {
            _class = @class;
        }

        public object Get(string name)
        {
            if (_fields.ContainsKey(name))
            {
                return _fields[name];
            }

            RouxFunction staticMethod = _class.FindStaticMethod(name);
            if (staticMethod != null)
            {
                // we don't want to bind a static method because it does not share the class's scope
                return staticMethod;
            }

            RouxFunction method = _class.FindMethod(name);
            if (method != null)
            {
                // bind the method to this instance with a new environment for it's scope within the class
                // we're doing this to give put the 'this' keyword in the method's scope 
                return method.Bind(this);
            }

            throw new RouxException($"Undefined property '{name}'.");
        }

        public void Set(string name, object value)
        {
            if (!_fields.ContainsKey(name))
            {
                throw new RouxException($"Undefined property '{name}'.");
            }
            _fields[name] = value;
        }

        internal object Get(Token name)
        {
            try
            {
                return Get(name.Lexeme);
            }
            catch (RouxException e)
            {
                throw new RuntimeException(name, e.Message);
            }
        }

        internal void Set(Token name, object value)
        {
            _fields[name.Lexeme] = value;
        }

        public override string ToString()
        {
            return $"{_class.Name} instance";
        }
    }
}
