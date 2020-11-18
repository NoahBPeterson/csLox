using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public abstract class Expr
    {
        public abstract T accept<T>(Visitor<T> visitor);


        public class TernaryExpr : Expr
        {
            public readonly Expr comparisonExpression;
            public readonly Expr trueExpression;
            public readonly Expr falseExpression;

            public TernaryExpr(Expr comparisonExpression, Expr trueExpression, Expr falseExpression)
            {
                this.comparisonExpression = comparisonExpression;
                this.trueExpression = trueExpression;
                this.falseExpression = falseExpression;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitTernaryExpr(this);
            }
        }
        public class BinaryExpr : Expr
        {
            public readonly Expr left;
            public readonly Token operatorToken;
            public readonly Expr right;

            public BinaryExpr(Expr left, Token operatorToken, Expr right) //operator is a keyword in C#
            {
                this.left = left;
                this.operatorToken = operatorToken;
                this.right = right;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitBinaryExpr(this);
            }
        }

        public class UnaryExpr : Expr
        {
            public readonly Token operatorToken;
            public readonly Expr right;

            public UnaryExpr(Token operatorToken, Expr expression)
            {
                this.operatorToken = operatorToken;
                this.right = expression;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitUnaryExpr(this);
            }
        }

        public class Grouping : Expr
        {
            public readonly Expr expression; // "(" expr ")"

            public Grouping(Expr expression)
            {
                this.expression = expression;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitGroupingExpr(this);
            }
        }

        public class Literal : Expr
        {
            public readonly Object literal;
            
            public Literal(Object value)
            {
                this.literal = value;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitLiteralExpr(this);
            }
        }

        public class Variable : Expr
        {
            public readonly Token name;

            public Variable(Token name)
            {
                this.name = name;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitVariable(this);
            }
        }

        public class AssignExpr : Expr
        {
            public readonly Token name;
            public readonly Expr value;

            public AssignExpr(Token name, Expr value)
            {
                this.name = name;
                this.value = value;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitAssignExpr(this);
            }
        }

        public class logicalExpr : Expr
        {
            public readonly Expr left;
            public readonly Token _operator;
            public readonly Expr right;

            public logicalExpr(Expr l, Token o, Expr r)
            {
                left = l;
                _operator = o;
                right = r;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitLogicalExpr(this);
            }
        }

    }
    public interface Visitor<T>
    {
        T visitLogicalExpr(Expr.logicalExpr logicalExpr);
        T visitAssignExpr(Expr.AssignExpr assignExpr);
        T visitTernaryExpr(Expr.TernaryExpr ternaryExpr);
        T visitBinaryExpr(Expr.BinaryExpr binaryExpr);
        T visitUnaryExpr(Expr.UnaryExpr unaryExpr);
        T visitGroupingExpr(Expr.Grouping grouping);
        T visitLiteralExpr(Expr.Literal literal);

        T visitVariable(Expr.Variable variable);
    }

}
