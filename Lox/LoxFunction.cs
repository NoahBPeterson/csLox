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
        private readonly Environment closure;

        public LoxFunction(Statement.function decl, Environment enclosure)
        {
            declaration = decl;
            closure = enclosure;
        }
        public int arity()
        {
            return declaration._params.Count;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(closure);
            for(int i = 0; i < declaration._params.Count; i++)
            {
                Object arg = arguments[i];
                if(arguments[i] is Statement.function)
                {
                    arg = new LoxFunction((Statement.function) arguments[i], environment);
                }
                environment.define(declaration._params[i].lexeme, arg);
            }
            try
            {
                interpreter.executeBlock(declaration.body, environment);
            } catch (Exceptions.Return returnValue)
            {
                return returnValue.value;
            }
            return null;
        }

        public override string ToString()
        {
            return "<fn " + declaration.name.lexeme + ">";
        }
    }
}
