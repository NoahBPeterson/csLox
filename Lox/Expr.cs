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

        public class Call : Expr
        {
            public readonly Expr callee;
            public readonly Token paren;
            public readonly List<Object> expressionArguments;

            public Call(Expr c, Token p, List<Object> expressionArgs)
            {
                callee = c;
                paren = p;
                expressionArguments = expressionArgs;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitCallExpr(this);
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
            public readonly Token type;
            
            public Literal(Object value, Token t)
            {
                this.literal = value;
                type = t;
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

        public class Get : Expr
        {
            public readonly Token name;
            public readonly Expr _object;

            public Get(Expr e, Token n)
            {
                name = n;
                _object = e;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitGetExpr(this);
            }
        }

        public class Set : Expr
        {
            public readonly Expr _object;
            public readonly Token name;
            public readonly Expr value;

            public Set(Expr o, Token n, Expr v)
            {
                _object = o;
                name = n;
                value = v;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitSetExpr(this);
            }
        }

        public class Super : Expr
        {
            public readonly Token keyword;
            public readonly Token method;

            public Super(Token k, Token m)
            {
                keyword = k;
                method = m;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitSuperExpr(this);
            }
        }

        public class This : Expr
        {
            public readonly Token keyword;
            public This(Token k)
            {
                keyword = k;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitThisExpr(this);
            }
        }

        public class prefix : Expr
        {
            public readonly Token keyword;
            public readonly Expr expr;

            public prefix(Token k, Expr e)
            {
                keyword = k;
                expr = e;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitPrefixExpr(this);
            }
        }

        public class postfix : Expr
        {
            public readonly Token keyword;
            public readonly Expr expr;

            public postfix(Token k, Expr e)
            {
                keyword = k;
                expr = e;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitPostfixExpr(this);
            }
        }

        public class Lambda : Expr
        {
            public readonly Token keyword;
            public readonly List<Token> _params;
            public readonly List<Statement> body;

            public Lambda(Token k, List<Token> _p, List<Statement> b)
            {
                keyword = k;
                _params = _p;
                body = b;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitLambdaFunction(this);
            }
        }

    }
    public interface Visitor<T>
    {
        T visitLambdaFunction(Expr.Lambda lambdaFunction);
        T visitPrefixExpr(Expr.prefix pf);
        T visitPostfixExpr(Expr.postfix pf);
        T visitSuperExpr(Expr.Super super);
        T visitThisExpr(Expr.This _this);
        T visitSetExpr(Expr.Set set);
        T visitGetExpr(Expr.Get get);
        T visitCallExpr(Expr.Call call);
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
