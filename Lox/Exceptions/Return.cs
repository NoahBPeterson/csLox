using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lox.Exceptions
{
    public class Return : SystemException
    {
        public readonly Object value;
        public Return(Object value)
        {
            this.value = value;
        }
    }
}
