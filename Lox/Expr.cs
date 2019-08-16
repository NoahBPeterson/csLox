using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public abstract class Expr<T>
    {
        public abstract T accept(Visitor<T> visitor);

        public class BinaryExpr : Expr<T>
        {
            public readonly Expr<T> left;
            public readonly Token operatorToken;
            public readonly Expr<T> right;

            public BinaryExpr(Expr<T> left, Token operatorToken, Expr<T> right) //operator is a keyword in C#
            {
                this.left = left;
                this.operatorToken = operatorToken;
                this.right = right;
            }

            public override T accept(Visitor<T> visitor)
            {
                return visitor.visitBinaryExpr(this);
            }
        }

        public class UnaryExpr : Expr<T>
        {
            public readonly Token operatorToken;
            public readonly Expr<T> right;

            public UnaryExpr(Token operatorToken, Expr<T> expression)
            {
                this.operatorToken = operatorToken;
                this.right = expression;
            }

            public override T accept(Visitor<T> visitor)
            {
                return visitor.visitUnaryExpr(this);
            }
        }

        public class Grouping : Expr<T>
        {
            public readonly Expr<T> expression; // "(" expr ")"

            public Grouping(Expr<T> expression)
            {
                this.expression = expression;
            }

            public override T accept(Visitor<T> visitor)
            {
                return visitor.visitGroupingExpr(this);
            }
        }

        public class Literal : Expr<T>
        {
            public readonly Object literal;
            
            public Literal(Object value)
            {
                this.literal = value;
            }

            public override T accept(Visitor<T> visitor)
            {
                return visitor.visitLiteralExpr(this);
            }
        }

    }
    public interface Visitor<T>
    {
        T visitBinaryExpr(Expr<T>.BinaryExpr binaryExpr);
        T visitUnaryExpr(Expr<T>.UnaryExpr unaryExpr);
        T visitGroupingExpr(Expr<T>.Grouping grouping);
        T visitLiteralExpr(Expr<T>.Literal literal);
    }

}
