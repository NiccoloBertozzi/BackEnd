using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAuthJWT.Helpers;
using API_AIBVC.Models;
using TEST.Models;

namespace API_AIBVC.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/tornei")]
    public class TorneoController : Controller
    {
        Database db = new Database();

        //Restituisce tornei prima della data inserita
        [HttpGet("GetTornei/{Data}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin,Delegato,Allenatore,Admin")]
        public DataTable GetTornei(DateTime Data)
        {
            return db.GetTorneiEntroData(Data);
        }
        //Restituisce tornei prima della data inserita
        [HttpGet("GetTorneiNonAutorizzati/{Data}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin,Delegato,Allenatore,Admin")]
        public DataTable GetTorneiNonAutorizzati(DateTime Data)
        {
            return db.GetTorneiNonAutorizzatiEntroData(Data);
        }

        [HttpPost("CreaTorneo")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Admin")]
        public ActionResult<InfoMsg> CreaTorneo([FromBody]AddTorneo torneo)
        {
            if (db.CreaTorneo(torneo.Titolo, torneo.PuntiVittoria, torneo.Montepremi, Convert.ToDateTime(torneo.DataChiusuraIscrizioni), Convert.ToDateTime(torneo.DataInizio), Convert.ToDateTime(torneo.DataFine), torneo.Genere, torneo.FormulaTorneo, torneo.NumTeamTabellone, torneo.NumTeamQualifiche, torneo.ParametriTorneo, torneo.TipoTorneo, torneo.Impianti,torneo.QuotaIscrizione,torneo.IDSocieta))
                return Ok(new InfoMsg(DateTime.Today, $"Torneo creato con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nella creazione del torneo"));
        }

        //crea una nuova squadra se non esite gia
        [HttpPost("InserisciSquadra")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta")]
        public Nullable<int> InsertSquadra([FromBody]Squadra squadra)
        {
            Nullable<int> idsquadra = db.InsertSquadra(squadra.Atleta1, squadra.Atleta2, squadra.NomeTeam);
            if (idsquadra != null)
                return idsquadra;
            else
                return null;
        }
        //iscrive una suadra ad un torneo
        [HttpPost("IscriviSquadra")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta")]
        public ActionResult<InfoMsg> IscriviSquadra([FromBody]ListaIscritti iscritti)
        {
            if (db.IscriviSquadra(iscritti.IDTorneo, iscritti.IDSquadra, iscritti.IDAllenatore))
                return Ok(new InfoMsg(DateTime.Today, $"Squadra Iscritta con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante l'iscrizione della squadra"));
        }

        //ritorna la lista degli atleti di una societa
        [HttpPost("AtletiSocieta/{idsocieta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin,Delegato,Allenatore,Admin")]
        public DataTable GetAtletiSocieta(int idsocieta)
        {
            return db.GetAtletiSocieta(idsocieta);
        }

        //ritorna la lista degli allenatori di una societa
        [HttpPost("AllenatoriSocieta/{idSocieta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin,Delegato,Allenatore,Admin")]
        public DataTable GetAllenatoriSocieta(int idsocieta)
        {
            return db.GetAllenatoreSocieta(idsocieta);
        }

        //ritorna allenatore in base alla tessera
        [HttpPost("AllenatoriTessera/{tessera}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin,Delegato,Allenatore,Admin")]
        public string GetAllenatoreTessera(string tessera)
        {
            return db.GetAllenatoreByTessera(tessera);
        }

        //ritorna atleta in base alla tessera
        [HttpPost("AtletaTessera/{tessera}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin,Delegato,Allenatore,Admin")]
        public string GetAtletaTessera(string tessera)
        {
            return db.GetAtletaByTessera(tessera);
        }

        //Restituisce tornei prima della data inserita
        [HttpGet("GetPartite/{NumeroPartite}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetTornei(int NumeroPartite)
        {
            return db.GetPartite(NumeroPartite);
        }

        //Return TipoTorneo
        [HttpGet("TipoTorneo")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetTipoTorneo()
        {
            return db.GetTipoTorneo();
        }

        //Return ListaDelegati
        [HttpGet("ListaDelegati/{tipo}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetDelegato(int tipo)
        {
            return db.GetDelegato(tipo);
        }

        //Return ListaDelegati
        [HttpGet("FormulaTorneo")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetFormula()
        {
            return db.GetFormula();
        }

        //Return ParametriTorneo
        [HttpGet("ParametroTorneo")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetParametriTorneo()
        {
            return db.GetParametriTorneo();
        }

        //Return Impianti società
        [HttpGet("GetImpianti/{idSocieta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetImpianti(int idSocieta)
        {
            return db.GetImpianti(idSocieta);
        }

        //Autorizza il torneo
        [HttpPut("AutorizzaTorneo/{idTorneo}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Admin")]
        public ActionResult<InfoMsg> AutorizzaTorneo(int idTorneo)
        {
            if (db.AutorizzaTorneo(idTorneo))
                return Ok(new InfoMsg(DateTime.Today, $"Torneo autorizzato con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante l'autorizzazione del torneo"));
        }

        //Assegnazione dei delegati del torneo
        [HttpPut("AssegnaDelegati")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "AdminDelegato")]
        public ActionResult<InfoMsg> AssegnaDelegati([FromBody]AddDelegatiTorneo delgatiTorn)
        {
            if (db.AssegnaSupervisori(delgatiTorn.NomeSupervisore, delgatiTorn.CognomeSupervisore, delgatiTorn.NomeSupArbitrale, delgatiTorn.CognomeSupArbitrale, delgatiTorn.NomeDirettore, delgatiTorn.CognomeDirettore, delgatiTorn.TitoloTorneo))
                return Ok(new InfoMsg(DateTime.Today, $"Delegati assegnati con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante l'assegnazione dei delegati"));
        }

        [HttpDelete("EliminaSquadra")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato")]
        public ActionResult<InfoMsg> EliminaSquadra([FromBody]EliminaTeam eliminaTeam)
        {
            if (db.ControlloSupervisore(eliminaTeam.IdDelegato, eliminaTeam.IdTorneo))
            {
                if (db.EliminaTeam(eliminaTeam.IdTorneo, eliminaTeam.IdSquadra))
                    return Ok(new InfoMsg(DateTime.Today, $"Team eliminato dal torneo con successo"));
                else
                    return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante l'eliminazione del team"));
            }
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Non sei il supervisore di questo torneo"));
        }

        //elimina la squdra da parte di un atleta
        [HttpDelete("EliminaSquadraByAtleta")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Admin")]
        public ActionResult<InfoMsg> EliminaSquadraByAtleta([FromBody]EliminaTeam eliminaTeam)
        {
            //ritorno risposta
            return Ok(new InfoMsg(DateTime.Today, db.EliminaTeamByAtleta(eliminaTeam.IdTorneo, eliminaTeam.IdSquadra)));
        }

        [HttpPut("AssegnaWildCard/{IDTorneo}/{IDDelegato}/{IDSquadra}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato")]
        public ActionResult<InfoMsg> AssegnaWildCard(int idTorneo, int idDelegato, int idSquadra)
        {
            if (db.ControlloSupervisore(idDelegato, idTorneo))
            {
                if (db.AssegnaWildCard(idTorneo, idSquadra))
                    return Ok(new InfoMsg(DateTime.Today, $"Assegnata la WC alla squadra con successo"));
                else
                    return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante l'assegnazione della WC alla squadra"));
            }
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Non sei il supervisore di questo torneo"));
        }
    }
}
