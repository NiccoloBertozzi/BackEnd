using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TEST.Models;

namespace API_Login_Registra.Models
{
    public class SocietaLogin
    {
        public Societa societa { get; set; }
        public CredenzialiExtraTabella cred { get; set; }
    }
}
