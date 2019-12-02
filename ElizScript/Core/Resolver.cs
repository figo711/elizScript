using ElizScript.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core
{
    public class Resolver : Expr.Visitor<Void>, Stmt.Visitor<Void>
    {
        private Interpreter interpreter;
        private List<Dictionary<string, bool>> scopes = new List<Dictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public void Resolve(List<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        void BeginScope()
        {
            //scopes.Push(new Dictionary<string, bool>());
            scopes.Add(new Dictionary<string, bool>());
        }

        void EndScope()
        {
            scopes.RemoveAt(scopes.Count - 1);
        }

        void Declare(Token name)
        {
            if (scopes.Count == 0) return;

            Dictionary<string, bool> scope = scopes.Last(); /*Peek()*/

            if (scope.ContainsKey(name.Lexeme))
            {
                ElizScriptCompiler.Error(name,
                    "Variable with this name already declared in this scope.");
            }

            scope[name.Lexeme] = false;
        }

        void Define(Token name)
        {
            if (scopes.Count == 0) return;

            scopes.Last()[name.Lexeme] = true;
        }

        void Delete(Token name)
        {
            if (scopes.Count == 0) return;

            scopes.
        }

        void ResolveLocal(Expr expr, Token name)
        {
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes.ElementAt(i).ContainsKey(name.Lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count - 1 - i);
                    return;
                }
            }
        }

        void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();

            foreach (Token param in function.@params)
            {
                Declare(param);
                Define(param);
            }

            Resolve(function.body);
            EndScope();

            currentFunction = enclosingFunction;
        }

        public Void VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);

            return null;
        }

        public Void VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public Void VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();

            return null;
        }

        public Void VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.callee);

            foreach (Expr argument in expr.arguments)
            {
                Resolve(argument);
            }

            return null;
        }

        public Void VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public Void VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public Void VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.expression);
            return null;
        }

        public Void VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);

            if (stmt.elseBranch != null) Resolve(stmt.elseBranch);

            return null;
        }

        public Void VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public Void VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);

            return null;
        }

        public Void VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public Void VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                ElizScriptCompiler.Error(stmt.keyword, "Cannot return from top-level code.");
            }

            if (stmt.value != null)
            {
                if (currentFunction == FunctionType.INITIALIZER)
                {
                    ElizScriptCompiler.Error(stmt.keyword,
                        "Cannot return a value from an initializer.");
                }

                Resolve(stmt.value);
            }

            return null;
        }

        public Void VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.right);

            return null;
        }

        public Void VisitVariableExpr(Expr.Variable expr)
        {
            try
            {
                if (scopes.Count > 0 && scopes.Last()[expr.name.Lexeme] == false)
                {
                    ElizScriptCompiler.Error(expr.name,
                         "Cannot read local variable in its own initializer.");
                }

                
            }
            catch { }

            ResolveLocal(expr, expr.name);

            return null;
        }

        public Void VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.name);

            if (stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }

            Define(stmt.name);

            return null;
        }

        public Void VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);

            return null;
        }

        public Void VisitClassStmt(Stmt.Class stmt)
        {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            Declare(stmt.name);
            Define(stmt.name);

            if (stmt.baseclass != null && stmt.name.Lexeme.Equals(stmt.baseclass.name.Lexeme))
            {
                ElizScriptCompiler.Error(stmt.baseclass.name, "A class cannot inherit from itself.");
            }

            if (stmt.baseclass != null)
            {
                currentClass = ClassType.SUBCLASS;
                Resolve(stmt.baseclass);
            }

            if (stmt.baseclass != null)
            {
                BeginScope();
                scopes.Last()["base"] = true;
            }

            BeginScope();
            scopes.Last()["this"] = true;

            foreach (Stmt.Function method in stmt.methods)
            {
                FunctionType declaration = FunctionType.METHOD;

                if (method.name.Lexeme.Equals("init"))
                {
                    declaration = FunctionType.INITIALIZER;
                }

                ResolveFunction(method, declaration);
            }

            EndScope();

            if (stmt.baseclass != null) EndScope();

            currentClass = enclosingClass;
            return null;
        }

        public Void VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.@object);

            return null;
        }

        public Void VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.value);
            Resolve(expr.@object);

            return null;
        }

        public Void VisitThisExpr(Expr.This expr)
        {
            if (currentClass == ClassType.NONE)
            {
                ElizScriptCompiler.Error(expr.keyword,
                    "Cannot use 'this' outside of a class.");
                return null;
            }

            ResolveLocal(expr, expr.keyword);
            return null;
        }

        public Void VisitBaseExpr(Expr.Base expr)
        {
            if (currentClass == ClassType.NONE)
            {
                ElizScriptCompiler.Error(expr.keyword, "Cannot use 'base' outside of a class.");
            }
            else if (currentClass != ClassType.SUBCLASS)
            {
                ElizScriptCompiler.Error(expr.keyword, "Cannot use 'base' in a class with no baseclass.");
            }

            ResolveLocal(expr, expr.keyword);
            return null;
        }

        public Void VisitDeleteStmt(Stmt.Delete stmt)
        {
            

            return null;
        }
    }
}
