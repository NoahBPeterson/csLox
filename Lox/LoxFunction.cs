using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class LoxFunction : LoxCallable
    {
        private readonly Statement.function declaration;
        private readonly Environment closure;
        private bool isInitializer;
        private bool isGet;

        public LoxFunction(Statement.function decl, Environment enclosure, bool isIn)
        {
            declaration = decl;
            closure = enclosure;
            isInitializer = isIn;
            isGet = false;
        }

        public LoxFunction(Statement.function decl, Environment enclosure, bool isIn, bool isGetter)
        {
            declaration = decl;
            closure = enclosure;
            isInitializer = isIn;
            this.isGet = isGetter;
        }

        public LoxFunction(Expr.Lambda decl, Environment enclosure, bool isIn)
        {
            declaration = new Statement.function(decl.keyword, decl._params, decl.body);
            closure = enclosure;
            isInitializer = isIn;
            isGet = false;
        }
        public LoxFunction bind(LoxInstance instance)
        {
            Environment environment = new Environment(closure);
            environment.define("this", instance);
            return new LoxFunction(declaration, environment, isInitializer, isGet);
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
                    arg = new LoxFunction((Statement.function) arguments[i], environment, false);
                }
                environment.define(declaration._params[i].lexeme, arg);
            }
            try
            {
                interpreter.executeBlock(declaration.body, environment);
            } catch (Exceptions.Return returnValue)
            {
                if (isInitializer) return closure.getAt(0, "this");
                return returnValue.value;
            }
            if (isInitializer) return closure.getAt(0, "this");
            return null;
        }

        public bool isGetter()
        {
            if (isGet) return true;
            return false;
        }

        public override string ToString()
        {
            return "<fn " + declaration.name.lexeme + ">";
        }
    }
}
