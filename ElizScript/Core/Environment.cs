using ElizScript.Core.Errors;
using System;
using System.Collections.Generic;

namespace ElizScript.Core
{
    public class Environment
    {
        public Environment enclosing;

        private Dictionary<string, object> values = new Dictionary<string, object>();

        public Environment()
        {
            enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public void Define(string name, object value)
        {
            values[name] = value;
        }

        public void Delete(string name)
        {
            if (values.ContainsKey(name))
            {
                values.Remove(name);
            }
            else throw new RuntimeError(null, $"Undefined variable '{name}'");
        }

        public object Get(Token name)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                return values[name.Lexeme];
            }

            if (enclosing != null) return enclosing.Get(name);

            throw new RuntimeError(name,
                $"Undefined variable '{name.Lexeme}'.");
        }

        public void Assign(Token name, object value)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name,
                $"Undefined variable '{name.Lexeme}'.");
        }

        public object GetAt(int distance, string name)
        {
            try
            {
                return Ancestor(distance).values[name];
            }
            catch
            {
                return null;
            }
        }

        Environment Ancestor(int distance)
        {
            Environment environment = this;

            for (int i = 0; i < distance; i++)
            {
                environment = environment.enclosing;
            }

            return environment;
        }

        public void AssignAt(int distance, Token name, object value)
        {
             Ancestor(distance).values[name.Lexeme] = value;
        }

        public override string ToString()
        {
            string result = values.ToString();

            if (enclosing != null)
            {
                result += " -> " + enclosing.ToString();
            }

            return result;
        }
    }
}
