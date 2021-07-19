using System.Collections.Generic;

namespace Roux
{
    internal class RouxFunction : ICallable
    {
        private readonly Stmt.Function _declaration;
        private readonly Environment _closure;
        private readonly bool _isConstructor;

        public int Arity => _declaration.Parameters.Count;
        public string Name => _declaration.Name.Lexeme;

        public RouxFunction(Stmt.Function declaration, Environment closure, bool isConstructor)
        {
            _declaration = declaration;
            _closure = closure;
            _isConstructor = isConstructor;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(_closure);
            for (int i = 0; i < _declaration.Parameters.Count; i++)
            {
                environment.Define(_declaration.Parameters[i].Lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (ReturnException returnValue)
            {
                if (_isConstructor)
                {
                    // always return 'this' in a constructor
                    return _closure.GetAt(0, "this");
                }

                return returnValue.Value;
            }

            // construct method always returns 'this' even when directly called
            if (_isConstructor) return _closure.GetAt(0, "this");

            return null;
        }

        /// <summary>
        /// Binds the function to a class instance, giving it access to the class's environment variables
        /// </summary>
        /// <param name="instance">The roux class instance to bind the function to</param>
        /// <returns>The bound method</returns>
        public RouxFunction Bind(RouxInstance instance)
        {
            Environment environment = new Environment(_closure);
            // Define the 'this' keyword in the new environment so the method can access it
            environment.Define("this", instance);
            return new RouxFunction(_declaration, environment, _isConstructor);
        }

        public override string ToString()
        {
            return $"<fn {_declaration.Name.Lexeme}>";
        }
    }

    internal class RouxLambda : ICallable
    {
        private readonly Expr.Lambda _declaration;
        private readonly Environment _closure;

        public int Arity => _declaration.Parameters.Count;
        public string Name => "lambda";

        public RouxLambda(Expr.Lambda declaration, Environment closure)
        {
            _declaration = declaration;
            _closure = closure;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(_closure);
            for (int i = 0; i < _declaration.Parameters.Count; i++)
            {
                environment.Define(_declaration.Parameters[i].Lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (ReturnException returnValue)
            {
                return returnValue.Value;
            }
            return null;
        }

        public override string ToString()
        {
            return $"<lambda fn>";
        }
    }
}
