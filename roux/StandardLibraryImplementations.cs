using System;
using System.Collections.Generic;

namespace Roux.StandardLibrary
{
    public abstract class ExternalInstance : RouxInstance
    {
        internal ExternalInstance(ExternalClass externalClass) : base(externalClass)
        {}
    }

    public abstract class ExternalClass : RouxClass
    {
        internal ExternalClass(string name) 
            : base(name, new Dictionary<string, ICallable>(), new Dictionary<string, ICallable>())
        {}
        
        public void DefineMethod(string name, Callable callable)
        {
            methods.Add(name, callable);
        }
        
        public void DefineStaticMethod(string name, Callable callable)
        {
            methods.Add(name, callable);
        }
    }
    
    public class List : ExternalInstance
    {
        private List<object> _objects = new List<object>();
        
        internal List(ExternalClass externalClass) : base(externalClass)
        {
            externalClass.DefineMethod("add", new Callable(1, arguments => _objects.Add((arguments.args[0]))));
            externalClass.DefineMethod("at", new Callable(1, arguments => _objects[Convert.ToInt32(arguments.args[0])]));
            externalClass.DefineMethod("count", new Callable(1, arguments => _objects.Count));
            externalClass.DefineMethod("setAt", new Callable(2, arguments => _objects[Convert.ToInt32(arguments.args[0])] = arguments.args[1]));
        }
    }
    
    public class ListClass : ExternalClass, IInternalCallable
    {
        internal ListClass() : base("List")
        {
        }

        public override object Call(RouxRuntime runtime, List<object> arguments)
        {
            return new List(this);
        }

        object IInternalCallable.Call(Interpreter interpreter, List<object> arguments)
        {
            return new List(this);
        }
    }
    
    public class Map : ExternalInstance
    {
        private Dictionary<object, object> _objects = new Dictionary<object, object>();
        
        internal Map(ExternalClass externalClass) : base(externalClass)
        {
            externalClass.DefineMethod("at", new Callable(1, (args) => _objects[args.args[0]]));
            externalClass.DefineMethod("add", new Callable(2, (args) => _objects.Add(args.args[0], args.args[1])));
            externalClass.DefineMethod("count", new Callable(1, arguments => _objects.Count));
            externalClass.DefineMethod("setAt", new Callable(2, (args) => _objects[args.args[0]] = args.args[1]));
        }
    }
    
    public class MapClass : ExternalClass, IInternalCallable
    {
        internal MapClass() : base("Map")
        {
        }

        public override object Call(RouxRuntime runtime, List<object> arguments)
        {
            return new Map(this);
        }

        object IInternalCallable.Call(Interpreter interpreter, List<object> arguments)
        {
            return new Map(this);
        }
    }
}