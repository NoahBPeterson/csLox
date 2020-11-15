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
            return parenthesize(variable.name.lexeme, new Expr[] { variable });
        }

        public string visitAssignExpr(Expr.AssignExpr assignExpr)
        {
            return parenthesize(assignExpr.name.lexeme, new Expr[] { assignExpr.value });
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


    }
}
