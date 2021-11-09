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
    [Route("v1/supervisore")]
    public class SupervisoreController : Controller
    {
        Database db = new Database();
        [HttpGet("GetAllTornei")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public JsonResult GetAllTornei()
        {
            return Json(new { myOutput = db.GetAllTornei() });
        }
        [HttpPost("GetNomiSquadreInPartita")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public ActionResult<InfoMsg> GetNomiSquadreInPartita([FromBody] CercaTeam cercaTeam)
        {
            return Ok(db.GetInfoSquadre(db.GetIDTorneo(cercaTeam.TitoloTorneo), cercaTeam.NumPartita));
        }
        [HttpPut("AggiornaRisultati")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public string AggiornaRisultati([FromBody] AggiornaSet aggiornaSet)
        {
            //Mi servono: IDTorneo,IDPartita,Numero del set e i punti fatti dalle 2 squadre
            return db.UploadResults(aggiornaSet.IdTorneo, aggiornaSet.NumPartita, aggiornaSet.IDPartita, aggiornaSet.pt1s1, aggiornaSet.pt2s1, aggiornaSet.pt1s2, aggiornaSet.pt2s2, aggiornaSet.pt1s3, aggiornaSet.pt2s3, aggiornaSet.NumSet, aggiornaSet.IDTorneoPrincipale);
        }
        [HttpGet("GetPartita/{idtorneo}/Partita/{numpartita}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        public DataTable GetPartita(int idtorneo, int numpartita)
        {
            return db.GetPartita(idtorneo, numpartita);
        }
        [HttpGet("GetIDPartita/{idtorneo}/Partita/{numpartita}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public int GetIdPartita(int idtorneo, int numpartita)
        {
            return db.GetIdPartita(idtorneo, numpartita);
        }
        [HttpGet("GetTorneoById/{id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        public DataTable GetTorneoByID(int id)
        {
            return db.GetTorneoByID(id)[0];
        }
        //ritorna nome cognome e id di un supervisore in base al cf
        [HttpGet("GetIDSupervisore/{CF}/Nome/{Nome}/Cognome/{Cognome}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetNomeCognomeSupervisore(string CF, string Nome, string Cognome)
        {
            return db.GetNomeCognomeSupervisore(CF,Nome,Cognome);
        }

        //ritorna nome cognome e id di un arbitro in base al cf
        [HttpGet("GetIDArbitro/{CF}/Nome/{Nome}/Cognome/{Cognome}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetNomeCognomeArbitro(string CF, string Nome, string Cognome)
        {
            return db.GetNomeCognomeArbitro(CF, Nome, Cognome);
        }

        //ritorna nome cognome e id di un direttore in base al cf
        [HttpGet("GetIDDirettore/{CF}/Nome/{Nome}/Cognome/{Cognome}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetNomeCognomeDirettore(string CF, string Nome, string Cognome)
        {
            return db.GetNomeCognomeDirettore(CF, Nome, Cognome);
        }

        //ritorna la lista dei supervisori
        [HttpGet("GetSupervisori")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetInfoSupervisore()
        {
            return db.GetInfoSupervisore();
        }

        //ritorna la lista dei arbitri
        [HttpGet("GetArbitri")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetInfoArbitro()
        {
            return db.GetInfoArbitro();
        }
        //ritorna la lista dei supervisori
        [HttpGet("GetDirettori")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
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
