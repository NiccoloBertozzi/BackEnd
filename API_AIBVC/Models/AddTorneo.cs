using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_AIBVC.Models
{
    public class AddTorneo
    {
        public string Titolo { get; set; }
        public int PuntiVittoria { get; set; }
        public float Montepremi { get; set; }
        public DateTime DataChiusuraIscrizioni { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public char Genere { get; set; }
        public string FormulaTorneo { get; set; }
        public int NumTeamTabellone { get; set; }
        public int NumTeamQualifiche { get; set; }
        public string[] ParametriTorneo { get; set; }
        public string TipoTorneo { get; set; }
        public string[] Impianti { get; set; }
    }
}
