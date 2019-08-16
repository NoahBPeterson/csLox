using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class AstPrinter : Visitor<string>
    {
        public string print(Expr<string> expr)
        {
            return expr.accept(this);
        }

        public string visitBinaryExpr(Expr<String>.BinaryExpr expr)
        {
            return parenthesize(expr.operatorToken.lexeme, new Expr<String>[] { expr.left, expr.right});
        }

        public string visitGroupingExpr(Expr<String>.Grouping expr)
        {
            return parenthesize("group", new Expr<String>[] { expr.expression });
        }

        public string visitLiteralExpr(Expr<String>.Literal expr)
        {
            if (expr.literal == null) return "nil";
            return expr.literal.ToString();
        }

        public string visitUnaryExpr(Expr<String>.UnaryExpr expr)
        {
            return parenthesize(expr.operatorToken.lexeme, new Expr<String>[] { expr.right });
        }

        private string parenthesize(string name, Expr<String>[] exprs)
        {
            StringBuilder builder = new StringBuilder();
            
            builder.Append("(").Append(name);
            foreach (Expr<String> expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}
