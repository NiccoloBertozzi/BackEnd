using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Route("api/v1/societa")]
    public class SocietaController : Controller
    {
        Database db = new Database();
        [HttpGet("SpecificaTorneo/{TitoloTorneo}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public JsonResult SpecificaTorneo(string titoloTorneo)
        {
            return Json(new { output = db.GetTorneoEPartecipanti(titoloTorneo) });
        }
        //Restituisce tornei prima della data inserita
        [HttpGet("GetTorneiSocieta/{Data}/IdSocieta/{idsocieta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Admin")]
        public DataTable GetTornei(DateTime Data,int idsocieta)
        {
            return db.GetTorneiEntroDataSocieta(Data,idsocieta);
        }
        [HttpPost("AddImpianto")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles ="Societa,Admin")]
        public ActionResult<InfoMsg> AddImpianto([FromBody] AddImpianto aggiungiImp)
        {
            if (db.AggiungiImpianto(aggiungiImp.NomeComune,aggiungiImp.NomeImpianto,aggiungiImp.NumeroCampi,aggiungiImp.Indirizzo,aggiungiImp.CAP,aggiungiImp.Descrizione,aggiungiImp.Email,aggiungiImp.Sito,aggiungiImp.Tel,aggiungiImp.IdSocieta))
                return Ok(new InfoMsg(DateTime.Today, $"Impianto aggiunto con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante l'aggiunta dell'impianto"));
        }
    }
}
