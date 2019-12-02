using ElizScript.Core.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core
{
    public class Scanner
    {
        private string source;
        private List<Token> tokens = new List<Token>();

        private int start = 0;
        private int current = 0;
        private int line = 1;

        private static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
        {
            { "and", TokenType.AND },
            { "class", TokenType.CLASS },
            { "else", TokenType.ELSE },
            { "false", TokenType.FALSE },
            { "for", TokenType.FOR },
            { "while", TokenType.WHILE },
            { "function", TokenType.FUNCTION },
            { "if", TokenType.IF },
            { "null", TokenType.NULL },
            { "or", TokenType.OR },
            { "print", TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "true", TokenType.TRUE },
            { "var", TokenType.VAR },
            { "this", TokenType.THIS },
            { "base", TokenType.BASE },
            { "import", TokenType.IMPORT },

            { "enum", TokenType.ENUM },

            { "continue", TokenType.CONTINUE },
            { "break", TokenType.BREAK },
            //{ "case", TokenType.CASE },
            { "default", TokenType.DEFAULT },

            //{ "catch", TokenType.CATCH },

            { "const", TokenType.CONST },

            { "abstract", TokenType.ABSTRACT},
            { "async", TokenType.ASYNC },
            { "await", TokenType.AWAIT },

            { "delegate", TokenType.DELEGATE },
            { "do", TokenType.DO },
            { "finally", TokenType.FINALLY },
            { "foreach", TokenType.FOREACH },
            //{ "get", TokenType.GET },
            //{ "in", TokenType.IN },
            { "interface", TokenType.INTERFACE },
            //{ "is", TokenType.IS },
            { "export", TokenType.EXPORT },
            //{ "new", TokenType.NEW },
            //{ "override", TokenType.OVERRIDE },
            //{ "params", TokenType.PARAMS },
            //{ "private", TokenType.PRIVATE },
            //{ "protected", TokenType.PROTECTED },
            //{ "public", TokenType.PUBLIC },
            //{ "readonly", TokenType.READONLY },
            { "delete", TokenType.DELETE },
            { "sealed", TokenType.SEALED },
            //{ "set", TokenType.SET },
            { "static", TokenType.STATIC },
            //{ "switch", TokenType.SWITCH },
            { "raise", TokenType.RAISE },
            { "try", TokenType.TRY },
            //{ "virtual", TokenType.VIRTUAL },
            //{ "volatile", TokenType.VOLATILE },
            //{ "where", TokenType.WHERE },
        };

        public Scanner(string source)
        {
            this.source = source;
        }

        public List<Token> ScanTokens()
        {
            while(!isAtEnd())
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));

            return tokens;
        }

        private bool isAtEnd()
        {
            return current >= source.Length;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;

                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.                
                        while (Peek() != '\n' && !isAtEnd()) Advance();
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;

                case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.                      
                    break;

                case '\n':
                    line++;
                    break;

                case '"': StringParse(); break;

                default:
                    if (IsDigit(c))
                    {
                        NumberParse();
                    }
                    else if (IsAlpha(c))
                    {
                        IdentifierParse();
                    }
                    else
                    {
                        ElizScriptCompiler.Error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private void IdentifierParse()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            // See if the identifier is a reserved word.   
            string text = source.Substring(start, current - start);

            if (keywords.ContainsKey(text))
            {
                TokenType? type = keywords[text];
                if (type == null) type = TokenType.IDENTIFIER;
                AddToken(type.Value);
            }
            else
            {
                AddToken(TokenType.IDENTIFIER);
            }
        }

        private void NumberParse()
        {
            while (IsDigit(Peek())) Advance();

            // Look for a fractional part.                            
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the "."                                      
                Advance();

                while (IsDigit(Peek())) Advance();
            }

            //Console.WriteLine(source.Substring(start, current - start));

            AddToken(TokenType.NUMBER,
                float.Parse(source.Substring(start, current - start), NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private void StringParse()
        {
            while (Peek() != '"' && !isAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            // Unterminated string.                                 
            if (isAtEnd())
            {
                ElizScriptCompiler.Error(line, "Unterminated string.");
                return;
            }

            // The closing ".                                       
            Advance();

            // Trim the surrounding quotes.                         
            string value = source.Substring(start + 1, current - start - 2);
            AddToken(TokenType.STRING, value);
        }

        private bool Match(char expected)
        {
            if (isAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        private char Peek()
        {
            if (isAtEnd()) return '\0';
            return source[current];
        }

        private char Advance()
        {
            current++;
            return source[current - 1];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }
    }
}
