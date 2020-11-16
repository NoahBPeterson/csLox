using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Lox.Token;

namespace Lox
{
    public class Interpreter : Visitor<Object>, Statement.Visitor<Object>
    {
        private Environment environment = new Environment();
        public void interpret(List<Statement> statements)
        {
            try
            {
                foreach (Statement statement in statements)
                {
                    execute(statement);
                }
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

        private void execute(Statement statement)
        {
            statement.accept<object>(this);
        }

        private void executeBlock(List<Statement> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (Statement statement in statements)
                {
                    execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
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
                    if ((double)right == 0) 
                        throw new RuntimeError(binaryExpr.operatorToken, "Cannot divide by zero.");
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

        object Statement.Visitor<object>.visitPrintStatement(Statement.Print printStmt)
        {
            Object value = evaluate(printStmt.expression);
            Console.WriteLine(stringify(value));
            return null;
        }

        object Statement.Visitor<object>.visitExprStatement(Statement.Expression exprStmt)
        {
            evaluate(exprStmt.expression);
            return null;
        }
        public object visitVariable(Expr.Variable variable)
        {
            return environment.get(variable.name);
        }

        public object visitVarStatement(Statement.Var varStmt)
        {
            Object value = null;
            if(varStmt.initializer != null)
            {
                value = evaluate(varStmt.initializer);
            }
            environment.define(varStmt.name.lexeme, value);
            return null;
        }

        public object visitAssignExpr(Expr.AssignExpr assignExpr)
        {
            Object value = evaluate(assignExpr.value);
            environment.assign(assignExpr.name, value);
            return null;
        }

        public object visitBlockStatement(Statement.Block blockStmt)
        {
            executeBlock(blockStmt.statements, new Environment(environment));
            return null;
        }
    }
}
