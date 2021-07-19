﻿using Roux;
using System;

namespace RouxCLI
{
    class Program
    {
        private static RouxEnvironment _roux = new RouxEnvironment();

        static void Main(string[] args)
        {
            _roux.IO.OnOutput += Console.WriteLine;
            _roux.IO.OnError += Console.WriteLine;

            if (args.Length > 0)
            {
                RunFile(args[0]);
            }
            else
            {
                RunFile(@"D:\Development\roux\examples\static_methods.rx");
                //RunPrompt();
            }
        }

        static void RunFile(string path)
        {
            string text = System.IO.File.ReadAllText(path);
            _roux.Run(text);
        }

        static void RunPrompt()
        {
            for (; ; )
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                _roux.Run(input);
                _roux.ResetErrorSystem();
            }
        }
    }
}
