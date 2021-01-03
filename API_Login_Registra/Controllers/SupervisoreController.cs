using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_Login_Registra.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPIAuthJWT.Helpers;

namespace API_Login_Registra.Controllers
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
            if (db.SetNuovaPsw(setNPSW.Email, setNPSW.Password))
                return Ok(new InfoMsg(DateTime.Today, $"Password cambiata con successo."));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nel cambio password."));
        }

        [HttpGet("GetAllTornei")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore")]
        public JsonResult GetAllTornei()
        {
            return Json(new { myOutput = db.GetAllTornei() });
        }

        [HttpGet("GetNomiSquadreInPartita/{TitoloTorneo}/NumeroPartita/{NumPartita}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore")]
        public ActionResult<InfoMsg> GetNomiSquadreInPartita(string TitoloTorneo, int NumPartita)
        {
            return Ok(db.GetInfoSquadre(db.GetIDTorneo(TitoloTorneo), NumPartita));
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

        [HttpGet("GetPartita/{TitoloTorneo}/NumeroPartita/{NumPartita}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore")]
        public ActionResult<InfoMsg> GetPartita(string TitoloTorneo, int NumPartita)
        {
            return Ok(db.GetPartita(db.GetIDTorneo(TitoloTorneo), NumPartita));
        }

        [HttpGet("GetTorneoByTitolo/{titolo}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore")]
        public JsonResult GetTorneoByTitolo(string titolo)
        {
            return Json(new { myOutput = db.GetTorneoByTitolo(db.GetIDTorneo(titolo)) });
        }
    }
}