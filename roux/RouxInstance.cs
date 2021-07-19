using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("Tools")]
namespace Roux
{
    public interface IInputOutput
    {
        Action<string> OnInput { get; set; }
        Action<string> OnOutput { get; set; }
        Action<string> OnError { get; set; }

        void Input(string message);
        void Output(string message);
        void Error(string message);
    }

    public class RouxIoStream : IInputOutput
    {
        private System.Action<string> _onInput = delegate { };
        private System.Action<string> _onOutput = delegate { };
        private System.Action<string> _onError = delegate { };

        private System.Action _onFlush = delegate { };

        public Action<string> OnInput { get => _onInput; set => _onInput = value; }
        public Action<string> OnOutput { get => _onOutput; set => _onOutput = value; }
        public Action<string> OnError { get => _onError; set => _onError = value; }
        public Action OnFlush { get => _onFlush; set => _onFlush = value; }

        public void Input(string message)
        {
            OnInput.Invoke(message);
        }

        public void Output(string message)
        {
            OnOutput.Invoke(message);
        }

        public void Error(string message)
        {
            OnError.Invoke(message);
        }

        public void Flush()
        {
            OnFlush.Invoke();
        }
    }

    public class RouxInstance
    {
        private readonly Interpreter _interpreter;

        private IInputOutput _io;
        private readonly IErrorReporter _errorReporter;

        public IInputOutput IO { get => _io; set => _io = value; }

        public RouxInstance()
        {
            _io = new RouxIoStream();
            _errorReporter = new RouxErrorReporter(_io);
            _interpreter = new Interpreter(_io, _errorReporter);
        }

        public RouxInstance(IInputOutput io)
        {
            _io = io;
            _errorReporter = new RouxErrorReporter(_io);
            _interpreter = new Interpreter(_io, _errorReporter);
        }

        public void Run(string source)
        {
            Scanner scanner = new Scanner(source, _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

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
        }

        public void ResetErrorSystem()
        {
            _errorReporter.Reset();
        }
    }
}
