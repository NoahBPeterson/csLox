using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class LoxClass : LoxCallable
    {
        public readonly string name;
        private readonly Dictionary<string, LoxFunction> methods;
        internal LoxClass(string n, Dictionary<string, LoxFunction> m)
        {
            name = n;
            this.methods = m;
        }

        public LoxFunction findMethod(string name)
        {
            if(methods.ContainsKey(name))
            {
                return methods[name];
            }
            return null;
        }

        public int arity()
        {
            return 0;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            return instance;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
