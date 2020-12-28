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
    public class TorneoController : Controller
    {
        Database db = new Database();

        //Restituisce tornei prima della data inserita
        [HttpGet("GetTornei/{Data}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin,Delegato,Allenatore")]
        public DataTable GetTornei(DateTime Data)
        {
            return db.GetTorneiEntroData(Data);
        }
    }
}