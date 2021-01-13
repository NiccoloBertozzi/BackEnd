using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_AIBVC.Models
{
    public class EliminaTeam
    {
        public int IdDelegato { get; set; }
        public int IdTorneo { get; set; }
        public int IdAtleta1 { get; set; }
        public int IdAtleta2 { get; set; }
    }
}
