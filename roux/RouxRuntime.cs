using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("Tools")]
namespace Roux
{
    public class RouxRuntime
    {
        private readonly Interpreter _interpreter;

        private IInputOutput _io;
        private readonly IErrorReporter _errorReporter;

        public IInputOutput IO { get => _io; set => _io = value; }

        internal Interpreter Interpreter => _interpreter;

        public RouxRuntime()
        {
            _io = new RouxIoStream();
            _errorReporter = new RouxErrorReporter(_io);
            _interpreter = new Interpreter(_io, _errorReporter);
        }

        public RouxRuntime(IInputOutput io)
        {
            _io = io;
            _errorReporter = new RouxErrorReporter(_io);
            _interpreter = new Interpreter(_io, _errorReporter);
        }

        public void Run(string source)
        {
            Scanner scanner = new Scanner(source, _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            //foreach (var token in tokens) IO.Output(token.ToString());

            Parser parser = new Parser(tokens, _errorReporter);
            List<Stmt> statements = parser.Parse();

            if (_errorReporter.HadError)
            {
                return;
            }

            Resolver resolver = new Resolver(_interpreter, _errorReporter);
            resolver.Resolve(statements);

            if (_errorReporter.HadError)
            {
                return;
            }

            _interpreter.Interpret(statements);

            var instance = CreateInstance("Sandwich", "rye", "corned beef", "sauerkraut", "swiss cheese");
            ((RouxFunction)instance.Get("describe")).Call(this, new List<object>());
            ((RouxFunction)instance.Get("define")).Call(this, new List<object>());
            
            IO.Output(instance.ToString());
            IO.Output($"cheese type: { instance.Get("cheese") }");

            var foo = GetValue("foo");
            IO.Output(foo.ToString());
            
            SetValue("foo", 123);
            foo = GetValue("foo");
            IO.Output(foo.ToString());
        }

        public void ResetErrorSystem()
        {
            _errorReporter.Reset();
        }

        #region Interaction
        
        public object CallFunction(string address, params object[] args)
        {
            if (GetValue(address) is ICallable callable)
                return CallFunction(callable, args);
            return null;
        }
        
        public object CallFunction(ICallable callable, params object[] args)
        {
            Interpreter.SanitizeObjects(args);
            return callable.Call(this, new List<object>(args));
        }

        public RouxInstance CreateInstance(string className, params object[] constructArgs)
        {
            return CreateInstance(GetClass(className), constructArgs);
        }
        
        public RouxInstance CreateInstance(RouxClass klass, params object[] constructArgs)
        {
            Interpreter.SanitizeObjects(constructArgs);
            return klass.Call(this, new List<object>(constructArgs)) as RouxInstance;
        }

        public RouxClass GetClass(string className)
        {
            return GetValue(className) as RouxClass;
        }

        public object GetValue(string address)
        {
            var containingEnvironment = AddressToEnvironment(address, out var endToken);

            if (containingEnvironment != null)
            {
                return containingEnvironment.Fetch(endToken, false);
            }

            return null;
        }

        public void SetValue(string address, object value)
        {
            var containingEnvironment = AddressToEnvironment(address, out var endToken);

            var val = Interpreter.SanitizeObject(value);

            if (containingEnvironment != null)
            {
                //containingEnvironment.Assign(endToken, val, true, false);
                containingEnvironment.Assign(endToken, val);
            }
        }
        
        private Environment AddressToEnvironment(string address, out string lastTokenLexeme)
        {
            var parts = address.Split('.');
            lastTokenLexeme = parts[^1];
            Environment returnEnvironment = _interpreter.Globals;

            for (int i = 0; i < parts.Length - 1 && returnEnvironment != null; i++)
            {
                returnEnvironment = returnEnvironment.Fetch(parts[i], false) as Environment;
            }

            return returnEnvironment;
        }

        #endregion
    }
}
