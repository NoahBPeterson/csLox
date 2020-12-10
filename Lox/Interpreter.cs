using Lox.NativeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Lox.Token;

namespace Lox
{
    public class Interpreter : Visitor<object>, Statement.Visitor<object>
    {
        public readonly Environment globals = new Environment();
        private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();
        private Environment environment;
        private bool _break = false;
        private bool _continue = false;
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
                    }else if(_continue && _loop)
                    {
                        _continue = false;
                        break; //Exits the loop, but doesn't prevent while loop from executing the body again.
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
                case TokenType.EQUALS_EQUALS:
                    if (left is double && right is double)
                    {
                        return (double)left == (double)right;
                    }
                    if (left is string && right is string)
                    {
                        return ((string)left).Equals((string)right);
                    }
                    return null;
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
                case TokenType.MINUS_MINUS:
                    checkNumberOperand(unaryExpr.operatorToken, right);
                    return (double) right - 1.0;
                case TokenType.PLUS_PLUS:
                    checkNumberOperand(unaryExpr.operatorToken, right);
                    return (double) right + 1.0;
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
            if(locals.TryGetValue(expr, out int distance))
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
            environment.define(varStmt.name.lexeme, value, varStmt.name);
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
            return value;
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
            if (stmt.doWhileLoop)
            {
                do
                {
                    _loop = true;
                    execute(stmt.body);
                    _continue = false;
                } while (isTruthy(evaluate(stmt.condition)) && !_break);
            }
            else
            {
                while (isTruthy(evaluate(stmt.condition)) && !_break)
                {
                    _loop = true;
                    execute(stmt.body);
                    _continue = false;
                }
            }
            _loop = false;
            _break = false;
            _continue = false;
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
                environment.define(func.name.lexeme, function, func.name);
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
            List<LoxClass> superClasses = new List<LoxClass>();
            if (classStatement.superclass.Count != 0)
            {
                foreach(Object superClass in classStatement.superclass)
                {
                    Object eval = evaluate((Expr.Variable)superClass);
                    if (!(eval is LoxClass))
                    {
                        throw new Exceptions.RuntimeError(new HelperFunctions.GetToken().evaluate((Expr)superClass), "Superclass must be a class.");
                    }
                    else
                    {
                        superClasses.Add((LoxClass)eval);
                    }
                }
            }

            environment.define(classStatement.name.lexeme, null, classStatement.name);

            if(classStatement.superclass.Count != 0)
            {
                environment = new Environment(environment);
                environment.define("super", superClasses);
            }

            Dictionary<string, LoxFunction> methods = new Dictionary<string, LoxFunction>();
            Dictionary<LoxFunction, Token> getters = new Dictionary<LoxFunction, Token>();
            List<Statement.function> methodsList = classStatement.methods;
            foreach (Statement.function method in methodsList)
            {
                if (method._params.Count == 1 && method._params.ElementAt(0).type == TokenType.SEMICOLON && method._params.ElementAt(0).lexeme.Equals("getter"))
                {
                    Statement.function noParams = new Statement.function(method.name, new List<Token>(), method.body);
                    LoxFunction getter = new LoxFunction(noParams, environment, false, true);
                    methods.Add(method.name.lexeme, getter);
                    getters[getter] = method.name;
                }
                else
                {
                    LoxFunction function = new LoxFunction(method, environment, method.name.lexeme.Equals("init"));
                    methods.Add(method.name.lexeme, function);
                }
            }
            LoxClass _class = new LoxClass(classStatement.name.lexeme, superClasses, methods);
            foreach(Statement.function staticFunction in classStatement.staticFunctions)
            {
                _class.set(staticFunction.name, staticFunction);
            }
            foreach(LoxFunction getter in getters.Keys)
            {
                _class.set(getters[getter], getter);
            }
            if(superClasses.Count != 0)
            {
                environment = environment.enclosing;
            }
            environment.assign(classStatement.name, _class);
            return null;
        }

        public object visitGetExpr(Expr.Get get)
        {
            Object _object = evaluate(get._object);
            if(_object is LoxInstance)
            {
                object result = ((LoxInstance)_object).get(get.name);
                if (result is LoxFunction)
                {
                    if (((LoxFunction)result).isGetter())
                    {
                        return ((LoxFunction)result).call(this, null);
                    }
                }

                return result;
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

        public object visitSuperExpr(Expr.Super super)
        {
            int distance = -1;
            locals.TryGetValue(super, out distance);
            //if (distance != -1)
            List<LoxClass> superClasses = (List<LoxClass>)environment.getAt(distance, "super");
            LoxInstance _object = (LoxInstance)environment.getAt(distance - 1, "this"); //May have to find a fix for this given my implementation of getAt().
            LoxFunction method = null;
            foreach(LoxClass superClass in superClasses)
            {
                method = superClass.findMethod(super.method.lexeme);
                if (method != null) break;
            }
            if (method == null)
            {
                throw new Exceptions.RuntimeError(super.method, "Undefined property '" + super.method.lexeme + "'.");
            }
            return method.bind(_object);
        }

        public object visitContinueStatement(Statement.continueStmt continueStatement)
        {
            _continue = true;
            return null;
        }

        public object visitPrefixExpr(Expr.prefix pf)
        {
            object value = evaluate(pf.expr);

            checkNumberOperand(pf.keyword, value);
            if (pf.keyword.type == TokenType.PLUS_PLUS)
            {
                value = ((double)value) + 1.0;
            }else
            {
                value = ((double)value) - 1.0;
            }
            int distance = -1;
            locals.TryGetValue(pf, out distance);
            if (distance != -1)
            {
                environment.assignAt(distance, new HelperFunctions.GetToken().evaluate(pf.expr), (double)value); //Assign value after change.
            }
            return value;
        }

        public object visitPostfixExpr(Expr.postfix pf)
        {
            object value = evaluate(pf.expr);

            checkNumberOperand(pf.keyword, value);
            object assignValue = value;
            if (pf.keyword.type == TokenType.PLUS_PLUS)
            {
                assignValue = ((double)value) + 1.0;
            }
            else
            {
                assignValue = ((double)value) - 1.0;
            }
            int distance = -1;
            locals.TryGetValue(pf, out distance);
            if (distance != -1)
            {
                environment.assignAt(distance, new HelperFunctions.GetToken().evaluate(pf.expr), (double)assignValue);
            }

            return value;
        }

        public object visitLambdaFunction(Expr.Lambda lambdaFunction)
        {
            LoxFunction function = new LoxFunction(lambdaFunction, environment, false);

            return function; //If the function is lambda
        }
    }
}
