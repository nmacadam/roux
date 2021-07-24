using System;

namespace Roux
{
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
}