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
    [Route("v1/atleti")]
    public class AtletaController : Controller
    {
        Database db = new Database();

        // get all atleti
        [HttpGet("GetAtleti")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Admin,Delegato")]
        public DataTable GetAtleti()
        {
            return db.GetAtleti();
        }

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

        // restituisce l'id dela societa in base al id dell'alttela
        [HttpGet("GetIdSocieta/{Atleti_Id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin")]
        public int Getidsocieta(int Atleti_Id)
        {
            return db.GetIdSocietaByAtleta(Atleti_Id);
        }
        // restituisce l'id dela squadra dal nome team
        [HttpGet("GetIdSquadra/{Atleti_Id}/Torneo/{IdTorneo}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin")]
        public int Getidsquadra(int Atleti_Id,int IdTorneo)
        {
            return db.GetIDSquadraByNomeTeam(Atleti_Id, IdTorneo);
        }

        [HttpPut("UpdateAtleta")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Atleta,Admin")]
        public ActionResult<InfoMsg> UpdateAtleta([FromBody] UpdateAtleta atletalogin)
        {
            if (db.GetIDSocieta(atletalogin.NomeSocieta).Rows.Count > 0)//controllo e prendo IDSocieta
            {
                atletalogin.atleta.IDSocieta = Convert.ToInt32(db.GetIDSocieta(atletalogin.NomeSocieta).Rows[0][0]);
                atletalogin.atleta.IDComuneNascita = "";
                atletalogin.atleta.IDComuneResidenza = "";
                if (atletalogin.ComuneNascita != null) if (db.GetIDComuneNascita(atletalogin.ComuneNascita).Rows.Count > 0) atletalogin.atleta.IDComuneNascita = db.GetIDComuneNascita(atletalogin.ComuneNascita).Rows[0][0].ToString();
                if (atletalogin.ComuneResidenza != null) if (db.GetIDComuneResidenza(atletalogin.ComuneResidenza).Rows.Count > 0) atletalogin.atleta.IDComuneResidenza = db.GetIDComuneResidenza(atletalogin.ComuneResidenza).Rows[0][0].ToString();
                //registro
                if (db.UpdateAnagraficaAtleta(atletalogin.atleta))
                    return Ok(new InfoMsg(DateTime.Today, $"Modifica dell'atleta {atletalogin.atleta.Nome} eseguito con successo."));
                else
                    return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori nella modifica dell'atleta {atletalogin.atleta.Nome}."));
            }
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Società non trovata."));
        }
    }
}
