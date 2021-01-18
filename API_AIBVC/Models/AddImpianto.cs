using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_AIBVC.Models
{
    public class AddImpianto
    {
        public string NomeComune { get; set; }
        public string NomeImpianto { get; set; }
        public int NumeroCampi { get; set; }
        public string Indirizzo { get; set; }
        public string CAP { get; set; }
        public string Descrizione { get; set; }
        public string Email { get; set; }
        public string Sito { get; set; }
        public string Tel { get; set; }
        public int IdSocieta { get; set; }
    }
}
