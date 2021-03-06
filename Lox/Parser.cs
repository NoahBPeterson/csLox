﻿using System;
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
        private bool parsingLoop = false;
        private bool inBlock = false;
        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Statement> parse()
        {
            List<Statement> statements = new List<Statement>();
            while (!isAtEnd())
            {
                statements.Add(declaration());
            }
            return statements;
        }

        private Expr expression()
        {
            Expr expr = assignment();


            return expr;
        }

        private Statement declaration()
        {
            try
            {
                if (match(TokenType.CLASS))
                    return classDeclaration();
                if (match(TokenType.FUNC))
                    return function("function");
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

        private Statement classDeclaration()
        {
            Token name = consume(TokenType.IDENTIFIER, "Expect class name.");

            List<Expr.Variable> superClasses = new List<Expr.Variable>();
            if (match(TokenType.LESS_THAN))
            {
                do
                {
                    consume(TokenType.IDENTIFIER, "Expect superclass name.");
                    superClasses.Add(new Expr.Variable(previous()));
                } while (match(TokenType.COMMA));
            }



            consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");

            List<Statement.function> methods = new List<Statement.function>();
            List<Statement.function> staticFunctions = new List<Statement.function>();
            while (!check(TokenType.RIGHT_BRACE) && !isAtEnd())
            {
                if (match(TokenType.CLASS))
                {
                    staticFunctions.Add(function("static"));
                }
                else
                {
                    methods.Add(function("method"));
                }
            }

            consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");
            return new Statement.Class(name, superClasses, methods, staticFunctions);
        }

        private Statement statement()
        {
            if (match(TokenType.PRINT)) return printStatement();
            if (match(TokenType.LEFT_BRACE)) return new Statement.Block(block());
            if (match(TokenType.IF)) return ifStatement();
            if (match(TokenType.RETURN)) return returnStatement();
            if (match(TokenType.DO)) return doWhileStatement();
            if (match(TokenType.WHILE)) return whileStatement();
            if (match(TokenType.FOR)) return forStatement();
            if (match(TokenType.BREAK))
            {
                if (parsingLoop)
                {
                    consume(TokenType.SEMICOLON, "Expected ';' after break statement.");
                    return new Statement.breakStmt(previous());
                }
                else
                {
                    throw error(previous(), "Cannot use break statement outside of loop structure.");
                }
            }
            if (match(TokenType.CONTINUE))
            {
                if (parsingLoop)
                {
                    consume(TokenType.SEMICOLON, "Expected ';' after continue statement.");
                    return new Statement.continueStmt(previous());
                }
                else
                {
                    throw error(previous(), "Cannot use continue statement outside of loop structure.");
                }
            }
            return expressionStatement();
        }

        private Statement forStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");
            Statement initializer;
            if (match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (match(TokenType.VAR))
            {
                initializer = varDeclaration();
            }
            else
            {
                initializer = expressionStatement();
            }

            Expr condition = null;
            if (!check(TokenType.SEMICOLON))
            {
                condition = expression();
            }
            consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!check(TokenType.RIGHT_PAREN))
            {
                increment = expression();
            }
            consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");
            parsingLoop = true;
            Statement body = statement();
            parsingLoop = false;
            if (increment != null)
            {
                body = new Statement.Block(new List<Statement> { body, new Statement.Expression(increment) });
            }

            if (condition == null) condition = new Expr.Literal(true, previous());
            body = new Statement.whileStmt(condition, body, false);

            if (initializer != null)
            {
                body = new Statement.Block(new List<Statement> { initializer, body });
            }

            return body;
        }

        private Statement ifStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            Statement thenBranch = statement();
            Statement elseBranch = null;
            if (match(TokenType.ELSE))
            {
                elseBranch = statement();
            }

            return new Statement.ifStmt(condition, thenBranch, elseBranch);
        }

        private Statement printStatement()
        {
            Expr value = expression();
            consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Statement.Print(value);
        }

        private Statement returnStatement()
        {
            Token keyword = previous();
            Expr value = null;
            if (!check(TokenType.SEMICOLON))
            {
                value = expression();
            }
            consume(TokenType.SEMICOLON, "Expect ';' after return value.");
            return new Statement.Return(keyword, value);
        }

        private Statement varDeclaration()
        {
            Token name = consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (match(TokenType.EQUALS))
            {
                initializer = expression();
            }
            while (match(TokenType.COMMA))
            {
                Token _operator = previous();
                Expr right = term();
                initializer = new Expr.BinaryExpr(initializer, _operator, right);
            }
            consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Statement.Var(name, initializer);
        }

        private Statement doWhileStatement()
        {
            parsingLoop = true;
            Statement body = statement();
            parsingLoop = false;
            consume(TokenType.WHILE, "Expect 'while' at the end of the do-while loop body.");
            consume(TokenType.LEFT_PAREN, "Expct '(' after 'while'.");
            Expr condition = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            consume(TokenType.SEMICOLON, "Expected ';' after while statement.");
            return new Statement.whileStmt(condition, body, true);
        }

        private Statement whileStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expct '(' after 'while'.");
            Expr condition = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            parsingLoop = true;
            Statement body = statement();
            parsingLoop = false;
            return new Statement.whileStmt(condition, body, false);
        }

        private Statement expressionStatement()
        {
            Expr expr = expression();
            if (expr is Expr.AssignExpr || expr is Expr.Call || expr is Expr.Set || expr is Expr.postfix || expr is Expr.prefix || expr is Expr.Variable || expr is Expr.Literal || expr is Expr.Super || expr is Expr.This) //Take out Literal
            {
                consume(TokenType.SEMICOLON, "Expect ';' after expression.");
                return new Statement.Expression(expr);
            }
            if((expr is Expr.TernaryExpr && ((Expr.TernaryExpr)expr).isNullAccessor)) //Optional semicolon, but doesn't print after evaluation.
            {
                match(TokenType.SEMICOLON);
                return new Statement.Expression(expr);
            }
            if (!inBlock)
            {
                match(TokenType.SEMICOLON);
                return new Statement.Print(expr);
            }
            throw error(previous(), "Cannot use REPL inside a body."+expr.GetType());
        }

        private Statement.function function(String kind)
        {
            Token name = consume(TokenType.IDENTIFIER, "Expect " + kind + " name.");

            if (kind.Equals("method") && match(TokenType.LEFT_BRACE))
            {
                List<Statement> bodyStmts = block();
                List<Token> sentinelParam = new List<Token>();
                sentinelParam.Add(new Token(TokenType.SEMICOLON, "getter", null, name.line, name.characters)); //Make an impossible Token
                return new Statement.function(name, sentinelParam, bodyStmts);
            }
            consume(TokenType.LEFT_PAREN, "Expect '(' after " + kind + " name.");
            List<Token> parameters = new List<Token>();
            if (!check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        throw error(peek(), "Can't have more than 255 parameters.");
                    }
                    parameters.Add(consume(TokenType.IDENTIFIER, "Expect parameter name."));
                } while (match(TokenType.COMMA));
            }
            consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
            consume(TokenType.LEFT_BRACE, "Expect '{' before " + kind + " body.");
            List<Statement> body = block();
            return new Statement.function(name, parameters, body);
        }

        private List<Statement> block()
        {
            List<Statement> statements = new List<Statement>();

            bool previousBlock = inBlock;
            inBlock = true;
            while (!check(TokenType.RIGHT_BRACE) && !isAtEnd())
            {
                statements.Add(declaration());
            }
            inBlock = previousBlock;

            consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Expr assignment()
        {
            Expr expr = ternaryExpression();

            if (match(TokenType.EQUALS))
            {
                Token equals = previous();
                Expr value = assignment();

                if (expr is Expr.Variable)
                {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.AssignExpr(name, value);
                }
                else if (expr is Expr.Get)
                {
                    Expr.Get get = (Expr.Get)expr;
                    return new Expr.Set(get._object, get.name, value, get.isCascading);
                }
                throw error(equals, "Invalid assignment target: " + expr.GetType());
            }
            return expr;
        }

        private Expr ternaryExpression()
        {
            Expr expr = isNull(); //Comparison expression
            Expr trueExpression; //Result if comparison is true

            if (match(TokenType.TERNARY_QUESTION))
            {
                trueExpression = isNull();
                if (match(TokenType.TERNARY_COLON))
                {
                    Expr falseExpression = isNull(); //Result if comparison is false.
                    Expr temp = expr;
                    expr = new Expr.TernaryExpr(temp, trueExpression, falseExpression);

                }
            }

            return expr;
        }

        private Expr isNull()
        {
            Expr expr = or();

            if(match(TokenType.QUESTION_QUESTION))
            {
                Token qq = previous();
                Expr right = ternaryExpression();
                expr = new Expr.BinaryExpr(expr, qq, right);
            }

            return expr;
        }

        private Expr or()
        {
            Expr expr = and();

            while (match(TokenType.OR))
            {
                Token _operator = previous();
                Expr right = and();
                expr = new Expr.logicalExpr(expr, _operator, right);
            }

            return expr;
        }

        private Expr and()
        {
            Expr expr = equality();

            while (match(TokenType.AND))
            {
                Token _operator = previous();
                Expr right = ternaryExpression();
                expr = new Expr.logicalExpr(expr, _operator, right);
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

            while (match(TokenType.MINUS, TokenType.PLUS))
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

            while (match(TokenType.FORWARD_SLASH, TokenType.ASTERISK))
            {
                Token _operator = previous();
                Expr right = unary();
                expr = new Expr.BinaryExpr(expr, _operator, right);
            }
            return expr;
        }

        private Expr unary()
        {
            if (match(TokenType.EXCLAMATION, TokenType.MINUS))
            {
                Token _operator = previous();
                Expr right = unary();
                return new Expr.UnaryExpr(_operator, right);
            }
            return prefix();
        }

        private Expr prefix()
        {
            if (match(TokenType.MINUS_MINUS, TokenType.PLUS_PLUS))
            {
                Token _operator = previous();
                Expr pre = primary();
                return new Expr.prefix(_operator, pre);
            }
            return postfix();
        }

        private Expr postfix()
        {
            Expr prim = call();
            if (prim is Expr.Call) return prim;
            if (match(TokenType.MINUS_MINUS, TokenType.PLUS_PLUS))
            {
                Token _operator = previous();
                return new Expr.postfix(_operator, prim);
            }
            return prim;
        }

        private Expr finishCall(Expr callee)
        {
            List<Object> arguments = new List<Object>();
            if (!check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        throw error(peek(), "Can't have more than 255 arguments.");
                    }
                    arguments.Add(expression());
                } while (match(TokenType.COMMA));
            }

            Token paren = consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

            return new Expr.Call(callee, paren, arguments);
        }

        private Expr call()
        {
            Expr expr = primary();

            while (true)
            {
                if (match(TokenType.LEFT_PAREN))
                {
                    expr = finishCall(expr);
                }
                else if (match(TokenType.DOT))
                {
                    Token name = consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                    expr = new Expr.Get(expr, name, false);
                }else if (match(TokenType.QUESTION_DOT))
                {
                    Token nil = new Token(TokenType.NIL, "nil", null, -1, -1);
                    Expr callIfNotNull = new Expr.Get(expr, consume(TokenType.IDENTIFIER, "Expect property after '?."), false);
                    if(match(TokenType.LEFT_PAREN))
                    {
                        callIfNotNull = finishCall(callIfNotNull);
                    }
                    expr = new Expr.TernaryExpr(
                        new Expr.BinaryExpr(expr, new Token(TokenType.EQUALS_EQUALS, "==", null, -1, -1), new Expr.Literal(null, nil)), // ?
                        new Expr.Literal(null, nil), // :
                        callIfNotNull); // new Expr.Get(expr, consume(TokenType.IDENTIFIER, "Expect property after '?."), false));
                    ((Expr.TernaryExpr)expr).isNullAccessor = true;
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        private Expr primary()
        {
            if (match(TokenType.FALSE)) return new Expr.Literal(false, previous());
            if (match(TokenType.TRUE)) return new Expr.Literal(true, previous());
            if (match(TokenType.NIL)) return new Expr.Literal(null, previous());


            if (match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(previous().literal, previous());
            }

            if (match(TokenType.LEFT_PAREN))
            {
                Expr expr = expression();
                consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            if (match(TokenType.SUPER_CLASS))
            {
                Token keyword = previous();
                consume(TokenType.DOT, "Expect '.' after 'super'.");
                Token method = consume(TokenType.IDENTIFIER, "Expect superclass method name.");
                return new Expr.Super(keyword, method);
            }

            if (match(TokenType.THIS_OBJECT)) return new Expr.This(previous());

            if (match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(previous());
            }

            if (match(TokenType.FUNC))
            {
                Token name = previous();

                if (match(TokenType.LEFT_PAREN))
                {
                    List<Token> parameters = new List<Token>();
                    if (!check(TokenType.RIGHT_PAREN))
                    {
                        do
                        {
                            if (parameters.Count >= 255)
                            {
                                throw error(peek(), "Can't have more than 255 parameters.");
                            }
                            parameters.Add(consume(TokenType.IDENTIFIER, "Expect parameter name."));
                        } while (match(TokenType.COMMA));
                    }
                    consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
                    consume(TokenType.LEFT_BRACE, "Expect '{' before function body.");
                    List<Statement> body = block();
                    return new Expr.Lambda(name, parameters, body);
                }
                else
                {
                    throw error(peek(), "Expect '(' after function declaration.");
                }

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
                if (check(type))
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

            while (!isAtEnd())
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
                advance();
            }
        }
    }
}
