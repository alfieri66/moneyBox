using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

using System.Net;
using System.Net.Mime;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Security.Authentication;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel.DataAnnotations;

namespace moneyBox
{
    /// <summary>
    /// Descrizione di riepilogo per interrogaDB
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Per consentire la chiamata di questo servizio Web dallo script utilizzando ASP.NET AJAX, rimuovere il commento dalla riga seguente. 
    [System.Web.Script.Services.ScriptService]
    public class interrogaDB : System.Web.Services.WebService
    {
        Random x = new Random(DateTime.Now.Millisecond);
        public struct TinfoUtente
        {
            public string ruolo;
            public string email;
            public string password;
            public string nuovaEmail;
            public string nuovaPassword;
            public string nuovaRipetiPassword;
        }

        public struct TLocali
        {
            public string stato;
            public int idLocale;
            public string nome;
            public string citta;
            public string indirizzo;
            public string tel;
            public Int64 codiceLocale;
        }

        public struct TUtenti
        {
            public int idUtente;
            public string stato;
            public string nome;
            public string cognome;
            public string email;
            public Boolean funzioniPlus;
            public string password;
            public string ripetiPassword;
        }

        public struct tDettaglioCassa
        {
            public string stato;
            public string idIncasso;
            public string nomeUtente;
            public string nomeLocale;
            public string data;
            public string acconto;
            public string recupero;
            public string daRiportare;
        }


        public struct tIncassoTotaleAgenti
        {
            public string stato;
            public string idAgente;
            public string nomeUtente;
            public string acconto;
            public string recupero;
            public string daRiportare;
            public string totCassa;
            public string flussoCassa;
            public string cassaGenerale;
        }

        public struct tIncassoDettagliatoAgente
        {
            public string stato;
            public string idAgente;
            public string idLocale;
            public string nomeUtente;
            public string nomeLocale;
            public string cittaLocale;
            public string oraIncasso;
            public string acconto;
            public string recupero;
            public string daRiportare;
        }

        struct tInfoAgenti
        {
            public string totAcconto;
            public string totDaRiportare;
            public string totRecupero;
            public string dettaglio;
            public string totali;
        }

        struct tCassa
        {
            public string locale;
            public string utenteNome;
            public string utenteCognome;
            public string dataIni;
            public string dataFin;
            public string totAcconto;
            public string totDaRiportare;
            public string totRecupero;
            public List<tDettaglioCassa> dettaglio;
        }

        public struct tRecEsito
        {
            public Boolean esito;
            public string messaggio;
        }
        MySqlConnection Connessione = new MySqlConnection();
        MySqlCommand comandoSQL = new MySqlCommand();
        MySqlDataReader tabella;
        cCostanti costanti = new cCostanti();
        cCrittografia crittoMd5 = new cCrittografia();


        public static bool mailCorretta(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public Boolean nullaOsta()
        {
            Boolean esito;
            if (Session["loginAdmin"] == null || (string)Session["loginAdmin"] != "OK")
            {
                esito = false;
            }
            else
            {
                esito = true;
            }
            return esito;
        }
        private bool apriDB()
        {
            bool riuscito = true;
            try
            {
                Connessione.ConnectionString = costanti.stringaConnessione;
                Connessione.Open();
            }
            catch
            {
                riuscito = false;
            }
            return riuscito;
        }

        private void chiudiDB()
        {
            Connessione.Close();
        }

        private int cercaIncasso(int idLocale, int idUtente, string dataIncasso)
        {
            int idIncasso = 0;
            string stringaSql;
            if (apriDB())
            {
                comandoSQL.Parameters.Clear();
                stringaSql = "SELECT idIncasso FROM incassi WHERE idLocale=@idLocale AND idUtente=@idUtente AND DATE(data)=@dataIncasso";
                comandoSQL.Parameters.AddWithValue("@idLocale", idLocale);
                comandoSQL.Parameters.AddWithValue("@idUtente", idUtente);
                comandoSQL.Parameters.AddWithValue("@dataIncasso", dataIncasso);

                comandoSQL.CommandText = stringaSql;
                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();
                if (tabella.Read())
                {
                    idIncasso = (int)tabella["idIncasso"];
                }
                chiudiDB();
            }
            return idIncasso;
        }

        private int recuperaIdLocale(long codiceLocale)
        {
            int idLocale = 0;
            string stringaSql;
            if (apriDB())
            {
                comandoSQL.Parameters.Clear();
                stringaSql = "SELECT * FROM locali WHERE codiceLocale=@codiceLocale";
                comandoSQL.Parameters.AddWithValue("@codiceLocale", codiceLocale);
                comandoSQL.CommandText = stringaSql;
                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();
                if (tabella.Read())
                {
                    idLocale = (int)tabella["idLocale"];
                }
                chiudiDB();
            }
            return idLocale;
        }

        private int recuperaIdUtente(string emailUtente)
        {
            string stringaSql;
            int idUtente = 0;
            if (apriDB())
            {
                comandoSQL.Parameters.Clear();
                stringaSql = "SELECT idUtente FROM utenti WHERE email=@emailUtente";
                comandoSQL.Parameters.AddWithValue("@emailUtente", emailUtente);
                comandoSQL.CommandText = stringaSql;
                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();
                if (tabella.Read())
                {
                    idUtente = (int)tabella["idUtente"];
                }
                chiudiDB();
            }
            return idUtente;
        }


        [WebMethod(EnableSession = true)]
        public string logout()
        {
            Session.Abandon();
            return "{'esito': 'true'}";
        }



        [WebMethod(EnableSession = true)]

        public string verificaLogin()
        {
            string stringaJson;
            tRecEsito recEsito;

            recEsito.esito = true;
            recEsito.messaggio = "";

            if (Session["loginAdmin"] == null || (string)Session["loginAdmin"] != "OK")
            {
                recEsito.esito = false;
            }
            stringaJson = JsonConvert.SerializeObject(recEsito);
            return stringaJson;
        }


        [WebMethod(EnableSession = true)]

        public string salvaLocali(List<TLocali> locali)
        {

            string stringaJson;
            string stringaSql;
            Int64 codiceLocale;
            TLocali locale = new TLocali();
            List<TLocali> localiTmp = new List<TLocali>();

            if (apriDB() && nullaOsta())
            {
                foreach (TLocali recLocale in locali)
                {
                    comandoSQL.Parameters.Clear();
                    if (recLocale.idLocale == 0)
                    {
                        try
                        {
                            codiceLocale = (Math.Abs((recLocale.nome + (x.Next() * 999).ToString()).GetHashCode())) % 999917;
                            if (codiceLocale.ToString().Length < 9)
                            {
                                codiceLocale = codiceLocale * (long)Math.Pow(10, (6 - codiceLocale.ToString().Length));
                            }

                            stringaSql = "insert into locali(nome , indirizzo, citta, tel, codiceLocale) Values (@nome , @indirizzo, @citta, @tel, @codiceLocale)";
                            comandoSQL.Parameters.AddWithValue("@nome", recLocale.nome);
                            comandoSQL.Parameters.AddWithValue("@indirizzo", recLocale.indirizzo);
                            comandoSQL.Parameters.AddWithValue("@citta", recLocale.citta);
                            comandoSQL.Parameters.AddWithValue("@tel", recLocale.tel);
                            comandoSQL.Parameters.AddWithValue("@codiceLocale", codiceLocale);

                            comandoSQL.CommandText = stringaSql;
                            comandoSQL.Connection = Connessione;
                            comandoSQL.ExecuteNonQuery();
                        }
                        catch (System.IO.IOException e)
                        {
                        }

                    }
                    else
                    {
                        switch (recLocale.stato)
                        {
                            case "M":
                                comandoSQL.Parameters.AddWithValue("@idLocale", recLocale.idLocale);
                                comandoSQL.Parameters.AddWithValue("@nome", recLocale.nome);
                                comandoSQL.Parameters.AddWithValue("@indirizzo", recLocale.indirizzo);
                                comandoSQL.Parameters.AddWithValue("@citta", recLocale.citta);
                                comandoSQL.Parameters.AddWithValue("@tel", recLocale.tel);
                                stringaSql = "Update locali Set nome = @nome, indirizzo = @indirizzo, citta = @citta, tel = @tel Where idLocale= @idLocale";
                                comandoSQL.CommandText = stringaSql;
                                comandoSQL.Connection = Connessione;
                                comandoSQL.ExecuteNonQuery();
                                break;
                            case "D":
                                stringaSql = "Delete from locali Where idLocale=@idLocale and (SELECT count(*) from incassi where idLocale=@idLocale ) = 0";
                                comandoSQL.Parameters.AddWithValue("@idLocale", recLocale.idLocale);
                                comandoSQL.CommandText = stringaSql;
                                comandoSQL.Connection = Connessione;
                                comandoSQL.ExecuteNonQuery();
                                break;
                        }
                    }
                }
                comandoSQL.CommandText = "SELECT * from locali ORDER BY nome";
                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();
                if (tabella.HasRows)
                {
                    while (tabella.Read() == true)
                    {
                        locale.stato = "X";
                        locale.idLocale = (int)tabella["idLocale"];
                        locale.nome = (string)tabella["nome"];
                        locale.indirizzo = (string)tabella["indirizzo"];
                        locale.citta = (string)tabella["citta"];
                        locale.tel = (string)tabella["tel"];
                        locale.codiceLocale = (Int64)tabella["codiceLocale"];
                        localiTmp.Add(locale);
                    }
                }
                chiudiDB();
            }

            stringaJson = JsonConvert.SerializeObject(localiTmp);
            return stringaJson;
        }


        [WebMethod(EnableSession = true)]
        public string esisteUtente(string ruolo, string email, string password)
        {
            string stringaJson;
            tRecEsito recEsito;
            recEsito.messaggio = "";

            recEsito.esito = false;

            if (apriDB())
            {
                comandoSQL.CommandText = "Select * from utenti where email=@email AND md5Password=@md5Password AND (ruolo=@ruolo or (ruolo='user' And Plus=true))";
                comandoSQL.Parameters.AddWithValue("@ruolo", ruolo.Trim());
                comandoSQL.Parameters.AddWithValue("@email", email.Trim());
                comandoSQL.Parameters.AddWithValue("@md5Password", crittoMd5.creaMD5(password));

                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();
                if (tabella.HasRows)
                {
                    tabella.Read();
                    recEsito.esito = true;
                    recEsito.messaggio = tabella["Nome"].ToString() + " " + tabella["Cognome"].ToString();
                    Session["email"] = email.Trim();
                    if ((string)tabella["ruolo"] == "admin")
                    {
                        Session["loginAdmin"] = "OK";
                        Session["loginPlus"] = "OK";
                        /*
                        comandoSQL.CommandText = "update utenti set ricevePdf=false";
                        comandoSQL.ExecuteNonQuery();
                        comandoSQL.CommandText = "update utenti set ricevePdf=true where email=@email and ruolo='admin'";
                        comandoSQL.ExecuteNonQuery();
                        */
                    }
                    else
                    {
                        Session["loginAdmin"] = "KO";
                        Session["loginPlus"] = "OK";
                    }
                    tabella.Close();
                }
                else
                {
                    Session["loginAdmin"] = "KO";
                    Session["email"] = "";
                }
                chiudiDB();
            }
            stringaJson = JsonConvert.SerializeObject(recEsito);
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]

        public string leggiLocali()
        {
            TLocali locale = new TLocali();
            List<TLocali> locali = new List<TLocali>();
            string stringaJson = "";
            if (apriDB() && nullaOsta())
            {
                comandoSQL.CommandText = "SELECT * from locali ORDER BY nome";
                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();
                if (tabella.HasRows)
                {
                    while (tabella.Read() == true)
                    {

                        locale.stato = "X";
                        locale.idLocale = (int)tabella["idLocale"];
                        locale.nome = (string)tabella["nome"];
                        locale.indirizzo = (string)tabella["indirizzo"];
                        locale.citta = (string)tabella["citta"];
                        locale.tel = (string)tabella["tel"];
                        locale.codiceLocale = (Int64)tabella["codiceLocale"];
                        locali.Add(locale);
                    }
                }
                chiudiDB();
                stringaJson = JsonConvert.SerializeObject(locali);
            }
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]
        public string leggiCassa(string locale, string utenteNome, string utenteCognome, string dataIni, string dataFin)
        {
            string stringaJson;
            tCassa cassa = new tCassa();
            tDettaglioCassa dettaglio = new tDettaglioCassa();
            string Condizione = "";
            Single totAcconto = 0, totRecupero = 0, totDaRiportare = 0;

            comandoSQL.Parameters.Clear();
            if (apriDB() && nullaOsta())
            {

                if (locale.Trim().Length > 0)
                {
                    Condizione = "locali.nome like @locale";
                    comandoSQL.Parameters.AddWithValue("@locale", '%' + locale + '%');
                }

                if (utenteNome.Trim().Length > 0)
                {
                    if (Condizione.Length > 0)
                        Condizione += " AND ";
                    Condizione += "utenti.nome like @utenteNome ";
                    comandoSQL.Parameters.AddWithValue("@utenteNome", '%' + utenteNome.Trim() + '%'); ;
                }

                if (utenteCognome.Trim().Length > 0)
                {
                    if (Condizione.Length > 0)
                        Condizione += " AND ";
                    Condizione += "utenti.cognome like @utenteCognome ";
                    comandoSQL.Parameters.AddWithValue("@utenteCognome", '%' + utenteCognome.Trim() + '%');
                }

                if (DateTime.TryParse(dataIni, out _) == true)
                {
                    if (Condizione.Length > 0)
                        Condizione += " AND ";
                    Condizione += "date(incassi.data)>=@dataIni ";
                    comandoSQL.Parameters.AddWithValue("@dataIni", dataIni);
                }
                if (DateTime.TryParse(dataFin, out _) == true)
                {
                    if (Condizione.Length > 0)
                        Condizione += " AND ";
                    Condizione += "date(incassi.data)<=@dataFin ";
                    comandoSQL.Parameters.AddWithValue("@dataFin", dataFin);
                }

                if (Condizione.Length > 0)
                    Condizione += " AND ";
                Condizione += "NOT (incassi.acconto=0 AND incassi.recupero=0 AND incassi.daRiportare=0) ";

                comandoSQL.CommandText = "Select utenti.idUtente, utenti.cognome, utenti.nome, " +
                                         "locali.idLocale, locali.nome as nomeLocale, incassi.idIncasso, incassi.data, incassi.acconto, incassi.recupero, incassi.daRiportare " +
                                         "from incassi inner JOIN utenti on incassi.idUtente = utenti.idUtente INNER Join locali on incassi.idLocale = locali.idLocale ";

                if (Condizione.Trim().Length > 0)
                    comandoSQL.CommandText += " WHERE " + Condizione;

                comandoSQL.CommandText += " ORDER BY data DESC";
                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();

                cassa.locale = locale;
                cassa.utenteNome = utenteNome;
                cassa.utenteCognome = utenteCognome;
                cassa.dataIni = dataIni;
                cassa.dataFin = dataFin;
                cassa.dettaglio = new List<tDettaglioCassa>();
                if (tabella.HasRows)
                {
                    while (tabella.Read() == true)
                    {
                        dettaglio.stato = "X";
                        dettaglio.idIncasso = tabella["idIncasso"].ToString();
                        dettaglio.nomeUtente = tabella["Nome"].ToString() + " " + tabella["Cognome"].ToString();
                        dettaglio.nomeLocale = tabella["nomeLocale"].ToString();

                        dettaglio.data = tabella["data"].ToString();
                        dettaglio.acconto = String.Format("{0:0,0.00}", (float)tabella["acconto"]);
                        dettaglio.recupero = String.Format("{0:0,0.00}", (float)tabella["recupero"]);
                        dettaglio.daRiportare = String.Format("{0:0,0.00}", (float)tabella["daRiportare"]);
                        cassa.dettaglio.Add(dettaglio);
                        totAcconto += (float)tabella["acconto"];
                        totRecupero += (float)tabella["recupero"];
                        totDaRiportare += (float)tabella["daRiportare"];
                    }
                }
                chiudiDB();
            }

            cassa.totAcconto = String.Format("{0:0,0.00}", totAcconto);
            cassa.totRecupero = String.Format("{0:0,0.00}", totRecupero);
            cassa.totDaRiportare = String.Format("{0:0,0.00}", totDaRiportare);

            stringaJson = JsonConvert.SerializeObject(cassa);
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]
        public string leggiAgentiPerAnalisi(string dataIncasso)
        {
            MySqlDataReader tabellaDettagli;
            tInfoAgenti infoAgenti = new tInfoAgenti();
            tIncassoTotaleAgenti agente = new tIncassoTotaleAgenti();
            List<tIncassoTotaleAgenti> agenti = new List<tIncassoTotaleAgenti>();
            tIncassoDettagliatoAgente dettaglioAgente = new tIncassoDettagliatoAgente();
            List<tIncassoDettagliatoAgente> dettaglioAgenti = new List<tIncassoDettagliatoAgente>();

            double totAcconto = 0;
            double totRecupero = 0;
            double totDaRiportare = 0;
            double tmpCarta = 0;
            double tmpMonete = 0;

            string stringaJson = "";
            if (apriDB())
            {
                comandoSQL.Parameters.Clear();
                comandoSQL.CommandText = "SELECT utenti.idUtente, utenti.cognome,utenti.nome, Sum(incassi.acconto) as totAcconto, sum(incassi.daRiportare) as totDaRiportare, Sum(incassi.recupero) as totRecupero, infoincasso.monete, infoincasso.carta " +
                                         "from incassi inner join utenti on incassi.idUtente = utenti.idUtente left join infoincasso on utenti.idUtente = infoincasso.idUtente and date(incassi.data)= date(infoincasso.dataIniRiepilogo) and date(incassi.data)= date(infoincasso.dataFineRiepilogo) " +
                                         "where date(incassi.data) = @dataIncasso " +
                                         "group by utenti.idUtente, utenti.Nome,utenti.Cognome, infoincasso.monete, infoincasso.carta " +
                                         "order by utenti.cognome, utenti.nome ";

                comandoSQL.Parameters.AddWithValue("@dataIncasso", dataIncasso);
                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();
                if (tabella.HasRows)
                {
                    while (tabella.Read() == true)
                    {
                        agente.stato = "X";
                        agente.idAgente = ((Int32)tabella["idUtente"]).ToString();
                        agente.nomeUtente = (string)tabella["cognome"] + " " + tabella["nome"];
                        agente.acconto = String.Format("{0:0,0.00}", (double)tabella["totAcconto"]);
                        agente.daRiportare = String.Format("{0:0,0.00}", (double)tabella["totDaRiportare"]);
                        agente.recupero = String.Format("{0:0,0.00}", (double)tabella["totRecupero"]);
                        
                        if (tabella["monete"].ToString() == "")
                            tmpMonete = 0;
                        else
                            tmpMonete =  (float)tabella["monete"];
                        if (tabella["carta"].ToString() == "")
                            tmpCarta = 0;
                        else
                            tmpCarta = (float)tabella["carta"];
                        
                        agente.totCassa = String.Format("{0:0,0.00}", tmpCarta + tmpMonete + (double)tabella["totRecupero"]);
                        agente.flussoCassa = String.Format("{0:0,0.00}", tmpCarta + tmpMonete + (double)tabella["totRecupero"] - (double)tabella["totDaRiportare"]);
                        agente.cassaGenerale= String.Format("{0:0,0.00}", (double)tabella["totAcconto"] + tmpCarta + tmpMonete + (double)tabella["totRecupero"] - (double)tabella["totDaRiportare"]);
                        agenti.Add(agente);
                    }
                }
                comandoSQL.Dispose();
                comandoSQL = new MySqlCommand();
                comandoSQL.CommandText = "SELECT time(incassi.data) as oraIncasso, utenti.idUtente, utenti.cognome,utenti.nome, locali.nome as nomeLocale, locali.citta as cittaLocale,  incassi.acconto, incassi.recupero, incassi.daRiportare " +
                                         "from incassi inner join utenti on incassi.idUtente = utenti.idUtente " +
                                         "INNER join locali on locali.idLocale = incassi.idLocale " +
                                         "where date(incassi.data) = @dataIncasso " +
                                         "order by utenti.cognome,utenti.nome, incassi.data DESC ";

                comandoSQL.Parameters.AddWithValue("@dataIncasso", dataIncasso);
                comandoSQL.Connection = Connessione;
                tabellaDettagli = comandoSQL.ExecuteReader();
                if (tabellaDettagli.HasRows)
                {
                    while (tabellaDettagli.Read() == true)
                    {
                        dettaglioAgente.idAgente = ((Int32)tabellaDettagli["idUtente"]).ToString();
                        dettaglioAgente.oraIncasso = (tabellaDettagli["oraIncasso"]).ToString();
                        dettaglioAgente.nomeUtente = (string)tabellaDettagli["cognome"] + " " + tabellaDettagli["nome"];
                        dettaglioAgente.nomeLocale = (string)tabellaDettagli["nomeLocale"];
                        dettaglioAgente.cittaLocale = (string)tabellaDettagli["cittaLocale"];
                        dettaglioAgente.acconto = String.Format("{0:0,0.00}", (float)tabellaDettagli["acconto"]);
                        dettaglioAgente.daRiportare = String.Format("{0:0,0.00}", (float)tabellaDettagli["daRiportare"]);
                        dettaglioAgente.recupero = String.Format("{0:0,0.00}", tabellaDettagli["recupero"]);
                        dettaglioAgenti.Add(dettaglioAgente);
                        totAcconto += (float)tabellaDettagli["acconto"];
                        totDaRiportare += (float)tabellaDettagli["daRiportare"];
                        totRecupero += (float)tabellaDettagli["recupero"];
                    }
                }

                infoAgenti.totali = JsonConvert.SerializeObject(agenti);
                infoAgenti.dettaglio = JsonConvert.SerializeObject(dettaglioAgenti);
                infoAgenti.totAcconto = String.Format("{0:0,0.00}", totAcconto);
                infoAgenti.totDaRiportare = String.Format("{0:0,0.00}", totDaRiportare);
                infoAgenti.totRecupero = String.Format("{0:0,0.00}", totRecupero);

                chiudiDB();
                stringaJson = JsonConvert.SerializeObject(infoAgenti);
            }
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]
        public string inviaMail(List<tDettaglioCassa> cassa, string dataIni, string dataFin, string utente, string locale)
        {
            StreamWriter fCassa;
            var client = new SmtpClient();
            var message = new MimeMessage();
            var bodyBuilder = new BodyBuilder();
            //System.Net.Mail.Attachment allegato;
            Single acconto = 0, recupero = 0, daRiportare = 0;
            Single tmpAcconto, tmpRecupero, tmpDaRiportare;


            tRecEsito recEsito;
            string stringaJson;

            recEsito.esito = false;
            recEsito.messaggio = "";

            if (cassa.Count > 0 && nullaOsta())
            {
                try
                {
                    fCassa = File.CreateText(Server.MapPath(costanti.fileCassa));
                    fCassa.Write("Locale; ");
                    fCassa.Write("Agente; ");
                    fCassa.Write("Data; ");
                    fCassa.Write("Acconto; ");
                    fCassa.Write("recupero da Riportare; ");
                    fCassa.WriteLine("Recupero ");

                    foreach (tDettaglioCassa recCassa in cassa)
                    {
                        fCassa.Write(recCassa.nomeLocale + "; ");
                        fCassa.Write(recCassa.nomeUtente + "; ");
                        fCassa.Write(recCassa.data + "; ");
                        fCassa.Write(recCassa.acconto + "; ");
                        fCassa.Write(recCassa.daRiportare + "; ");
                        fCassa.WriteLine(recCassa.recupero);

                        Single.TryParse(recCassa.acconto, out tmpAcconto);
                        Single.TryParse(recCassa.recupero, out tmpRecupero);
                        Single.TryParse(recCassa.daRiportare, out tmpDaRiportare);

                        acconto += tmpAcconto;
                        recupero += tmpRecupero;
                        daRiportare += tmpDaRiportare;
                    }


                    fCassa.Write("; ");
                    fCassa.Write("; ");
                    fCassa.Write("; ");
                    fCassa.Write(acconto.ToString().Replace(".", ",") + "; ");
                    fCassa.Write(daRiportare.ToString().Replace(".", ",") + "; ");
                    fCassa.WriteLine(recupero.ToString().Replace(".", ","));

                    fCassa.Close();


                    client.SslProtocols = SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                    client.Connect(costanti.clientHost, 465, true);
                    client.Authenticate(costanti.mailFrom, costanti.mailPassord);
                    bodyBuilder.Attachments.Add(Server.MapPath(costanti.fileCassa));
                    bodyBuilder.HtmlBody = aggiungiPiePaginaMail("il dettaglio in allegato <br>");

                    message.From.Add(new MailboxAddress("Money Box", costanti.mailFrom));
                    message.To.Add(new MailboxAddress((string)Session["email"], (string)Session["email"]));
                    message.ReplyTo.Add(new MailboxAddress((string)Session["email"], (string)Session["email"]));
                    message.Subject = "MONEY BOX, report " + "dal " + dataIni + " al " + dataFin + ", Utente: " + utente + ", locale: " + locale;
                    message.Body = bodyBuilder.ToMessageBody();

                    client.Send(message);
                    client.Disconnect(true);
                    client.Dispose();
                    File.Delete(Server.MapPath(costanti.fileCassa));
                    recEsito.esito = true;
                    recEsito.messaggio = "Messaggio inviato, controlla l'email";
                }
                catch (System.IO.IOException e)
                {
                    recEsito.esito = false;
                    recEsito.messaggio = "Errore!";
                }
            }
            else
            {
                recEsito.messaggio = "Non ci sono dati da inviare!";
            }
            stringaJson = JsonConvert.SerializeObject(recEsito);
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]
        public string aggiornaUtente(string ruolo, string email, string password, string nuovaEmail, string nuovaPassword, string nuovaRipetiPassword)
        {
            tRecEsito recEsito;
            string strPassword, strNuovaPassword;

            recEsito.esito = false;
            recEsito.messaggio = "";

            if (nullaOsta() && email.Trim() != "" && password.Trim() != "" && nuovaPassword.Trim() != "")
            {
                if (apriDB() && nullaOsta())
                {
                    if (nuovaEmail.Trim() == "")
                    {
                        nuovaEmail = email;
                    }
                    comandoSQL.Parameters.AddWithValue("@email", email);
                    comandoSQL.CommandText = "Select * from utenti where ruolo='admin' and email=@email";
                    comandoSQL.Connection = Connessione;
                    tabella = comandoSQL.ExecuteReader();
                    if (tabella.Read())
                    {
                        strPassword = crittoMd5.creaMD5(password);
                        strNuovaPassword = crittoMd5.creaMD5(nuovaPassword);
                        if (tabella["email"].ToString().Trim() == email && strPassword == (string)tabella["md5Password"] && nuovaPassword == nuovaRipetiPassword && mailCorretta(nuovaEmail))
                        {
                            tabella.Dispose();
                            comandoSQL.CommandText = "Update utenti set email=@nuovaEmail, md5Password=@md5Password where ruolo='admin' and email=@email";
                            comandoSQL.Parameters.AddWithValue("@nuovaEmail", nuovaEmail);
                            comandoSQL.Parameters.AddWithValue("@md5Password", strNuovaPassword);
                            comandoSQL.Connection = Connessione;
                            try
                            {
                                tabella = comandoSQL.ExecuteReader();
                                recEsito.esito = true;
                            }
                            catch (IOException e)
                            {
                                recEsito.esito = false;
                            }
                        }
                    }
                    chiudiDB();
                }
            }
            return JsonConvert.SerializeObject(recEsito);
        }

        [WebMethod(EnableSession = true)]

        public string leggiUtenti()
        {
            TUtenti utente = new TUtenti();
            List<TUtenti> utenti = new List<TUtenti>();
            string stringaJson = "";
            if (apriDB() && nullaOsta())
            {
                comandoSQL.CommandText = "SELECT * from utenti  where ruolo='user' ORDER BY nome, cognome";
                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();
                if (tabella.HasRows)
                {
                    while (tabella.Read() == true)
                    {
                        utente.stato = "X";
                        utente.idUtente = (int)tabella["idUtente"];
                        utente.nome = (string)tabella["nome"];
                        utente.cognome = (string)tabella["cognome"];
                        utente.email = (string)tabella["email"];
                        utente.funzioniPlus = (Boolean)tabella["plus"];
                        utente.password = "";
                        utenti.Add(utente);
                    }
                }
                chiudiDB();
                stringaJson = JsonConvert.SerializeObject(utenti);
            }
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]
        public string verificaOperazioneCassa(long codiceLocale, string emailUtente, float acconto, float recupero, float daRiportare)
        {
            string stringaJson;
            TLocali locale = new TLocali();
            string stringaSql;
            int idUtente;

            locale.stato = "-";
            locale.idLocale = 0;
            locale.codiceLocale = 0;
            locale.nome = "";
            locale.indirizzo = "";
            locale.citta = "";
            locale.tel = "";

            idUtente = recuperaIdUtente(emailUtente);
            if (apriDB() && idUtente > 0)
            {
                comandoSQL.Parameters.Clear();
                stringaSql = "SELECT * FROM locali WHERE codiceLocale=@codiceLocale";
                comandoSQL.Parameters.AddWithValue("@codiceLocale", codiceLocale);
                comandoSQL.CommandText = stringaSql;
                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();
                if (tabella.Read())
                {
                    locale.stato = "+";
                    locale.idLocale = (int)tabella["idLocale"];
                    locale.codiceLocale = codiceLocale;
                    locale.nome = (string)tabella["nome"];
                    locale.indirizzo = (string)tabella["indirizzo"];
                    locale.citta = (string)tabella["citta"];
                    locale.tel = (string)tabella["tel"];
                }
                chiudiDB();
            }
            stringaJson = JsonConvert.SerializeObject(locale);
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]
        public string eseguiOperazioneCassa(long codiceLocale, string emailUtente, float acconto, float recupero, float daRiportare)
        {
            string stringaJson;
            string stringaSql;
            tRecEsito esito = new tRecEsito();
            int idUtente = 0, idLocale;

            esito.esito = false;
            esito.messaggio = "Inserimento non effettuato!";
            idLocale = recuperaIdLocale(codiceLocale);
            if (idLocale != 0)
            {
                idUtente = recuperaIdUtente(emailUtente);
            }
            //idIncasso = cercaIncasso(idLocale, idUtente, DateTime.Now.ToString("yyyy-MM-dd"));

            if (idLocale != 0 && idUtente != 0)
            {
                if (apriDB())
                {
                    //comandoSQL.Parameters.AddWithValue("@idIncasso", idIncasso);
                    comandoSQL.Parameters.Clear();
                    comandoSQL.Parameters.AddWithValue("@idUtente", idUtente);
                    comandoSQL.Parameters.AddWithValue("@idLocale", idLocale);
                    comandoSQL.Parameters.AddWithValue("@acconto", acconto);
                    comandoSQL.Parameters.AddWithValue("@recupero", recupero);
                    comandoSQL.Parameters.AddWithValue("@daRiportare", daRiportare);
                    comandoSQL.Parameters.AddWithValue("@data", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    stringaSql = "insert into incassi(idUtente, idLocale, acconto, recupero, daRiportare, data) " +
                                 "       values(@idUtente, @idLocale, @acconto, @recupero, @daRiportare, @data) ";

                    esito.messaggio = "Inserimento eseguito correttamente \n" +
                                       "codice:   " + codiceLocale.ToString() + "\n" +
                                       "acconto: " + String.Format("{0:0,0.00}", acconto) + "\n" +
                                       "recupero: " + String.Format("{0:0,0.00}", recupero) + "\n" +
                                       "da riportare: " + String.Format("{0:0,0.00}", daRiportare) + "\n";
                    /*
                    stringaSql = "Update incassi " +
                                     "       SET " +
                                     "       acconto = @acconto, " +
                                     "       recupero = @recupero, " +
                                     "       daRiportare = @daRiportare, " +
                                     "       data = @data " +
                                     "       WHERE " +
                                     "       idIncasso=@idIncasso";
                    esito.messaggio = "Aggiornamento eseguito correttamente \n" +
                                      "codice:   " + codiceLocale.ToString() + "\n" +
                                      "acconto: " + String.Format("{0:0,0.00}", acconto) + "\n" +
                                      "recupero: " + String.Format("{0:0,0.00}", recupero) + "\n" +
                                      "da riportare: " + String.Format("{0:0,0.00}", daRiportare) + "\n";
                    */
                    try
                    {
                        comandoSQL.CommandText = stringaSql;
                        comandoSQL.ExecuteNonQuery();
                        esito.esito = true;
                        esito.messaggio = "";
                    }
                    catch
                    {
                        esito.messaggio = "Errore nella fase di registrazione o aggiornamento!";
                    }
                    chiudiDB();
                }
            }
            stringaJson = JsonConvert.SerializeObject(esito);
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]
        public string salvaUtenti(List<TUtenti> utenti)
        {

            string stringaJson;
            string stringaSql;
            TUtenti utente = new TUtenti();
            List<TUtenti> utentiTmp = new List<TUtenti>();


            if (apriDB() && nullaOsta())
            {
                foreach (TUtenti recLocale in utenti)
                {
                    comandoSQL.Parameters.Clear();

                    if (recLocale.idUtente == 0)
                    {
                        try
                        {
                            if (recLocale.password == recLocale.ripetiPassword && mailCorretta(recLocale.email) && recLocale.ripetiPassword.Trim().Length > 0)
                            {
                                stringaSql = "insert into utenti(ruolo, nome , cognome, email, plus, md5Password, md5PasswordSmart) Values ('user', @nome , @cognome, @email, @plus, @md5Password, '')";
                                comandoSQL.Parameters.AddWithValue("@nome", recLocale.nome);
                                comandoSQL.Parameters.AddWithValue("@cognome", recLocale.cognome);
                                comandoSQL.Parameters.AddWithValue("@email", recLocale.email);
                                comandoSQL.Parameters.AddWithValue("@plus", recLocale.funzioniPlus);
                                comandoSQL.Parameters.AddWithValue("@md5Password", crittoMd5.creaMD5(recLocale.password));

                                comandoSQL.CommandText = stringaSql;
                                comandoSQL.Connection = Connessione;
                                comandoSQL.ExecuteNonQuery();
                            }
                        }
                        catch (System.IO.IOException e)
                        {
                        }

                    }
                    else
                    {
                        switch (recLocale.stato)
                        {
                            case "M":
                                try
                                {
                                    if (recLocale.password == recLocale.ripetiPassword && mailCorretta(recLocale.email))
                                    {

                                        comandoSQL.Parameters.AddWithValue("@idUtente", recLocale.idUtente);
                                        comandoSQL.Parameters.AddWithValue("@nome", recLocale.nome);
                                        comandoSQL.Parameters.AddWithValue("@cognome", recLocale.cognome);
                                        comandoSQL.Parameters.AddWithValue("@email", recLocale.email);
                                        comandoSQL.Parameters.AddWithValue("@plus", recLocale.funzioniPlus);
                                        comandoSQL.Parameters.AddWithValue("@md5Password", crittoMd5.creaMD5(recLocale.password));
                                        if (recLocale.password.ToString().Length > 0)
                                        {
                                            stringaSql = "Update utenti Set nome = @nome, cognome = @cognome, email = @email, plus = @plus, md5Password = @md5Password, md5PasswordSmart = '' Where idUtente= @idUtente";
                                        }
                                        else
                                        {
                                            stringaSql = "Update utenti Set nome = @nome, cognome = @cognome, email = @email, plus = @plus  Where idUtente= @idUtente";
                                        }
                                        comandoSQL.CommandText = stringaSql;
                                        comandoSQL.Connection = Connessione;
                                        comandoSQL.ExecuteNonQuery();
                                    }
                                }
                                catch { }

                                break;
                            case "D":
                                try
                                {
                                    stringaSql = "Delete from utenti Where idUtente=@idUtente and (SELECT count(*) from incassi where idUtente=@idUtente ) = 0";
                                    comandoSQL.Parameters.AddWithValue("@idUtente", recLocale.idUtente);
                                    comandoSQL.CommandText = stringaSql;
                                    comandoSQL.Connection = Connessione;
                                    comandoSQL.ExecuteNonQuery();
                                }
                                catch { }
                                break;
                        }
                    }
                }
                comandoSQL.CommandText = "SELECT * from utenti where ruolo='user' ORDER BY nome";
                comandoSQL.Connection = Connessione;

                tabella = comandoSQL.ExecuteReader();
                if (tabella.HasRows)
                {
                    while (tabella.Read() == true)
                    {
                        utente.stato = "X";
                        utente.idUtente = (int)tabella["idUtente"];
                        utente.nome = (string)tabella["nome"];
                        utente.cognome = (string)tabella["cognome"];
                        utente.funzioniPlus = (Boolean)tabella["plus"];
                        utente.email = (string)tabella["email"];
                        utente.password = "";
                        utentiTmp.Add(utente);
                    }
                }
                chiudiDB();

            }
            stringaJson = JsonConvert.SerializeObject(utentiTmp);
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]
        public string rimuoviOperazioni(List<tDettaglioCassa> cassa)
        {
            tRecEsito esito = new tRecEsito();
            string stringaJson;
            string stringaSql;

            if (apriDB() && nullaOsta())
            {
                foreach (tDettaglioCassa recCassa in cassa)
                {
                    comandoSQL.Parameters.Clear();
                    stringaSql = "Delete from incassi Where idIncasso=@idCassa";
                    comandoSQL.Parameters.AddWithValue("@idCassa", recCassa.idIncasso);
                    comandoSQL.CommandText = stringaSql;
                    comandoSQL.Connection = Connessione;
                    comandoSQL.ExecuteNonQuery();
                }
                esito.esito = true;
                esito.messaggio = "Operazione avvenuta con successo!";
                chiudiDB();
            }
            else
            {
                esito.esito = false;
                esito.messaggio = "Errore!";
            }
            stringaJson = JsonConvert.SerializeObject(esito);
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]
        public string leggiCassaUtente(string email, string dataIni, string dataFin)
        {
            string stringaJson;
            cCostanti.tOperazione dettaglio = new cCostanti.tOperazione();
            List<cCostanti.tOperazione> cassa = new List<cCostanti.tOperazione>();

            comandoSQL.Parameters.Clear();
            if (apriDB())
            {
                comandoSQL.Parameters.AddWithValue("@dataIni", dataIni);
                comandoSQL.Parameters.AddWithValue("@dataFin", dataFin);
                comandoSQL.Parameters.AddWithValue("@email", email.Trim());
                comandoSQL.CommandText = "Select locali.codiceLocale, locali.nome as nomeLocale, " +
                                         "       incassi.data, incassi.acconto, incassi.daRiportare, incassi.recupero " +
                                         "       from locali Inner join incassi on locali.idLocale=incassi.idLocale Inner join utenti on utenti.idUtente=incassi.idUtente " +
                                         "       where date(incassi.data) >= @dataIni AND date(incassi.data) <= @dataFin AND incassi.idUtente IN " +
                                         "       (Select idUtente from utenti where email = @email)";

                comandoSQL.CommandText += " ORDER BY data DESC ";
                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();

                if (tabella.HasRows)
                {
                    while (tabella.Read() == true)
                    {
                        dettaglio.codiceLocale = tabella["codiceLocale"].ToString();
                        dettaglio.nomeLocale = tabella["nomeLocale"].ToString();
                        dettaglio.data = tabella["data"].ToString();
                        dettaglio.acconto = (float)tabella["acconto"];
                        dettaglio.recupero = (float)tabella["recupero"];
                        dettaglio.daRiportare = (float)tabella["daRiportare"];
                        cassa.Add(dettaglio);
                    }
                }
                chiudiDB();
            }

            stringaJson = JsonConvert.SerializeObject(cassa);
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]

        public string generaDocLocale(long codiceLocale, string nome, string indirizzo, string citta, string tel)
        {
            StreamReader fRead;
            StreamWriter fWrite;
            string stringaJson;
            tRecEsito esito = new tRecEsito();
            string fileName = codiceLocale.ToString() + "_" + nome.Replace(" ", "_") + ".rtf";
            string fileModello = Server.MapPath(costanti.pathRemoto) + "/modelloLocale.rtf";
            string fileTmp = Server.MapPath(costanti.pathRemoto) + "/" + fileName;
            string fileDownload = costanti.pathRemotoWeb + "/" + fileName;

            string riga;
            fRead = File.OpenText(fileModello);
            fWrite = File.CreateText(fileTmp);

            while (!fRead.EndOfStream)
            {
                riga = fRead.ReadLine();
                riga = riga.Replace("codiceLocale", codiceLocale.ToString("###-###"));
                riga = riga.Replace("nomeLocale", nome);
                riga = riga.Replace("indirizzoLocale", indirizzo);
                riga = riga.Replace("cittaLocale", citta);
                riga = riga.Replace("telefonoLocale", tel);
                fWrite.WriteLine(riga);
            }
            fRead.Close();
            fWrite.Close();
            esito.esito = true;
            esito.messaggio = fileDownload;
            stringaJson = JsonConvert.SerializeObject(esito);
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]
        public string pdfCassaUtente(string email, string dataIni, string dataFin, Single monete, Single carta)
        {
            string stringaJson;
            tRecEsito esito;
            string periodoRiferimento;
            string nomeAgente, destinatarioMail;
            Single totAcconto = 0, totRecupero = 0, totDaRiportare = 0;
            string filePdf = "", infoSorgente;
            string pathFileName;

            cPdf documentoPdf = new cPdf();
            cCostanti.tOperazione dettaglio = new cCostanti.tOperazione();
            List<cCostanti.tOperazione> cassa = new List<cCostanti.tOperazione>();


            aggiornaInfoIncassi(email, dataIni, dataFin, monete, carta, "");
            esito.esito = false;
            esito.messaggio = "-";
            if (apriDB())
            {
                nomeAgente = cercaAgente(email);
                destinatarioMail = cercaDestinatari();
                infoSorgente = "Report: " + nomeAgente;
                comandoSQL.Parameters.Clear();
                comandoSQL.Parameters.AddWithValue("@dataIni", dataIni);
                comandoSQL.Parameters.AddWithValue("@dataFin", dataFin);
                comandoSQL.Parameters.AddWithValue("@email", email.Trim());
                comandoSQL.CommandText = "Select locali.codiceLocale, locali.nome as nomeLocale, " +
                                         "       incassi.data, incassi.acconto, incassi.daRiportare, incassi.recupero " +
                                         "       from locali Inner join incassi on locali.idLocale=incassi.idLocale Inner join utenti on utenti.idUtente=incassi.idUtente " +
                                         "       where date(incassi.data) >= @dataIni AND date(incassi.data) <= @dataFin AND incassi.idUtente IN " +
                                         "       (Select idUtente from utenti where email = @email)";

                comandoSQL.CommandText += " ORDER BY data DESC ";
                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();

                if (tabella.HasRows)
                {
                    while (tabella.Read() == true)
                    {
                        dettaglio.codiceLocale = tabella["codiceLocale"].ToString();
                        dettaglio.nomeLocale = tabella["nomeLocale"].ToString();
                        dettaglio.data = tabella["data"].ToString();
                        dettaglio.acconto = (float)tabella["acconto"];
                        dettaglio.recupero = (float)tabella["recupero"];
                        dettaglio.daRiportare = (float)tabella["daRiportare"];
                        totAcconto += (float)tabella["acconto"];
                        totRecupero += (float)tabella["recupero"];
                        totDaRiportare += (float)tabella["daRiportare"];
                        cassa.Add(dettaglio);
                    }
                }
                chiudiDB();
                if (dataIni == dataFin)
                {
                    periodoRiferimento = Convert.ToDateTime(dataIni).ToString("dd-MM-yyyy");
                }
                else
                {
                    periodoRiferimento = "da " +
                        Convert.ToDateTime(dataIni).ToString("dd-MM-yyyy") +
                        " a " +
                        Convert.ToDateTime(dataFin).ToString("dd-MM-yyyy");
                }

                //strAcconto = String.Format("{0:0,0.00}", totAcconto);
                //strRecupero = String.Format("{0:0,0.00}", totRecupero);
                //strDaRiportare = String.Format("{0:0,0.00}", totDaRiportare);

                esito.esito = true;
                esito.messaggio = destinatarioMail + " -- " + infoSorgente + " -- " + filePdf;

                filePdf = documentoPdf.pdfRapportoAgente(nomeAgente, email, periodoRiferimento, totAcconto, totRecupero, totDaRiportare, monete, carta, cassa);
                pathFileName = HttpContext.Current.Server.MapPath(costanti.pathRemoto + "/" + filePdf);

                inviaPdf(destinatarioMail, infoSorgente, filePdf);
                esito.esito = true;
                esito.messaggio = destinatarioMail + " -- " + infoSorgente + " -- " + filePdf;
            }

            stringaJson = JsonConvert.SerializeObject(esito);
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]
        public string pdfCassaUtentePlus(string email, string dataIni, string dataFin, Single monete, Single carta, string targa)
        {
            string stringaJson;
            tRecEsito esito;
            string periodoRiferimento;
            string nomeAgente, destinatarioMail;
            Single totAcconto = 0, totRecupero = 0, totDaRiportare = 0;
            string filePdf = "", infoSorgente;
            string emailAgente = email;

            cPdf documentoPdf = new cPdf();
            cCostanti.tOperazione dettaglio = new cCostanti.tOperazione();
            List<cCostanti.tOperazione> cassa = new List<cCostanti.tOperazione>();

            aggiornaInfoIncassi(email, dataIni, dataFin, monete, carta, targa);
            esito.esito = false;
            esito.messaggio = "-";
            if (apriDB())
            {
                nomeAgente = cercaAgente(email);
                destinatarioMail = cercaDestinatari();
                infoSorgente = "Report: " + nomeAgente;
                comandoSQL.Parameters.Clear();
                comandoSQL.Parameters.AddWithValue("@dataIni", dataIni);
                comandoSQL.Parameters.AddWithValue("@dataFin", dataFin);
                comandoSQL.Parameters.AddWithValue("@email", email.Trim());
                comandoSQL.CommandText = "Select locali.codiceLocale, locali.nome as nomeLocale, " +
                                         "       incassi.data, incassi.acconto, incassi.daRiportare, incassi.recupero " +
                                         "       from locali Inner join incassi on locali.idLocale=incassi.idLocale Inner join utenti on utenti.idUtente=incassi.idUtente " +
                                         "       where date(incassi.data) >= @dataIni AND date(incassi.data) <= @dataFin AND incassi.idUtente IN " +
                                         "       (Select idUtente from utenti where email = @email)";

                comandoSQL.CommandText += " ORDER BY data DESC ";
                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();

                if (tabella.HasRows)
                {
                    while (tabella.Read() == true)
                    {
                        dettaglio.codiceLocale = tabella["codiceLocale"].ToString();
                        dettaglio.nomeLocale = tabella["nomeLocale"].ToString();
                        dettaglio.data = tabella["data"].ToString();
                        dettaglio.acconto = (float)tabella["acconto"];
                        dettaglio.recupero = (float)tabella["recupero"];
                        dettaglio.daRiportare = (float)tabella["daRiportare"];
                        totAcconto += (float)tabella["acconto"];
                        totRecupero += (float)tabella["recupero"];
                        totDaRiportare += (float)tabella["daRiportare"];
                        cassa.Add(dettaglio);
                    }
                }
                chiudiDB();
                if (dataIni == dataFin)
                {
                    periodoRiferimento = Convert.ToDateTime(dataIni).ToString("dd-MM-yyyy");
                }
                else
                {
                    periodoRiferimento = "da " +
                        Convert.ToDateTime(dataIni).ToString("dd-MM-yyyy") +
                        " a " +
                        Convert.ToDateTime(dataFin).ToString("dd-MM-yyyy");
                }

                filePdf = documentoPdf.pdfRapportoAgentePlus(nomeAgente, email, periodoRiferimento, totAcconto, totRecupero, totDaRiportare, monete, carta, targa, cassa);

                esito.esito = true;
                esito.messaggio = destinatarioMail + ", " + emailAgente + " -- " + infoSorgente + " -- " + filePdf;
            }

            stringaJson = JsonConvert.SerializeObject(esito);
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]
        public string cercaDestinatari()
        {
            string elencoDestinatari = "";
            comandoSQL.CommandText = "Select email from utenti where ruolo='admin' AND ricevePdf=true";
            comandoSQL.Connection = Connessione;
            tabella = comandoSQL.ExecuteReader();

            if (tabella.HasRows)
            {
                tabella.Read();

                elencoDestinatari = (string)tabella["email"];
            }
            tabella.Close();
            return elencoDestinatari;
        }

        [WebMethod(EnableSession = true)]
        public string cercaAgente(string email)
        {
            string nomeAgente = "";
            comandoSQL.Parameters.Clear();
            comandoSQL.Parameters.AddWithValue("@email", email.Trim());
            comandoSQL.CommandText = "Select nome, cognome " +
                                     "       from utenti " +
                                     "       where ruolo='user' AND email = @email";
            comandoSQL.Connection = Connessione;
            tabella = comandoSQL.ExecuteReader();

            if (tabella.HasRows)
            {
                tabella.Read();
                nomeAgente = (string)tabella["nome"] + " ";
                nomeAgente += (string)tabella["cognome"];
            }
            tabella.Close();

            return nomeAgente;
        }


        [WebMethod(EnableSession = true)]
        public int cercaIdAgente(string email)
        {
                int idAgente = 0;
                comandoSQL.Parameters.Clear();
                comandoSQL.Parameters.AddWithValue("@email", email.Trim());
                comandoSQL.CommandText = "Select idUtente " +
                                         "       from utenti " +
                                         "       where ruolo='user' AND email = @email";
                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();

                if (tabella.HasRows)
                {
                    tabella.Read();
                    idAgente = (int)tabella["idUtente"];
                }
                tabella.Close();


                return idAgente;
        }


        [WebMethod(EnableSession = true)]
        public void inviaPdf(string emailDestinatari, string infoSorgente, string nomeAllegato)
        {

            var client = new SmtpClient();
            var message = new MimeMessage();
            var bodyBuilder = new BodyBuilder();
            tRecEsito esito;
            esito.esito = false;
            esito.messaggio = "-";
            try
            {
                client.SslProtocols = SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                client.Connect(costanti.clientHost, 465, true);
                client.Authenticate(costanti.mailFrom, costanti.mailPassord);
                bodyBuilder.Attachments.Add(Server.MapPath(costanti.pathRemoto + "/" + nomeAllegato));
                bodyBuilder.HtmlBody = aggiungiPiePaginaMail("il dettaglio in allegato <br>");

                message.From.Add(new MailboxAddress("Agente MoneyBOX", costanti.mailFrom));
                message.To.Add(new MailboxAddress("MoneyBox", emailDestinatari));
                message.ReplyTo.Add(new MailboxAddress("MoneyBox", emailDestinatari));
                message.Subject = infoSorgente;
                message.Body = bodyBuilder.ToMessageBody();
                client.Send(message);
                client.Disconnect(true);
                client.Dispose();
                //File.Delete(nomeAllegato);
                esito.esito = true;
            }
            catch (System.IO.IOException e)
            {
            }
        }

        [WebMethod(EnableSession = true)]
        public void downloadAPK()
        {
            HttpContext.Current.Response.Redirect("https://www.moneysmart.cloud/download.html");
        }


        [WebMethod(EnableSession = true)]
        public string downloadElenco(string qualeElenco)
        {
            string stringaSql;
            StreamWriter fElenco;
            tRecEsito recEsito;
            string stringaJson;
            long codiceLocale;
            string nome, citta, indirizzo, tel;
            string cognome, ruolo, email;
            WebClient downloadAgent = new WebClient();
            string fileDownload;

            recEsito.esito = false;
            recEsito.messaggio = "Impossibile accedere ai dati!";

            if (apriDB())
            {
                switch (qualeElenco)
                {
                    case "locali":
                        fileDownload = costanti.pathRemotoWeb + "/" + costanti.fileElencoLocali;
                        fElenco = File.CreateText(Server.MapPath("/public/" + costanti.fileElencoLocali));
                        fElenco.Write("codice locale; ");
                        fElenco.Write("citta'; ");
                        fElenco.Write("nome locale; ");
                        fElenco.Write("indirizzo; ");
                        fElenco.WriteLine("telefono");
                        stringaSql = "SELECT * FROM locali order by citta, nome";
                        comandoSQL.CommandText = stringaSql;
                        comandoSQL.Connection = Connessione;
                        tabella = comandoSQL.ExecuteReader();
                        while (tabella.Read())
                        {
                            codiceLocale = (long)tabella["codiceLocale"];
                            citta = (string)tabella["citta"];
                            nome = (string)tabella["nome"];
                            indirizzo = (string)tabella["indirizzo"];
                            tel = (string)tabella["tel"];

                            fElenco.Write(codiceLocale.ToString() + "; ");
                            fElenco.Write(citta + "; ");
                            fElenco.Write(nome + "; ");
                            fElenco.Write(indirizzo + "; ");
                            fElenco.WriteLine(tel);
                        }
                        chiudiDB();
                        fElenco.Close();

                        downloadAgent.DownloadDataAsync(new Uri(fileDownload), "locali.csv");
                        recEsito.esito = true;
                        recEsito.messaggio = costanti.fileElencoLocali;
                        break;

                    case "agenti":
                        fileDownload = costanti.pathRemotoWeb + "/" + costanti.fileElencoAgenti;
                        fElenco = File.CreateText(Server.MapPath("/public/" + costanti.fileElencoAgenti));
                        fElenco.Write("cognome; ");
                        fElenco.Write("nome; ");
                        fElenco.Write("ruolo; ");
                        fElenco.WriteLine("e-mail");

                        stringaSql = "Select * from utenti order by ruolo, cognome, nome";
                        comandoSQL.CommandText = stringaSql;
                        comandoSQL.Connection = Connessione;
                        tabella = comandoSQL.ExecuteReader();
                        while (tabella.Read())
                        {
                            cognome = (string)tabella["cognome"];
                            nome = (string)tabella["nome"];
                            ruolo = (string)tabella["ruolo"];
                            email = (string)tabella["email"];

                            fElenco.Write(cognome + "; ");
                            fElenco.Write(nome + "; ");
                            fElenco.Write(ruolo + "; ");
                            fElenco.WriteLine(email);
                        }
                        chiudiDB();
                        fElenco.Close();
                        downloadAgent.DownloadDataAsync(new Uri(fileDownload), "agenti.csv");
                        recEsito.esito = true;
                        recEsito.messaggio = costanti.fileElencoAgenti;
                        break;

                    default:
                        recEsito.esito = false;
                        recEsito.messaggio = "Non ci sono dati da scaricare!";
                        break;
                }

            }
            stringaJson = JsonConvert.SerializeObject(recEsito);
            return stringaJson;
        }

        [WebMethod(EnableSession = true)]
        public string downloadPdfAgente(string dataIncasso, string idAgente)
        {
            string stringaJson;
            tRecEsito esito;
            string periodoRiferimento;
            Single totAcconto = 0, totRecupero = 0, totDaRiportare = 0;
            string filePdf = "", infoSorgente;
            string nomeAgente = "";
            string emailAgente = "";
            int riga = 0;
            float monete = 0;
            float carta = 0;
            string targa = "";
            string fileDownload = "";
            WebClient downloadAgent = new WebClient();
            int idAgenteInt = Convert.ToInt32(idAgente);

            leggiInfoIncassi(idAgenteInt, dataIncasso, dataIncasso, ref carta, ref monete, ref targa);

            cPdf documentoPdf = new cPdf();
            cCostanti.tOperazione dettaglio = new cCostanti.tOperazione();
            List<cCostanti.tOperazione> cassa = new List<cCostanti.tOperazione>();

            esito.esito = true;
            esito.messaggio = "-";
            if (apriDB())
            {
                infoSorgente = "Report: " + nomeAgente;
                comandoSQL.Parameters.Clear();
                comandoSQL.Parameters.AddWithValue("@dataIni", dataIncasso);
                comandoSQL.Parameters.AddWithValue("@dataFin", dataIncasso);
                comandoSQL.Parameters.AddWithValue("@idAgente", idAgente);
                comandoSQL.CommandText = "Select locali.codiceLocale as codiceLocale, locali.nome as nomeLocale,  " +
                    "incassi.data, incassi.acconto, incassi.daRiportare, incassi.recupero, utenti.email,  utenti.nome, utenti.cognome " +
                    "from locali Inner join incassi on locali.idLocale = incassi.idLocale Inner join utenti on utenti.idUtente = incassi.idUtente " +
                    "where date(incassi.data) >= @dataIni AND date(incassi.data) <= @dataFin AND incassi.idUtente = @idAgente " +
                    "order by incassi.data DESC";

                comandoSQL.Connection = Connessione;
                tabella = comandoSQL.ExecuteReader();

                if (tabella.HasRows)
                {
                    while (tabella.Read() == true)
                    {
                        riga++;
                        if (riga == 1)
                        {
                            nomeAgente = tabella["cognome"].ToString() + " " + tabella["nome"].ToString();
                            emailAgente = tabella["email"].ToString();
                        }
                        dettaglio.codiceLocale = tabella["codiceLocale"].ToString();
                        dettaglio.nomeLocale = tabella["nomeLocale"].ToString();
                        dettaglio.data = tabella["data"].ToString();
                        dettaglio.acconto = (float)tabella["acconto"];
                        dettaglio.recupero = (float)tabella["recupero"];
                        dettaglio.daRiportare = (float)tabella["daRiportare"];
                        totAcconto += (float)tabella["acconto"];
                        totRecupero += (float)tabella["recupero"];
                        totDaRiportare += (float)tabella["daRiportare"];
                        cassa.Add(dettaglio);
                    }
                }
                chiudiDB();
                periodoRiferimento = Convert.ToDateTime(dataIncasso).ToString("dd-MM-yyyy");

                filePdf = documentoPdf.pdfRapportoAgentePlus(nomeAgente, emailAgente, periodoRiferimento, totAcconto, totRecupero, totDaRiportare, monete, carta, targa, cassa);
                fileDownload = costanti.pathRemotoWeb + "/" + filePdf;
                esito.esito = true;
                esito.messaggio = fileDownload;
            }
            stringaJson = JsonConvert.SerializeObject(esito);
            return stringaJson;
        }



        [WebMethod(EnableSession = true)]
        public string emailElenco(string qualeElenco)
        {
            string stringaSql;
            StreamWriter fElenco;
            tRecEsito recEsito;
            string stringaJson;
            long codiceLocale;
            string nome, citta, indirizzo, tel;
            string cognome, ruolo, email;
            string fileAttach = "";
            string oggettoEmail = "", testoHtml = "";

            var client = new SmtpClient();
            var message = new MimeMessage();
            var bodyBuilder = new BodyBuilder();

            recEsito.esito = false;
            recEsito.messaggio = "Impossibile accedere ai dati!";

            if (apriDB() && nullaOsta())
            {
                switch (qualeElenco)
                {
                    case "locali":
                        testoHtml = "L'elenco dei locali è in allegato <br>";
                        oggettoEmail = "MONEY BOX, elenco dei locali registrati " + DateTime.Now.Date.ToString("dd/MM/yyyy");
                        fileAttach = Server.MapPath("/public/" + costanti.fileElencoLocali);
                        fElenco = File.CreateText(fileAttach);

                        fElenco.Write("codice locale; ");
                        fElenco.Write("citta'; ");
                        fElenco.Write("nome locale; ");
                        fElenco.Write("indirizzo; ");
                        fElenco.WriteLine("telefono");
                        stringaSql = "SELECT * FROM locali order by citta, nome";
                        comandoSQL.CommandText = stringaSql;
                        comandoSQL.Connection = Connessione;
                        tabella = comandoSQL.ExecuteReader();
                        while (tabella.Read())
                        {
                            codiceLocale = (long)tabella["codiceLocale"];
                            citta = (string)tabella["citta"];
                            nome = (string)tabella["nome"];
                            indirizzo = (string)tabella["indirizzo"];
                            tel = (string)tabella["tel"];

                            fElenco.Write(codiceLocale.ToString() + "; ");
                            fElenco.Write(citta + "; ");
                            fElenco.Write(nome + "; ");
                            fElenco.Write(indirizzo + "; ");
                            fElenco.WriteLine(tel);
                        }
                        chiudiDB();
                        fElenco.Close();
                        break;

                    case "agenti":
                        testoHtml = "L'elenco degli agenti è in allegato <br>";
                        oggettoEmail = "MONEY BOX, elenco degli agenti registrati " + DateTime.Now.Date.ToString("dd/MM/yyyy");
                        fileAttach = Server.MapPath("/public/" + costanti.fileElencoAgenti);
                        fElenco = File.CreateText(fileAttach);

                        fElenco.Write("cognome; ");
                        fElenco.Write("nome; ");
                        fElenco.Write("ruolo; ");
                        fElenco.WriteLine("e-mail");
                        stringaSql = "Select * from utenti order by ruolo, cognome, nome";
                        comandoSQL.CommandText = stringaSql;
                        comandoSQL.Connection = Connessione;
                        tabella = comandoSQL.ExecuteReader();
                        while (tabella.Read())
                        {
                            cognome = (string)tabella["cognome"];
                            nome = (string)tabella["nome"];
                            ruolo = (string)tabella["ruolo"];
                            email = (string)tabella["email"];

                            fElenco.Write(cognome + "; ");
                            fElenco.Write(nome + "; ");
                            fElenco.Write(ruolo + "; ");
                            fElenco.WriteLine(email);
                        }
                        chiudiDB();
                        fElenco.Close();
                        break;
                }
            }

            recEsito.esito = false;
            recEsito.messaggio = "";

            try
            {
                bodyBuilder.Attachments.Add(fileAttach);
                bodyBuilder.HtmlBody = aggiungiPiePaginaMail(testoHtml);

                message.From.Add(new MailboxAddress("Money Box", costanti.mailFrom));
                message.To.Add(new MailboxAddress((string)Session["email"], (string)Session["email"]));
                message.ReplyTo.Add(new MailboxAddress((string)Session["email"], (string)Session["email"]));
                message.Subject = oggettoEmail;
                message.Body = bodyBuilder.ToMessageBody();

                client.SslProtocols = SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                client.Connect(costanti.clientHost, 465, true);
                client.Authenticate(costanti.mailFrom, costanti.mailPassord);
                client.Send(message);
                client.Disconnect(true);
                client.Dispose();
                recEsito.esito = true;
                recEsito.messaggio = "Messaggio inviato, controlla l'email";
            }
            catch (System.IO.IOException e)
            {
                recEsito.esito = false;
                recEsito.messaggio = "Errore!";
            }

            stringaJson = JsonConvert.SerializeObject(recEsito);
            return stringaJson;
        }

        string aggiungiPiePaginaMail(string testoHtml)
        {
            string testoPiePagina = testoHtml +
                                   "<br> <br> <br> <br> <br> " +
                                   "per scaricare l'ultima versione dell'APP MONEY SMART, fai clic sul seguente link:  <br>" +
                                   "<a href='https://www.moneysmart.cloud/install.html'>https://www.moneysmart.cloud/install.html </a><br> ";
            return testoPiePagina;
        }


        [WebMethod(EnableSession = true)]
        public void provaPdf()
        {
            //pdfCassaUtente("agostino@gmail.com", "2020-01-01", "2021-05-01", 5, 25);
        }


        [WebMethod(EnableSession = true)]
        public void aggiornaInfoIncassi(string email, string dataIni, string dataFin, Single monete, Single carta, string targa)
        {
            int idAgente = 0;
            bool nuovoInserimento;

            if (apriDB())
            {
                idAgente = cercaIdAgente(email);
                if (idAgente > 0)
                {
                    comandoSQL.Parameters.Clear();
                    comandoSQL.Parameters.AddWithValue("@dataIni", dataIni);
                    comandoSQL.Parameters.AddWithValue("@dataFin", dataFin);
                    comandoSQL.Parameters.AddWithValue("@idUtente", idAgente);
                    comandoSQL.CommandText = "Select * from infoincasso where idUtente=@idUtente and dataIniRiepilogo=@dataIni AND dataFineRiepilogo=@dataFin";
                    comandoSQL.Connection = Connessione;
                    tabella = comandoSQL.ExecuteReader();

                    if (tabella.HasRows)
                        nuovoInserimento = false;
                    else
                        nuovoInserimento = true;

                    tabella.Close();
                    comandoSQL.Dispose();
                    comandoSQL = new MySqlCommand();
                    comandoSQL.Connection = Connessione;
                    comandoSQL.Parameters.AddWithValue("@targa", targa);
                    comandoSQL.Parameters.AddWithValue("@monete", monete);
                    comandoSQL.Parameters.AddWithValue("@carta", carta);

                    comandoSQL.Parameters.AddWithValue("@dataIni", dataIni);
                    comandoSQL.Parameters.AddWithValue("@dataFin", dataFin);
                    comandoSQL.Parameters.AddWithValue("@idUtente", idAgente);

                    if (nuovoInserimento)
                    {
                        comandoSQL.CommandText = "INSERT INTO infoincasso (idUtente, dataIniRiepilogo, dataFineRiepilogo, targa, monete, carta) " +
                           "VALUES (@idUtente, @dataIni, @dataFin, @targa, @monete, @carta)";
                    }
                    else
                    {
                        comandoSQL.CommandText = "Update infoincasso set targa = @targa, monete=@monete, carta=@carta " +
                             "Where idUtente=@idUtente and dataIniRiepilogo=@dataIni AND dataFineRiepilogo=@dataFin";
                    }

                    comandoSQL.ExecuteNonQuery();
                    comandoSQL.Dispose();
                }
                chiudiDB();
            }
        }

        [WebMethod(EnableSession = true)]
        public void leggiInfoIncassi(int idAgente, string dataIni, string dataFin, ref float carta, ref float monete, ref string targa)

        {
            carta = 0;
            monete = 0;
            targa = "";

            if (apriDB())
            {
                if (idAgente > 0)
                {
                    comandoSQL.Parameters.Clear();
                    comandoSQL.Parameters.AddWithValue("@dataIni", dataIni);
                    comandoSQL.Parameters.AddWithValue("@dataFin", dataFin);
                    comandoSQL.Parameters.AddWithValue("@idUtente", idAgente);
                    comandoSQL.CommandText = "Select * from infoincasso where idUtente=@idUtente and dataIniRiepilogo=@dataIni AND dataFineRiepilogo=@dataFin";
                    comandoSQL.Connection = Connessione;
                    tabella = comandoSQL.ExecuteReader();

                    if (tabella.HasRows)
                    {
                        tabella.Read();
                        carta = (float)tabella["carta"];
                        monete = (float)tabella["monete"];
                        targa = (string)tabella["targa"];
                    }
                    tabella.Close();
                    comandoSQL.Dispose();

                }
                chiudiDB();
            }
        }

        [WebMethod(EnableSession = true)]
        public void prova()
        {
            float carta = 900;
            float monete = 1200;
            string targa = "Punto Azzurro RX789AA";
            string email = "neri@test.it";

            pdfCassaUtentePlus(email, "2021-07-01", "2021-07-01", carta, monete, targa);
        }

    }
}

