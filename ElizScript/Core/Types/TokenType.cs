namespace ElizScript.Core.Types
{
    public enum TokenType
    {
        // Single-character tokens.                      
        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
        COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

        // One or two character tokens.                  
        BANG, BANG_EQUAL,
        EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,

        // Literals.                                     
        IDENTIFIER, STRING, NUMBER,

        // Keywords.                                     
        AND, CLASS, ELSE, FALSE, FUNCTION, FOR, IF, NULL, OR,
        PRINT, RETURN, BASE, THIS, TRUE, VAR, WHILE,

        // New Keywords. Priority Level [1 - Very Important, 2 - Important, 3 - Normal, ...]

        // Level 1
        DELETE, BREAK, CONTINUE, ENUM

        // Level 2
        CONST,

        // Level 3
        DO,

        // Level 4
        IMPORT, EXPORT,


        // Level 5
        FOREACH, INTERFACE, ABSTRACT, SEALED, STATIC,

        // Level 6
        TRY,
        CATCH,
        FINALLY,

        DELEGATE,
        RAISE,
        DEFAULT,

        ASYNC, AWAIT,

        EOF
    }
}
