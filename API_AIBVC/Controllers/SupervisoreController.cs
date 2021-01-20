using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAuthJWT.Helpers;
using API_Login_Registra.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using API_Supervisore.Models;
using System.Data;

namespace API_AIBVC.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1")]
    public class SupervisoreController : Controller
    {
        Database db = new Database();
        [HttpGet("GetAllTornei")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public JsonResult GetAllTornei()
        {
            return Json(new { myOutput = db.GetAllTornei() });
        }
        [HttpPost("GetNomiSquadreInPartita")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public ActionResult<InfoMsg> GetNomiSquadreInPartita([FromBody] CercaTeam cercaTeam)
        {
            return Ok(db.GetInfoSquadre(db.GetIDTorneo(cercaTeam.TitoloTorneo), cercaTeam.NumPartita));
        }
        [HttpPut("AggiornaRisultati")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Delegato,Admin")]
        public ActionResult<InfoMsg> AggiornaRisultati([FromBody] AggiornaSet aggiornaSet)
        {
            //Mi servono: IDTorneo,IDPartita,Numero del set e i punti fatti dalle 2 squadre
            if (db.UploadResults(db.GetIDTorneo(aggiornaSet.TitoloTorneo), aggiornaSet.NumPartita, aggiornaSet.NumSet, aggiornaSet.PuntiTeam1, aggiornaSet.PuntiTeam2))
                return Ok(new InfoMsg(DateTime.Today, $"Risultato aggiornato con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nell'aggiornamento del risultato"));
        }
        [HttpPost("GetPartita")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public ActionResult<InfoMsg> GetPartita([FromBody] CercaTeam cercaPartita)
        {
            return Ok(db.GetPartita(db.GetIDTorneo(cercaPartita.TitoloTorneo), cercaPartita.NumPartita));
        }
        [HttpGet("GetTorneoById/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetTorneoByID(int id)
        {
            return db.GetTorneoByID(id)[0];
        }
        [HttpGet("GetIDSupervisore/{CF}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetNomeCognomeSupervisore(string CF)
        {
            return db.GetNomeCognomeSupervisore(CF);
        }
    }
}
