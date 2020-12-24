using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAuthJWT.Helpers;
using API_Login_Registra.Models;

namespace API_Login_Registra.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/login")]
    public class LoginController : Controller
    {
        Database db = new Database();

        [HttpGet("Login")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(InfoMsg))]
        public IActionResult Login([FromBody] Credenziali credenziali)
        {
            string tokenJWT = "";
            if (db.Authenticate(credenziali.Email, credenziali.Password))
                tokenJWT = db.GetToken(credenziali.Email);
            else
                return BadRequest(new InfoMsg(DateTime.Today, string.Format($"Username e/o Password errati.")));
            return Ok(new { token = tokenJWT });
        }
    }
}
