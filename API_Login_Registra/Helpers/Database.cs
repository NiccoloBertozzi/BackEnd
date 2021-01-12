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

        // parametri token JWT
        string JWT_secretKey = ConfigurationManager.AppSetting["AppSettings:Secret"];
        int JWT_expirationMinutes = Convert.ToInt32(ConfigurationManager.AppSetting["AppSettings:ExpirationMinute"]);

        public Database(/*IConfiguration config*/)//Richiamo il file appsettings
        {
            builder = new SqlConnectionStringBuilder();
            /*builder.DataSource = config.GetSection("DBSettings:DataSource").Value;
            builder.UserID = config.GetSection("DBSettings:UserID").Value;
            builder.Password = config.GetSection("DBSettings:Password").Value;
            builder.InitialCatalog = config.GetSection("DBSettings:InitialCatalog").Value;*/
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
                    claims.Add(new Claim(ClaimTypes.Role, "Delegato"));
                else if (dr["IDAtleta"].ToString() != "")
                    claims.Add(new Claim(ClaimTypes.Role, "Atleta"));
                else if (dr["IDAllenatore"].ToString() != "")
                    claims.Add(new Claim(ClaimTypes.Role, "Allenatore"));
                else if (dr["Admin"].ToString() != "")
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
        public DataTable GetAnagrafica(int idAtleta)
        {
            conn.Open();
            string sql = "";
            sql += "SELECT Nome,Cognome,DataNascita,Email,Tel,Sesso ";
            sql += "FROM Atleta ";
            sql += "WHERE idAtleta='" + idAtleta + "'";
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
            sql += " SELECT Torneo.idTorneo,IDTipoTorneo,PuntiVittoria,Torneo.Montepremi,DataInizio,DataFine,Torneo.Gender,NumTeamTabellone,NumTeamQualifiche,Squadra.IDAtleta1,Squadra.IDAtleta2";
            sql += " FROM Torneo,ListaIscritti,Squadra,Atleta";
            sql += " WHERE ListaIscritti.IDSquadra=Squadra.IDSquadra AND ListaIscritti.IDTorneo=Torneo.IDTorneo " +
                   " AND (Squadra.IDAtleta1=Atleta.IDAtleta OR Squadra.IDAtleta2=Atleta.IDAtleta)" +
                   " AND (Squadra.idAtleta1='" + idAtleta + "' OR Squadra.idAtleta2='" + idAtleta + "')";

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
            sql += " SELECT *";
            sql += " FROM Torneo";
            sql += " WHERE CAST(DataInizio as DATE) <= '" + data.Date.ToString() + "' ";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public bool RegisterAllenatore(int idSocieta, string codTessera, string grado, string nome, string cognome, string sesso, string cF, DateTime dataNascita, string comuneNascita, string comuneResidenza, string indirizzo, string cap, string email, string tel, string pwd)
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
        public bool RegisterAtleta(int idSocieta, string codTessera, string nome, string cognome, string sesso, string cF, DateTime dataNascita, string comuneNascita, string comuneResidenza, string indirizzo, string cap, string email, string tel, int altezza, int peso, DateTime scadenzaCert, string pwd)
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
                if(sesso!=null)
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
        public bool RegisterDelegato(string nome, string cognome, string sesso, string cF, DateTime dataNascita, string comuneNascita, string comuneResidenza, string indirizzo, string cap, string email, string tel, bool arbitro, bool supervisore, string pwd)
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
            string sql;
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
            string sql;
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
            string sql;
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
            string sql;
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
            string sql;
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
            string sql;
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
    }
}
