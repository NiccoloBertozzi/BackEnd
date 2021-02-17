using API_Login_Registra.Models;
using API_Supervisore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System;
using System.Net.Http;
using WebAPIAuthJWT.Helpers;

namespace API_AIBVC.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/LoginRegister")]
    public class LoginRegisterController : Controller
    {
        Database db = new Database();

        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status405MethodNotAllowed, Type = typeof(InfoMsg))]
        public IActionResult Login([FromBody] Credenziali credenziali)
        {
            string[] risposta;
            string tokenJWT = "";
            if (db.Authenticate(credenziali.Email, credenziali.Password))
            {
                risposta = db.GetToken(credenziali.Email);
                tokenJWT = risposta[0];
                //Set cookie con ruolo
                HttpContext.Response.Cookies.Append("ruolo", risposta[2]);
            }
            else
                return BadRequest(new InfoMsg(DateTime.Today, string.Format($"Username e/o Password errati.")));
            return Ok(new { token = tokenJWT, id = risposta[1] });
        }
        [HttpPost("RegistraAllenatore")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<InfoMsg> RegisterAllenatore([FromBody] AllenatoreLogin allenatoreLogin)
        {
            if (db.GetIDSocieta(allenatoreLogin.cred.NomeSocieta).Rows.Count > 0)
            {
                allenatoreLogin.allenatore.IDSocieta = Convert.ToInt32(db.GetIDSocieta(allenatoreLogin.cred.NomeSocieta).Rows[0][0]);
                allenatoreLogin.allenatore.IDComuneNascita = "";
                allenatoreLogin.allenatore.IDComuneResidenza = "";
                if (allenatoreLogin.cred.ComuneNascita != null) if (db.GetIDComuneNascita(allenatoreLogin.cred.ComuneNascita).Rows.Count > 0) allenatoreLogin.allenatore.IDComuneNascita = db.GetIDComuneNascita(allenatoreLogin.cred.ComuneNascita).Rows[0][0].ToString();
                if (allenatoreLogin.cred.ComuneResidenza != null) if (db.GetIDComuneResidenza(allenatoreLogin.cred.ComuneResidenza).Rows.Count > 0) allenatoreLogin.allenatore.IDComuneResidenza = db.GetIDComuneResidenza(allenatoreLogin.cred.ComuneResidenza).Rows[0][0].ToString();
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
            if (db.GetIDSocieta(atletalogin.cred.NomeSocieta).Rows.Count > 0)//controllo e prendo IDSocieta
            {
                atletalogin.atleta.IDSocieta = Convert.ToInt32(db.GetIDSocieta(atletalogin.cred.NomeSocieta).Rows[0][0]);
                atletalogin.atleta.IDComuneNascita = "";
                atletalogin.atleta.IDComuneResidenza = "";
                if (atletalogin.cred.ComuneNascita != null) if (db.GetIDComuneNascita(atletalogin.cred.ComuneNascita).Rows.Count > 0) atletalogin.atleta.IDComuneNascita = db.GetIDComuneNascita(atletalogin.cred.ComuneNascita).Rows[0][0].ToString();
                if (atletalogin.cred.ComuneResidenza != null) if (db.GetIDComuneResidenza(atletalogin.cred.ComuneResidenza).Rows.Count > 0) atletalogin.atleta.IDComuneResidenza = db.GetIDComuneResidenza(atletalogin.cred.ComuneResidenza).Rows[0][0].ToString();
                //registro
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
            delegatologin.delegato.IDComuneNascita = "";
            delegatologin.delegato.IDComuneResidenza = "";
            if (delegatologin.cred.ComuneNascita != null) if (db.GetIDComuneNascita(delegatologin.cred.ComuneNascita).Rows.Count > 0) delegatologin.delegato.IDComuneNascita = db.GetIDComuneNascita(delegatologin.cred.ComuneNascita).Rows[0][0].ToString();
            if (delegatologin.cred.ComuneResidenza != null) if (db.GetIDComuneResidenza(delegatologin.cred.ComuneResidenza).Rows.Count > 0) delegatologin.delegato.IDComuneResidenza = db.GetIDComuneResidenza(delegatologin.cred.ComuneResidenza).Rows[0][0].ToString();
            if (db.RegisterDelegato(delegatologin.delegato.Nome, delegatologin.delegato.Cognome, delegatologin.delegato.Sesso, delegatologin.delegato.CF, delegatologin.delegato.DataNascita, delegatologin.delegato.IDComuneNascita, delegatologin.delegato.IDComuneResidenza, delegatologin.delegato.Indirizzo, delegatologin.delegato.CAP, delegatologin.delegato.Email, delegatologin.delegato.Tel, delegatologin.delegato.Arbitro, delegatologin.delegato.Supervisore, delegatologin.cred.Password))
                return Ok(new InfoMsg(DateTime.Today, $"Inserimento del delegato {delegatologin.delegato.Nome} eseguito con successo."));
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
                if (db.RegisterSocieta(societalogin.societa.IDComune, societalogin.societa.NomeSocieta, societalogin.societa.IndirizzoSoc, societalogin.societa.CAPSoc, societalogin.societa.DataFondazione, societalogin.societa.DataAffiliazione, societalogin.societa.CodiceAffiliazione, societalogin.societa.Affiliata, societalogin.societa.EmailSoc, societalogin.societa.Sito, societalogin.societa.Tel1, societalogin.societa.Tel2, societalogin.societa.Pec, societalogin.societa.PIVA, societalogin.societa.CFSoc, societalogin.societa.CU, societalogin.cred.Password))
                    return Ok(new InfoMsg(DateTime.Today, $"Inserimento della societa {societalogin.societa.NomeSocieta} eseguito con successo."));
                else
                    return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori in inserimento della societa {societalogin.societa.NomeSocieta}."));
            }
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori in inserimento della societa {societalogin.societa.NomeSocieta}."));
        }
        [HttpPut("CambiaPsw")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<InfoMsg> CambiaPsw([FromBody] SetNuovaPsw setNPSW)
        {
            PasswordHasher hasher = new PasswordHasher();
            setNPSW.Password = hasher.Hash(setNPSW.Password);
            if (db.SetNuovaPsw(setNPSW.Email, setNPSW.Password))
                return Ok(new InfoMsg(DateTime.Today, $"Password cambiata con successo."));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nel cambio password."));
        }

        [HttpPost("RecuperaPassword")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<InfoMsg> RecuperaPassword([FromBody] SetNuovaPsw setNPSW)
        {
            if (db.RecuperaPassword(setNPSW.Email))
                return Ok(new InfoMsg(DateTime.Today, $"OK"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nel cambio password."));
        }

        [HttpPut("UpdateAtleta")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        [HttpPut("UpdateSocieta")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Societa,Admin")]
        public ActionResult<InfoMsg> UpdateSocieta([FromBody] UpdateSocieta societalogin)
        {
            if (societalogin.ComuneSocieta != null) 
                if (db.GetIDComuneNascita(societalogin.ComuneSocieta).Rows.Count > 0)
                    societalogin.societa.IDComune = db.GetIDComuneNascita(societalogin.ComuneSocieta).Rows[0][0].ToString();
            if (db.UpdateAnagraficaSocieta(societalogin.societa))
                return Ok(new InfoMsg(DateTime.Today, $"Modifica società {societalogin.societa.NomeSocieta} eseguito con successo."));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori nella modifica della società {societalogin.societa.NomeSocieta}."));
        }

        [HttpPut("UpdateDelegatoTecnico")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InfoMsg))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Delegato,Admin")]
        public ActionResult<InfoMsg> UpdateDelegatoTecnico([FromBody] UpdateDelegatoTecnico delegatologin)
        {
            //ritorna null
            delegatologin.delegato.IDComuneNascita = "";
            delegatologin.delegato.IDComuneResidenza = "";
            if (delegatologin.ComuneNascita != null) 
                if (db.GetIDComuneNascita(delegatologin.ComuneNascita).Rows.Count > 0) 
                    delegatologin.delegato.IDComuneNascita = db.GetIDComuneNascita(delegatologin.ComuneNascita).Rows[0][0].ToString();
            if (delegatologin.ComuneResidenza != null) 
                if (db.GetIDComuneResidenza(delegatologin.ComuneResidenza).Rows.Count > 0) 
                    delegatologin.delegato.IDComuneResidenza = db.GetIDComuneResidenza(delegatologin.ComuneResidenza).Rows[0][0].ToString();
            if (db.UpdateAnagraficaDelegato(delegatologin.delegato))
                return Ok(new InfoMsg(DateTime.Today, $"Modifica del delegato {delegatologin.delegato.Nome} eseguito con successo."));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errori nella modifica del delegato {delegatologin.delegato.Nome}."));
        }
    }
}
