using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core.Tool
{
    public class ASTGenerator
    {
        public static void _Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: ast <output directory> <namespace>");
                Console.ReadKey();
                System.Environment.Exit(1);
            }

            string output = args[1];
            string namespacePut = args[2];

            DefineAST(output, namespacePut, "Expr", new List<string>
            {
                "Assign   : Token name, Expr value",
                "Binary   : Expr left, Token oper, Expr right",
                "Call     : Expr callee, Token paren, List<Expr> arguments",
                "Get      : Expr @object, Token name",
                "Grouping : Expr expression",
                "Literal  : Object value",
                "Logical  : Expr left, Token oper, Expr right",
                "Set      : Expr @object, Token name, Expr value",
                "Base    : Token keyword, Token method",
                "This     : Token keyword",
                "Unary    : Token oper, Expr right",
                "Variable : Token name",
            });

            DefineAST(output, namespacePut, "Stmt", new List<string>
            {
                "Block      : List<Stmt> statements",
                "Class      : Token name, Expr.Variable baseclass, List<Stmt.Function> methods",
                "Expression : Expr expression",
                "Function   : Token name, List<Token> @params, List<Stmt> body",
                "If         : Expr condition, Stmt thenBranch, Stmt elseBranch",
                "Print      : Expr expression",
                "Return     : Token keyword, Expr value",
                "Var        : Token name, Expr initializer",
                "While      : Expr condition, Stmt body",
            });
        }

        static void DefineAST(string output, string namespaceString, string baseName, List<string> types)
        {
            string path =  $"{output}/{baseName}.cs";

            path = Path.GetFullPath(path);

            using (StreamWriter writer = new StreamWriter(File.Create(path)))
            {
                writer.WriteLine("///////////////////////////////////////");
                writer.WriteLine("/////GENERATED//BY//AST//GENERATOR/////");
                writer.WriteLine("///////////////////////////////////////");
                writer.WriteLine();
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine();
                writer.WriteLine($"namespace {namespaceString}");
                writer.WriteLine("{");
                writer.WriteLine($"    public abstract class {baseName}");
                writer.WriteLine( "    {");

                DefineVisitor(writer, baseName, types);

                // The AST classes.
                foreach (string type in types)
                {
                    string className = type.Split(':')[0].Trim();
                    string fields = type.Split(':')[1].Trim();

                    DefineType(writer, baseName, className, fields);
                }

                writer.WriteLine();
                writer.WriteLine( "        public abstract T Accept<T>(Visitor<T> visitor);");

                writer.WriteLine( "    }");
                writer.WriteLine("}");
            }
        }

        static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
        {
            writer.WriteLine("        public interface Visitor<T>");
            writer.WriteLine("        {");

            foreach (string type in types)
            {
                string typeName = type.Split(':')[0].Trim();
                writer.WriteLine($"            T Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
            }

            writer.WriteLine("        }");
            writer.WriteLine();
        }

        static void DefineType(StreamWriter writer, string baseName, string className, string fieldList)
        {
            writer.WriteLine($"        public class {className} : {baseName}");
            writer.WriteLine( "        {");

            // Constructor.                                              
            writer.WriteLine($"            public {className}({fieldList})");
            writer.WriteLine( "            {");

            // Store parameters in fields.            
            string[] fields = fieldList.Split(new[] { ", " }, StringSplitOptions.None);
            foreach (string field in fields)
            {
                string name = field.Split(' ')[1];
                writer.WriteLine($"                this.{name} = {name};");
            }

            writer.WriteLine( "            }");

            writer.WriteLine();
            writer.WriteLine( "            public override T Accept<T>(Visitor<T> visitor)");
            writer.WriteLine( "            {");
            writer.WriteLine($"                return visitor.Visit{className}{baseName}(this);");
            writer.WriteLine( "            }");

            // Fields.                                                   
            writer.WriteLine();
            foreach (string field in fields)
            {
                writer.WriteLine($"            public {field};");
            }

            writer.WriteLine("        }");
            writer.WriteLine();
        }
    }
}
