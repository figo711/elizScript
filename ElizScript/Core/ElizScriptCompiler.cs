using ElizScript.Core.Errors;
using ElizScript.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core
{
    public static class ElizScriptCompiler
    {
        private static Interpreter interpreter = new Interpreter();

        public static bool hadError = false;
        public static bool hadRuntimeError = false;

        public static void RunFile(string path)
        {
            string text = File.ReadAllText(Path.GetFullPath(path));
            Run(text);

            if (hadError)
            {
                Console.ReadKey();
                System.Environment.Exit(65);
            }

            if (hadRuntimeError)
            {
                Console.ReadKey();
                System.Environment.Exit(70);
            }
        }

        static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            //foreach (Token t in tokens) Console.WriteLine(t);
            //Console.WriteLine();

            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.Parse();

            // Stop if there was a syntax error.
            if (hadError) return;

            Resolver resolver = new Resolver(interpreter);
            resolver.Resolve(statements);

            // Stop if there was a resolution error.
            if (hadError) return;

            interpreter.Interpret(statements);
        }

        public static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                Run(Console.ReadLine());
                hadError = false;
            }
        }

        public static void Error(int line, string msg)
        {
            Report(line, "", msg);
        }

        static void Report(int line, string where, string msg)
        {
            Console.WriteLine($"[line: {line}] Error {where}: {msg}");
            hadError = true;
        }

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, " at '" + token.Lexeme + "'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.WriteLine($"{error.Message}\n[line {error.token.Line}]");
            hadRuntimeError = true;
        }
    }
}
