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
            if (db.UploadResults(aggiornaSet.IdTorneo, aggiornaSet.NumPartita, aggiornaSet.NumSet, aggiornaSet.PuntiTeam1, aggiornaSet.PuntiTeam2))
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
        public DataTable GetTorneoByID(int id)
        {
            HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "POST, GET, OPTIONS, DELETE, PUT");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "Authorization, Cookie");
            return db.GetTorneoByID(id)[0];
        }
        //ritorna nome cognome e id di un supervisore in base al cf
        [HttpGet("GetIDSupervisore/{CF}/Nome/{Nome}/Cognome/{Cognome}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetNomeCognomeSupervisore(string CF, string Nome, string Cognome)
        {
            return db.GetNomeCognomeSupervisore(CF,Nome,Cognome);
        }

        //ritorna nome cognome e id di un arbitro in base al cf
        [HttpGet("GetIDArbitro/{CF}/Nome/{Nome}/Cognome/{Cognome}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetNomeCognomeArbitro(string CF, string Nome, string Cognome)
        {
            return db.GetNomeCognomeArbitro(CF, Nome, Cognome);
        }

        //ritorna nome cognome e id di un direttore in base al cf
        [HttpGet("GetIDDirettore/{CF}/Nome/{Nome}/Cognome/{Cognome}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetNomeCognomeDirettore(string CF, string Nome, string Cognome)
        {
            return db.GetNomeCognomeDirettore(CF, Nome, Cognome);
        }

        //ritorna la lista dei supervisori
        [HttpGet("GetSupervisori")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetInfoSupervisore()
        {
            return db.GetInfoSupervisore();
        }

        //ritorna la lista dei arbitri
        [HttpGet("GetArbitri")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetInfoArbitro()
        {
            return db.GetInfoArbitro();
        }
        //ritorna la lista dei supervisori
        [HttpGet("GetDirettori")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetInfoDirettore()
        {
            return db.GetInfoDirettore();
        }
        //ritorna anagrafica supervisore
        [HttpGet("GetAnagraficaDelegato/{Supervisori_Id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Admin")]
        public DataTable GetAnagraficaDelegato(int Supervisori_Id)
        {
            return db.GetAnagraficaDelegato(Supervisori_Id);
        }

        [HttpPut("AssegnaCampo")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Delegato,Admin")]
        public ActionResult<InfoMsg> AssegnaCampo([FromBody] AssegnaCampo assegnaCampo)
        {
            if (db.AssegnaCampo(assegnaCampo.IdPartita, assegnaCampo.NumeroCampo))
                return Ok(new InfoMsg(DateTime.Today, $"Campo assegnato con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nella assegnazione del campo"));
        }
    }
}
