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
        [HttpGet("GetTorneiSocieta/{Data}/{idsocieta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Admin")]
        public DataTable GetTornei(DateTime Data,int idsocieta)
        {
            return db.GetTorneiEntroDataSocieta(Data,idsocieta);
        }
    }
}
