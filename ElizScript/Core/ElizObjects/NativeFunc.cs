using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core.ElizObjects
{
    public class NativeFunc<TResult> : IElizCallable
    {
        private int argsCount;

        private Dictionary<int, Func<List<object>, TResult>> funcs;

        public NativeFunc(int arity)
        {
            argsCount = arity;
            funcs = new Dictionary<int, Func<List<object>, TResult>>();
        }

        public void AddFunc(int arity, Func<List<object>, TResult> func)
        {
            funcs.Add(arity, func);
        }

        public int Arity()
        {
            return argsCount;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return funcs[arguments.Count].Invoke(arguments);
            //return func.Invoke(arguments);
        }

        public override string ToString()
        {
            return "<native method>";
        }
    }
}
