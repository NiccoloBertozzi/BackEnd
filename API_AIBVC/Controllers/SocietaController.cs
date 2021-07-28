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
        [HttpGet("SpecificaTorneo/{IDTorneo}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public JsonResult SpecificaTorneo(int IDTorneo)
        {
            return Json(new { output = db.GetTorneoEPartecipanti(IDTorneo) });
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

        //Restituisce tornei prima della data inserita
        [HttpGet("GetInfoSocieta/{idsocieta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public DataTable GetSocieta(int idsocieta)
        {
            return db.GetInfoSocieta(idsocieta);
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

        // restituisce i campi di anagrafica di una societa
        [HttpGet("GetAnagraficaSocieta/{Societa_Id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin")]
        public DataTable GetAnagraficaSocieta(int Societa_Id)
        {
            return db.GetAnagraficaSocieta(Societa_Id);
        }

        //Restituisce gli impianti di una società
        [HttpGet("GetAllImpianti/{IDSocieta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Delegato,Admin")]
        public DataTable GetImpianti(int idSocieta)
        {
            return db.GetAllImpiantiSocieta(idSocieta);
        }
        //Restituisce un impianto di una società
        [HttpGet("GetImpianto/{IDSocieta}/Impianto/{IDImpianto}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Delegato,Admin,Atleta,Allenatore")]
        public DataTable GetImpianto(int idSocieta, int IDImpianto)
        {
            return db.GetImpiantoSocieta(idSocieta, IDImpianto);
        }
        //Restituisce tutte le societa
        [HttpGet("GetAllSocieta")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetAllSocieta()
        {
            return db.GetAllSocieta();
        }

        //Restituisce tutti i tornei della società
        [HttpGet("GetAllTorneiSocieta/{IDSocieta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Admin")]
        public JsonResult GetAllTorneiSocieta(int idSocieta)
        {
            return Json(new { output = db.GetTorneiBySocieta(idSocieta) });
        }

        //Restituisce tutti gli impianti di tutte le societa
        [HttpGet("GetAllImpianti")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Admin,Delegato")]
        public DataTable GetAllImpianti()
        {
            return db.GetAllImpianti();
        }

        //Assegna le tessere agli atleti
        [HttpPost("AssegnaTessereBySocieta")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Admin")]
        public ActionResult<InfoMsg> AssegnaTessereBySocieta([FromBody]AssegnaTessere assegnaTessere)
        {
            if(db.AssegnaTessereBySocieta(assegnaTessere.IDAtleta,assegnaTessere.IDSocieta,assegnaTessere.CodiceTessera,assegnaTessere.TipoTessera,assegnaTessere.DataTesseramento,assegnaTessere.AnnoTesseramento,assegnaTessere.Importo))
                return Ok(new InfoMsg(DateTime.Today, $"Tessera aggiunta con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante l'aggiunta della tessera"));
        }

        //Assegna le tessere agli allenatori
        [HttpPost("AssegnaTessereAllenatoreBySocieta")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Admin")]
        public ActionResult<InfoMsg> AssegnaTessereAllenatoreBySocieta([FromBody]AssegnaTessere assegnaTessere)
        {
            if (db.AssegnaTessereAllenatoreBySocieta(assegnaTessere.IDAtleta, assegnaTessere.IDSocieta, assegnaTessere.CodiceTessera, assegnaTessere.TipoTessera, assegnaTessere.DataTesseramento, assegnaTessere.AnnoTesseramento, assegnaTessere.Importo))
                return Ok(new InfoMsg(DateTime.Today, $"Tessera aggiunta con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante l'aggiunta della tessera"));
        }

        //Informazioni Tessere Societa
        [HttpGet("TessereSocieta/{idsocieta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetTesseraInfo(int idsocieta)
        {
            //[Authorize(Roles = "Societa,Admin")]
            HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "POST, GET, OPTIONS, DELETE, PUT");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "Authorization, Cookie");
            return db.GetTesseraInfo(idsocieta);
        }

        //Informazioni Tessere Societa allenatori
        [HttpGet("TessereSocietaAllenatore/{idsocieta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Admin")]
        public DataTable GetTesseraInfoAllenatore(int idsocieta)
        {
            return db.GetTesseraInfoAllenatore(idsocieta);
        }

        //numero campi impianto
        [HttpGet("NumeroCampiSocieta/{idimpianto}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Admin,Delegato")]
        public DataTable GetNumeroCampiSocieta(int idimpianto)
        {
            return db.GetNumeroCampiSocieta(idimpianto);
        }
    }
}
