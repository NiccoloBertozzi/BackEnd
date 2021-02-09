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
    [Route("api/v1/allenatori")]
    public class AllenatoreController : Controller
    {
        Database db = new Database();

        // restituisce i campi di anagrafica
        [HttpGet("GetAnagraficaAllenatore/{Allenatore_Id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Allenatore,Societa,Admin")]
        public DataTable GetAnagrafica(int Allenatore_Id)
        {
            return db.GetAnagraficaAllenatore(Allenatore_Id);
        }

        [HttpPut("UpdateAllenatore")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Allenatore,Admin")]
        public ActionResult<InfoMsg> UpdateAllenatore([FromBody] UpdateAllenatore allenatoreLogin)
        {
            if (db.GetIDSocieta(allenatoreLogin.NomeSocieta).Rows.Count > 0)//controllo e prendo IDSocieta
            {
                allenatoreLogin.allenatore.IDSocieta = Convert.ToInt32(db.GetIDSocieta(allenatoreLogin.NomeSocieta).Rows[0][0]);
                allenatoreLogin.allenatore.IDComuneNascita = "";
                allenatoreLogin.allenatore.IDComuneResidenza = "";
                if (allenatoreLogin.ComuneNascita != null) if (db.GetIDComuneNascita(allenatoreLogin.ComuneNascita).Rows.Count > 0) allenatoreLogin.allenatore.IDComuneNascita = db.GetIDComuneNascita(allenatoreLogin.ComuneNascita).Rows[0][0].ToString();
                if (allenatoreLogin.ComuneResidenza != null) if (db.GetIDComuneResidenza(allenatoreLogin.ComuneResidenza).Rows.Count > 0) allenatoreLogin.allenatore.IDComuneResidenza = db.GetIDComuneResidenza(allenatoreLogin.ComuneResidenza).Rows[0][0].ToString();
                //registro
                if (db.UpdateAnagraficaAllenatore(allenatoreLogin.allenatore))
                    return Ok(new InfoMsg(DateTime.Today, $"Modifica dell'allenatore {allenatoreLogin.allenatore.Nome} eseguito con successo."));
                else
                    return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori nella modifica dell'allenatore {allenatoreLogin.allenatore.Nome}."));
            }
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Società non trovata."));
        }
    }
}
