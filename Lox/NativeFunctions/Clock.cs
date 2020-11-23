using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox.NativeFunctions
{
    class Clock : LoxCallable
    {
        public int arity()
        {
            return 0;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            return (double)DateTimeOffset.Now.ToUnixTimeMilliseconds() /1000.0;
        }

        public override String ToString()
        {
            return "<native fn>";
        }
    }
}
