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
        public int IDPartita { get; set; }
        public int pt1s3 { get; set; }
        public int pt1s2 { get; set; }
        public int pt1s1 { get; set; }
        public int pt2s3 { get; set; }
        public int pt2s2 { get; set; }
        public int pt2s1 { get; set; }
        public int NumSet { get; set; }
        public int IDTorneoPrincipale { get; set; }
    }
}
