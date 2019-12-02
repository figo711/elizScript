using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core.Errors
{
    public class RuntimeError : Exception
    {
        public Token token;

        public RuntimeError(Token token, string msg) : base(msg)
        {
            this.token = token;
        }
    }
}
