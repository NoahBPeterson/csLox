using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    class Environment
    {
        private Dictionary<String, Object> values = new Dictionary<string, object>();

        public void define(String name, Object value)
        {
            values.Add(name, value);
        }

        public Object get(Token name)
        {
            if(values.ContainsKey(name.lexeme))
            {
                return values[name.lexeme];
            }
            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }
    }
}
