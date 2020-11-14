using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Lox.Token;

namespace Lox
{
    public class Parser<T>
    {
        private class ParseError : Exception
        {

        }
        private List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public Expr<T> parse()
        {
            try
            {
                return expression();
            }
            catch (ParseError _)
            {
                return null;
            }
        }

        private Expr<T> expression()
        {
            Expr<T> expr = ternaryExpression();

            while (match(TokenType.COMMA))
            {
                Token _operator = previous();
                Expr<T> right = term();
                expr = new Expr<T>.BinaryExpr(expr, _operator, right);
            }

            return expr;
        }

        private Expr<T> ternaryExpression()
        {
            Expr<T> expr = equality(); //Comparison expression
            Expr<T> trueExpression; //Result if comparison is true

            if (match(TokenType.TERNARY_QUESTION))
            {
                trueExpression = equality();
                if(match(TokenType.TERNARY_COLON))
                {
                    Expr<T> falseExpression = equality(); //Result if comparison is false.
                    Expr<T> temp = expr;
                    expr = new Expr<T>.TernaryExpr(temp, trueExpression, falseExpression);

                }
            }

            return expr;
        }

        private Expr<T> equality()
        {
            Expr<T> expr = comparison();

            while (match(TokenType.EXCLAMATION_EQUALS, TokenType.EQUALS_EQUALS))
            {
                Token _operator = previous();
                Expr<T> right = comparison();
                expr = new Expr<T>.BinaryExpr(expr, _operator, right);
            }
            return expr;
        }

        private Expr<T> comparison()
        {
            Expr<T> expr = term();

            while (match(TokenType.GREATER_THAN, TokenType.GREATER_THAN_EQUALS, TokenType.LESS_THAN, TokenType.LESS_THAN_EQUALS))
            {
                Token _operator = previous();
                Expr<T> right = term();
                expr = new Expr<T>.BinaryExpr(expr, _operator, right);
            }
            return expr;
        }

        private Expr<T> term()
        {
            Expr<T> expr = factor();

            while(match(TokenType.MINUS, TokenType.PLUS))
            {
                Token _operator = previous();
                Expr<T> right = factor();
                expr = new Expr<T>.BinaryExpr(expr, _operator, right);
            }
            return expr;
        }

        private Expr<T> factor()
        {
            Expr<T> expr = unary();
            
            while(match(TokenType.FORWARD_SLASH, TokenType.ASTERISK))
            {
                Token _operator = previous();
                Expr<T> right = unary();
                expr = new Expr<T>.BinaryExpr(expr, _operator, right);
            }
            return expr;
        }

        private Expr<T> unary()
        {
            if(match(TokenType.EXCLAMATION, TokenType.MINUS))
            {
                Token _operator = previous();
                Expr<T> right = unary();
                return new Expr<T>.UnaryExpr(_operator, right);
            }
            return primary();
        }

        private Expr<T> primary()
        {
            if (match(TokenType.FALSE)) return new Expr<T>.Literal(false);
            if (match(TokenType.TRUE)) return new Expr<T>.Literal(true);
            if (match(TokenType.NIL)) return new Expr<T>.Literal(null);

            if(match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr<T>.Literal(previous().literal);
            }

            if(match(TokenType.LEFT_PAREN))
            {
                Expr<T> expr = expression();
                consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr<T>.Grouping(expr);
            }

            //If we find a binary operator but no left binary expression, consume the right-hand expression and throw an error.
            if (match(TokenType.EQUALS_EQUALS, TokenType.EXCLAMATION_EQUALS, TokenType.GREATER_THAN, TokenType.GREATER_THAN_EQUALS,
                      TokenType.LESS_THAN, TokenType.LESS_THAN_EQUALS, TokenType.FORWARD_SLASH, TokenType.ASTERISK, TokenType.MINUS, TokenType.PLUS))
            {
                Token peekToken = previous();
                expression(); //Discard right-hand expression.
                throw error(peekToken, "Expected expression before binary operator.");
            }

            throw error(peek(), "Expect expression.");
        }

        private Boolean match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if(check(type))
                {
                    advance();
                    return true;
                }
            }
            return false;
        }

        private Token consume(TokenType type, String message)
        {
            if (check(type)) return advance();

            throw error(peek(), message);
        }

        private Boolean check(TokenType type)
        {
            if (isAtEnd()) return false;
            return peek().type == type;
        }

        private Token advance()
        {
            if (!isAtEnd()) current++;
            return previous();
        }

        private Boolean isAtEnd()
        {
            return peek().type == TokenType.EOF;
        }

        private Token peek()
        {
            return tokens.ElementAt(current);
        }

        private Token previous()
        {
            return tokens.ElementAt(current - 1);
        }

        private ParseError error(Token token, String message)
        {
            Lox.error(token, message);
            return new ParseError();
        }

        private void synchronize()
        {
            advance();

            while(!isAtEnd())
            {
                if (previous().type == TokenType.SEMICOLON) return;

                switch (peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUNC:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }
            }
            advance();
        }
    }
}
