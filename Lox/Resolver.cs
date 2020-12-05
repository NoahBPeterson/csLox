using Lox.NativeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Lox.Token;

namespace Lox
{
    public class Resolver : Visitor<object>, Statement.Visitor<object>
    {
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, Variable>> scopes = new Stack<Dictionary<string, Variable>>();
        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;
        private bool isInLoop = false;
        private bool returned = false;

        public Resolver(Interpreter i)
        {
            interpreter = i;
        }

        private enum FunctionType { NONE, FUNCTION, INITIALIZER, METHOD }
        private enum ClassType { NONE, CLASS, SUBCLASS }

        public void resolve(List<Statement> statements)
        {
            foreach(Statement stmt in statements)
            {
                if (returned)
                {
                    returned = false;
                    Token name = new HelperFunctions.GetToken().evaluate(stmt);
                    Lox.warn(name, "Unreachable code placed after return statement.");
                }
                resolve(stmt);
            }
            if(returned) returned = false;
        }

        private void resolveFunction(Statement.function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;
            beginScope();
            foreach (Token _param in function._params)
            {
                declare(_param);
                define(_param);
            }
            resolve(function.body);
            endScope();
            currentFunction = enclosingFunction;
        }

        private void beginScope()
        {
            scopes.Push(new Dictionary<string, Variable>());
        }

        private void endScope()
        {
            foreach(string variable in scopes.Peek().Keys)
            {
                if(!scopes.Peek()[variable].used && !variable.Equals("this")) //If the variable has not been initialized within the scope, issue a warning.
                {
                    Lox.warn(scopes.Peek()[variable].name, "Variable has not been used.");
                }
            }
            foreach(string variable in scopes.Peek().Keys)
            {
                if(!scopes.Peek()[variable].initialized)
                {
                    Lox.warn(scopes.Peek()[variable].name, "Variable has not been initialized.");
                }
            }
            scopes.Pop();
        }

        private void declare(Token name)
        {
            if(scopes.Count == 0) return;

            Dictionary<string, Variable> scope = scopes.Peek();
            if(scope.ContainsKey(name.lexeme))
            {
                Lox.error(name, "A variable with this name already exists in this scope.");
            }
            scope[name.lexeme] = new Variable(name);
            scope[name.lexeme].initialized = false;
        }

        private void define(Token name)
        {
            if (scopes.Count == 0) return;
            scopes.Peek()[name.lexeme].initialized = true;
        }

        private void resolveLocal(Expr expr, Token name)
        {
            for(int i = scopes.Count-1; i >= 0; i--)
            {
                if(scopes.ElementAt(i).ContainsKey(name.lexeme))
                {
                    interpreter.resolve(expr, scopes.Count - 1 - i);
                    scopes.ElementAt(i)[name.lexeme].used = true;
                    return;
                }
            }
        }

        public object visitAssignExpr(Expr.AssignExpr assignExpr)
        {
            resolve(assignExpr.value);
            resolveLocal(assignExpr, assignExpr.name);
            return null;
        }

        public object visitBinaryExpr(Expr.BinaryExpr binaryExpr)
        {
            resolve(binaryExpr.left);
            resolve(binaryExpr.right);
            return null;
        }

        public object visitBlockStatement(Statement.Block blockStmt)
        {
            beginScope();
            resolve(blockStmt.statements);
            endScope();
            return null;
        }

        void resolve(Statement stmt)
        {
            stmt.accept(this);
        }

        private void resolve(Expr expr)
        {
            expr.accept(this);
        }

        public object visitBreakStatement(Statement.breakStmt breakStmt)
        {
            if (!isInLoop) Lox.error(breakStmt.keyword, "Cannot use break statement outside of a loop.");
            return null;
        }

        public object visitCallExpr(Expr.Call call)
        {
            resolve(call.callee);

            foreach (Object arg in call.expressionArguments)
            {
                if (arg is Expr) resolve((Expr)arg);
                else if (arg is Statement.function) resolve((Statement.function)arg);
            }
            return null;
        }

        public object visitExprStatement(Statement.Expression exprStmt)
        {
            resolve(exprStmt.expression);
            return null;
        }

        public object visitFunction(Statement.function func)
        {
            declare(func.name);
            define(func.name);

            resolveFunction(func, FunctionType.FUNCTION);
            return null;
        }

        public object visitGroupingExpr(Expr.Grouping grouping)
        {
            resolve(grouping.expression);
            return null;
        }

        public object visitIfStatement(Statement.ifStmt ifStmt)
        {
            resolve(ifStmt.condition);
            resolve(ifStmt.thenBranch);
            if (ifStmt.elseBranch != null) resolve(ifStmt.elseBranch);
            return null;
        }

        public object visitLiteralExpr(Expr.Literal literal)
        {
            return null;
        }

        public object visitLogicalExpr(Expr.logicalExpr logicalExpr)
        {
            resolve(logicalExpr.left);
            resolve(logicalExpr.right);
            return null;
        }

        public object visitPrintStatement(Statement.Print printStmt)
        {
            resolve(printStmt.expression);
            return null;
        }

        public object visitReturnStatement(Statement.Return returnStmt)
        {
            if(currentFunction == FunctionType.NONE)
            {
                Lox.error(returnStmt.keyword, "Can't return from top-level code.");
            }
            if(returnStmt.value != null)
            {
                if(currentFunction == FunctionType.INITIALIZER)
                {
                    Lox.error(returnStmt.keyword, "Can't return a value from an initializer.");
                }
                resolve(returnStmt.value);
            }
            returned = true;
            return null;
        }

        public object visitTernaryExpr(Expr.TernaryExpr ternaryExpr)
        {
            resolve(ternaryExpr.comparisonExpression);
            resolve(ternaryExpr.falseExpression);
            resolve(ternaryExpr.trueExpression);
            return null;
        }

        public object visitUnaryExpr(Expr.UnaryExpr unaryExpr)
        {
            resolve(unaryExpr.right);
            return null;
        }

        public object visitVariable(Expr.Variable variable)
        {
            if (scopes.Count != 0)
            {
                Variable value = null;
                scopes.Peek().TryGetValue(variable.name.lexeme, out value);
                if (scopes.Peek().ContainsKey(variable.name.lexeme) && value.initialized == false)
                    Lox.error(variable.name, "Can't read local variable in its own initializer.");
            }
            resolveLocal(variable, variable.name);
            return null;
        }

        public object visitVarStatement(Statement.Var varStmt)
        {
            declare(varStmt.name);
            if(varStmt.initializer != null)
            {
                resolve(varStmt.initializer);
            }
            define(varStmt.name);
            return null;
        }

        public object visitWhileStatement(Statement.whileStmt whileStmt)
        {
            bool enclosing = isInLoop;
            isInLoop = true;
            resolve(whileStmt.condition);
            resolve(whileStmt.body);
            isInLoop = enclosing;
            return null;
        }

        public object visitClassStatement(Statement.Class classStatement)
        {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;
            declare(classStatement.name);
            define(classStatement.name);


            if (classStatement.superclass != null)
            {
                if (classStatement.name.lexeme.Equals(classStatement.superclass.name.lexeme))
                {
                    Lox.error(classStatement.superclass.name, "A class can't inherit from itself.");
                }
                currentClass = ClassType.SUBCLASS;
                resolve(classStatement.superclass);
                beginScope();
                Variable sup = new Variable(classStatement.name);
                sup.initialized = true;
                sup.used = true;
                scopes.Peek().Add("super", sup);
            }

            beginScope();
            Token thisVar = new Token(TokenType.THIS_OBJECT, "this", null, -1);
            Variable varHacky = new Variable(thisVar);
            varHacky.initialized = true;
            scopes.Peek().Add("this", varHacky);

            foreach(Statement.function method in classStatement.methods)
            {
                FunctionType declaration = FunctionType.METHOD;
                if(method.name.lexeme.Equals("init"))
                {
                    declaration = FunctionType.INITIALIZER;
                }
                if(method._params.Count == 1 && method._params.ElementAt(0).type == TokenType.SEMICOLON && method._params.ElementAt(0).lexeme.Equals("getter"))
                {
                    FunctionType previous = currentFunction;
                    currentFunction = FunctionType.METHOD;
                    Variable getter = new Variable(method.name);
                    getter.initialized = (method.body.Count != 0);
                    getter.used = true; //We don't want warnings for member functions/getters.
                    resolve(method.body);
                    scopes.Peek().Add(method.name.lexeme, getter);
                    currentFunction = previous;
                }
                else
                {
                    resolveFunction(method, declaration);
                }
            }
            endScope();

            if (classStatement.superclass != null) endScope();

            currentClass = enclosingClass;
            return null;
        }

        public object visitGetExpr(Expr.Get get)
        {
            resolve(get._object);
            return null;
        }

        public object visitSetExpr(Expr.Set set)
        {
            resolve(set.value);
            resolve(set._object);
            return null;
        }

        public object visitThisExpr(Expr.This _this)
        {
            if(currentClass == ClassType.NONE)
            {
                Lox.error(_this.keyword, "Can't use 'this' outside of a class.");
                return null;
            }
            resolveLocal(_this, _this.keyword);
            return null;
        }

        public object visitSuperExpr(Expr.Super super)
        {
            if (currentClass == ClassType.NONE)
            {
                Lox.error(super.keyword, "Can't use 'super' outside of a class.");
            }
            else if (currentClass != ClassType.SUBCLASS)
            {
                Lox.error(super.keyword, "Can't use 'super' in a class with no superclass.");
            }
            resolveLocal(super, super.keyword);
            return null;
        }

        public object visitContinueStatement(Statement.continueStmt continueStatement)
        {
            if (!isInLoop) Lox.error(continueStatement.keyword, "Cannot use continue statement outside of a loop.");
            return null;
        }

        public object visitPrefixExpr(Expr.prefix pf)
        {
            resolve(pf.expr);
            return null;
        }

        public object visitPostfixExpr(Expr.postfix pf)
        {
            resolve(pf.expr);
            return null;
        }

        public class Variable
        {
            public Token name;
            public bool used;
            public bool initialized;
            public Variable(Token name)
            {
                this.name = name;
            }
        }
    }
}
