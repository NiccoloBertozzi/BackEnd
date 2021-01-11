using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAuthJWT.Helpers;

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
        [HttpGet("GetSocieta/{nomeSocieta}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore,Admin")]
        public JsonResult GetSocieta(string nomeSocieta)
        {
            return Json(new { output = db.GetSocieta(nomeSocieta) });
        }
    }
}
