using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Supervisore.Models
{
    public class AggiornaSet
    {
        public int IdTorneo { get; set; }
        public int NumPartita { get; set; }
        public int NumSet { get; set; }
        public int PuntiTeam1 { get; set; }
        public int PuntiTeam2 { get; set; }
    }
}
