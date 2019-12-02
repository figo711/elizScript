using ElizScript.Core.Types;

namespace ElizScript.Core
{
    public class Token
    {
        private TokenType type;
        private string lexeme;
        private object literal;
        private int line;

        public TokenType Type { get => type; set => type = value; }
        public string Lexeme { get => lexeme; set => lexeme = value; }
        public object Literal { get => literal; set => literal = value; }
        public int Line { get => line; set => line = value; }

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        public override string ToString()
        {
            return $"{type} {lexeme} {literal}";
        }
    }
}
