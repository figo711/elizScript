///////////////////////////////////////
/////GENERATED//BY//AST//GENERATOR/////
///////////////////////////////////////

using System;
using System.Collections.Generic;

namespace ElizScript.Core
{
    public abstract class Expr
    {
        public interface Visitor<T>
        {
            T VisitAssignExpr(Assign expr);
            T VisitBinaryExpr(Binary expr);
            T VisitCallExpr(Call expr);
            T VisitGetExpr(Get expr);
            T VisitGroupingExpr(Grouping expr);
            T VisitLiteralExpr(Literal expr);
            T VisitLogicalExpr(Logical expr);
            T VisitSetExpr(Set expr);
            T VisitBaseExpr(Base expr);
            T VisitThisExpr(This expr);
            T VisitUnaryExpr(Unary expr);
            T VisitVariableExpr(Variable expr);
        }

        public class Assign : Expr
        {
            public Assign(Token name, Expr value)
            {
                this.name = name;
                this.value = value;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }

            public Token name;
            public Expr value;
        }

        public class Binary : Expr
        {
            public Binary(Expr left, Token oper, Expr right)
            {
                this.left = left;
                this.oper = oper;
                this.right = right;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }

            public Expr left;
            public Token oper;
            public Expr right;
        }

        public class Call : Expr
        {
            public Call(Expr callee, Token paren, List<Expr> arguments)
            {
                this.callee = callee;
                this.paren = paren;
                this.arguments = arguments;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitCallExpr(this);
            }

            public Expr callee;
            public Token paren;
            public List<Expr> arguments;
        }

        public class Get : Expr
        {
            public Get(Expr @object, Token name)
            {
                this.@object = @object;
                this.name = name;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitGetExpr(this);
            }

            public Expr @object;
            public Token name;
        }

        public class Grouping : Expr
        {
            public Grouping(Expr expression)
            {
                this.expression = expression;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }

            public Expr expression;
        }

        public class Literal : Expr
        {
            public Literal(Object value)
            {
                this.value = value;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

            public Object value;
        }

        public class Logical : Expr
        {
            public Logical(Expr left, Token oper, Expr right)
            {
                this.left = left;
                this.oper = oper;
                this.right = right;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }

            public Expr left;
            public Token oper;
            public Expr right;
        }

        public class Set : Expr
        {
            public Set(Expr @object, Token name, Expr value)
            {
                this.@object = @object;
                this.name = name;
                this.value = value;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitSetExpr(this);
            }

            public Expr @object;
            public Token name;
            public Expr value;
        }

        public class Base : Expr
        {
            public Base(Token keyword, Token method)
            {
                this.keyword = keyword;
                this.method = method;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitBaseExpr(this);
            }

            public Token keyword;
            public Token method;
        }

        public class This : Expr
        {
            public This(Token keyword)
            {
                this.keyword = keyword;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitThisExpr(this);
            }

            public Token keyword;
        }

        public class Unary : Expr
        {
            public Unary(Token oper, Expr right)
            {
                this.oper = oper;
                this.right = right;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }

            public Token oper;
            public Expr right;
        }

        public class Variable : Expr
        {
            public Variable(Token name)
            {
                this.name = name;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }

            public Token name;
        }


        public abstract T Accept<T>(Visitor<T> visitor);
    }
}
