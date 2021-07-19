namespace Roux
{
    internal interface IErrorReporter
    {
        void Warning(string message);
        void Warning(int line, string message);
        void Warning(Token token, string message);

        void Error(string message);
        void Error(int line, string message);
        void Error(Token token, string message);
        void RuntimeError(Token token, string message);
        //void Report(int line, string where, string message);

        void Reset();

        bool HadError { get; }
        bool HadRuntimeError { get; }
    }
}