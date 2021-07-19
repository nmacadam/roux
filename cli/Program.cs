using Roux;

namespace RouxCLI
{
    class Program
    {
        private static RouxEnvironment _roux = new RouxEnvironment();

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                _roux.RunFile(args[0]);
            }
            else
            {
                _roux.RunPrompt();
            }
        }
    }
}
