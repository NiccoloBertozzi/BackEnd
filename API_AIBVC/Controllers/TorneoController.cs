using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAuthJWT.Helpers;
using API_AIBVC.Models;

namespace API_AIBVC.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Authorize(Roles = "Societa, Delegato, Atleta, Allenatore")]
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
            if (db.CreaTorneo(torneo.Titolo, torneo.PuntiVittoria, torneo.Montepremi, Convert.ToDateTime(torneo.DataChiusuraIscrizioni), Convert.ToDateTime(torneo.DataInizio), Convert.ToDateTime(torneo.DataFine), torneo.Genere, torneo.FormulaTorneo, torneo.NumTeamTabellone, torneo.NumTeamQualifiche, torneo.ParametriTorneo, torneo.TipoTorneo, torneo.Impianti))
                return Ok(new InfoMsg(DateTime.Today, $"Torneo creato con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nella creazione del torneo"));
        }
        [HttpPut("AutorizzaTorneo/{nomeTorneo}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Admin")]
        public ActionResult<InfoMsg> AutorizzaTorneo(string nomeTorneo)
        {
            //Questa API viene richiamata solo se l'AIBVC autorizza il torneo
            if(db.AutorizzaTorneo(nomeTorneo))
                return Ok(new InfoMsg(DateTime.Today, $"Torneo autorizzato con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nell'autorizzazione del torneo"));
        }
        [HttpPut("AssegnaSupervisori")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "AdminDelegato")]
        /*
         * Se l'admin decide di non inserire il supervisore arbitrale e/o il direttore, i dati di questi vengono impostati null
         */
        public ActionResult<InfoMsg> AssegnaSupervisori([FromBody]AssegnaDelegati assegnaDel)
        {
            if (db.AssegnaSupervisori(assegnaDel.NomeSupervisore, assegnaDel.CognomeSupervisore, assegnaDel.NomeSupArbitrale, assegnaDel.CognomeSupArbitrale, assegnaDel.NomeDirettore, assegnaDel.CognomeDirettore, assegnaDel.TitoloTorneo))
                return Ok(new InfoMsg(DateTime.Today, $"Delegati assegnati con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nell'assegnazione dei delegati"));
        }
    }
}
