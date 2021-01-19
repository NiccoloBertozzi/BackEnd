using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_AIBVC.Models
{
    public class AddDelegatiTorneo
    {
        public int IdSupervisore { get; set; }
        public int IdSupArbitrale { get; set; }
        public int IdDirettore { get; set; }
        public int IdTorneo { get; set; }
    }
}
