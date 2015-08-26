using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Ddd
{
    public class AuthorizationContext
    {
        public AuthorizationContext()
        {            
        }

        public bool IsAuthenticated { get; set; }
        public Guid UserAccountId { get; set; } // claim type sub        
        public List<string> Roles { get; set; } // claim type role
        public string UserIpAddress { get; set; }

        public override string ToString()
        {
            return string.Format("{0} UserAccountId: {1}, Roles: {2}",
                IsAuthenticated ? "authenticated" : "NOT authenticated",
                UserAccountId,                 
                string.Join(", ", Roles.ToArray()));
        }
    }
}
