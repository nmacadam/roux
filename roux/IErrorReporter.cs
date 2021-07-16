namespace Roux
{
    internal interface IErrorReporter
    {
        void Error(string message);
        void Error(int line, string message);
        void Error(Token token, string message);
        //void RuntimeError(RuntimeError error);
        //void Report(int line, string where, string message);

        bool HadError { get; }
        //bool HadRuntimeError { get; }
    }
}