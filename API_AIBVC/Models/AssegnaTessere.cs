using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_AIBVC.Models
{
    public class AssegnaTessere
    {
        public int IDAtleta { get; set; }
        public int IDSocieta { get; set; } 
        public string CodiceTessera { get; set; }
        public string TipoTessera { get; set; } 
        public DateTime DataTesseramento { get; set; }
        public int AnnoTesseramento { get; set; }
        public double Importo { get; set; }
    }
}
