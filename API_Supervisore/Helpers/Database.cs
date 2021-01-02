using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace API_Supervisore.Helpers
{
    public class Database
    {
        SqlConnectionStringBuilder builder;
        SqlConnection conn;
        SqlDataAdapter adapter;
        SqlCommand comando;
        DataTable query;
        string sql;

        // parametri token JWT
        string JWT_secretKey = ConfigurationManager.AppSetting["AppSettings:Secret"];
        int JWT_expirationMinutes = Convert.ToInt32(ConfigurationManager.AppSetting["AppSettings:ExpirationMinute"]);
        public Database()
        {
            builder = new SqlConnectionStringBuilder();
            builder.DataSource = ConfigurationManager.AppSetting["DBSettings:DataSource"];
            builder.UserID = ConfigurationManager.AppSetting["DBSettings:UserID"];
            builder.Password = ConfigurationManager.AppSetting["DBSettings:Password"];
            builder.InitialCatalog = ConfigurationManager.AppSetting["DBSettings:InitialCatalog"];
            conn = new SqlConnection(builder.ConnectionString);
        }
        public int GetIDTorneo(string titolo)//Metodo che restituisce l'ID del torneo cercato
        {
            try
            {
                sql = "";
                sql += "SELECT IDTorneo FROM Torneo WHERE Titolo='" + titolo + "'";
                query = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return Convert.ToInt32(query.Rows[0][0]);
            }
            catch
            {
                return 0;
            }
        }
        public bool SetNuovaPsw(string email, string psw)//Metodo per aggiornare la password dell'utente
        {
            try
            {
                sql = "";
                sql += "UPDATE Login SET PWD='" + psw + "',DataUltimoCambioPwd='" + DateTime.Now.Date + "',DataRichiestaCambioPwd='" + DateTime.Now.Date + "',DataUltimoAccesso='" + DateTime.Now.Date + "' " +
                    "WHERE Email='" + email + "'";
                comando = new SqlCommand(sql, conn);
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public DataTable[] GetAllTornei()//Metodo che restituisce tutti i tornei autorizzati
        {
            DataTable[] risultati = new DataTable[2];
            DataTable ris1, ris2;
            sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,Supervisore.Nome as NomeSupervisore,Supervisore.Cognome as CognomeSupervisore,SupervisoreArbitrale.Nome as NomeSupervisoreArbitrale,SupervisoreArbitrale.Cognome as CognomeSupervisoreArbitrale,DirettoreCompetizione.Nome as NomeDirettoreCompetizione,DirettoreCompetizione.Cognome as CognomeDirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumTeamTabellone,Torneo.NumTeamQualifiche " +
            "FROM(((((((((Torneo Left join TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)Left Join DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT join ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT join DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)Left join DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT Join FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)Left Join ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)left join Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)Left Join Comune On Impianto.IDComune = Comune.IDComune) " +
            "WHERE Torneo.Autorizzato=1";
            ris1 = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(ris1);
            conn.Close();
            sql = "";
            sql += "SELECT Titolo,NomeParametro " +
            "FROM ParametroQualita, ParametroTorneo, Torneo " +
            "WHERE Torneo.Autorizzato=1 AND ParametroTorneo.idtorneo = Torneo.idtorneo AND ParametroTorneo.IDParametro = ParametroQualita.IDParametro";
            ris2 = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(ris2);
            conn.Close();
            risultati[0] = ris1;
            risultati[1] = ris2;
            return risultati;
        }
        /*public DataTable GetTorneiByData(DateTime data)//Metodo che restituisce tutti i tornei che ci sono entro una certa data
        {
            sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,Supervisore.Nome as NomeSupervisore,Supervisore.Cognome as CognomeSupervisore,SupervisoreArbitrale.Nome as NomeSupervisoreArbitrale,SupervisoreArbitrale.Cognome as CognomeSupervisoreArbitrale,DirettoreCompetizione.Nome as NomeDirettoreCompetizione,DirettoreCompetizione.Cognome as CognomeDirettoreCompetizione,FormulaTorneo.Formula,ParametroQualita.NomeParametro,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumTeamTabellone,Torneo.NumTeamQualifiche " +
            "FROM TipoTorneo,DelegatoTecnico as Supervisore,DelegatoTecnico as SupervisoreArbitrale,DelegatoTecnico as DirettoreCompetizione,FormulaTorneo,ParametroQualita,Impianto,Comune,Torneo,ParametroTorneo,ImpiantoTorneo,ArbitraTorneo " +
            "WHERE CAST(Torneo.DataFine as DATE) >= '" + data.Date.ToString() + "' AND Torneo.IDTipoTorneo=TipoTorneo.IDTipoTorneo " + //Tipo torneo +
            "AND Torneo.IDSupervisore = Supervisore.IDDelegato AND(ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale AND Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato) AND Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato " +
            "AND Torneo.IDFormula = FormulaTorneo.IDFormula " +
            "AND ParametroTorneo.IDTorneo = Torneo.IDTorneo AND ParametroTorneo.IDParametro = ParametroQualita.IDParametro " +
            "AND ImpiantoTorneo.IDTorneo = Torneo.IDTorneo AND ImpiantoTorneo.IDImpianto = Impianto.IDImpianto " +
            "AND Impianto.IDComune = Comune.IDComune";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }*/
        public DataTable GetInfoSquadre(int idTorneo, int numPartita)//Metodo che restituisce i nomi delle squadre in una partita
        {
            sql = "";
            sql += "SELECT t1.NomeTeam as Team1,t2.NomeTeam As Team2 " +
            "FROM((Partita LEFT JOIN Squadra t1 ON Partita.idsq1 = t1.idsquadra) LEFT JOIN Squadra t2 ON Partita.idsq2 = t2.idsquadra) " +
            "WHERE Partita.IDTorneo=" + idTorneo + " AND Partita.NumPartita=" + numPartita;
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public bool UploadResults(int idTorneo, int idPartita, int numSet, int puntiTeam1, int puntiTeam2)
        {
            //Metodo che aggiurna i risultati di una partita
            try
            {
                sql = "";
                sql += "UPDATE Partita ";
                switch (numSet)
                {
                    case 1:
                        sql += "SET PT1S1=" + puntiTeam1 + ",PT2S1=" + puntiTeam2 + " ";
                        break;
                    case 2:
                        sql += "SET PT1S2=" + puntiTeam1 + ",PT2S2=" + puntiTeam2 + " ";
                        break;
                    case 3:
                        sql += "SET PT1S3=" + puntiTeam1 + ",PT2S3=" + puntiTeam2 + " ";
                        break;
                }
                sql += "WHERE IDPartita=" + idPartita + " AND IDTorneo=" + idTorneo;
                comando = new SqlCommand(sql, conn);
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public DataTable GetPartita(int idTorneo, int numPartita)//Metodo che restituisce le informazioni di una partita
        {
            sql = "";
            sql += "SELECT " +
            "CONCAT(A1.Nome,' ',A1.cognome) as Atleta1,CONCAT(A2.Nome,' ',A2.cognome) as Atleta2,S1.NomeTeam as Team1, " +
            "CONCAT(A3.Nome,' ',A3.cognome) as Atleta3,CONCAT(A4.Nome,' ',A4.cognome) AS Atleta4, S2.NomeTeam as Team2, " +
            "CONCAT(D1.Nome,' ',D1.Cognome) as Arbitro1,CONCAT(D2.Nome,' ',D2.Cognome) as Arbitro2, " +
            "Partita.Fase,Partita.Campo,Partita.DataPartita,Partita.OraPartita, " +
            "Partita.PT1S1 as PtTeam1Set1,Partita.PT2S1 as PtTeam2Set1,Partita.PT1S2 as PtTeam1Set2,Partita.PT2S2 as PtTeam2Set2, " +
            "Partita.PT1S3 as PtTeam1Set3,Partita.PT2S3 as PtTeam2Set3 " +
            "FROM((((((((" +
            "Partita LEFT JOIN Squadra S1 ON Partita.idsq1 = S1.idsquadra) LEFT JOIN Squadra S2 ON Partita.idsq2 = S2.idsquadra) " +
            "left JOIN Atleta A1 ON S1.IDAtleta1 = A1.IDAtleta) LEFT JOIN Atleta A2 ON S1.IDAtleta2 = A2.IDAtleta) " +
            "left JOIN Atleta A3 ON S2.IDAtleta1 = A3.IDAtleta) LEFT JOIN Atleta A4 ON S2.IDAtleta2 = A4.IDAtleta) " +
            "left JOIN DelegatoTecnico D1 ON Partita.idarbitro1 = D1.IDDelegato) LEFT JOIN DelegatoTecnico D2 ON Partita.idarbitro2 = D2.IDDelegato) " +
            "WHERE Partita.IDTorneo =" + idTorneo + " AND Partita.NumPartita = " + numPartita +";";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable[] GetTorneoByTitolo(int idTorneo)//Metodo che restituisce un torneo tramite l'ID
        {
            DataTable[] risultati = new DataTable[2];
            DataTable ris1, ris2;
            sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,Supervisore.Nome as NomeSupervisore,Supervisore.Cognome as CognomeSupervisore,SupervisoreArbitrale.Nome as NomeSupervisoreArbitrale,SupervisoreArbitrale.Cognome as CognomeSupervisoreArbitrale,DirettoreCompetizione.Nome as NomeDirettoreCompetizione,DirettoreCompetizione.Cognome as CognomeDirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumTeamTabellone,Torneo.NumTeamQualifiche " +
            "FROM(((((((((Torneo Left join TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)Left Join DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT join ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT join DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)Left join DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT Join FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)Left Join ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)left join Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)Left Join Comune On Impianto.IDComune = Comune.IDComune) " +
            "WHERE Torneo.IDTorneo=" + idTorneo;
            ris1 = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(ris1);
            conn.Close();
            sql = "";
            sql += "SELECT NomeParametro " +
            "FROM ParametroQualita, ParametroTorneo, Torneo " +
            "WHERE Torneo.IDTorneo=" + idTorneo + " AND ParametroTorneo.idtorneo = Torneo.idtorneo AND ParametroTorneo.IDParametro = ParametroQualita.IDParametro";
            ris2 = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(ris2);
            conn.Close();
            risultati[0] = ris1;
            risultati[1] = ris2;
            return risultati;
        }
    }
}
