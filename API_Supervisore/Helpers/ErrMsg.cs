using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Supervisore.Helpers
{
    public class ErrMsg
    {
        public ErrMsg(string messaggio, string errore)
        {
            this.messaggio = messaggio;
            this.errore = errore;
        }

        public string messaggio { get; set; }
        public string errore { get; set; }
    }
}
