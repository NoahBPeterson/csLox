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
            
            tokens.Add(new Token(TokenType.EOF, "", null, line));
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
                case '(': addToken(TokenType.LEFT_PAREN); break;
                case ')': addToken(TokenType.RIGHT_PAREN); break;
                case '{': addToken(TokenType.LEFT_BRACE); break;
                case '}': addToken(TokenType.RIGHT_BRACE); break;
                case ',': addToken(TokenType.COMMA); break;
                case '.': addToken(TokenType.DOT); break;
                case '-': addToken(TokenType.MINUS); break;
                case '+': addToken(TokenType.PLUS); break;
                case ';': addToken(TokenType.SEMICOLON); break;
                case '*': addToken(TokenType.ASTERISK); break;
                case '!': addToken(Match('=') ? TokenType.EXCLAMATION_EQUALS : TokenType.EXCLAMATION); break;
                case '=': addToken(Match('=') ? TokenType.EQUALS_EQUALS : TokenType.EQUALS); break;
                case '<': addToken(Match('=') ? TokenType.LESS_THAN_EQUALS : TokenType.LESS_THAN); break;
                case '>': addToken(Match('=') ? TokenType.GREATER_THAN_EQUALS : TokenType.GREATER_THAN); break;
                case '/':
                    if(Match('/'))
                    { //Comments
                        while (peek() != '\n' && !isAtEnd())
                        { advance(); }
                    }else{
                        addToken(TokenType.FORWARD_SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    break; //Whitespace characters
                case '\n': //Newline character
                    line++;
                    break;
                case '"': stringScanner(); break;
                default:
                    Lox.error(line, "Unexpected character.");
                    break;
            }
        }

        private void stringScanner()
        {
            while(peek() != '"' && !isAtEnd()) //Checks if "" is given, in which case we do nothing.
            {
                if (peek() == '\n')
                { line++; }
                advance(); //Advance until next quotation mark
            }

            if(isAtEnd()) //Never got to the next quotation mark
            {
                Lox.error(line, "Unterminated string.");
                return;
            }

            advance(); //Last "

            //Trim quotes, add token
            string value = source.Substring(start + 1, current - 1);
            addToken(TokenType.STRING, value);
        }

        private char peek()
        {
            if(isAtEnd())
            {
                return '\0'; // '\n' ?
            }
            return source.ToCharArray()[current];
        }

        private bool Match(char expected) //Conditional advance()
        {
            if(isAtEnd())
            { return false;}
            if (source.ToCharArray()[current] != expected)
            { return false; }

            current++;
            return true;
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