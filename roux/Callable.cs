using System;
using System.Collections.Generic;
using System.Text;

namespace Roux
{
    public class FunctionArguments
    {
        public List<object> args;

        public T At<T>(int i) => (T)args[i];
        public object At(int i) => args[i];
        public static FunctionArguments New(params object[] arguments) =>
            new FunctionArguments() { args = new List<object>(arguments) };
        public static FunctionArguments New(List<object> arguments) =>
            new FunctionArguments() { args = arguments };
    }
    
    internal class Callable : IInternalCallable
    {
        private readonly Func<Interpreter, FunctionArguments, object> _func;
        private readonly int _arity;

        public int Arity => _arity;
        public string Name => "callable";

        public Callable(int arity, Func<Interpreter, FunctionArguments, object> func)
        {
            _arity = arity;
            _func = func;
        }

        public Callable(int arity, Func<FunctionArguments, object> func)
        {
            _arity = arity;
            _func = (interp, funcArgs) => func(funcArgs);
        }

        public Callable(int arity, Action<Interpreter> func)
        {
            _arity = arity;
            _func = (interp, funcArgs) => { func(interp); return null; };
        }

        public Callable(int arity, Action<Interpreter, FunctionArguments> func)
        {
            _arity = arity;
            _func = (interp, funcArgs) => { func(interp, funcArgs); return null; };
        }

        public Callable(int arity, Action<FunctionArguments> func)
        {
            _arity = arity;
            _func = (interp, funcArgs) => { func(funcArgs); return null; };
        }

        public Callable(Action func)
        {
            _arity = 0;
            _func = (interp, funcArgs) => { func(); return null; };
        }

        public Callable(Func<object> func)
        {
            _arity = 0;
            _func = (interp, funcArgs) => func();
        }
        
        public object Call(RouxRuntime runtime, List<object> arguments)
        {
            return Call(runtime.Interpreter, arguments);
        }

        public object Call(Interpreter interpreter, List<object> funcArgs)
        {
            return Interpreter.SanitizeObject(_func?.Invoke(interpreter, new FunctionArguments() { args = funcArgs })); ;
        }
        
        public object Call(Interpreter interpreter, FunctionArguments funcArgs)
        {
            return Interpreter.SanitizeObject(_func?.Invoke(interpreter, funcArgs));
        }
    }
}