using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("Tools")]
namespace Roux
{
    public class RouxEnvironment
    {
        private readonly RouxErrorReporter _errorReporter = new RouxErrorReporter();

        public void RunPrompt()
        {
            for (; ; )
            {
                Console.Write("> ");
                Run(Console.ReadLine());

                if (_errorReporter.HadError)
                {
                    _errorReporter.Reset();
                }            
            }
        }

        public void RunFile(string path)
        {
            string text = System.IO.File.ReadAllText(path);

            Run(text);

            if (_errorReporter.HadError)
            {
                _errorReporter.Reset();
            }
        }

        private void Run(string source)
        {
            Scanner scanner = new Scanner(source, _errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            foreach (var token in tokens)
            {
                Console.WriteLine("- " + token);
            }

            Parser parser = new Parser(tokens, _errorReporter);
            Expr expression = parser.Parse();

            AstPrinter printer = new AstPrinter();
            Console.WriteLine(printer.Print(expression));
        }
    }
}
