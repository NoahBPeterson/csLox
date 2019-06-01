using System.Collections.Generic;
using static Lox.Token;

namespace Lox
{
    class Scanner
    {
        private string source;
        private List<Token> tokens = new List<Token>();

        private int start = 0;
        private int current = 0;
        private int line = 1;

        Scanner(string source)
        {
            this.source = source;
        }

        List<Token> scanTokens()
        {
            while(!isAtEnd())
            {
                start = current;
                scanTokens();
            }
            
            tokens.Add(new Token(Token.TokenType.EOF, "", null, line));
            return tokens;
        }

        private bool isAtEnd()
        {
            return current >= source.Length;
        }

        private void scanToken()
        {
            char c = advance();
            switch(c)
            {
                case '(': addToken(Token.TokenType.LEFT_PAREN); break;
                case ')': addToken(Token.TokenType.RIGHT_PAREN); break;
                case '{': addToken(Token.TokenType.LEFT_BRACE); break;
                case '}': addToken(Token.TokenType.RIGHT_BRACE); break;
                case ',': addToken(Token.TokenType.COMMA); break;
                case '.': addToken(Token.TokenType.DOT); break;
                case '-': addToken(Token.TokenType.MINUS); break;
                case '+': addToken(Token.TokenType.PLUS); break;
                case ';': addToken(Token.TokenType.SEMICOLON); break;
                case '*': addToken(Token.TokenType.ASTERISK); break;
            }
        }

        private char advance()
        {
            current++;
            char[] c = source.ToCharArray();
            return c[current - 1];
        }

        private void addToken(TokenType type)
        {
            addToken(type, null);
        }

        private void addToken(TokenType type, object literal)
        {
            string text = source.Substring(start, current);
            tokens.Add(new Token(type, text, literal, line));
        }
    }
}