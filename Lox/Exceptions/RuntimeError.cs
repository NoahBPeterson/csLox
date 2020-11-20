using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lox.Exceptions
{
    class RuntimeError : SystemException
    {
        public readonly Token token;

        public RuntimeError(Token token, String message) : base(message)
        {
            this.token = token;
        }
    }
}
