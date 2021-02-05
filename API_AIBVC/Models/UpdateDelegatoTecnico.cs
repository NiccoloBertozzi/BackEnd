using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TEST.Models;

namespace API_Login_Registra.Models
{
    public class UpdateDelegatoTecnico
    {
        public DelegatoTecnico delegato { get; set; }
        public string ComuneNascita { get; set; }
        public string ComuneResidenza { get; set; }
    }
}
