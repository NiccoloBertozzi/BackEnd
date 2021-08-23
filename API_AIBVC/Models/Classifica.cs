using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_AIBVC.Models
{
    public class Classifica
    {
        public Classifica(string ca1, string ca2,int rank,string team,double punti)
        {
            CA1 = ca1;
            CA2 = ca2;
            Rank = rank;
            Team = team;
            Punti = punti;
        }
        public string CA1 { get; set; }
        public string CA2 { get; set; }
        public int Rank { get; set; }
        public string Team { get; set; }
        public double Punti { get; set; }
    }
}
