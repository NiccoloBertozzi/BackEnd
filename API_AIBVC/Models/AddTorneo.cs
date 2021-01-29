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
        public double Montepremi { get; set; }
        public string DataChiusuraIscrizioni { get; set; }
        public string DataInizio { get; set; }
        public string DataFine { get; set; }
        public char Genere { get; set; }
        public double QuotaIscrizione { get; set; }
        public string FormulaTorneo { get; set; }
        public int NumMaxTeamMainDraw { get; set; }
        public int NumMaxTeamQualifiche { get; set; }
        public string[] ParametriTorneo { get; set; }
        public string TipoTorneo { get; set; }
        public string[] Impianti { get; set; }
        public int IDSocieta { get; set; }
    }
}
