using Roux;

namespace RouxCLI
{
    class Program
    {
        private static RouxEnvironment _roux = new RouxEnvironment();

        static void Main(string[] args)
        {
            _roux.RunPrompt();
        }
    }
}
