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
    [Route("api/v1/gestionale")]
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
            HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "POST, GET, OPTIONS, DELETE, PUT");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "Authorization, Cookie");
            return db.GetClassifica(sesso);
        }

        [HttpGet("GetComuni")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        public DataTable GetComuni()
        {
            return db.GetComuni();
        }
    }
}
