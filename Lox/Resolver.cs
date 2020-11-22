using Lox.NativeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Lox.Token;

namespace Lox
{
    public class Resolver : Visitor<Object>, Statement.Visitor<Object>
    {
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;
        private bool isInLoop = false;

        public Resolver(Interpreter i)
        {
            interpreter = i;
        }

        private enum FunctionType { NONE, FUNCTION }

        public void resolve(List<Statement> statements)
        {
            foreach(Statement stmt in statements)
            {
                resolve(stmt);
            }
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
            scopes.Push(new Dictionary<string, bool>());
        }

        private void endScope()
        {
            scopes.Pop();
        }

        private void declare(Token name)
        {
            if(scopes.Count == 0) return;

            Dictionary<string, bool> scope = scopes.Peek();
            if(scope.ContainsKey(name.lexeme))
            {
                Lox.error(name, "Already variable with this name in this scope.");
            }
            scope[name.lexeme] = false;
        }

        private void define(Token name)
        {
            if (scopes.Count == 0) return;
            scopes.Peek()[name.lexeme] = true;
        }

        private void resolveLocal(Expr expr, Token name)
        {
            for(int i = scopes.Count-1; i >= 0; i--)
            {
                if(scopes.ElementAt(i).ContainsKey(name.lexeme))
                {
                    interpreter.resolve(expr, scopes.Count - 1 - i);
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
            if (!isInLoop) Lox.error(breakStmt.name, "Cannot use break statement outside of a loop.");
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
                resolve(returnStmt.value);
            }
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
                bool value = true;
                scopes.Peek().TryGetValue(variable.name.lexeme, out value);
                if (value == false && scopes.Peek().ContainsKey(variable.name.lexeme))
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
    }
}
