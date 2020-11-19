using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    class LoxFunction : LoxCallable
    {
        private readonly Statement.function declaration;

        public LoxFunction(Statement.function decl)
        {
            declaration = decl;
        }
        public int arity()
        {
            return declaration._params.Count;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(interpreter.globals);
            for(int i = 0; i < declaration._params.Count; i++)
            {
                environment.define(declaration._params[i].lexeme, arguments[i]);
            }

            interpreter.executeBlock(declaration.body, environment);
            return null;
        }

        public override string ToString()
        {
            return "<fn " + declaration.name.lexeme + ">";
        }
    }
}
