using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MogoMyPti.Entities
{
    public class LoginException : Exception
    {
        public LoginException() : base("Login exception")
        {

        }
    }
}
