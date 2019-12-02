using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core.ElizObjects
{
    public class ElizFunction : IElizCallable
    {
        private Stmt.Function declaration;
        private Environment closure;
        private bool isInitializer;

        public ElizFunction(Stmt.Function declaration, Environment closure, bool isInitializer)
        {
            this.closure = closure;
            this.declaration = declaration;
            this.isInitializer = isInitializer;
        }

        public ElizFunction Bind(ElizInstance instance)
        {
            Environment environment = new Environment(closure);
            environment.Define("this", instance);
            return new ElizFunction(declaration, environment, isInitializer);
        }

        public int Arity()
        {
            return declaration.@params.Count;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(closure);

            for (int i = 0; i < declaration.@params.Count; i++)
            {
                environment.Define(declaration.@params[i].Lexeme,
                    arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            }
            catch (Return returnValue)
            {
                if (isInitializer) return closure.GetAt(0, "this");

                return returnValue.value;
            }


            if (isInitializer) return closure.GetAt(0, "this");
            return null;
        }

        public override string ToString()
        {
            return "<method " + declaration.name.Lexeme + ">";
        }
    }
}
