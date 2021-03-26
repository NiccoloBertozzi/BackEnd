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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;

namespace API_AIBVC.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/tornei")]
    public class TorneoController : Controller
    {
        Database db = new Database();

        //Restituisce tornei prima della data inserita
        [HttpGet("GetTornei")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetTornei()
        {
            HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "POST, GET, OPTIONS, DELETE, PUT");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "Authorization, Cookie");
            return db.GetTorneiEntroData();
        }        //Restituisce tornei prima della data inserita

        //Restituisce tornei di un determinato tipo
        [HttpGet("GetTorneiTipo/{idTipo}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        public DataTable GetTorneiTipo(int idTipo)
        {
            HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Methods", "POST, GET, OPTIONS, DELETE, PUT");
            HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "Authorization, Cookie");
            return db.GetTorneiTipo(idTipo);
        }        //Restituisce tornei di un determinato tipo

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
        public ActionResult<InfoMsg> CreaTorneo([FromBody] AddTorneo torneo)
        {
            if (db.CreaTorneo(torneo.Titolo, torneo.PuntiVittoria, torneo.Montepremi, Convert.ToDateTime(torneo.DataChiusuraIscrizioni), Convert.ToDateTime(torneo.DataInizio), Convert.ToDateTime(torneo.DataFine), torneo.Genere, torneo.FormulaTorneo, torneo.NumMaxTeamMainDraw, torneo.NumMaxTeamQualifiche, torneo.ParametriTorneo, torneo.TipoTorneo, torneo.QuotaIscrizione, torneo.IDSocieta, torneo.NumTeamQualificati, torneo.NumWildCard, torneo.IDImpianto, torneo.Outdoor, torneo.RiunioneTecnica, torneo.OraInizio))
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
        public Nullable<int> InsertSquadra([FromBody] Squadra squadra)
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
        public ActionResult<InfoMsg> IscriviSquadra([FromBody] ListaIscritti iscritti)
        {
            if (db.IscriviSquadra(iscritti.IDTorneo, iscritti.IDSquadra, iscritti.IDAllenatore))
                return Ok(new InfoMsg(DateTime.Today, $"Squadra Iscritta con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante l'iscrizione della squadra"));
        }

        //ritorna la lista degli atleti di una societa
        [HttpGet("AtletiSocieta/{idsocieta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(InfoMsg))]
        [Authorize(Roles = "Atleta,Societa,Admin,Delegato,Allenatore,Admin")]
        public DataTable GetAtletiSocieta(int idsocieta)
        {
            return db.GetAtletiSocieta(idsocieta);
        }

        //ritorna la lista degli allenatori di una societa
        [HttpGet("AllenatoriSocieta/{idSocieta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(InfoMsg))]
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
        public string GetAllenatoreTessera(string tessera, int Societa)
        {
            return db.GetAllenatoreByTessera(tessera, Societa);
        }

        //ritorna atleta in base alla tessera
        [HttpPost("AtletaTessera/{tessera}/Societa/{Societa}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atleta,Societa,Admin,Delegato,Allenatore,Admin")]
        public string GetAtletaTessera(string tessera, int Societa)
        {
            return db.GetAtletaByTessera(tessera, Societa);
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

        //Return Tornei atleta Finiti
        [HttpGet("TorneInCorso/{idAtleta}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Admin,Atleta")]
        public DataTable GetTorneiInCorsoAlteta(int idAtleta)
        {
            return db.GetTorneiInCorsoAlteta(idAtleta);
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

        //Autorizza o non autorizza il torneo
        [HttpPut("AutorizzaTorneo/{idTorneo}/{Autorizza}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Admin")]
        public string AutorizzaTorneo(int idTorneo, bool autorizzaONo)
        {
            return db.AutorizzaTorneo(idTorneo, autorizzaONo);
        }

        //Assegnazione dei delegati del torneo
        [HttpPut("AssegnaDelegati")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "AdminDelegato")]
        public ActionResult<InfoMsg> AssegnaDelegati([FromBody] AddDelegatiTorneo delgatiTorn)
        {
            if (db.AssegnaSupervisori(delgatiTorn.IdSupervisore, delgatiTorn.IdSupArbitrale, delgatiTorn.IdDirettore, delgatiTorn.IdTorneo))
                return Ok(new InfoMsg(DateTime.Today, $"Delegati assegnati con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante l'assegnazione dei delegati"));
        }

        [HttpDelete("EliminaSquadra")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato")]
        public ActionResult<InfoMsg> EliminaSquadra([FromBody] EliminaTeam eliminaTeam)
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
        public ActionResult<InfoMsg> EliminaSquadraByAtleta([FromBody] EliminaTeam eliminaTeam)
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
        public ActionResult<InfoMsg> EliminaSquadraBySupervisore([FromBody] EliminaTeam eliminaTeam)
        {
            //ritorno risposta
            return Ok(new InfoMsg(DateTime.Today, db.EliminaSquadraBySupervisore(eliminaTeam.IdTorneo, eliminaTeam.IdSquadra, eliminaTeam.IdSupervisore)));
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
        public ActionResult<InfoMsg> AddArbitriTorneo([FromBody] ArbitraTorneo arbitraTorneo)
        {
            if (db.ControlloSupervisore(arbitraTorneo.IDDelegato, arbitraTorneo.IDTorneo))
            {
                if (db.AddArbitro(arbitraTorneo.IDArbitro, arbitraTorneo.IDTorneo, arbitraTorneo.MezzaGiornata))
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

        /*[HttpPost("GeneraListaIngresso/{IDTorneo}/{IDSupervisore}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Admin")]
        public ActionResult<InfoMsg> GeneraListaIngresso(int idTorneo,int idSupervisore)
        {
            //Metodo che crea la lista d'ingresso definitiva del torneo
            if (db.ControlloSupervisore(idSupervisore,idTorneo))
            {
                if(db.CreaListaIngressoETorneoQualifica(idTorneo))
                    return Ok(new InfoMsg(DateTime.Today, $"Lista di ingresso creata con successo"));
                else
                    return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore durante la creazione della lista di ingresso"));
            }
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Non sei il supervisore di questo torneo"));
        }*/

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

        [HttpGet("GetPartiteTorneo/{IDTorneo}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Atltea,Societa,Delegato,Admin")]
        public DataTable GetPartiteTorneo(int idTorneo)
        {
            return db.GetPartiteTorneo(idTorneo);
        }

        [HttpPut("AssegnaInfoPartita")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Admin")]
        public string AssegnaInfoPartita([FromBody] InfoPartita infoPartita)
        {
            return db.AssegnaInfoPartita(infoPartita.IDArbitro1, infoPartita.IDArbitro2, infoPartita.Campo, Convert.ToDateTime(infoPartita.DataPartita), Convert.ToDateTime(infoPartita.OraPartita), infoPartita.IDPartita);
        }

        [HttpPut("UpdateInfoTorneo")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Delegato,Admin")]
        public string UpdateInfoTorneo([FromBody]AddTorneo modTorneo)
        {
            return db.UpdateTorneo(modTorneo.Titolo, modTorneo.PuntiVittoria, modTorneo.Montepremi, Convert.ToDateTime(modTorneo.DataChiusuraIscrizioni), Convert.ToDateTime(modTorneo.DataInizio), Convert.ToDateTime(modTorneo.DataFine), modTorneo.Genere, modTorneo.IDFormulaTorneo, modTorneo.NumMaxTeamMainDraw, modTorneo.NumMaxTeamQualifiche, modTorneo.IDParametriTorneo, modTorneo.IDTipoTorneo, modTorneo.QuotaIscrizione, modTorneo.IDSocieta, modTorneo.NumTeamQualificati, modTorneo.NumWildCard, modTorneo.IDImpianto, modTorneo.Outdoor, modTorneo.RiunioneTecnica, modTorneo.OraInizio, modTorneo.IDSupervisore, modTorneo.IDSupArbitrale, modTorneo.IDDirettore, Convert.ToDateTime(modTorneo.DataPubblicazioneLista), modTorneo.VisibilitaListaIngresso, modTorneo.UrlLocandina, modTorneo.IDTorneo);
        }

        [HttpPost("CreaListaIngresso/{IDTorneo}/{IDSupervisore}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Admin")]
        public string CreaListaIngresso(int idTorneo, int idSupervisore)
        {
            //Metodo che crea la lista d'ingresso definitiva del torneo
            if (db.ControlloSupervisore(idSupervisore, idTorneo))
                return db.CreaLista(idTorneo);
            else
                return "Non sei il supervisore di questo torneo";
        }

        [HttpPost("CreaTorneoQualifiche/{IDTorneo}/{dataInizioQualifiche}/{dataFineQualifiche}/{dataPartite2Turno}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Admin")]
        public string CreaTorneoQualifiche(int idTorneo, string dataInizioQualifiche, string dataFineQualifiche, string dataPartite2Turno)
        {
            return db.CreaTorneoQualifica(idTorneo, Convert.ToDateTime(dataInizioQualifiche), Convert.ToDateTime(dataFineQualifiche), Convert.ToDateTime(dataPartite2Turno));
        }

        [HttpPut("AssegnaArbitriPartite")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Admin")]
        public string AssegnaArbitriPartite([FromBody]AddArbitroPartita addArbitroPart)
        {
            if (db.ControlloSupervisore(addArbitroPart.IDDelegato, addArbitroPart.IDTorneo))
                return db.AssegnaArbitriPartita(addArbitroPart.IDArbitro1, addArbitroPart.IDArbitro2, addArbitroPart.IDTorneo, addArbitroPart.NumPartita);
            else
                return "Non sei il supervisore di questo torneo";
        }

        [HttpGet("GetStatoTorneo/{IDTorneo}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Societa,Admin")]
        public string GetStatoTorneo(int idTorneo)
        {
            return db.GetStatoTornei(idTorneo);
        }

        [HttpGet("GetListaIngresso/{IDTorneo}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Societa,Atleta,Admin")]
        public DataTable GetListaIngresso(int idTorneo)
        {
            return db.GetListaIngresso(idTorneo);
        }

        [HttpPost("AvanzaTabelloneQualifiche/{IDTorneoQualifiche}/{NumPartita}/{IDTorneoPrincipale}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Delegato,Admin")]
        public string AvanzaTabelloneQualifiche(int idTorneoQualifiche, int numPartita, int idTorneoPrincipale)
        {
            return db.AvanzaTabelloneQualifiche(idTorneoQualifiche, numPartita, idTorneoPrincipale);
        }
    }
}
