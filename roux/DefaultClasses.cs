using System.Collections.Generic;

namespace Roux
{
    public class ArrayInstance : RouxInstance
    {
        private List<object> _objects = new List<object>() { 1, 2, 3 };

        internal ArrayInstance(RouxClass @class) : base(@class)
        {
            //@class.Environment.Define("get", new Callable(1, (args) => _objects[(int)args.At<double>(0)]));
            //@class.Environment.Define("set", new Callable(2, (args) => _objects[(int)args.At<double>(0)] = args.At<double>(1)));
        }
    }
    
    public class Array : RouxClass, IInternalCallable
    {
        internal Array() : base("Array", new Dictionary<string, RouxFunction>(), new Dictionary<string, RouxFunction>())
        {}

        public override object Call(RouxRuntime runtime, List<object> arguments)
        {
            return new ArrayInstance(this);
        }

        object IInternalCallable.Call(Interpreter interpreter, List<object> arguments)
        {
            return new ArrayInstance(this);
        }
    }
}