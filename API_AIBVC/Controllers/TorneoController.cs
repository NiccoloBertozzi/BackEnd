using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAuthJWT.Helpers;
using API_AIBVC.Models;
using TEST.Models;

namespace API_AIBVC.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/tornei")]
    public class TorneoController : Controller
    {
        Database db = new Database();

        //Restituisce tornei prima della data inserita
        [HttpGet("GetTornei/{Data}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin,Delegato,Allenatore,Admin")]
        public DataTable GetTornei(DateTime Data)
        {
            return db.GetTorneiEntroData(Data);
        }

        [HttpPost("CreaTorneo")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Admin")]
        public ActionResult<InfoMsg> CreaTorneo([FromBody]AddTorneo torneo)
        {
            if (db.CreaTorneo(torneo.Titolo, torneo.PuntiVittoria, torneo.Montepremi, Convert.ToDateTime(torneo.DataChiusuraIscrizioni), Convert.ToDateTime(torneo.DataInizio), Convert.ToDateTime(torneo.DataFine), torneo.Genere, torneo.FormulaTorneo, torneo.NumTeamTabellone, torneo.NumTeamQualifiche, torneo.ParametriTorneo, torneo.TipoTorneo, torneo.Impianti,torneo.QuotaIngresso))
                return Ok(new InfoMsg(DateTime.Today, $"Torneo creato con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nella creazione del torneo"));
        }

        [HttpPost("InserisciSquadra")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta")]
        public Nullable<int> InsertSquadra([FromBody]Squadra squadra)
        {
            Nullable<int> idsquadra = db.InsertSquadra(squadra.Atleta1, squadra.Atleta2, squadra.NomeTeam);
            if (idsquadra != null)
                return idsquadra;
            else
                return null;
        }

        [HttpPost("IscriviSquadra")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta")]
        public ActionResult<InfoMsg> IscriviSquadra([FromBody]ListaIscritti iscritti)
        {
            if (db.IscriviSquadra(iscritti.IDTorneo, iscritti.IDSquadra, iscritti.IDAllenatore))
                return Ok(new InfoMsg(DateTime.Today, $"Squadra Iscritta con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante l'iscrizione della squadra"));
        }

        //Restituisce tornei prima della data inserita
        [HttpGet("GetPartite/{NumeroPartite}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetTornei(int NumeroPartite)
        {
            return db.GetPartite(NumeroPartite);
        }

        //Return TipoTorneo
        [HttpGet("TipoTorneo")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetTipoTorneo()
        {
            return db.GetTipoTorneo();
        }

        //Return ListaDelegati
        [HttpGet("ListaDelegati/{tipo}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetDelegato(int tipo)
        {
            return db.GetDelegato(tipo);
        }

        //Return ListaDelegati
        [HttpGet("FormulaTorneo")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetFormula()
        {
            return db.GetFormula();
        }

        //Return ParametriTorneo
        [HttpGet("ParametroTorneo")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetParametriTorneo()
        {
            return db.GetParametriTorneo();
        }

        //Return Impianti società
        [HttpGet("GetImpianti/{idSocieta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetImpianti(int idSocieta)
        {
            return db.GetImpianti(idSocieta);
        }

        //Autorizza il torneo
        [HttpPut("AutorizzaTorneo/{titoloTorneo}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Admin")]
        public ActionResult<InfoMsg> AutorizzaTorneo(string titoloTorneo)
        {
            if(db.AutorizzaTorneo(titoloTorneo))
                return Ok(new InfoMsg(DateTime.Today, $"Torneo autorizzato con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante l'autorizzazione del torneo"));
        }
    }
}
