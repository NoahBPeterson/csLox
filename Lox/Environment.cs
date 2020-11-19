using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class Environment
    {
        public Environment enclosing;
        private Dictionary<String, Object> values = new Dictionary<string, object>();

        public Environment()
        {
            enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }
        public void define(String name, Object value)
        {
            if (!values.ContainsKey(name))
            {
                values.Add(name, value);
                return;
            }
            throw new RuntimeError(new Token(Token.TokenType.VAR, name, value, 0), "The var "+name+" has already been defined.");
                
        }

        public Object get(Token name)
        {
            if(values.ContainsKey(name.lexeme))
            {
                if (values[name.lexeme] == null) throw new RuntimeError(name, "Variable '"+name.lexeme+"' has not been initialized.");
                return values[name.lexeme];
            }
            if (enclosing != null) return enclosing.get(name);
            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }

        public void assign(Token name, Object value)
        {
            if(values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }

            if(enclosing != null)
            {
                enclosing.assign(name, value);
                return;
            }

            throw new RuntimeError(name, "Undefined variable '"
                + name.lexeme + "'.");
        }
    }
}
