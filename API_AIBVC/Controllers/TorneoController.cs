﻿using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Societa, Delegato, Atleta, Allenatore")]
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
        [HttpPost("CreaTorneo")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(DataTable))]
        [Authorize(Roles = "Societa,Admin")]
        public ActionResult<InfoMsg> CreaTorneo([FromBody]AddTorneo torneo)
        {
            if (db.CreaTorneo(torneo.Titolo, torneo.PuntiVittoria, torneo.Montepremi, torneo.DataChiusuraIscrizioni, torneo.DataInizio, torneo.DataFine, torneo.Genere, torneo.FormulaTorneo, torneo.NumTeamTabellone, torneo.NumTeamQualifiche, torneo.ParametriTorneo, torneo.TipoTorneo, torneo.Impianti))
                return Ok(new InfoMsg(DateTime.Today, $"Torneo creato con successo"));
            else
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Errore nella creazione del torneo"));
        }

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
    }
}
