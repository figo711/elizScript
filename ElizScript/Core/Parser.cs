using ElizScript.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core
{
    public class Parser
    {
        private class ParseError : Exception { }

        private List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Stmt> Parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Stmt Declaration()
        {
            try
            {
                // Experimental
                if (Match(TokenType.IMPORT)) return ImportDeclaration();
                if (Match(TokenType.CONST)) return VarDeclaration();
                //

                if (Match(TokenType.CLASS)) return ClassDeclaration();
                if (Match(TokenType.FUNCTION)) return Function("function");
                if (Match(TokenType.VAR)) return VarDeclaration();

                return Statement();
            }
            catch (ParseError error)
            {
                Synchronize();
                return null;
            }
        }

        //Experimental
        private Stmt ImportDeclaration()
        {
            return null;
        }

        private Stmt ClassDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect class name.");

            Expr.Variable baseclass = null;

            if (Match(TokenType.LESS))
            {
                Consume(TokenType.IDENTIFIER, "Expect baseclass name.");
                baseclass = new Expr.Variable(Previous());
            }

            Consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");

            List<Stmt.Function> methods = new List<Stmt.Function>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                methods.Add(Function("method"));
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");

            return new Stmt.Class(name, baseclass, methods);
        }

        private Stmt.Function Function(string kind)
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect " + kind + " name.");

            Consume(TokenType.LEFT_PAREN, "Expect '(' after " + kind + " name.");

            List<Token> parameters = new List<Token>();

            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 8)
                    {
                        Error(Peek(), "Cannot have more than 8 parameters.");
                    }

                    parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
                } while (Match(TokenType.COMMA));
            }

            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

            Consume(TokenType.LEFT_BRACE, "Expect '{' before " + kind + " body.");
            List<Stmt> body = Block();
            return new Stmt.Function(name, parameters, body);
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt ConstVarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect const variable name.");

            Expr initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }
            else
            {
                Consume(TokenType.EQUAL, "Missing initializer in const declaration");
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt Statement()
        {
            // Experimental
            if (Match(TokenType.DELETE)) return DeleteStatement();
            //

            if (Match(TokenType.FOR)) return ForStatement();
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.RETURN)) return ReturnStatement();
            if (Match(TokenType.WHILE)) return WhileStatement();
            if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());

            return ExpressionStatement();
        }

        private Stmt DeleteStatement()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");
            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");

            return new Stmt.Delete(name);
        }

        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null;

            if (!Check(TokenType.SEMICOLON))
            {
                value = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
            return new Stmt.Return(keyword, value);
        }

        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt initializer;

            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr condition = null;

            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            Stmt body = Statement();

            if (increment != null)
            {
                body = new Stmt.Block(new List<Stmt>()
                {
                    body,
                    new Stmt.Expression(increment),
                });
            }

            if (condition == null) condition = new Expr.Literal(true);
            body = new Stmt.While(condition, body);

            if (initializer != null)
            {
                body = new Stmt.Block(new List<Stmt> { initializer, body });
            }

            return body;
        }

        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            Stmt body = Statement();

            return new Stmt.While(condition, body);
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(TokenType.AND))
            {
                Token oper = Previous();
                Expr right = Equality();
                //expr = new Expr.Logical(expr, oper, right);
            }

            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();

            while (Match(TokenType.OR))
            {
                Token oper = Previous();
                Expr right = And();
                //expr = new Expr.Logical(expr, oper, right);
            }

            return expr;
        }

        private Expr Assignment()
        {
            Expr expr = Or();

            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr is Expr.Variable)
                {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.Assign(name, value);
                }
                else if (expr is Expr.Get) 
                {
                    Expr.Get get = (Expr.Get)expr;
                    return new Expr.Set(get.@object, get.name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token oper = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, oper, right); 
            }

            return expr;
        }

        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return tokens[current];
        }

        private Token Previous()
        {
            return tokens[current - 1];
        }

        private Expr Comparison()
        {
            Expr expr = Addition();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token oper = Previous();
                Expr right = Addition();
                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        private Expr Addition()
        {
            Expr expr = Multiplication();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token oper = Previous();
                Expr right = Multiplication();
                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        private Expr Multiplication()
        {
            Expr expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token oper = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token oper = Previous();
                Expr right = Unary();
                return new Expr.Unary(oper, right);
            }

            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else if (Match(TokenType.DOT))
                {
                    Token name = Consume(TokenType.IDENTIFIER,
                        "Expect property name after '.'.");
                    expr = new Expr.Get(expr, name);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();

            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        Error(Peek(), "Cannot have more than 255 arguments.");
                    }

                    arguments.Add(Expression());
                } while (Match(TokenType.COMMA));
            }

            Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

            return new Expr.Call(callee, paren, arguments);
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NULL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous().Literal);
            }

            if (Match(TokenType.BASE))
            {
                Token keyword = Previous();

                Consume(TokenType.DOT, "Expect '.' after 'super'.");

                Token method = Consume(TokenType.IDENTIFIER, "Expect superclass method name.");

                return new Expr.Base(keyword, method);
            }

            if (Match(TokenType.THIS)) return new Expr.This(Previous());

            if (Match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(Previous());
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        private Token Consume(TokenType type, string msg)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), msg);
        }

        private ParseError Error(Token token, String message)
        {
            ElizScriptCompiler.Error(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.SEMICOLON) return;

                switch (Peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUNCTION:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }
    }
}
