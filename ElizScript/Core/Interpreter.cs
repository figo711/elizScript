using ElizScript.Core.ElizObjects;
using ElizScript.Core.Errors;
using ElizScript.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core
{
    public class Void
    {
        public static readonly Void Instance = null;
        private Void() { }
    }

    public class Interpreter : Expr.Visitor<object>, Stmt.Visitor<Void>
    {
        #region FIELDS
        public readonly Environment globals;
        private Environment environment;
        private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();
        #endregion

        public Interpreter()
        {
            globals = new Environment();
            environment = globals;

            var timeFunc = new NativeFunc<string>(0);
            var helpFunc = new NativeFunc<Void>(0);
            var inputFunc = new NativeFunc<string>(1);

            timeFunc.AddFunc(0, (args) =>
            {
                return DateTime.Now.ToLongDateString();
            });

            helpFunc.AddFunc(0, (args) =>
            {
                VisitPrintStmt(new Stmt.Print(new Expr.Literal($"\nWelcome to Eliz Script {Program.Version} help program!\n\t- Keywords:\n\t\t- and\n\t\t- base\n\t\t- bool\n\t\t- class\n\t\t- else\n\t\t- false\n\t\t- for\n\t\t- if\n\t\t- ")));
                return null;
            });

            inputFunc.AddFunc(0, (args) =>
            {
                return Console.ReadLine();
            });

            inputFunc.AddFunc(1, (args) =>
            {
                Console.Write(args[0]);
                return Console.ReadLine();
            });

            globals.Define("time", timeFunc);
            globals.Define("help", helpFunc);
            globals.Define("input", inputFunc);
        }

        public void Resolve(Expr expr, int depth)
        {
            Console.WriteLine($"Resolve: {expr} {depth}");
            locals[expr] = depth;
        }

        public object LookUpVariable(Token name, Expr expr)
        {
            try
            {
                int distance = locals[expr];

                return environment.GetAt(distance, name.Lexeme);
            }
            catch (KeyNotFoundException e)
            {
                return globals.Get(name);
            }
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                ElizScriptCompiler.RuntimeError(error);
            }
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private string Stringify(object obj)
        {
            if (obj == null) return "null";

            if (obj is float)
            {
                string text = obj.ToString();

                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }

                return text;
            }

            return obj.ToString();
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.oper.Type)
            {
                case TokenType.PLUS:
                    if (left is int && right is int)
                    {
                        return (int)left + (int)right;
                    }

                    if (left is float && right is float)
                    {
                        return (float)left + (float)right;
                    }

                    if (left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.oper, "Operands must be two numbers or two strings.");

                case TokenType.GREATER:
                    CheckNumberOperands(expr.oper, left, right);
                    return (float)left > (float)right;

                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.oper, left, right);
                    return (float)left >= (float)right;

                case TokenType.LESS:
                    CheckNumberOperands(expr.oper, left, right);
                    return (float)left < (float)right;

                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.oper, left, right);
                    return (float)left <= (float)right;

                case TokenType.BANG_EQUAL: return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL: return IsEqual(left, right);

                case TokenType.MINUS:
                    CheckNumberOperands(expr.oper, left, right);
                    return (float)left - (float)right;

                case TokenType.STAR:
                    CheckNumberOperands(expr.oper, left, right);
                    return (float)left * (float)right;

                case TokenType.SLASH:
                    CheckNumberOperands(expr.oper, left, right);
                    return (float)left / (float)right;
            }

            // Unreachable.
            return null;
        }

        private bool IsEqual(object a, object b)
        {
            // null is only equal to null.               
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.oper.Type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    CheckNumberOperand(expr.oper, right);
                    return -(float)right;
            }

            // Unreachable.
            return null;
        }

        private void CheckNumberOperand(Token oper, object operand)
        {
            if (operand is float) return;
            throw new RuntimeError(oper, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token oper, object left, object right)
        {
            if (left is float && right is float) return;

            throw new RuntimeError(oper, "Operands must be numbers.");
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool) return (bool)obj;
            return true;
        }

        public Void VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public Void VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookUpVariable(expr.name, expr);
            //return environment.Get(expr.name);
        }

        public Void VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;

            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            environment.Define(stmt.name.Lexeme, value);

            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);

            try
            {
                int distance = locals[expr];

                environment.AssignAt(distance, expr.name, value);
            }
            catch
            {
                globals.Assign(expr.name, value);
            }

            return value;
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public Void VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public Void VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);

            if (expr.oper.Type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.right);
        }

        public Void VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            object callee = Evaluate(expr.callee);

            List<object> arguments = new List<object>();

            foreach (Expr argument in expr.arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (!(callee is IElizCallable))
            {
                throw new RuntimeError(expr.paren,
                    "Can only call functions and classes.");
            }

            IElizCallable function = callee as IElizCallable;

            if (arguments.Count != function.Arity())
            {
                throw new RuntimeError(expr.paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");
            }

            return function.Call(this, arguments);
        }

        public Void VisitFunctionStmt(Stmt.Function stmt)
        {
            ElizFunction function = new ElizFunction(stmt, environment, false);

            environment.Define(stmt.name.Lexeme, function);

            return null;
        }

        public Void VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;

            if (stmt.value != null) value = Evaluate(stmt.value);

            throw new Return(value);
        }

        public Void VisitClassStmt(Stmt.Class stmt)
        {
            object baseclass = null;

            if (stmt.baseclass != null)
            {
                baseclass = Evaluate(stmt.baseclass);

                if (!(baseclass is ElizClass))
                {
                    throw new RuntimeError(stmt.baseclass.name, "Baseclass must be a class.");
                }
            }

            environment.Define(stmt.name.Lexeme, null);

            if (stmt.baseclass != null)
            {
                environment = new Environment(environment);
                environment.Define("base", baseclass);
            }

            Dictionary<string, ElizFunction> methods = new Dictionary<string, ElizFunction>();

            foreach (Stmt.Function method in stmt.methods)
            {
                ElizFunction function = new ElizFunction(method, environment, method.name.Lexeme.Equals("init"));
                methods[method.name.Lexeme] = function;
            }

            ElizClass klass = new ElizClass(stmt.name.Lexeme, (ElizClass)baseclass, methods);

            if (baseclass != null)
            {
                environment = environment.enclosing;
            }

            environment.Assign(stmt.name, klass);
            return null;
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            object @object = Evaluate(expr.@object);

            if (@object is ElizInstance) 
            {
                return ((ElizInstance)@object).Get(expr.name);
            }

            throw new RuntimeError(expr.name,
                "Only instances have properties.");
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            object @object = Evaluate(expr.@object);

            if (!(@object is ElizInstance)) {
                throw new RuntimeError(expr.name, "Only instances have fields.");
            }

            object value = Evaluate(expr.value);
            ((ElizInstance)@object).Set(expr.name, value);
            return value;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            return LookUpVariable(expr.keyword, expr);
        }

        public object VisitBaseExpr(Expr.Base expr)
        {
            int distance = locals[expr];

            ElizClass superclass = (ElizClass)environment.GetAt(distance, "base");

            // "this" is always one level nearer than "super"'s environment.
            ElizInstance @object = (ElizInstance)environment.GetAt(distance - 1, "this");

            ElizFunction method = superclass.FindMethod(expr.method.Lexeme);

            if (method == null)
            {
                throw new RuntimeError(expr.method, $"Undefined property '{expr.method.Lexeme}'.");
            }

            return method.Bind(@object);
        }
    }
}
