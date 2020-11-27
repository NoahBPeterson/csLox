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

        public class function : Statement
        {
            public readonly Token name;
            public readonly List<Token> _params;
            public readonly List<Statement> body;

            public function(Token n, List<Token> p, List<Statement> b)
            {
                name = n;
                _params = p;
                body = b;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitFunction(this);
            }
        }

        public class ifStmt : Statement
        {
            public readonly Expr condition;
            public readonly Statement thenBranch;
            public readonly Statement elseBranch;

            public ifStmt(Expr c, Statement tB, Statement eB)
            {
                condition = c;
                thenBranch = tB;
                elseBranch = eB;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitIfStatement(this);
            }

        }

        public class whileStmt : Statement
        {
            public readonly Expr condition;
            public readonly Statement body;

            public whileStmt(Expr c, Statement b)
            {
                condition = c;
                body = b;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitWhileStatement(this);
            }
        }

        public class breakStmt : Statement
        {
            public readonly Token name;
            public breakStmt(Token n)
            {
                name = n;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitBreakStatement(this);
            }

        }
        public class Return : Statement
        {
            public readonly Token keyword;
            public readonly Expr value;

            public Return(Token k, Expr v)
            {
                keyword = k;
                value = v;
            }

            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitReturnStatement(this);
            }
        }

        public class Class : Statement
        {
            public readonly Token name;
            public readonly List<Statement.function> methods;

            public Class(Token n, List<Statement.function> m)
            {
                name = n;
                methods = m;
            }
            public override T accept<T>(Visitor<T> visitor)
            {
                return visitor.visitClassStatement(this);
            }
        }
        public interface Visitor<T>
        {
            T visitClassStatement(Statement.Class classStatement);
            T visitReturnStatement(Statement.Return returnStmt);
            T visitFunction(Statement.function func);
            T visitBreakStatement(Statement.breakStmt breakStmt);
            T visitWhileStatement(Statement.whileStmt whileStmt);
            T visitIfStatement(Statement.ifStmt ifStmt);
            T visitBlockStatement(Statement.Block blockStmt);
            T visitPrintStatement(Statement.Print printStmt);
            T visitExprStatement(Statement.Expression exprStmt);
            T visitVarStatement(Statement.Var varStmt);
        }
    }
}
