using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox.HelperFunctions
{
    public class GetToken : Visitor<Token>, Statement.Visitor<Token>
    {
        public Token evaluate(Statement stmt)
        {
            return stmt.accept(this);
        }

        public Token evaluate(Expr expr)
        {
            return expr.accept(this);
        }

        private Token defaultToken()
        {
            return new Token(Token.TokenType.EOF, "", null, 0);
        }

        public Token visitAssignExpr(Expr.AssignExpr assignExpr)
        {
            return assignExpr.name;
        }

        public Token visitBinaryExpr(Expr.BinaryExpr binaryExpr)
        {
            return evaluate(binaryExpr.left);
        }

        public Token visitBlockStatement(Statement.Block blockStmt)
        {
            if(blockStmt.statements.Count > 0)
            {
                return evaluate(blockStmt.statements.ElementAt(0));
            }
            return defaultToken();
        }

        public Token visitBreakStatement(Statement.breakStmt breakStmt)
        {
            return breakStmt.name;
        }

        public Token visitCallExpr(Expr.Call call)
        {
            return evaluate(call.callee);
        }

        public Token visitExprStatement(Statement.Expression exprStmt)
        {
            return evaluate(exprStmt.expression);
        }

        public Token visitFunction(Statement.function func)
        {
            return func.name;
        }

        public Token visitGroupingExpr(Expr.Grouping grouping)
        {
            return evaluate(grouping.expression);
        }

        public Token visitIfStatement(Statement.ifStmt ifStmt)
        {
            return evaluate(ifStmt.condition);
        }

        public Token visitLiteralExpr(Expr.Literal literal)
        {
            return literal.type;
        }

        public Token visitLogicalExpr(Expr.logicalExpr logicalExpr)
        {
            return evaluate(logicalExpr.left);
        }

        public Token visitPrintStatement(Statement.Print printStmt)
        {
            return evaluate(printStmt.expression);
        }

        public Token visitReturnStatement(Statement.Return returnStmt)
        {
            return returnStmt.keyword;
        }

        public Token visitTernaryExpr(Expr.TernaryExpr ternaryExpr)
        {
            return evaluate(ternaryExpr.comparisonExpression);
        }

        public Token visitUnaryExpr(Expr.UnaryExpr unaryExpr)
        {
            return evaluate(unaryExpr.right);
        }

        public Token visitVariable(Expr.Variable variable)
        {
            return variable.name;
        }

        public Token visitVarStatement(Statement.Var varStmt)
        {
            return varStmt.name;
        }

        public Token visitWhileStatement(Statement.whileStmt whileStmt)
        {
            return new Token(Token.TokenType.WHILE, "while", null, evaluate(whileStmt.condition).line);
        }

        public Token visitClassStatement(Statement.Class classStatement)
        {
            return classStatement.name;
        }

        public Token visitSetExpr(Expr.Set set)
        {
            return set.name;
        }

        public Token visitGetExpr(Expr.Get get)
        {
            return get.name;
        }

        public Token visitThisExpr(Expr.This _this)
        {
            return _this.keyword;
        }

        public Token visitSuperExpr(Expr.Super super)
        {
            return super.keyword;
        }
    }
}
