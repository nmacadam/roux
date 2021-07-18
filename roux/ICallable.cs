using System;
using System.Collections.Generic;
using System.Text;

namespace Roux
{
    internal interface ICallable
    {
        int Arity { get; }
        string Name { get; }
        object Call(Interpreter interpeter, List<object> arguments);
    }
}
