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
            sql += "SELECT IDSocieta,Comune.Citta,NomeSocieta,Indirizzo,CAP,DataFondazione,DataAffiliazione,CodiceAffiliazione,Affiliata,Email,Sito,Tel1,Tel2,Pec,PIVA,CF,CU,Presidente,Referente ";
            sql += "FROM Societa ";
            sql += "LEFT JOIN Comune ON Societa.IDComune = Comune.IDComune ";
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
        public DataTable GetTorneiTipo(int idTipo)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable risultato;
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,Societa.NomeSocieta,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.DataChiusuraIscrizioni,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,NumTeamQualificati,NumWildCard,Outdoor,RiunioneTecnica,OraInizio " +
            "FROM((((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On Torneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune)LEFT JOIN Societa On Societa.IDSocieta = Torneo.IDSocieta) " +
            "WHERE TipoTorneo.IDtipotorneo=@tipoTorneo";
            comando = new SqlCommand(sql, conn);
            comando.Parameters.Add(new SqlParameter("tipoTorneo", idTipo));
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
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,PuntiVittoria,Torneo.Montepremi,DataInizio,DataFine,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,Squadra.NomeTeam,CONCAT(atleta1.Nome, ' ', atleta1.cognome) AS Atleta1, CONCAT(atleta2.Nome, ' ', atleta2.cognome) AS Atleta2 "+
            "FROM(((((Torneo "+
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
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,Societa.NomeSocieta,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.DataChiusuraIscrizioni,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,NumTeamQualificati,NumWildCard,Outdoor,RiunioneTecnica,OraInizio "+
            "FROM((((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On Torneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune)LEFT JOIN Societa On Societa.IDSocieta = Torneo.IDSocieta) "+
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
                if (codTessera != null)
                    parametro = new SqlParameter("CodiceTessera", codTessera);
                else
                    parametro = new SqlParameter("CodiceTessera", DBNull.Value);
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
                string c = e.Message;
            }
            return regRiuscita;
        }
        public bool RegisterDelegato(string nome, string cognome, string sesso, string cF, DateTime dataNascita, string comuneNascita, string comuneResidenza, string indirizzo, string cap, string email, string tel, bool arbitro, bool supervisore, string pwd)
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
        public bool RegisterSocieta(string comune, string nomeSocieta, string indirizzo,string citta, string cap, DateTime dataFondazione, DateTime dataAffilizione, string codAffiliazione, bool affiliata, string email,string presidente,string referente, string sito, string tel1, string tel2, string pec, string piva, string cF, string cU, string pwd)
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
                    regRiuscita = true;
                }
            }
            catch (Exception e)
            {
                string c = e.Message;
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
            catch(Exception e)
            {
                conn.Close();
                return risultato;
            }
        }
        public bool UploadResults(int idTorneo, int idPartita, int pt1s1, int pt2s1, int pt1s2, int pt2s2, int pt1s3, int pt2s3, int numSet)
        {
            SqlCommand comando;
            SqlParameter parametro;
            SqlDataAdapter adapter;
            DataTable puntiVittoria;
            string sql;
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
                //Update del punteggio
                sql = "" + 
                "UPDATE Partita " +
                "SET PT1S1=@pt1s1,PT2S1=@pt2s1" +
                ",PT1S2=@pt1s2,PT2S2=@pt2s2" +
                ",PT1S3=@pt1s3,PT2S3=@pt2s3 " +
                ",SetSQ1=@SetSQ1,SetSQ2=@SetSQ2 " +
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
                //Controllo se qualcuno ha vinto il set
                if ((((pt1s1 - pt2s1) >= 2 || (pt1s1 - pt2s1) <= -2) && (pt1s1 >= Convert.ToInt32(puntiVittoria.Rows[0]["PuntiVittoria"]) || pt2s1 >= Convert.ToInt32(puntiVittoria.Rows[0]["PuntiVittoria"]))) || (((pt1s2 - pt2s2) >= 2 || (pt1s2 - pt2s2) <= -2) && (pt1s2 >= Convert.ToInt32(puntiVittoria.Rows[0]["PuntiVittoria"]) || pt2s2 >= Convert.ToInt32(puntiVittoria.Rows[0]["PuntiVittoria"]))) || (((pt1s3 - pt2s3) >= 2 || (pt1s3 - pt2s3) <= -2) && (pt1s3 >= Convert.ToInt32(puntiVittoria.Rows[0]["PuntiVittoria"]) || pt2s3 >= Convert.ToInt32(puntiVittoria.Rows[0]["PuntiVittoria"]))))
                {
                    if (Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ1"]) < 3 && Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ2"]) < 3)
                    {
                        //Controllo a che set sono
                        switch (numSet)
                        {
                            case 1:
                                if ((pt1s1 - pt2s1) >= 2)
                                    comando.Parameters.Add(new SqlParameter("SetSQ1", Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ1"]) + 1));
                                else
                                    comando.Parameters.Add(new SqlParameter("SetSQ1", Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ2"]) + 1));
                                break;
                            case 2:
                                if ((pt1s2 - pt2s2) >= 2)
                                    comando.Parameters.Add(new SqlParameter("SetSQ1", Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ1"]) + 1));
                                else
                                    comando.Parameters.Add(new SqlParameter("SetSQ1", Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ2"]) + 1));
                                break;
                            case 3:
                                if ((pt1s3 - pt2s3) >= 2)
                                    comando.Parameters.Add(new SqlParameter("SetSQ1", Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ1"]) + 1));
                                else
                                    comando.Parameters.Add(new SqlParameter("SetSQ1", Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ2"]) + 1));
                                break;
                        }
                    }
                }
                else //Se non è finito il set, lascio i set vinti di prima
                {
                    comando.Parameters.Add(new SqlParameter("SetSQ1", Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ1"])));
                    comando.Parameters.Add(new SqlParameter("SetSQ1", Convert.ToInt32(GetNumSetVinti(idTorneo, idPartita).Rows[0]["SetSQ2"])));
                }
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
            sql += "SELECT " +
            "CONCAT(A1.Nome,' ',A1.cognome) as Atleta1,CONCAT(A2.Nome,' ',A2.cognome) as Atleta2,S1.NomeTeam as Team1, " +
            "CONCAT(A3.Nome,' ',A3.cognome) as Atleta3,CONCAT(A4.Nome,' ',A4.cognome) AS Atleta4, S2.NomeTeam as Team2, " +
            "CONCAT(D1.Nome,' ',D1.Cognome) as Arbitro1,CONCAT(D2.Nome,' ',D2.Cognome) as Arbitro2, " +
            "Partita.Fase,Partita.Campo,Partita.DataPartita,Partita.OraPartita, " +
            "Partita.PT1S1 as PtTeam1Set1,Partita.PT2S1 as PtTeam2Set1,Partita.PT1S2 as PtTeam1Set2,Partita.PT2S2 as PtTeam2Set2, " +
            "Partita.PT1S3 as PtTeam1Set3,Partita.PT2S3 as PtTeam2Set3 " +
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
            sql += "SELECT DISTINCT Torneo.Titolo,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.Gender,Torneo.NumMaxTeamMainDraw,Torneo.NumMaxTeamQualifiche,Torneo.NumTeamQualificati,Torneo.NumWildCard,Outdoor,Torneo.IDImpianto,RiunioneTecnica,OraInizio " +
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
            "WHERE Torneo.IDTorneo=@IDTorneo AND ParametroTorneo.idtorneo = Torneo.idtorneo AND ParametroTorneo.IDParametro = ParametroQualita.IDParametro";
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
        public bool CreaTorneo(string titolo, int puntiVittoria, double montepremi, DateTime dataChiusuraIscrizioni, DateTime dataInizio, DateTime dataFine, char genere, string formulaTorneo, int NumMaxTeamMainDraw, int NumMaxTeamQualifiche, string[] parametriTorneo, string tipoTorneo, double quotaIscrizione, int idSocieta, int numTeamQualificati, int numWildCard, int idImpianto, bool outdoor, bool riunioneTecnica, string oraInizio)
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
                sql += "INSERT INTO Torneo(IDSocieta,IDTipoTorneo,IDFormula,Titolo,PuntiVittoria,Montepremi,DataChiusuraIscrizioni,DataInizio,DataFine,Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,QuotaIscrizione,NumTeamQualificati,NumWildCard,IDImpianto,Outdoor,RiunioneTecnica,OraInizio) ";
                sql += "VALUES(@IDSocieta,@IDTipoTorneo,@IDFormula,@Titolo,@PuntiVittoria,@Montepremi,@DataChiusuraIScrizioni,@DataInzio,@DataFine,@Gender,@NumMaxTeamMainDraw,@NumMaxTeamQualifiche,@QuotaIscrizione,@NumTeamQualificati,@NumWildCard,@IDImpianto,@Outdoor,@RiunioneTecnica,@OraInizio)";
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
                        parametro = new SqlParameter("IDTorneo",idTorneo.Rows[0][0]);
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
        public bool IscriviSquadra(int idTorneo, int idSquadra, string idAllenatore)
        {
            SqlDataAdapter adapter;
            SqlCommand comando;
            SqlParameter parametro;
            DataTable query, numMaxQualifiche, numTeamQualifiche;
            string sql;
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
                        sql += "SELECT NumMaxTeamQualifiche FROM Torneo WHERE IDTorneo=@IDTorneo";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                        adapter = new SqlDataAdapter(comando);
                        numMaxQualifiche = new DataTable();
                        conn.Open();
                        adapter.Fill(numMaxQualifiche);
                        conn.Close();
                        //Prendo il numero di team che sarebbero in qualifica
                        sql = "";
                        sql += "SELECT COUNT(*) FROM ListaIscritti WHERE WC=0 AND IDSquadra NOT IN(SELECT TOP  "; //Da completare
                        sql = "";
                        sql += "SELECT Sesso FROM Atleta, Squadra WHERE((Squadra.IDAtleta1 = @IDAtleta1 AND Atleta.IDAtleta = Squadra.IDAtleta1) OR(Squadra.IDAtleta2 = @IDAtleta2 AND Atleta.IDAtleta = Squadra.IDAtleta2))";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDAtleta1", query.Rows[0]["IDAtleta1"]));
                        comando.Parameters.Add(new SqlParameter("IDAtleta2", query.Rows[0]["IDAtleta2"]));
                        query = new DataTable();
                        adapter = new SqlDataAdapter(comando);
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
                                        int idAllenatore1 = GetIDAllenatore(idAllenatore);
                                        sql = "";
                                        sql += "INSERT INTO ListaIscritti(IDSquadra,IDTorneo,IDAllenatore,DataIscrizione,Cancellata)VALUES (@IDSquadra,@IDTorneo,@IDAllenatore,@DataIscrizione,@Cancellata)";
                                        comando = new SqlCommand(sql, conn);
                                        parametro = new SqlParameter("IDSquadra", idSquadra);
                                        comando.Parameters.Add(parametro);
                                        parametro = new SqlParameter("IDTorneo", idTorneo);
                                        comando.Parameters.Add(parametro);
                                        if (idAllenatore1 != 0)
                                            parametro = new SqlParameter("IDAllenatore", idAllenatore1);
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

        public string CreaLista(int idTorneo)
        {
            SqlCommand comando;
            SqlDataAdapter adapter;
            DataTable query, numTabelloneWildCard; //DataTable per raccogliere il numero di team del tabellone e il numero di wildcard
            string sql;
            try
            {
                //Prendo le squadre con la wildcard
                sql = "";
                sql += "SELECT * FROM ListaIscritti WHERE IDTorneo=@IDTorneo AND WC=1";
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
                sql += "SELECT NumMaxTeamMainDraw,NumWildCard FROM Torneo WHERE IDTorneo=@IDTorneo";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                adapter = new SqlDataAdapter(comando);
                numTabelloneWildCard = new DataTable();
                conn.Open();
                adapter.Fill(numTabelloneWildCard);
                conn.Close();
                //Prendo le squadre che vanno direttamente nel tabellone
                sql = "";
                sql += "SELECT TOP @NumeroTeamTabellone * FROM ListaIscritti WHERE IDTorneo=@IDTorneo AND WC=0 ORDER BY EntryPoints DESC";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                comando.Parameters.Add(new SqlParameter("NumeroTeamTabellone", Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumMaxTeamMainDraw"]) - Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumWildCard"])));
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
                return "Lista di ingresso creata con successo!";
            }
            catch (Exception e)
            {
                return "Errore: " + e.Message;
            }
        }
        //Metodo che crea il torneo di qualifica
        public string CreaTorneoQualifica(int idTorneo, DateTime dataInizioQualifiche, DateTime dataFineQualifiche, DateTime dataPartite2Turno)
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
                    sql += "INSERT INTO Torneo(IDSocieta,IDTipoTorneo,IDFormula,Titolo,PuntiVittoria,Montepremi,DataChiusuraIscrizioni,DataInizio,DataFine,Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,QuotaIscrizione,NumTeamQualificati,NumWildCard,Autorizzato,IDImpianto,Outdoor,RiunioneTecnica,OraInizio) ";
                    sql += "VALUES(@IDSocieta,@IDTipoTorneo,@IDFormula,@Titolo,@PuntiVittoria,@Montepremi,@DataChiusuraIscrizioni,@DataInzio,@DataFine,@Gender,@NumMaxTeamMainDraw,@NumMaxTeamQualifiche,@QuotaIscrizione,@NumTeamQualificati,@NumWildCard,1,@IDImpianto,@Outdoor,@RiunioneTecnica,@OraInizio)";
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
                    conn.Open();
                    comando.ExecuteNonQuery();
                    conn.Close();
                    //Prendo l'id del torneo di qualifica
                    sql = "";
                    sql += "SELECT IDTorneo FROM Torneo WHERE Titolo=@Titolo";
                    comando = new SqlCommand(sql, conn);
                    comando.Parameters.Add(new SqlParameter("Titolo", torneoPrincipale[0].Rows[0]["Titolo"] += " Qualifiche"));
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
                    sql += "SELECT TOP @NumMaxTeamQualifiche * FROM ListaIscritti WHERE WC=0 AND IDTorneo=@IDTorneo AND IDSquadra NOT IN (SELECT TOP @NumeroTeamTabellone IDSquadra FROM ListaIscritti WHERE IDTorneo=@IDTorneo AND WC=0 ORDER BY EntryPoints DESC)";
                    comando = new SqlCommand(sql, conn);
                    comando.Parameters.Add(new SqlParameter("NumMaxTeamQualifiche", numTabelloneWildCard.Rows[0]["NumMaxTeamQualifiche"]));
                    comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneo));
                    comando.Parameters.Add(new SqlParameter("NumeroTeamTabellone", Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumMaxTeamMainDraw"]) - Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumWildCard"])));
                    adapter = new SqlDataAdapter(comando);
                    squadreQualifica = new DataTable();
                    conn.Open();
                    adapter.Fill(squadreQualifica);
                    conn.Close();
                    //Collego le squadre al torneo di qualifica
                    for (int i = 0; i < squadreQualifica.Rows.Count; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partecipa(IDSquadra,IDTorneo,IDAllenatore,EntryPoints) ";
                        sql += "VALUES (@IDSquadra,@IDTorneo,@IDAllenatore,@EntryPoints)";
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
                            PartiteNTQ4_8(Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumMaxTeamQualifiche"]), squadreQualifica, Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumTeamQualificati"]), Convert.ToInt32(idTorneoQualifica.Rows[0]["IDTorneo"]), dataPartite2Turno,dataInizioQualifiche);
                            break;

                        case 4:
                            PartiteNTQ4_8(Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumMaxTeamQualifiche"]), squadreQualifica, Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumTeamQualificati"]), Convert.ToInt32(idTorneoQualifica.Rows[0]["IDTorneo"]), dataPartite2Turno, dataInizioQualifiche);
                            break;

                        case 6:
                            PartiteNTQ6(Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumMaxTeamQualifiche"]), squadreQualifica, Convert.ToInt32(numTabelloneWildCard.Rows[0]["NumTeamQualificati"]), Convert.ToInt32(idTorneoQualifica.Rows[0]["IDTorneo"]), dataPartite2Turno, dataInizioQualifiche);
                            break;
                    }
                    return "Torneo di qualifica creato!";
                }
                else
                    return "Torneo principale non trovato";
            }
            catch (Exception e)
            {
                return "Errore: " + e.Message;
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
                if (numSquadreQualifiche <= 8)
                {
                    for(int i = 0; i < 4; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDSQ1,IDSQ2,IDTorneo,NumPartita,Fase,DataPartita) " +
                            "VALUES (@IDSQ1,@IDSQ2,@IDTorneo,@NumPartita,@Fase,@DataPartita)";
                        comando = new SqlCommand(sql, conn);
                        if (squadreQualifica.Rows[i]["IDSquadra"] != null)//Prima squadra
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
                        if (squadreQualifica.Rows[7 - i]["IDSquadra"] != null)//Seconda squadra
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[7 - i]["IDSquadra"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
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
                        comando.Parameters.Add(new SqlParameter("Fase", "Singolo turno eliminatorio"));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                if(numSquadreQualifiche>8 || numSquadreQualifiche <= 16)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDSQ1,IDSQ2,IDTorneo,NumPartita,Fase,DataPartita) " +
                            "VALUES (@IDSQ1,@IDSQ2,@IDTorneo,@NumPartita,@Fase,@DataPartita)";
                        comando = new SqlCommand(sql, conn);
                        if (squadreQualifica.Rows[i]["IDSquadra"] != null)
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
                        if (squadreQualifica.Rows[15 - i]["IDSquadra"] != null)
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[15 - i]["IDSquadra"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
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
                        comando.Parameters.Add(new SqlParameter("Fase", "Primo turno eliminatorio"));
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
                            sql += "INSERT INTO Partita(IDTorneo,NumPartita,Fase,DataPartita)" +
                                "VALUES(@IDTorneo,@NumPartita,@Fase,@DataPartita)";
                            comando = new SqlCommand(sql, conn);
                            comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                            comando.Parameters.Add(new SqlParameter("NumPartita", i + 9));
                            comando.Parameters.Add(new SqlParameter("Fase", "Secondo turno eliminatorio"));
                            comando.Parameters.Add(new SqlParameter("DataPartita", dataPartite2Turno.Date));
                            conn.Open();
                            comando.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                }
                if (numSquadreQualifiche > 16 || numSquadreQualifiche <= 32)
                {

                }
                if (numSquadreQualifiche > 32 || numSquadreQualifiche <= 64)
                {

                }
            }
            catch(Exception e)
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
                    for(int i = 0; i < 6; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDSQ1,IDSQ2,IDTorneo,NumPartita,Fase,DataPartita) " +
                            "VALUES (@IDSQ1,@IDSQ2,@IDTorneo,@NumPartita,@Fase,@DataPartita)";
                        comando = new SqlCommand(sql, conn);
                        if (squadreQualifica.Rows[i]["IDSquadra"] != null)//Prima squadra
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
                        if (squadreQualifica.Rows[11 - i]["IDSquadra"] != null)//Seconda squadra
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[11 - i]["IDSquadra"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
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
                        sql += "INSERT INTO Partita(IDSQ1,IDSQ2,IDTorneo,NumPartita,Fase,DataPartita) " +
                            "VALUES (@IDSQ1,@IDSQ2,@IDTorneo,@NumPartita,@Fase,@DataPartita)";
                        comando = new SqlCommand(sql, conn);
                        if (squadreQualifica.Rows[i]["IDSquadra"] != null)//Prima squadra
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[i]["IDSquadra"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
                        if (squadreQualifica.Rows[23 - i]["IDSquadra"] != null)//Seconda squadra
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreQualifica.Rows[23 - i]["IDSquadra"]));
                        else
                            comando.Parameters.Add(new SqlParameter("IDSQ1", squadreBye.Rows[0]["IDSquadra"]));
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
                                comando.Parameters.Add(new SqlParameter("NumPartita", 12));
                                break;
                            case 2:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 7));
                                break;
                            case 3:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 6));
                                break;
                            case 4:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 4));
                                break;
                            case 5:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 9));
                                break;
                            case 6:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 10));
                                break;
                            case 7:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 3));
                                break;
                            case 8:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 5));
                                break;
                            case 9:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 8));
                                break;
                            case 10:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 11));
                                break;
                            case 11:
                                comando.Parameters.Add(new SqlParameter("NumPartita", 2));
                                break;
                        }
                        comando.Parameters.Add(new SqlParameter("Fase", "Primo turno eliminatorio"));
                        conn.Open();
                        comando.ExecuteNonQuery();
                        conn.Close();
                    }
                    //Secondo turno eliminatorio
                    for(int i = 0; i < 6; i++)
                    {
                        sql = "";
                        sql += "INSERT INTO Partita(IDTorneo,NumPartita,Fase,DataPartita) " +
                            "VALUES (@IDTorneo,@NumPartita,@Fase,@DataPartita)";
                        comando = new SqlCommand(sql, conn);
                        comando.Parameters.Add(new SqlParameter("IDTorneo", idTorneoQualifica));
                        comando.Parameters.Add(new SqlParameter("DataPartita", dataPartite2Turno.Date));
                        comando.Parameters.Add(new SqlParameter("NumPartita", i + 13));
                        comando.Parameters.Add(new SqlParameter("Fase", "Secondo turno eliminatorio"));
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
        public DataTable GetTorneiDisputatiByDelegato(int idDelegato)
        {
            //Metodo che retituisce i tornei a cui ha partecipato un supervisore
            SqlDataAdapter adapter;
            SqlCommand comando;
            string sql;
            DataTable query;
            sql = "";
            sql += "SELECT DISTINCT Torneo.IDTorneo,Torneo.Titolo,Societa.NomeSocieta,TipoTorneo.Descrizione AS TipoTorneo,CONCAT(Supervisore.Nome,' ',Supervisore.Cognome) as SupervisoreTorneo,CONCAT(SupervisoreArbitrale.Nome,' ',SupervisoreArbitrale.Cognome) AS SupervisoreArbitrale,CONCAT(DirettoreCompetizione.Nome,' ',DirettoreCompetizione.Cognome) as DirettoreCompetizione,FormulaTorneo.Formula,Impianto.NomeImpianto,Comune.Citta,Torneo.QuotaIscrizione,Torneo.PuntiVittoria,Torneo.Montepremi,Torneo.DataInizio,Torneo.DataFine,Torneo.DataChiusuraIscrizioni,Torneo.Gender,NumMaxTeamMainDraw,NumMaxTeamQualifiche,NumTeamQualificati,NumWildCard,Outdoor,RiunioneTecnica,OraInizio "+
            "FROM((((((((((Torneo LEFT JOIN TipoTorneo On Torneo.IDTipoTorneo = TipoTorneo.IDTipoTorneo)LEFT JOIN DelegatoTecnico Supervisore ON Torneo.IDSupervisore = Supervisore.IDDelegato)LEFT JOIN ArbitraTorneo On ArbitraTorneo.IDDelegato = Torneo.IDSupervisoreArbitrale)LEFT JOIN DelegatoTecnico SupervisoreArbitrale On Torneo.IDSupervisoreArbitrale = SupervisoreArbitrale.IDDelegato)LEFT JOIN DelegatoTecnico DirettoreCompetizione On Torneo.IDDirettoreCompetizione = DirettoreCompetizione.IDDelegato)LEFT JOIN FormulaTorneo ON Torneo.IDFormula = FormulaTorneo.IDFormula)LEFT JOIN ImpiantoTorneo On ImpiantoTorneo.IDTorneo = Torneo.IDTorneo)LEFT JOIN Impianto On Torneo.IDImpianto = Impianto.IDImpianto)LEFT JOIN Comune On Impianto.IDComune = Comune.IDComune)LEFT JOIN Societa On Societa.IDSocieta = Torneo.IDSocieta) "+
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
            catch(Exception e)
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
            sql += "SELECT Partita.NumPartita,Squadra1.NomeTeam,Squadra2.NomeTeam,CONCAT(Arbitro1.Nome,' ',Arbitro1.Cognome) As Arbitro1,CONCAT(Arbitro2.Nome,' ',Arbitro2.Cognome) As Arbitro2,Torneo.Titolo,Partita.Fase,Partita.Campo,Partita.DataPartita,Partita.OraPartita,Partita.Risultato,Partita.Durata,Partita.PT1S1,Partita.PT2S1,Partita.PT1S2,Partita.PT2S2,Partita.PT1S3,Partita.PT2S3,Partita.SetSQ1,Partita.SetSQ2 " +
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
        public string AssegnaInfoPartita(int idArbitro1, int idArbitro2, string campo, DateTime dataPartita, DateTime oraPartita, int idPartita)
        {
            string sql;
            SqlCommand comando;
            try
            {
                sql = "";
                sql += "UPDATE Partita SET IDArbitro1=@IDArbitro1,IDArbitro2=@IDArbitro2,Campo=@Campo,DataPartita=@DataPartita,OraPartita=@OraPartita " +
                    "WHERE IDPartita=@IDPartita";
                comando = new SqlCommand(sql, conn);
                comando.Parameters.Add(new SqlParameter("IDArbitro1", idArbitro1));
                comando.Parameters.Add(new SqlParameter("IDArbitro2", idArbitro2));
                comando.Parameters.Add(new SqlParameter("Campo", campo));
                comando.Parameters.Add(new SqlParameter("DataPartita", dataPartita.Date));
                comando.Parameters.Add(new SqlParameter("OraPartita", oraPartita.TimeOfDay));
                comando.Parameters.Add(new SqlParameter("IDPartita", idPartita));
                conn.Open();
                comando.ExecuteNonQuery();
                conn.Close();
                return "Info assegnate correttamente";
            }
            catch(Exception e)
            {
                return "ERRORE: " + e.Message;
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
            catch(Exception e)
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
                for(int i = 0; i < delegatiTorneo.Rows.Count; i++)
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
            catch(Exception e)
            {
                return "ERRORE: " + e.Message;
            }
        }
        public string AvanzaTabelloneQualifiche(int idTorneo, int idPartita, int pt1s1, int pt2s1, int pt1s2, int pt2s2, int pt1s3, int pt2s3, int numSet)
        {
            //Metodo per l'avanzamento nel tabellone di qualifiche
            //Utilizzo il metodo UploadResults per registrare i punti partita
            if (UploadResults(idTorneo, idPartita, pt1s1, pt2s1, pt1s2, pt2s2, pt1s3, pt2s3, numSet))
            {

                return "Avanzamento avvenuto con successo";
            }
            else
                return "Problemi con la registrazione dei punti";
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
            catch(Exception exc)
            {
                return "Errore: " + exc.Message;
            }
        }
    }
}
