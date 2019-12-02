using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core.ElizObjects
{
    public interface IElizCallable
    {
        object Call(Interpreter interpreter, List<object> arguments);
        int Arity();
    }
}
