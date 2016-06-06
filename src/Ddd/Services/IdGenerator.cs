using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd.Services
{
    public class IdGenerator
    {
        private static Func<Guid> generator;

        public static Func<Guid> GenerateGuid
        {
            get
            {
                generator = generator ?? Guid.NewGuid;
                return generator;
            }
            set
            {
                generator = value;
            }
        }
    }
}
