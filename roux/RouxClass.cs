using System;
using System.Collections.Generic;
using System.Text;

namespace Roux
{
    public class RouxClass : RouxInstance, IInternalCallable
    {
        private Dictionary<string, ICallable> _methods;
        private Dictionary<string, ICallable> _staticMethods;

        public int Arity { get; }
        public string Name { get; }

        internal Dictionary<string, ICallable> methods => _methods;
        internal Dictionary<string, ICallable> staticMethods => _staticMethods;

        internal RouxClass(string name, Dictionary<string, ICallable> methods, Dictionary<string, ICallable> staticMethods)
            : base(null)
        {
            Class = this;
            Name = name;
            _methods = methods;
            _staticMethods = staticMethods;

            // set arity based on constructor argument count
            RouxFunction constructor = (RouxFunction)FindMethod("construct");
            Arity = constructor == null ? 0 : constructor.Arity;
        }

        public virtual object Call(RouxRuntime runtime, List<object> arguments)
        {
            return ((IInternalCallable)this).Call(runtime.Interpreter, arguments);
        }

        object IInternalCallable.Call(Interpreter interpreter, List<object> arguments)
        {
            // When a class is called (i.e. Foo()), an instance is created
            RouxInstance instance = new RouxInstance(this);

            // If the class has a constructor, we bind it and call it along with any given arguments
            RouxFunction constructor = (RouxFunction)FindMethod("construct");
            if (constructor != null)
            {
                ((IInternalCallable)constructor.Bind(instance)).Call(interpreter, arguments);
            }

            return instance;
        }

        internal ICallable FindMethod(string name)
        {
            if (_methods.ContainsKey(name))
            {
                return _methods[name];
            }
            return null;
        }

        internal ICallable FindStaticMethod(string name)
        {
            if (_staticMethods.ContainsKey(name))
            {
                return _staticMethods[name];
            }
            return null;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
