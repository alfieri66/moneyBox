using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace moneyBox
{
    public class cCostanti
    {
        //public string stringaConnessione { get; set; } = "Database=moneyBox ;Server=127.0.0.1;User=root; password=;";
        //public string stringaConnessione { get; set; } = "Database=Sql1399902_1;Server=89.46.111.162;uid=Sql1399902; pwd=0240be030d;";

        public struct tOperazione
        {
            public string codiceLocale;
            public string nomeLocale;
            public string data;
            public Single acconto;
            public Single recupero;
            public Single daRiportare;
        }

        public cCostanti()
        {
            var appSettings = ConfigurationManager.AppSettings["livelloEsecuzione"].ToString();
            switch (appSettings.Trim().ToUpper())
            {
                case "L":
                    var settingsTestLocale = ConfigurationManager.ConnectionStrings["stringaConnessioneTestLocale"];
                    stringaConnessione = settingsTestLocale.ConnectionString;
                    pathRemotoWeb = "https://localhost:44348/public";
                    break;
                case "T":
                    var settingsTestWeb = ConfigurationManager.ConnectionStrings["stringaConnessioneTestWeb"];
                    stringaConnessione = settingsTestWeb.ConnectionString;
                    pathRemotoWeb = "https://www.dolcemare.eu/public";
                    break;
                case "W":
                    var settingsWeb = ConfigurationManager.ConnectionStrings["stringaConnessioneWeb"];
                    stringaConnessione = settingsWeb.ConnectionString;
                    pathRemotoWeb = "https://www.moneysmart.cloud/public";
                    break;
            }
        }

        public string stringaConnessione { get; set; }
        public string fileCassa { get; set; } = "/public/cassa.csv";
        public string fileElencoLocali { get; set; } = "/public/elencoLocali.csv";
        public string fileElencoAgenti { get; set; } = "/public/elencoAgenti.csv";
        public string clientHost { get; set; } = "smtps.aruba.it";
        public int clientPort { get; set; } = 465;
        public Boolean clientEnableSsl { get; set; } = true;
        public int clientTimeout { get; set; } = 10000;
        public string mailFrom { get; set; } = "moneysmart@moneysmart.cloud";
        public string mailPassord { get; set; } = "Sisco965";
        public string pathRemotoWeb { get; set; }
        public string pathRemoto { get; set; } = "/public";
    }
}