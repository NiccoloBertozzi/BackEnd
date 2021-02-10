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
        public DataTable GetTornei(DateTime Data)
        {
            return db.GetTorneiEntroData(Data);
        }        //Restituisce tornei prima della data inserita

        //get Tornei Iniziati
        [HttpGet("GetIscrizioniIniziate/{idAtleta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Admin")]
        public DataTable GetIscrizioniIniziate(int idAtleta)
        {
            return db.GetIscrizioniIniziate(idAtleta);
        }
        //Restituisce tornei prima della data inserita
        [HttpGet("GetTorneiNonAutorizzati")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Admin")]
        public DataTable GetTorneiNonAutorizzati()
        {
            return db.GetTorneiNonAutorizzatiEntroData();
        }

        [HttpPost("CreaTorneo")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Admin")]
        public ActionResult<InfoMsg> CreaTorneo([FromBody]AddTorneo torneo)
        {
            if (db.CreaTorneo(torneo.Titolo, torneo.PuntiVittoria, torneo.Montepremi, Convert.ToDateTime(torneo.DataChiusuraIscrizioni), Convert.ToDateTime(torneo.DataInizio), Convert.ToDateTime(torneo.DataFine), torneo.Genere, torneo.FormulaTorneo, torneo.NumMaxTeamMainDraw, torneo.NumMaxTeamQualifiche, torneo.ParametriTorneo, torneo.TipoTorneo, torneo.QuotaIscrizione,torneo.IDSocieta,torneo.NumTeamQualificati,torneo.NumWildCard, torneo.IDImpianto, torneo.Outdoor, torneo.RiunioneTecnica, torneo.OraInizio))
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
        [HttpPost("AllenatoriTessera/{tessera}/Societa/{Societa}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin,Delegato,Allenatore,Admin")]
        public string GetAllenatoreTessera(string tessera,int Societa)
        {
            return db.GetAllenatoreByTessera(tessera,Societa);
        }

        //ritorna atleta in base alla tessera
        [HttpPost("AtletaTessera/{tessera}/Societa/{Societa}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin,Delegato,Allenatore,Admin")]
        public string GetAtletaTessera(string tessera, int Societa)
        {
            return db.GetAtletaByTessera(tessera,Societa);
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

        //Return Tornei atleta Finiti
        [HttpGet("ToreniFiniti/{idAtleta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Admin,Atleta")]
        public DataTable GetTorneiFinitiAtleta(int idAtleta)
        {
            return db.GetTorneiFinitiAtleta(idAtleta);
        }

        //Return ParametriTorneo Iscritti
        [HttpGet("TorneiIscritti/{idAtleta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Admin,Atleta")]
        public DataTable GetidTorneiIscritti(int idAtleta)
        {
            return db.GetidTorneiIscritti(idAtleta);
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
            if (db.AssegnaSupervisori(delgatiTorn.IdSupervisore,delgatiTorn.IdSupArbitrale,delgatiTorn.IdDirettore,delgatiTorn.IdTorneo))
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
            //Eliminazione della squadra da parte del supervisore del torneo
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

        //elimina la squdra da parte di un supervisore
        [HttpDelete("EliminaSquadraBySupervisore")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Admin")]
        public ActionResult<InfoMsg> EliminaSquadraBySupervisore([FromBody]EliminaTeam eliminaTeam)
        {
            //ritorno risposta
            return Ok(new InfoMsg(DateTime.Today, db.EliminaSquadraBySupervisore(eliminaTeam.IdTorneo, eliminaTeam.IdSquadra,eliminaTeam.IdSupervisore)));
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
        
        [HttpPost("AddArbitriTorneo")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Admin")]
        public ActionResult<InfoMsg> AddArbitriTorneo([FromBody]ArbitraTorneo arbitraTorneo)
        {
            if (db.ControlloSupervisore(arbitraTorneo.IDDelegato, arbitraTorneo.IDTorneo))
            {
                if(db.AddArbitro(arbitraTorneo.IDArbitro,arbitraTorneo.IDTorneo,arbitraTorneo.MezzaGiornata))
                    return Ok(new InfoMsg(DateTime.Today, $"Arbitro aggiunto al torneo con successo"));
                else
                    return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante l'aggiunta dell'arbitro al torneo"));
            }
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Non sei il supervisore di questo torneo"));
        }

        [HttpGet("GetArbitriTorneo/{IDTorneo}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Delegato,Allenatore,Admin")]
        public JsonResult GetArbitriTorneo(int idTorneo)
        {
            return Json(new { output = db.GetArbitriTorneo(idTorneo) });
        }

        [HttpPost("GeneraListaIngresso/{IDTorneo}/{IDSupervisore}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Admin")]
        public ActionResult<InfoMsg> GeneraListaIngresso(int idTorneo,int idSupervisore)
        {
            //Metodo che crea la lista d'ingresso definitiva del torneo
            if (db.ControlloSupervisore(idSupervisore,idTorneo))
            {
                if(db.CreaListaIngresso(idTorneo))
                    return Ok(new InfoMsg(DateTime.Today, $"Lista di ingresso creata con successo"));
                else
                    return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante la creazione della lista di ingresso"));
            }
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Non sei il supervisore di questo torneo"));
        }

        [HttpGet("GetTorneiSvoltiBySupervisore/{IDSupervisore}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Admin")]
        public DataTable GetTorneiSvoltiBySupervisore(int idSupervisore)
        {
            //Metodo che restituisce i torni a cui ha partecipato un supervisore
            return db.GetTorneiDisputatiByDelegato(idSupervisore);
        }

        //torna squadre di un torneo
        [HttpGet("SquadreTorneo/{IdTorneo}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atltea,Societa,Delegato,Admin")]
        public DataTable SquadreTorneo(int IdTorneo)
        {
            //Metodo che restituisce i torni a cui ha partecipato un supervisore
            return db.SquadreTorneo(IdTorneo);
        }

        //torna infosquadra
        [HttpGet("Squadra/{Idsquadra}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atltea,Societa,Delegato,Admin")]
        public DataTable GetSquadra(int Idsquadra)
        {
            return db.GetSquadra(Idsquadra);
        }

        [HttpGet("GetTorneiFinitiByAtleta/{IDAtleta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atltea,Admin")]
        public JsonResult GetTorneiFinitiByAtleta(int idAtleta)
        {
            return Json(new { output = db.GetTorneiFinitiByAtleta(idAtleta) });
        }
    }
}
