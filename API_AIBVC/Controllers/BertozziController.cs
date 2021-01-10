using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPIAuthJWT.Helpers;

namespace API_Login_Registra.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Authorize(Roles = "Societa, Delegato, Atleta, Allenatore")]
    [Route("api/v1/")]
    public class BertozziController : Controller
    {
        Database db = new Database();

        //Restituisce partite di un torneo
        [HttpGet("Atleta/StoricoTornei/SpecificheStoricoTorneo/{idTorneo}/StoricoPartite")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin,Delegato,Allenatore")]
        public DataTable GetTornei(int IdTorneo)
        {
            return db.GetStoricoPartiteTorneo(IdTorneo);
        }

        //Inserisce l'allenatore all'interno di una squadra
        [HttpPost("Allenatore/Torneo/{idTorneo}/Squadra/{idSquadra}/Allenatore/{idAllenatore}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa")]
        public ActionResult<InfoMsg> InsertAllenatore(int idTorneo, int idSquadra, int idAllenatore)
        {
            if(db.SetAllenatoreSquadra(idTorneo, idSquadra, idAllenatore))
                return Ok(new InfoMsg(DateTime.Today, $"Allenatore assegnato con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nell'assegnamento dell'allenatore"));
        }
    }
}