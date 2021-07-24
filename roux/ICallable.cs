using System;
using System.Collections.Generic;
using System.Text;

namespace Roux
{
    public interface ICallable
    {
        int Arity { get; }
        string Name { get; }
        object Call(RouxRuntime runtime, List<object> arguments);
    }
    
    internal interface IInternalCallable : ICallable
    {
        object Call(Interpreter interpeter, List<object> arguments);
    }
}
