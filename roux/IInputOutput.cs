using System;

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
}