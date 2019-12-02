using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core.Tool
{
    public class ASTPrinter : Expr.Visitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            return Parenthesize("=", expr.value);
        }

        public string VisitBaseExpr(Expr.Base expr)
        {
            return "base";
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.oper.Lexeme, expr.left, expr.right);
        }

        public string VisitCallExpr(Expr.Call expr)
        {
            return Parenthesize("func", expr);
        }

        public string VisitGetExpr(Expr.Get expr)
        {
            return Parenthesize("get", expr.@object);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.value == null) return "null";
            return expr.value.ToString();
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            return Parenthesize("if", expr.left, expr.right);
        }

        public string VisitSetExpr(Expr.Set expr)
        {
            return Parenthesize("set", expr.@object, expr.value);
        }

        public string VisitThisExpr(Expr.This expr)
        {
            return Parenthesize("this");
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.oper.Lexeme, expr.right);
        }

        public string VisitVariableExpr(Expr.Variable expr)
        {
            return Parenthesize("var", expr);
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}
