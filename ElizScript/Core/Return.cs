using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElizScript.Core
{
    public class Return : SystemException
    {
        public readonly object value;

        public Return(object value) : base(null, null)
        {
            this.value = value;
        }
    }
}
