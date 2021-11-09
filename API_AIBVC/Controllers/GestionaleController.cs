using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAuthJWT.Helpers;

namespace API_AIBVC.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("v1/gestionale")]
    public class GestionaleController : Controller
    {
        Database db = new Database();

        [HttpGet("GetAllTorneiMaschili")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore")]
        public JsonResult GetAllTorneiMaschili()
        {
            return Json(new { output = db.GetAllTorneiMaschili() });
        }

        [HttpGet("GetAllTorneiFemminili")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore")]
        public JsonResult GetAllTorneiFemminili()
        {
            return Json(new { output = db.GetAllTorneiFemminili() });
        }
        //in base al sesso passato ritorna la classifica
        [HttpGet("GetClassifica/{sesso}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        public DataTable GetClassificaMaschile(string sesso)
        {
            return db.GetClassifica(sesso);
        }
        //in base al sesso passato ritorna la classifica del tour 
        [HttpGet("GetClassificaTour/{sesso}/Tour/{tour}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        public DataTable GetClassificaTour(string sesso,string tour)
        {
            return db.GetClassificaTour(sesso, tour);
        }

        //punti atleta di ogni tappa
        [HttpGet("GetPuntiAletaTappa/{tour}/Atleta/{idatleta}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        public DataTable GetPuntiAtletaTappa(string tour, int idatleta)
        {
            return db.GetPuntiAtletaTappa(tour, idatleta);
        }

        //numero tappe
        [HttpGet("GetTappe/{tour}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        public string[] GetNumeroTappe(string tour)
        {
            return db.GetTappe(tour);
        }

        [HttpGet("GetComuni")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        public DataTable GetComuni()
        {
            return db.GetComuni();
        }
    }
}
