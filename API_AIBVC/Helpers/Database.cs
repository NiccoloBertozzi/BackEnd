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
using Microsoft.AspNetCore.Routing.Matching;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Reflection;
using System.Diagnostics;
using API_AIBVC.Models;

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
        public int GetIDAllenatore(string tessera)
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
        public DataTable GetNomeCognomeSupervisore(string cf, string nome, string cognome)
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
            sql += "SELECT CONCAT(Cognome,' ',Nome)AS Delegato, CF, IDDelegato ";
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
            sql += "SELECT CONCAT(Cognome,' ',Nome)AS Delegato, CF, IDDelegato ";
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
            sql += "SELECT CONCAT(Cognome,' ',Nome)AS Delegato, CF, IDDelegato ";
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
        public DataTable GetComuni()
        {
            SqlDataAdapter adapter;
            DataTable query;
            conn.Open();
            string sql;
            sql = "";
            sql += "SELECT Comune.IDComune, Comune.Citta FROM Comune ORDER BY Citta ASC";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetAllSocieta()
        {
            SqlDataAdapter adapter;
            DataTable query;
            conn.Open();
            string sql;
            sql = "";
            sql += "SELECT Societa.IDSocieta, Societa.NomeSocieta FROM Societa ORDER BY NomeSocieta ASC";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(query);
            conn.Close();
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
            sql += "SELECT Atleta.CodiceTessera,Societa.NomeSocieta,Atleta.Nome,Atleta.Cognome,Atleta.Sesso,Atleta.CF,Atleta.DataNascita,ComuneNascita.Citta as ComuneNascita,ComuneResidenza.Citta as ComuneResidenza,Atleta.Indirizzo,Atleta.CAP,Atleta.Email,Atleta.Tel,Atleta.Altezza,Atleta.Peso,Atleta.DataScadenzaCertificato " +
            "FROM Atleta, Societa, Comune as ComuneNascita, Comune as ComuneResidenza WHERE Atleta.IDSocieta = Societa.IDSocieta AND Atleta.IDComuneNascita = ComuneNascita.IDComune AND Atleta.IDComuneResidenza = ComuneResidenza.IDComune AND Atleta.IDAtleta =@IDAtleta";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDAtleta", id_Atleta));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public bool UpdateAnagraficaAtleta(Atleta atleta)
        {
            try
            {
                SqlDataAdapter adapter;
                SqlCommand comando;
                DataTable query;
                string sql;
                conn.Open();
                sql = "";
                sql += "UPDATE Atleta SET IDSocieta=@idsocieta,Nome=@idnome,Cognome=@idcognome,Sesso=@sesso,CF=@cf,DataNascita=@datanascita,IDComuneResidenza=@idcomuneresidenza,IDComuneNascita=@idcomunenascita,Indirizzo=@indirizzo,CAP=@cap,Email=@email,Tel=@tel,Altezza=@altezza,Peso=@peso,DataScadenzaCertificato=@datascadenzacertificato " +
                    "WHERE Atleta.IDAtleta=@idatleta;";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("idsocieta", atleta.IDSocieta));
                comando.Parameters.Add(new SqlParameter("idnome", atleta.Nome));
                comando.Parameters.Add(new SqlParameter("idcognome", atleta.Cognome));
                comando.Parameters.Add(new SqlParameter("sesso", atleta.Sesso));
                comando.Parameters.Add(new SqlParameter("cf", atleta.CF));
                comando.Parameters.Add(new SqlParameter("datanascita", atleta.DataNascita.Date));
                comando.Parameters.Add(new SqlParameter("idcomuneresidenza", atleta.IDComuneResidenza));
                comando.Parameters.Add(new SqlParameter("idcomunenascita", atleta.IDComuneNascita));
                comando.Parameters.Add(new SqlParameter("cap", atleta.CAP));
                comando.Parameters.Add(new SqlParameter("indirizzo", atleta.Indirizzo));
                comando.Parameters.Add(new SqlParameter("email", atleta.Email));
                comando.Parameters.Add(new SqlParameter("tel", atleta.Tel));
                comando.Parameters.Add(new SqlParameter("altezza", atleta.Altezza));
                comando.Parameters.Add(new SqlParameter("peso", atleta.Peso));
                comando.Parameters.Add(new SqlParameter("datascadenzacertificato", atleta.DataScadenzaCertificato.Date));
                comando.Parameters.Add(new SqlParameter("idatleta", atleta.IDAtleta));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                adapter.Fill(query);
                conn.Close();
                int p = query.Rows.Count;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public DataTable GetAnagraficaAllenatore(int id_Allenatore)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            conn.Open();
            sql = "";
            sql += "SELECT Allenatore.CodiceTessera,Allenatore.Grado,Societa.NomeSocieta,Allenatore.Nome,Allenatore.Cognome,Allenatore.Sesso,Allenatore.CF,Allenatore.DataNascita,ComuneNascita.Citta as ComuneNascita,ComuneResidenza.Citta as ComuneResidenza,Allenatore.Indirizzo,Allenatore.CAP,Allenatore.Email,Allenatore.Tel FROM Allenatore, Comune as ComuneNascita, Comune as ComuneResidenza ,Societa WHERE Allenatore.IDSocieta = Societa.IDSocieta AND Allenatore.IDComuneNascita = ComuneNascita.IDComune AND Allenatore.IDComuneResidenza = ComuneResidenza.IDComune AND Allenatore.IDAllenatore =@IDAllenatore;  ";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDAllenatore", id_Allenatore));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public bool UpdateAnagraficaAllenatore(Allenatore allenatore)
        {
            try
            {
                SqlDataAdapter adapter;
                SqlCommand comando;
                DataTable query;
                string sql;
                conn.Open();
                sql = "";
                sql += "UPDATE Allenatore SET IDSocieta=@idsocieta,Nome=@idnome,Cognome=@idcognome,Sesso=@sesso,CF=@cf,DataNascita=@datanascita,IDComuneResidenza=@idcomuneresidenza,IDComuneNascita=@idcomunenascita,Indirizzo=@indirizzo,CAP=@cap,Email=@email,Tel=@tel,Grado=@grado,CodiceTessera=@codicetessera " +
                    "WHERE Allenatore.IDAllenatore=@idallenatore;";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("idsocieta", allenatore.IDSocieta));
                comando.Parameters.Add(new SqlParameter("idnome", allenatore.Nome));
                comando.Parameters.Add(new SqlParameter("idcognome", allenatore.Cognome));
                comando.Parameters.Add(new SqlParameter("sesso", allenatore.Sesso));
                comando.Parameters.Add(new SqlParameter("cf", allenatore.CF));
                comando.Parameters.Add(new SqlParameter("datanascita", allenatore.DataNascita.Date));
                comando.Parameters.Add(new SqlParameter("idcomuneresidenza", allenatore.IDComuneResidenza));
                comando.Parameters.Add(new SqlParameter("idcomunenascita", allenatore.IDComuneNascita));
                comando.Parameters.Add(new SqlParameter("cap", allenatore.CAP));
                comando.Parameters.Add(new SqlParameter("indirizzo", allenatore.Indirizzo));
                comando.Parameters.Add(new SqlParameter("email", allenatore.Email));
                comando.Parameters.Add(new SqlParameter("tel", allenatore.Tel));
                comando.Parameters.Add(new SqlParameter("grado", allenatore.Grado));
                comando.Parameters.Add(new SqlParameter("codicetessera", allenatore.CodiceTessera));
                comando.Parameters.Add(new SqlParameter("idallenatore", allenatore.IDAllenatore));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                adapter.Fill(query);
                conn.Close();
                int p = query.Rows.Count;
                return true;
            }
            catch
            {
                return false;
            }
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
        public bool UpdateAnagraficaSocieta(Societa societa)
        {
            try
            {
                SqlDataAdapter adapter;
                SqlCommand comando;
                DataTable query;
                string sql;
                conn.Open();
                sql = "";
                sql += "UPDATE Societa SET IDComune=@idcomune,NomeSocieta=@nomeSocieta,Indirizzo=@indirizzo," +
                    "CAP = @cap,DataFondazione = @datafondazione, Citta=@Citta,DataAffiliazione = @dataaffiliazione," +
                    "CodiceAffiliazione = @codiceaffiliazione,Referente=@referente,Presidente=@presidente,Affiliata = @affiliata,Email = @email,Sito = @sito,Tel1 = @tel1," +
                    "Tel2 = @tel2,Pec = @pec,PIVA = @piva,CF = @cf,CU = @cu" +
                    " WHERE Societa.IDSocieta = @idsocieta;";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("idcomune", societa.IDComune));
                comando.Parameters.Add(new SqlParameter("nomeSocieta", societa.NomeSocieta));
                comando.Parameters.Add(new SqlParameter("indirizzo", societa.IndirizzoSoc));
                comando.Parameters.Add(new SqlParameter("cap", societa.CAPSoc));
                comando.Parameters.Add(new SqlParameter("datafondazione", societa.DataFondazione));
                comando.Parameters.Add(new SqlParameter("Citta", societa.citta));
                comando.Parameters.Add(new SqlParameter("dataaffiliazione", societa.DataAffiliazione));
                comando.Parameters.Add(new SqlParameter("codiceaffiliazione", societa.CodiceAffiliazione));
                comando.Parameters.Add(new SqlParameter("referente", societa.Referente));
                comando.Parameters.Add(new SqlParameter("presidente", societa.Presidente));
                comando.Parameters.Add(new SqlParameter("affiliata", societa.Affiliata));
                comando.Parameters.Add(new SqlParameter("email", societa.EmailSoc));
                comando.Parameters.Add(new SqlParameter("sito", societa.Sito));
                comando.Parameters.Add(new SqlParameter("tel1", societa.Tel1));
                comando.Parameters.Add(new SqlParameter("tel2", societa.Tel2));
                comando.Parameters.Add(new SqlParameter("pec", societa.Pec));
                comando.Parameters.Add(new SqlParameter("piva", societa.PIVA));
                comando.Parameters.Add(new SqlParameter("cf", societa.CFSoc));
                comando.Parameters.Add(new SqlParameter("cu", societa.CU));
                comando.Parameters.Add(new SqlParameter("idsocieta", societa.IDSocieta));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                adapter.Fill(query);
                conn.Close();
                int p = query.Rows.Count;
                return true;
            }
            catch (Exception e)
            {
                string c = e.Message;
                return false;
            }
        }
        public DataTable GetAnagraficaDelegato(int id_Delegato)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            conn.Open();
            sql = "";
            sql += "SELECT IDDelegato,Nome,Cognome,Sesso,CF,DataNascita,ComuneNascita.Citta as ComuneNascita, ComuneResidenza.Citta as ComuneResidenza, Indirizzo,CAP,Email,Tel,Arbitro,Supervisore,CodiceTessera  ";
            sql += "FROM DelegatoTecnico, Comune as ComuneNascita, Comune as ComuneResidenza ";
            sql += "WHERE DelegatoTecnico.IDComuneNascita = ComuneNascita.IDComune AND DelegatoTecnico.IDComuneResidenza = ComuneResidenza.IDComune AND IDDelegato=@IDDelegato";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDDelegato", id_Delegato));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public bool UpdateAnagraficaDelegato(DelegatoTecnico delegatoTecnico)
        {
            try
            {
                SqlDataAdapter adapter;
                SqlCommand comando;
                DataTable query;
                string sql;
                conn.Open();
                sql = "";
                sql += "UPDATE DelegatoTecnico SET Nome=@nome,Cognome=@cognome,Sesso=@sesso," +
                    "CF = @cf,DataNascita = @datanascita, IDComuneNascita = @idcomnascita," +
                    "IDComuneResidenza = @idcomresidenza,Indirizzo = @indirizzo,CAP = @cap,Email = @email,Tel = @tel," +
                    "Arbitro = @arbitro,Supervisore = @supervisore,CodiceTessera = @codtessera " +
                    " WHERE DelegatoTecnico.IDDelegato = @iddelegato;";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("nome", delegatoTecnico.Nome));
                comando.Parameters.Add(new SqlParameter("cognome", delegatoTecnico.Cognome));
                comando.Parameters.Add(new SqlParameter("sesso", delegatoTecnico.Sesso));
                comando.Parameters.Add(new SqlParameter("cf", delegatoTecnico.CF));
                comando.Parameters.Add(new SqlParameter("datanascita", delegatoTecnico.DataNascita));
                comando.Parameters.Add(new SqlParameter("idcomnascita", delegatoTecnico.IDComuneNascita));
                comando.Parameters.Add(new SqlParameter("idcomresidenza", delegatoTecnico.IDComuneResidenza));
                comando.Parameters.Add(new SqlParameter("indirizzo", delegatoTecnico.Indirizzo));
                comando.Parameters.Add(new SqlParameter("cap", delegatoTecnico.CAP));
                comando.Parameters.Add(new SqlParameter("email", delegatoTecnico.Email));
                comando.Parameters.Add(new SqlParameter("tel", delegatoTecnico.Tel));
                comando.Parameters.Add(new SqlParameter("arbitro", delegatoTecnico.Arbitro));
                comando.Parameters.Add(new SqlParameter("supervisore", delegatoTecnico.Supervisore));
                comando.Parameters.Add(new SqlParameter("codtessera", delegatoTecnico.CodiceTessera));
                comando.Parameters.Add(new SqlParameter("iddelegato", delegatoTecnico.IDDelegato));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                adapter.Fill(query);
                conn.Close();
                int p = query.Rows.Count;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public DataTable GetIscrizioni(int idAtleta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            conn.Open();
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,Societa.NomeSocieta,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.DataChiusuraIscrizioni,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,NumTeamQualificati,NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
            "FROM((((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On Torneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune)LEFT JOIN Societa On Societa.IDSocieta = Torneo.IDSocieta) " +
            "WHERE GETDATE()<CAST(Torneo.dataFine AS DATE) AND Torneo.IDTorneo IN(SELECT DISTINCT ListaIscritti.IDTorneo FROM ListaIscritti, Squadra, Torneo WHERE Squadra.IDSquadra= ListaIscritti.IDSquadra AND (Squadra.IDAtleta1= @IDAtleta OR Squadra.IDAtleta2= @IDAtleta))";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDAtleta", idAtleta));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        //Get di tutti i tornei con data di inizio<data attuale<data di chiusura
        public DataTable GetIscrizioniIniziate(int idAtleta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            conn.Open();
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,Societa.NomeSocieta,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.DataChiusuraIscrizioni,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,NumTeamQualificati,NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
            "FROM((((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On Torneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune)LEFT JOIN Societa On Societa.IDSocieta = Torneo.IDSocieta) " +
            "WHERE Squadra.IDAtleta1=@IDAtleta or Squadra.IDAtleta2=@IDAtleta AND " +
                " CAST(DataChiusuraIscrizioni as DATE) <= @Data AND Autorizzato = 1" +
                " AND CAST(DataInizio as DATE)>= @Data";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDAtleta", idAtleta));
            comando.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            adapter.Fill(query);
            conn.Close();
            int p = query.Rows.Count;
            return query;
        }
        public DataTable GetTorneiEntroData()
        {
            SqlDataAdapter adapter;
            string sql;
            DataTable risultato;
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,Societa.NomeSocieta,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.DataChiusuraIscrizioni,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,NumTeamQualificati,NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
            "FROM((((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On Torneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune)LEFT JOIN Societa On Societa.IDSocieta = Torneo.IDSocieta) " +
            "WHERE CAST(DataChiusuraIscrizioni as DATE) >= GETDATE() AND Autorizzato = 1";
            risultato = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(risultato);
            conn.Close();
            return risultato;
            /*if (risultati[0].Rows.Count > 0) //Se ci sarà bisogno dei parametri e degli impianti ci ripenseremo
            {
                for (int i = 0; i < risultati[0].Rows.Count; i++)
                {
                    sql = "";
                    sql += "SELECT Titolo,NomeParametro " +
                    "FROM ParametroQualita, ParametroTorneo, Torneo " +
                    "WHERE Torneo.IDTorneo=@IDTorneo AND ParametroTorneo.idtorneo = Torneo.idtorneo AND ParametroTorneo.IDParametro = ParametroQualita.IDParametro";
                    comando = new SqlCommand(sql, conn);
                    comando.Parameters.Add(new SqlParameter("IDTorneo", risultati[0].Rows[i]["IDTorneo"]));
                    risultati.Add(new DataTable());
                    adapter = new SqlDataAdapter(comando);
                    conn.Open();
                    adapter.Fill(risultati[i + 1]);
                    conn.Close();
                }
                for (int i = 0; i < risultati[0].Rows.Count; i++)
                {
                    sql = "";
                    sql += "SELECT NomeImpianto,Citta ";
                    sql += "FROM ((Impianto LEFT JOIN ImpiantoTorneo ON Impianto.IDImpianto=ImpiantoTorneo.IDImpianto)LEFT JOIN Comune ON Impianto.IDComune=Comune.IDComune) ";
                    sql += "WHERE ImpiantoTorneo.IDTorneo=@IDTorneo";
                    comando = new SqlCommand(sql, conn);
                    comando.Parameters.Add(new SqlParameter("IDTorneo", risultati[0].Rows[i]["IDTorneo"]));
                    risultati.Add(new DataTable());
                    adapter = new SqlDataAdapter(comando);
                    conn.Open();
                    adapter.Fill(risultati[i + 1]);
                    conn.Close();
                }
            }*/
        }
        public DataTable GetTorneiTipo(string Tipo)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable risultato;
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,Societa.NomeSocieta,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.DataChiusuraIscrizioni,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,NumTeamQualificati,NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
            "FROM((((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On Torneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune)LEFT JOIN Societa On Societa.IDSocieta = Torneo.IDSocieta) " +
            "WHERE TipoTorneo.Descrizione=@tipoTorneo AND Torneo.Autorizzato=1";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("tipoTorneo", Tipo));
            risultato = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(risultato);
            conn.Close();
            return risultato;
        }
        public DataTable GetTorneiFinitiAtleta(int idatleta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable risultato;
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,Societa.NomeSocieta,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.DataChiusuraIscrizioni,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,NumTeamQualificati,NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
            "FROM((((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On Torneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune)LEFT JOIN Societa On Societa.IDSocieta = Torneo.IDSocieta) " +
            "WHERE CAST(Torneo.DataFine as DATE) <= GETDATE() AND Torneo.IDTorneo IN(SELECT DISTINCT ListaIscritti.IDTorneo FROM ListaIscritti, Squadra, Torneo WHERE Squadra.IDSquadra= ListaIscritti.IDSquadra AND (Squadra.IDAtleta1= @IDAtleta OR Squadra.IDAtleta2= @IDAtleta))";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDAtleta", idatleta));
            risultato = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(risultato);
            conn.Close();
            return risultato;
        }
        public DataTable GetTorneiInCorsoAlteta(int idatleta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable risultato;
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,Societa.NomeSocieta,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.DataChiusuraIscrizioni,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,NumTeamQualificati,NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
            "FROM((((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On Torneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune)LEFT JOIN Societa On Societa.IDSocieta = Torneo.IDSocieta) " +
            "WHERE (GETDATE() BETWEEN CAST(Torneo.DataInizio as DATE) AND CAST(Torneo.DataFine as DATE))  AND Torneo.IDTorneo IN(SELECT DISTINCT ListaIscritti.IDTorneo FROM ListaIscritti, Squadra, Torneo WHERE Squadra.IDSquadra= ListaIscritti.IDSquadra AND (Squadra.IDAtleta1= @IDAtleta OR Squadra.IDAtleta2= @IDAtleta))";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDAtleta", idatleta));
            risultato = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(risultato);
            conn.Close();
            return risultato;
        }
        public DataTable GetTorneiFinitiAllenatore(int idallenatore)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable risultato;
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,PuntiVittoria,Torneo.Montepremi,DataInizio,DataFine,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,Squadra.NomeTeam,CONCAT(atleta1.Nome,' ',atleta1.cognome) AS Atleta1,CONCAT(atleta2.Nome,' ',atleta2.cognome) AS Atleta2 ";
            sql += "FROM (((((Torneo " +
            "LEFT JOIN TipoTorneo ON Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)" +
            "LEFT JOIN ListaIscritti ON ListaIscritti.IDTorneo=Torneo.IDTorneo)" +
            "LEFT JOIN Squadra ON ListaIscritti.IDSquadra = Squadra.IDSquadra)" +
            "LEFT JOIN Atleta atleta1 ON Squadra.IDAtleta1 = atleta1.IDAtleta)" +
            "LEFT JOIN Atleta atleta2 ON Squadra.IDAtleta2 = atleta2.IDAtleta) " +
            "WHERE(ListaIscritti.IDAllenatore = @idallenatore) AND GETDATE() > CAST(Torneo.dataFine AS DATE)";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("idallenatore", idallenatore));
            risultato = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(risultato);
            conn.Close();
            return risultato;
        }
        public DataTable GetTorneiInCorsoAllenatore(int idallenatore)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable risultato;
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,PuntiVittoria,Torneo.Montepremi,DataInizio,DataFine,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,Squadra.NomeTeam,CONCAT(atleta1.Nome,' ',atleta1.cognome) AS Atleta1,CONCAT(atleta2.Nome,' ',atleta2.cognome) AS Atleta2 ";
            sql += "FROM (((((Torneo " +
            "LEFT JOIN TipoTorneo ON Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)" +
            "LEFT JOIN ListaIscritti ON ListaIscritti.IDTorneo=Torneo.IDTorneo)" +
            "LEFT JOIN Squadra ON ListaIscritti.IDSquadra = Squadra.IDSquadra)" +
            "LEFT JOIN Atleta atleta1 ON Squadra.IDAtleta1 = atleta1.IDAtleta)" +
            "LEFT JOIN Atleta atleta2 ON Squadra.IDAtleta2 = atleta2.IDAtleta) " +
            "WHERE(GETDATE() BETWEEN CAST(Torneo.DataInizio as DATE) AND CAST(Torneo.DataFine as DATE)) AND Torneo.IDTorneo IN(SELECT DISTINCT ListaIscritti.IDTorneo FROM ListaIscritti, Squadra, Torneo WHERE Squadra.IDSquadra= ListaIscritti.IDSquadra)AND(ListaIscritti.IdAllenatore = @idallenatore)";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("idallenatore", idallenatore));
            risultato = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(risultato);
            conn.Close();
            return risultato;
        }
        public DataTable GetTorneiIscrittiAllenatore(int idallenatore)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable risultato;
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,PuntiVittoria,Torneo.Montepremi,DataInizio,DataFine,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,Squadra.NomeTeam,CONCAT(atleta1.Nome, ' ', atleta1.cognome) AS Atleta1, CONCAT(atleta2.Nome, ' ', atleta2.cognome) AS Atleta2 " +
            "FROM(((((Torneo " +
            "LEFT JOIN TipoTorneo ON Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo) " +
            "LEFT JOIN ListaIscritti ON ListaIscritti.IDTorneo = Torneo.IDTorneo) " +
            "LEFT JOIN Squadra ON ListaIscritti.IDSquadra = Squadra.IDSquadra) " +
            "LEFT JOIN Atleta atleta1 ON Squadra.IDAtleta1 = atleta1.IDAtleta) " +
            "LEFT JOIN Atleta atleta2 ON Squadra.IDAtleta2 = atleta2.IDAtleta) " +
            "WHERE(ListaIscritti.IDAllenatore = @idallenatore) AND GETDATE()< CAST(Torneo.dataInizio AS DATE)";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("idallenatore", idallenatore));
            risultato = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(risultato);
            conn.Close();
            return risultato;
        }
        public DataTable GetTorneiNonAutorizzatiEntroData()
        {
            SqlDataAdapter adapter;
            DataTable query;
            conn.Open();
            string sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,Societa.NomeSocieta,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.DataChiusuraIscrizioni,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,NumTeamQualificati,NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
            "FROM((((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On Torneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune)LEFT JOIN Societa On Societa.IDSocieta = Torneo.IDSocieta) " +
           "WHERE Autorizzato= 0";
            query = new DataTable();
            adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetTorneiEntroDataSocieta(DateTime data, int idsocieta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            conn.Open();
            string sql = "";
            sql += sql += "SELECT DISTINCT Torneo.IDTorneo, Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome, ' ', Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome, ' ', SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale, CONCAT(DirettoreCompetizione.Nome, ' ', DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche,Torneo.NumTeamQualificati,Torneo.NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
            "FROM(((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune) " +
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
        public string RegisterAllenatore(int idSocieta, string codTessera, string grado, string nome, string cognome, string sesso, string cF, DateTime dataNascita, string comuneNascita, string comuneResidenza, string indirizzo, string cap, string email, string tel, string pwd)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable idAllenatore;
            string regRiuscita = "false";
            string sql;
            try
            {
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
                    //Insert nella tabella Allenatore
                    sql = "";
                    sql += "INSERT INTO Allenatore(IDSocieta,CodiceTessera,Grado,Nome,Cognome,Sesso,CF,DataNascita,IDComuneNascita,IDComuneResidenza,Indirizzo,CAP,Email,Tel) ";
                    sql += "VALUES (@IDSocieta,@CodiceTessera,@Grado,@Nome,@Cognome,@Sesso,@CF,@DataNascita,@IDComuneNascita,@IDComuneResidenza,@Indirizzo,@CAP,@Email,@Tel)";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("IDSocieta", idSocieta);
                    comando.Parameters.Add(parametro);
                    if (codTessera != null)
                        parametro = new SqlParameter("CodiceTessera", codTessera);
                    else
                        parametro = new SqlParameter("CodiceTessera", DBNull.Value);
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
                        regRiuscita = "true";
                    }
                }
                else return "Allenatore già presente";
            }
            catch (Exception e)
            {
                string error = e.Message;
                return error;
            }
            return regRiuscita;
        }
        public string RegisterAtleta(int idSocieta, string codTessera, string nome, string cognome, char sesso, string cF, DateTime dataNascita, string comuneNascita, string comuneResidenza, string indirizzo, string cap, string email, string tel, int altezza, int peso, DateTime scadenzaCert, string pwd)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable idAtleta;
            string regRiuscita = "false";
            string sql;
            try
            {
                sql = "";
                sql += "SELECT IDAtleta FROM Atleta WHERE Email=@Email";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("Email", email));
                idAtleta = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(idAtleta);
                conn.Close();
                if (idAtleta.Rows.Count == 0)//Controllo abbia trovato l'allenatore
                {
                    //Insert nella tabella Atleta
                    sql = "";
                    sql += "INSERT INTO Atleta(IDSocieta,CodiceTessera,Nome,Cognome,Sesso,CF,DataNascita,IDComuneNascita,IDComuneResidenza,Indirizzo,CAP,Email,Tel,Altezza,Peso,DataScadenzaCertificato) ";
                    sql += "VALUES (@IDSocieta,@CodiceTessera,@Nome,@Cognome,@Sesso,@CF,@DataNascita,@IDComuneNascita,@IDComuneResidenza,@Indirizzo,@CAP,@Email,@Tel,@Altezza,@Peso,@DataScadenzaCertificato)";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("IDSocieta", idSocieta);
                    comando.Parameters.Add(parametro);
                    if (codTessera != null)
                        parametro = new SqlParameter("CodiceTessera", codTessera);
                    else
                        parametro = new SqlParameter("CodiceTessera", DBNull.Value);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("Nome", nome);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("Cognome", cognome);
                    comando.Parameters.Add(parametro);
                    if (sesso.ToString() != string.Empty)
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
                        regRiuscita = "true";
                    }
                }
                else return "Atleta già presente";
            }
            catch (Exception e)
            {
                string error = e.Message;
                return error;
            }
            return regRiuscita;
        }
        public string RegisterDelegato(string nome, string cognome, string sesso, string cF, DateTime dataNascita, string comuneNascita, string comuneResidenza, string indirizzo, string cap, string email, string tel, bool arbitro, bool supervisore, string pwd)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable idDelegato;
            string regRiuscita = "false";
            string sql;
            try
            {
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
                        regRiuscita = "true";
                    }
                }
                else return "Delegato già presente";
            }
            catch (Exception e)
            {
                string error = e.Message;
                return error;
            }
            return regRiuscita;
        }
        public string RegisterSocieta(string comune, string nomeSocieta, string indirizzo, string citta, string cap, DateTime dataFondazione, DateTime dataAffilizione, string codAffiliazione, bool affiliata, string email, string presidente, string referente, string sito, string tel1, string tel2, string pec, string piva, string cF, string cU, string pwd)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable idSocieta;
            string regRiuscita = "false";
            string sql;
            try
            {
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
                    sql = "";
                    sql += "INSERT INTO Societa(IDComune,NomeSocieta,Indirizzo,Citta,CAP,DataFondazione,DataAffiliazione,CodiceAffiliazione,Affiliata,Email,Presidente,Referente,Sito,Tel1,Tel2,Pec,PIVA,CF,CU) ";
                    sql += "VALUES (@IDComune,@NomeSocieta,@Indirizzo,@Citta,@CAP,@DataFondazione,@DataAffiliazione,@CodiceAffiliazione,@Affiliata,@Email,@presidente,@referente,@Sito,@Tel1,@Tel2,@Pec,@PIVA,@CF,@CU)";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("IDComune", comune);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("NomeSocieta", nomeSocieta);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("Indirizzo", indirizzo);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("Citta", citta);
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
                    if (referente != null)
                        parametro = new SqlParameter("referente", referente);
                    else
                        parametro = new SqlParameter("referente", DBNull.Value);
                    comando.Parameters.Add(parametro);
                    if (presidente != null)
                        parametro = new SqlParameter("presidente", presidente);
                    else
                        parametro = new SqlParameter("presidente", DBNull.Value);
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
                        regRiuscita = "true";
                    }
                }
                else return "Societa già presente";
            }
            catch (Exception e)
            {
                string error = e.Message;
                return error;
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
            try
            {
                sql = "";
                sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche,NumTeamQualificati,NumWildCard " +
                "FROM(((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune) " +
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
            }
            catch (Exception e)
            {
                string errore = e.Message;
            }
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
        public DataTable GetInfoSocieta(int idSocieta)//Metodo che restituisce le info di una societa
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT * FROM Societa WHERE idSocieta=@IDSocieta";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDSocieta", idSocieta));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetNumSetVinti(int idTorneo, int idPartita)
        {
            //Metodo che restituisce il numero di set vinti dalle squadre durante la partita
            SqlCommand comando;
            SqlDataAdapter adapter;
            DataTable risultato = new DataTable();
            string sql;
            try
            {
                sql = "";
                sql += "SELECT SetSQ1,SetSQ2 FROM Partita WHERE IDTorneo=@IDTorneo AND IDPartita=@IDPartita";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                comando.Parameters.Add(new SqlParameter("IDPartita", idPartita));
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(risultato);
                conn.Close();
                return risultato;
            }
            catch (Exception e)
            {
                conn.Close();
                return risultato;
            }
        }
        public int GetIdPartita(int idtorneo, int numpartita)
        {
            try
            {
                string sql = "";
                sql += "SELECT IdPartita FROM Partita WHERE IdTorneo=@IDTorneo AND NumPartita=@num";
                SqlCommand comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                comando.Parameters.Add(new SqlParameter("num", numpartita));
                SqlDataAdapter adapter = new SqlDataAdapter(comando);
                DataTable id = new DataTable();
                conn.Open();
                adapter.Fill(id);
                conn.Close();
                return Convert.ToInt32(id.Rows[0][0]);
            }
            catch { return 0; }
        }
        public string UploadResults(int idTorneo, int numPartita, int idPartita, int pt1s1, int pt2s1, int pt1s2, int pt2s2, int pt1s3, int pt2s3, int numSet, int idTorneoPrincipale)
        {
            SqlCommand comando;
            SqlParameter parametro;
            SqlDataAdapter adapter;
            DataTable puntiVittoria, titoloTorneo;
            string sql, risposta = "";
            //Metodo che aggiurna i risultati di una partita
            try
            {
                //Prendo i punti vittoria dal torneo
                sql = "";
                sql += "SELECT PuntiVittoria FROM Torneo WHERE IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                puntiVittoria = new DataTable();
                conn.Open();
                adapter.Fill(puntiVittoria);
                conn.Close();
                //Prendo il titolo del torneo per controllare se è un torneo di qualifiche o no
                sql = "";
                sql += "SELECT * FROM Torneo WHERE IDTorneo=@IDTorneo AND Titolo LIKE '%Qualifiche%'";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                titoloTorneo = new DataTable();
                conn.Open();
                adapter.Fill(titoloTorneo);
                conn.Close();
                //Update del punteggio
                sql = "" +
                "UPDATE Partita " +
                "SET PT1S1=@pt1s1,PT2S1=@pt2s1" +
                ",PT1S2=@pt1s2,PT2S2=@pt2s2" +
                ",PT1S3=@pt1s3,PT2S3=@pt2s3 " +
                ",SetSQ1=@SetSQ1,SetSQ2=@SetSQ2 " +
                ",Risultato=@ris " +
                "WHERE IDPartita=@IDPartita AND IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("pt1s1", pt1s1);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("pt1s2", pt1s2);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("pt1s3", pt1s3);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("pt2s1", pt2s1);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("pt2s2", pt2s2);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("pt2s3", pt2s3);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDPartita", idPartita);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDTorneo", idTorneo);
                comando.Parameters.Add(parametro);
                int setsq1 = Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ1"]);
                int setsq2 = Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ2"]);
                //Controllo se qualcuno ha vinto il set
                if ((((pt1s1 - pt2s1) >= 2 || (pt1s1 - pt2s1) <= -2) && (pt1s1 >= Convert.ToInt32(puntiVittoria.Rows[0]["PuntiVittoria"]) || pt2s1 >= Convert.ToInt32(puntiVittoria.Rows[0]["PuntiVittoria"]))) || (((pt1s2 - pt2s2) >= 2 || (pt1s2 - pt2s2) <= -2) && (pt1s2 >= Convert.ToInt32(puntiVittoria.Rows[0]["PuntiVittoria"]) || pt2s2 >= Convert.ToInt32(puntiVittoria.Rows[0]["PuntiVittoria"]))) || (((pt1s3 - pt2s3) >= 2 || (pt1s3 - pt2s3) <= -2) && (pt1s3 >= Convert.ToInt32(puntiVittoria.Rows[0]["PuntiVittoria"]) || pt2s3 >= Convert.ToInt32(puntiVittoria.Rows[0]["PuntiVittoria"]))))
                {
                    //Controllo a che set sono
                    switch (numSet)
                    {
                        case 1:
                            if ((pt1s1 - pt2s1) >= 2)
                                setsq1 = Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ1"]) + 1;
                            else
                                setsq2 = Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ2"]) + 1;
                            break;
                        case 2:
                            if ((pt1s2 - pt2s2) >= 2)
                                setsq1 = Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ1"]) + 1;
                            else
                                setsq2 = Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ2"]) + 1;
                            break;
                        case 3:
                            if ((pt1s3 - pt2s3) >= 2)
                                setsq1 = Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ1"]) + 1;
                            else
                                setsq2 = Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ2"]) + 1;
                            break;
                    }
                }
                else
                    risposta = "Punteggio aggiornato con successo!";
                comando.Parameters.Add(new SqlParameter("SetSQ1", setsq1));
                comando.Parameters.Add(new SqlParameter("SetSQ2", setsq2));
                comando.Parameters.Add(new SqlParameter("ris", setsq1 + "-" + setsq2));
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                risposta = "Set aggiornato con successo!";
                if (setsq1 == 2 || setsq2 == 2)//Controllo se la partita è finita
                {
                    if (titoloTorneo.Rows.Count == 1)//Se è finita una partita di qualifiche:
                        risposta = AvanzaTabelloneQualifiche(idTorneo, numPartita, idTorneoPrincipale, setsq1, setsq2);
                    else
                        AvanzaTabellone(idTorneo, numPartita, setsq1, setsq2);
                }

                //calcolo punteggi per tornei poolplay
                sql = "SELECT Formula FROM Torneo " +
                    "INNER JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula " +
                    "WHERE IDTorneo = @IDTorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                puntiVittoria = new DataTable();
                conn.Open();
                adapter.Fill(puntiVittoria);
                conn.Close();

                //prendo numero squadre qualifiche
                conn.Open();
                string query = "SELECT COUNT(*)FROM Partita  WHERE IdTorneo=@IDTorneo";//scarico numero squadre
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                DataTable dtb = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dtb);
                int numteam = Convert.ToInt32(dtb.Rows[0][0]);//numero di team
                conn.Close();
                //scarico team che hanno finito la partita delle qualifiche
                conn.Open();
                query = "SELECT COUNT(*)FROM Partita  WHERE IdTorneo=@IDTorneo AND (SetSQ1=2 OR SetSQ2=2)";//scarico numero squadre
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                int numteampartitaconclusa = Convert.ToInt32(dtb.Rows[0][0]);//numero di team
                conn.Close();
                if (numteam == numteampartitaconclusa)
                {
                    conn.Open();
                    query = "SELECT COUNT(*)FROM Partita  WHERE IdTorneo=@IDTorneo AND Fase LIKE '%Pool%'";//controllo che non ci siamo gia i pool
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    if (Convert.ToInt32(dtb.Rows[0][0]) == 0)
                    {
                        conn.Open();
                        query = "SELECT IdTorneo FROM Torneo WHERE Titolo IN (SELECT DISTINCT(SUBSTRING((SELECT DISTINCT(Titolo) FROM Torneo " +
                            "INNER JOIN Partita ON Partita.IDTorneo =@IDTorneo AND Partita.IDTorneo = Torneo.IDTorneo), 1, LEN((SELECT DISTINCT(Titolo) FROM Torneo " +
                            "INNER JOIN Partita ON Partita.IDTorneo =@IDTorneo AND Partita.IDTorneo = Torneo.IDTorneo))-CHARINDEX(' ', REVERSE((SELECT DISTINCT(Titolo) FROM Torneo " +
                            "INNER JOIN Partita ON Partita.IDTorneo =@IDTorneo AND Partita.IDTorneo = Torneo.IDTorneo))))) FROM Torneo)";//prendo id torneo collegato a quello di qualfica
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                        dtb = new DataTable();
                        da = new SqlDataAdapter(command);
                        da.Fill(dtb);
                        conn.Close();
                        CreaPool(Convert.ToInt32(dtb.Rows[0][0]));
                    }
                }
                //controllo che sia un torneo pool play
                if (puntiVittoria.Rows[0][0].ToString().Contains("Pool play"))
                {
                    calcPunteggiPool(idTorneo);
                    //prendo numero squadre
                    conn.Open();
                    query = "SELECT COUNT(*)FROM Partecipa WHERE IdTorneo=@IDTorneo";//scarico numero squadre
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    numteam = Convert.ToInt32(dtb.Rows[0][0]);//numero di team
                    conn.Close();
                    //scarico team che hanno finito la partita dei pool
                    conn.Open();
                    query = "SELECT COUNT(*)FROM Partita  WHERE IdTorneo=@IDTorneo AND Fase LIKE '%Pool%' AND (SetSQ1=2 OR SetSQ2=2)";//scarico numero squadre
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    numteampartitaconclusa = Convert.ToInt32(dtb.Rows[0][0]);//numero di team
                    conn.Close();
                    //controllo che non ci siano gia gli ottavi
                    sql = "SELECT * FROM Partita WHERE Fase='Ottavi' AND IDTorneo=@IDTorneo";
                    comando = new SqlCommand(sql, conn);
                    comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                    adapter = new SqlDataAdapter(comando);
                    DataTable dtbn = new DataTable();
                    conn.Open();
                    adapter.Fill(dtbn);
                    conn.Close();
                    if (dtbn.Rows.Count == 0)
                    {
                        //partono i pool win lose
                        if (numteampartitaconclusa == numteam / 2)
                        {
                            conn.Open();
                            query = "SELECT COUNT(*)FROM Partita  WHERE IdTorneo=@IDTorneo AND Fase LIKE '%Ottavi%'";//controllo che non ci siamo gia gli ottavi
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                            dtb = new DataTable();
                            da = new SqlDataAdapter(command);
                            da.Fill(dtb);
                            conn.Close();
                            if (Convert.ToInt32(dtb.Rows[0][0]) == 0) CreatePoolWinLose(idTorneo);

                        }
                        //partono gli ottavi e i sedisesimi
                        else if (numteampartitaconclusa == numteam)
                        {
                            conn.Open();
                            query = "SELECT COUNT(*)FROM Partita  WHERE IdTorneo=@IDTorneo AND Fase LIKE '%Ottavi%'";//controllo che non ci siamo gia i sedicesimi
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                            dtb = new DataTable();
                            da = new SqlDataAdapter(command);
                            da.Fill(dtb);
                            conn.Close();
                            if (Convert.ToInt32(dtb.Rows[0][0]) == 0) CreaOttavi(idTorneo);
                        }
                    }
                }
                return risposta;
            }
            catch (Exception e)
            {
                return "ERRORE: " + e.Message;
            }
        }
        public bool AssegnaCampo(int idPartita, int numeroCampo)
        {
            SqlCommand comando;
            SqlParameter parametro;
            string sql;
            try
            {
                sql = "";
                sql += "UPDATE Partita SET campo = @numeroCampo WHERE idPartita = @idPartita";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("numeroCampo", numeroCampo);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("idPartita", idPartita);
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
            sql += "SELECT IDPartita, " +
            "CONCAT(A1.Nome,' ',A1.cognome) as Atleta1,CONCAT(A2.Nome,' ',A2.cognome) as Atleta2,S1.NomeTeam as Team1, " +
            "CONCAT(A3.Nome,' ',A3.cognome) as Atleta3,CONCAT(A4.Nome,' ',A4.cognome) AS Atleta4, S2.NomeTeam as Team2, " +
            "CONCAT(D1.Nome,' ',D1.Cognome) as Arbitro1,CONCAT(D2.Nome,' ',D2.Cognome) as Arbitro2, " +
            "Partita.Fase,Partita.Campo, Partita.DataPartita,Convert(varchar, Partita.OraPartita, 8)as OraPartita, " +
            "Partita.PT1S1 ,Partita.PT2S1 ,Partita.PT1S2 ,Partita.PT2S2 , " +
            "Partita.PT1S3 ,Partita.PT2S3 , Partita.SetSQ1 , Partita.SetSQ2 , Risultato " +
            "FROM((((((((" +
            "Partita LEFT JOIN Squadra S1 ON Partita.idsq1 = S1.idsquadra) LEFT JOIN Squadra S2 ON Partita.idsq2 = S2.idsquadra) " +
            "LEFT JOIN Atleta A1 ON S1.IDAtleta1 = A1.IDAtleta) LEFT JOIN Atleta A2 ON S1.IDAtleta2 = A2.IDAtleta) " +
            "LEFT JOIN Atleta A3 ON S2.IDAtleta1 = A3.IDAtleta) LEFT JOIN Atleta A4 ON S2.IDAtleta2 = A4.IDAtleta) " +
            "LEFT JOIN DelegatoTecnico D1 ON Partita.idarbitro1 = D1.IDDelegato) LEFT JOIN DelegatoTecnico D2 ON Partita.idarbitro2 = D2.IDDelegato) " +
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
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche,Torneo.NumTeamQualificati,Torneo.NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
            "FROM(((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune) " +
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
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche,Torneo.NumTeamQualificati,Torneo.NumWildCard,Outdoor,Torneo.IDImpianto,RiunioneTecnica,OraInizio,Torneo.IDSocieta,Torneo.DataChiusuraIscrizioni,Torneo.IDTipoTorneo,Torneo.IDFormula,Torneo.IDSupervisore,Torneo.IDSupervisoreArbitrale,Torneo.Tour " +
            "FROM(((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune) " +
            "WHERE Torneo.IDTorneo=@IDTorneo";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", id));
            risultati[0] = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(risultati[0]);
            conn.Close();
            sql = "";
            sql += "SELECT ParametroQualita.IDParametro,NomeParametro " +
            "FROM ParametroQualita,ParametroTorneo,Torneo " +
            "WHERE Torneo.IDTorneo=@IDTorneo AND ParametroTorneo.IDTorneo = Torneo.IDTorneo AND ParametroTorneo.IDParametro = ParametroQualita.IDParametro";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", id));
            risultati[1] = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(risultati[1]);
            conn.Close();
            sql = "";
            sql += "SELECT Impianto.IDImpianto,NomeImpianto,Impianto.Citta ";
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
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche,Torneo.NumTeamQualificati,NumWildCard " +
            "FROM(((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune) " +
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
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche,Torneo.NumTeamQualificati,NumWildCard " +
            "FROM(((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune) " +
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
            "SELECT idatleta, cognome, nome, sum(punti+Atleta.PuntiBase) AS Punteggi " +
            "FROM punteggi, atleta WHERE idatleta=idAtl AND atleta.sesso=@Sesso " +
            "GROUP BY idatleta,cognome,nome HAVING sum(punti+Atleta.PuntiBase)>0 ORDER BY sum(punti+Atleta.PuntiBase) DESC,Cognome,Nome";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Sesso", sesso));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetClassificaTour(string sesso,string tour)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "WITH punteggi(idAtl, punti) AS( " +
            "SELECT idatleta1, sum(punti) / 2.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo AND Torneo.Tour=@tour AND Titolo NOT LIKE '%Qualifiche%' " +
            "GROUP BY idatleta1 " +
            "UNION " +
            "SELECT idatleta2, sum(punti) / 2.0 FROM Partecipa, Squadra, Torneo WHERE " +
            "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo AND Torneo.Tour=@tour AND Titolo NOT LIKE '%Qualifiche%' " +
            "GROUP BY idatleta2 " +
            ") " +
            "SELECT idatleta, cognome, nome, sum(punti) AS Punteggi " +
            "FROM punteggi, atleta WHERE idatleta=idAtl AND atleta.sesso=@Sesso " +
            "GROUP BY idatleta,cognome,nome HAVING sum(punti)>0 ORDER BY sum(punti) DESC,Cognome,Nome";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Sesso", sesso));
            comando.Parameters.Add(new SqlParameter("tour", tour));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetPuntiAtletaTappa (string tourTorneo,int idAtleta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT Torneo.IDTorneo,Impianto.Citta, Torneo.Gender,Torneo.Datainizio,(Partecipa.Punti/2) as Punti FROM Torneo " +
                "INNER JOIN Impianto ON Torneo.IDImpianto = Impianto.IDImpianto " +
                "INNER JOIN Squadra ON Squadra.IDAtleta1 = @IDAtleta OR Squadra.IDAtleta2 = @IDAtleta " +
                "INNER JOIN Partecipa ON Partecipa.IDTorneo = Torneo.IDTorneo AND Squadra.IDSquadra = Partecipa.IDSquadra " +
                "WHERE Torneo.Tour = @Tour AND Titolo NOT LIKE '%Qualifiche%' ORDER BY DataInizio";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Tour", tourTorneo));
            comando.Parameters.Add(new SqlParameter("IDAtleta", idAtleta));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public string[] GetTappe(string tour)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT CONCAT (Impianto.Citta,' ',FORMAT( Torneo.Datainizio, 'dd/MM/yyyy', 'en-US' ))as Tappa FROM Torneo " +
                "INNER JOIN Impianto ON Torneo.IDImpianto = Impianto.IDImpianto " +
                "WHERE Torneo.Tour = @Tour AND Titolo NOT LIKE '%Qualifiche%'";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Tour", tour));
            query = new DataTable();
            adapter = new SqlDataAdapter(comando);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query.Rows.OfType<DataRow>().Select(k => k[0].ToString()).ToArray();
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
            "LEFT JOIN Atleta A1 ON S1.IDAtleta1 = A1.IDAtleta) LEFT JOIN Atleta A2 ON S1.IDAtleta2 = A2.IDAtleta) " +
            "LEFT JOIN Atleta A3 ON S2.IDAtleta1 = A3.IDAtleta) LEFT JOIN Atleta A4 ON S2.IDAtleta2 = A4.IDAtleta) " +
            "LEFT JOIN DelegatoTecnico D1 ON Partita.idarbitro1 = D1.IDDelegato) LEFT JOIN DelegatoTecnico D2 ON Partita.idarbitro2 = D2.IDDelegato) " +
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
        public bool CreaTorneo(string titolo, string Tour, int puntiVittoria, double montepremi, DateTime dataChiusuraIscrizioni, DateTime dataInizio, DateTime dataFine, char genere, string formulaTorneo, int NumMaxTeamMainDraw, int NumMaxTeamQualifiche, string[] parametriTorneo, string tipoTorneo, double quotaIscrizione, int idSocieta, int numTeamQualificati, int numWildCard, int idImpianto, bool outdoor, bool riunioneTecnica, string oraInizio)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable query;
            string sql;
            DataTable idTorneo;
            List<int> idParametriTorneo = new List<int>();
            List<int> idImpianti = new List<int>();
            try
            {
                //Creo il torneo
                sql = "";
                sql += "INSERT INTO Torneo(IDSocieta,IDTipoTorneo,IDFormula,Titolo,PuntiVittoria,Montepremi,DataChiusuraIscrizioni,DataInizio,DataFine,Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,QuotaIscrizione,NumTeamQualificati,NumWildCard,IDImpianto,Outdoor,RiunioneTecnica,OraInizio,Tour) ";
                sql += "VALUES(@IDSocieta,@IDTipoTorneo,@IDFormula,@Titolo,@PuntiVittoria,@Montepremi,@DataChiusuraIScrizioni,@DataInzio,@DataFine,@Gender,@NumMaxTeamMainDraw,@NumMaxTeamQualifiche,@QuotaIscrizione,@NumTeamQualificati,@NumWildCard,@IDImpianto,@Outdoor,@RiunioneTecnica,@OraInizio,@Tour)";
                comando = new SqlCommand(sql, conn);
                parametro = new SqlParameter("IDSocieta", idSocieta);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDTipoTorneo", tipoTorneo);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDFormula", formulaTorneo);
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
                parametro = new SqlParameter("NumTeamQualificati", numTeamQualificati);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("NumWildCard", numWildCard);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("IDImpianto", idImpianto);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("Outdoor", outdoor);
                comando.Parameters.Add(parametro);
                parametro = new SqlParameter("RiunioneTecnica", riunioneTecnica);
                comando.Parameters.Add(parametro);
                if (oraInizio != null)
                    parametro = new SqlParameter("OraInizio", oraInizio);
                else
                    parametro = new SqlParameter("OraInizio", DBNull.Value);
                comando.Parameters.Add(parametro);
                if (Tour != null)
                    parametro = new SqlParameter("Tour", Tour);
                else
                    parametro = new SqlParameter("Tour", DBNull.Value);
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
                sql += "SELECT IDTorneo FROM Torneo WHERE Titolo=@titolotorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("titolotorneo", titolo));
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
                        parametro = new SqlParameter("IDTorneo", idTorneo.Rows[0][0]);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("IDParametro", idParametriTorneo[i]);
                        comando.Parameters.Add(parametro);
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
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
                    "<a href=\"http://80.211.0.174/app/login\"> Cambia Password</a> ";
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("recoverypass.aibvc@gmail.com", "hwwzapfnfxhkkxzf");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        } //invia l'email per recuperare la password
        public bool InsertSquadra(string NomeAtleta1, string NomeAtleta2, string NomeTeam,int IDAllenatore,string IDTorneo,bool wildcard)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT IDSquadra FROM Squadra WHERE (IDAtleta1=@IDAtleta1 AND IDAtleta2=@IDAtleta2) OR (IDAtleta1=@IDAtleta1 AND IDAtleta2=@IDAtleta2)";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDAtleta1", NomeAtleta1));
                comando.Parameters.Add(new SqlParameter("IDAtleta2", NomeAtleta2));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                //controllo che non ci sia gia una squadra
                if (query.Rows.Count > 0)
                {
                    if (IscriviSquadra(IDTorneo, Convert.ToInt32(query.Rows[0]["IDSquadra"]), NomeAtleta1, NomeAtleta2, IDAllenatore, wildcard)) return true;
                    else return false;
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
                        parametro = new SqlParameter("IDAtleta1", NomeAtleta1);
                        comando.Parameters.Add(parametro);
                        parametro = new SqlParameter("IDAtleta2", NomeAtleta2);
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
                        comando.Parameters.Add(new SqlParameter("IDAtleta1", NomeAtleta1));
                        comando.Parameters.Add(new SqlParameter("IDAtleta2", NomeAtleta2));
                        query = new DataTable();
                        adapter = new SqlDataAdapter(comando);
                        conn.Open();
                        adapter.Fill(query);
                        conn.Close();
                        if (query.Rows.Count > 0)
                        {
                            if (IscriviSquadra(IDTorneo, Convert.ToInt32(query.Rows[0]["IDSquadra"]), NomeAtleta1, NomeAtleta2, IDAllenatore, wildcard)) return true;
                            else return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
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
        public bool IscriviSquadra(string idTorneo, int idSquadra,string idatleta1, string idatleta2, int idAllenatore,bool wildcard)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable query, numMaxQualifiche,punti;
            string sql;
            int[] punteggio = new int[2];
            try
            {
                sql = "";
                sql += "SELECT IDAtleta1, IDAtleta2 FROM Squadra WHERE IDSquadra=@IDSquadra";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDSquadra", idSquadra));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                //se ha trovato una squdra con quel id
                if (query.Rows.Count > 0)
                {
                    //prende il sesso degli atleti
                    try
                    {
                        //Controllo che il torneo non sia gia pieno con i team in qualifica
                        //Prendo NumMaxTeamQualifiche
                        sql = "";
                        sql += "SELECT sum(NumMaxTeamQualifiche+NumMaxTeamMainDraw)as NumMax,(SELECT COUNT(*) FROM ListaIscritti WHERE IDTorneo=@IDTorneo) FROM Torneo WHERE IDTorneo=@IDTorneo";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                        adapter = new SqlDataAdapter(comando);
                        numMaxQualifiche = new DataTable();
                        conn.Open();
                        adapter.Fill(numMaxQualifiche);
                        conn.Close();
                        if (Convert.ToInt32(numMaxQualifiche.Rows[0][1]) < Convert.ToInt32(numMaxQualifiche.Rows[0][0]))
                        {
                            //prendo punti atleta 1
                            sql = "WITH punteggi(punti) AS( " +
                                "SELECT sum(punti) / 2.0 FROM Partecipa, Squadra, Torneo WHERE " +
                                "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo AND Squadra.IDAtleta1 = @at1 " +
                                "AND datediff(day, datafine, GETDATE())< 120 GROUP BY idatleta1 " +
                                "UNION " +
                                "SELECT sum(punti) / 2.0 FROM Partecipa, Squadra, Torneo WHERE " +
                                "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo AND Squadra.IDAtleta2 = @at1 " +
                                "AND datediff(day, datafine, GETDATE())< 120 GROUP BY idatleta2 " +
                                "UNION " +
                                "SELECT sum(punti) / 4.0 FROM Partecipa, Squadra, Torneo WHERE " +
                                "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo AND Squadra.IDAtleta1 = @at1 " +
                                "AND datediff(day, datafine, GETDATE()) BETWEEN 121 AND 365 GROUP BY idatleta1 " +
                                "UNION " +
                                "SELECT sum(punti) / 4.0 FROM Partecipa, Squadra, Torneo WHERE " +
                                "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo AND Squadra.IDAtleta2 = @at1 " +
                                "AND datediff(day, datafine, GETDATE()) BETWEEN 121 AND 365 GROUP BY idatleta2) " +
                                "SELECT TOP(10) sum(punti+Atleta.PuntiBase) FROM Punteggi, Atleta " +
                                "WHERE Atleta.IDAtleta=@at1";
                            comando = new SqlCommand(sql, conn);
                            parametro = new SqlParameter("at1", idatleta1);
                            comando.Parameters.Add(parametro);
                            adapter = new SqlDataAdapter(comando);
                            punti = new DataTable();
                            conn.Open();
                            adapter.Fill(punti);
                            conn.Close();
                            if (punti.Rows[0][0] == DBNull.Value)
                            {
                                sql = "SELECT Atleta.PuntiBase FROM Atleta WHERE Atleta.IDAtleta=@at2";
                                comando = new SqlCommand(sql, conn);
                                parametro = new SqlParameter("at2", idatleta2);
                                comando.Parameters.Add(parametro);
                                adapter = new SqlDataAdapter(comando);
                                punti = new DataTable();
                                conn.Open();
                                adapter.Fill(punti);
                                conn.Close();
                            }
                            punteggio[0] = Convert.ToInt32(punti.Rows[0][0]);
                            //prendo punti atleta 2
                            sql = "WITH punteggi(punti) AS( " +
                                "SELECT sum(punti) / 2.0 FROM Partecipa, Squadra, Torneo WHERE " +
                                "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo AND Squadra.IDAtleta1 = @at2 " +
                                "AND datediff(day, datafine, GETDATE())< 120 GROUP BY idatleta1 " +
                                "UNION " +
                                "SELECT sum(punti) / 2.0 FROM Partecipa, Squadra, Torneo WHERE " +
                                "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo AND Squadra.IDAtleta2 = @at2 " +
                                "AND datediff(day, datafine, GETDATE())< 120 GROUP BY idatleta2 " +
                                "UNION " +
                                "SELECT sum(punti) / 4.0 FROM Partecipa, Squadra, Torneo WHERE " +
                                "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo AND Squadra.IDAtleta1 = @at2 " +
                                "AND datediff(day, datafine, GETDATE()) BETWEEN 121 AND 365 GROUP BY idatleta1 " +
                                "UNION " +
                                "SELECT sum(punti) / 4.0 FROM Partecipa, Squadra, Torneo WHERE " +
                                "Partecipa.idsquadra = Squadra.idsquadra AND Partecipa.idtorneo = Torneo.idtorneo AND Squadra.IDAtleta2 = @at2 " +
                                "AND datediff(day, datafine, GETDATE()) BETWEEN 121 AND 365 GROUP BY idatleta2) " +
                                "SELECT TOP(10) sum(punti+Atleta.PuntiBase) FROM Punteggi, Atleta " +
                                "WHERE Atleta.IDAtleta=@at2";
                            comando = new SqlCommand(sql, conn);
                            parametro = new SqlParameter("at2", idatleta2);
                            comando.Parameters.Add(parametro);
                            adapter = new SqlDataAdapter(comando);
                            punti = new DataTable();
                            conn.Open();
                            adapter.Fill(punti);
                            conn.Close();
                            if (punti.Rows[0][0] == DBNull.Value)
                            {
                                sql = "SELECT Atleta.PuntiBase FROM Atleta WHERE Atleta.IDAtleta=@at2";
                                comando = new SqlCommand(sql, conn);
                                parametro = new SqlParameter("at2", idatleta2);
                                comando.Parameters.Add(parametro);
                                adapter = new SqlDataAdapter(comando);
                                punti = new DataTable();
                                conn.Open();
                                adapter.Fill(punti);
                                conn.Close();
                            }
                            punteggio[1] = Convert.ToInt32(punti.Rows[0][0]);
                            //trasformo il codice della tessera dell'allenatore ricevuto con id 
                            sql = "";
                            sql += "INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,Cancellata,WC,EntryPoints)VALUES (@IDSquadra,@IDTorneo,@IDAllenatore,@DataIscrizione,@Cancellata,@WildCard,@punti)";
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
                            if (wildcard == true)
                                parametro = new SqlParameter("WildCard", wildcard);
                            else
                                parametro = new SqlParameter("WildCard", false);
                            comando.Parameters.Add(parametro);
                            parametro = new SqlParameter("punti", (punteggio[0]+punteggio[1]));
                            comando.Parameters.Add(parametro);
                            conn.Open();
                            comando.ExecuteNonQuery();
                            conn.Close();
                            return true;
                        }
                        else return false;
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
                "LEFT JOIN Atleta A1 ON S1.IDAtleta1 = A1.IDAtleta) LEFT JOIN Atleta A2 ON S1.IDAtleta2 = A2.IDAtleta) " +
                "LEFT JOIN Atleta A3 ON S2.IDAtleta1 = A3.IDAtleta) LEFT JOIN Atleta A4 ON S2.IDAtleta2 = A4.IDAtleta) " +
                "LEFT JOIN DelegatoTecnico D1 ON Partita.idarbitro1 = D1.IDDelegato) LEFT JOIN DelegatoTecnico D2 ON Partita.idarbitro2 = D2.IDDelegato) ";
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
                sql += "SELECT * FROM TipoTorneo";
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
        public DataTable GetTitoloTorneo(int idtoreo)
        {
            SqlDataAdapter adapter;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT Titolo FROM Torneo WHERE IDTorneo=@idtorneo";
                query = new DataTable();
                SqlCommand comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("idtorneo", idtoreo));
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
        }
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
        public bool ControlAutorizzazioneTorneo(int idTorneo)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            try
            {
                sql = "";
                sql += "SELECT * FROM Torneo WHERE IDTorneo=@idtorneo AND Autorizzato=0 AND Annullato=0";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("idtorneo", idTorneo));
                query = new DataTable();
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                if (query.Rows.Count > 0) return true;
                return false;
            }
            catch
            {
                return false;
            }
        }//ritorna se un torneo è autorizzato o no
        public string AutorizzaTorneo(int idTorneo, bool autorizza)
        {
            SqlCommand comando;
            SqlParameter parametro;
            string sql;
            try
            {
                if (autorizza)
                {
                    sql = "";
                    sql += "UPDATE Torneo SET Autorizzato=1 WHERE IdTorneo=@idTorneo";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("idTorneo", idTorneo);
                    comando.Parameters.Add(parametro);
                    conn.Open();
                    comando.ExecuteNonQuery();
                    conn.Close();
                    return "Torneo autorizzato";
                }
                else
                {
                    sql = "";
                    sql += "UPDATE Torneo SET Annullato=1 WHERE IdTorneo=@idTorneo";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("idTorneo", idTorneo);
                    comando.Parameters.Add(parametro);
                    conn.Open();
                    comando.ExecuteNonQuery();
                    conn.Close();
                    return "Torneo non autorizzato";
                }
            }
            catch (Exception e)
            {
                return "Errore: " + e.Message;
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
        public DataTable GetAtleti()
        {
            string sql = "";
            sql += "SELECT * FROM Atleta;";
            DataTable query = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetAllenatori()
        {
            string sql = "";
            sql += "SELECT * FROM Allenatore";
            DataTable query = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetAtletiSocieta(int idsocieta)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "SELECT Atleta.IDAtleta,Atleta.IDSocieta,Atleta.CodiceTessera,Atleta.Nome,Atleta.Cognome,Atleta.Sesso,Atleta.CF,Atleta.DataNascita,Comune.Citta as ComuneNascita,Comune.Citta as ComuneResidenza,Atleta.Indirizzo,Atleta.CAP,Atleta.Email,Atleta.Tel,Atleta.Altezza,Atleta.Peso,Atleta.DataScadenzaCertificato " +
            "FROM(Atleta LEFT JOIN Comune ON Atleta.IDComuneNascita = Comune.IDComune AND Atleta.IDComuneResidenza = Comune.IDComune) " +
            "WHERE Atleta.IDSocieta = @IDSocieta";
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
            sql += "SELECT Allenatore.IDAllenatore,Allenatore.CodiceTessera,Allenatore.Nome,Allenatore.Cognome,Allenatore.Sesso,Allenatore.CF,Allenatore.DataNascita,Comune.Citta as ComuneNascita,Comune.Citta as ComuneResidenza,Allenatore.Indirizzo,Allenatore.CAP,Allenatore.Email,Allenatore.Tel " +
                "FROM(Allenatore LEFT JOIN Comune ON Allenatore.IDComuneNascita = Comune.IDComune AND Allenatore.IDComuneResidenza = Comune.IDComune) " +
                "WHERE Allenatore.IDSocieta = @IDSocieta";
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
            SqlDataAdapter adapter;
            DataTable numMaxWC, numWCPresenti;
            string sql;
            try
            {
                //Prendo il numero massimo di wildcard che il torneo può dare
                sql = "";
                sql += "SELECT NumWildCard FROM Torneo WHERE IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                numMaxWC = new DataTable();
                conn.Open();
                adapter.Fill(numMaxWC);
                conn.Close();
                //Conto quante wildcard già ci sono
                sql = "";
                sql += "SELECT COUNT(*) FROM ListaIscritti WHERE IDTorneo=@IDTorneo AND WC=1";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                numWCPresenti = new DataTable();
                conn.Open();
                adapter.Fill(numWCPresenti);
                conn.Close();
                //Controllo che i posti per le wildcard non siano già finiti
                if (Convert.ToInt32(numWCPresenti.Rows[0][0]) < Convert.ToInt32(numMaxWC.Rows[0][0]))
                {
                    //Assegno la wildcard
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
                else return false;
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
                sql += "SELECT DISTINCT Torneo.IDTorneo, Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome, ' ', Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome, ' ', SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale, CONCAT(DirettoreCompetizione.Nome, ' ', DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche,Torneo.NumTeamQualificati,Torneo.NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
                "FROM(((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune) " +
                " WHERE Autorizzato= 1 AND Torneo.IDTorneo= @idtorneo" +
                " AND CAST(DataFine as DATE) < GETDATE()";
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
                comando.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date.ToString("yyyy-MM-dd")));
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
        public string EliminaSquadraBySupervisore(int idTorneo, int idSquadra, int idsupervisore)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            try
            {
                //controllo che la data di iscrizione sia ancora aperta
                sql = "";
                sql = "SELECT IDTorneo FROM Torneo WHERE IDTorneo=@idTorneo AND IDSupervisore=@idSupervisore AND CAST(DataInizio as DATE) >= @Data";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("idTorneo", idTorneo));
                comando.Parameters.Add(new SqlParameter("idSupervisore", idsupervisore));
                comando.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date.ToString("yyyy-MM-dd")));
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
                else return "Il torneo è gia iniziato.";
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

        public DataTable GetAllImpiantiSocieta(int idSocieta)
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
        public DataTable GetImpiantoSocieta(int idSocieta, int idImpianto)
        {
            //Metodo che restituisce gli impianti di una società
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            string sql;
            sql = "";
            sql += "Select Impianto.NomeImpianto,Impianto.Citta as Città,numerocampi,cap,Impianto.Descrizione,Impianto.Indirizzo,Impianto.Email,Impianto.Sito,Impianto.Tel " +
                   "From Impianto, ImpiantoSocieta " +
                   "Where ImpiantoSocieta.IDSocieta = @IDSocieta AND ImpiantoSocieta.IDImpianto = @IDImpianto AND Impianto.IDImpianto = ImpiantoSocieta.IDImpianto ";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDSocieta", idSocieta));
            comando.Parameters.Add(new SqlParameter("IDImpianto", idImpianto));
            adapter = new SqlDataAdapter(comando);
            query = new DataTable();
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public bool CreaLista(int idTorneo)
        {
            SqlCommand comando;
            SqlDataAdapter adapter;
            DataTable query, numTabelloneWildCard; //DataTable per raccogliere il numero di team del tabellone e il numero di wildcard
            string sql;
            try
            {
                //Prendo le squadre con la wildcard
                sql = "";
                sql += "SELECT * FROM ListaIscritti WHERE IDTorneo=@IDTorneo AND WC=1 ORDER BY EntryPoints";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                query = new DataTable();
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                //Inserisco i team con la WC nel tabellone
                if (query.Rows.Count > 0)
                {
                    for (int i = 0; i < query.Rows.Count; i++)
                    {
                        //Inserisco le squadre nella tabella Partecipa
                        sql = "";
                        sql += "INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints) ";
                        sql += "VALUES (@IDSquadra,@IDTorneo,@IDAllenatore,@EntryPoints)";
                        comando = new SqlCommand(sql, conn);
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
                    }
                }
                //Prendo NumMaxTeamMainDraw del torneo
                sql = "";
                sql += "SELECT NumMaxTeamMainDraw,NumWildCard,NumTeamQualificati FROM Torneo WHERE IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                numTabelloneWildCard = new DataTable();
                conn.Open();
                adapter.Fill(numTabelloneWildCard);
                conn.Close();
                //Prendo le squadre che vanno direttamente nel tabellone
                sql = "";
                sql += "SELECT TOP (@NumeroTeamTabellone) * FROM ListaIscritti WHERE IDTorneo=@IDTorneo AND WC=0 ORDER BY EntryPoints DESC";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                comando.Parameters.Add(new SqlParameter("NumeroTeamTabellone", Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumMaxTeamMainDraw"]) - Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumWildCard"]) - Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumTeamQualificati"])));
                adapter = new SqlDataAdapter(comando);
                query = new DataTable();
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                if (query.Rows.Count > 0)
                {
                    for (int i = 0; i < query.Rows.Count; i++)
                    {
                        //Inserisco le squadre nella tabella Partecipa
                        sql = "";
                        sql += "INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints) ";
                        sql += "VALUES (@IDSquadra,@IDTorneo,@IDAllenatore,@EntryPoints)";
                        comando = new SqlCommand(sql, conn);
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
                    }
                }
                //controllo se mancano squadre e metto le bye
                if (query.Rows.Count < Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumMaxTeamMainDraw"]) && Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumTeamQualificati"]) == 0)
                {
                    for (int i = 0; i < (Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumMaxTeamMainDraw"]) - query.Rows.Count); i++)
                    {
                        //Inserisco le squadre nella tabella Partecipa
                        sql = "";
                        sql += "INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints) ";
                        sql += "VALUES (@IDSquadra,@IDTorneo,@IDAllenatore,@EntryPoints)";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDSquadra", i + 1));
                        comando.Parameters.Add(new SqlParameter("IDTorneo", query.Rows[i]["IDTorneo"]));
                        if (query.Rows[i]["IDAllenatore"] != null)
                            comando.Parameters.Add(new SqlParameter("IDAllenatore", query.Rows[i]["IDAllenatore"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDAllenatore", DBNull.Value));
                        comando.Parameters.Add(new SqlParameter("EntryPoints", query.Rows[i]["EntryPoints"]));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        //Metodo che restituisce la lista d'ingresso
        public DataTable GetListaIngresso(int idTorneo)
        {
            SqlCommand comando;
            SqlDataAdapter adapter;
            DataTable query = new DataTable();
            string sql;
            try
            {
                sql = "";
                sql = "SELECT Squadra.NomeTeam,CONCAT(Allenatore.Nome,'',Allenatore.Cognome) AS Allenatore,Partecipa.EntryPoints,Partecipa.PosizioneFinale,Partecipa.Punti,Partecipa.Montepremi " +
                     "FROM((Partecipa LEFT JOIN Allenatore ON Partecipa.IDAllenatore = Allenatore.IDAllenatore)LEFT JOIN Squadra ON Partecipa.IDSquadra = Squadra.IDSquadra) " +
                     "WHERE Partecipa.IDTorneo = @IDTorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                return query;
            }
            catch (Exception e)
            {
                conn.Close();
                return query;
            }
        }
        //Metodo che crea il torneo di qualifica
        public bool CreaTorneoQualifica(int idTorneo, DateTime dataInizioQualifiche, DateTime dataFineQualifiche, DateTime dataPartite2Turno)
        {
            SqlCommand comando;
            SqlDataAdapter adapter;
            SqlParameter parametro;
            DataTable idTorneoQualifica, numTabelloneWildCard, squadreQualifica; //DataTable per raccogliere il numero di team del tabellone e il numero di wildcard
            DataTable[] torneoPrincipale;
            string sql;
            try
            {
                //Prendo NumMaxTeamMainDraw,NumWildCard,NumMaxTeamQualifiche e NumTeamQualificati del torneo
                sql = "";
                sql += "SELECT NumMaxTeamMainDraw,NumWildCard,NumMaxTeamQualifiche,NumTeamQualificati FROM Torneo WHERE IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                numTabelloneWildCard = new DataTable();
                conn.Open();
                adapter.Fill(numTabelloneWildCard);
                conn.Close();
                //Prendo le informazioni del torneo
                torneoPrincipale = new DataTable[3];
                torneoPrincipale = GetTorneoByID(idTorneo);
                //Creo il torneo di qualifica
                if (torneoPrincipale[0].Rows.Count == 1)//Controllo che abbia trovato il torneo
                {
                    sql = "";
                    sql += "INSERT INTO Torneo(IDSocieta,IDTipoTorneo,IDFormula,IDSupervisore,Titolo,PuntiVittoria,Montepremi,DataChiusuraIscrizioni,DataInizio,DataFine,Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,QuotaIscrizione,NumTeamQualificati,NumWildCard,Autorizzato,IDImpianto,Outdoor,RiunioneTecnica,OraInizio,IDSupervisoreArbitrale,Tour) ";
                    sql += "VALUES(@IDSocieta,@IDTipoTorneo,@IDFormula,@IDSupervisore,@Titolo,@PuntiVittoria,@Montepremi,@DataChiusuraIscrizioni,@DataInzio,@DataFine,@Gender,@NumMaxTeamMainDraw,@NumMaxTeamQualifiche,@QuotaIscrizione,@NumTeamQualificati,@NumWildCard,1,@IDImpianto,@Outdoor,@RiunioneTecnica,@OraInizio,@IDSupervisoreArbitrale,@Tour)";
                    comando = new SqlCommand(sql, conn);
                    parametro = new SqlParameter("IDSocieta", torneoPrincipale[0].Rows[0]["IDSocieta"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDTipoTorneo", torneoPrincipale[0].Rows[0]["IDTipoTorneo"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDFormula", torneoPrincipale[0].Rows[0]["IDFormula"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("Titolo", torneoPrincipale[0].Rows[0]["Titolo"] += " Qualifiche");
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("PuntiVittoria", torneoPrincipale[0].Rows[0]["PuntiVittoria"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("Montepremi", torneoPrincipale[0].Rows[0]["Montepremi"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataChiusuraIscrizioni", Convert.ToDateTime(torneoPrincipale[0].Rows[0]["DataChiusuraIscrizioni"]).Date);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataInzio", dataInizioQualifiche.Date);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("DataFine", dataFineQualifiche.Date);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("Gender", torneoPrincipale[0].Rows[0]["Gender"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("NumMaxTeamMainDraw", torneoPrincipale[0].Rows[0]["NumMaxTeamMainDraw"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("NumMaxTeamQualifiche", torneoPrincipale[0].Rows[0]["NumMaxTeamQualifiche"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("QuotaIscrizione", torneoPrincipale[0].Rows[0]["QuotaIscrizione"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("NumTeamQualificati", torneoPrincipale[0].Rows[0]["NumTeamQualificati"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("NumWildCard", torneoPrincipale[0].Rows[0]["NumWildCard"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDImpianto", torneoPrincipale[0].Rows[0]["IDImpianto"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("Outdoor", torneoPrincipale[0].Rows[0]["Outdoor"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("RiunioneTecnica", torneoPrincipale[0].Rows[0]["RiunioneTecnica"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("OraInizio", torneoPrincipale[0].Rows[0]["OraInizio"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDSupervisore", torneoPrincipale[0].Rows[0]["IDSupervisore"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("IDSupervisoreArbitrale", torneoPrincipale[0].Rows[0]["IDSupervisoreArbitrale"]);
                    comando.Parameters.Add(parametro);
                    parametro = new SqlParameter("Tour", torneoPrincipale[0].Rows[0]["Tour"]);
                    comando.Parameters.Add(parametro);
                    conn.Open();
                    comando.ExecuteNonQuery();
                    conn.Close();
                    //Prendo l'id del torneo di qualifica
                    sql = "";
                    sql += "SELECT IDTorneo FROM Torneo WHERE Titolo=@Titolo";
                    comando = new SqlCommand(sql, conn);
                    comando.Parameters.Add(new SqlParameter("Titolo", torneoPrincipale[0].Rows[0]["Titolo"]));
                    adapter = new SqlDataAdapter(comando);
                    idTorneoQualifica = new DataTable();
                    conn.Open();
                    adapter.Fill(idTorneoQualifica);
                    conn.Close();
                    if (torneoPrincipale[1].Rows.Count > 0)//Prendo i parametri del torneo
                    {
                        for (int i = 0; i < torneoPrincipale[1].Rows.Count; i++)
                        {
                            sql = "";
                            sql += "INSERT INTO ParametroTorneo(IDTorneo,IDParametro) VALUES(@IDTorneo,@IDParametro)";
                            comando = new SqlCommand(sql, conn);
                            parametro = new SqlParameter("IDTorneo", idTorneoQualifica.Rows[0]["IDTorneo"]);
                            comando.Parameters.Add(parametro);
                            parametro = new SqlParameter("IDParametro", torneoPrincipale[1].Rows[i]["IDParametro"]);
                            comando.Parameters.Add(parametro);
                            conn.Open();
                            comando.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                    //Seleziono le squadre che dovranno giocare le qualifiche
                    sql = "";
                    sql += "SELECT TOP (@NumMaxTeamQualifiche) * FROM ListaIscritti WHERE IDTorneo=@idTorneo AND IDSquadra NOT IN (SELECT IDSquadra FROM Partecipa WHERE IDTorneo=@idTorneo) ORDER BY EntryPoints DESC";
                    comando = new SqlCommand(sql, conn);
                    comando.Parameters.Add(new SqlParameter("NumMaxTeamQualifiche", numTabelloneWildCard.Rows[0]["NumMaxTeamQualifiche"]));
                    comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                    adapter = new SqlDataAdapter(comando);
                    squadreQualifica = new DataTable();
                    conn.Open();
                    adapter.Fill(squadreQualifica);
                    conn.Close();
                    //Collego le squadre al torneo di qualifica
                    if (squadreQualifica.Rows.Count > 0)
                    {
                        for (int i = 0; i < squadreQualifica.Rows.Count; i++)
                        {
                            sql = "";
                            sql += "INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints) ";
                            sql += "VALUES (@IDSquadra,@IDTorneo,@IDAllenatore,@EntryPoints)";
                            comando = new SqlCommand(sql, conn);
                            comando.Parameters.Add(new SqlParameter("IDSquadra", squadreQualifica.Rows[i]["IDSquadra"]));
                            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica.Rows[0][0]));
                            if (squadreQualifica.Rows[i]["IDAllenatore"] != null)
                                comando.Parameters.Add(new SqlParameter("IDAllenatore", squadreQualifica.Rows[i]["IDAllenatore"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDAllenatore", DBNull.Value));
                            comando.Parameters.Add(new SqlParameter("EntryPoints", squadreQualifica.Rows[i]["EntryPoints"]));
                            conn.Open();
                            comando.ExecuteNonQuery();
                            conn.Close();
                        }
                        //Creo le partite di qualifica in base al numero di team che si qualificano al torneo
                        switch (Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumTeamQualificati"]))
                        {
                            case 8:
                                PartiteNTQ4_8(squadreQualifica.Rows.Count, squadreQualifica, Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumTeamQualificati"]), Convert.ToInt32(idTorneoQualifica.Rows[0]["IDTorneo"]), dataPartite2Turno, dataInizioQualifiche);
                                break;
                            case 4:
                                PartiteNTQ4_8(squadreQualifica.Rows.Count, squadreQualifica, Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumTeamQualificati"]), Convert.ToInt32(idTorneoQualifica.Rows[0]["IDTorneo"]), dataPartite2Turno, dataInizioQualifiche);
                                break;
                            case 6:
                                PartiteNTQ6(squadreQualifica.Rows.Count, squadreQualifica, Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumTeamQualificati"]), Convert.ToInt32(idTorneoQualifica.Rows[0]["IDTorneo"]), dataPartite2Turno, dataInizioQualifiche);
                                break;
                            case 1:
                                PartiteNTQ1(squadreQualifica.Rows.Count, squadreQualifica, Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumTeamQualificati"]), Convert.ToInt32(idTorneoQualifica.Rows[0]["IDTorneo"]), dataPartite2Turno, dataInizioQualifiche);
                                break;
                        }
                    }
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        //Metodo che crea le partite per il torneo di qualifica con 4 o 8 NTQ (NumTeamQualificati) 
        private void PartiteNTQ4_8(int numSquadreQualifiche, DataTable squadreQualifica, int numQualificati, int idTorneoQualifica, DateTime dataPartite2Turno, DateTime dataInizioQualifiche)
        {
            SqlCommand comando;
            SqlDataAdapter adapter;
            DataTable squadreBye;
            string sql;
            try
            {
                //Prendo le squadre bye
                sql = "";
                sql += "SELECT * FROM Squadra WHERE NomeTeam='Bye'";
                adapter = new SqlDataAdapter(sql, conn);
                squadreBye = new DataTable();
                conn.Open();
                adapter.Fill(squadreBye);
                conn.Close();
                //Partite
                if (numSquadreQualifiche <= 4)
                {
                    for (int i = 0; i < numSquadreQualifiche; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints) " +
                            "VALUES (@IDSquadra,@IDTorneo,@IDAllenatore,@EntryPoints)";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDSquadra", squadreQualifica.Rows[i]["IDSquadra"]));
                        comando.Parameters.Add(new SqlParameter("IDTorneo", squadreQualifica.Rows[i]["IDTorneo"]));
                        if (squadreQualifica.Rows[i]["IDAllenatore"] != DBNull.Value)
                            comando.Parameters.Add(new SqlParameter("IDAllenatore", squadreQualifica.Rows[i]["IDAllenatore"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDAllenatore", DBNull.Value));
                        comando.Parameters.Add(new SqlParameter("EntryPoints", squadreQualifica.Rows[i]["EntryPoints"]));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                else if (numSquadreQualifiche <= 8)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDSQ1,IDSQ2,IDTorneo,NumPartita,Fase,DataPartita,OraPartita) " +
                            "VALUES (@IDSQ1,@IDSQ2,@IDTorneo,@NumPartita,@Fase,@DataPartita,@OraPartita)";
                        comando = new SqlCommand(sql, conn);
                        if (i < squadreQualifica.Rows.Count && (7 - i) < squadreQualifica.Rows.Count)
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                            comando.Parameters.Add(new SqlParameter("IDSQ2", squadreQualifica.Rows[7 - i]["IDSquadra"]));
                        }
                        else
                        {
                            if (i >= squadreQualifica.Rows.Count)
                                comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                            if ((7 - i) >= squadreQualifica.Rows.Count)
                                comando.Parameters.Add(new SqlParameter("IDSQ2", squadreBye.Rows[0]["IDSquadra"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ2", squadreQualifica.Rows[7 - i]["IDSquadra"]));
                        }
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("DataPartita", dataInizioQualifiche.Date));
                        /*
                         * Avendo messo le partite in ordine numerico (e non a forma di tabellone), in NumPartita metto 
                         * la posizione che la partita dovrebbe avere nel tabellone
                         */
                        switch (i)
                        {
                            case 0:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 1));
                                break;
                            case 1:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 4));
                                break;
                            case 2:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 2));
                                break;
                            case 3:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 3));
                                break;
                        }
                        comando.Parameters.Add(new SqlParameter("Fase", "1 turno eliminatorio"));
                        comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                else if (numSquadreQualifiche > 8 || numSquadreQualifiche <= 16)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDSQ1,IDSQ2,IDTorneo,NumPartita,Fase,DataPartita,OraPartita) " +
                            "VALUES (@IDSQ1,@IDSQ2,@IDTorneo,@NumPartita,@Fase,@DataPartita,@OraPartita)";
                        comando = new SqlCommand(sql, conn);
                        if (i < squadreQualifica.Rows.Count && (15 - i) < squadreQualifica.Rows.Count)
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                            comando.Parameters.Add(new SqlParameter("IDSQ2", squadreQualifica.Rows[15 - i]["IDSquadra"]));
                        }
                        else
                        {
                            if (i >= squadreQualifica.Rows.Count)
                                comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                            if ((15 - i) >= squadreQualifica.Rows.Count)
                                comando.Parameters.Add(new SqlParameter("IDSQ2", squadreBye.Rows[0]["IDSquadra"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ2", squadreQualifica.Rows[15 - i]["IDSquadra"]));
                        }
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("DataPartita", dataInizioQualifiche.Date));
                        /*
                         * Avendo messo le partite in ordine numerico (e non a forma di tabellone), in NumPartita metto 
                         * la posizione che la partita dovrebbe avere nel tabellone
                         */
                        switch (i)
                        {
                            case 0:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 1));
                                break;
                            case 1:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 8));
                                break;
                            case 2:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 5));
                                break;
                            case 3:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 4));
                                break;
                            case 4:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 3));
                                break;
                            case 5:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 6));
                                break;
                            case 6:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 7));
                                break;
                            case 7:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 2));
                                break;
                        }
                        comando.Parameters.Add(new SqlParameter("Fase", "1 turno eliminatorio"));
                        comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    if (numQualificati == 4) //Se a qualificarsi sono 4 squadre e non 8, bisogna fare un secondo turno
                    {
                        //Creazione partite secondo turno eliminatorio
                        for (int i = 0; i < 4; i++)
                        {
                            sql = "";
                            sql += "INSERT INTO Partita(IDTorneo,NumPartita,Fase,DataPartita,OraPartita)" +
                                "VALUES(@IDTorneo,@NumPartita,@Fase,@DataPartita,@OraPartita)";
                            comando = new SqlCommand(sql, conn);
                            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                            comando.Parameters.Add(new SqlParameter("NumPartita", i + 9));
                            comando.Parameters.Add(new SqlParameter("Fase", "2 turno eliminatorio"));
                            comando.Parameters.Add(new SqlParameter("DataPartita", dataPartite2Turno.Date));
                            comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                            conn.Open();
                            comando.ExecuteNonQuery();
                            conn.Close();
                        }
                        //Di conseguenza, inserisco nelle partite del 1 turno i NumPartitaSuccessiva
                        for (int i = 1; i <= 8; i++)
                        {
                            sql = "";
                            sql += "UPDATE Partita " +
                                   "SET NumPartitaSuccessiva = @NumPartitaSuccessiva " +
                                   "WHERE IDTorneo = @IDTorneo AND NumPartita = @NumPartita";
                            comando = new SqlCommand(sql, conn);
                            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                            comando.Parameters.Add(new SqlParameter("NumPartita", i));
                            switch (i)
                            {
                                case 1:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 9));
                                    break;
                                case 2:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 9));
                                    break;
                                case 3:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 10));
                                    break;
                                case 4:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 10));
                                    break;
                                case 5:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 11));
                                    break;
                                case 6:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 11));
                                    break;
                                case 7:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 12));
                                    break;
                                case 8:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 12));
                                    break;
                            }
                            conn.Open();
                            comando.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                }
                else if (numSquadreQualifiche > 16 || numSquadreQualifiche <= 32)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDSQ1,IDSQ2,IDTorneo,NumPartita,Fase,DataPartita,OraPartita) " +
                            "VALUES (@IDSQ1,@IDSQ2,@IDTorneo,@NumPartita,@Fase,@DataPartita,@OraPartita)";
                        comando = new SqlCommand(sql, conn);
                        if (i < squadreQualifica.Rows.Count && (31 - i) < squadreQualifica.Rows.Count)
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                            comando.Parameters.Add(new SqlParameter("IDSQ2", squadreQualifica.Rows[31 - i]["IDSquadra"]));
                        }
                        else
                        {
                            if (i >= squadreQualifica.Rows.Count)
                                comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                            if ((31 - i) >= squadreQualifica.Rows.Count)
                                comando.Parameters.Add(new SqlParameter("IDSQ2", squadreBye.Rows[0]["IDSquadra"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ2", squadreQualifica.Rows[31 - i]["IDSquadra"]));
                        }
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("DataPartita", dataInizioQualifiche.Date));
                        comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                        /*
                         * Avendo messo le partite in ordine numerico (e non a forma di tabellone), in NumPartita metto 
                         * la posizione che la partita dovrebbe avere nel tabellone
                         */
                        /*switch (i)
                        {
                            case 0:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 1));
                                break;
                            case 1:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 16));
                                break;
                            case 2:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 3:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 4:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 5:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 6:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 7:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 8:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 9:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 10:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 11:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 12:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 13:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 14:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 15:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                        }*/
                        comando.Parameters.Add(new SqlParameter("Fase", "1 turno eliminatorio"));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    //Creazione partite secondo turno eliminatorio
                    for (int i = 0; i < 8; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDTorneo,NumPartita,Fase,DataPartita,OraPartita)" +
                            "VALUES(@IDTorneo,@NumPartita,@Fase,@DataPartita,@OraPartita)";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("NumPartita", i + 17));
                        comando.Parameters.Add(new SqlParameter("Fase", "2 turno eliminatorio"));
                        comando.Parameters.Add(new SqlParameter("DataPartita", dataPartite2Turno.Date));
                        comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    //Di conseguenza, inserisco nelle partite del 1 turno i NumPartitaSuccessiva
                    for (int i = 1; i <= 16; i++)
                    {
                        sql = "";
                        sql += "UPDATE Partita " +
                               "SET NumPartitaSuccessiva = @NumPartitaSuccessiva " +
                               "WHERE IDTorneo = @IDTorneo AND NumPartita = @NumPartita";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("NumPartita", i));
                        switch (i)
                        {
                            case 1:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 17));
                                break;
                            case 2:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 17));
                                break;
                            case 3:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 18));
                                break;
                            case 4:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 18));
                                break;
                            case 5:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 19));
                                break;
                            case 6:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 19));
                                break;
                            case 7:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 20));
                                break;
                            case 8:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 20));
                                break;
                            case 9:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 21));
                                break;
                            case 10:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 21));
                                break;
                            case 11:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 22));
                                break;
                            case 12:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 22));
                                break;
                            case 13:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 23));
                                break;
                            case 14:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 23));
                                break;
                            case 15:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 24));
                                break;
                            case 16:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 24));
                                break;
                        }
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    //Se si devono qualificare in 4, bisogna fare un terzo turno eliminatorio
                    if (numQualificati == 4)
                    {
                        //Creazione partite terzo turno eliminatorio
                        for (int i = 0; i < 4; i++)
                        {
                            sql = "";
                            sql += "INSERT INTO Partita(IDTorneo,NumPartita,Fase,DataPartita,OraPartita)" +
                                "VALUES(@IDTorneo,@NumPartita,@Fase,@DataPartita,@OraPartita)";
                            comando = new SqlCommand(sql, conn);
                            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                            comando.Parameters.Add(new SqlParameter("NumPartita", i + 25));
                            comando.Parameters.Add(new SqlParameter("Fase", "3 turno eliminatorio"));
                            comando.Parameters.Add(new SqlParameter("DataPartita", dataPartite2Turno.Date));
                            comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                            conn.Open();
                            comando.ExecuteNonQuery();
                            conn.Close();
                        }
                        //Di conseguenza, inserisco nelle partite del 2 turno i NumPartitaSuccessiva
                        for (int i = 17; i <= 24; i++)
                        {
                            sql = "";
                            sql += "UPDATE Partita " +
                                   "SET NumPartitaSuccessiva = @NumPartitaSuccessiva " +
                                   "WHERE IDTorneo = @IDTorneo AND NumPartita = @NumPartita";
                            comando = new SqlCommand(sql, conn);
                            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                            comando.Parameters.Add(new SqlParameter("NumPartita", i));
                            switch (i)
                            {
                                case 17:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 25));
                                    break;
                                case 18:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 25));
                                    break;
                                case 19:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 26));
                                    break;
                                case 20:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 26));
                                    break;
                                case 21:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 27));
                                    break;
                                case 22:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 27));
                                    break;
                                case 23:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 28));
                                    break;
                                case 24:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 28));
                                    break;
                            }
                            conn.Open();
                            comando.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                }
                else if (numSquadreQualifiche > 32 || numSquadreQualifiche <= 64)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDSQ1,IDSQ2,IDTorneo,NumPartita,Fase,DataPartita,OraPartita) " +
                            "VALUES (@IDSQ1,@IDSQ2,@IDTorneo,@NumPartita,@Fase,@DataPartita,@OraPartita)";
                        comando = new SqlCommand(sql, conn);
                        if (i < squadreQualifica.Rows.Count && (63 - i) < squadreQualifica.Rows.Count)
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                            comando.Parameters.Add(new SqlParameter("IDSQ2", squadreQualifica.Rows[63 - i]["IDSquadra"]));
                        }
                        else
                        {
                            if (i >= squadreQualifica.Rows.Count)
                                comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                            if ((63 - i) >= squadreQualifica.Rows.Count)
                                comando.Parameters.Add(new SqlParameter("IDSQ2", squadreBye.Rows[0]["IDSquadra"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ2", squadreQualifica.Rows[63 - i]["IDSquadra"]));
                        }
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("DataPartita", dataInizioQualifiche.Date));
                        comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                        /*
                         * Avendo messo le partite in ordine numerico (e non a forma di tabellone), in NumPartita metto 
                         * la posizione che la partita dovrebbe avere nel tabellone
                         */
                        /*switch (i)
                        {
                            case 0:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 1));
                                break;
                            case 1:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 32));
                                break;
                            case 2:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 3:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 4:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 5:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 6:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 7:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 8:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 9:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 10:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 11:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 12:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 13:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 14:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 15:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 16:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 17:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 18:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 19:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 20:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 21:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 22:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 23:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 24:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 25:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 26:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 27:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 28:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 29:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 30:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                            case 31:
                                comando.Parameters.Add(new SqlParameter("NumPartita", ));
                                break;
                        }*/
                        comando.Parameters.Add(new SqlParameter("Fase", "1 turno eliminatorio"));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    //Creazione partite secondo turno eliminatorio
                    for (int i = 0; i < 16; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDTorneo,NumPartita,Fase,DataPartita,OraPartita)" +
                            "VALUES(@IDTorneo,@NumPartita,@Fase,@DataPartita,@OraPartita)";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("NumPartita", i + 33));
                        comando.Parameters.Add(new SqlParameter("Fase", "2 turno eliminatorio"));
                        comando.Parameters.Add(new SqlParameter("DataPartita", dataPartite2Turno.Date));
                        comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    //Di conseguenza, inserisco nelle partite del 1 turno i NumPartitaSuccessiva
                    for (int i = 1; i <= 32; i++)
                    {
                        sql = "";
                        sql += "UPDATE Partita " +
                               "SET NumPartitaSuccessiva = @NumPartitaSuccessiva " +
                               "WHERE IDTorneo = @IDTorneo AND NumPartita = @NumPartita";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("NumPartita", i));
                        switch (i)
                        {
                            case 1:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 33));
                                break;
                            case 2:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 33));
                                break;
                            case 3:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 34));
                                break;
                            case 4:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 34));
                                break;
                            case 5:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 35));
                                break;
                            case 6:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 35));
                                break;
                            case 7:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 36));
                                break;
                            case 8:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 36));
                                break;
                            case 9:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 37));
                                break;
                            case 10:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 37));
                                break;
                            case 11:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 38));
                                break;
                            case 12:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 38));
                                break;
                            case 13:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 39));
                                break;
                            case 14:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 39));
                                break;
                            case 15:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 40));
                                break;
                            case 16:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 40));
                                break;
                            case 17:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 41));
                                break;
                            case 18:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 41));
                                break;
                            case 19:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 42));
                                break;
                            case 20:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 42));
                                break;
                            case 21:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 43));
                                break;
                            case 22:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 43));
                                break;
                            case 23:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 44));
                                break;
                            case 24:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 44));
                                break;
                            case 25:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 45));
                                break;
                            case 26:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 45));
                                break;
                            case 27:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 46));
                                break;
                            case 28:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 46));
                                break;
                            case 29:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 47));
                                break;
                            case 30:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 47));
                                break;
                            case 31:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 48));
                                break;
                            case 32:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 48));
                                break;
                        }
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    //Creazione partite terzo turno eliminatorio
                    for (int i = 0; i < 8; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDTorneo,NumPartita,Fase,DataPartita,OraPartita)" +
                            "VALUES(@IDTorneo,@NumPartita,@Fase,@DataPartita,@OraPartita)";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("NumPartita", i + 49));
                        comando.Parameters.Add(new SqlParameter("Fase", "3 turno eliminatorio"));
                        comando.Parameters.Add(new SqlParameter("DataPartita", dataPartite2Turno.Date));
                        comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    //Di conseguenza, inserisco nelle partite del 2 turno i NumPartitaSuccessiva
                    for (int i = 33; i <= 48; i++)
                    {
                        sql = "";
                        sql += "UPDATE Partita " +
                               "SET NumPartitaSuccessiva = @NumPartitaSuccessiva " +
                               "WHERE IDTorneo = @IDTorneo AND NumPartita = @NumPartita";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("NumPartita", i));
                        switch (i)
                        {
                            case 33:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 49));
                                break;
                            case 34:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 49));
                                break;
                            case 35:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 50));
                                break;
                            case 36:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 50));
                                break;
                            case 37:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 51));
                                break;
                            case 38:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 51));
                                break;
                            case 39:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 52));
                                break;
                            case 40:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 52));
                                break;
                            case 41:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 53));
                                break;
                            case 42:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 53));
                                break;
                            case 43:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 54));
                                break;
                            case 44:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 54));
                                break;
                            case 45:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 55));
                                break;
                            case 46:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 55));
                                break;
                            case 47:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 56));
                                break;
                            case 48:
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 56));
                                break;
                        }
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    //Se si qualificano 4 squadre, serve anche un 4 turno eliminatorio
                    if (numQualificati == 4)
                    {
                        //Creazione partite quarto turno eliminatorio
                        for (int i = 0; i < 4; i++)
                        {
                            sql = "";
                            sql += "INSERT INTO Partita(IDTorneo,NumPartita,Fase,DataPartita,OraPartita)" +
                                "VALUES(@IDTorneo,@NumPartita,@Fase,@DataPartita,@OraPartita)";
                            comando = new SqlCommand(sql, conn);
                            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                            comando.Parameters.Add(new SqlParameter("NumPartita", i + 57));
                            comando.Parameters.Add(new SqlParameter("Fase", "3 turno eliminatorio"));
                            comando.Parameters.Add(new SqlParameter("DataPartita", dataPartite2Turno.Date));
                            comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                            conn.Open();
                            comando.ExecuteNonQuery();
                            conn.Close();
                        }
                        //Di conseguenza, inserisco nelle partite del 3 turno i NumPartitaSuccessiva
                        for (int i = 49; i <= 56; i++)
                        {
                            sql = "";
                            sql += "UPDATE Partita " +
                                   "SET NumPartitaSuccessiva = @NumPartitaSuccessiva " +
                                   "WHERE IDTorneo = @IDTorneo AND NumPartita = @NumPartita";
                            comando = new SqlCommand(sql, conn);
                            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                            comando.Parameters.Add(new SqlParameter("NumPartita", i));
                            switch (i)
                            {
                                case 49:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 57));
                                    break;
                                case 50:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 57));
                                    break;
                                case 51:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 58));
                                    break;
                                case 52:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 58));
                                    break;
                                case 53:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 59));
                                    break;
                                case 54:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 59));
                                    break;
                                case 55:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 60));
                                    break;
                                case 56:
                                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 60));
                                    break;
                            }
                            conn.Open();
                            comando.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errore = e.Message;
            }
        }
        //Metodo che crea le partite per il torneo di qualifica con 6 NTQ
        private void PartiteNTQ6(int numSquadreQualifiche, DataTable squadreQualifica, int numQualificati, int idTorneoQualifica, DateTime dataPartite2Turno, DateTime dataInizioQualifiche)
        {
            SqlCommand comando;
            SqlDataAdapter adapter;
            DataTable squadreBye;
            string sql;
            try
            {
                //Prendo le squadre bye
                sql = "";
                sql += "SELECT * FROM Squadra WHERE NomeTeam='Bye'";
                adapter = new SqlDataAdapter(sql, conn);
                squadreBye = new DataTable();
                conn.Open();
                adapter.Fill(squadreBye);
                conn.Close();
                //Partite
                if (numSquadreQualifiche <= 12)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDSQ1,IDSQ2,IDTorneo,NumPartita,Fase,DataPartita,OraPartita) " +
                            "VALUES (@IDSQ1,@IDSQ2,@IDTorneo,@NumPartita,@Fase,@DataPartita,@OraPartita)";
                        comando = new SqlCommand(sql, conn);
                        if (i < squadreQualifica.Rows.Count && (11 - i) < squadreQualifica.Rows.Count)
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                            comando.Parameters.Add(new SqlParameter("IDSQ2", squadreQualifica.Rows[11 - i]["IDSquadra"]));
                        }
                        else
                        {
                            if (i >= squadreQualifica.Rows.Count)
                                comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                            if ((11 - i) >= squadreQualifica.Rows.Count)
                                comando.Parameters.Add(new SqlParameter("IDSQ2", squadreBye.Rows[0]["IDSquadra"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ2", squadreQualifica.Rows[11 - i]["IDSquadra"]));
                        }
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("DataPartita", dataInizioQualifiche.Date));
                        /*
                         * Avendo messo le partite in ordine numerico (e non a forma di tabellone), in NumPartita metto 
                         * la posizione che la partita dovrebbe avere nel tabellone
                         */
                        switch (i)
                        {
                            case 0:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 1));
                                break;
                            case 1:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 6));
                                break;
                            case 2:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 4));
                                break;
                            case 3:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 3));
                                break;
                            case 4:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 2));
                                break;
                            case 5:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 5));
                                break;
                        }
                        comando.Parameters.Add(new SqlParameter("Fase", "Singolo turno eliminatorio"));
                        comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                else if (numSquadreQualifiche <= 24)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDSQ1,IDSQ2,IDTorneo,NumPartita,Fase,DataPartita,NumPartitaSuccessiva,OraPartita) " +
                            "VALUES (@IDSQ1,@IDSQ2,@IDTorneo,@NumPartita,@Fase,@DataPartita,@NumPartitaSuccessiva,@OraPartita)";
                        comando = new SqlCommand(sql, conn);
                        if (i < squadreQualifica.Rows.Count && (23 - i) < squadreQualifica.Rows.Count)
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                            comando.Parameters.Add(new SqlParameter("IDSQ2", squadreQualifica.Rows[23 - i]["IDSquadra"]));
                        }
                        else
                        {
                            if (i >= squadreQualifica.Rows.Count)
                                comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                            if ((23 - i) >= squadreQualifica.Rows.Count)
                                comando.Parameters.Add(new SqlParameter("IDSQ2", squadreBye.Rows[0]["IDSquadra"]));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ2", squadreQualifica.Rows[23 - i]["IDSquadra"]));
                        }
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("DataPartita", dataInizioQualifiche.Date));
                        /*
                         * Avendo messo le partite in ordine numerico (e non a forma di tabellone), in NumPartita metto 
                         * la posizione che la partita dovrebbe avere nel tabellone e la partita successiva
                         */
                        switch (i)
                        {
                            case 0:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 1));
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 13));
                                break;
                            case 1:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 12));
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 18));
                                break;
                            case 2:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 7));
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 16));
                                break;
                            case 3:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 6));
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 15));
                                break;
                            case 4:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 4));
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 14));
                                break;
                            case 5:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 9));
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 17));
                                break;
                            case 6:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 10));
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 17));
                                break;
                            case 7:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 3));
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 14));
                                break;
                            case 8:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 5));
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 15));
                                break;
                            case 9:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 8));
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 16));
                                break;
                            case 10:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 11));
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 18));
                                break;
                            case 11:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 2));
                                comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", 13));
                                break;
                        }
                        comando.Parameters.Add(new SqlParameter("Fase", "Primo turno eliminatorio"));
                        comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    //Secondo turno eliminatorio
                    for (int i = 0; i < 6; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDTorneo,NumPartita,Fase,DataPartita,OraPartita) " +
                            "VALUES (@IDTorneo,@NumPartita,@Fase,@DataPartita,@OraPartita)";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("DataPartita", dataPartite2Turno.Date));
                        comando.Parameters.Add(new SqlParameter("NumPartita", i + 13));
                        comando.Parameters.Add(new SqlParameter("Fase", "Secondo turno eliminatorio"));
                        comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                }
            }
            catch (Exception e)
            {
                string errore = e.Message;
            }
        }
        //Metodo che crea le partite per il torneo di qualifica con 1 NTQ
        private void PartiteNTQ1(int numSquadreQualifiche, DataTable squadreQualifica, int numQualificati, int idTorneoQualifica, DateTime dataPartite2Turno, DateTime dataInizioQualifiche)
        {
            SqlCommand comando;
            SqlDataAdapter adapter;
            DataTable squadreBye;
            string sql;
            try
            {
                //Prendo le squadre bye
                sql = "";
                sql += "SELECT * FROM Squadra WHERE NomeTeam='Bye'";
                adapter = new SqlDataAdapter(sql, conn);
                squadreBye = new DataTable();
                conn.Open();
                adapter.Fill(squadreBye);
                conn.Close();
                sql = "";
                sql += "INSERT INTO Partita(IDSQ1,IDSQ2,IDTorneo,NumPartita,Fase,DataPartita,OraPartita) " +
                    "VALUES (@IDSQ1,@IDSQ2,@IDTorneo,@NumPartita,@Fase,@DataPartita,@OraPartita)";
                comando = new SqlCommand(sql, conn);
                if (squadreQualifica.Rows.Count == 2)
                {
                    comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[0]["IDSquadra"]));
                    comando.Parameters.Add(new SqlParameter("IDSQ2", squadreQualifica.Rows[1]["IDSquadra"]));
                }
                else
                {
                    comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[0]["IDSquadra"]));
                    comando.Parameters.Add(new SqlParameter("IDSQ2", squadreBye.Rows[0]["IDSquadra"]));
                }
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                comando.Parameters.Add(new SqlParameter("DataPartita", dataInizioQualifiche.Date));
                comando.Parameters.Add(new SqlParameter("NumPartita", 1));
                comando.Parameters.Add(new SqlParameter("Fase", "Singolo turno eliminatorio"));
                comando.Parameters.Add(new SqlParameter("OraPartita", DateTime.Now.TimeOfDay));
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception e)
            {
                string errore = e.Message;
            }
        }
        //Metodo che gestisce le partite che contengono le squadre "Bye"
        public string GestisciSquadraBye(int idTorneoQualifiche, int numPartita, int idTorneoPrincipale)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable partita, infoSquadra, infoPartitaSuccessiva = new DataTable();
            try
            {
                //Prendo le info della partita
                sql = "";
                sql += "SELECT Squadra1.NomeTeam As Squadra1,Squadra2.NomeTeam As Squadra2,Partita.NumPartitaSuccessiva,Squadra1.IDSquadra As IDSquadra1,Squadra2.IDSquadra As IDSquadra2 " +
                    "FROM((Partita LEFT JOIN Squadra Squadra1 ON Partita.IDSQ1 = Squadra1.IDSquadra)LEFT JOIN Squadra Squadra2 ON Partita.IDSQ2 = Squadra2.IDSquadra) " +
                    "WHERE IDTorneo = @IDTorneo AND NumPartita = @NumPartita";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifiche));
                comando.Parameters.Add(new SqlParameter("NumPartita", numPartita));
                adapter = new SqlDataAdapter(comando);
                partita = new DataTable();
                conn.Open();
                adapter.Fill(partita);
                conn.Close();
                //Prendo i dati delle 2 squadre
                sql = "";
                sql += "SELECT * FROM Partecipa WHERE IDTorneo=@IDTorneo AND (IDSquadra=@IDSquadra1 OR IDSquadra=@IDSquadra2)";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifiche));
                comando.Parameters.Add(new SqlParameter("IDSquadra1", partita.Rows[0]["IDSquadra1"]));
                comando.Parameters.Add(new SqlParameter("IDSquadra2", partita.Rows[0]["IDSquadra2"]));
                adapter = new SqlDataAdapter(comando);
                infoSquadra = new DataTable();
                conn.Open();
                adapter.Fill(infoSquadra);
                conn.Close();
                //Prendo le info della partita successiva (se c'è)
                if (partita.Rows[0]["NumPartitaSuccessiva"] != DBNull.Value)
                {
                    sql = "";
                    sql += "SELECT IDSQ1,IDSQ2 FROM Partita WHERE IDTorneo=@IDTorneo AND NumPartita=@NumPartitaSuccessiva";
                    comando = new SqlCommand(sql, conn);
                    comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifiche));
                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", partita.Rows[0]["NumPartitaSuccessiva"]));
                    adapter = new SqlDataAdapter(comando);
                    conn.Open();
                    adapter.Fill(infoPartitaSuccessiva);
                    conn.Close();
                }
                if (partita.Rows[0]["Squadra1"].ToString() == "Bye" && partita.Rows[0]["Squadra2"].ToString() == "Bye")
                {
                    if (partita.Rows[0]["NumPartitaSuccessiva"] != DBNull.Value)
                    {
                        sql = "";
                        sql += "UPDATE Partita " +
                            "SET IDSQ1=@IDSQ1,IDSQ2=@IDSQ2 " +
                            "WHERE IDTorneo=@IDTorneo AND NumPartita=@NumPartitaSuccessiva";
                        comando = new SqlCommand(sql, conn);
                        if (numPartita % 2 != 0) //Se il NumPartita è dispari, la squadra vincente sarà la squadra 1 nella partita successiva
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ1", partita.Rows[0]["IDSquadra2"]));
                            if (infoPartitaSuccessiva.Rows[0]["IDSQ2"] == DBNull.Value)
                                comando.Parameters.Add(new SqlParameter("IDSQ2", DBNull.Value));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ2", infoPartitaSuccessiva.Rows[0]["IDSQ2"]));
                        }
                        else
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ2", partita.Rows[0]["IDSquadra2"]));
                            if (infoPartitaSuccessiva.Rows[0]["IDSQ1"] == DBNull.Value)
                                comando.Parameters.Add(new SqlParameter("IDSQ1", DBNull.Value));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ1", infoPartitaSuccessiva.Rows[0]["IDSQ1"]));
                        }
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    else
                    {

                    }
                }
                else if (partita.Rows[0]["Squadra1"].ToString() == "Bye")
                {
                    if (partita.Rows[0]["NumPartitaSuccessiva"] != DBNull.Value)//Se c'è un ulteriore turno eliminatorio:
                    {
                        sql = "";
                        sql += "UPDATE Partita " +
                            "SET IDSQ1=@IDSQ1,IDSQ2=@IDSQ2 " +
                            "WHERE IDTorneo=@IDTorneo AND NumPartita=@NumPartitaSuccessiva";
                        comando = new SqlCommand(sql, conn);
                        if (numPartita % 2 != 0) //Se il NumPartita è dispari, la squadra vincente sarà la squadra 1 nella partita successiva
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ1", partita.Rows[0]["IDSquadra2"]));
                            if (infoPartitaSuccessiva.Rows[0]["IDSQ2"] == DBNull.Value)
                                comando.Parameters.Add(new SqlParameter("IDSQ2", DBNull.Value));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ2", infoPartitaSuccessiva.Rows[0]["IDSQ2"]));
                        }
                        else
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ2", partita.Rows[0]["IDSquadra2"]));
                            if (infoPartitaSuccessiva.Rows[0]["IDSQ1"] == DBNull.Value)
                                comando.Parameters.Add(new SqlParameter("IDSQ1", DBNull.Value));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ1", infoPartitaSuccessiva.Rows[0]["IDSQ1"]));
                        }
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    else
                    {
                        sql = "";
                        sql += "INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints,Punti)" +
                            "VALUES(@IDSquadra,@IDTorneo,@IDAllenatore,@EntryPoints,@Punti)";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDSquadra", infoSquadra.Rows[1]["IDSquadra"]));
                        if (infoSquadra.Rows[1]["IDAllenatore"] != DBNull.Value)
                            comando.Parameters.Add(new SqlParameter("IDAllenatore", infoSquadra.Rows[1]["IDAllenatore"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDAllenatore", DBNull.Value));
                        comando.Parameters.Add(new SqlParameter("EntryPoints", infoSquadra.Rows[1]["EntryPoints"]));
                        comando.Parameters.Add(new SqlParameter("Punti", infoSquadra.Rows[1]["Punti"]));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                else
                {
                    if (partita.Rows[0]["NumPartitaSuccessiva"] != DBNull.Value)
                    {
                        sql = "";
                        sql += "UPDATE Partita " +
                            "SET IDSQ1=@IDSQ1,IDSQ2=@IDSQ2 " +
                            "WHERE IDTorneo=@IDTorneo AND NumPartita=@NumPartitaSuccessiva";
                        comando = new SqlCommand(sql, conn);
                        if (numPartita % 2 != 0) //Se il NumPartita è dispari, la squadra vincente sarà la squadra 1 nella partita successiva
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ1", partita.Rows[0]["IDSquadra1"]));
                            if (infoPartitaSuccessiva.Rows[0]["IDSQ2"] == DBNull.Value)
                                comando.Parameters.Add(new SqlParameter("IDSQ2", DBNull.Value));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ2", infoPartitaSuccessiva.Rows[0]["IDSQ2"]));
                        }
                        else
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ2", partita.Rows[0]["IDSquadra1"]));
                            if (infoPartitaSuccessiva.Rows[0]["IDSQ1"] == DBNull.Value)
                                comando.Parameters.Add(new SqlParameter("IDSQ1", DBNull.Value));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ1", infoPartitaSuccessiva.Rows[0]["IDSQ1"]));
                        }
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    else
                    {
                        sql = "";
                        sql += "INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints,Punti)" +
                            "VALUES(@IDSquadra,@IDTorneo,@IDAllenatore,@EntryPoints,@Punti)";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDSquadra", infoSquadra.Rows[0]["IDSquadra"]));
                        if (infoSquadra.Rows[0]["IDAllenatore"] != DBNull.Value)
                            comando.Parameters.Add(new SqlParameter("IDAllenatore", infoSquadra.Rows[0]["IDAllenatore"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDAllenatore", DBNull.Value));
                        comando.Parameters.Add(new SqlParameter("EntryPoints", infoSquadra.Rows[0]["EntryPoints"]));
                        comando.Parameters.Add(new SqlParameter("Punti", infoSquadra.Rows[0]["Punti"]));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                return "Gestione avvenuta con successo";
            }
            catch (Exception e)
            {
                return "ERRORE: " + e.Message;
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
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,Societa.NomeSocieta,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.DataChiusuraIscrizioni,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,NumTeamQualificati,NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
            "FROM((((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On Torneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune)LEFT JOIN Societa On Societa.IDSocieta = Torneo.IDSocieta) " +
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
                sql += "SELECT DISTINCT Squadra.IDSquadra,Squadra.NomeTeam, CONCAT(Atleta1.Nome,' ',Atleta1.Cognome) as Atleta1, CONCAT(Atleta2.Nome,' ',Atleta2.Cognome) as Atleta2, ListaIscritti.EntryPoints ";
                sql += "FROM Squadra, ListaIscritti, Atleta as Atleta1, Atleta as Atleta2 ";
                sql += "WHERE ListaIscritti.IDSquadra = Squadra.IDSquadra AND ListaIscritti.IDTorneo = @IDTorneo AND Squadra.IDAtleta1 = Atleta1.IDAtleta AND Squadra.IDAtleta2 = Atleta2.IDAtleta ORDER BY ListaIscritti.EntryPoints DESC";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
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
        public DataTable GetTorneiFinitiByAtleta(int idAtleta)
        {
            string sql;
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            sql = "";
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche,Torneo.NumTeamQualificati,Torneo.NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
            "FROM(((((((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune)LEFT JOIN ListaIscritti ON ListaIscritti.IDTorneo = Torneo.IDTorneo)LEFT JOIN Squadra ON Squadra.IDSquadra = ListaIscritti.IDSquadra)LEFT JOIN Atleta Atleta1 ON Squadra.IDAtleta1 = Atleta1.IDAtleta OR Squadra.IDAtleta2 = Atleta1.IDAtleta)LEFT JOIN Atleta Atleta2 ON Squadra.IDAtleta1 = Atleta2.IDAtleta OR Squadra.IDAtleta2 = Atleta2.IDAtleta) " +
            "WHERE CAST(Torneo.DataFine as DATE) <= @Data AND(Squadra.IDAtleta1 = @IDAtleta OR Squadra.IDAtleta2 = @IDAtleta)";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));
            comando.Parameters.Add(new SqlParameter("IDAtleta", idAtleta));
            adapter = new SqlDataAdapter(comando);
            query = new DataTable();
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetTorneiBySocieta(int idSocieta)
        {
            string sql;
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo, Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome, ' ', Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome, ' ', SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale, CONCAT(DirettoreCompetizione.Nome, ' ', DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche,Torneo.NumTeamQualificati,Torneo.NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
            "FROM(((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On ImpiantoTorneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune) " +
            "WHERE Torneo.IDSocieta = @IDSocieta";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDSocieta", idSocieta));
            adapter = new SqlDataAdapter(comando);
            query = new DataTable();
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetAllImpianti()
        {
            string sql;
            SqlDataAdapter adapter;
            DataTable query;
            sql = "";
            sql += "SELECT IDImpianto,NomeImpianto FROM Impianto ORDER BY NomeImpianto ASC";
            adapter = new SqlDataAdapter(sql, conn);
            query = new DataTable();
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetNumeroCampiSocieta(int idimpianto)
        {
            string sql;
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            sql = "";
            sql += "SELECT NumeroCampi FROM Impianto WHERE idImpianto=@idimpianto";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("idimpianto", idimpianto));
            adapter = new SqlDataAdapter(comando);
            query = new DataTable();
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetTesseraInfo(int idsocieta)
        {
            string sql;
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            sql = "";
            sql += "SELECT DISTINCT CONCAT(Atleta.Nome,' ',Atleta.Cognome)as Atleta,StoricoTessereAtleti.CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento,Importo FROM StoricoTessereAtleti,Atleta,Societa WHERE StoricoTessereAtleti.IDAtleta=Atleta.IDAtleta AND AnnoTesseramento= YEAR(GETDATE()) AND StoricoTessereAtleti.IDSocieta=@idsocieta;";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("idsocieta", idsocieta));
            adapter = new SqlDataAdapter(comando);
            query = new DataTable();
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        public DataTable GetTesseraInfoAllenatore(int idsocieta)
        {
            string sql;
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            sql = "";
            sql += "SELECT DISTINCT CONCAT(Allenatore.Nome,' ',Allenatore.Cognome)as Allenatore,StoricoTessereAllenatori.CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento,Importo FROM StoricoTessereAllenatori,Allenatore,Societa WHERE StoricoTessereAllenatori.IDAllenatore=Allenatore.IDAllenatore AND AnnoTesseramento= YEAR(GETDATE()) AND StoricoTessereAllenatori.IDSocieta=@idsocieta";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("idsocieta", idsocieta));
            adapter = new SqlDataAdapter(comando);
            query = new DataTable();
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }

        public bool AssegnaTessereBySocieta(int idAtleta, int idSocieta, string codiceTessera, string tipoTessera, DateTime dataTesseramento, int annoTesseramento, double importo)
        {
            string sql;
            SqlCommand comando;
            try
            {
                sql = "";
                sql += "UPDATE Atleta " +
                    "SET CodiceTessera=@Codicetessera " +
                    "WHERE IDAtleta=@idatleta";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("idatleta", idAtleta));
                comando.Parameters.Add(new SqlParameter("Codicetessera", codiceTessera));
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                try
                {
                    sql = "";
                    sql += "INSERT INTO StoricoTessereAtleti(IDAtleta,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento,Importo) " +
                        "VALUES(@IDAtleta,@IDSocieta,@CodiceTessera,@TipoTessera,@DataTesseramento,@AnnoTesseramento,@Importo)";
                    comando = new SqlCommand(sql, conn);
                    comando.Parameters.Add(new SqlParameter("IDAtleta", idAtleta));
                    comando.Parameters.Add(new SqlParameter("IDSocieta", idSocieta));
                    comando.Parameters.Add(new SqlParameter("CodiceTessera", codiceTessera));
                    comando.Parameters.Add(new SqlParameter("TipoTessera", tipoTessera));
                    comando.Parameters.Add(new SqlParameter("DataTesseramento", dataTesseramento.Date));
                    comando.Parameters.Add(new SqlParameter("AnnoTesseramento", annoTesseramento));
                    comando.Parameters.Add(new SqlParameter("Importo", importo));
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
            catch (Exception e)
            {
                return false;
            }

        }
        public bool AssegnaTessereAllenatoreBySocieta(int idAtleta, int idSocieta, string codiceTessera, string tipoTessera, DateTime dataTesseramento, int annoTesseramento, double importo)
        {
            string sql;
            SqlCommand comando;
            try
            {
                sql = "";
                sql += "UPDATE Allenatore " +
                    "SET CodiceTessera=@Codicetessera " +
                    "WHERE IDAllenatore=@idallenatore";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("idallenatore", idAtleta));
                comando.Parameters.Add(new SqlParameter("Codicetessera", codiceTessera));
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                try
                {
                    sql = "";
                    sql += "INSERT INTO StoricoTessereAllenatori(IDAllenatore,IDSocieta,CodiceTessera,TipoTessera,DataTesseramento,AnnoTesseramento,Importo) " +
                        "VALUES(@IDAllenatore,@IDSocieta,@CodiceTessera,@TipoTessera,@DataTesseramento,@AnnoTesseramento,@Importo)";
                    comando = new SqlCommand(sql, conn);
                    comando.Parameters.Add(new SqlParameter("IDAllenatore", idAtleta));
                    comando.Parameters.Add(new SqlParameter("IDSocieta", idSocieta));
                    comando.Parameters.Add(new SqlParameter("CodiceTessera", codiceTessera));
                    comando.Parameters.Add(new SqlParameter("TipoTessera", tipoTessera));
                    comando.Parameters.Add(new SqlParameter("DataTesseramento", dataTesseramento.Date));
                    comando.Parameters.Add(new SqlParameter("AnnoTesseramento", annoTesseramento));
                    comando.Parameters.Add(new SqlParameter("Importo", importo));
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
            catch (Exception e)
            {
                return false;
            }

        }
        public DataTable GetPartiteTorneo(int idTorneo)
        {
            string sql;
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            sql = "";
            sql += "SELECT Partita.NumPartita,Squadra1.NomeTeam As Team1,Squadra2.NomeTeam As Team2,CONCAT(Arbitro1.Nome,' ',Arbitro1.Cognome) As Arbitro1,CONCAT(Arbitro2.Nome,' ',Arbitro2.Cognome) As Arbitro2,Torneo.Titolo,Partita.Fase,Partita.Campo,Partita.risultato,Partita.DataPartita,Partita.OraPartita,Partita.Durata,Partita.PT1S1,Partita.PT2S1,Partita.PT1S2,Partita.PT2S2,Partita.PT1S3,Partita.PT2S3,Partita.SetSQ1,Partita.SetSQ2 " +
                   "FROM(((((Partita LEFT JOIN Squadra Squadra1 ON Partita.IDSQ1 = Squadra1.IDSquadra)LEFT JOIN Squadra Squadra2 ON Partita.IDSQ2 = Squadra2.IDSquadra)LEFT JOIN DelegatoTecnico Arbitro1 ON Partita.IDArbitro1 = Arbitro1.IDDelegato)LEFT JOIN DelegatoTecnico Arbitro2 ON Partita.IDArbitro2 = Arbitro2.IDDelegato)LEFT JOIN Torneo ON Partita.IDTorneo = Torneo.IDTorneo) " +
                   "WHERE Partita.IDTorneo = @IDTorneo";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
            adapter = new SqlDataAdapter(comando);
            query = new DataTable();
            conn.Open();
            adapter.Fill(query);
            conn.Close();
            return query;
        }
        //Metodo che riempe la partita con le informazioni mancanti
        public bool AssegnaInfoPartita(int idArbitro1, int idArbitro2, string campo, DateTime dataPartita, DateTime oraPartita, int idPartita)
        {
            string sql;
            SqlCommand comando;
            try
            {
                sql = "";
                sql += "UPDATE Partita SET IDArbitro1=@IDArbitro1,IDArbitro2=@IDArbitro2,Campo=@Campo,DataPartita=@DataPartita,OraPartita=@OraPartita " +
                    "WHERE IDPartita=@IDPartita";
                comando = new SqlCommand(sql, conn);
                if (idArbitro1 != 0)
                    comando.Parameters.Add(new SqlParameter("IDArbitro1", idArbitro1));
                else
                    comando.Parameters.Add(new SqlParameter("IDArbitro1", DBNull.Value));
                if (idArbitro2 != 0)
                    comando.Parameters.Add(new SqlParameter("IDArbitro2", idArbitro1));
                else
                    comando.Parameters.Add(new SqlParameter("IDArbitro2", DBNull.Value));
                comando.Parameters.Add(new SqlParameter("Campo", campo));
                comando.Parameters.Add(new SqlParameter("DataPartita", dataPartita.Date));
                comando.Parameters.Add(new SqlParameter("OraPartita", oraPartita.TimeOfDay));
                comando.Parameters.Add(new SqlParameter("IDPartita", idPartita));
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
        //Metodo per modificare le info del torneo
        public string UpdateTorneo(string titolo, int puntiVittoria, double montepremi, DateTime dataChiusuraIscrizioni, DateTime dataInizio, DateTime dataFine, char genere, int idFormulaTorneo, int NumMaxTeamMainDraw, int NumMaxTeamQualifiche, int[] idParametriTorneo, int idTipoTorneo, double quotaIscrizione, int idSocieta, int numTeamQualificati, int numWildCard, int idImpianto, bool outdoor, bool riunioneTecnica, string oraInizio, int idSupervisore, int idSupArbitrale, int idDirettore, DateTime dataPubblicazioneLista, int visibilitaListaIngresso, string urlLocandina, int idTorneo)
        {
            string sql;
            SqlCommand comando;
            try
            {

                //Cambio i dati del torneo
                sql = "";
                sql += "UPDATE Torneo " +
                    "SET IDSocieta=@IDSocieta," +
                    "IDImpianto=@IDImpianto," +
                    "IDTipoTorneo=@IDTipoTorneo," +
                    "IDSupervisore=@IDSupervisore," +
                    "IDSupervisoreArbitrale=@IDSupervisoreArbitrale," +
                    "IDDirettoreCompetizione=@IDDirettoreCompetizione," +
                    "IDFormula=@IDFormula," +
                    "Titolo=@Titolo," +
                    "QuotaIscrizione=@QuotaIscrizione," +
                    "PuntiVittoria=@PuntiVittoria," +
                    "Montepremi=@Montepremi," +
                    "DataChiusuraIscrizioni=@DataChiusuraIscrizioni," +
                    "DataPubblicazioneLista=@DataPubblicazioneLista," +
                    "DataInizio=@DataInizio," +
                    "OraInizio=@OraInizio," +
                    "RiunioneTecnica=@RiunioneTecnica," +
                    "DataFine=@DataFine," +
                    "Gender=@Gender," +
                    "Outdoor=@Outdoor," +
                    "VisibilitaListaIngresso=@VisibilitaListaIngresso," +
                    "NumMaxTeamMainDraw=@NumMaxTeamMainDraw," +
                    "NumMaxTeamQualifiche=@NumMaxTeamQualifiche," +
                    "NumTeamQualificati=@NumMaxTeamQualifiche," +
                    "NumWildCard=@NumWildCard," +
                    "URL_Locanina=@URL_Locanina " +
                    "WHERE IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDSocieta", idSocieta));
                comando.Parameters.Add(new SqlParameter("IDImpianto", idImpianto));
                comando.Parameters.Add(new SqlParameter("IDTipoTorneo", idTipoTorneo));
                comando.Parameters.Add(new SqlParameter("IDSupervisore", idSupervisore));
                comando.Parameters.Add(new SqlParameter("IDSupervisoreArbitrale", idSupArbitrale));
                comando.Parameters.Add(new SqlParameter("IDDirettoreCompetizione", idDirettore));
                comando.Parameters.Add(new SqlParameter("IDFormula", idFormulaTorneo));
                comando.Parameters.Add(new SqlParameter("Titolo", titolo));
                comando.Parameters.Add(new SqlParameter("QuotaIscrizione", quotaIscrizione));
                comando.Parameters.Add(new SqlParameter("PuntiVittoria", puntiVittoria));
                comando.Parameters.Add(new SqlParameter("Montepremi", montepremi));
                comando.Parameters.Add(new SqlParameter("DataChiusuraIscrizioni", dataChiusuraIscrizioni.Date));
                comando.Parameters.Add(new SqlParameter("DataPubblicazioneLista", dataPubblicazioneLista.Date));
                comando.Parameters.Add(new SqlParameter("DataInizio", dataInizio.Date));
                comando.Parameters.Add(new SqlParameter("OraInizio", oraInizio));
                comando.Parameters.Add(new SqlParameter("RiunioneTecnica", riunioneTecnica));
                comando.Parameters.Add(new SqlParameter("DataFine", dataFine.Date));
                comando.Parameters.Add(new SqlParameter("Gender", genere));
                comando.Parameters.Add(new SqlParameter("Outdoor", outdoor));
                comando.Parameters.Add(new SqlParameter("VisibilitaListaIngresso", visibilitaListaIngresso));
                comando.Parameters.Add(new SqlParameter("NumMaxTeamMainDraw", NumMaxTeamMainDraw));
                comando.Parameters.Add(new SqlParameter("NumMaxTeamQualifiche", NumMaxTeamQualifiche));
                comando.Parameters.Add(new SqlParameter("NumTeamQualificati", numTeamQualificati));
                comando.Parameters.Add(new SqlParameter("NumWildCard", numWildCard));
                comando.Parameters.Add(new SqlParameter("URL_Locanina", urlLocandina));
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                //Cambio i parametri
                //Tocca capire come inserire tutti i paramtri (uno ad uno) in una tabella n a n
                /*for (int i = 0; i < idParametriTorneo.Length; i++)
                {
                    sql = "";
                    sql += "UPDATE ParametroTorneo " +
                        "SET IDParametro=@IDParametro " +
                        "WHERE IDTorneo=@"

                }*/
                return "Info cambiate con successo";
            }
            catch (Exception e)
            {
                return "ERRORE: " + e.Message;
            }
        }
        public string AssegnaArbitriPartita(int idArbitro1, int idArbitro2, int idTorneo, int numPartita)
        {
            //Metodo per assegnare gli arbitri alle partite
            string sql;
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable delegatiTorneo;
            bool trovaArbitri = false;
            try
            {
                //Prendo tutti gli arbitri collegati al torneo
                sql = "";
                sql += "SELECT IDDelegato FROM ArbitraTorneo WHERE IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                delegatiTorneo = new DataTable();
                conn.Open();
                adapter.Fill(delegatiTorneo);
                conn.Close();
                //Cerco se gli arbitri sono presenti
                for (int i = 0; i < delegatiTorneo.Rows.Count; i++)
                {
                    if (Convert.ToInt32(delegatiTorneo.Rows[i]["IDDelegato"]) == idArbitro1 || Convert.ToInt32(delegatiTorneo.Rows[i]["IDDelegato"]) == idArbitro2)
                        trovaArbitri = true;
                }
                if (trovaArbitri)//Se almeno un arbitro è stato trovato:
                {
                    sql = "";
                    sql += "UPDATE Partita " +
                        "SET IDArbitro1=@Arbitro1,IDArbitro2=@Arbitro2 " +
                        "WHERE IDTorneo=@IDTorneo AND NumPartita=@NumPartita";
                    comando = new SqlCommand(sql, conn);
                    if (idArbitro1 != 0)
                        comando.Parameters.Add(new SqlParameter("Arbitro1", idArbitro1));
                    else
                        comando.Parameters.Add(new SqlParameter("Arbitro1", DBNull.Value));
                    if (idArbitro2 != 0)
                        comando.Parameters.Add(new SqlParameter("Arbitro2", idArbitro2));
                    else
                        comando.Parameters.Add(new SqlParameter("Arbitro2", DBNull.Value));
                    comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                    comando.Parameters.Add(new SqlParameter("NumPartita", numPartita));
                    conn.Open();
                    comando.ExecuteNonQuery();
                    conn.Close();
                    return "Arbitri/o assegnati/o alla partita";
                }
                else
                    return "Nessuno dei due arbitri trovato tra quelli collegati al torneo";
            }
            catch (Exception e)
            {
                return "ERRORE: " + e.Message;
            }
        }
        private string AvanzaTabelloneQualifiche(int idTorneoQualifiche, int numPartita, int idTorneoPrincipale, int setsq1, int setsq2)
        {
            //Metodo per l'avanzamento nel tabellone di qualifiche
            //IMPORTANTE: QUESTO METODO VA RICHIAMATO CON LA FINE DELLA PARTITA
            string sql;
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable setPartitaFinita, numPartitaSuccessiva, infoSquadra, infoPartitaSuccessiva = new DataTable();
            try
            {
                //prendo idtorneopricipale
                sql = "";
                sql += "SELECT IDTorneo FROM Torneo WHERE Titolo=(SELECT SUBSTRING(Titolo, 0,((SELECT LEN(Titolo)FROM Torneo WHERE IDTorneo=@IDTorneo)-10)) FROM Torneo WHERE IDTorneo = @IDTorneo) ";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifiche));
                comando.Parameters.Add(new SqlParameter("NumPartita", numPartita));
                adapter = new SqlDataAdapter(comando);
                setPartitaFinita = new DataTable();
                conn.Open();
                adapter.Fill(setPartitaFinita);
                conn.Close();
                idTorneoPrincipale = Convert.ToInt32(setPartitaFinita.Rows[0][0]);
                //Prendo i set della partita in modo da vedere chi ha vinto e gli id delle squadre
                sql = "";
                sql += "SELECT SetSQ1,SetSQ2,IDSQ1,IDSQ2 FROM Partita WHERE IDTorneo=@IDTorneo AND NumPartita=@NumPartita";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifiche));
                comando.Parameters.Add(new SqlParameter("NumPartita", numPartita));
                adapter = new SqlDataAdapter(comando);
                setPartitaFinita = new DataTable();
                conn.Open();
                adapter.Fill(setPartitaFinita);
                conn.Close();
                //Aggiungo i punti alle 2 squadre
                //AddPuntiSquadre(idTorneoQualifiche, numPartita, Convert.ToInt32(setPartitaFinita.Rows[0]["IDSQ1"]), Convert.ToInt32(setPartitaFinita.Rows[0]["IDSQ2"]));
                //Prendo il NumPartitaSuccessiva
                sql = "";
                sql += "SELECT NumPartitaSuccessiva FROM Partita WHERE IDTorneo=@IDTorneo AND NumPartita=@NumPartita";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifiche));
                comando.Parameters.Add(new SqlParameter("NumPartita", numPartita));
                adapter = new SqlDataAdapter(comando);
                numPartitaSuccessiva = new DataTable();
                conn.Open();
                adapter.Fill(numPartitaSuccessiva);
                conn.Close();
                //Prendo i dati delle 2 squadre
                sql = "";
                sql += "SELECT * FROM Partecipa WHERE IDTorneo=@IDTorneo AND (IDSquadra=@IDSquadra1 OR IDSquadra=@IDSquadra2)";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifiche));
                comando.Parameters.Add(new SqlParameter("IDSquadra1", setPartitaFinita.Rows[0]["IDSQ1"]));
                comando.Parameters.Add(new SqlParameter("IDSquadra2", setPartitaFinita.Rows[0]["IDSQ2"]));
                adapter = new SqlDataAdapter(comando);
                infoSquadra = new DataTable();
                conn.Open();
                adapter.Fill(infoSquadra);
                conn.Close();
                //Prendo le info della partita successiva (se c'è)
                if (numPartitaSuccessiva.Rows[0]["NumPartitaSuccessiva"] != DBNull.Value)
                {
                    sql = "";
                    sql += "SELECT IDSQ1,IDSQ2 FROM Partita WHERE IDTorneo=@IDTorneo AND NumPartita=@NumPartitaSuccessiva";
                    comando = new SqlCommand(sql, conn);
                    comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifiche));
                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", numPartitaSuccessiva.Rows[0]["NumPartitaSuccessiva"]));
                    adapter = new SqlDataAdapter(comando);
                    conn.Open();
                    adapter.Fill(infoPartitaSuccessiva);
                    conn.Close();
                }
                //Controllo se c'è il 2 turno eliminatorio
                if (numPartitaSuccessiva.Rows[0]["NumPartitaSuccessiva"] != DBNull.Value)
                {
                    sql = "";
                    sql += "UPDATE Partita " +
                        "SET IDSQ1=@IDSQ1,IDSQ2=@IDSQ2 " +
                        "WHERE IDTorneo=@IDTorneo AND NumPartita=@NumPartitaSuccessiva";
                    comando = new SqlCommand(sql, conn);
                    //Controllo quale squadra ha vinto
                    if (setsq1 > setsq2)
                    {
                        if (numPartita % 2 != 0) //Se il NumPartita è dispari, la squadra vincente sarà la squadra 1 nella partita successiva
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ1", setPartitaFinita.Rows[0]["IDSQ1"]));
                            if (infoPartitaSuccessiva.Rows[0]["IDSQ2"] == DBNull.Value)
                                comando.Parameters.Add(new SqlParameter("IDSQ2", DBNull.Value));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ2", infoPartitaSuccessiva.Rows[0]["IDSQ2"]));
                        }
                        else
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ2", setPartitaFinita.Rows[0]["IDSQ1"]));
                            if (infoPartitaSuccessiva.Rows[0]["IDSQ1"] == DBNull.Value)
                                comando.Parameters.Add(new SqlParameter("IDSQ1", DBNull.Value));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ1", infoPartitaSuccessiva.Rows[0]["IDSQ1"]));
                        }
                    }
                    else
                    {
                        if (numPartita % 2 != 0) //Se il NumPartita è dispari, la squadra vincente sarà la squadra 1 nella partita successiva
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ1", setPartitaFinita.Rows[0]["IDSQ2"]));
                            if (infoPartitaSuccessiva.Rows[0]["IDSQ2"] == DBNull.Value)
                                comando.Parameters.Add(new SqlParameter("IDSQ2", DBNull.Value));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ2", infoPartitaSuccessiva.Rows[0]["IDSQ2"]));
                        }
                        else
                        {
                            comando.Parameters.Add(new SqlParameter("IDSQ2", setPartitaFinita.Rows[0]["IDSQ2"]));
                            if (infoPartitaSuccessiva.Rows[0]["IDSQ1"] == DBNull.Value)
                                comando.Parameters.Add(new SqlParameter("IDSQ1", DBNull.Value));
                            else
                                comando.Parameters.Add(new SqlParameter("IDSQ1", infoPartitaSuccessiva.Rows[0]["IDSQ1"]));
                        }
                    }
                    comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifiche));
                    comando.Parameters.Add(new SqlParameter("NumPartitaSuccessiva", numPartitaSuccessiva.Rows[0]["NumPartitaSuccessiva"]));
                    conn.Open();
                    comando.ExecuteNonQuery();
                    conn.Close();
                }
                else
                {
                    sql = "";
                    sql += "INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints)" +
                        "VALUES(@IDSquadra,@IDTorneo,@IDAllenatore,@EntryPoints)";
                    comando = new SqlCommand(sql, conn);
                    //Controllo quale squadra ha vinto
                    if (setsq1 > setsq2)
                    {
                        comando.Parameters.Add(new SqlParameter("IDSquadra", infoSquadra.Rows[0]["IDSquadra"]));
                        if (infoSquadra.Rows[0]["IDAllenatore"] != DBNull.Value)
                            comando.Parameters.Add(new SqlParameter("IDAllenatore", infoSquadra.Rows[0]["IDAllenatore"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDAllenatore", DBNull.Value));
                        comando.Parameters.Add(new SqlParameter("EntryPoints", infoSquadra.Rows[0]["EntryPoints"]));
                    }
                    else
                    {
                        comando.Parameters.Add(new SqlParameter("IDSquadra", infoSquadra.Rows[1]["IDSquadra"]));
                        if (infoSquadra.Rows[1]["IDAllenatore"] != DBNull.Value)
                            comando.Parameters.Add(new SqlParameter("IDAllenatore", infoSquadra.Rows[1]["IDAllenatore"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDAllenatore", DBNull.Value));
                        comando.Parameters.Add(new SqlParameter("EntryPoints", infoSquadra.Rows[1]["EntryPoints"]));
                    }
                    comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoPrincipale));
                    conn.Open();
                    comando.ExecuteNonQuery();
                    conn.Close();
                }
                return "Avanzamento avvenuto con successo";
            }
            catch (Exception e)
            {
                return "Errore: " + e.Message;
            }
        }
        public string GetStatoTornei(int idTorneo)
        {
            //Metodo che restituisce lo stato del torneo (rifiutato, accettato, in attesa)
            string sql;
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable query;
            try
            {
                sql = "";
                sql += "SELECT Autorizzato,Annullato FROM Torneo WHERE IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                query = new DataTable();
                conn.Open();
                adapter.Fill(query);
                conn.Close();
                if (Convert.ToInt32(query.Rows[0]["Autorizzato"]) == 1)
                    return "Torneo autorizzato";
                else if (Convert.ToInt32(query.Rows[0]["Annullato"]) == 1)
                    return "Torneo non autorizzato";
                else
                    return "Torneo in attesa";
            }
            catch (Exception exc)
            {
                return "Errore: " + exc.Message;
            }
        }
        public bool ControlloDataIscrizioni(int idTorneo)
        {
            //CONTROLLARE ANCHE CHE LE QUALIFICHE SIANO GIA PRONTE
            string sql;
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable result;
            sql = "";
            sql = "SELECT IDTorneo FROM Torneo WHERE IDTorneo = @IDTorneo AND DataChiusuraIscrizioni <= GETDATE()";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
            adapter = new SqlDataAdapter(comando);
            result = new DataTable();
            conn.Open();
            adapter.Fill(result);
            conn.Close();
            if (result.Rows.Count > 0) return true;
            else return false;
        }
        public bool ControlloInizioTorneo(int idTorneo)
        {
            string sql;
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable result;
            sql = "";
            sql = "SELECT IDTorneo FROM Torneo WHERE IDTorneo = @IDTorneo AND GETDATE() <= DataInizio";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
            adapter = new SqlDataAdapter(comando);
            result = new DataTable();
            conn.Open();
            adapter.Fill(result);
            conn.Close();
            if (result.Rows.Count > 0) return true;
            else return false;
        }
        public bool ControlloPresenzaTabellone(int idTorneo)
        {
            string sql;
            SqlDataAdapter adapter;
            SqlCommand comando;
            DataTable result;
            sql = "";
            sql = "SELECT IDTorneo FROM Partecipa WHERE Partecipa.IDTorneo=@IDTorneo";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
            adapter = new SqlDataAdapter(comando);
            result = new DataTable();
            conn.Open();
            adapter.Fill(result);
            conn.Close();
            if (result.Rows.Count > 0) return true;
            else return false;
        }
        public bool CreaPool(int idTorneo)
        {
            string[,] matrice;
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            DataTable dtb;
            try
            {
                //pulisco i pool se c'è qualche dato sporco
                string query = "DELETE FROM Pool WHERE IDTorneo=@IDTorneo";
                conn.Open();
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                SqlDataAdapter da = new SqlDataAdapter(command);
                dtb = new DataTable();
                da.Fill(dtb);
                conn.Close();

                query = "SELECT IDSquadra FROM Partecipa WHERE IDTorneo=@IDTorneo order by Entrypoints DESC";
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                da = new SqlDataAdapter(command);
                dtb = new DataTable();
                da.Fill(dtb);
                conn.Close();

                matrice = new string[4, (dtb.Rows.Count / 4)];//creo matrice per pool
                int row = 0;
                int counter = 0;
                //riempio matrice
                for (int i = 0; i <= matrice.GetLength(1); i++)
                {
                    if (row == 4) break;
                    if (i == (dtb.Rows.Count / 4))//se è arrivato in fondo cambio giro
                    {
                        i = -1;//resetto il counter per giro successivo
                        row++;//incremento riga
                        for (int k = matrice.GetLength(1) - 1; k >= 0; k--)
                        {
                            matrice[row, k] = dtb.Rows[counter][0].ToString();
                            counter++;
                        }
                        row++;//incremento riga
                    }
                    else
                    {
                        matrice[row, i] = dtb.Rows[counter][0].ToString();
                        counter++;
                    }
                }
                int alphacount = 0;
                for (int i = 0; i < matrice.GetLength(1); i++)
                {
                    for (int j = 0; j < matrice.GetLength(0); j++)
                    {
                        query = "INSERT INTO Pool (IdPool,IdSquadra,IdTorneo) VALUES('" + alpha[alphacount] + "'," + Convert.ToInt32(matrice[j, i]) + ",@IDTorneo)";
                        conn.Open();
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                        da = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        conn.Close();
                    }
                    alphacount++;
                }
                //creo prime partite pool
                CreaPartitePool(matrice, dtb, idTorneo);
                return true;
            }
            catch (Exception e)
            {
                //elimino i pool se qualcosa è andato storto
                string query = "DELETE FROM Pool WHERE IDTorneo=@IDTorneo";
                conn.Open();
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                SqlDataAdapter da = new SqlDataAdapter(command);
                dtb = new DataTable();
                da.Fill(dtb);
                conn.Close();
                return false;
            }
        }
        private bool CreaPartitePool(string[,] matrice, DataTable dtb, int idtorneo)
        {
            try
            {
                string query;
                int numpartita = 0;
                for (int i = 0; i < matrice.GetLength(1); i++)
                {
                    //1vs4
                    numpartita++;
                    DataRow[] team1 = dtb.Select("IDSquadra='" + matrice[0, i].ToString() + "'");
                    DataRow[] team2 = dtb.Select("IDSquadra='" + matrice[3, i].ToString() + "'");
                    query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDSQ1,IDSQ2,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@sq1,@sq2,@IDTorneo,@num);";
                    conn.Open();
                    SqlCommand command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO
                    command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                    command.Parameters.Add(new SqlParameter("Pool", "Pool " + Convert.ToInt32(dtb.Rows.IndexOf(team1[0]) + 1).ToString() + "vs" + Convert.ToInt32(dtb.Rows.IndexOf(team2[0]) + 1).ToString()));
                    command.Parameters.Add(new SqlParameter("sq1", matrice[0, i].ToString()));
                    command.Parameters.Add(new SqlParameter("sq2", matrice[3, i].ToString()));
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", numpartita));
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    command.ExecuteNonQuery();
                    conn.Close();

                    //2vs3
                    numpartita++;
                    team1 = dtb.Select("IDSquadra='" + matrice[1, i].ToString() + "'");
                    team2 = dtb.Select("IDSquadra='" + matrice[2, i].ToString() + "'");
                    query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDSQ1,IDSQ2,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@sq1,@sq2,@IDTorneo,@num);";
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));
                    command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                    command.Parameters.Add(new SqlParameter("Pool", "Pool " + Convert.ToInt32(dtb.Rows.IndexOf(team1[0]) + 1).ToString() + "vs" + Convert.ToInt32(dtb.Rows.IndexOf(team2[0]) + 1).ToString()));
                    command.Parameters.Add(new SqlParameter("sq1", matrice[1, i].ToString()));
                    command.Parameters.Add(new SqlParameter("sq2", matrice[2, i].ToString()));
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", numpartita));
                    da = new SqlDataAdapter(command);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                //elimino i pool se qualcosa è andato storto
                string query = "DELETE FROM Partita WHERE IDTorneo=@IDTorneo AND Fase LIKE '%Pool%' AND Fase LIKE '%vs%'";
                conn.Open();
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                SqlDataAdapter da = new SqlDataAdapter(command);
                dtb = new DataTable();
                da.Fill(dtb);
                conn.Close();
                return false;
            }
        }
        private bool calcPunteggiPool(int idtorneo)
        {
            DataTable dtb;
            try
            {
                conn.Open();
                //pulisco tabella pool
                string query = "UPDATE Pool SET PF=0,PS=0,QP=0,PP=0 WHERE IdTorneo=@IDTorneo";
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                command.ExecuteNonQuery();
                conn.Close();
                //scarico partite
                conn.Open();
                query = "SELECT IDSQ1,IDSQ2,CONCAT(PT1S1,'-',PT2S1,',',PT1S2,'-',PT2S2,',',PT1S3,'-',PT2S3)AS Punteggio,Fase,Risultato FROM Partita WHERE IDTorneo=@IDTorneo AND Fase LIKE '%Pool%'";
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                dtb = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dtb);
                conn.Close();
                for (int i = 0; i < dtb.Rows.Count; i++)
                {
                    conn.Open();
                    string[] punti = dtb.Rows[i][2].ToString().Split(',', '-');
                    int P_team1 = 0;
                    int P_team2 = 0;
                    //calcolo i punti subiti e punti fatti
                    for (int counter = 0; counter < punti.Length; counter++)
                    {//nelle posizioni pari ci sono i punti del TEAM 1 in quelle dispari del TEAM 2
                        if (counter % 2 == 0) P_team1 += Convert.ToInt32(punti[counter]);
                        else P_team2 += Convert.ToInt32(punti[counter]);
                    }
                    SqlCommand command1;
                    //aggiorno punti squadra 1
                    if (!dtb.Rows[i][3].ToString().Contains("Loser") || (dtb.Rows[i][3].ToString().Contains("Loser") && P_team1 > P_team2))
                    {

                        query = "UPDATE Pool SET PF= PF+ @pt1,PS= PS+ @pt2 WHERE IdSquadra= @squadra AND IdTorneo=@idtorneo";
                        command1 = new SqlCommand(query, conn);
                        command1.Parameters.Add(new SqlParameter("pt1", P_team1));
                        command1.Parameters.Add(new SqlParameter("pt2", P_team2));
                        command1.Parameters.Add(new SqlParameter("idtorneo", idtorneo));
                        command1.Parameters.Add(new SqlParameter("squadra", Convert.ToInt32(dtb.Rows[i][0])));
                        command1.ExecuteNonQuery();

                    }
                    //aggiorno punti squadra 2
                    if (!dtb.Rows[i][3].ToString().Contains("Loser") || (dtb.Rows[i][3].ToString().Contains("Loser") && P_team2 > P_team1))// controllo se non è la partita LOSER 
                    {
                        query = "UPDATE Pool SET PF= PF+ @pt2,PS= PS+ @pt1 WHERE IdSquadra= @squadra AND IdTorneo=@idtorneo";
                        command1 = new SqlCommand(query, conn);
                        command1.Parameters.Add(new SqlParameter("pt1", P_team1));
                        command1.Parameters.Add(new SqlParameter("pt2", P_team2));
                        command1.Parameters.Add(new SqlParameter("idtorneo", idtorneo));
                        command1.Parameters.Add(new SqlParameter("squadra", Convert.ToInt32(dtb.Rows[i][1])));
                        command1.ExecuteNonQuery();
                    }
                    int PP = 0;
                    string[] risultato = dtb.Rows[i][4].ToString().Split('-');//prendo risultato
                    //se è la prima partita do 5 punti altrimenti 3 in modo da avere una classifica sulle partite vinte
                    if (dtb.Rows[i][3].ToString().Contains("Loser") || dtb.Rows[i][3].ToString().Contains("Winner")) PP = 3;
                    else if (!dtb.Rows[i][3].ToString().Contains("Loser") && !dtb.Rows[i][3].ToString().Contains("Winner") && dtb.Rows[i][3].ToString().Contains("Pool")) PP = 5;
                    //se ha vinto la squadra 1
                    if (Convert.ToInt32(risultato[0]) > Convert.ToInt32(risultato[1]))
                    {
                        query = "UPDATE Pool SET PP= PP+ @pp WHERE IdSquadra= @squadra AND IdTorneo=@idtorneo";
                        command1 = new SqlCommand(query, conn);
                        command1.Parameters.Add(new SqlParameter("pp", PP));
                        command1.Parameters.Add(new SqlParameter("idtorneo", idtorneo));
                        command1.Parameters.Add(new SqlParameter("squadra", Convert.ToInt32(dtb.Rows[i][0])));
                        command1.ExecuteNonQuery();
                    }
                    //squadra 2
                    else if (Convert.ToInt32(risultato[0]) < Convert.ToInt32(risultato[1]))
                    {
                        query = "UPDATE Pool SET PP= PP+ @pp WHERE IdSquadra= @squadra AND IdTorneo=@idtorneo";
                        command1 = new SqlCommand(query, conn);
                        command1.Parameters.Add(new SqlParameter("pp", PP));
                        command1.Parameters.Add(new SqlParameter("idtorneo", idtorneo));
                        command1.Parameters.Add(new SqlParameter("squadra", Convert.ToInt32(dtb.Rows[i][1])));
                        command1.ExecuteNonQuery();
                    }
                    conn.Close();
                }
                //calcolo QP
                query = "UPDATE Pool SET QP = CASE WHEN PS = 0 THEN PF ELSE cast((cast(PF as float)/ cast(PS as float)) as float) END WHERE IdTorneo=@idtorneo;";
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("idtorneo", idtorneo));
                command.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                conn.Close();
                return false;
            }
        }
        public bool CreatePoolWinLose(int idtorneo)
        {
            try
            {
                string query = "SELECT * FROM POOL WHERE idTorneo=@idtorneo ORDER BY IdPool ASC, PP DESC;";//prendo le pool in ordine di punti per fare pool vincente e perdente
                conn.Open();
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("idtorneo", idtorneo));
                DataTable dtb = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dtb);
                conn.Close();
                int numpartita = GetLastNumberPartita(idtorneo);
                for (int i = 0; i < dtb.Rows.Count; i += 4)
                {
                    numpartita++;//incremetno numero partita
                    conn.Open();
                    query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDSQ1,IDSQ2,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@sq1,@sq2,@IDTorneo,@num);";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO?
                    command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                    command.Parameters.Add(new SqlParameter("Pool", "Pool " + dtb.Rows[i][0].ToString() + " Winner"));
                    command.Parameters.Add(new SqlParameter("sq1", dtb.Rows[i][1].ToString()));
                    command.Parameters.Add(new SqlParameter("sq2", dtb.Rows[i + 1][1].ToString()));
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", numpartita));
                    da = new SqlDataAdapter(command);
                    command.ExecuteNonQuery();
                    numpartita++;//incremetno numero partita
                    query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDSQ1,IDSQ2,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@sq1,@sq2,@IDTorneo,@num);";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO?
                    command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                    command.Parameters.Add(new SqlParameter("Pool", "Pool " + dtb.Rows[i][0].ToString() + " Loser"));
                    command.Parameters.Add(new SqlParameter("sq1", dtb.Rows[i + 3][1].ToString()));
                    command.Parameters.Add(new SqlParameter("sq2", dtb.Rows[i + 2][1].ToString()));
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", numpartita));
                    da = new SqlDataAdapter(command);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
                return true;
            }
            catch (Exception e)
            { return false; }
        }
        private int GetLastNumberPartita(int idtorneo)
        {
            string query = "SELECT TOP(1) NumPartita FROM Partita WHERE IDTorneo=@IDTorneo ORDER BY NumPartita DESC";
            conn.Open();
            SqlCommand command = new SqlCommand(query, conn);
            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
            DataTable dtb = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dtb);
            conn.Close();
            return Convert.ToInt32(dtb.Rows[0][0]);
        }
        public bool CreaOttaviSedicesimi(int idtorneo)
        {
            string query = "SELECT COUNT(*) FROM Partecipa WHERE idTorneo=@idtorneo";//numero team
            conn.Open();
            SqlCommand command = new SqlCommand(query, conn);
            command.Parameters.Add(new SqlParameter("idtorneo", idtorneo));
            DataTable dtb = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dtb);
            string numerot = dtb.Rows[0][0].ToString();
            conn.Close();
            if (numerot == "12")
            {
                try
                {
                    ArrayList idPoolEstratti = new ArrayList();
                    Random rnd = new Random();
                    char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
                    //prendo numero pool
                    query = "SELECT COUNT(DISTINCT(IdPool))as num From pool WHERE IdTorneo=@IDTorneo";
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    int numPool = Convert.ToInt32(dtb.Rows[0][0]); //numero pool
                    conn.Close();
                    int numpartita = GetLastNumberPartita(idtorneo);
                    //inserimento delle partite di pool
                    for (int i = 0; i < 1; i++)
                    {
                        numpartita++;
                        query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num);";
                        conn.Open();
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO
                        command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                        command.Parameters.Add(new SqlParameter("Pool", "Ottavi"));
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        command.Parameters.Add(new SqlParameter("num", numpartita));
                        da = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        conn.Close();
                    }
                    string[] tabellone = new string[Convert.ToInt32((Convert.ToInt32(numerot) - numPool))];
                    //set prime 4 squadre a caso 
                    query = "SELECT * FROM Partita WHERE Fase='Ottavi' AND (IDSQ1 IS NULL AND IDSQ2 IS NULL) AND IdTorneo=@IDTorneo";
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    DataTable ottavi = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(ottavi);
                    conn.Close();
                    query = "SELECT IdSquadra FROM Pool WHERE IDTorneo=@IDTorneo AND PP=8 order by QP DESC";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    tabellone[0] = dtb.Rows[0][0].ToString();
                    tabellone[1] = dtb.Rows[1][0].ToString();
                    tabellone[2] = dtb.Rows[2][0].ToString();
                    //BARARE
                    string temp = tabellone[2];
                    tabellone[2] = tabellone[1];
                    tabellone[1] = temp;
                    string[] team2 = new string[3];
                    query = "SELECT IdSquadra FROM Pool WHERE IDTorneo=@IDTorneo AND PP=5 order by QP DESC";//scarico 2 team
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    tabellone[3] = dtb.Rows[0][0].ToString();
                    tabellone[4] = dtb.Rows[1][0].ToString();
                    tabellone[5] = dtb.Rows[2][0].ToString();
                    query = "SELECT IdSquadra FROM Pool WHERE IDTorneo=@IDTorneo AND PP=3 order by QP DESC";//scarico 2 team
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    tabellone[6] = dtb.Rows[0][0].ToString();
                    tabellone[7] = dtb.Rows[1][0].ToString();
                    tabellone[8] = dtb.Rows[2][0].ToString();
                    query = "SELECT TOP(1) NumPartita FROM Partita WHERE IdTorneo=@IDTorneo AND Fase='Ottavi'";//numpartita
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    string nump = dtb.Rows[0][0].ToString();
                    conn.Close();
                    conn.Open();
                    query = "UPDATE Partita SET IDSQ1=@idteam1,IDSQ2=@idteam2 WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@numpartita";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("idteam1", tabellone[7]));
                    command.Parameters.Add(new SqlParameter("idteam2", tabellone[8]));
                    command.Parameters.Add(new SqlParameter("numpartita", nump));
                    command.ExecuteNonQuery();
                    conn.Close();
                    //if (!CreaPartiteRestanti(idtorneo)) throw new Exception();//se torna false lo mando in errore
                    try
                    {
                        numpartita = Convert.ToInt32(nump);
                        //INSERT QUARTI
                        for (int i = 0; i < 4; i++)
                        {
                            numpartita++;
                            conn.Open();
                            query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num);";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO?
                            command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                            command.Parameters.Add(new SqlParameter("Pool", "Quarti"));
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpartita));
                            da = new SqlDataAdapter(command);
                            command.ExecuteNonQuery();
                            conn.Close();
                        }
                        //SETTO NUMERO PARTITA SUCCESSIVA OTTAVI
                        int numsuc = GetLastNumberPartita(idtorneo);
                        conn.Open();
                        query = "UPDATE Partita SET NumPartitaSuccessiva=@num WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo";
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        command.Parameters.Add(new SqlParameter("num", numsuc));
                        command.ExecuteNonQuery();
                        conn.Close();
                        //SELECT QUARTI
                        query = "SELECT NumPartita FROM Partita WHERE Fase='Quarti' AND IdTorneo=@IDTorneo"; //prendo squadre quarti
                        conn.Open();
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        da = new SqlDataAdapter(command);
                        dtb = new DataTable();
                        da.Fill(dtb);
                        conn.Close();
                        int index = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam1,IDSQ2=@idteam2 WHERE Fase='Quarti' AND IdTorneo=@IDTorneo AND NumPartita=@numpartita";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("idteam1", tabellone[index]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[index + 1]));
                            command.Parameters.Add(new SqlParameter("numpartita", dtb.Rows[i][0]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            index += 2;
                        }
                        numpartita = GetLastNumberPartita(idtorneo);
                        //INSERT SEMIFINALI
                        for (int i = 0; i < 2; i++)
                        {
                            numpartita++;
                            conn.Open();
                            query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num);";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO?
                            command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                            command.Parameters.Add(new SqlParameter("Pool", "Semifinali"));
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpartita));
                            da = new SqlDataAdapter(command);
                            command.ExecuteNonQuery();
                            conn.Close();
                        }
                        //prendo numpartita quarti
                        query = "SELECT TOP(1) NumPartita FROM Partita WHERE Fase='Quarti' AND IdTorneo=@IDTorneo"; //prendo squadre quarti
                        conn.Open();
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        da = new SqlDataAdapter(command);
                        dtb = new DataTable();
                        da.Fill(dtb);
                        conn.Close();
                        int xindex = Convert.ToInt32(dtb.Rows[0][0].ToString());
                        for (int i = 0; i < 4; i++)
                        {
                            if (i < 2) numsuc = (GetLastNumberPartita(idtorneo) - 1);
                            else numsuc = GetLastNumberPartita(idtorneo);
                            //SETTO NUMERO PARTITA SUCCESSIVA QUARTI
                            conn.Open();
                            query = "UPDATE Partita SET NumPartitaSuccessiva=@num WHERE Fase='Quarti' AND IdTorneo=@IDTorneo AND NumPartita=@nump";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numsuc));
                            command.Parameters.Add(new SqlParameter("nump", xindex));
                            xindex++;
                            command.ExecuteNonQuery();
                            conn.Close();
                        }
                        //SELECT SEMIFINALI
                        query = "SELECT IDPartita,IDTorneo,NumPartita,NumPartitaSuccessiva FROM Partita WHERE Fase='Semifinali' AND IdTorneo=@IDTorneo"; //prendo squadre semifinali
                        conn.Open();
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        da = new SqlDataAdapter(command);
                        dtb = new DataTable();
                        da.Fill(dtb);
                        conn.Close();
                        //SET NUMPARTITASUCCESSIVA
                        SetPartiteSuccessive(idtorneo, 2, dtb);//set numero partite successive               
                        numpartita = GetLastNumberPartita(idtorneo);
                        numpartita++;
                        //CREO FINALI
                        conn.Open();
                        query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num);";
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO?
                        command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                        command.Parameters.Add(new SqlParameter("Pool", "Finale 3/4"));
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        command.Parameters.Add(new SqlParameter("num", numpartita));
                        da = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        conn.Close();
                        numpartita++;
                        conn.Open();
                        query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num);";
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO?
                        command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                        command.Parameters.Add(new SqlParameter("Pool", "Finale 1/2"));
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        command.Parameters.Add(new SqlParameter("num", numpartita));
                        da = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                    catch
                    {
                        conn.Close();
                        return false;
                    }
                    return true;
                }
                catch (Exception e)
                {
                    query = "DELETE From Partita WHERE IdTorneo=@IDTorneo AND (Fase=@fase OR Fase=@fase1)";
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("fase", "Ottavi"));
                    command.Parameters.Add(new SqlParameter("fase1", "Sedicesimi"));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    return false;
                }
            }
            else if (numerot == "16")
            {
                try
                {
                    ArrayList idPoolEstratti = new ArrayList();
                    Random rnd = new Random();
                    char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
                    //prendo numero pool
                    query = "SELECT COUNT(DISTINCT(IdPool))as num From pool WHERE IdTorneo=@IDTorneo";
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    int numPool = Convert.ToInt32(dtb.Rows[0][0]); //numero pool
                    conn.Close();
                    int numpartita = GetLastNumberPartita(idtorneo);
                    //inserimento delle partite di pool
                    for (int i = 0; i < ((Convert.ToInt32(numerot) - numPool) / 2); i++)
                    {
                        numpartita++;
                        query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num);";
                        conn.Open();
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO
                        command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                        command.Parameters.Add(new SqlParameter("Pool", "Ottavi"));
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        command.Parameters.Add(new SqlParameter("num", numpartita));
                        da = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        conn.Close();
                    }
                    string[] tabellone = new string[Convert.ToInt32((Convert.ToInt32(numerot) - numPool))];
                    //set prime 4 squadre a caso 
                    query = "SELECT * FROM Partita WHERE Fase='Ottavi' AND (IDSQ1 IS NULL AND IDSQ2 IS NULL) AND IdTorneo=@IDTorneo";
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    DataTable ottavi = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(ottavi);
                    conn.Close();
                    for (int i = 0; i < numPool; i++) //id partita nella tabella e prendo id partita/squadra 
                    {
                        query = "SELECT IdSquadra FROM Pool WHERE QP IN (SELECT MAX(QP)From Pool WHERE IdPool=@pool) AND IdTorneo=@IDTorneo AND IdPool=@pool";//scarico 1 team di quel pool
                        conn.Open();
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        command.Parameters.Add(new SqlParameter("pool", alpha[i].ToString()));
                        dtb = new DataTable();
                        da = new SqlDataAdapter(command);
                        da.Fill(dtb);
                        string team = dtb.Rows[0][0].ToString();
                        conn.Close();
                        tabellone[i] = team;
                    }
                    string[] team2 = new string[4];
                    for (int i = 0; i < numPool; i++) //id partita nella tabella e prendo id partita/squadra 
                    {
                        query = "SELECT IdSquadra FROM Pool WHERE IdTorneo=@IDTorneo AND IdPool=@pool AND PP=5";//scarico 2 team
                        conn.Open();
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        command.Parameters.Add(new SqlParameter("pool", alpha[i].ToString()));
                        dtb = new DataTable();
                        da = new SqlDataAdapter(command);
                        da.Fill(dtb);
                        team2[i] = dtb.Rows[0][0].ToString();
                        conn.Close();
                    }
                    team2 = team2.OrderBy(x => rnd.Next()).ToArray();
                    tabellone[7] = team2[0];
                    tabellone[4] = team2[1];
                    tabellone[5] = team2[2];
                    tabellone[6] = team2[3];
                    for (int i = 0; i < numPool; i++) //id partita nella tabella e prendo id partita/squadra 
                    {
                        query = "SELECT IdSquadra FROM Pool WHERE IdTorneo=@IDTorneo AND IdPool=@pool AND PP=3";//scarico 2 team
                        conn.Open();
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        command.Parameters.Add(new SqlParameter("pool", alpha[i].ToString()));
                        dtb = new DataTable();
                        da = new SqlDataAdapter(command);
                        da.Fill(dtb);
                        team2[i] = dtb.Rows[0][0].ToString();
                        conn.Close();
                    }
                    team2 = team2.OrderBy(x => rnd.Next()).ToArray();
                    tabellone[8] = team2[0];
                    tabellone[11] = team2[1];
                    tabellone[10] = team2[2];
                    tabellone[9] = team2[3];
                    query = "SELECT TOP(1) NumPartita FROM Partita WHERE IdTorneo=@IDTorneo AND Fase='Ottavi'";//numpartita
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    string nump = dtb.Rows[0][0].ToString();
                    conn.Close();
                    int index = 0;
                    int last = Convert.ToInt32(nump) + ((Convert.ToInt32(numerot) - numPool) / 2);
                    for (int i = Convert.ToInt32(nump); i < last; i++)
                    {
                        conn.Open();
                        query = "UPDATE Partita SET IDSQ1=@idteam1,IDSQ2=@idteam2 WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@numpartita";
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        command.Parameters.Add(new SqlParameter("idteam1", tabellone[index]));
                        command.Parameters.Add(new SqlParameter("idteam2", tabellone[index + 1]));
                        command.Parameters.Add(new SqlParameter("numpartita", i));
                        command.ExecuteNonQuery();
                        conn.Close();
                        index += 2;
                    }
                    if (!CreaPartiteRestanti(idtorneo)) throw new Exception();//se torna false lo mando in errore
                    return true;
                }
                catch (Exception e)
                {
                    query = "DELETE From Partita WHERE IdTorneo=@IDTorneo AND (Fase=@fase OR Fase=@fase1)";
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("fase", "Ottavi"));
                    command.Parameters.Add(new SqlParameter("fase1", "Sedicesimi"));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    return false;
                }
            }
            else
            {
                try
                {
                    //Creo ottavi
                    if (!CreaOttavi(idtorneo)) throw new Exception();//se torna false lo mando in errore
                                                                     //Creo Sedicesimi
                    if (!CreaSedicesimi(idtorneo)) throw new Exception();//se torna false lo mando in errore
                                                                         //elimino gli ottavi
                    query = "DELETE From Partita WHERE IdTorneo=@IDTorneo AND Fase=@fase";
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("fase", "Ottavi"));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    //RiCreo ottavi in ordine
                    if (!CreaOttavi(idtorneo)) throw new Exception();//se torna false lo mando in errore
                    if (!CreaPartiteRestanti(idtorneo)) throw new Exception();//se torna false lo mando in errore
                    return true;
                }
                catch (Exception e)
                {
                    query = "DELETE From Partita WHERE IdTorneo=@IDTorneo AND (Fase=@fase OR Fase=@fase1)";
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("fase", "Ottavi"));
                    command.Parameters.Add(new SqlParameter("fase1", "Sedicesimi"));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    return false;
                }
            }
        }
        private bool CreaOttavi(int idtorneo)
        {
            try
            {
                ArrayList idPoolEstratti = new ArrayList();
                Random rnd = new Random();
                char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
                //prendo numero pool
                string query = "SELECT COUNT(DISTINCT(IdPool))as num From pool WHERE IdTorneo=@IDTorneo";
                conn.Open();
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                DataTable dtb = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dtb);
                int numPool = Convert.ToInt32(dtb.Rows[0][0]); //numero pool
                conn.Close();
                int numpartita = GetLastNumberPartita(idtorneo);
                //PRENDO NUMERO TEAM TOTALI
                query = "SELECT COUNT(*) FROM Partecipa WHERE IdTorneo=@IDTorneo";
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                conn.Close();
                int teamTotali = Convert.ToInt32(dtb.Rows[0][0]); //numero pool

                //IN BASE AI TEAM CREO GLI OTTAVI
                if (teamTotali == 8)
                {
                    //creo tabellone
                    //FINCHE NON SI INCOTRANO DUE SQUADRE DEI POOL IN SEMIFINALE
                    List<string> tabellone = CreoTabellone(teamTotali, numPool, idtorneo);
                    //CREO QUARTI
                    CreaPartiteRestanti(idtorneo);
                    query = "SELECT NumPartita FROM Partita WHERE IdTorneo=@IDTorneo AND Fase='Quarti'";
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    conn.Open();
                    query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Quarti' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", dtb.Rows[0][0]));
                    command.Parameters.Add(new SqlParameter("idteam", tabellone[0]));
                    command.Parameters.Add(new SqlParameter("idteam2", tabellone[7]));
                    command.ExecuteNonQuery();
                    conn.Close();
                    conn.Open();
                    query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Quarti' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", dtb.Rows[1][0]));
                    command.Parameters.Add(new SqlParameter("idteam", tabellone[4]));
                    command.Parameters.Add(new SqlParameter("idteam2", tabellone[3]));
                    command.ExecuteNonQuery();
                    conn.Close();
                    conn.Open();
                    query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Quarti' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", dtb.Rows[2][0]));
                    command.Parameters.Add(new SqlParameter("idteam", tabellone[2]));
                    command.Parameters.Add(new SqlParameter("idteam2", tabellone[5]));
                    command.ExecuteNonQuery();
                    conn.Close();
                    conn.Open();
                    query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Quarti' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", dtb.Rows[3][0]));
                    command.Parameters.Add(new SqlParameter("idteam", tabellone[6]));
                    command.Parameters.Add(new SqlParameter("idteam2", tabellone[1]));
                    command.ExecuteNonQuery();
                    conn.Close();
                }
                else if (teamTotali == 12)
                {
                    //creo tabellone
                    //CONTROLLO SQUADRE NELLE FASI SUCCESSIVE
                    List<string> tabellone = CreoTabellone(teamTotali, numPool, idtorneo);
                    //creo un solo ottavo
                    numpartita++;
                    query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num);";
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO
                    command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                    command.Parameters.Add(new SqlParameter("Pool", "Ottavi"));
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", numpartita));
                    da = new SqlDataAdapter(command);
                    command.ExecuteNonQuery();
                    conn.Close();
                    //SET OTTAVO 8v9
                    conn.Open();
                    query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", numpartita));
                    command.Parameters.Add(new SqlParameter("numsuc", numpartita + 1));
                    command.Parameters.Add(new SqlParameter("idteam", tabellone[7]));
                    command.Parameters.Add(new SqlParameter("idteam2", tabellone[8]));
                    command.ExecuteNonQuery();
                    conn.Close();
                    //CREO QUARTI
                    CreaPartiteRestanti(idtorneo);
                    query = "SELECT NumPartita FROM Partita WHERE IdTorneo=@IDTorneo AND Fase='Quarti'";
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    conn.Open();
                    query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Quarti' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", dtb.Rows[0][0]));
                    command.Parameters.Add(new SqlParameter("idteam", tabellone[0]));
                    command.Parameters.Add(new SqlParameter("idteam2", DBNull.Value));
                    command.ExecuteNonQuery();
                    conn.Close();
                    conn.Open();
                    query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Quarti' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", dtb.Rows[1][0]));
                    command.Parameters.Add(new SqlParameter("idteam", tabellone[4]));
                    command.Parameters.Add(new SqlParameter("idteam2", tabellone[3]));
                    command.ExecuteNonQuery();
                    conn.Close();
                    conn.Open();
                    query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Quarti' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", dtb.Rows[2][0]));
                    command.Parameters.Add(new SqlParameter("idteam", tabellone[2]));
                    command.Parameters.Add(new SqlParameter("idteam2", tabellone[5]));
                    command.ExecuteNonQuery();
                    conn.Close();
                    conn.Open();
                    query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Quarti' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", dtb.Rows[3][0]));
                    command.Parameters.Add(new SqlParameter("idteam", tabellone[6]));
                    command.Parameters.Add(new SqlParameter("idteam2", tabellone[1]));
                    command.ExecuteNonQuery();
                    conn.Close();
                }
                else if (teamTotali > 12)
                {
                    List<string> tabellone = new List<string>();
                    do
                    {
                        int numpFisrtottavi = GetLastNumberPartita(idtorneo);
                        //creo tabellone
                        //CONTROLLO SQUADRE NELLE FASI SUCCESSIVE
                        tabellone = CreoTabellone(teamTotali, numPool, idtorneo);
                        if (teamTotali == 32)
                        {
                            //creo 8 Sedicesimi come di default
                            for (int i = 0; i < 8; i++)
                            {
                                numpartita++;
                                query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita,NumPartitaSuccessiva) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num,@numsucc);";
                                conn.Open();
                                command = new SqlCommand(query, conn);
                                command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO
                                command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                                command.Parameters.Add(new SqlParameter("Pool", "Sedicesimi"));
                                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                                command.Parameters.Add(new SqlParameter("num", numpartita));
                                command.Parameters.Add(new SqlParameter("numsucc", numpartita + 8));//SISTEMARE 
                                da = new SqlDataAdapter(command);
                                command.ExecuteNonQuery();
                                conn.Close();
                            }
                        }
                        else if (teamTotali == 24)
                        {
                            //creo 2 Sedicesimi PRETURNI
                            for (int i = 0; i < 2; i++)
                            {
                                numpartita++;
                                query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita,NumPartitaSuccessiva) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num,@numsucc);";
                                conn.Open();
                                command = new SqlCommand(query, conn);
                                command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO
                                command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                                command.Parameters.Add(new SqlParameter("Pool", "Sedicesimi"));
                                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                                command.Parameters.Add(new SqlParameter("num", numpartita));
                                if(i==0)
                                command.Parameters.Add(new SqlParameter("numsucc", numpartita + 2));
                                else if(i==1)
                                command.Parameters.Add(new SqlParameter("numsucc", numpartita + 9));
                                da = new SqlDataAdapter(command);
                                command.ExecuteNonQuery();
                                conn.Close();
                            }
                        }
                        //creo 8 ottavi come di default
                        for (int i = 0; i < 8; i++)
                        {
                            numpartita++;
                            query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num);";
                            conn.Open();
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO
                            command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                            command.Parameters.Add(new SqlParameter("Pool", "Ottavi"));
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpartita));
                            da = new SqlDataAdapter(command);
                            command.ExecuteNonQuery();
                            conn.Close();
                        }
                        if (teamTotali == 16)
                        {
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 8));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[0]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[15]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 7));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[7]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[8]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 7));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[4]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[11]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 6));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[3]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[12]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 6));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[2]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[13]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 5));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[5]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[10]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 5));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[6]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[9]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 4));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[1]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[14]));
                            command.ExecuteNonQuery();
                            conn.Close();
                        }
                        else if (teamTotali == 24)
                        {
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Sedicesimi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[14]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[17]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Sedicesimi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[15]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[16]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 8));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[0]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 7));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[7]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[8]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 7));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[4]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[11]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 6));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[3]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[12]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 6));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[2]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[13]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 5));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[5]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[10]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 5));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[6]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[9]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 4));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[1]));
                            command.ExecuteNonQuery();
                            conn.Close();
                        }
                        else if (teamTotali == 32)
                        {
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Sedicesimi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[15]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[16]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Sedicesimi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[8]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[23]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Sedicesimi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[11]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[20]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Sedicesimi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[12]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[19]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Sedicesimi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[13]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[18]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Sedicesimi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[10]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[21]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Sedicesimi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[9]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[22]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,IDSQ2=@idteam2 WHERE Fase='Sedicesimi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[14]));
                            command.Parameters.Add(new SqlParameter("idteam2", tabellone[17]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            //----------OTTAVI-------
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 8));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[0]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 7));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[7]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 7));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[4]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 6));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[3]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 6));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[2]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 5));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[5]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 5));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[6]));
                            command.ExecuteNonQuery();
                            conn.Close();
                            numpFisrtottavi++;
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ1=@idteam,NumPartitaSuccessiva=@numsuc WHERE Fase='Ottavi' AND IdTorneo=@IDTorneo AND NumPartita=@num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            command.Parameters.Add(new SqlParameter("num", numpFisrtottavi));
                            command.Parameters.Add(new SqlParameter("numsuc", numpFisrtottavi + 4));
                            command.Parameters.Add(new SqlParameter("idteam", tabellone[1]));
                            command.ExecuteNonQuery();
                            conn.Close();
                        }
                    } while (!ControlFasi("Ottavi", numPool, idtorneo, tabellone));
                    CreaPartiteRestanti(idtorneo);//CREO QUARTI-SEMI-FIN
                }
                return true;
            }
            catch
            {
                conn.Close();
                return false;
            }
        }
        private bool ControlFasi(string fase, int numpool, int idtorneo, List<string> tabe)
        {
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            DataColumn[] keyColumns = new DataColumn[1];
            if (fase == "Ottavi")
            {
                for (int i = 0; i < (tabe.Count / 2); i += 2)
                {
                    for (int k = 0; k < numpool; k++)
                    {
                        //PRENDO Team per pool
                        string query = "SELECT IDSquadra FROM Pool WHERE IdTorneo=@IDTorneo AND IDPool=@pool";
                        conn.Open();
                        SqlCommand command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        command.Parameters.Add(new SqlParameter("pool", alpha[k]));
                        DataTable dtb = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(dtb);
                        conn.Close();
                        keyColumns[0] = dtb.Columns[0];
                        dtb.PrimaryKey = keyColumns;
                        if (dtb.Rows.Contains(tabe[i]) && dtb.Rows.Contains(tabe[i + 1]))
                        {
                            //Elimino partite create
                            query = "DELETE FROM Partita WHERE IdTorneo=@IDTorneo AND Fase='Ottavi'";
                            conn.Open();
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                            dtb = new DataTable();
                            da = new SqlDataAdapter(command);
                            da.Fill(dtb);
                            conn.Close();
                            return false;//controllo che non appartengano allo stesso pool
                        }
                    }
                }
            }
            return true;
        }
        public DataTable GetTestadiSerie(int idtorneo)
        {
            DataTable TestadiSerie = new DataTable();
            //prendo numero pool
            string query = "SELECT COUNT(DISTINCT(IdPool))as num From pool WHERE IdTorneo=@IDTorneo";
            conn.Open();
            SqlCommand command = new SqlCommand(query, conn);
            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
            DataTable dtb = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(dtb);
            int numPool = Convert.ToInt32(dtb.Rows[0][0]); //numero pool
            conn.Close();
            int numpartita = GetLastNumberPartita(idtorneo);
            //PRENDO NUMERO TEAM TOTALI
            query = "SELECT COUNT(*) FROM Partecipa WHERE IdTorneo=@IDTorneo";
            conn.Open();
            command = new SqlCommand(query, conn);
            command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
            dtb = new DataTable();
            da = new SqlDataAdapter(command);
            da.Fill(dtb);
            conn.Close();
            int teamTotali = Convert.ToInt32(dtb.Rows[0][0]); //numero pool
            List<string>Tabellone=CreoTabellone(teamTotali, numPool, idtorneo);
            for(int i = 0; i < Tabellone.Count; i++)
            {
                //prendo info da idsquadra
                query = "SELECT Squadra.NomeTeam,Pool.IDPool,Pool.QP,Torneo.Titolo FROM POOL " +
                    "INNER JOIN Squadra ON Squadra.IDSquadra = Pool.IDSquadra AND Squadra.IDSquadra = @IDSquadra " +
                    "INNER JOIN Torneo ON Torneo.IDTorneo=Pool.IDTorneo AND Torneo.IDTorneo=@IDTorneo";
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                command.Parameters.Add(new SqlParameter("IDSquadra", Tabellone[i]));
                da = new SqlDataAdapter(command);
                da.Fill(TestadiSerie);
                conn.Close();
            }

            return TestadiSerie;
        }
        private List<string> CreoTabellone(int teamTotali, int numpool, int idtorneo)
        {
            string query;
            SqlCommand command;
            DataTable dtb;
            SqlDataAdapter da;
            List<string> prime = new List<string>();
            List<string> seconde = new List<string>();
            List<string> terze = new List<string>();
            Random rnd = new Random();
            List<string> team = new List<string>();

            if (teamTotali == 8)
            {
                //SCARICO TUTTE LE PRIME
                query = "SELECT IdSquadra FROM Pool WHERE PP=8 AND IdTorneo=@IDTorneo ORDER BY QP DESC,(SELECT EntryPoints FROM Partecipa WHERE Partecipa.IDSquadra=Pool.IDSquadra AND Partecipa.IDTorneo=@IDTorneo)DESC ";//scarico 1 team di quel pool
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                foreach (DataRow dr in dtb.Rows)
                {
                    prime.Add(dr[0].ToString());
                }
                conn.Close();
                //SCARICO TUTTE LE SECONDE
                query = "SELECT IdSquadra FROM Pool WHERE PP=5 AND IdTorneo=@IDTorneo ORDER BY idpool ASC ";//scarico 1 team di quel pool
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                foreach (DataRow dr in dtb.Rows)
                {
                    seconde.Add(dr[0].ToString());
                }
                conn.Close();
                //SCARICO TUTTE LE SECONDE
                query = "SELECT IdSquadra FROM Pool WHERE PP=3 AND IdTorneo=@IDTorneo ORDER BY idpool ASC ";//scarico 1 team di quel pool
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                foreach (DataRow dr in dtb.Rows)
                {
                    terze.Add(dr[0].ToString());
                }
                conn.Close();
                team.AddRange(prime);
                team.AddRange(seconde);
                team.AddRange(terze);
                //SET SQUADRE BYE
                team.Add("1");
                team.Add("1");
            }
            else if (teamTotali == 12)
            {
                //SCARICO TUTTE LE PRIME
                query = "SELECT IdSquadra FROM Pool WHERE PP=8 AND IdTorneo=@IDTorneo ORDER BY QP DESC,(SELECT EntryPoints FROM Partecipa WHERE Partecipa.IDSquadra=Pool.IDSquadra AND Partecipa.IDTorneo=@IDTorneo)DESC ";//scarico 1 team di quel pool
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                foreach (DataRow dr in dtb.Rows)
                {
                    prime.Add(dr[0].ToString());
                }
                string supporto = prime.First();
                prime.RemoveAt(0);//rimuovo la prima
                var listsup = prime.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                listsup.Insert(0, supporto);//rimetto la prima
                prime = listsup;
                conn.Close();
                //SCARICO TUTTE LE SECONDE
                query = "SELECT IdSquadra FROM Pool WHERE PP=5 AND IdTorneo=@IDTorneo ORDER BY QP DESC,(SELECT EntryPoints FROM Partecipa WHERE Partecipa.IDSquadra=Pool.IDSquadra AND Partecipa.IDTorneo=@IDTorneo)DESC ";//scarico 1 team di quel pool
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                foreach (DataRow dr in dtb.Rows)
                {
                    seconde.Add(dr[0].ToString());
                }
                supporto = seconde.First();
                seconde.RemoveAt(0);//rimuovo la prima
                listsup = seconde.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                listsup.Insert(0, supporto);//rimetto la prima
                seconde = listsup;
                conn.Close();
                //SCARICO TUTTE LE TERZE
                query = "SELECT IdSquadra FROM Pool WHERE PP=3 AND IdTorneo=@IDTorneo ";//scarico 1 team di quel pool
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                foreach (DataRow dr in dtb.Rows)
                {
                    terze.Add(dr[0].ToString());
                }
                conn.Close();
                team.AddRange(prime);
                team.AddRange(seconde);
                team.AddRange(terze);
            }
            else if (teamTotali > 12)
            {
                //SE IL TORNEO E' DA 16
                if (teamTotali == 16)
                {
                    //SCARICO LE PRIME PRIMI (A,B) PER POINT RATIO
                    query = "SELECT IdSquadra FROM Pool WHERE PP=8 AND IdTorneo=@IDTorneo AND (idPool='A' OR idpool='B') ORDER BY QP DESC,(SELECT EntryPoints FROM Partecipa WHERE Partecipa.IDSquadra=Pool.IDSquadra AND Partecipa.IDTorneo=@IDTorneo)DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    foreach (DataRow dr in dtb.Rows)
                    {
                        prime.Add(dr[0].ToString());
                    }
                    conn.Close();
                    //SCARICO LE SECONDE PRIMI (C,D) PER POINT RATIO
                    query = "SELECT IdSquadra FROM Pool WHERE PP=8 AND IdTorneo=@IDTorneo AND (idPool='C' OR idpool='D') ORDER BY QP DESC,(SELECT EntryPoints FROM Partecipa WHERE Partecipa.IDSquadra=Pool.IDSquadra AND Partecipa.IDTorneo=@IDTorneo)DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    foreach (DataRow dr in dtb.Rows)
                    {
                        prime.Add(dr[0].ToString());
                    }
                    conn.Close();
                    //SCARICO TUTTE LE SECONDE
                    query = "SELECT IdSquadra FROM Pool WHERE PP=5 AND IdTorneo=@IDTorneo ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    foreach (DataRow dr in dtb.Rows)
                    {
                        seconde.Add(dr[0].ToString());
                    }
                    var listsup = seconde.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    seconde[3] = listsup[0];//8 =3
                    seconde[0] = listsup[1];//5 =0
                    seconde[1] = listsup[2];//6 =1
                    seconde[2] = listsup[3];//7 =2
                    conn.Close();
                    //SCARICO TUTTE LE TERZE
                    query = "SELECT IdSquadra FROM Pool WHERE PP=3 AND IdTorneo=@IDTorneo ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    foreach (DataRow dr in dtb.Rows)
                    {
                        terze.Add(dr[0].ToString());
                    }
                    listsup = terze.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    terze[0] = listsup[0];//9  =0
                    terze[3] = listsup[1];//12 =3
                    terze[2] = listsup[2];//11 =2
                    terze[1] = listsup[3];//10 =1
                    conn.Close();
                    team.AddRange(prime);
                    team.AddRange(seconde);
                    team.AddRange(terze);
                    //SET SQUADRE BYE
                    team.Add("1");
                    team.Add("1");
                    team.Add("1");
                    team.Add("1");
                }
                else if (teamTotali == 20)
                {
                    //SCARICO LE PRIME PRIMI (A,B) PER POINT RATIO
                    query = "SELECT IdSquadra FROM Pool WHERE PP=8 AND IdTorneo=@IDTorneo AND (idPool='A' OR idpool='B') ORDER BY QP DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    prime = ConvertDataTable<string>(dtb);
                    conn.Close();
                    //SCARICO LE SECONDE PRIMI (C,D) PER POINT RATIO
                    query = "SELECT IdSquadra FROM Pool WHERE PP=8 AND IdTorneo=@IDTorneo AND (idPool='C' OR idpool='D' OR idpool='E') ORDER BY QP DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    prime.Concat(ConvertDataTable<string>(dtb));
                    conn.Close();
                    //SCARICO LE MIGLIORI 3 SECONDE
                    query = "SELECT TOP(3) IdSquadra FROM Pool WHERE PP=5 AND IdTorneo=@IDTorneo ORDER BY QP DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    seconde = ConvertDataTable<string>(dtb);
                    var listsup = seconde.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    seconde.Clear();
                    seconde[2] = listsup[0];//8 =3
                    seconde[0] = listsup[1];//6 =0
                    seconde[1] = listsup[2];//7 =1
                    //SCARICO LE PEGGIORI 2 SECONDE
                    query = "SELECT TOP(2) IdSquadra FROM Pool WHERE PP=5 AND IdTorneo=@IDTorneo ORDER BY QP ASC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    seconde = ConvertDataTable<string>(dtb);
                    listsup = seconde.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    seconde[3] = listsup[3];//9 =2
                    seconde[4] = listsup[4];//10 =2
                    conn.Close();
                    //SCARICO TUTTE LE TERZE
                    query = "SELECT IdSquadra FROM Pool WHERE PP=3 AND IdTorneo=@IDTorneo ORDER BY QP DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    terze = ConvertDataTable<string>(dtb);
                    listsup = terze.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    terze.Clear();
                    terze[1] = listsup[0];//12  =0
                    terze[2] = listsup[1];//13 =3
                    terze[3] = listsup[2];//14 =2
                    terze[0] = listsup[3];//11 =1
                    terze[4] = listsup[4];//15 =1
                    conn.Close();
                    team.AddRange(prime);
                    team.AddRange(seconde);
                    team.AddRange(terze); team[15] = "1"; //ADD SQUADRA BYE PER FARE 16
                }
                else if (teamTotali == 24)
                {
                    //SCARICO LE PRIME PRIMI (A,B) PER POINT RATIO
                    query = "SELECT IdSquadra FROM Pool WHERE PP=8 AND IdTorneo=@IDTorneo AND (idPool='A' OR idpool='B') ORDER BY QP DESC,(SELECT EntryPoints FROM Partecipa WHERE Partecipa.IDSquadra=Pool.IDSquadra AND Partecipa.IDTorneo=@IDTorneo)DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    foreach (DataRow dr in dtb.Rows)
                    {
                        prime.Add(dr[0].ToString());
                    }
                    conn.Close();
                    //SCARICO LE SECONDE PRIMI (C,D) PER POINT RATIO
                    query = "SELECT IdSquadra FROM Pool WHERE PP=8 AND IdTorneo=@IDTorneo AND (idPool='C' OR idpool='D' OR idpool='E' OR idpool='F') ORDER BY QP DESC,(SELECT EntryPoints FROM Partecipa WHERE Partecipa.IDSquadra=Pool.IDSquadra AND Partecipa.IDTorneo=@IDTorneo)DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    List<string> supp = new List<string>();
                    foreach (DataRow dr in dtb.Rows)
                    {
                        supp.Add(dr[0].ToString());
                    }
                    var listsup = supp.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    prime.Add(listsup[2]);//la terza=3
                    prime.Add(listsup[1]);//la seconda=4
                    prime.Add(listsup[0]);//la prima=5
                    prime.Add(listsup[3]);//la quarta=6
                    conn.Close();
                    //SCARICO LE MIGLIORI 2 SECONDE
                    query = "SELECT IdSquadra FROM Pool WHERE PP=5 AND IdTorneo=@IDTorneo ORDER BY QP DESC,(SELECT EntryPoints FROM Partecipa WHERE Partecipa.IDSquadra=Pool.IDSquadra AND Partecipa.IDTorneo=@IDTorneo)DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    for (int i = 0; i < 2; i++)
                    {
                        seconde.Add(dtb.Rows[i][0].ToString());
                    }
                    listsup = seconde.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    seconde[1] = listsup[0];//8 =1
                    seconde[0] = listsup[1];//7 =0
                    List<string> secondesup = new List<string>();
                    for (int i = 2; i < 6; i++)
                    {
                        secondesup.Add(dtb.Rows[i][0].ToString());
                    }
                    listsup = secondesup.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    seconde.Add(listsup[0]);//9 =2
                    seconde.Add(listsup[3]);//10 =3
                    seconde.Add(listsup[2]);//11 =4
                    seconde.Add(listsup[1]);//12 =5  
                    conn.Close();
                    //SCARICO LE MIGLIORI 2 TERZE
                    query = "SELECT IdSquadra FROM Pool WHERE PP=3 AND IdTorneo=@IDTorneo ORDER BY QP DESC,(SELECT EntryPoints FROM Partecipa WHERE Partecipa.IDSquadra=Pool.IDSquadra AND Partecipa.IDTorneo=@IDTorneo)DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    for (int i = 0; i < 2; i++)
                    {
                        terze.Add(dtb.Rows[i][0].ToString());
                    }
                    listsup = terze.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    terze[0] = listsup[0];//13  =0
                    terze[1] = listsup[1];//14 =1
                    List<string> terzesup = new List<string>();
                    for (int i = 2; i < 6; i++)
                    {
                        terzesup.Add(dtb.Rows[i][0].ToString());
                    }
                    terze.Add(terzesup[0]);
                    terze.Add(terzesup[1]);
                    terze.Add(terzesup[2]);
                    terze.Add(terzesup[3]);
                    conn.Close();
                    team.AddRange(prime);
                    team.AddRange(seconde);
                    team.AddRange(terze);
                }
                else if (teamTotali == 32)
                {
                    //SCARICO LE PRIME PRIMI (A,B) PER POINT RATIO
                    query = "SELECT IdSquadra FROM Pool WHERE PP=8 AND IdTorneo=@IDTorneo AND (idPool='A' OR idpool='B') ORDER BY QP DESC,(SELECT EntryPoints FROM Partecipa WHERE Partecipa.IDSquadra=Pool.IDSquadra AND Partecipa.IDTorneo=@IDTorneo)DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    foreach (DataRow dr in dtb.Rows)
                    {
                        prime.Add(dr[0].ToString());
                    }
                    conn.Close();
                    //SCARICO LE PRIME PRIMI (A,B) PER POINT RATIO
                    query = "SELECT IdSquadra FROM Pool WHERE PP=8 AND IdTorneo=@IDTorneo AND (idPool='C' OR idpool='D') ORDER BY QP DESC,(SELECT EntryPoints FROM Partecipa WHERE Partecipa.IDSquadra=Pool.IDSquadra AND Partecipa.IDTorneo=@IDTorneo)DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    foreach (DataRow dr in dtb.Rows)
                    {
                        prime.Add(dr[0].ToString());
                    }
                    conn.Close();
                    //SCARICO LE SECONDE PRIMI (C,D) PER POINT RATIO
                    query = "SELECT IdSquadra FROM Pool WHERE PP=8 AND IdTorneo=@IDTorneo AND (idPool='E' OR idpool='F' OR idpool='G' OR idpool='H') ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    List<string> supp = new List<string>();
                    foreach (DataRow dr in dtb.Rows)
                    {
                        supp.Add(dr[0].ToString());
                    }
                    var listsup = supp.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    prime.Add(listsup[1]);//la seconda=5
                    prime.Add(listsup[2]);//la terza=6
                    prime.Add(listsup[3]);//la quarta=7
                    prime.Add(listsup[0]);//la prima=8
                    conn.Close();
                    //SCARICO LE MIGLIORI 2 SECONDE
                    query = "SELECT IdSquadra FROM Pool WHERE PP=5 AND IdTorneo=@IDTorneo ORDER BY QP DESC,(SELECT EntryPoints FROM Partecipa WHERE Partecipa.IDSquadra=Pool.IDSquadra AND Partecipa.IDTorneo=@IDTorneo)DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    for (int i = 0; i < 4; i++)
                    {
                        seconde.Add(dtb.Rows[i][0].ToString());
                    }
                    listsup = seconde.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    seconde[0] = listsup[0];//9 =1
                    seconde[1] = listsup[3];//10 =4
                    seconde[2] = listsup[2];//11 =3
                    seconde[3] = listsup[1];//12 =2
                    conn.Close();
                    List<string> secondesup = new List<string>();
                    for (int i = 4; i < 8; i++)
                    {
                        secondesup.Add(dtb.Rows[i][0].ToString());
                    }
                    listsup = secondesup.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    seconde.Add(listsup[1]);//13 =2
                    seconde.Add(listsup[2]);//14 =3
                    seconde.Add(listsup[3]);//15 =4
                    seconde.Add(listsup[0]);//16 =1
                    //SCARICO LE MIGLIORI 2 TERZE
                    query = "SELECT IdSquadra FROM Pool WHERE PP=3 AND IdTorneo=@IDTorneo ORDER BY QP DESC,(SELECT EntryPoints FROM Partecipa WHERE Partecipa.IDSquadra=Pool.IDSquadra AND Partecipa.IDTorneo=@IDTorneo)DESC ";//scarico 1 team di quel pool
                    conn.Open();
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    for (int i = 0; i < 4; i++)
                    {
                        terze.Add(dtb.Rows[i][0].ToString());
                    }
                    listsup = terze.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    terze[0] = listsup[0];//17 =1
                    terze[1] = listsup[3];//18 =4
                    terze[2] = listsup[2];//19 =3
                    terze[3] = listsup[1];//20 =2
                    List<string> terzesup = new List<string>();
                    for (int i = 4; i < 8; i++)
                    {
                        terzesup.Add(dtb.Rows[i][0].ToString());
                    }
                    listsup = terzesup.OrderBy(item => rnd.Next()).ToList();//mischio le altre due
                    terze.Add(listsup[1]);//21 =2
                    terze.Add(listsup[2]);//22 =3
                    terze.Add(listsup[3]);//23 =4
                    terze.Add(listsup[0]);//24 =1
                    conn.Close();
                    team.AddRange(prime);
                    team.AddRange(seconde);
                    team.AddRange(terze);
                }
            }
            return team;
        }
        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }
        private bool CreaSedicesimi(int idtorneo)
        {
            try
            {
                //riempio pool
                conn.Open();
                string query = "SELECT count(*)as numero FROM Partita WHERE IdTorneo=@IDTorneo AND Fase='Ottavi'";//scarico numero partite
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                DataTable dtb = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dtb);
                int numpool = Convert.ToInt32(dtb.Rows[0][0]);//numero di pool
                conn.Close();
                do
                {
                    DataTable tabelloneSedicesimi;
                    if (tabelloneSediceseimi(idtorneo, numpool) != null) { tabelloneSedicesimi = tabelloneSediceseimi(idtorneo, numpool); }
                    else
                    {
                        conn.Close();
                        return false;
                    }
                    //elimo i sedicesimi presenti 
                    conn.Open();
                    query = "DELETE FROM Partita WHERE Fase='Sedicesimi'";
                    command = new SqlCommand(query, conn);
                    command.ExecuteNonQuery();
                    conn.Close();
                    Random random = new Random();
                    int numpartita = GetLastNumberPartita(idtorneo) - numpool;//last id meno gli ottavi che verrano messi sotto poi
                    while (tabelloneSedicesimi.Rows.Count > 0)
                    {
                        //estraggo numero casuale
                        int row = random.Next(0, tabelloneSedicesimi.Rows.Count);
                        numpartita++;
                        query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,IDSQ1,IDSQ2,NumPartita,NumPartitaSuccessiva) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@sq1,@sq2,@num,@numsucce);";
                        conn.Open();
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO
                        command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                        command.Parameters.Add(new SqlParameter("Pool", "Sedicesimi"));
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        command.Parameters.Add(new SqlParameter("sq1", tabelloneSedicesimi.Rows[row][0].ToString()));
                        command.Parameters.Add(new SqlParameter("sq2", tabelloneSedicesimi.Rows[(tabelloneSedicesimi.Rows.Count - 1) - row][0].ToString()));
                        command.Parameters.Add(new SqlParameter("num", numpartita));
                        command.Parameters.Add(new SqlParameter("numsucce", numpartita + 8));
                        da = new SqlDataAdapter(command);
                        command.ExecuteNonQuery();
                        conn.Close();
                        tabelloneSedicesimi.Rows[row].Delete();
                        tabelloneSedicesimi.Rows[(tabelloneSedicesimi.Rows.Count - 1) - row].Delete();
                        tabelloneSedicesimi.AcceptChanges();
                    }
                } while (!ControlloSedicesimi(idtorneo));//ciclo che controlla che i sedicesimi non incontri qualcuno dello stesso pool agli ottavi
                return true;
            }
            catch
            {
                conn.Close();
                return false;
            }
        }
        private DataTable tabelloneSediceseimi(int idtorneo, int num)
        {
            try
            {
                int numpool = num;
                DataTable tabelloneSedicesimi = new DataTable();
                DataTable dtb = new DataTable();
                //prendo i migliori secondi
                string query = "SELECT TOP(@limit) IdSquadra FROM Pool WHERE IdTorneo=@IDTorneo AND PP IN (SELECT MAX( PP )FROM Pool WHERE PP < ( SELECT MAX( PP )FROM Pool )) ORDER BY QP DESC";
                conn.Open();
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                command.Parameters.Add(new SqlParameter("limit", numpool / 2));
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dtb);
                conn.Close();
                tabelloneSedicesimi.Merge(ShufflingTable(dtb));//salvo migliori secondi gia sorteggiati

                //prendo i peggiori secondi
                query = "SELECT TOP(@limit) IdSquadra FROM Pool WHERE IdTorneo=@IDTorneo AND PP IN (SELECT MAX( PP )FROM Pool WHERE PP < ( SELECT MAX( PP )FROM Pool )) ORDER BY QP ASC";
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                command.Parameters.Add(new SqlParameter("limit", numpool / 2));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                conn.Close();
                tabelloneSedicesimi.Merge(ShufflingTable(dtb));//salvo Peggiori secondi gia sorteggiati

                //prendo i migliori terzi
                query = "SELECT TOP(@limit) IdSquadra FROM Pool WHERE IdTorneo=@IDTorneo AND PP IN (SELECT MAX( PP )FROM Pool WHERE PP < ( SELECT MAX( PP )FROM Pool  WHERE PP < ( SELECT MAX( PP )FROM Pool))) ORDER BY QP DESC";
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                command.Parameters.Add(new SqlParameter("limit", numpool / 2));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                conn.Close();
                tabelloneSedicesimi.Merge(ShufflingTable(dtb));//salvo migliori terzi gia sorteggiati

                //prendo i peggiori terzi
                query = "SELECT TOP(@limit) IdSquadra FROM Pool WHERE IdTorneo=@IDTorneo AND PP IN (SELECT MAX( PP )FROM Pool WHERE PP < ( SELECT MAX( PP )FROM Pool  WHERE PP < ( SELECT MAX( PP )FROM Pool))) ORDER BY QP ASC";
                conn.Open();
                command = new SqlCommand(query, conn);
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                command.Parameters.Add(new SqlParameter("limit", numpool / 2));
                da.Fill(dtb);
                conn.Close();
                tabelloneSedicesimi.Merge(ShufflingTable(dtb));//salvo Peggiori terzi gia sorteggiati
                return tabelloneSedicesimi;
            }
            catch
            {
                conn.Close();
                return null;
            }
        }
        private DataTable ShufflingTable(DataTable table)
        {
            DataTable clone = table.Clone();
            if (table.Rows.Count > 0)
            {
                clone.Clear();
                Random random = new Random();

                while (table.Rows.Count > 0)
                {
                    int row = random.Next(0, table.Rows.Count);
                    clone.ImportRow(table.Rows[row]);
                    table.Rows[row].Delete();
                    table.AcceptChanges();
                }
            }
            return clone;
        }
        private bool ControlloSedicesimi(int idtorneo)
        {
            try
            {
                Dictionary<char, List<string>> pool = new Dictionary<char, List<string>>();
                DataTable sedicesimi = new DataTable();
                DataTable ottavi = new DataTable();
                //scarico squadre sedicesimi
                string query = "SELECT IDSQ1,IDSQ2 FROM Partita WHERE IdTorneo=@IDTorneo AND Fase='Sedicesimi'";
                conn.Open();
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                DataTable dtb = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dtb);
                sedicesimi.Merge(dtb);

                //scarico squadre ottavi
                query = "SELECT IDSQ1,IDSQ2 FROM Partita WHERE IdTorneo=@IDTorneo AND Fase='Ottavi'";
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                ottavi.Merge(dtb);

                //riempio pool
                query = "SELECT count(DISTINCT(IdPool))as numero FROM POOL WHERE IdTorneo=@IDTorneo";//scarico numero partite
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                int numpool = Convert.ToInt32(dtb.Rows[0][0]);//numero di pool

                for (int i = 0; i < numpool; i++)
                {
                    query = "SELECT IdPool FROM Pool WHERE IdTorneo=@IDTorneo AND IdSquadra='" + ottavi.Rows[i][0] + "'";//scarico pool dove è la squadra agli ottavi
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    string pul = dtb.Rows[0][0].ToString();//salvo pool
                    query = "SELECT IdSquadra FROM Pool WHERE IdTorneo=@IDTorneo AND IdPool='" + pul + "'";//scarico pool dove è la squadra agli ottavi
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    for (int j = 0; j < dtb.Rows.Count; j++)
                    {
                        string teampool = dtb.Rows[j][0].ToString();
                        string team1 = sedicesimi.Rows[i][0].ToString();
                        string team2 = sedicesimi.Rows[i][1].ToString();
                        if (teampool == team1 || teampool == team2)
                        {
                            conn.Close();
                            return false;
                        }
                    }
                }
                conn.Close();
                return true;
            }
            catch
            {
                conn.Close();
                return false;
            }
        }
        private bool SetPartiteSuccessive(int idtorneo, int num, DataTable dtb)
        {
            try
            {
                int count = 0;
                for (int i = 0; i < num; i++)
                {
                    if (i % 2 == 0)
                    {
                        dtb.Rows[i][3] = Convert.ToInt32(dtb.Rows[i][2]) + num - count;
                        count++;
                    }
                    else dtb.Rows[i][3] = Convert.ToInt32(dtb.Rows[i][2]) + num - count;
                    //aggiorno record
                    conn.Open();
                    string query = "UPDATE Partita SET NumPartitaSuccessiva=@num WHERE IdTorneo=@IDTorneo AND IDPartita=@idpartita";
                    SqlCommand command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("idpartita", dtb.Rows[i][0]));
                    command.Parameters.Add(new SqlParameter("num", dtb.Rows[i][3]));
                    command.ExecuteNonQuery();
                    conn.Close();
                }
                return true;
            }
            catch { return false; }
        }
        //crea quarti semifinali e finali
        private bool CreaPartiteRestanti(int idtorneo)
        {
            try
            {
                string query;
                SqlCommand command;
                DataTable dtb = new DataTable();
                SqlDataAdapter da;
                int numpartita = GetLastNumberPartita(idtorneo);
                //INSERT QUARTI
                for (int i = 0; i < 4; i++)
                {
                    numpartita++;
                    conn.Open();
                    query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num);";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO?
                    command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                    command.Parameters.Add(new SqlParameter("Pool", "Quarti"));
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", numpartita));
                    da = new SqlDataAdapter(command);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
                //SELECT QUARTI
                query = "SELECT IDPartita,IDTorneo,NumPartita,NumPartitaSuccessiva FROM Partita WHERE Fase='Quarti' AND IdTorneo=@IDTorneo"; //prendo squadre quarti
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                conn.Close();
                //SET NUMPARTITASUCCESSIVA
                SetPartiteSuccessive(idtorneo, 4, dtb);//set numero partite successive    
                numpartita = GetLastNumberPartita(idtorneo);
                //INSERT SEMIFINALI
                for (int i = 0; i < 2; i++)
                {
                    numpartita++;
                    conn.Open();
                    query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num);";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO?
                    command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                    command.Parameters.Add(new SqlParameter("Pool", "Semifinali"));
                    command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                    command.Parameters.Add(new SqlParameter("num", numpartita));
                    da = new SqlDataAdapter(command);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
                //SELECT SEMIFINALI
                query = "SELECT IDPartita,IDTorneo,NumPartita,NumPartitaSuccessiva FROM Partita WHERE Fase='Semifinali' AND IdTorneo=@IDTorneo"; //prendo squadre semifinali
                conn.Open();
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                da = new SqlDataAdapter(command);
                dtb = new DataTable();
                da.Fill(dtb);
                conn.Close();
                //SET NUMPARTITASUCCESSIVA
                SetPartiteSuccessive(idtorneo, 2, dtb);//set numero partite successive               
                numpartita = GetLastNumberPartita(idtorneo);
                numpartita++;
                //CREO FINALI
                conn.Open();
                query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num);";
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO?
                command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                command.Parameters.Add(new SqlParameter("Pool", "Finale 3/4"));
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                command.Parameters.Add(new SqlParameter("num", numpartita));
                da = new SqlDataAdapter(command);
                command.ExecuteNonQuery();
                conn.Close();
                numpartita++;
                conn.Open();
                query = "INSERT INTO Partita (DataPartita,OraPartita,Fase,Risultato,IDTorneo,NumPartita) VALUES (@Data,@Ora,@Pool,'0-0',@IDTorneo,@num);";
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("Data", DateTime.Now.Date));//COME GESTIRE LE DATE E L'ORARIO?
                command.Parameters.Add(new SqlParameter("Ora", DateTime.Now.TimeOfDay));
                command.Parameters.Add(new SqlParameter("Pool", "Finale 1/2"));
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                command.Parameters.Add(new SqlParameter("num", numpartita));
                da = new SqlDataAdapter(command);
                command.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch
            {
                conn.Close();
                return false;
            }
        }
        private bool AvanzaTabellone(int idtorneo, int numpartita, int setsq1, int setsq2)
        {
            try
            {
                int number=0;
                //prendo la partita e controllo che non sia un pool
                conn.Open();
                string query = "SELECT IDPartita,IDSQ1,IDSQ2,IDTorneo,NumPartita,SetSQ1,SetSQ2,NumPartitaSuccessiva,Fase FROM Partita WHERE Fase NOT LIKE '%Pool%' AND IDTorneo=@IDTorneo AND NumPartita=@num";
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                command.Parameters.Add(new SqlParameter("num", numpartita));
                SqlDataAdapter da = new SqlDataAdapter(command);
                DataTable dtb = new DataTable();
                da.Fill(dtb);
                conn.Close();
                if (dtb.Rows[0][7] != DBNull.Value) number = Convert.ToInt32(dtb.Rows[0][7]);
                // se è diverso da zero significa che non è un pool
                if (dtb.Rows.Count != 0)
                {
                    if (dtb.Rows[0][8].ToString() == "Sedicesimi")
                    {
                        query = "SELECT TOP(1) NumPartita FROM Partita WHERE Fase='Ottavi' AND IDTorneo=@IDTorneo";
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        da = new SqlDataAdapter(command);
                        dtb = new DataTable();
                        da.Fill(dtb);
                        numpartita = Convert.ToInt32(dtb.Rows[0][0].ToString());
                        conn.Close();
                        conn.Open();
                        query = "SELECT IDPartita,IDSQ1,IDSQ2,IDTorneo,NumPartita,SetSQ1,SetSQ2,NumPartitaSuccessiva,Fase FROM Partita WHERE Fase='Sedicesimi' AND IDTorneo=@IDTorneo";
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        da = new SqlDataAdapter(command);
                        dtb = new DataTable();
                        da.Fill(dtb);
                        conn.Close();
                        if (setsq1 == 2)
                        {
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ2 = @sq2 WHERE IdTorneo = @IDTorneo AND NumPartita = @num ";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", dtb.Rows[0][3]));
                            command.Parameters.Add(new SqlParameter("sq2", dtb.Rows[0][1]));
                            command.Parameters.Add(new SqlParameter("num", numpartita));
                            command.ExecuteNonQuery();
                            conn.Close();
                            return true;
                        }
                        //vittoria squadra 2
                        else if (setsq2 == 2)
                        {
                            conn.Open();
                            query = "UPDATE Partita SET IDSQ2 = @sq2 WHERE IdTorneo = @IDTorneo AND NumPartita = @num ";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", dtb.Rows[0][3]));
                            command.Parameters.Add(new SqlParameter("sq2", dtb.Rows[0][2]));
                            command.Parameters.Add(new SqlParameter("num", numpartita));
                            command.ExecuteNonQuery();
                            conn.Close();
                            return true;
                        }
                    }
                    else if (dtb.Rows[0][8].ToString() == "Ottavi")
                    {
                        conn.Open();
                        query = "SELECT IDPartita,IDSQ1,IDSQ2,IDTorneo,NumPartita,SetSQ1,SetSQ2,NumPartitaSuccessiva,Fase FROM Partita WHERE Fase='Ottavi' AND IDTorneo=@IDTorneo AND NumPartita=@num";
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneo));
                        command.Parameters.Add(new SqlParameter("num", numpartita));
                        da = new SqlDataAdapter(command);
                        dtb = new DataTable();
                        da.Fill(dtb);
                        conn.Close();
                        if (setsq1 == 2)
                        {
                            conn.Open();
                            query = "IF(SELECT IDSQ1 FROM Partita  WHERE IdTorneo = @IDTorneo AND NumPartita = @num) IS NULL " +
                                "UPDATE Partita SET IDSQ1 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num " +
                                "ELSE " +
                                "UPDATE Partita SET IDSQ2 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", dtb.Rows[0][3]));
                            command.Parameters.Add(new SqlParameter("sq1", dtb.Rows[0][1]));
                            command.Parameters.Add(new SqlParameter("num", number));
                            command.ExecuteNonQuery();
                            conn.Close();
                            return true;
                        }
                        //vittoria squadra 2
                        else if (setsq2 == 2)
                        {
                            conn.Open();
                            query = "IF(SELECT IDSQ1 FROM Partita  WHERE IdTorneo = @IDTorneo AND NumPartita = @num) IS NULL " +
                                "UPDATE Partita SET IDSQ1 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num " +
                                "ELSE " +
                                "UPDATE Partita SET IDSQ2 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", dtb.Rows[0][3]));
                            command.Parameters.Add(new SqlParameter("sq1", dtb.Rows[0][2]));
                            command.Parameters.Add(new SqlParameter("num", number));
                            command.ExecuteNonQuery();
                            conn.Close();
                            return true;
                        }
                    }
                    if (dtb.Rows[0][8].ToString() == "Finale 1/2" || dtb.Rows[0][8].ToString() == "Finale 3/4")
                    {
                        if (dtb.Rows[0][8].ToString() == "Finale 1/2")
                        {
                            //Update punti in partecipa
                            DataTable Classifica = CreateDataTable(GeneraClassifica(idtorneo));
                            for (int i = 0; i < Classifica.Rows.Count; i++)
                            {
                                conn.Open();
                                query = "UPDATE P SET Punti=@punti,PosizioneFinale=@pos FROM Partecipa as P " +
                                    "INNER JOIN Squadra AS S ON S.NomeTeam =@team " +
                                    "WHERE P.IDSquadra = S.IDSquadra";
                                command = new SqlCommand(query, conn);
                                command.Parameters.Add(new SqlParameter("punti", Convert.ToInt32(Classifica.Rows[i][4])));
                                command.Parameters.Add(new SqlParameter("pos", Classifica.Rows[i][2].ToString()));
                                command.Parameters.Add(new SqlParameter("team", Classifica.Rows[i][3].ToString()));
                                command.ExecuteNonQuery();
                                conn.Close();
                            }
                        }
                        return true;
                    }
                    if (dtb.Rows[0][8].ToString() == "Semifinali")
                    {
                        //vittoria squadra 1
                        if (setsq1 == 2)
                        {
                            //Vincitore
                            conn.Open();
                            query = "IF(SELECT IDSQ1 FROM Partita  WHERE IdTorneo = @IDTorneo AND NumPartita = @num) IS NULL " +
                                "UPDATE Partita SET IDSQ1 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num " +
                                "ELSE " +
                                "UPDATE Partita SET IDSQ2 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", dtb.Rows[0][3]));
                            command.Parameters.Add(new SqlParameter("sq1", dtb.Rows[0][1]));
                            command.Parameters.Add(new SqlParameter("num", number + 1));
                            command.ExecuteNonQuery();
                            conn.Close();
                            //perdente
                            conn.Open();
                            query = "IF(SELECT IDSQ1 FROM Partita  WHERE IdTorneo = @IDTorneo AND NumPartita = @num) IS NULL " +
                                "UPDATE Partita SET IDSQ1 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num " +
                                "ELSE " +
                                "UPDATE Partita SET IDSQ2 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", dtb.Rows[0][3]));
                            command.Parameters.Add(new SqlParameter("sq1", dtb.Rows[0][2]));
                            command.Parameters.Add(new SqlParameter("num", number));
                            command.ExecuteNonQuery();
                            conn.Close();
                            return true;
                        }
                        //vittoria squadra 2
                        else if (setsq2 == 2)
                        {
                            //Vincitore
                            conn.Open();
                            query = "IF(SELECT IDSQ1 FROM Partita  WHERE IdTorneo = @IDTorneo AND NumPartita = @num) IS NULL " +
                                "UPDATE Partita SET IDSQ1 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num " +
                                "ELSE " +
                                "UPDATE Partita SET IDSQ2 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", dtb.Rows[0][3]));
                            command.Parameters.Add(new SqlParameter("sq1", dtb.Rows[0][2]));
                            command.Parameters.Add(new SqlParameter("num", number + 1));
                            command.ExecuteNonQuery();
                            conn.Close();
                            //perdente
                            conn.Open();
                            query = "IF(SELECT IDSQ1 FROM Partita  WHERE IdTorneo = @IDTorneo AND NumPartita = @num) IS NULL " +
                                "UPDATE Partita SET IDSQ1 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num " +
                                "ELSE " +
                                "UPDATE Partita SET IDSQ2 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num";
                            command = new SqlCommand(query, conn);
                            command.Parameters.Add(new SqlParameter("IDTorneo", dtb.Rows[0][3]));
                            command.Parameters.Add(new SqlParameter("sq1", dtb.Rows[0][1]));
                            command.Parameters.Add(new SqlParameter("num", number));
                            command.ExecuteNonQuery();
                            conn.Close();
                            return true;
                        }
                    }
                    //vittoria squadra 1
                    if (setsq1 == 2)
                    {
                        conn.Open();
                        query = "IF(SELECT IDSQ1 FROM Partita  WHERE IdTorneo = @IDTorneo AND NumPartita = @num) IS NULL " +
                            "UPDATE Partita SET IDSQ1 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num " +
                            "ELSE " +
                            "UPDATE Partita SET IDSQ2 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num";
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", dtb.Rows[0][3]));
                        command.Parameters.Add(new SqlParameter("sq1", dtb.Rows[0][1]));
                        command.Parameters.Add(new SqlParameter("num", number));
                        command.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                    //vittoria squadra 2
                    else if (setsq2 == 2)
                    {
                        conn.Open();
                        query = "IF(SELECT IDSQ1 FROM Partita  WHERE IdTorneo = @IDTorneo AND NumPartita = @num) IS NULL " +
                            "UPDATE Partita SET IDSQ1 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num " +
                            "ELSE " +
                            "UPDATE Partita SET IDSQ2 = @sq1 WHERE IdTorneo = @IDTorneo AND NumPartita = @num";
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", dtb.Rows[0][3]));
                        command.Parameters.Add(new SqlParameter("sq1", dtb.Rows[0][2]));
                        command.Parameters.Add(new SqlParameter("num", number));
                        command.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                    return false;
                }
                else return false;
            }
            catch { return false; }
        }
        public List<Classifica> GeneraClassifica(int idTorneo)
        {
            int[] rank = new int[10] { 1, 2, 3, 4, 5, 9, 17, 25, 33, 41 };
            double[] montepremiMoltiplicatori = new double[10] { 10, 9, 8, 7, 6, 4, 2, 1, 0.6, 0.4 };
            int counter = 0;
            int montepremiCounter = 0;
            double montepremi;
            try
            {
                //scarico montepremi
                conn.Open();
                string query = "SELECT Montepremi FROM Torneo WHERE IdTorneo=@IDTorneo";//scarico numero squadre
                SqlCommand command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                DataTable dtb = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dtb);
                montepremi = (Convert.ToDouble(dtb.Rows[0][0]) / 100);//numero di team
                conn.Close();

                List<Classifica> classifica = new List<Classifica>();
                //scarico team
                conn.Open();
                query = "SELECT COUNT(*) FROM Partita WHERE IdTorneo=@IDTorneo";//scarico numero squadre
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                int numteam = Convert.ToInt32(dtb.Rows[0][0]);//numero di team
                conn.Close();
                //scarico team che hanno finito la partita 
                conn.Open();
                query = "SELECT COUNT(*) FROM Partita WHERE IdTorneo=@IDTorneo AND (SetSQ1=2 OR SetSQ2=2)";//scarico numero squadre
                command = new SqlCommand(query, conn);
                command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                dtb = new DataTable();
                da = new SqlDataAdapter(command);
                da.Fill(dtb);
                int numteampartitaconclusa = Convert.ToInt32(dtb.Rows[0][0]);//numero di team
                conn.Close();
                int numpartita = GetLastNumberPartita(idTorneo);
                //tutte partite concluse
                if (numteam == numteampartitaconclusa)
                {
                    conn.Open();
                    query = "SELECT at1s1.CodiceTessera as CS1A1,at2s1.CodiceTessera as CS1A2," +
                        "at1s2.CodiceTessera as CS2A1,at2s2.CodiceTessera as CS2A2," +
                        "sq1.NomeTeam as SQ1,sq2.NomeTeam as SQ2,SetSQ1,SetSQ2 FROM Partita " +
                        "INNER JOIN Squadra as sq1 ON sq1.IDSquadra = Partita.IDSQ1 " +
                        "INNER JOIN Squadra as sq2 ON sq2.IDSquadra = Partita.IDSQ2 " +
                        "INNER JOIN Atleta as at1s1 ON at1s1.IDAtleta=sq1.IDAtleta1 " +
                        "INNER JOIN Atleta as at2s1 ON at2s1.IDAtleta = sq1.IDAtleta2 " +
                        "INNER JOIN Atleta as at1s2 ON at1s2.IDAtleta = sq2.IDAtleta1 " +
                        "INNER JOIN Atleta as at2s2 ON at2s2.IDAtleta = sq2.IDAtleta2 " +
                        "WHERE IdTorneo=@IDTorneo AND (NumPartita=@num OR Numpartita=@num1) ORDER BY NumPartita DESC";//prendo finali
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                    command.Parameters.Add(new SqlParameter("num", numpartita));
                    command.Parameters.Add(new SqlParameter("num1", numpartita - 1));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    //prendo i vincitori e i perdenti delle finali
                    for (int i = 0; i < 2; i++)
                    {
                        //se sq1 ha vinto 
                        if (Convert.ToInt32(dtb.Rows[i][6]) == 2)
                        {
                            classifica.Add(new Classifica(dtb.Rows[i][0].ToString(), dtb.Rows[i][1].ToString(), rank[counter], dtb.Rows[i][4].ToString(), (montepremi * montepremiMoltiplicatori[montepremiCounter]) * 2));
                            montepremiCounter++;
                            classifica.Add(new Classifica(dtb.Rows[i][2].ToString(), dtb.Rows[i][3].ToString(), rank[counter + 1], dtb.Rows[i][5].ToString(), (montepremi * montepremiMoltiplicatori[montepremiCounter]) * 2));
                            montepremiCounter++;
                        }
                        //se sq2 ha vinto
                        else if (Convert.ToInt32(dtb.Rows[i][7]) == 2)
                        {
                            classifica.Add(new Classifica(dtb.Rows[i][2].ToString(), dtb.Rows[i][3].ToString(), rank[counter], dtb.Rows[i][5].ToString(), (montepremi * montepremiMoltiplicatori[montepremiCounter]) * 2));
                            montepremiCounter++;
                            classifica.Add(new Classifica(dtb.Rows[i][0].ToString(), dtb.Rows[i][1].ToString(), rank[counter + 1], dtb.Rows[i][4].ToString(), (montepremi * montepremiMoltiplicatori[montepremiCounter]) * 2));
                            montepremiCounter++;
                        }
                        counter = 2;
                    }
                    counter = 4;
                    //prendo numpartita quarti
                    conn.Open();
                    query = "SELECT TOP(1) NumPartita FROM Partita WHERE IDTorneo=@IDTorneo AND Fase='Quarti' ORDER BY NumPartita DESC";//scarico numero squadre
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    numpartita = Convert.ToInt32(dtb.Rows[0][0]);//numero di team
                    conn.Close();
                    //prendo partite quarti ottavi sedicesimi
                    conn.Open();
                    query = "SELECT at1s1.CodiceTessera as CS1A1,at2s1.CodiceTessera as CS1A2," +
                        "at1s2.CodiceTessera as CS2A1,at2s2.CodiceTessera as CS2A2," +
                        "sq1.NomeTeam as SQ1,sq2.NomeTeam as SQ2,SetSQ1,SetSQ2,Fase FROM Partita " +
                        "INNER JOIN Squadra as sq1 ON sq1.IDSquadra = Partita.IDSQ1 " +
                        "INNER JOIN Squadra as sq2 ON sq2.IDSquadra = Partita.IDSQ2 " +
                        "INNER JOIN Atleta as at1s1 ON at1s1.IDAtleta=sq1.IDAtleta1 " +
                        "INNER JOIN Atleta as at2s1 ON at2s1.IDAtleta = sq1.IDAtleta2 " +
                        "INNER JOIN Atleta as at1s2 ON at1s2.IDAtleta = sq2.IDAtleta1 " +
                        "INNER JOIN Atleta as at2s2 ON at2s2.IDAtleta = sq2.IDAtleta2 " +
                        "WHERE IDTorneo=@IDTorneo AND NumPartita<=@num AND Fase NOT LIKE '%Pool%' ORDER BY NumPartita DESC";//prendo finali
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                    command.Parameters.Add(new SqlParameter("num", numpartita));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    //prendo i vincitori e i perdenti
                    for (int i = 0; i < dtb.Rows.Count; i++)
                    {
                        if (dtb.Rows[i][8].ToString() == "Quarti") { counter = 4; montepremiCounter = 4; }
                        else if (dtb.Rows[i][8].ToString() == "Ottavi") { counter = 5; montepremiCounter = 5; }
                        //se sq1 ha vinto 
                        if (Convert.ToInt32(dtb.Rows[i][6]) == 2)
                            classifica.Add(new Classifica(dtb.Rows[i][2].ToString(), dtb.Rows[i][3].ToString(), rank[counter], dtb.Rows[i][5].ToString(), (montepremi * montepremiMoltiplicatori[montepremiCounter]) * 2));
                        //se sq2 ha vinto
                        else if (Convert.ToInt32(dtb.Rows[i][7]) == 2)
                            classifica.Add(new Classifica(dtb.Rows[i][0].ToString(), dtb.Rows[i][1].ToString(), rank[counter], dtb.Rows[i][4].ToString(), (montepremi * montepremiMoltiplicatori[montepremiCounter]) * 2));
                    }
                    counter++;
                    montepremiCounter++;
                    //controllo num squadre torneo
                    conn.Open();
                    query = "SELECT COUNT(*) FROM Partecipa WHERE IdTorneo=@IDTorneo";//scarico numero squadre
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    int numsquadre = Convert.ToInt32(dtb.Rows[0][0]);//numero di team
                    conn.Close();
                    if (numsquadre == 24)
                    {
                        //prendo team pool
                        conn.Open();
                        query = "SELECT TOP(2) at1s1.CodiceTessera as CA1,at2s1.CodiceTessera as CA2,NomeTeam,Pool.IdSquadra FROM Pool " +
                            "INNER JOIN Squadra ON Squadra.IDSquadra = Pool.IDSquadra " +
                            "INNER JOIN Atleta as at1s1 ON at1s1.IDAtleta = Squadra.IDAtleta1 " +
                            "INNER JOIN Atleta as at2s1 ON at2s1.IDAtleta = Squadra.IDAtleta2 " +
                            "WHERE IDTorneo =@IDTorneo AND PP = 3 ORDER BY QP ASC";//scarico numero squadre
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                        dtb = new DataTable();
                        da = new SqlDataAdapter(command);
                        da.Fill(dtb);
                        conn.Close();
                        classifica.Add(new Classifica(dtb.Rows[1][0].ToString(), dtb.Rows[1][1].ToString(), rank[counter], dtb.Rows[1][2].ToString(), (montepremi * montepremiMoltiplicatori[montepremiCounter]) * 2));
                        classifica.Add(new Classifica(dtb.Rows[0][0].ToString(), dtb.Rows[0][1].ToString(), rank[counter], dtb.Rows[0][2].ToString(), (montepremi * montepremiMoltiplicatori[montepremiCounter]) * 2));
                    }
                    //4 nei pool
                    conn.Open();
                    query = "SELECT at1s1.CodiceTessera as CA1,at2s1.CodiceTessera as CA2,NomeTeam,Pool.IdSquadra FROM Pool " +
                        "INNER JOIN Squadra ON Squadra.IDSquadra = Pool.IDSquadra " +
                        "INNER JOIN Atleta as at1s1 ON at1s1.IDAtleta = Squadra.IDAtleta1 " +
                        "INNER JOIN Atleta as at2s1 ON at2s1.IDAtleta = Squadra.IDAtleta2 " +
                        "WHERE IDTorneo = @IDTorneo AND PP = 0 ORDER BY QP DESC";
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    //prendo i vincitori e i perdenti
                    for (int i = 0; i < dtb.Rows.Count; i++)
                    {
                        classifica.Add(new Classifica(dtb.Rows[i][0].ToString(), dtb.Rows[i][1].ToString(), rank[counter], dtb.Rows[i][2].ToString(), (montepremi * montepremiMoltiplicatori[montepremiCounter]) * 2));
                    }
                    counter++;
                    montepremiCounter++;
                    //prendo qulifiche se ci sono
                    conn.Open();
                    query = "SELECT IDTorneo FROM Torneo WHERE Titolo LIKE '%'+(SELECT Titolo FROM Torneo WHERE IDTorneo=@IDTorneo)+'%' AND IDTorneo != @IDTorneo";//prendo finali
                    command = new SqlCommand(query, conn);
                    command.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                    dtb = new DataTable();
                    da = new SqlDataAdapter(command);
                    da.Fill(dtb);
                    conn.Close();
                    //ci sono qualfiche
                    if (dtb.Rows.Count > 0)
                    {
                        int idtorneoqualifica = Convert.ToInt32(dtb.Rows[0][0]);
                        //prendo tutte le partite di qualifica
                        conn.Open();
                        query = "SELECT at1s1.CodiceTessera as CS1A1,at2s1.CodiceTessera as CS1A2," +
                        "at1s2.CodiceTessera as CS2A1,at2s2.CodiceTessera as CS2A2," +
                        "sq1.NomeTeam as SQ1,sq2.NomeTeam as SQ2,SetSQ1,SetSQ2 FROM Partita " +
                        "INNER JOIN Squadra as sq1 ON sq1.IDSquadra = Partita.IDSQ1 " +
                        "INNER JOIN Squadra as sq2 ON sq2.IDSquadra = Partita.IDSQ2 " +
                        "INNER JOIN Atleta as at1s1 ON at1s1.IDAtleta=sq1.IDAtleta1 " +
                        "INNER JOIN Atleta as at2s1 ON at2s1.IDAtleta = sq1.IDAtleta2 " +
                        "INNER JOIN Atleta as at1s2 ON at1s2.IDAtleta = sq2.IDAtleta1 " +
                        "INNER JOIN Atleta as at2s2 ON at2s2.IDAtleta = sq2.IDAtleta2 " +
                        "WHERE IDTorneo =@IDTorneo ORDER BY NumPartita DESC";//prendo finali
                        command = new SqlCommand(query, conn);
                        command.Parameters.Add(new SqlParameter("IDTorneo", idtorneoqualifica));
                        dtb = new DataTable();
                        da = new SqlDataAdapter(command);
                        da.Fill(dtb);
                        conn.Close();
                        //prendo i vincitori e i perdenti
                        for (int i = 0; i < dtb.Rows.Count; i++)
                        {
                            //se sq1 ha vinto 
                            if (Convert.ToInt32(dtb.Rows[i][6]) == 2)
                                classifica.Add(new Classifica(dtb.Rows[i][2].ToString(), dtb.Rows[i][3].ToString(), rank[counter], dtb.Rows[i][5].ToString(), (montepremi * montepremiMoltiplicatori[montepremiCounter]) * 2));
                            //se sq2 ha vinto
                            else if (Convert.ToInt32(dtb.Rows[i][7]) == 2)
                                classifica.Add(new Classifica(dtb.Rows[i][0].ToString(), dtb.Rows[i][1].ToString(), rank[counter], dtb.Rows[i][4].ToString(), (montepremi * montepremiMoltiplicatori[montepremiCounter]) * 2));
                        }
                    }
                    return classifica;
                }
            }
            catch (Exception e)
            {
                List<Classifica> error = new List<Classifica>();
                error.Add(new Classifica(null, null, 0, e.Message.ToString(), 0));
                return error;
            }
            return null;
        }
        public DataTable CreateDataTable<T>(IEnumerable<T> list)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            DataTable dataTable = new DataTable();
            dataTable.TableName = typeof(T).FullName;
            foreach (PropertyInfo info in properties)
            {
                dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            foreach (T entity in list)
            {
                object[] values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(entity);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
        public string[] GetTourTornei()
        {
            try
            {
                conn.Open();
                string query = "SELECT Tour FROM Torneo GROUP BY Tour";//scarico tour tornei
                SqlCommand command = new SqlCommand(query, conn);
                DataTable dtb = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dtb);
                conn.Close();
                return dtb.Rows.OfType<DataRow>().Select(k => k[0].ToString()).ToArray();
            }
            catch 
            {
                return null;
            }
        }
    }
}
