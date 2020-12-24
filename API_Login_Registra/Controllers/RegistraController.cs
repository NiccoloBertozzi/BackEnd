using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAuthJWT.Helpers;
using TEST.Models;
using API_Login_Registra.Models;

namespace API_Login_Registra.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/registrati")]
    public class RegistraController : Controller
    {
        Database db = new Database();

        [HttpPost("RegistraAllenatore")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<InfoMsg> RegisterAllenatore([FromBody] AllenatoreLogin allenatoreLogin)
        {
            if (db.GetIDComuneNascita(allenatoreLogin.cred.ComuneNascita).Rows.Count > 0 && db.GetIDComuneResidenza(allenatoreLogin.cred.ComuneResidenza).Rows.Count > 0 && db.GetIDSocieta(allenatoreLogin.cred.NomeSocieta).Rows.Count > 0)
            {
                allenatoreLogin.allenatore.IDComuneNascita = db.GetIDComuneNascita(allenatoreLogin.cred.ComuneNascita).Rows[0][0].ToString();
                allenatoreLogin.allenatore.IDComuneResidenza = db.GetIDComuneResidenza(allenatoreLogin.cred.ComuneResidenza).Rows[0][0].ToString();
                allenatoreLogin.allenatore.IDSocieta = Convert.ToInt32(db.GetIDSocieta(allenatoreLogin.cred.NomeSocieta).Rows[0][0]);
                if (db.RegisterAllenatore(allenatoreLogin.allenatore.IDSocieta, allenatoreLogin.allenatore.CodiceTessera, allenatoreLogin.allenatore.Grado, allenatoreLogin.allenatore.Nome, allenatoreLogin.allenatore.Cognome, allenatoreLogin.allenatore.Sesso, allenatoreLogin.allenatore.CF, allenatoreLogin.allenatore.DataNascita, allenatoreLogin.allenatore.IDComuneNascita, allenatoreLogin.allenatore.IDComuneResidenza, allenatoreLogin.allenatore.Indirizzo, allenatoreLogin.allenatore.CAP, allenatoreLogin.allenatore.Email, allenatoreLogin.allenatore.Tel, allenatoreLogin.cred.Password))
                    return Ok(new InfoMsg(DateTime.Today, $"Inserimento dell'allenatore { allenatoreLogin.allenatore.Nome} eseguito con successo."));
                else
                    return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori in inserimento dell'allenatore { allenatoreLogin.allenatore.Nome}."));
            }
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori in inserimento dell'allenatore { allenatoreLogin.allenatore.Nome}."));
        }
        [HttpPost("RegistraAtleta")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<InfoMsg> RegisterAtleta([FromBody] AtletaLogin atletalogin)
        {
            if (db.GetIDComuneNascita(atletalogin.cred.ComuneNascita).Rows.Count > 0 && db.GetIDComuneResidenza(atletalogin.cred.ComuneResidenza).Rows.Count > 0 && db.GetIDSocieta(atletalogin.cred.NomeSocieta).Rows.Count > 0)
            {
                atletalogin.atleta.IDComuneNascita = db.GetIDComuneNascita(atletalogin.cred.ComuneNascita).Rows[0][0].ToString();
                atletalogin.atleta.IDComuneResidenza = db.GetIDComuneResidenza(atletalogin.cred.ComuneResidenza).Rows[0][0].ToString();
                atletalogin.atleta.IDSocieta = Convert.ToInt32(db.GetIDSocieta(atletalogin.cred.NomeSocieta).Rows[0][0]);
                if (db.RegisterAtleta(atletalogin.atleta.IDSocieta, atletalogin.atleta.CodiceTessera, atletalogin.atleta.Nome, atletalogin.atleta.Cognome, atletalogin.atleta.Sesso, atletalogin.atleta.CF, atletalogin.atleta.DataNascita, atletalogin.atleta.IDComuneNascita, atletalogin.atleta.IDComuneResidenza, atletalogin.atleta.Indirizzo, atletalogin.atleta.CAP, atletalogin.atleta.Email, atletalogin.atleta.Tel, atletalogin.atleta.Altezza, atletalogin.atleta.Peso, atletalogin.atleta.DataScadenzaCertificato, atletalogin.cred.Password))
                    return Ok(new InfoMsg(DateTime.Today, $"Inserimento dell'atleta {atletalogin.atleta.Nome} eseguito con successo."));
                else
                    return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori in inserimento dell'atleta {atletalogin.atleta.Nome}."));
            }
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori in inserimento dell'atleta {atletalogin.atleta.Nome}."));
        }
        [HttpPost("RegistraDelegato")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<InfoMsg> RegisterDelegato([FromBody] DelegatoLogin delegatologin)
        {
            if(db.GetIDComuneNascita(delegatologin.cred.ComuneNascita).Rows.Count > 0 && db.GetIDComuneResidenza(delegatologin.cred.ComuneResidenza).Rows.Count > 0)
            {
                delegatologin.delegato.IDComuneNascita = db.GetIDComuneNascita(delegatologin.cred.ComuneNascita).Rows[0][0].ToString();
                delegatologin.delegato.IDComuneResidenza = db.GetIDComuneResidenza(delegatologin.cred.ComuneResidenza).Rows[0][0].ToString();
                if (db.RegisterDelegato(delegatologin.delegato.Nome, delegatologin.delegato.Cognome, delegatologin.delegato.Sesso, delegatologin.delegato.CF, delegatologin.delegato.DataNascita, delegatologin.delegato.IDComuneNascita, delegatologin.delegato.IDComuneResidenza, delegatologin.delegato.Indirizzo, delegatologin.delegato.CAP, delegatologin.delegato.Email, delegatologin.delegato.Tel, delegatologin.delegato.Arbitro, delegatologin.delegato.Supervisore, delegatologin.cred.Password))
                    return Ok(new InfoMsg(DateTime.Today, $"Inserimento del delegato {delegatologin.delegato.Nome} eseguito con successo."));
                else
                    return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori in inserimento del delegato {delegatologin.delegato.Nome}."));
            }
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori in inserimento del delegato {delegatologin.delegato.Nome}."));
        }
        [HttpPost("RegistraSocieta")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<InfoMsg> RegisterSocieta([FromBody] SocietaLogin societalogin)
        {
            if (db.GetIDComuneResidenza(societalogin.cred.ComuneResidenza).Rows.Count > 0)
            {
                societalogin.societa.IDComune = db.GetIDComuneResidenza(societalogin.cred.ComuneResidenza).Rows[0][0].ToString();
                if (db.RegisterSocieta(societalogin.societa.IDComune, societalogin.societa.NomeSocieta, societalogin.societa.Indirizzo, societalogin.societa.CAP, societalogin.societa.DataFondazione, societalogin.societa.DataAffiliazione, societalogin.societa.CodiceAffiliazione, societalogin.societa.Affiliata, societalogin.societa.Email, societalogin.societa.Sito, societalogin.societa.Tel1, societalogin.societa.Tel2, societalogin.societa.Pec, societalogin.societa.PIVA, societalogin.societa.CF, societalogin.societa.CU, societalogin.cred.Password))
                    return Ok(new InfoMsg(DateTime.Today, $"Inserimento della societa {societalogin.societa.NomeSocieta} eseguito con successo."));
                else
                    return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori in inserimento della societa {societalogin.societa.NomeSocieta}."));
            }
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori in inserimento della societa {societalogin.societa.NomeSocieta}."));
        }
    }
}
