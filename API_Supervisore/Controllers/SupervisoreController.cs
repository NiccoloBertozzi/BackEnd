using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TEST.Models;
using API_Supervisore.Models;
using API_Supervisore.Helpers;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;

namespace API_Supervisore.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/supervisore")]
    public class SupervisoreController : Controller
    {
        Database db = new Database();

        [HttpPut("CambiaPsw")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<InfoMsg> CambiaPsw([FromBody] SetNuovaPsw setNPSW)
        {
            PasswordHasher hasher = new PasswordHasher();
            setNPSW.Password = hasher.Hash(setNPSW.Password);
            if(db.SetNuovaPsw(setNPSW.Email,setNPSW.Password))
                return Ok(new InfoMsg(DateTime.Today, $"Password cambiata con successo."));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nel cambio password."));
        }
        [HttpGet("GetAllTornei")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore")]
        public JsonResult GetAllTornei()
        {
            return Json(new { myOutput = db.GetAllTornei()});
        }
        /*[HttpGet("GetTorneiByData/{data}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore")]
        public ActionResult<InfoMsg> GetTorneiByData(string data)
        {
            return Ok(db.GetTorneiByData(Convert.ToDateTime(data)));
        }*/
        [HttpPost("GetNomiSquadreInPartita")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore")]
        public ActionResult<InfoMsg> GetNomiSquadreInPartita([FromBody] CercaTeam cercaTeam)
        {
            return Ok(db.GetInfoSquadre(db.GetIDTorneo(cercaTeam.TitoloTorneo), cercaTeam.NumPartita));
        }
        [HttpPut("AggiornaRisultati")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Delegato")]
        public ActionResult<InfoMsg> AggiornaRisultati([FromBody]AggiornaSet aggiornaSet)
        {
            //Mi servono: IDTorneo,IDPartita,Numero del set e i punti fatti dalle 2 squadre
            if (db.UploadResults(db.GetIDTorneo(aggiornaSet.TitoloTorneo), aggiornaSet.NumPartita, aggiornaSet.NumSet, aggiornaSet.PuntiTeam1, aggiornaSet.PuntiTeam2))
                return Ok(new InfoMsg(DateTime.Today, $"Risultato aggiornato con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nell'aggiornamento del risultato"));
        }
        [HttpPost("GetPartita")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore")]
        public ActionResult<InfoMsg> GetPartita([FromBody]CercaTeam cercaPartita)
        {
            return Ok(db.GetPartita(db.GetIDTorneo(cercaPartita.TitoloTorneo), cercaPartita.NumPartita));
        }
        [HttpGet("GetTorneoByTitolo/{titolo}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore")]
        public JsonResult GetTorneoByTitolo(string titolo)
        {
            return Json(new { myOutput = db.GetTorneoByTitolo(db.GetIDTorneo(titolo))});
        }
    }
}
