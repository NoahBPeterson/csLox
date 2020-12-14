using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class LoxClass : LoxInstance, LoxCallable
    {
        public readonly string name;
        public readonly List<LoxClass> superclasses;
        private readonly Dictionary<string, LoxFunction> methods;
        public LoxClass(string n, List<LoxClass> superClasses, Dictionary<string, LoxFunction> m)
        {
            base.setClass(this);
            name = n;
            this.superclasses = superClasses;
            this.methods = m;
        }

        public LoxFunction findMethod(string name)
        {
            if(methods.ContainsKey(name))
            {
                return methods[name];
            }
            LoxFunction method = null;
            LoxClass superClassFound = null;
            foreach(LoxClass superClass in superclasses)
            {
                if(superClass.findMethod(name) != null)
                {
                    if (method == null)
                    {
                        method = superClass.findMethod(name);
                        superClassFound = superClass;
                    }
                    else
                    {
                        HelperFunctions.GetToken getToken = new HelperFunctions.GetToken();
                        throw new Exceptions.RuntimeError(new Token(Token.TokenType.CLASS, "", superClass, -1, -1),
                            "Error: Tried to call a method from subclass "+this.ToString()+" which exists in superclass " + superClassFound.ToString() +
                            " and superclass " + superClass.ToString() + ".");
                    }
                }                    
                    
            }
            return method;
        }

        public int arity()
        {
            LoxFunction initializer = findMethod("init");
            if (initializer == null) return 0;
            return initializer.arity();
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            LoxFunction initializer = findMethod("init");
            if(initializer != null)
            {
                initializer.bind(instance).call(interpreter, arguments);
            }
            return instance;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
