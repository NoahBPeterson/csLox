using Lox.NativeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Lox.Token;

namespace Lox
{
    public class Interpreter : Visitor<Object>, Statement.Visitor<Object>
    {
        public readonly Environment globals = new Environment();
        private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();
        private Environment environment;
        private bool _break = false;
        private bool _loop = false;

        public Interpreter()
        {
            environment = globals;
            globals.define("clock", new Clock());
        }
        public void interpret(List<Statement> statements)
        {
            try
            {
                foreach (Statement statement in statements)
                {
                    execute(statement);
                }
            }
            catch (Exceptions.RuntimeError error)
            {
                Lox.runtimeError(error);
            }
        }
        private object evaluate(Expr expr)
        {
            return expr.accept(this);
        }

        private void execute(Statement statement)
        {
            statement.accept<object>(this);
        }

        public void resolve(Expr expr, int depth)
        {
            locals.Add(expr, depth);
        }

        public void executeBlock(List<Statement> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (Statement statement in statements)
                {
                    if(_break && _loop)
                    {
                        break;
                    }else
                    {
                        execute(statement);
                    }
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public object visitBinaryExpr(Expr.BinaryExpr binaryExpr)
        {
            object left = evaluate(binaryExpr.left);
            object right = evaluate(binaryExpr.right);

            switch(binaryExpr.operatorToken.type)
            {
                case TokenType.COMMA:
                    return right;
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
                    if(left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }
                    if (left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }
                    if (left is double && right is string)
                    {
                        return ((double)left).ToString() + (string)right;
                    }
                    if (left is string && right is double)
                    {
                        return (string)left + ((double)right).ToString();
                    }
                    throw new Exceptions.RuntimeError(binaryExpr.operatorToken, "Operands must be numbers or strings.");
                case TokenType.FORWARD_SLASH:
                    checkNumberOperand(binaryExpr.operatorToken, left, right);
                    if ((double)right == 0) 
                        throw new Exceptions.RuntimeError(binaryExpr.operatorToken, "Cannot divide by zero.");
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
            object right = evaluate(unaryExpr.right);

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
        private void checkNumberOperand(Token _operator, object operand)
        {
            if (operand is double)
                return;
            throw new Exceptions.RuntimeError(_operator, "Operand must be a number.");
        }
        private void checkNumberOperand(Token _operator, object operand, object operandTwo)
        {
            if (operand is double && operandTwo is double)
                return;
            throw new Exceptions.RuntimeError(_operator, "Operand must be a number.");
        }

        private bool isTruthy(object objectA)
        {
            if (objectA == null) 
                return false;
            if (objectA is bool) 
                return (bool) objectA;
            return true;
        }
        private bool isEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private string stringify(object _object)
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
            object value = evaluate(printStmt.expression);
            Console.WriteLine(stringify(value));
            return null;
        }

        object Statement.Visitor<object>.visitExprStatement(Statement.Expression exprStmt)
        {
            return evaluate(exprStmt.expression);
        }
        public object visitVariable(Expr.Variable variable)
        {
            return lookUpVariable(variable.name, variable);
        }

        private Object lookUpVariable(Token name, Expr expr)
        {
            int distance = -1;
            locals.TryGetValue(expr, out distance);
            if(distance != -1)
            {
                return environment.getAt(distance, name.lexeme);
            } else
            {
                return globals.get(name);
            }
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
            object value = evaluate(assignExpr.value);

            int distance = -1;
            locals.TryGetValue(assignExpr, out distance);
            if(distance != -1)
            {
                environment.assignAt(distance, assignExpr.name, value);
            }else
            {
                globals.assign(assignExpr.name, value);
            }
            return null;
        }

        public object visitBlockStatement(Statement.Block blockStmt)
        {
            executeBlock(blockStmt.statements, new Environment(environment));
            return null;
        }

        public object visitIfStatement(Statement.ifStmt ifStmt)
        {
            if(isTruthy(evaluate(ifStmt.condition)))
            {
                execute(ifStmt.thenBranch);
            }else if(ifStmt.elseBranch != null)
            {
                execute(ifStmt.elseBranch);
            }
            return null;
        }

        public object visitLogicalExpr(Expr.logicalExpr logicalExpr)
        {
            object left = evaluate(logicalExpr.left);

            if (logicalExpr._operator.type == TokenType.OR)
            {
                if (isTruthy(left)) return left;
            }
            else
            {
                if (!isTruthy(left)) return left;
            }
            return evaluate(logicalExpr.right);
        }

        public object visitWhileStatement(Statement.whileStmt stmt)
        {
            while(isTruthy(evaluate(stmt.condition)) && !_break)
            {
                _loop = true;
                execute(stmt.body);
            }
            _loop = false;
            _break = false;
            return null;
        }

        public object visitBreakStatement(Statement.breakStmt breakStmt)
        {
            _break = true;
            return null;
        }

        public object visitCallExpr(Expr.Call call)
        {
            Object callee = evaluate(call.callee);

            List<Object> arguments = new List<Object>();
            foreach (Object argument in call.expressionArguments)
            {
                if (argument is Expr)
                {
                    arguments.Add(evaluate((Expr) argument));
                } else if(argument is Statement.function)
                {
                    arguments.Add((Statement.function)argument);
                }
            }

            if (!(callee is LoxCallable) && !(callee is Statement.function))
            {
                throw new Exceptions.RuntimeError(call.paren, "Can only call functions and classes.");
            }else if(callee is Statement.function)
            {
                Statement.function staticFunction = (Statement.function) callee;
                LoxFunction classStaticFunction = new LoxFunction(staticFunction, globals, false);
                return classStaticFunction.call(this, arguments);
            }
            LoxCallable function = (LoxCallable)callee;
            if(arguments.Count != function.arity())
            {
                throw new Exceptions.RuntimeError(call.paren, "Expected " +
                    function.arity() + " arguments but got " +
                    arguments.Count + ".");
            }
            return function.call(this, arguments);
        }

        public object visitFunction(Statement.function func)
        {
            LoxFunction function = new LoxFunction(func, environment, false);
            if (func.name != null) //If the function is named
            {
                environment.define(func.name.lexeme, function);
                return null;
            }

            return function; //If the function is lambda
        }

        public object visitReturnStatement(Statement.Return returnStmt)
        {
            Object value = null;
            if (returnStmt.value != null) value = evaluate(returnStmt.value);

            throw new Exceptions.Return(value);
        }

        public object visitClassStatement(Statement.Class classStatement)
        {
            environment.define(classStatement.name.lexeme, null);

            Dictionary<string, LoxFunction> methods = new Dictionary<string, LoxFunction>();
            foreach(Statement.function method in classStatement.methods)
            {
                LoxFunction function = new LoxFunction(method, environment, method.name.lexeme.Equals("init"));
                methods.Add(method.name.lexeme, function);
            }
            LoxClass _class = new LoxClass(classStatement.name.lexeme, methods);
            foreach(Statement.function staticFunction in classStatement.staticFunctions)
            {
                _class.set(staticFunction.name, staticFunction);
            }
            environment.assign(classStatement.name, _class);
            return null;
        }

        public object visitGetExpr(Expr.Get get)
        {
            Object _object = evaluate(get._object);
            if(_object is LoxInstance)
            {
                return ((LoxInstance)_object).get(get.name);
            }

            throw new Exceptions.RuntimeError(get.name, "Only instances have properties.");
        }

        public object visitSetExpr(Expr.Set set)
        {
            Object _object = evaluate(set._object);

            if(!(_object is LoxInstance))
            {
                throw new Exceptions.RuntimeError(set.name, "Only instances have fields.");
            }

            Object value = evaluate(set.value);
            ((LoxInstance)_object).set(set.name, value);
            return value;
        }

        public object visitThisExpr(Expr.This _this)
        {
            return lookUpVariable(_this.keyword, _this);
        }
    }
}
