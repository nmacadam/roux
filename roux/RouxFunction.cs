using System.Collections.Generic;

namespace Roux
{
    internal class RouxFunction : ICallable
    {
        private readonly Stmt.Function _declaration;
        private readonly Environment _closure;

        public int Arity => _declaration.Parameters.Count;
        public string Name => _declaration.Name.Lexeme;

        public RouxFunction(Stmt.Function declaration, Environment closure)
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
