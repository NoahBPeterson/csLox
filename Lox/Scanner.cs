using System.Collections.Generic;
using System.IO;
using System;
using static Lox.Token;

namespace Lox
{
    public class Scanner
    {
        private string source;
        private List<Token> tokens = new List<Token>();

        private static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            { "and", TokenType.AND},
            { "class", TokenType.CLASS },
            { "else", TokenType.ELSE },
            { "false", TokenType.FALSE },
            { "for", TokenType.FOR },
            { "fun", TokenType.FUNC },
            { "if", TokenType.IF },
            {  "nil", TokenType.NIL },
            { "or", TokenType.OR },
            { "print", TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "super", TokenType.SUPER_CLASS },
            { "this", TokenType.THIS_OBJECT },
            { "true", TokenType.TRUE },
            { "var", TokenType.VAR },
            { "while", TokenType.WHILE }
        };


        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(string source)
        {
            this.source = source;
        }

        public List<Token> scanTokens()
        {
            while (!isAtEnd())
            {
                start = current;
                scanToken();
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
            switch (c)
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
                    if (Match('/'))
                    { //Comments
                        while (peek() != '\n' && !isAtEnd())
                        { advance(); }
                    } else {
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
                case 'o': //'or' but not 'orchid'
                    if (peek() == 'r')
                    {
                        if(peekNext() == ' ' || peekNext() == '\t' || peekNext() == '\r' || peekNext() == '\n')
                        {
                            addToken(TokenType.OR);
                            advance();
                        } else
                        {
                            current--;
                            identifier();
                        }
                    } break;


                default:
                    if (isDigit(c))
                    {
                        number();
                    } else if (isAlpha(c))
                    {
                        identifier();
                    } else
                    {
                        Lox.error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        private void identifier()
        {
            while (isAlphaNumeric(peek())) { advance(); }

            string text = source.Substring(start, current-start);
            TokenType type = TokenType.IDENTIFIER;
            keywords.TryGetValue(text, out type);

            if (type == 0) //0 = TokenType.LEFT_PAREN, which would not reach this.
            {
                type = TokenType.IDENTIFIER;
            }
            addToken(type);

        }

        private bool isAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   (c == '_');
        }

        private bool isAlphaNumeric(char c)
        {
            return isAlpha(c) || isDigit(c);
        }

        private void number()
        {
            while (isDigit(peek())) advance();

            if (peek() == '.' && isDigit(peekNext()))
            {
                advance(); //Consume the .

                while (isDigit(peek())) advance();

            }

            addToken(TokenType.NUMBER, double.Parse(source.Substring(start, current-start)));
        }



        private bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void stringScanner()
        {
            while (peek() != '"' && !isAtEnd()) //Checks if "" is given, in which case we do nothing.
            {
                if (peek() == '\n')
                { line++; }
                advance(); //Advance until next quotation mark
            }

            if (isAtEnd()) //Never got to the next quotation mark
            {
                Lox.error(line, "Unterminated string.");
                return;
            }

            advance(); //Last "

            //Trim quotes, add token
            string value = source.Substring(start + 1, (current-start) - 2);
            addToken(TokenType.STRING, value);
        }

        private char peek()
        {
            if (isAtEnd())
            {
                return '\0'; // '\n' ?
            }
            return source.ToCharArray()[current];
        }

        private char peekNext()
        {
            if (current + 1 >= source.Length) { return '\0'; } //If there's nothing next, return '\0'

            return source.ToCharArray()[current + 1];

        }

        private bool Match(char expected) //Conditional advance()
        {
            if (isAtEnd())
            { return false; }
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
            string text = source.Substring(start, current-start);
            tokens.Add(new Token(type, text, literal, line));
        }


    }
}