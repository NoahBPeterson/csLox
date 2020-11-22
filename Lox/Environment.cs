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

        Environment ancestor(int distance)
        {
            Environment environment = this;
            //while (environment != null && environment.contains("ke);
            for (int i = 0; i < distance; i++)
            {
                environment = environment.enclosing;
            }
            return environment;
        }

        Environment ancestor(string key)
        {
            Environment environment = this;
            while(environment != null && !environment.contains(key))
            {
                environment = environment.enclosing;
            }
            return environment;
        }

        public void define(String name, Object value)
        {
            if (!values.ContainsKey(name))
            {
                values.Add(name, value);
                return;
            }
            throw new Exceptions.RuntimeError(new Token(Token.TokenType.VAR, name, value, 0), "The var "+name+" has already been defined.");
                
        }

        public Object getAt(int distance, string name)
        {
            Object obj;
            //ancestor(distance).values.TryGetValue(name, out obj);
            ancestor(name).values.TryGetValue(name, out obj);
            return obj;
        }

        public void assignAt(int distance, Token name, Object value)
        {
            Environment env = ancestor(name.lexeme);
            if (env.values.ContainsKey(name.lexeme))
            {
                assign(name, value);
                return;
            }
            env.values.Add(name.lexeme, value);
        }

        public Object get(Token name)
        {
            if(values.ContainsKey(name.lexeme))
            {
                if (values[name.lexeme] == null) throw new Exceptions.RuntimeError(name, "Variable '"+name.lexeme+"' has not been initialized.");
                return values[name.lexeme];
            }
            if (enclosing != null) return enclosing.get(name);
            throw new Exceptions.RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
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

            throw new Exceptions.RuntimeError(name, "Undefined variable '"
                + name.lexeme + "'.");
        }

        public bool contains(String key)
        {
            if (values.ContainsKey(key)) return true;
            return false;
        }

        private int depth()
        {
            int depth = 0;
            Environment env = this;
            while(env != null)
            {
                env = env.enclosing;
                depth++;
            }
            return depth - 1;
        }
    }
}
