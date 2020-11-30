using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class LoxInstance
    {
        private LoxClass _class;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();
        public LoxInstance(LoxClass _class)
        {
            this._class = _class;
        }

        public LoxInstance() { }

        public void setClass(LoxClass _c)
        {
            _class = _c;
        }

        public override string ToString()
        {
            return _class.name + " instance";
        }

        public object get(Token name)
        {
            if(fields.ContainsKey(name.lexeme))
            {
                return fields[name.lexeme];
            }
            LoxFunction method = _class.findMethod(name.lexeme);
            if (method != null) return method.bind(this);

            throw new Exceptions.RuntimeError(name, "Undefined property '" + name.lexeme + "'.");
        }

        public void set(Token name, Object value)
        {
            fields.Add(name.lexeme, value);
        }
    }
}
