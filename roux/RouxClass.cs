using System;
using System.Collections.Generic;
using System.Text;

namespace Roux
{
    internal class RouxClass : RouxInstance, ICallable
    {
        private readonly int _arity;
        private readonly string _name;
        private readonly Dictionary<string, RouxFunction> _methods = new Dictionary<string, RouxFunction>();
        private readonly Dictionary<string, RouxFunction> _staticMethods = new Dictionary<string, RouxFunction>();

        public int Arity => _arity;
        public string Name => _name;

        public RouxClass(string name, Dictionary<string, RouxFunction> methods, Dictionary<string, RouxFunction> staticMethods)
            : base(null)
        {
            klass = this;
            _name = name;
            _methods = methods;
            _staticMethods = staticMethods;

            // set arity based on constructor argument count
            RouxFunction constructor = FindMethod("construct");
            _arity = constructor == null ? 0 : constructor.Arity;
        }


        public object Call(Interpreter interpreter, List<object> arguments)
        {
            // When a class is called (i.e. Foo()), an instance is created
            RouxInstance instance = new RouxInstance(this);

            // If the class has a constructor, we bind it and call it along with any given arguments
            RouxFunction constructor = FindMethod("construct");
            if (constructor != null)
            {
                constructor.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }

        public RouxFunction FindMethod(string name)
        {
            if (_methods.ContainsKey(name))
            {
                return _methods[name];
            }
            return null;
        }

        public RouxFunction FindStaticMethod(string name)
        {
            if (_staticMethods.ContainsKey(name))
            {
                return _staticMethods[name];
            }
            return null;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
