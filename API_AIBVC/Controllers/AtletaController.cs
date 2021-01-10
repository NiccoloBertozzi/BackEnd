using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAuthJWT.Helpers;
using API_Login_Registra.Models;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace API_AIBVC.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/atleti")]
    public class AtletaController : Controller
    {
        Database db = new Database();

        // restituisce i campi di anagrafica
        [HttpGet("GetAnagraficaAtleta/{Atleti_Id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin")]
        public DataTable GetAnagrafica(int Atleti_Id)
        {
            return db.GetAnagrafica(Atleti_Id);
        }
        //Restituisce le informazioni fondamentali relative a tutti i tornei in cui un atleta è iscritto
        [HttpGet("Iscrizioni/{Atleti_Id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Admin")]

        public DataTable GetIscrizioni(int Atleti_Id)
        {
            return db.GetIscrizioni(Atleti_Id);
        }
    }
}
