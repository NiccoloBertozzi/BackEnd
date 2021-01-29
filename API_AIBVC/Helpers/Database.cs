using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using TEST.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;

namespace WebAPIAuthJWT.Helpers
{
    public class Database
    {
        SqlConnectionStringBuilder builder;
        SqlConnection conn;


        // parametri token JWT
        string JWT_secretKey = ConfigurationManager.AppSetting["AppSettings:Secret"];
        int JWT_expirationMinutes = Convert.ToInt32(ConfigurationManager.AppSetting["AppSettings:ExpirationMinute"]);

        public Database()//Richiamo il file appsettings
        {
            builder = new SqlConnectionStringBuilder();
            builder.DataSource = ConfigurationManager.AppSetting["DBSettings:DataSource"];
            builder.UserID = ConfigurationManager.AppSetting["DBSettings:UserID"];
            builder.Password = ConfigurationManager.AppSetting["DBSettings:Password"];
            builder.InitialCatalog = ConfigurationManager.AppSetting["DBSettings:InitialCatalog"];
            conn = new SqlConnection(builder.ConnectionString);
        }
        public bool Authenticate(string email, string password)//Autenticazione dell'utente
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            bool autenticato = false;
            string pswCript;//Password criptata
            try
            {
                sql = "";
                sql += "SELECT * ";
                sql += "FROM Login ";
                sql += "WHERE Email=@Email";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("Email", email));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                PasswordHasher pswHasher = new PasswordHasher();
                if (query.Rows[0]["Email"].ToString().Length > 0)
                {
                    pswCript = query.Rows[0]["PWD"].ToString();
                    autenticato = pswHasher.Check(pswCript, password).Verified;
                }
            }
            catch (Exception e)
            {
                string errore = e.Message;
                autenticato = false;
            }
            return autenticato;
        }
        public string[] GetToken(string email)//Rilascio token
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable dtUtente = this.CheckUser(email);
            string[] risposta = new string[3];

            //Creazione del Token Jwt
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JWT_secretKey);
            //Query
            sql = "";
            sql += "SELECT * FROM Login WHERE Email=@Email";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Email", email));
            DataTable dtProfili = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(dtProfili);
            conn.Close();
            /*DataTable dtProfili = DBTable(
                string.Format("SELECT * FROM Login WHERE Email ='{0}'", email));*/

            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Name, dtUtente.Rows[0]["Email"].ToString()));

            foreach (DataRow dr in dtProfili.Rows)
            {
                if (dr["IDSocieta"].ToString() != "")
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Societa"));
                    risposta[1] = dtUtente.Rows[0]["IDSocieta"].ToString();
                    risposta[2] = "Societa";
                }
                else if (dr["IDDelegato"].ToString() != "")
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Delegato"));
                    if (Convert.ToInt32(dr["AdminDelegati"]) != 0)
                        claims.Add(new Claim(ClaimTypes.Role, "AdminDelegato"));
                    risposta[1] = dtUtente.Rows[0]["IDDelegato"].ToString();
                    risposta[2] = "Delegato";
                }
                else if (dr["IDAtleta"].ToString() != "")
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Atleta"));
                    risposta[1] = dtUtente.Rows[0]["IDAtleta"].ToString();
                    risposta[2] = "Atleta";
                }
                else if (dr["IDAllenatore"].ToString() != "")
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Allenatore"));
                    risposta[1] = dtUtente.Rows[0]["IDAllenatore"].ToString();
                    risposta[2] = "Allenatore";
                }
                if (Convert.ToInt32(dr["Admin"]) != 0)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    risposta[2] = "Admin";
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                //Validità del Token
                Expires = DateTime.UtcNow.AddMinutes(JWT_expirationMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            risposta[0] = tokenHandler.WriteToken(token);
            return risposta;
        }
        public DataTable CheckUser(string email)
        {
            string sql;
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            // prepara la QUERY
            sql = "";
            sql += "SELECT * ";
            sql += "FROM Login ";
            sql += "WHERE Email=@Email";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Email", email));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        /// <summary>
        /// Esegui una query SQL (INSERT/UPDATE/DELETE)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public DataTable GetIDComuneNascita(string comuneNascita)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            conn.Open();
            sql = "";
            sql += "SELECT IDComune ";
            sql += "FROM Comune ";
            sql += "WHERE Citta=@ComuneNascita";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("ComuneNascita", comuneNascita));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetIDComuneResidenza(string comuneResidenza)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT IDComune ";
            sql += "FROM Comune ";
            sql += "WHERE Citta=@ComuneResidenza";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("ComuneResidenza", comuneResidenza));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public int GetIDAllenatore(int tessera)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            conn.Open();
            string sql;
            sql = "";
            sql += "SELECT IDAllenatore ";
            sql += "FROM Allenatore ";
            sql += "WHERE Codicetessera=@Tessera";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Tessera", tessera.ToString()));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return Convert.ToInt32(query.Rows[0]["IDAllenatore"]);
        }
        public int GetIDDelegato(int tessera)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            conn.Open();
            string sql;
            sql = "";
            sql += "SELECT IDDelegato ";
            sql += "FROM DelegatoTecnico ";
            sql += "WHERE Codicetessera=@Tessera";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Tessera", tessera.ToString()));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return Convert.ToInt32(query.Rows[0]["IDDelegato"]);
        }
        public DataTable GetNomeCognomeSupervisore(string cf,string nome,string cognome)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            conn.Open();
            string sql;
            sql = "";
            sql += "SELECT IDDelegato, CONCAT(Nome,' ',Cognome)AS Delegato ";
            sql += "FROM DelegatoTecnico ";
            sql += "WHERE CF=@Codicefiscale AND Supervisore=1 AND Nome=@nome AND Cognome=@cognome";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Codicefiscale", cf));
            comando.Parameters.Add(new SqlParameter("nome", nome));
            comando.Parameters.Add(new SqlParameter("cognome", cognome));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetNomeCognomeArbitro(string cf, string nome, string cognome)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            conn.Open();
            sql = "";
            sql += "SELECT IDDelegato, CONCAT(Nome,' ',Cognome)AS Delegato ";
            sql += "FROM DelegatoTecnico ";
            sql += "WHERE CF=@Codicefiscale AND Arbitro=1 AND Nome=@nome AND Cognome=@cognome";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Codicefiscale", cf));
            comando.Parameters.Add(new SqlParameter("nome", nome));
            comando.Parameters.Add(new SqlParameter("cognome", cognome));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetNomeCognomeDirettore(string cf, string nome, string cognome)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            conn.Open();
            string sql;
            sql = "";
            sql += "SELECT IDDelegato, CONCAT(Nome,' ',Cognome)AS Delegato ";
            sql += "FROM DelegatoTecnico ";
            sql += "WHERE CF=@Codicefiscale AND Nome=@nome AND Cognome=@cognome ";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Codicefiscale", cf));
            comando.Parameters.Add(new SqlParameter("nome", nome));
            comando.Parameters.Add(new SqlParameter("cognome", cognome));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetInfoDirettore()
        {
            SqlDataAdapter adapter;
            DataTable query;
            conn.Open();
            string sql;
            sql = "";
            sql += "SELECT CONCAT(Cognome,' ',Nome)AS Delegato, CF ";
            sql += "FROM DelegatoTecnico ";
            sql += "ORDER BY Delegato ASC";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetInfoSupervisore()
        {
            SqlDataAdapter adapter;
            DataTable query;
            conn.Open();
            string sql;
            sql = "";
            sql += "SELECT CONCAT(Cognome,' ',Nome)AS Delegato, CF ";
            sql += "FROM DelegatoTecnico ";
            sql += "WHERE Supervisore=1 ";
            sql += "ORDER BY Delegato ASC";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(query);
            conn.Close();
            return query;
        }

        public DataTable GetInfoArbitro()
        {
            SqlDataAdapter adapter;
            DataTable query;
            conn.Open();
            string sql;
            sql = "";
            sql += "SELECT CONCAT(Cognome,' ',Nome)AS Delegato, CF ";
            sql += "FROM DelegatoTecnico ";
            sql += "WHERE Arbitro=1 ";
            sql += "ORDER BY Delegato ASC";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetIDSocieta(string nomeSocieta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            conn.Open();
            string sql;
            sql = "";
            sql += "SELECT IDSocieta ";
            sql += "FROM Societa ";
            sql += "WHERE NomeSocieta=@NomeSocieta";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("NomeSocieta", nomeSocieta));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetAnagrafica(int id_Atleta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            conn.Open();
            sql = "";
            sql += "SELECT Nome,Cognome,DataNascita,Email,Tel,Sesso ";
            sql += "FROM Atleta ";
            sql += "WHERE IDAtleta=@IDAtleta";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDAtleta", id_Atleta));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetAnagraficaSocieta(int id_Societa)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            conn.Open();
            sql = "";
            sql += "SELECT * ";
            sql += "FROM Societa ";
            sql += "WHERE IDSocieta=@IDSocieta";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDSocieta", id_Societa));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetAnagraficaDelegato(int id_Delegato)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            conn.Open();
            sql = "";
            sql += "SELECT * ";
            sql += "FROM DelegatoTecnico ";
            sql += "WHERE IDDelegato=@IDDelegato";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDDelegato", id_Delegato));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetIscrizioni(int idAtleta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            conn.Open();
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,PuntiVittoria,Torneo.Montepremi,DataInizio,DataFine,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,Squadra.NomeTeam,CONCAT(atleta1.Nome,' ',atleta1.cognome) AS Atleta1,CONCAT(atleta2.Nome,' ',atleta2.cognome) AS Atleta2 ";
            sql += "FROM (((((Torneo " +
            "LEFT JOIN TipoTorneo ON Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)" +
            "LEFT JOIN ListaIscritti ON ListaIscritti.IDTorneo=Torneo.IDTorneo)" +
            "LEFT JOIN Squadra ON ListaIscritti.IDSquadra = Squadra.IDSquadra)" +
            "LEFT JOIN Atleta atleta1 ON Squadra.IDAtleta1 = atleta1.IDAtleta)" +
            "LEFT JOIN Atleta atleta2 ON Squadra.IDAtleta2 = atleta2.IDAtleta)";
            sql += "WHERE Squadra.IDAtleta1=@IDAtleta or Squadra.IDAtleta2=@IDAtleta";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDAtleta", idAtleta));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetTorneiEntroData(DateTime data)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            conn.Open();
            string sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo, Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome, ' ', Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome, ' ', SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale, CONCAT(DirettoreCompetizione.Nome, ' ', DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche,Torneo.NumTeamQualificati,Torneo.NumWildCard " +
            "FROM(((((((((Torneo Left join TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)Left Join DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT join ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT join DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)Left join DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT Join FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)Left Join ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)left join Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)Left Join Comune On Impianto.IDComune = Comune.IDComune) " +
            " WHERE CAST(DataInizio as DATE) <= @Data AND Autorizzato= 1";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Data", data.Date));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetTorneiNonAutorizzatiEntroData(DateTime data)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            conn.Open();
            string sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo, Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome, ' ', Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome, ' ', SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale, CONCAT(DirettoreCompetizione.Nome, ' ', DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche" +
            "FROM(((((((((Torneo Left join TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)Left Join DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT join ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT join DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)Left join DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT Join FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)Left Join ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)left join Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)Left Join Comune On Impianto.IDComune = Comune.IDComune) " +
            " WHERE CAST(DataInizio as DATE) <= @Data AND Autorizzato= 0";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Data", data.Date.ToString()));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetTorneiEntroDataSocieta(DateTime data, int idsocieta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            conn.Open();
            string sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo, Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome, ' ', Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome, ' ', SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale, CONCAT(DirettoreCompetizione.Nome, ' ', DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche" +
            "FROM(((((((((Torneo Left join TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)Left Join DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT join ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT join DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)Left join DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT Join FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)Left Join ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)left join Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)Left Join Comune On Impianto.IDComune = Comune.IDComune) " +
            " WHERE CAST(DataInizio as DATE) <= @Data AND Autorizzato= 1 AND Torneo.IDSocieta= @idSocieta";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Data", data.Date));
            comando.Parameters.Add(new SqlParameter("idSocieta", idsocieta));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public bool RegisterAllenatore(int idSocieta, string codTessera, string grado, string nome, string cognome, string sesso, string cF, DateTime dataNascita, string comuneNascita, string comuneResidenza, string indirizzo, string cap, string email, string tel, string pwd)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable idAllenatore;
            bool regRiuscita = false;
            string sql;
            try
            {
                //Insert nella tabella Allenatore
                sql = "";
                sql += "INSERT INTO Allenatore(IDSocieta,CodiceTessera,Grado,Nome,Cognome,Sesso,CF,DataNascita,IDComuneNascita,IDComuneResidenza,Indirizzo,CAP,Email,Tel) ";
                sql += "VALUES (@IDSocieta,@CodiceTessera,@Grado,@Nome,@Cognome,@Sesso,@CF,@DataNascita,@IDComuneNascita,@IDComuneResidenza,@Indirizzo,@CAP,@Email,@Tel)";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("IDSocieta", idSocieta);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CodiceTessera", codTessera);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Grado", grado);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Nome", nome);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Cognome", cognome);
                comando.Parameters.Add(parametro);
                if (sesso != null)
                    parametro = new SqlParameter("Sesso", sesso);
                else
                    parametro = new SqlParameter("Sesso", DBNull.Value);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CF", cF);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataNascita", dataNascita);
                comando.Parameters.Add(parametro);
                if (comuneNascita != "")
                    parametro = new SqlParameter("IDComuneNascita", comuneNascita);
                else
                    parametro = new SqlParameter("IDComuneNascita", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (comuneResidenza != "")
                    parametro = new SqlParameter("IDComuneResidenza", comuneResidenza);
                else
                    parametro = new SqlParameter("IDComuneResidenza", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (indirizzo != null)
                    parametro = new SqlParameter("Indirizzo", indirizzo);
                else
                    parametro = new SqlParameter("Indirizzo", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (cap != null)
                    parametro = new SqlParameter("CAP", cap);
                else
                    parametro = new SqlParameter("CAP", DBNull.Value);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Email", email);
                comando.Parameters.Add(parametro);
                if (tel != null)
                    parametro = new SqlParameter("Tel", tel);
                else
                    parametro = new SqlParameter("Tel", DBNull.Value);
                comando.Parameters.Add(parametro);
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                //Cifro la password
                PasswordHasher hasher = new PasswordHasher();
                string cifredPWD = hasher.Hash(pwd);
                //Faccio una query per prendere l'IDAllenatore
                sql = "";
                sql += "SELECT IDAllenatore FROM Allenatore WHERE Email=@Email";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("Email", email));
                idAllenatore = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(idAllenatore);
                conn.Close();
                if (idAllenatore.Rows.Count > 0)//Controllo abbia trovato l'allenatore
                {
                    //Insert nella tabella Login
                    sql = "";
                    sql += "INSERT INTO Login(Email,PWD,IDAllenatore,DataUltimoCambioPwd,DataRichiestaCambioPwd,DataUltimoAccesso) ";
                    sql += "VALUES (@Email,@PWD,@IDAllenatore,@DataUltimoCambioPwd,@DataRichiestaCambioPwd,@DataUltimoAccesso) ";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("Email", email);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("PWD", cifredPWD);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDAllenatore", idAllenatore.Rows[0]["IDAllenatore"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataUltimoCambioPwd", DateTime.Now.Date);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataRichiestaCambioPwd", DateTime.Now.Date);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataUltimoAccesso", DateTime.Now.Date);
                    comando.Parameters.Add(parametro);
                    conn.Open();
                    comando.ExecuteNonQuery();
                    conn.Close();
                    regRiuscita = true;
                }
            }
            catch (Exception e)
            {

            }
            return regRiuscita;
        }
        public bool RegisterAtleta(int idSocieta, string codTessera, string nome, string cognome, char sesso, string cF, DateTime dataNascita, string comuneNascita, string comuneResidenza, string indirizzo, string cap, string email, string tel, int altezza, int peso, DateTime scadenzaCert, string pwd)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable idAtleta;
            bool regRiuscita = false;
            string sql;
            try
            {
                //Insert nella tabella Atleta
                sql = "";
                sql += "INSERT INTO Atleta(IDSocieta,CodiceTessera,Nome,Cognome,Sesso,CF,DataNascita,IDComuneNascita,IDComuneResidenza,Indirizzo,CAP,Email,Tel,Altezza,Peso,DataScadenzaCertificato) ";
                sql += "VALUES (@IDSocieta,@CodiceTessera,@Nome,@Cognome,@Sesso,@CF,@DataNascita,@IDComuneNascita,@IDComuneResidenza,@Indirizzo,@CAP,@Email,@Tel,@Altezza,@Peso,@DataScadenzaCertificato)";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("IDSocieta", idSocieta);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CodiceTessera", codTessera);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Nome", nome);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Cognome", cognome);
                comando.Parameters.Add(parametro);
                if (sesso != null)
                    parametro = new SqlParameter("Sesso", sesso);
                else
                    parametro = new SqlParameter("Sesso", DBNull.Value);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CF", cF);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataNascita", dataNascita);
                comando.Parameters.Add(parametro);
                if (comuneNascita != "")
                    parametro = new SqlParameter("IDComuneNascita", comuneNascita);
                else
                    parametro = new SqlParameter("IDComuneNascita", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (comuneResidenza != "")
                    parametro = new SqlParameter("IDComuneResidenza", comuneResidenza);
                else
                    parametro = new SqlParameter("IDComuneResidenza", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (indirizzo != null)
                    parametro = new SqlParameter("Indirizzo", indirizzo);
                else
                    parametro = new SqlParameter("Indirizzo", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (cap != null)
                    parametro = new SqlParameter("CAP", cap);
                else
                    parametro = new SqlParameter("CAP", DBNull.Value);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Email", email);
                comando.Parameters.Add(parametro);
                if (tel != null)
                    parametro = new SqlParameter("Tel", tel);
                else
                    parametro = new SqlParameter("Tel", DBNull.Value);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Altezza", altezza);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Peso", peso);
                comando.Parameters.Add(parametro);
                if (scadenzaCert != DateTime.MinValue)
                    parametro = new SqlParameter("DataScadenzaCertificato", scadenzaCert);
                else
                    parametro = new SqlParameter("DataScadenzaCertificato", DBNull.Value);
                comando.Parameters.Add(parametro);
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                //Cifro la password
                PasswordHasher hasher = new PasswordHasher();
                string cifredPWD = hasher.Hash(pwd);
                //Faccio una query per prendere l'IDAllenatore
                sql = "";
                sql += "SELECT IDAtleta FROM Atleta WHERE Email=@Email";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("Email", email));
                idAtleta = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(idAtleta);
                conn.Close();
                if (idAtleta.Rows.Count > 0)//Controllo abbia trovato l'allenatore
                {
                    //Insert nella tabella Login
                    sql = "";
                    sql += "INSERT INTO Login(Email,PWD,IDAtleta,DataUltimoCambioPwd,DataRichiestaCambioPwd,DataUltimoAccesso) ";
                    sql += "VALUES (@Email,@PWD,@IDAtleta,@DataUltimoCambioPwd,@DataRichiestaCambioPwd,@DataUltimoAccesso) ";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("Email", email);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("PWD", cifredPWD);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDAtleta", idAtleta.Rows[0]["IDAtleta"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataUltimoCambioPwd", DateTime.Now.Date);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataRichiestaCambioPwd", DateTime.Now.Date);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataUltimoAccesso", DateTime.Now.Date);
                    comando.Parameters.Add(parametro);
                    conn.Open();
                    comando.ExecuteNonQuery();
                    conn.Close();
                    regRiuscita = true;
                }
            }
            catch (Exception e)
            {

            }
            return regRiuscita;
        }
        public bool RegisterDelegato(string nome, string cognome, char sesso, string cF, DateTime dataNascita, string comuneNascita, string comuneResidenza, string indirizzo, string cap, string email, string tel, bool arbitro, bool supervisore, string pwd)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable idDelegato;
            bool regRiuscita = false;
            string sql;
            try
            {
                //Insert nella tabella Delegato
                sql = "";
                sql += "INSERT INTO DelegatoTecnico(Nome,Cognome,Sesso,CF,DataNascita,IDComuneNascita,IDComuneResidenza,Indirizzo,CAP,Email,Tel,Arbitro,Supervisore) ";
                sql += "VALUES (@Nome,@Cognome,@Sesso,@CF,@DataNascita,@IDComuneNascita,@IDComuneResidenza,@Indirizzo,@CAP,@Email,@Tel,@Arbitro,@Supervisore)";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("Nome", nome);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Cognome", cognome);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CF", cF);
                comando.Parameters.Add(parametro);
                if (sesso != null)
                    parametro = new SqlParameter("Sesso", sesso);
                else
                    parametro = new SqlParameter("Sesso", DBNull.Value);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataNascita", dataNascita);
                comando.Parameters.Add(parametro);
                if (comuneNascita != "")
                    parametro = new SqlParameter("IDComuneNascita", comuneNascita);
                else
                    parametro = new SqlParameter("IDComuneNascita", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (comuneResidenza != "")
                    parametro = new SqlParameter("IDComuneResidenza", comuneResidenza);
                else
                    parametro = new SqlParameter("IDComuneResidenza", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (indirizzo != null)
                    parametro = new SqlParameter("Indirizzo", indirizzo);
                else
                    parametro = new SqlParameter("Indirizzo", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (cap != null)
                    parametro = new SqlParameter("CAP", cap);
                else
                    parametro = new SqlParameter("CAP", DBNull.Value);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Email", email);
                comando.Parameters.Add(parametro);
                if (tel != null)
                    parametro = new SqlParameter("Tel", tel);
                else
                    parametro = new SqlParameter("Tel", DBNull.Value);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Arbitro", arbitro);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Supervisore", supervisore);
                comando.Parameters.Add(parametro);
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                //Cifro la password
                PasswordHasher hasher = new PasswordHasher();
                string cifredPWD = hasher.Hash(pwd);
                //Faccio una query per prendere l'IDAllenatore
                sql = "";
                sql += "SELECT IDDelegato FROM DelegatoTecnico WHERE Email=@Email";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("Email", email));
                idDelegato = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(idDelegato);
                conn.Close();
                if (idDelegato.Rows.Count > 0)//Controllo abbia trovato l'allenatore
                {
                    //Insert nella tabella Login
                    sql = "";
                    sql += "INSERT INTO Login(Email,PWD,IDDelegato,DataUltimoCambioPwd,DataRichiestaCambioPwd,DataUltimoAccesso) ";
                    sql += "VALUES (@Email,@PWD,@IDDelegato,@DataUltimoCambioPwd,@DataRichiestaCambioPwd,@DataUltimoAccesso) ";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("Email", email);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("PWD", cifredPWD);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDDelegato", idDelegato.Rows[0]["IDDelegato"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataUltimoCambioPwd", DateTime.Now.Date);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataRichiestaCambioPwd", DateTime.Now.Date);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataUltimoAccesso", DateTime.Now.Date);
                    comando.Parameters.Add(parametro);
                    conn.Open();
                    comando.ExecuteNonQuery();
                    conn.Close();
                    regRiuscita = true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {

            }
            return regRiuscita;
        }
        public bool RegisterSocieta(string comune, string nomeSocieta, string indirizzo, string cap, DateTime dataFondazione, DateTime dataAffilizione, string codAffiliazione, bool affiliata, string email, string sito, string tel1, string tel2, string pec, string piva, string cF, string cU, string pwd)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable idSocieta;
            bool regRiuscita = false;
            string sql;
            try
            {
                sql = "";
                sql += "INSERT INTO Societa(IDComune,NomeSocieta,Indirizzo,CAP,DataFondazione,DataAffiliazione,CodiceAffiliazione,Affiliata,Email,Sito,Tel1,Tel2,Pec,PIVA,CF,CU) ";
                sql += "VALUES (@IDComune,@NomeSocieta,@Indirizzo,@CAP,@DataFondazione,@DataAffiliazione,@CodiceAffiliazione,@Affiliata,@Email,@Sito,@Tel1,@Tel2,@Pec,@PIVA,@CF,@CU)";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("IDComune", comune);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("NomeSocieta", nomeSocieta);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Indirizzo", indirizzo);
                comando.Parameters.Add(parametro);
                if (cap != null)
                    parametro = new SqlParameter("CAP", cap);
                else
                    parametro = new SqlParameter("CAP", DBNull.Value);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataFondazione", dataFondazione);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataAffiliazione", dataAffilizione);
                comando.Parameters.Add(parametro);
                if (codAffiliazione != null)
                    parametro = new SqlParameter("CodiceAffiliazione", codAffiliazione);
                else
                    parametro = new SqlParameter("CodiceAffiliazione", DBNull.Value);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Affiliata", affiliata);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Email", email);
                comando.Parameters.Add(parametro);
                if (sito != null)
                    parametro = new SqlParameter("Sito", sito);
                else
                    parametro = new SqlParameter("Sito", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (tel1 != null)
                    parametro = new SqlParameter("Tel1", tel1);
                else
                    parametro = new SqlParameter("Tel1", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (tel2 != null)
                    parametro = new SqlParameter("Tel2", tel2);
                else
                    parametro = new SqlParameter("Tel2", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (pec != null)
                    parametro = new SqlParameter("Pec", pec);
                else
                    parametro = new SqlParameter("Pec", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (piva != null)
                    parametro = new SqlParameter("PIVA", piva);
                else
                    parametro = new SqlParameter("PIVA", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (cF != null)
                    parametro = new SqlParameter("CF", cF);
                else
                    parametro = new SqlParameter("CF", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (cU != null)
                    parametro = new SqlParameter("CU", cU);
                else
                    parametro = new SqlParameter("CU", DBNull.Value);
                comando.Parameters.Add(parametro);
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                //Cifro la password
                PasswordHasher hasher = new PasswordHasher();
                string cifredPWD = hasher.Hash(pwd);
                //Faccio una query per prendere l'IDAllenatore
                sql = "";
                sql += "SELECT IDSocieta FROM Societa WHERE Email=@Email";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("Email", email));
                idSocieta = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(idSocieta);
                conn.Close();
                if (idSocieta.Rows.Count > 0)//Controllo abbia trovato l'allenatore
                {
                    //Insert nella tabella Login
                    sql = "";
                    sql += "INSERT INTO Login(Email,PWD,IDSocieta,DataUltimoCambioPwd,DataRichiestaCambioPwd,DataUltimoAccesso) ";
                    sql += "VALUES (@Email,@PWD,@IDSocieta,@DataUltimoCambioPwd,@DataRichiestaCambioPwd,@DataUltimoAccesso) ";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("Email", email);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("PWD", cifredPWD);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDSocieta", idSocieta.Rows[0]["IDSocieta"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataUltimoCambioPwd", DateTime.Now.Date);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataRichiestaCambioPwd", DateTime.Now.Date);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataUltimoAccesso", DateTime.Now.Date);
                    comando.Parameters.Add(parametro);
                    conn.Open();
                    comando.ExecuteNonQuery();
                    conn.Close();
                    regRiuscita = true;
                }
            }
            catch (Exception e)
            {

            }
            return regRiuscita;
        }
        public int GetIDTorneo(string titolo)//Metodo che restituisce l'ID del torneo cercato
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT IDTorneo FROM Torneo WHERE Titolo=@Titolo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("Titolo", titolo));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
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
            SqlCommand comando;
            SqlParameter parametro;
            string sql;
            try
            {
                sql = "";
                sql += "UPDATE Login SET PWD=@PWD,DataUltimoCambioPwd=@DataUltimoCambio,DataRichiestaCambioPwd=@DataRichiestaCambio,DataUltimoAccesso=@DataUltimoAccesso " +
                    "WHERE Email='" + email + "'";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("PWD", psw);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataUltimoCambio", DateTime.Now.Date);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataRichiestaCambio", DateTime.Now.Date);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataUltimoAccesso", DateTime.Now.Date);
                comando.Parameters.Add(parametro);
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
        /*Ho deciso di creare un metodo che restituisca un array di DataTable in modo che, dato che ogni torneo 
         * puo avere N parametri, non ci saranno delle rindondanze nei record restituiti*/
        public DataTable[] GetAllTornei()//Metodo che restituisce tutti i tornei autorizzati
        {
            SqlDataAdapter adapter;
            string sql;
            DataTable[] risultati = new DataTable[3];
            DataTable ris1, ris2, ris3;
            sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche" +
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
            sql = "";
            sql += "SELECT Torneo.Titolo,NomeImpianto,Citta ";
            sql += "FROM (((Impianto LEFT JOIN ImpiantoTorneo ON Impianto.IDImpianto=ImpiantoTorneo.IDImpianto)LEFT JOIN Torneo ON ImpiantoTorneo.IDTorneo=Torneo.IDTorneo)LEFT JOIN Comune ON Impianto.IDComune=Comune.IDComune) ";
            ris3 = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(ris3);
            conn.Close();
            risultati[0] = ris1;
            risultati[1] = ris2;
            risultati[2] = ris3;
            return risultati;
        }
        public DataTable GetInfoSquadre(int idTorneo, int numPartita)//Metodo che restituisce i nomi delle squadre in una partita
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT t1.NomeTeam as Team1,t2.NomeTeam As Team2 " +
            "FROM((Partita LEFT JOIN Squadra t1 ON Partita.idsq1 = t1.idsquadra) LEFT JOIN Squadra t2 ON Partita.idsq2 = t2.idsquadra) " +
            "WHERE Partita.IDTorneo=@IDTorneo AND Partita.NumPartita=@NumPartita";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
            comando.Parameters.Add(new SqlParameter("NumPartita", numPartita));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public bool UploadResults(int idTorneo, int idPartita, int numSet, int puntiTeam1, int puntiTeam2)
        {
            SqlCommand comando;
            SqlParameter parametro;
            string sql;
            //Metodo che aggiurna i risultati di una partita
            try
            {
                sql = "";
                sql += "UPDATE Partita ";
                switch (numSet)
                {
                    case 1:
                        sql += "SET PT1S1=@PuntiTeam1,PT2S1=@PuntiTeam2 ";
                        break;
                    case 2:
                        sql += "SET PT1S2=@PuntiTeam1,PT2S2=@PuntiTeam2 ";
                        break;
                    case 3:
                        sql += "SET PT1S3=@PuntiTeam1,PT2S3=@PuntiTeam2 ";
                        break;
                }
                sql += "WHERE IDPartita=@IDPartita AND IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("PuntiTeam1", puntiTeam1);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("PuntiTeam2", puntiTeam2);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDPartita", idPartita);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDTorneo", idTorneo);
                comando.Parameters.Add(parametro);
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
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
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
            "WHERE Partita.IDTorneo=@IDTorneo AND Partita.NumPartita=@NumPartita;";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
            comando.Parameters.Add(new SqlParameter("NumPartita", numPartita));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable[] GetTorneoByTitolo(int idTorneo)//Metodo che restituisce un torneo tramite l'ID
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable[] risultati = new DataTable[3];
            DataTable ris1, ris2, ris3;
            sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche" +
            "FROM(((((((((Torneo Left join TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)Left Join DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT join ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT join DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)Left join DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT Join FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)Left Join ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)left join Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)Left Join Comune On Impianto.IDComune = Comune.IDComune) " +
            "WHERE Torneo.IDTorneo=@IDTorneo";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
            ris1 = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(ris1);
            conn.Close();
            sql = "";
            sql += "SELECT NomeParametro " +
            "FROM ParametroQualita, ParametroTorneo, Torneo " +
            "WHERE Torneo.IDTorneo=@IDTorneo AND ParametroTorneo.idtorneo = @IDTorneo AND ParametroTorneo.IDParametro = ParametroQualita.IDParametro";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
            ris2 = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(ris2);
            conn.Close();
            sql = "";
            sql += "SELECT NomeImpianto,Citta ";
            sql += "FROM ((Impianto LEFT JOIN ImpiantoTorneo ON Impianto.IDImpianto=ImpiantoTorneo.IDImpianto)LEFT JOIN Comune ON Impianto.IDComune=Comune.IDComune) ";
            sql += "WHERE ImpiantoTorneo.IDTorneo=@IDTorneo";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
            ris3 = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(ris3);
            conn.Close();
            risultati[0] = ris1;
            risultati[1] = ris2;
            risultati[2] = ris3;
            return risultati;
        }
        public DataTable[] GetTorneoByID(int id)//Metodo che restituisce un torneo tramite l'ID
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable[] risultati = new DataTable[3];
            sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche " +
            "FROM(((((((((Torneo Left join TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)Left Join DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT join ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT join DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)Left join DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT Join FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)Left Join ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)left join Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)Left Join Comune On Impianto.IDComune = Comune.IDComune) " +
            "WHERE Torneo.IDTorneo=@IDTorneo";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", id));
            risultati[0] = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(risultati[0]);
            conn.Close();
            sql = "";
            sql += "SELECT NomeParametro " +
            "FROM ParametroQualita, ParametroTorneo, Torneo " +
            "WHERE Torneo.IDTorneo=@IDTorneo AND ParametroTorneo.idtorneo = Torneo.idtorneo AND ParametroTorneo.IDParametro = ParametroQualita.IDParametro";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", id));
            risultati[1] = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(risultati[1]);
            conn.Close();
            sql = "";
            sql += "SELECT NomeImpianto,Citta ";
            sql += "FROM ((Impianto LEFT JOIN ImpiantoTorneo ON Impianto.IDImpianto=ImpiantoTorneo.IDImpianto)LEFT JOIN Comune ON Impianto.IDComune=Comune.IDComune) ";
            sql += "WHERE ImpiantoTorneo.IDTorneo=@IDTorneo";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", id));
            risultati[2] = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(risultati[2]);
            conn.Close();
            return risultati;
        }
        public DataTable[] GetAllTorneiMaschili()
        {
            SqlDataAdapter adapter;
            string sql;
            DataTable[] risultati = new DataTable[2];
            DataTable ris1, ris2;
            sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche" +
            "FROM(((((((((Torneo Left join TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)Left Join DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT join ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT join DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)Left join DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT Join FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)Left Join ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)left join Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)Left Join Comune On Impianto.IDComune = Comune.IDComune) " +
            "WHERE Torneo.Autorizzato=1 AND Gender='M'";
            ris1 = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(ris1);
            conn.Close();
            sql = "";
            sql += "SELECT Titolo,NomeParametro " +
            "FROM ParametroQualita, ParametroTorneo, Torneo " +
            "WHERE Torneo.Autorizzato=1 AND Gender='M' AND ParametroTorneo.idtorneo = Torneo.idtorneo AND ParametroTorneo.IDParametro = ParametroQualita.IDParametro";
            ris2 = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(ris2);
            conn.Close();
            risultati[0] = ris1;
            risultati[1] = ris2;
            return risultati;
        }
        public DataTable[] GetAllTorneiFemminili()
        {
            SqlDataAdapter adapter;
            string sql;
            DataTable[] risultati = new DataTable[2];
            DataTable ris1, ris2;
            sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche" +
            "FROM(((((((((Torneo Left join TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)Left Join DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT join ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT join DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)Left join DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT Join FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)Left Join ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)left join Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)Left Join Comune On Impianto.IDComune = Comune.IDComune) " +
            "WHERE Torneo.Autorizzato=1 AND Gender='F'";
            ris1 = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(ris1);
            conn.Close();
            sql = "";
            sql += "SELECT Titolo,NomeParametro " +
            "FROM ParametroQualita, ParametroTorneo, Torneo " +
            "WHERE Torneo.Autorizzato=1 AND Gender='F' AND ParametroTorneo.idtorneo = Torneo.idtorneo AND ParametroTorneo.IDParametro = ParametroQualita.IDParametro";
            ris2 = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(ris2);
            conn.Close();
            risultati[0] = ris1;
            risultati[1] = ris2;
            return risultati;
        }
        public DataTable GetClassifica(string sesso)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "WITH punteggi(idAtl, punti) AS(" +
            "SELECT idatleta1, sum(punti) / 2.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo " +
            "AND datediff(day,datafine,GETDATE())<120 GROUP BY idatleta1 " +
            "UNION " +
            "SELECT idatleta2, sum(punti) / 2.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo " +
            "AND datediff(day,datafine,GETDATE())<120 GROUP BY idatleta2 " +
            "UNION " +
            "SELECT idatleta1, sum(punti) / 4.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo " +
            "AND datediff(day,datafine,GETDATE()) BETWEEN 121 AND 365 GROUP BY idatleta1 " +
            "UNION " +
            "SELECT idatleta2, sum(punti) / 4.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo " +
            "AND datediff(day,datafine,GETDATE()) BETWEEN 121 AND 365 GROUP BY idatleta2 " +
            ") " +
            "SELECT idatleta, cognome, nome, sum(punti) AS Punteggi " +
            "FROM punteggi, atleta WHERE idatleta=idAtl AND atleta.sesso=@Sesso " +
            "GROUP BY idatleta,cognome,nome HAVING sum(punti)>0 ORDER BY sum(punti) DESC,Cognome,Nome";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Sesso", sesso));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetStoricoPartiteTorneo(int idTorneo)//Metodo che restituisce la lista delle partite di un torneo
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT " +
            "Partita.Fase,Partita.Campo,Partita.DataPartita,Partita.OraPartita, " +
            "Partita.PT1S1 as PtTeam1Set1,Partita.PT2S1 as PtTeam2Set1,Partita.PT1S2 as PtTeam1Set2,Partita.PT2S2 as PtTeam2Set2, " +
            "Partita.PT1S3 as PtTeam1Set3,Partita.PT2S3 as PtTeam2Set3 " +
            "FROM((((((((" +
            "Partita LEFT JOIN Squadra S1 ON Partita.idsq1 = S1.idsquadra) LEFT JOIN Squadra S2 ON Partita.idsq2 = S2.idsquadra) " +
            "left JOIN Atleta A1 ON S1.IDAtleta1 = A1.IDAtleta) LEFT JOIN Atleta A2 ON S1.IDAtleta2 = A2.IDAtleta) " +
            "left JOIN Atleta A3 ON S2.IDAtleta1 = A3.IDAtleta) LEFT JOIN Atleta A4 ON S2.IDAtleta2 = A4.IDAtleta) " +
            "left JOIN DelegatoTecnico D1 ON Partita.idarbitro1 = D1.IDDelegato) LEFT JOIN DelegatoTecnico D2 ON Partita.idarbitro2 = D2.IDDelegato) " +
            "WHERE Partita.IDTorneo =@IDTorneo;";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public bool SetAllenatoreSquadra(int IdTorneo, int idSquadra, int idAllenatore)//Metodo che aggiunge l'allenatore all'interno della squadra iscritta
        {
            SqlCommand comando;
            SqlParameter parametro;
            //prova ad aggiornare la tabella inserento l'allenatore
            try
            {
                string sql;
                sql = "";
                sql += "UPDATE ListaIscritti" +
                    " SET IDAllenatore=@IDAllenatore" +
                    " WHERE IDTorneo=@IDTorneo AND IDSquadra=@IDSquadra;";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("IDAllenatore", idAllenatore);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDTorneo", IdTorneo);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDSquadra", idSquadra);
                comando.Parameters.Add(parametro);
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
        public DataTable[] GetTorneoEPartecipanti(int idTorneo)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable[] risultati = new DataTable[3];
            risultati[0] = GetTorneoByID(idTorneo)[0];//Informazioni sul torneo
            risultati[1] = GetTorneoByID(idTorneo)[1];//Parametri del torneo
            risultati[2] = new DataTable();
            sql = "";
            sql += "SELECT CONCAT(Atleta1.Nome,' ',Atleta1.Cognome) AS Atleta1,CONCAT(Atleta2.Nome,' ',Atleta2.Cognome) AS Atleta2,Squadra.NomeTeam AS NomeTeam,ListaIScritti.WC ";
            sql += "FROM(((ListaIscritti LEFT JOIN Squadra ON Squadra.IDSquadra=ListaIscritti.IDSquadra)LEFT JOIN Atleta Atleta1 ON Squadra.IDAtleta1=Atleta1.IDAtleta)LEFT JOIN Atleta Atleta2 ON Squadra.IDAtleta2=Atleta2.IDAtleta) ";
            sql += "WHERE ListaIscritti.IDTorneo=@IDTorneo";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(risultati[2]);
            conn.Close();
            return risultati;
        }
        public bool CreaTorneo(string titolo, int puntiVittoria, double montepremi, DateTime dataChiusuraIscrizioni, DateTime dataInizio, DateTime dataFine, char genere, string formulaTorneo, int NumMaxTeamMainDraw, int NumMaxTeamQualifiche, string[] parametriTorneo, string tipoTorneo, string[] impianti, double quotaIscrizione, int idSocieta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable query;
            string sql;
            DataTable idFormula, idTipoTorneo, idTorneo;
            List<int> idParametriTorneo = new List<int>();
            List<int> idImpianti = new List<int>();
            try
            {
                //Prendo l'IDTorneo se ne esiste già uno con lo stesso nome
                sql = "";
                sql += "SELECT IDTorneo FROM Torneo WHERE Titolo=@Titolo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("Titolo", titolo));
                idTorneo = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(idTorneo);
                conn.Close();

                try
                {
                    //controllo che non ci sia gia un torneo con quel nome
                    if (idTorneo.Rows.Count == 0)
                    {
                        //Trovo l'IDFormula
                        sql = "";
                        sql += "SELECT IDFormula FROM FormulaTorneo WHERE Formula=@FormulaTorneo";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("FormulaTorneo", formulaTorneo));
                        idFormula = new DataTable();
                        adapter = new SqlDataAdapter(comando);
                        conn.Open();
                        adapter.Fill(idFormula);
                        conn.Close();
                        //Trovo l'IDTipoTorneo
                        sql = "";
                        sql += "SELECT IDTipoTorneo FROM TipoTorneo WHERE Descrizione=@TipoTorneo";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("TipoTorneo", tipoTorneo));
                        idTipoTorneo = new DataTable();
                        adapter = new SqlDataAdapter(comando);
                        conn.Open();
                        adapter.Fill(idTipoTorneo);
                        conn.Close();
                        //Creo il torneo
                        sql = "";
                        sql += "INSERT INTO Torneo(IDSocieta,IDTipoTorneo,IDFormula,Titolo,PuntiVittoria,Montepremi,DataChiusuraIscrizioni,DataInizio,DataFine,Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,QuotaIscrizione) ";
                        sql += "VALUES(@IDSocieta,@IDTipoTorneo,@IDFormula,@Titolo,@PuntiVittoria,@Montepremi,@DataChiusuraIScrizioni,@DataInzio,@DataFine,@Gender,@NumMaxTeamMainDraw,@NumMaxTeamQualifiche,@QuotaIscrizione)";
                        comando = new SqlCommand(sql, conn);
                        parametro = new SqlParameter("IDSocieta", idSocieta);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("IDTipoTorneo", idTipoTorneo.Rows[0][0]);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("IDFormula", idFormula.Rows[0][0]);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("Titolo", titolo);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("PuntiVittoria", puntiVittoria);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("Montepremi", montepremi);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("DataChiusuraIScrizioni", dataChiusuraIscrizioni.Date);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("DataInzio", dataInizio.Date);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("DataFine", dataFine.Date);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("Gender", genere);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("NumMaxTeamMainDraw", NumMaxTeamMainDraw);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("NumMaxTeamQualifiche", NumMaxTeamQualifiche);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("QuotaIscrizione", quotaIscrizione);
                        comando.Parameters.Add(parametro);
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                        //Trovo gli ID dei parametri
                        for (int i = 0; i < parametriTorneo.Length; i++)
                        {
                            sql = "";
                            sql += "SELECT IDParametro FROM ParametroQualita WHERE NomeParametro=@ParametriTorneo";
                            comando = new SqlCommand(sql, conn);
                            comando.Parameters.Add(new SqlParameter("ParametriTorneo", parametriTorneo[i]));
                            query = new DataTable();
                            adapter = new SqlDataAdapter(comando);
                            conn.Open();
                            adapter.Fill(query);
                            conn.Close();
                            if (query.Rows.Count > 0)
                                idParametriTorneo.Add(Convert.ToInt32(query.Rows[0][0]));
                        }
                        //Prendo l'IDTorneo
                        sql = "";
                        sql += "SELECT IDTorneo FROM Torneo WHERE Titolo=@Titolo";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("Titolo", titolo));
                        idTorneo = new DataTable();
                        adapter = new SqlDataAdapter(comando);
                        conn.Open();
                        adapter.Fill(idTorneo);
                        conn.Close();
                        //Assegno i parametri al torneo
                        if (idParametriTorneo.Count > 0)
                        {
                            for (int i = 0; i < idParametriTorneo.Count; i++)
                            {
                                sql = "";
                                sql += "INSERT INTO ParametroTorneo(IDTorneo,IDParametro) VALUES(@IDTorneo,@IDParametro)";
                                comando = new SqlCommand(sql, conn);
                                parametro = new SqlParameter("IDTorneo", idTorneo.Rows[0]["IDTorneo"]);
                                comando.Parameters.Add(parametro);
                                parametro = new SqlParameter("IDParametro", idParametriTorneo[i]);
                                comando.Parameters.Add(parametro);
                                conn.Open();
                                comando.ExecuteNonQuery();
                                conn.Close();
                            }
                        }
                        //Prendo gli id deiìgli impianti
                        for (int i = 0; i < impianti.Length; i++)
                        {
                            sql = "";
                            sql += "SELECT IDImpianto FROM Impianto WHERE NomeImpianto=@Impianti";
                            comando = new SqlCommand(sql, conn);
                            comando.Parameters.Add(new SqlParameter("Impianti", impianti[i]));
                            query = new DataTable();
                            adapter = new SqlDataAdapter(comando);
                            conn.Open();
                            adapter.Fill(query);
                            conn.Close();
                            if (query.Rows.Count > 0)
                                idImpianti.Add(Convert.ToInt32(query.Rows[0][0]));
                        }
                        //Assegno gli impianti al torneo
                        if (idImpianti.Count > 0)
                        {
                            for (int i = 0; i < idImpianti.Count; i++)
                            {
                                sql = "";
                                sql += "INSERT INTO ImpiantoTorneo(IDTorneo,IDImpianto) VALUES(@IDTorneo,@IDImpianto)";
                                comando = new SqlCommand(sql, conn);
                                parametro = new SqlParameter("IDTorneo", idTorneo.Rows[0][0]);
                                comando.Parameters.Add(parametro);
                                parametro = new SqlParameter("IDImpianto", idImpianti[i]);
                                comando.Parameters.Add(parametro);
                                conn.Open();
                                comando.ExecuteNonQuery();
                                conn.Close();
                            }
                        }
                        return true;
                    }
                    else return false;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                string errore = e.Message;
                return false;
            }
        }
        public bool RecuperaPassword(string email)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("recoverypass.aibvc@gmail.com");
                mail.To.Add(email);
                mail.Subject = "Recupero password Account AIBVC";
                mail.Body = "Clicca il bottone per cambiare la password" +
                    "<br><form action=\"https://aibvcwa.azurewebsites.net/nuovapassword\" method=\"post\">" +
                    "<input type=\"hidden\" name=\"email\" value=\"" + email + "\" id=\"email\"/>" +
                    "<button type =\"submit\"> Cambia Password </button>" +
                    "</form> ";
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("recoverypass.aibvc@gmail.com", "rpsabv21");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        } //invia l'email per recuperare la password
        public Nullable<int> InsertSquadra(string NomeAtleta1, string NomeAtleta2, string NomeTeam)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable query;
            string sql;
            int idatleta1, idatleta2;
            try
            {
                idatleta1 = Convert.ToInt32(NomeAtleta1);
                idatleta2 = GetIDAtleta(NomeAtleta2);

            }
            catch
            {
                return null;
            }
            try
            {
                sql = "";
                sql += "SELECT IDSquadra FROM Squadra WHERE (IDAtleta1=@IDAtleta1 AND IDAtleta2=@IDAtleta2) OR (IDAtleta1=@IDAtleta1 AND IDAtleta2=@IDAtleta2)";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDAtleta1", idatleta1));
                comando.Parameters.Add(new SqlParameter("IDAtleta2", idatleta2));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                //controllo che non ci sia gia una squadra
                if (query.Rows.Count > 0)
                {
                    //restituisco id squadra
                    return (Convert.ToInt32(query.Rows[0]["IDSquadra"]));
                }
                else
                {
                    try
                    {
                        //Insert nella tabella squadra
                        sql = "";
                        sql += "INSERT INTO Squadra(IDAtleta1,IDAtleta2,NomeTeam)";
                        sql += "VALUES (@IDAtleta1,@IDAtleta2,@NomeTeam)";
                        comando = new SqlCommand(sql, conn);
                        parametro = new SqlParameter("IDAtleta1", idatleta1);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("IDAtleta2", idatleta2);
                        comando.Parameters.Add(parametro);
                        if (NomeTeam != null)
                            parametro = new SqlParameter("NomeTeam", NomeTeam);
                        else
                            parametro = new SqlParameter("NomeTeam", DBNull.Value);
                        comando.Parameters.Add(parametro);
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                        //riscarico id squadra appena inserito
                        sql = "";
                        sql += "SELECT IDSquadra FROM Squadra WHERE IDAtleta1=@IDAtleta1 AND IDAtleta2=@IDAtleta2";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDAtleta1", idatleta1));
                        comando.Parameters.Add(new SqlParameter("IDAtleta2", idatleta2));
                        query = new DataTable();
                        adapter = new SqlDataAdapter(comando);
                        conn.Open();
                        adapter.Fill(query);
                        conn.Close();
                        //controllo che non ci sia gia una squadra
                        if (query.Rows.Count > 0)
                        {
                            //restituisco id squadra
                            return (Convert.ToInt32(query.Rows[0]["IDSquadra"]));
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }// inserisce le squadre
        public int GetIDAtleta(string data)//Metodo che restituisce l'ID del atleta cercato
        {
            //uso il codice della tessera in modo tale che un atleta per invitare un compagno debba conoscerlo e quindi richiederlo
            //a differenza del nome e cognome che è conosciuto da tutti
            //data= Nome Cognome CodiceTesera
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT IDAtleta FROM Atleta WHERE CodiceTessera =@Data";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("Data", data));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return Convert.ToInt32(query.Rows[0]["IDAtleta"]);
            }
            catch
            {
                return 0;
            }
        }
        public bool IscriviSquadra(int idTorneo, int idSquadra, int idAllenatore)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT IDAtleta1, IDAtleta2 FROM Squadra WHERE IDSquadra=" + idSquadra + "";
                query = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                //se ha trovato una squdra con quel id
                if (query.Rows.Count > 0)
                {
                    //prende il sesso degli atleti
                    try
                    {
                        sql = "";
                        sql += "SELECT Sesso FROM Atleta, Squadra WHERE((Squadra.IDAtleta1 = " + query.Rows[0]["IDAtleta1"] + " AND Atleta.IDAtleta = Squadra.IDAtleta1) OR(Squadra.IDAtleta2 = " + query.Rows[0]["IDAtleta1"] + " AND Atleta.IDAtleta = Squadra.IDAtleta2))";
                        query = new DataTable();
                        adapter = new SqlDataAdapter(sql, conn);
                        conn.Open();
                        adapter.Fill(query);
                        conn.Close();
                        //controllo che il sesso della squadra sia idoneo
                        if (query.Rows.Count > 0)
                        {
                            sql = "";
                            sql += "SELECT IDTorneo FROM Torneo WHERE torneo.Gender = '" + query.Rows[0]["Sesso"] + "' AND IDTorneo = " + idTorneo + ";";
                            query = new DataTable();
                            adapter = new SqlDataAdapter(sql, conn);
                            conn.Open();
                            adapter.Fill(query);
                            conn.Close();
                            //se è stata trovata una corrispondenza significa che il sesso è corretto
                            if (query.Rows.Count > 0)
                            {
                                try
                                {
                                    //controllo che l'allentatore si presente
                                    if (GetIDAllenatore(idAllenatore) > 0)
                                    {
                                        //trasformo il codice della tessera dell'allenatore ricevuto con id 
                                        idAllenatore = GetIDAllenatore(idAllenatore);
                                        sql = "";
                                        sql += "INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,Cancellata)VALUES (@IDSquadra,@IDTorneo,@IDAllenatore,@DataIscrizione,@Cancellata)";
                                        comando = new SqlCommand(sql, conn);
                                        parametro = new SqlParameter("IDSquadra", idSquadra);
                                        comando.Parameters.Add(parametro);
                                        parametro = new SqlParameter("IDTorneo", idTorneo);
                                        comando.Parameters.Add(parametro);
                                        if (idAllenatore != 0)
                                            parametro = new SqlParameter("IDAllenatore", idAllenatore);
                                        else
                                            parametro = new SqlParameter("IDAllenatore", DBNull.Value);
                                        comando.Parameters.Add(parametro);
                                        parametro = new SqlParameter("DataIscrizione", DateTime.Now.Date);
                                        comando.Parameters.Add(parametro);
                                        parametro = new SqlParameter("Cancellata", SqlDbType.Bit) { Value = 0 };//di base settata a FALSE
                                        comando.Parameters.Add(parametro);
                                        conn.Open();
                                        comando.ExecuteNonQuery();
                                        conn.Close();
                                        return true;
                                    }
                                    else return false;
                                }
                                catch (Exception ex)
                                {
                                    string error = ex.Message;
                                    return false;
                                }
                            }
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }

        }//Inserisci la squadra nella lista iscritti ti un torneo
        public DataTable GetPartite(int NumeroPartite)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT TOP @NumeroPartite " +
                " CONCAT(A1.Nome,' ',A1.cognome) as Atleta1,CONCAT(A2.Nome,' ',A2.cognome) as Atleta2,S1.NomeTeam as Team1, " +
                "CONCAT(A3.Nome,' ',A3.cognome) as Atleta3,CONCAT(A4.Nome,' ',A4.cognome) AS Atleta4, S2.NomeTeam as Team2, " +
                "CONCAT(D1.Nome,' ',D1.Cognome) as Arbitro1,CONCAT(D2.Nome,' ',D2.Cognome) as Arbitro2, " +
                "Partita.Fase,Partita.Campo,Partita.DataPartita,Partita.OraPartita, " +
                "Partita.PT1S1 as PtTeam1Set1,Partita.PT2S1 as PtTeam2Set1,Partita.PT1S2 as PtTeam1Set2,Partita.PT2S2 as PtTeam2Set2, " +
                "Partita.PT1S3 as PtTeam1Set3,Partita.PT2S3 as PtTeam2Set3 " +
                "FROM((((((((" +
                "Partita LEFT JOIN Squadra S1 ON Partita.idsq1 = S1.idsquadra) LEFT JOIN Squadra S2 ON Partita.idsq2 = S2.idsquadra) " +
                "left JOIN Atleta A1 ON S1.IDAtleta1 = A1.IDAtleta) LEFT JOIN Atleta A2 ON S1.IDAtleta2 = A2.IDAtleta) " +
                "left JOIN Atleta A3 ON S2.IDAtleta1 = A3.IDAtleta) LEFT JOIN Atleta A4 ON S2.IDAtleta2 = A4.IDAtleta) " +
                "left JOIN DelegatoTecnico D1 ON Partita.idarbitro1 = D1.IDDelegato) LEFT JOIN DelegatoTecnico D2 ON Partita.idarbitro2 = D2.IDDelegato) ";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("NumeroPartite", NumeroPartite));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return query;
            }
            catch
            {
                return null;
            }
        }//torna partite in base al numero passato TRIF
        public DataTable GetTipoTorneo()
        {
            SqlDataAdapter adapter;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT IDTipoTorneo,Descrizione FROM TipoTorneo";
                query = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return query;
            }
            catch
            {
                return null;
            }
        }//ritorna i tipi di torneo 
        public DataTable GetSupervisore()
        {
            SqlDataAdapter adapter;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT IDDelegato,CONCAT(Nome,' ',Cognome)as Nome FROM DelegatoTecnico WHERE Supervisore='true'";
                query = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return query;
            }
            catch
            {
                return null;
            }
        }//ritorna i tutti i Supervisori
        public DataTable GetDelegato(int tipo)
        {
            SqlDataAdapter adapter;
            DataTable query;
            string sql;
            try
            {
                if (tipo == 1)
                {
                    sql = "";
                    sql += "SELECT IDDelegato,CONCAT(Nome,' ',Cognome)as Nome FROM DelegatoTecnico WHERE Supervisore='true'";
                }
                else if (tipo == 2)
                {
                    sql = "";
                    sql += "SELECT IDDelegato,CONCAT(Nome,' ',Cognome)as Nome FROM DelegatoTecnico WHERE Arbitro='true'";
                }
                else return null;
                query = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return query;
            }
            catch
            {
                return null;
            }
        }//ritorna i tutti i Supervisori
        public DataTable GetFormula()
        {
            SqlDataAdapter adapter;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT IDFormula,Formula FROM FormulaTorneo;";
                query = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return query;
            }
            catch
            {
                return null;
            }
        }//ritorna i tutti i tipi di formula
        public DataTable GetParametriTorneo()
        {
            SqlDataAdapter adapter;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT IDParametro,NomeParametro FROM ParametroQualita;";
                query = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return query;
            }
            catch
            {
                return null;
            }
        }//ritorna i tutti i tipi di formula
        public DataTable GetImpianti(int idimpianti)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT Impianto.IDImpianto, Impianto.NomeImpianto FROM Impianto, ImpiantoSocieta WHERE ImpiantoSocieta.IDSocieta=@IDImpianti AND Impianto.IDImpianto= ImpiantoSocieta.IDImpianto;";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDImpianti", idimpianti));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return query;
            }
            catch
            {
                return null;
            }
        }//ritorna i tutti i tipi di formula
        public bool AutorizzaTorneo(int idTorneo)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            string sql;
            try
            {
                sql = "";
                sql += "UPDATE Torneo SET Autorizzato=1 WHERE IdTorneo=@idTorneo";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("idTorneo", idTorneo);
                comando.Parameters.Add(parametro);
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                string errore = e.Message;
                return false;
            }
        }
        public bool AssegnaSupervisori(int idSupervisore, int idSupArbitrale, int idDirettore, int idTorneo)
        {
            SqlCommand comando = new SqlCommand();
            SqlParameter parametro;
            string sql;
            try
            {
                sql = "";
                if (idSupArbitrale != 0 && idDirettore != 0)
                {
                    sql += "UPDATE Torneo ";
                    sql += "SET IDSupervisore=@IDSupervisore,IDSupervisoreArbitrale=@IDSupArbitrale,IDDirettoreCompetizione=@IDDirettore ";
                    sql += "WHERE IDTorneo=@IDTorneo";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("IDSupervisore", idSupervisore);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDSupArbitrale", idSupArbitrale);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDDirettore", idDirettore);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDTorneo", idTorneo);
                    comando.Parameters.Add(parametro);
                }
                if (idSupArbitrale == 0 && idDirettore == 0)
                {
                    sql += "UPDATE Torneo ";
                    sql += "SET IDSupervisore=@IDSupervisore ";
                    sql += "WHERE IDTorneo=@IDTorneo";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("IDSupervisore", idSupervisore);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDTorneo", idTorneo);
                    comando.Parameters.Add(parametro);
                }
                else if (idSupArbitrale == 0)

                {
                    sql += "UPDATE Torneo ";
                    sql += "SET IDSupervisore=@IDSupervisore,IDDirettoreCompetizione=@IDDirettore ";
                    sql += "WHERE IDTorneo=@IDTorneo";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("IDSupervisore", idSupervisore);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDDirettore", idDirettore);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDTorneo", idTorneo);
                    comando.Parameters.Add(parametro);
                }
                else if (idDirettore == 0)
                {
                    sql += "UPDATE Torneo ";
                    sql += "SET IDSupervisore=@IDSupervisore,IDSupervisoreArbitrale=@IDSupArbitrale ";
                    sql += "WHERE IDTorneo=@IDTorneo";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("IDSupervisore", idSupervisore);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDSupArbitrale", idSupArbitrale); comando.Parameters.Add(parametro);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDTorneo", idTorneo);
                    comando.Parameters.Add(parametro);
                }
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                string errore = e.Message;
                return false;
            }
        }
        public int GetIDDelegato(string nomeSupervisore, string cognomeSupervisore)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT IDDelegato FROM DelegatoTecnico WHERE Nome=@NomeSupervisore AND Cognome=@CognomeSupervisore";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("NomeSupervisore", nomeSupervisore));
            comando.Parameters.Add(new SqlParameter("CognomeSupervisore", cognomeSupervisore));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return Convert.ToInt32(query.Rows[0][0]);
        }
        public DataTable GetAtletiSocieta(int idsocieta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT Atleta.CodiceTessera FROM Atleta,Societa WHERE Atleta.IDSocieta=Societa.IDSocieta AND Societa.IDSocieta=@IDSocieta;";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDSocieta", idsocieta));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }//torna lista atleti di una societa
        public DataTable GetAllenatoreSocieta(int idsocieta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT Allenatore.CodiceTessera FROM Allenatore,Societa WHERE Allenatore.IDSocieta=Societa.IDSocieta AND Societa.IDSocieta=@IDSocieta;";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDSocieta", idsocieta));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }//torna lista allenatori di una societa
        public string GetAtletaByTessera(string tessera, int idsocieta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT CONCAT(Atleta.Nome,' ',Atleta.Cognome)as Atleta FROM Atleta WHERE Atleta.CodiceTessera=@Tessera AND Atleta.IDSocieta=@idSocieta;";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Tessera", tessera));
            comando.Parameters.Add(new SqlParameter("idSocieta", idsocieta));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query.Rows[0]["Atleta"].ToString();
        } //torna nome cognome atleta con la tessera
        public string GetAllenatoreByTessera(string tessera, int idsocieta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT CONCAT(Allenatore.Nome,' ',Allenatore.Cognome)as Allenatore FROM Allenatore WHERE Allenatore.CodiceTessera=@Tessera AND Allenatore.IDSocieta=@idSocieta;";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Tessera", tessera));
            comando.Parameters.Add(new SqlParameter("idSocieta", idsocieta));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query.Rows[0]["Allenatore"].ToString();
        }//torna nome cognome allenatore con la tessera
        public bool ControlloSupervisore(int idDelegato, int idTorneo)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT IDSupervisore FROM Torneo WHERE Torneo.IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                if (Convert.ToInt32(query.Rows[0][0]) == idDelegato)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
        public bool EliminaTeam(int idTorneo, int idSquadra)
        {
            SqlCommand comando;
            SqlParameter parametro;
            string sql;
            try
            {
                //Elimino il team
                sql = "";
                sql += "DELETE FROM ListaIscritti WHERE IDSquadra=@IDSquadra AND IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("IDSquadra", idSquadra);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDTorneo", idTorneo);
                comando.Parameters.Add(parametro);
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                string errore = e.Message;
                return false;
            }
        }
        public bool AssegnaWildCard(int idTorneo, int idSquadra)
        {
            SqlCommand comando;
            SqlParameter parametro;
            string sql;
            try
            {
                sql = "";
                sql += "UPDATE ListaIscritti SET WC=@WC WHERE IDTorneo=@IDTorneo AND IDSquadra=@IDSquadra";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("WC", true);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDTorneo", idTorneo);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDSquadra", idSquadra);
                comando.Parameters.Add(parametro);
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
        public int GetIdSocietaByAtleta(int idatleta)
        {
            SqlDataAdapter adapter;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT IDSocieta FROM Atleta WHERE IDAtleta=" + idatleta + ";";
                query = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return Convert.ToInt32(query.Rows[0]["IDSocieta"]);
            }
            catch
            {
                return 0;
            }
        }//ritorna id societa dal id alteta
        public int GetIDSquadraByNomeTeam(int idatleta, int idtorneo)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT Squadra.IDSquadra FROM Squadra,ListaIscritti WHERE (Squadra.IDAtleta1=@IDAtleta OR Squadra.IDAtleta2=@IDAtleta) AND ListaIscritti.IDTorneo=@idtorneo AND ListaIscritti.IDSquadra=Squadra.IDSquadra";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDAtleta", idatleta));
                comando.Parameters.Add(new SqlParameter("idtorneo", idtorneo));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return Convert.ToInt32(query.Rows[0]["IDSquadra"]);
            }
            catch
            {
                return 0;
            }
        }//ritorna id squadra dal id atleta e nome team
        public DataTable GetidTorneiIscritti(int idatleta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT IDTorneo FROM ListaIscritti,Squadra WHERE Squadra.IDSquadra=ListaIscritti.IDSquadra AND (Squadra.IDAtleta1=@IDAtleta OR Squadra.IDAtleta2=@IDAtleta)";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDAtleta", idatleta));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return GetTorneiEntroDataSocieta(query);//Scarico tutti i tornei iscritto di quel atleta
        }//torna lista tornei iscritti
        public DataTable GetTorneiEntroDataSocieta(DataTable dt)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            string[] arrray = dt.Rows.OfType<DataRow>().Select(k => k[0].ToString()).ToArray();
            conn.Open();
            query = new DataTable();
            for (int i = 0; i < arrray.Length; i++)
            {
                sql = "";
                sql += "SELECT DISTINCT Torneo.IDTorneo, Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome, ' ', Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome, ' ', SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale, CONCAT(DirettoreCompetizione.Nome, ' ', DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche" +
                "FROM(((((((((Torneo Left join TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)Left Join DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT join ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT join DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)Left join DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT Join FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)Left Join ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)left join Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)Left Join Comune On Impianto.IDComune = Comune.IDComune) " +
                " WHERE Autorizzato= 1 AND Torneo.IDTorneo= @idtorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("idtorneo", arrray[i]));
                adapter = new SqlDataAdapter(comando);
                adapter.Fill(query);
            }
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public string EliminaTeamByAtleta(int idTorneo, int idSquadra)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            try
            {
                //controllo che la data di iscrizione sia ancora aperta
                sql = "";
                sql = "SELECT IDTorneo FROM Torneo WHERE IDTorneo=@idTorneo AND CAST(DataChiusuraIscrizioni as DATE) >= @Data";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("idTorneo", idTorneo));
                comando.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date.ToString("MM-dd-yyyy")));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                // la data di iscrizione è ancora aperta
                if (query.Rows.Count > 0)
                {
                    //Elimino la squadra
                    sql = "";
                    sql += "DELETE FROM ListaIscritti WHERE IDSquadra=@IDSquadra AND IDTorneo=@IDtorneo";
                    comando = new SqlCommand(sql, conn);
                    comando.Parameters.Add(new SqlParameter("IDSquadra", idSquadra));
                    comando.Parameters.Add(new SqlParameter("IDtorneo", idTorneo));
                    query = new DataTable();
                    adapter = new SqlDataAdapter(comando);
                    conn.Open();
                    adapter.Fill(query);
                    conn.Close();
                    return "Squadra eliminata con successo";
                }
                else return "Data di iscrizione chiusa.";
            }
            catch { return "Errore nell'eliminazione della squadra."; }
        }//elimina una squadra da un torneo by atleta 
        public bool AggiungiImpianto(string nomeComune, string nomeImpianto, int numeroCampi, string indirizzo, string cap, string descrizione, string email, string sito, string tel, int idSocieta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            string sql;
            DataTable idImpianto;
            try
            {
                //Insert per aggiungere un impianto
                sql = "";
                sql += "INSERT INTO Impianto(IDComune,NomeImpianto,NumeroCampi,Indirizzo,CAP,Descrizione,Email,Sito,Tel) ";
                sql += "VALUES(@IDComune,@NomeImpianto,@NumeroCampi,@Indirizzo,@CAP,@Descrizione,@Email,@Sito,@Tel)";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("IDComune", GetIDComuneResidenza(nomeComune).Rows[0][0]);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("NomeImpianto", nomeImpianto);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("NumeroCampi", numeroCampi);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Indirizzo", indirizzo);
                comando.Parameters.Add(parametro);
                if (cap != null)
                    parametro = new SqlParameter("CAP", cap);
                else
                    parametro = new SqlParameter("CAP", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (descrizione != null)
                    parametro = new SqlParameter("Descrizione", descrizione);
                else
                    parametro = new SqlParameter("Descrizione", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (email != null)
                    parametro = new SqlParameter("Email", email);
                else
                    parametro = new SqlParameter("Email", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (sito != null)
                    parametro = new SqlParameter("Sito", sito);
                else
                    parametro = new SqlParameter("Sito", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (tel != null)
                    parametro = new SqlParameter("Tel", tel);
                else
                    parametro = new SqlParameter("Tel", DBNull.Value);
                comando.Parameters.Add(parametro);
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                //Prendo l'IDImpianto
                sql = "";
                sql += "Select idimpianto from Impianto WHERE NomeImpianto=@NomeImpianto";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("NomeImpianto", nomeImpianto));
                adapter = new SqlDataAdapter(comando);
                idImpianto = new DataTable();
                conn.Open();
                adapter.Fill(idImpianto);
                conn.Close();
                //Insert per associare l'impianto alla società
                if (idImpianto.Rows.Count > 0)//Controllo che l'impianto sia stato trovato
                {
                    sql = "";
                    sql += "INSERT INTO ImpiantoSocieta(IDSocieta,IDImpianto) VALUES(@IDSocieta,@IDImpianto)";
                    comando = new SqlCommand(sql, conn);
                    comando.Parameters.Add(new SqlParameter("IDSocieta", idSocieta));
                    comando.Parameters.Add(new SqlParameter("IDImpianto", idImpianto.Rows[0][0]));
                    conn.Open();
                    comando.ExecuteNonQuery();
                    conn.Close();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                string errore = e.Message;
                return false;
            }
        }
        public bool AddArbitro(int idDelegato, int idTorneo, bool mezzaGiornata)
        {
            SqlCommand comando;
            string sql;
            //Metodo che aggiunge un arbitro al torneo
            try
            {
                sql = "";
                sql += "INSERT INTO ArbitraTorneo(IDTorneo,IDDelegato,MezzaGiornata) " +
                    "VALUES(@IDTorneo,@IDDelegato,@MezzaGiornata)";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                comando.Parameters.Add(new SqlParameter("IDDelegato", idDelegato));
                comando.Parameters.Add(new SqlParameter("MezzaGiornata", mezzaGiornata));
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                string errore = e.Message;
                return false;
            }
        }
        public DataTable GetArbitriTorneo(int idTorneo)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT CONCAT(DelegatoTecnico.Nome,' ',DelegatoTecnico.Cognome) AS Arbitro ";
            sql += "FROM DelegatoTecnico,ArbitraTorneo ";
            sql += "WHERE ArbitraTorneo.IDTorneo=@IDTorneo AND ArbitraTorneo.IDDelegato=DelegatoTecnico.IDDelegato";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
            adapter = new SqlDataAdapter(comando);
            query = new DataTable();
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }

        public DataTable GetImpiantiSocieta(int idSocieta)
        {
            //Metodo che restituisce gli impianti di una società
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "Select Impianto.NomeImpianto,Comune.Citta as Città,numerocampi,cap,Impianto.Descrizione,Impianto.Email,Impianto.Sito,Impianto.Tel " +
                   "From Impianto, ImpiantoSocieta, Comune " +
                   "Where ImpiantoSocieta.IDSocieta = @IDSocieta AND Impianto.IDImpianto = ImpiantoSocieta.IDImpianto And Comune.IDComune = Impianto.IDComune";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDSocieta", idSocieta));
            adapter = new SqlDataAdapter(comando);
            query = new DataTable();
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }

        public bool CreaListaIngresso(int idTorneo)
        {
            SqlCommand comando;
            SqlDataAdapter adapter;
            DataTable query;
            string sql;
            int maxTeam, contaSquadre = 0;//Numero massimo di team che ci possono essere e contatore per le squadre
            try
            {
                //Prendo il numero massimo di team che possono partecipare al torneo
                sql = "";
                sql += "SELECT NumMaxTeamMainDraw FROM Torneo WHERE IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                query = new DataTable();
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                maxTeam = Convert.ToInt32(query.Rows[0][0]);
                //Controllo se ci sono squadre con WildCard
                sql = "";
                sql += "SELECT * FROM ListaIscritti WHERE WC=1 AND IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                query = new DataTable();
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                if (query.Rows.Count > 0 && contaSquadre <= maxTeam)
                {
                    for (int i = 0; i < query.Rows.Count; i++)
                    {
                        if (contaSquadre <= maxTeam)
                        {
                            sql = "";
                            sql += "INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints) ";
                            sql += "VALUES(@IDSquadra,@IDTorneo,@IDAllenatore,@EntryPoints)";
                            comando.Parameters.Add(new SqlParameter("IDSquadra", query.Rows[i]["IDSquadra"]));
                            comando.Parameters.Add(new SqlParameter("IDTorneo", query.Rows[i]["IDTorneo"]));
                            if (query.Rows[i]["IDAllenatore"] != null)
                                comando.Parameters.Add(new SqlParameter("IDAllenatore", query.Rows[i]["IDAllenatore"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDAllenatore", DBNull.Value));
                            if (query.Rows[i]["EntryPoints"] != null)
                                comando.Parameters.Add(new SqlParameter("EntryPoints", query.Rows[i]["EntryPoints"]));
                            else
                                comando.Parameters.Add(new SqlParameter("EntryPoints", DBNull.Value));
                            conn.Open();
                            comando.ExecuteNonQuery();
                            conn.Close();
                            contaSquadre++;
                        }
                    }
                }
                //Prendo le squadre con gli EntryPoints più alti
                sql = "";
                sql += "SELECT * FROM ListaIscritti,Torneo WHERE ListaIscritti.idtorneo=@IDTorneo and Torneo.IDTorneo=ListaIscritti.IDTorneo and ListaIscritti.EntryPoints>Torneo.QuotaIscrizione ORDER BY ListaIscritti.EntryPoints DESC";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                query = new DataTable();
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                if (query.Rows.Count > 0 && contaSquadre <= maxTeam)
                {
                    for (int i = 0; i < query.Rows.Count; i++)
                    {
                        if (contaSquadre <= maxTeam)
                        {
                            sql = "";
                            sql += "INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints) ";
                            sql += "VALUES(@IDSquadra,@IDTorneo,@IDAllenatore,@EntryPoints)";
                            comando.Parameters.Add(new SqlParameter("IDSquadra", query.Rows[i]["IDSquadra"]));
                            comando.Parameters.Add(new SqlParameter("IDTorneo", query.Rows[i]["IDTorneo"]));
                            if (query.Rows[i]["IDAllenatore"] != null)
                                comando.Parameters.Add(new SqlParameter("IDAllenatore", query.Rows[i]["IDAllenatore"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDAllenatore", DBNull.Value));
                            comando.Parameters.Add(new SqlParameter("EntryPoints", query.Rows[i]["EntryPoints"]));
                            conn.Open();
                            comando.ExecuteNonQuery();
                            conn.Close();
                            contaSquadre++;
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                string errore = e.Message;
                return false;
            }
        }
        public DataTable GetTorneiDisputatiByDelegato(int idDelegato)
        {
            //Metodo che retituisce i tornei a cui ha partecipato un supervisore
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable query;
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche" +
            "FROM(((((((((Torneo Left join TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)Left Join DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT join ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT join DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)Left join DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT Join FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)Left Join ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)left join Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)Left Join Comune On Impianto.IDComune = Comune.IDComune) " +
            "WHERE Torneo.IDSupervisore=@IDDelegato OR Torneo.IDSupervisoreArbitrale=@IDDelegato or Torneo.IDDirettoreCompetizione=@IDDelegato";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDDelegato", idDelegato));
            adapter = new SqlDataAdapter(comando);
            query = new DataTable();
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable SquadreTorneo(int idTorneo)
        {
            try
            {
                SqlDataAdapter adapter;
                SqlCommand comando;
                DataTable query;
                string sql;
                sql = "";
                sql += "SELECT DISTINCT Squadra.IDSquadra,Squadra.NomeTeam, CONCAT(Atleta1.Nome,' ',Atleta1.Cognome) as Atleta1, CONCAT(Atleta2.Nome,' ',Atleta2.Cognome) as Atleta2 ";
                sql += "FROM Squadra, Partecipa, Atleta as Atleta1, Atleta as Atleta2 ";
                sql += "WHERE Partecipa.IDSquadra = Squadra.IDSquadra AND Partecipa.IDTorneo = @IDTorneo AND Squadra.IDAtleta1 = Atleta1.IDAtleta AND Squadra.IDAtleta2 = Atleta2.IDAtleta; ";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                query = new DataTable();
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return query;
            }catch(Exception e)
            {
                string error = e.Message;
                return null;
            }
        }

        public DataTable GetSquadra(int idsquadra)
        {
            try
            {
                SqlDataAdapter adapter;
                SqlCommand comando;
                DataTable query;
                string sql;
                sql = "";
                sql += "SELECT DISTINCT Squadra.IDSquadra,Squadra.NomeTeam, CONCAT(Atleta1.Nome,' ',Atleta1.Cognome) as Atleta1, CONCAT(Atleta2.Nome,' ',Atleta2.Cognome) as Atleta2 ";
                sql += "FROM Squadra, Atleta as Atleta1, Atleta as Atleta2 ";
                sql += "WHERE Squadra.IDSquadra=@IDSquadra AND Squadra.IDAtleta1 = Atleta1.IDAtleta AND Squadra.IDAtleta2 = Atleta2.IDAtleta; ";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDSquadra", idsquadra));
                adapter = new SqlDataAdapter(comando);
                query = new DataTable();
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return query;
            }
            catch (Exception e)
            {
                string error = e.Message;
                return null;
            }
        }
    }
}
