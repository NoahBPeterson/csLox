using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lox
{
    public abstract class Statement
    {
        public abstract T accept<T>(Visitor<T> visitor);
        public class Expression : Statement
        {
            public readonly Expr expression;
            public Expression(Expr expression)
            {
                this.expression = expression;
            }
            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitExprStatement(this);
            }
        }
        public class Print : Statement
        {
            public readonly Expr expression;
            public Print(Expr expression)
            {
                this.expression = expression;
            }
            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitPrintStatement(this);
            }
        }

        public class Var : Statement
        {
            public readonly Token name;
            public readonly Expr initializer;

            public Var(Token name, Expr initializer)
            {
                this.name = name;
                this.initializer = initializer;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitVarStatement(this);
            }
        }

        public class Block : Statement
        {
            public readonly List<Statement> statements;

            public Block(List<Statement> statements)
            {
                this.statements = statements;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitBlockStatement(this);
            }
        }
        public interface Visitor<T>
        {
            T visitBlockStatement(Statement.Block blockStmt);
            T visitPrintStatement(Statement.Print printStmt);
            T visitExprStatement(Statement.Expression exprStmt);
            T visitVarStatement(Statement.Var varStmt);
        }
    }
}
