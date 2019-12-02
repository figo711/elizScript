//#define AST_GENERATOR

using ElizScript.Core;
using ElizScript.Core.Tool;
using System;
using System.Reflection;

namespace ElizScript
{
    class Program
    {
        public static string Version;

        static void Main(string[] args)
        {
#if AST_GENERATOR
            ASTGenerator._Main(args);
#elif !AST_GENERATOR

            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = AssemblyName.GetAssemblyName(assembly.Location);

            Version = assemblyName.Version.ToString();
            _Main(args);
            return;
#endif
        }

        static void _Main(string[] args)
        {
            if (args.Length > 2)
            {
                Console.WriteLine("Usage: eliz [script]");
                Console.ReadKey();
            }
            else if (args.Length == 2)
            {
                ElizScriptCompiler.RunFile(args[1]);
            }
            else
            {
                Console.WriteLine($"Eliz Script v{Version}");
                Console.WriteLine("Type \"help\", or \"credits\" for more information.");

                ElizScriptCompiler.RunPrompt();
            }
        }
    }

    // Error Reporting Exemple:
    //Error: Unexpected "," in argument list.
    //
    //15 | function(first, second,);
    //                           ^-- Here.
}
