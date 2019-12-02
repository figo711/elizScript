using ElizScript.Core.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core.ElizObjects
{
    public class ElizInstance
    {
        public ElizClass klass;
        private Dictionary<string, object> fields = new Dictionary<string, object>();

        public ElizInstance(ElizClass klass)
        {
            this.klass = klass;
        }

        public override string ToString()
        {
            return $"<instance of {klass.name}>";
        }

        public object Get(Token name)
        {
            if (fields.ContainsKey(name.Lexeme))
            {
                return fields[name.Lexeme];
            }

            ElizFunction method = klass.FindMethod(name.Lexeme);

            if (method != null) return method.Bind(this);

            throw new RuntimeError(name,
                $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(Token name, object value)
        {
            if (fields.ContainsKey(name.Lexeme))
                fields[name.Lexeme] = value;
            else
                fields.Add(name.Lexeme, value);
        }
    }
}
