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
        [HttpGet("GetClassificaMaschile")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore")]
        public JsonResult GetClassificaMaschile()
        {
            return Json(new { output = db.GetClassificaMaschile() });
        }
        [HttpGet("GetClassificaFemminile")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Delegato,Atleta,Societa,Allenatore")]
        public JsonResult GetClassificaFemminile()
        {
            return Json(new { output = db.GetClassificaFemminile() });
        }
    }
}
