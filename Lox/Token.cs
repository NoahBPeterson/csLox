using System.Collections.Generic;

namespace Lox
{
    public class Token
    {
        TokenType type;
        string lexeme;
        object literal;
        int line;

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        public static List<Token> Tokenize(string input)
        {
            List<Token> tokenizerList = new List<Token>();

            return tokenizerList; //Placeholder
        }

        public string toString() //ex. TokenType.NUMBER 3251 @objectID
        {
            return type + " " + lexeme + " " + literal;
        }



        public enum TokenType
        {
            //1 char tokens
            LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
            COMMA, DOT, MINUS, PLUS, FORWARD_SLASH, ASTERISK, SEMICOLON,

            //1-2 char tokens
            EXCLAMATION, EXCLAMATION_EQUALS,
            EQUALS, EQUALS_EQUALS,
            GREATER_THAN, GREATER_THAN_EQUALS,
            LESS_THAN, LESS_THAN_EQUALS,

            //Literals, arbitrary char length
            IDENTIFIER, STRING, NUMBER,

            //Keywords
            AND, OR, CLASS, IF, ELSE, FUNC, FOR, NIL, FALSE,
            PRINT, RETURN, SUPER_CLASS, THIS_OBJECT, TRUE, VAR, WHILE,

            EOF
        }
    }
}