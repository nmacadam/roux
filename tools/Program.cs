using System;
using System.Collections.Generic;
using System.IO;

namespace RouxTools
{
    class Program
    {
        static void Main(string[] args)
        {
            string outputDirectory;

            if (args.Length == 0)
            {
                outputDirectory = Console.ReadLine();
            }
            else if (args.Length == 1)
            {
                outputDirectory = args[0];
            }
            else
            {
                Console.WriteLine("Usage: generate_ast <output_directory>");
                return;
            }

            ASTGenerator.DefineAST(outputDirectory, "Expr", new List<string>(){
                "Assign   : Token name, Expr value",
                "Ternary  : Expr left, Expr middle, Expr right",
                "Binary   : Expr left, Token operator, Expr right",
                "Grouping : Expr expression",
                "Literal  : object value",
                "Logical  : Expr left, Token operator, Expr right",
                "Unary    : Token operator, Expr right",
                "Variable : Token name"
            });

            ASTGenerator.DefineAST(outputDirectory, "Stmt", new List<string>(){
                "Block      : List<Stmt> statements",
                "Expression  : Expr expression",
                "If         : Expr condition, Stmt thenBranch, Stmt elseBranch",
                "Print   : Expr expression",
                "Var        : Token name, Expr initializer"
            });
        }
    }
}
