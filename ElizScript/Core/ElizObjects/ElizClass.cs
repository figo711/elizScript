using System.Collections.Generic;

namespace ElizScript.Core.ElizObjects
{
    public class ElizClass : IElizCallable
    {
        public string name;
        public ElizClass baseclass;
        private Dictionary<string, ElizFunction> methods;

        public ElizClass(string name, ElizClass baseclass, Dictionary<string, ElizFunction> methods)
        {
            this.name = name;
            this.methods = methods;
            this.baseclass = baseclass;
        }

        public int Arity()
        {
            ElizFunction initializer = FindMethod("init");
            if (initializer == null) return 0;

            return initializer.Arity();
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            ElizInstance instance = new ElizInstance(this);

            ElizFunction initializer = FindMethod("init");
            if (initializer != null)
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }

        public override string ToString()
        {
            return $"<class {name}>";
        }

        public ElizFunction FindMethod(string name)
        {
            if (methods.ContainsKey(name))
            {
                return methods[name];
            }

            if (baseclass != null)
            {
                return baseclass.FindMethod(name);
            }

            return null;
        }
    }
}
