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

        public Expr parse()
        {
            try
            {
                return expression();
            }
            catch (ParseError)
            {
                return null;
            }
        }

        private Expr expression()
        {
            Expr expr = ternaryExpression();

            while (match(TokenType.COMMA))
            {
                Token _operator = previous();
                Expr right = term();
                expr = new Expr.BinaryExpr(expr, _operator, right);
            }

            return expr;
        }

        private Expr ternaryExpression()
        {
            Expr expr = equality(); //Comparison expression
            Expr trueExpression; //Result if comparison is true

            if (match(TokenType.TERNARY_QUESTION))
            {
                trueExpression = equality();
                if(match(TokenType.TERNARY_COLON))
                {
                    Expr falseExpression = equality(); //Result if comparison is false.
                    Expr temp = expr;
                    expr = new Expr.TernaryExpr(temp, trueExpression, falseExpression);

                }
            }

            return expr;
        }

        private Expr equality()
        {
            Expr expr = comparison();

            while (match(TokenType.EXCLAMATION_EQUALS, TokenType.EQUALS_EQUALS))
            {
                Token _operator = previous();
                Expr right = comparison();
                expr = new Expr.BinaryExpr(expr, _operator, right);
            }
            return expr;
        }

        private Expr comparison()
        {
            Expr expr = term();

            while (match(TokenType.GREATER_THAN, TokenType.GREATER_THAN_EQUALS, TokenType.LESS_THAN, TokenType.LESS_THAN_EQUALS))
            {
                Token _operator = previous();
                Expr right = term();
                expr = new Expr.BinaryExpr(expr, _operator, right);
            }
            return expr;
        }

        private Expr term()
        {
            Expr expr = factor();

            while(match(TokenType.MINUS, TokenType.PLUS))
            {
                Token _operator = previous();
                Expr right = factor();
                expr = new Expr.BinaryExpr(expr, _operator, right);
            }
            return expr;
        }

        private Expr factor()
        {
            Expr expr = unary();
            
            while(match(TokenType.FORWARD_SLASH, TokenType.ASTERISK))
            {
                Token _operator = previous();
                Expr right = unary();
                expr = new Expr.BinaryExpr(expr, _operator, right);
            }
            return expr;
        }

        private Expr unary()
        {
            if(match(TokenType.EXCLAMATION, TokenType.MINUS))
            {
                Token _operator = previous();
                Expr right = unary();
                return new Expr.UnaryExpr(_operator, right);
            }
            return primary();
        }

        private Expr primary()
        {
            if (match(TokenType.FALSE)) return new Expr.Literal(false);
            if (match(TokenType.TRUE)) return new Expr.Literal(true);
            if (match(TokenType.NIL)) return new Expr.Literal(null);

            if(match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(previous().literal);
            }

            if(match(TokenType.LEFT_PAREN))
            {
                Expr expr = expression();
                consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
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
