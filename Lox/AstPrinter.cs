﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class AstPrinter : Visitor<string>
    {
        public string print(Expr expr)
        {
            return expr.accept(this);
        }

        public string visitTernaryExpr(Expr.TernaryExpr expr)
        {
            return parenthesize("?:", new Expr[] { expr.comparisonExpression, expr.trueExpression, expr.falseExpression });
        }
        public string visitBinaryExpr(Expr.BinaryExpr expr)
        {
            return parenthesize(expr.operatorToken.lexeme, new Expr[] { expr.left, expr.right});
        }

        public string visitGroupingExpr(Expr.Grouping expr)
        {
            return parenthesize("group", new Expr[] { expr.expression });
        }

        public string visitLiteralExpr(Expr.Literal expr)
        {
            if (expr.literal == null) return "nil";
            return expr.literal.ToString();
        }

        public string visitUnaryExpr(Expr.UnaryExpr expr)
        {
            return parenthesize(expr.operatorToken.lexeme, new Expr[] { expr.right });
        }

        public string visitVariable(Expr.Variable variable)
        {
            return parenthesize(variable.name.lexeme, new Expr[] {});
        }

        public string visitAssignExpr(Expr.AssignExpr assignExpr)
        {
            return parenthesize("= "+assignExpr.name.lexeme, new Expr[] { assignExpr.value });
        }

        private string parenthesize(string name, Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();
            
            builder.Append("(").Append(name);
            foreach (Expr expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }

        public string visitLogicalExpr(Expr.logicalExpr logicalExpr)
        {
            return parenthesize(logicalExpr._operator.lexeme, new Expr[] { logicalExpr.left, logicalExpr.right });
        }

        public string visitCallExpr(Expr.Call call)
        {
            Expr[] expressions = new Expr[call.expressionArguments.Count];
            for(int i = 0; i < call.expressionArguments.Count; i++)
            {
                if (call.expressionArguments[i] is Expr)
                {
                    expressions[i] = (Expr) call.expressionArguments[i];
                } else if(call.expressionArguments[i] is Statement.function)
                {
                    Statement.function lambda = (Statement.function)call.expressionArguments[i];
                    expressions[i] = new Expr.Lambda(lambda.name, lambda._params, lambda.body);
                }
            }
            return parenthesize(call.callee.ToString(), expressions);
        }

        public string visitSetExpr(Expr.Set set)
        {
            return "(= "+set._object.accept(this)+"."+set.name.lexeme+" "+set.value.accept(this)+")";
        }

        public string visitGetExpr(Expr.Get get)
        {
            return "("+get._object.accept(this) + "." + get.name.lexeme+")";
        }

        public string visitThisExpr(Expr.This _this)
        {
            return "(this)";
        }

        public string visitSuperExpr(Expr.Super super)
        {
            return "(super)";
        }

        public string visitPrefixExpr(Expr.prefix pf)
        {
            return parenthesize("p"+pf.keyword.lexeme, new Expr[] { pf.expr });
        }

        public string visitPostfixExpr(Expr.postfix pf)
        {
            return parenthesize(pf.keyword.lexeme+"p", new Expr[] { pf.expr });
        }

        public string visitLambdaFunction(Expr.Lambda lambdaFunction)
        {
            return parenthesize(lambdaFunction.keyword.lexeme, new Expr[] { });
        }
    }
}
