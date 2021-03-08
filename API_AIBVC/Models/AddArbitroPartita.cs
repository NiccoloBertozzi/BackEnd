using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_AIBVC.Models
{
    public class AddArbitroPartita
    {
        public int IDDelegato { get; set; }
        public int IDArbitro1 { get; set; }
        public int IDArbitro2 { get; set; }
        public int IDTorneo { get; set; }
        public int NumPartita { get; set; }
    }
}
