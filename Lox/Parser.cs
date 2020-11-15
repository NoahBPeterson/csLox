using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Lox.Token;

namespace Lox
{
    public class Parser
    {
        private class ParseError : Exception
        {
            public ParseError() { }
        }
        private List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Statement> parse()
        {
            List<Statement> statements = new List<Statement>();
            while(!isAtEnd())
            {
                statements.Add(declaration());
            }
            return statements;
        }

        private Expr expression()
        {
            Expr expr = assignment();

            while (match(TokenType.COMMA))
            {
                Token _operator = previous();
                Expr right = term();
                expr = new Expr.BinaryExpr(expr, _operator, right);
            }

            return expr;
        }

        private Statement declaration()
        {
            try
            {
                if (match(TokenType.VAR))
                    return varDeclaration();
                return statement();
            }
            catch (ParseError)
            {
                synchronize();
                return null;
            }
        }

        private Statement statement()
        {
            if(match(TokenType.PRINT))
            {
                return printStatement();
            }
            if(match(TokenType.LEFT_BRACE))
            {
                return new Statement.Block(block());
            }
            return expressionStatement();
        }

        private Statement printStatement()
        {
            Expr value = expression();
            consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Statement.Print(value);
        }

        private Statement varDeclaration()
        {
            Token name = consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if(match(TokenType.EQUALS))
            {
                initializer = expression();
            }
            consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Statement.Var(name, initializer);
        }

        private Statement expressionStatement()
        {
            Expr expr = expression();
            consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Statement.Expression(expr);
        }

        private List<Statement> block()
        {
            List<Statement> statements = new List<Statement>();

            while(!check(TokenType.RIGHT_BRACE) && !isAtEnd())
            {
                statements.Add(declaration());
            }

            consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Expr assignment()
        {
            Expr expr = ternaryExpression();

            if(match(TokenType.EQUALS))
            {
                Token equals = previous();
                Expr value = assignment();

                if(expr.GetType() == typeof(Expr.Variable))
                {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.AssignExpr(name, value);
                }
                error(equals, "Invalid assignment target.");
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

            if(match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(previous());
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
