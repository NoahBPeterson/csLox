using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox
{
    public class LoxClass
    {
        public readonly string name;
        public LoxClass(String n)
        {
            name = n;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
