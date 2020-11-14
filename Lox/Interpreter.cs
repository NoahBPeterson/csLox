﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Lox.Token;

namespace Lox
{
    public class Interpreter : Visitor<Object>
    {
        public void interpret(Expr expression)
        {
            try
            {
                Object value = evaluate(expression);
                Console.WriteLine(stringify(value));
            }
            catch (RuntimeError error)
            {
                Lox.runtimeError(error);
            }
        }
        private Object evaluate(Expr expr)
        {
            return expr.accept(this);
        }

        public object visitBinaryExpr(Expr.BinaryExpr binaryExpr)
        {
            Object left = evaluate(binaryExpr.left);
            Object right = evaluate(binaryExpr.right);

            switch(binaryExpr.operatorToken.type)
            {
                case TokenType.GREATER_THAN:
                    checkNumberOperand(binaryExpr.operatorToken, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_THAN_EQUALS:
                    checkNumberOperand(binaryExpr.operatorToken, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS_THAN:
                    checkNumberOperand(binaryExpr.operatorToken, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_THAN_EQUALS:
                    checkNumberOperand(binaryExpr.operatorToken, left, right);
                    return (double)left <= (double)right;
                case TokenType.EXCLAMATION:
                    return isEqual(left, right);
                case TokenType.EXCLAMATION_EQUALS:
                    return !isEqual(left, right);
                case TokenType.MINUS:
                    checkNumberOperand(binaryExpr.operatorToken, left, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    if(left.GetType() == typeof(double) && right.GetType() == typeof(double))
                    {
                        return (double)left + (double)right;
                    }
                    if (left.GetType() == typeof(string) && right.GetType() == typeof(string))
                    {
                        return (string)left + (string)right;
                    }
                    if (left.GetType() == typeof(double) && right.GetType() == typeof(string))
                    {
                        return ((double)left).ToString() + (string)right;
                    }
                    if (left.GetType() == typeof(string) && right.GetType() == typeof(double))
                    {
                        return (string)left + ((double)right).ToString();
                    }
                    throw new RuntimeError(binaryExpr.operatorToken, "Operands must be numbers or strings.");
                case TokenType.FORWARD_SLASH:
                    checkNumberOperand(binaryExpr.operatorToken, left, right);
                    return (double)left / (double)right;
                case TokenType.ASTERISK:
                    checkNumberOperand(binaryExpr.operatorToken, left, right);
                    return (double)left * (double)right;
            }

            return null;
        }

        public object visitGroupingExpr(Expr.Grouping grouping)
        {
            return evaluate(grouping.expression);
        }

        public object visitLiteralExpr(Expr.Literal literal)
        {
            return literal.literal;
        }

        public object visitTernaryExpr(Expr.TernaryExpr ternaryExpr)
        {
            bool comparisonExpression = (bool)evaluate(ternaryExpr.comparisonExpression);

            if(comparisonExpression)
            {
                return evaluate(ternaryExpr.trueExpression);
            }else
            {
                return evaluate(ternaryExpr.falseExpression);
            }

            throw new NotImplementedException();
        }

        public object visitUnaryExpr(Expr.UnaryExpr unaryExpr)
        {
            Object right = evaluate(unaryExpr.right);

            switch (unaryExpr.operatorToken.type)
            {
                case TokenType.EXCLAMATION:
                    return !isTruthy(right);
                case TokenType.MINUS:
                    checkNumberOperand(unaryExpr.operatorToken, right);
                    return -(double)right;
            }
            return null;
        }
        private void checkNumberOperand(Token _operator, Object operand)
        {
            if (operand.GetType() == typeof(double))
                return;
            throw new RuntimeError(_operator, "Operand must be a number.");
        }
        private void checkNumberOperand(Token _operator, Object operand, Object operandTwo)
        {
            if (operand.GetType() == typeof(double) && operandTwo.GetType() == typeof(double))
                return;
            throw new RuntimeError(_operator, "Operand must be a number.");
        }

        private bool isTruthy(Object objectA)
        {
            if (objectA == null) 
                return false;
            if (objectA.GetType() == typeof(bool)) 
                return (bool) objectA;
            return true;
        }
        private bool isEqual(Object a, Object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private string stringify(Object _object)
        {
            if (_object == null) return "nil";

            if (_object.GetType() == typeof(double)) {
                string text = _object.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return _object.ToString();
        }
    }
}
