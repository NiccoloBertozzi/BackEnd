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

namespace WebAPIAuthJWT.Helpers
{
    public class Database
    {
        SqlConnectionStringBuilder builder;
        SqlConnection conn;
        SqlDataAdapter adapter;
        SqlCommand comando;
        SqlParameter parametro;
        DataTable query;
        string sql;

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
            bool autenticato = false;
            string pswCript;//Password criptata
            try
            {
                string sql = "";
                sql += "SELECT * ";
                sql += "FROM Login ";
                sql += "WHERE Email='" + email + "'";
                query = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
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

            }
            return autenticato;
        }

        public string GetToken(string email)//Rilascio token
        {
            DataTable dtUtente = this.CheckUser(email);
            string sql;

            //Creazione del Token Jwt
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JWT_secretKey);
            //Query
            sql = "";
            sql += "SELECT * FROM Login WHERE Email=" + "'" + email + "'";
            DataTable dtProfili = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(dtProfili);
            conn.Close();
            /*DataTable dtProfili = DBTable(
                string.Format("SELECT * FROM Login WHERE Email ='{0}'", email));*/

            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Name, dtUtente.Rows[0]["Email"].ToString()));

            foreach (DataRow dr in dtProfili.Rows)
            {
                if (dr["IDSocieta"].ToString() !="")
                    claims.Add(new Claim(ClaimTypes.Role, "Societa"));
                else if (dr["IDDelegato"].ToString() != "")
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Delegato"));
                    if (Convert.ToInt32(dr["AdminDelegati"]) == 1)
                        claims.Add(new Claim(ClaimTypes.Role, "AdminDelegato"));
                }
                else if (dr["IDAtleta"].ToString() != "")
                    claims.Add(new Claim(ClaimTypes.Role, "Atleta"));
                else if (dr["IDAllenatore"].ToString() != "")
                    claims.Add(new Claim(ClaimTypes.Role, "Allenatore"));
                if (Convert.ToInt32(dr["Admin"].ToString()) == 1)
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
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

            return tokenHandler.WriteToken(token);
        }

        public DataTable CheckUser(string email)
        {
            string sql;

            // prepara la QUERY
            sql = "";
            sql += "SELECT * ";
            sql += "FROM Login ";
            sql += "WHERE Email='" + email + "'";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
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
            conn.Open();
            string sql;
            sql = "";
            sql += "SELECT IDComune ";
            sql += "FROM Comune ";
            sql += "WHERE Citta='" + comuneNascita + "'";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);        
            adapter.Fill(query);
            conn.Close();
            int  p = query.Rows.Count;
            return query;
        }

        public DataTable GetIDComuneResidenza(string comuneResidenza)
        {
            conn.Open();
            string sql;
            sql = "";
            sql += "SELECT IDComune ";
            sql += "FROM Comune ";
            sql += "WHERE Citta='" + comuneResidenza + "'";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }

        public DataTable GetIDSocieta(string nomeSocieta)
        {
            conn.Open();
            string sql;
            sql = "";
            sql += "SELECT IDSocieta ";
            sql += "FROM Societa ";
            sql += "WHERE NomeSocieta='" + nomeSocieta + "'";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetAnagrafica(int id_Atleta)
        {
            conn.Open();
            string sql = "";
            sql += "SELECT Nome,Cognome,DataNascita,Email,Tel,Sesso ";
            sql += "FROM Atleta ";
            sql += "WHERE IDAtleta=" + id_Atleta;
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetIscrizioni(int idAtleta)
        {
            conn.Open();
            string sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,PuntiVittoria,Torneo.Montepremi,DataInizio,DataFine,Torneo.Gender,NumTeamTabellone,NumTeamQualifiche,Squadra.NomeTeam,CONCAT(atleta1.Nome,' ',atleta1.cognome) AS Atleta1,CONCAT(atleta2.Nome,' ',atleta2.cognome) AS Atleta2 ";
            sql += "FROM (((((Torneo " +
            "LEFT JOIN TipoTorneo ON Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)"+
            "LEFT JOIN ListaIscritti ON ListaIscritti.IDTorneo=Torneo.IDTorneo)"+
            "LEFT JOIN Squadra ON ListaIscritti.IDSquadra = Squadra.IDSquadra)"+
            "LEFT JOIN Atleta atleta1 ON Squadra.IDAtleta1 = atleta1.IDAtleta)"+
            "LEFT JOIN Atleta atleta2 ON Squadra.IDAtleta2 = atleta2.IDAtleta)";
            sql += "WHERE Squadra.IDAtleta1=" + idAtleta + "or Squadra.IDAtleta2=" + idAtleta;
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetTorneiEntroData(DateTime data)
        {
            conn.Open();
            string sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome, ' ', Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome, ' ', SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale, CONCAT(DirettoreCompetizione.Nome, ' ', DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumTeamTabellone,Torneo.NumTeamQualifiche " +
            "FROM(((((((((Torneo Left join TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)Left Join DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT join ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT join DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)Left join DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT Join FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)Left Join ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)left join Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)Left Join Comune On Impianto.IDComune = Comune.IDComune) " +
            " WHERE CAST(DataInizio as DATE) >= '" + data.Date.ToString() + "' ";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public bool RegisterAllenatore(int idSocieta, string codTessera, string grado, string nome, string cognome, char sesso, string cF, DateTime dataNascita, string comuneNascita, string comuneResidenza, string indirizzo, string cap, string email, string tel, string pwd)
        {
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
                parametro = new SqlParameter("Sesso", sesso);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CF", cF);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataNascita", dataNascita);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDComuneNascita", comuneNascita);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDComuneResidenza", comuneResidenza);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Indirizzo", indirizzo);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CAP", cap);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Email", email);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Tel", tel);
                comando.Parameters.Add(parametro);
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                //Cifro la password
                PasswordHasher hasher = new PasswordHasher();
                string cifredPWD = hasher.Hash(pwd);
                //Faccio una query per prendere l'IDAllenatore
                sql = "";
                sql += "SELECT IDAllenatore FROM Allenatore WHERE Email='" + email + "'";
                idAllenatore = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
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
                parametro = new SqlParameter("Sesso", sesso);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CF", cF);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataNascita", dataNascita);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDComuneNascita", comuneNascita);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDComuneResidenza", comuneResidenza);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Indirizzo", indirizzo);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CAP", cap);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Email", email);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Tel", tel);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Altezza", altezza);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Peso", peso);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataScadenzaCertificato", scadenzaCert);
                comando.Parameters.Add(parametro);
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                //Cifro la password
                PasswordHasher hasher = new PasswordHasher();
                string cifredPWD = hasher.Hash(pwd);
                //Faccio una query per prendere l'IDAllenatore
                sql = "";
                sql += "SELECT IDAtleta FROM Atleta WHERE Email='" + email + "'";
                idAtleta = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
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
                parametro = new SqlParameter("Sesso", sesso);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CF", cF);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataNascita", dataNascita);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDComuneNascita", comuneNascita);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDComuneResidenza", comuneResidenza);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Indirizzo", indirizzo);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CAP", cap);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Email", email);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Tel", tel);
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
                sql += "SELECT IDDelegato FROM DelegatoTecnico WHERE Email='" + email + "'";
                idDelegato = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
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
            }
            catch (Exception e)
            {

            }
            return regRiuscita;
        }
        public bool RegisterSocieta(string comune, string nomeSocieta, string indirizzo, string cap, DateTime dataFondazione, DateTime dataAffilizione, string codAffiliazione, bool affiliata, string email, string sito, string tel1, string tel2, string pec, string piva, string cF, string cU, string pwd)
        {
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
                parametro = new SqlParameter("CAP", cap);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataFondazione", dataFondazione);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("DataAffiliazione", dataAffilizione);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CodiceAffiliazione", codAffiliazione);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Affiliata", affiliata);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Email", email);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Sito", sito);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Tel1", tel1);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Tel2", tel2);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Pec", pec);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("PIVA", piva);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CF", cF);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("CU", cU);
                comando.Parameters.Add(parametro);
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                //Cifro la password
                PasswordHasher hasher = new PasswordHasher();
                string cifredPWD = hasher.Hash(pwd);
                //Faccio una query per prendere l'IDAllenatore
                sql = "";
                sql += "SELECT IDSocieta FROM Societa WHERE Email='" + email + "'";
                idSocieta = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
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
            DataTable[] risultati = new DataTable[2];
            DataTable ris1, ris2;
            sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumTeamTabellone,Torneo.NumTeamQualifiche " +
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
                        sql += "SET PT1S1=@PuntiTeam1,PT2S1=@PuntiTeam2 ";
                        break;
                    case 2:
                        sql += "SET PT1S2=@PuntiTeam1,PT2S2 =@PuntiTeam2 ";
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
            "WHERE Partita.IDTorneo =" + idTorneo + " AND Partita.NumPartita = " + numPartita + ";";
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
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumTeamTabellone,Torneo.NumTeamQualifiche " +
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
        public DataTable[] GetAllTorneiMaschili()
        {
            DataTable[] risultati = new DataTable[2];
            DataTable ris1, ris2;
            sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumTeamTabellone,Torneo.NumTeamQualifiche " +
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
            DataTable[] risultati = new DataTable[2];
            DataTable ris1, ris2;
            sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumTeamTabellone,Torneo.NumTeamQualifiche " +
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
        public DataTable GetClassificaMaschile()
        {
            sql = "";
            sql += "WITH punteggi(idAtl,punti) AS (SELECT idatleta1, sum(punti)/ 2.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo " +
            "AND julianday('now')-julianday(datafine) < 120 GROUP BY idatleta1 UNION " +
            "SELECT idatleta2, sum(punti)/ 2.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo " +
            "AND julianday('now')-julianday(datafine) < 120 GROUP BY idatleta2 UNION " +
            "SELECT idatleta1, sum(punti)/ 4.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo " +
            "AND julianday('now')-julianday(datafine) BETWEEN 121 AND 365 GROUP BY idatleta1 " +
            "UNION " +
            "SELECT idatleta2, sum(punti)/ 4.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo " +
            "AND julianday('now')-julianday(datafine) BETWEEN 121 AND 365 GROUP BY idatleta2 " +
            ") " +
            "SELECT cognome, nome, sum(punti) " +
            "FROM punteggi, atleta WHERE idatleta = idAtl AND atleta.sesso = 'M' " +
            "GROUP BY idatleta ORDER BY sum(punti) DESC,Cognome,Nome";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetClassificaFemminile()
        {
            sql = "";
            sql += "WITH punteggi(idAtl,punti) AS (SELECT idatleta1, sum(punti)/ 2.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo " +
            "AND julianday('now')-julianday(datafine) < 120 GROUP BY idatleta1 UNION " +
            "SELECT idatleta2, sum(punti)/ 2.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo " +
            "AND julianday('now')-julianday(datafine) < 120 GROUP BY idatleta2 UNION " +
            "SELECT idatleta1, sum(punti)/ 4.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo " +
            "AND julianday('now')-julianday(datafine) BETWEEN 121 AND 365 GROUP BY idatleta1 " +
            "UNION " +
            "SELECT idatleta2, sum(punti)/ 4.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo " +
            "AND julianday('now')-julianday(datafine) BETWEEN 121 AND 365 GROUP BY idatleta2 " +
            ") " +
            "SELECT cognome, nome, sum(punti) " +
            "FROM punteggi, atleta WHERE idatleta = idAtl AND atleta.sesso = 'F' " +
            "GROUP BY idatleta ORDER BY sum(punti) DESC,Cognome,Nome";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetStoricoPartiteTorneo(int idTorneo)//Metodo che restituisce la lista delle partite di un torneo
        {
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
            "WHERE Partita.IDTorneo =" + idTorneo + ";";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public bool SetAllenatoreSquadra(int IdTorneo, int idSquadra, int idAllenatore)//Metodo che aggiunge l'allenatore all'interno della squadra iscritta
        {
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
        public DataTable[] GetTorneoEPartecipanti(string titoloTorneo)
        {
            DataTable[] risultati = new DataTable[3];
            risultati[0] = GetTorneoByTitolo(GetIDTorneo(titoloTorneo))[0];//Informazioni sul torneo
            risultati[1] = GetTorneoByTitolo(GetIDTorneo(titoloTorneo))[1];//Parametri del torneo
            sql = "";
            sql += "SELECT IDTorneo FROM Torneo WHERE Titolo='" + titoloTorneo + "'";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            risultati[2] = new DataTable();
            sql = "";
            sql += "SELECT CONCAT(Atleta1.Nome,' ',Atleta1.Cognome) AS Atleta1,CONCAT(Atleta2.Nome,' ',Atleta2.Cognome) AS Atleta2,Squadra.NomeTeam AS NomeTeam ";
            sql += "FROM(((ListaIscritti LEFT JOIN Squadra ON Squadra.IDSquadra=ListaIscritti.IDSquadra)LEFT JOIN Atleta Atleta1 ON Squadra.IDAtleta1=Atleta1.IDAtleta)LEFT JOIN Atleta Atleta2 ON Squadra.IDAtleta2=Atleta2.IDAtleta) ";
            sql += "WHERE ListaIscritti.IDTorneo=" + query.Rows[0]["IDTorneo"];
            adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(risultati[2]);
            conn.Close();
            return risultati;
        }
        public bool CreaTorneo(string titolo, int puntiVittoria, float montepremi, DateTime dataChiusuraIscrizioni, DateTime dataInizio, DateTime dataFine, char genere, string formulaTorneo, int numTeamTabellone, int numTeamQualifiche, string[] parametriTorneo, string tipoTorneo, string[] impianti)
        {
            DataTable idFormula, idTipoTorneo, idTorneo;
            List<int> idParametriTorneo = new List<int>();
            List<int> idImpianti = new List<int>();
            try
            {
                //Trovo l'IDFormula
                sql = "";
                sql += "SELECT IDFormula FROM FormulaTorneo WHERE Formula='" + formulaTorneo + "'";
                idFormula = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
                conn.Open();
                adapter.Fill(idFormula);
                conn.Close();
                //Trovo l'IDTipoTorneo
                sql = "";
                sql += "SELECT IDTipoTorneo FROM TipoTorneo WHERE Descrizione='" + tipoTorneo + "'";
                idTipoTorneo = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
                conn.Open();
                adapter.Fill(idTipoTorneo);
                conn.Close();
                //Creo il torneo
                sql = "";
                sql += "INSERT INTO Torneo(IDTipoTorneo,IDFormula,Titolo,PuntiVittoria,Montepremi,DataChiusuraIscrizioni,DataInizio,DataFine,Gender,NumTeamTabellone,NumTeamQualifiche) ";
                sql += "VALUES(@IDTipoTorneo,@IDFormula,@Titolo,@PuntiVittoria,@Montepremi,@DataChiusuraIScrizioni,@DataInzio,@DataFine,@Gender,@NumTeamTabellone,@NumTeamQualifiche)";
                comando = new SqlCommand(sql, conn);
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
                parametro = new SqlParameter("NumTeamTabellone", numTeamTabellone);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("NumTeamQualifiche", numTeamQualifiche);
                comando.Parameters.Add(parametro);
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                //Trovo gli ID dei parametri
                for(int i = 0; i < parametriTorneo.Length; i++)
                {
                    sql = "";
                    sql += "SELECT IDParametro FROM ParametroQualita WHERE NomeParametro='" + parametriTorneo[i] + "'";
                    query = new DataTable();
                    adapter = new SqlDataAdapter(sql, conn);
                    conn.Open();
                    adapter.Fill(query);
                    conn.Close();
                    if (query.Rows.Count > 0)
                        idParametriTorneo.Add(Convert.ToInt32(query.Rows[0][0]));
                }
                //Prendo l'IDTorneo
                sql = "";
                sql += "SELECT IDTorneo FROM Torneo WHERE Titolo='" + titolo + "'";
                idTorneo = new DataTable();
                adapter = new SqlDataAdapter(sql, conn);
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
                        parametro = new SqlParameter("IDTorneo", idTorneo.Rows[0][0]);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("IDParametro", idParametriTorneo[i]);
                        comando.Parameters.Add(parametro);
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                //Prendo gli id deiìgli impianti
                for(int i = 0; i < impianti.Length; i++)
                {
                    sql = "";
                    sql += "SELECT IDImpainto FROM Impianto WHERE NomeImpianto='" + impianti[i] + "'";
                    query = new DataTable();
                    adapter = new SqlDataAdapter(sql, conn);
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
            catch
            {
                return false;
            }
        }
    }
}
