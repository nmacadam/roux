using System.Collections.Generic;
using System.IO;

namespace RouxTools
{
    static class ASTGenerator
    {
        public static void DefineAST(string outputDirectory, string baseName, List<string> types)
        {
            string filePath = $"{outputDirectory}/{baseName}.cs";

            using (StreamWriter outputFile = new StreamWriter(filePath))
            {
                outputFile.WriteLine("using System.Collections.Generic;");
                outputFile.WriteLine();
                outputFile.WriteLine("namespace Roux");
                outputFile.WriteLine("{");

                outputFile.WriteLine("\tinternal abstract class Expr");
                outputFile.WriteLine("\t{");

                DefineVisitor(outputFile, baseName, types);
                outputFile.WriteLine();

                outputFile.WriteLine("\t\tpublic abstract T Accept<T>(Visitor<T> visitor);");
                outputFile.WriteLine();

                foreach (var type in types)
                {
                    string className = type.Split(":")[0].Trim();
                    string fields = type.Split(":")[1].Trim();
                    DefineType(outputFile, baseName, className, fields);
                }

                outputFile.WriteLine("\t}");

                outputFile.WriteLine("}");
            }
        }

        private static void DefineVisitor(StreamWriter outputFile, string baseName, List<string> types)
        {
            outputFile.WriteLine("\t\tpublic interface Visitor<T>");
            outputFile.WriteLine("\t\t{");

            foreach (var type in types)
            {
                string typeName = type.Split(":")[0].Trim();
                outputFile.WriteLine($"\t\t\tT Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
            }

            outputFile.WriteLine("\t\t}");
        }

        private static void DefineType(StreamWriter outputFile, string baseName, string className, string fieldList)
        {
            outputFile.WriteLine($"\t\tinternal class {className} : {baseName}");
            outputFile.WriteLine("\t\t{");

            // Fields
            string[] fields = fieldList.Split(", ");
            foreach (var field in fields)
            {
                string[] fieldInfo = field.Split(' ');
                outputFile.WriteLine($"\t\t\tpublic readonly {fieldInfo[0]} {UppercaseFirst(fieldInfo[1])};");
            }
            outputFile.WriteLine();

            // Constructor
            outputFile.WriteLine($"\t\t\tpublic {className}({fieldList})");
            outputFile.WriteLine("\t\t\t{");
            foreach (var field in fields)
            {
                string name = field.Split(" ")[1];
                outputFile.WriteLine($"\t\t\t\t{UppercaseFirst(name)} = {name};");
            }
            outputFile.WriteLine("\t\t\t}");

            // Visitor Pattern
            outputFile.WriteLine();
            outputFile.WriteLine($"\t\t\tpublic override T Accept<T>(Visitor<T> visitor)");
            outputFile.WriteLine("\t\t\t{");
            outputFile.WriteLine($"\t\t\t\treturn visitor.Visit{className}{baseName}(this);");
            outputFile.WriteLine("\t\t\t}");

            outputFile.WriteLine("\t\t}");
            outputFile.WriteLine();
        }

        private static string UppercaseFirst(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            return char.ToUpper(str[0]) + str.Substring(1).ToLower(); ;
        }
    }
}
