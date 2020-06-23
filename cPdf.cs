using System;
using System.Collections.Generic;
using System.Linq;

using System.Web;
using PdfSharp.Drawing;
using PdfSharp.Pdf;


using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System.Diagnostics;


namespace moneyBox
{
    public class cPdf
    {
        public string  pdfRapportoAgente(string autore, string emailAutore, string periodoRiferimento, Single acconto, Single recupero, Single daRiportare, Single monete, Single carta, List<cCostanti.tOperazione> info)
        {
            cCostanti costanti = new cCostanti();
            string testo = "";
            string fileName, pathFileName;
            string pageFooterText = "";

            DateTime currentTime = DateTime.Now;
            Document document = new Document();
            Table table = new MigraDoc.DocumentObjectModel.Tables.Table();
            Paragraph infoPremilinari = new MigraDoc.DocumentObjectModel.Paragraph();
            Column colonna ;
 
            fileName = ("pdf_" + emailAutore).Replace(" ", "").Replace(".", "_").Replace("@", "_") + ".pdf";
            pathFileName = HttpContext.Current.Server.MapPath(costanti.pathRemoto + "/"+ fileName);

            Style style = document.Styles["Normal"];
            style.Font.Name = "Arial Unicode MS";
            style.Font.Size = 12;
            style.Font.Bold = false;
            

            //table Style
            style = document.Styles.AddStyle("tabella", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Size = 12;
            style.Font.Bold = false;

            style = document.Styles.AddStyle("rigaBold", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Size = 12;
            style.Font.Bold = true;

            style = document.Styles.AddStyle("riga24", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Size = 24;
            style.Font.Bold = true;

            style = document.Styles.AddStyle("testoBold", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Size = 12;
            style.Font.Bold = true;

            style = document.Styles.AddStyle("testoRosso", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Size = 14;
            style.Font.Bold = true;
            style.Font.Color = Colors.OrangeRed;

            style = document.Styles.AddStyle("testoBlu", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Size = 14;
            style.Font.Bold = true;
            style.Font.Color = Colors.DarkBlue;

            style = document.Styles.AddStyle("testoNero", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Size = 14;
            style.Font.Bold = true;
            style.Font.Color = Colors.Black;

            style = document.Styles.AddStyle("Titolo", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Size = 16;
            style.Font.Bold = true;
            style.Font.Color = Colors.Orange;

            Section page = document.AddSection();
            
            //header
            Paragraph header = page.Headers.Primary.AddParagraph();
         
            header.AddText("Money BOX");
            header.Format.Alignment = ParagraphAlignment.Left;
            header.Style = "Titolo";
            testo = string.Format("{0}, {1} ", autore, emailAutore) + "\n";
            header = page.Headers.Primary.AddParagraph();
            header.AddText(testo);
            header.Format.Alignment = ParagraphAlignment.Left;


            //footer
            pageFooterText = string.Format(" report del {0}, {1} ", currentTime.ToShortDateString(), currentTime.ToShortTimeString()) + "\n";
            Paragraph footer = page.Footers.Primary.AddParagraph();
            footer.AddText(pageFooterText);
            footer.Format.Alignment = ParagraphAlignment.Center;

            table.Style = "tabella";
            table.Borders.Color = Colors.Black;
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0;

            colonna = table.AddColumn("6cm");
            colonna.Format.Alignment = ParagraphAlignment.Left;
            colonna = table.AddColumn("3.5cm");
            colonna.Format.Alignment = ParagraphAlignment.Left;
            colonna = table.AddColumn("3.5cm");
            colonna.Format.Alignment = ParagraphAlignment.Left;
            colonna = table.AddColumn("3.5cm");
            colonna.Format.Alignment = ParagraphAlignment.Left;

            infoPremilinari.AddText("\nPeriodo di riferimento: ");
            infoPremilinari.AddFormattedText(periodoRiferimento, TextFormat.Bold);
            infoPremilinari.AddText("\n\n");


            page.Add(infoPremilinari);
            Row riga = table.AddRow();
            riga.Style = "rigaBold";
            riga.Cells[0].AddParagraph("Locale");
            riga.Cells[1].AddParagraph("acconto");
            riga.Cells[2].AddParagraph("recupero \n da riportare");
            riga.Cells[3].AddParagraph("da Riportare");
            riga.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            riga.Cells[1].Format.Alignment = ParagraphAlignment.Right;
            riga.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            riga.Cells[3].Format.Alignment = ParagraphAlignment.Right;

            foreach (var value in info)
            {
                riga = table.AddRow();
                riga.Cells[0].AddParagraph(value.nomeLocale + "\n" + value.data);
                if (value.acconto==0)
                {
                    riga.Cells[1].AddParagraph("");
                }
                else
                {
                    riga.Cells[1].AddParagraph(String.Format("{0:0,0.00}", value.acconto));
                }

                if (value.recupero == 0)
                {
                    riga.Cells[2].AddParagraph("");
                }
                else
                {
                    riga.Cells[2].AddParagraph(String.Format("{0:0,0.00}", value.recupero));
                }
                if (value.daRiportare == 0)
                {
                    riga.Cells[3].AddParagraph("");
                }
                else
                {
                    riga.Cells[3].AddParagraph(String.Format("{0:0,0.00}", value.daRiportare));
                }
                riga.Cells[0].Format.Alignment = ParagraphAlignment.Left;
                riga.Cells[1].Format.Alignment = ParagraphAlignment.Right;
                riga.Cells[2].Format.Alignment = ParagraphAlignment.Right;
                riga.Cells[3].Format.Alignment = ParagraphAlignment.Right;
            }

            riga = table.AddRow();
            riga.Style = "rigaBold";
            riga.Cells[0].AddParagraph("\nFlusso acconti\n\n");
            riga.Cells[1].AddParagraph("\n" + String.Format("{0:0,0.00}", acconto));
            riga.Cells[2].AddParagraph("");
            riga.Cells[2].MergeRight=1;
            riga.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            riga.Cells[1].Format.Alignment = ParagraphAlignment.Right;
            riga.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            riga.Cells[0].Style = "testoNero";
            riga.Cells[1].Style = "testoBlu";

            riga = table.AddRow();
            riga.Style = "rigaBold";
            riga.Cells[0].AddParagraph("Flusso di cassa");
            riga.Cells[0].MergeRight = 1;
            riga.Cells[2].AddParagraph(String.Format("{0:0,0.00}", recupero));
            riga.Cells[3].AddParagraph(String.Format("{0:0,0.00}", daRiportare));
            riga.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            riga.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            riga.Cells[3].Format.Alignment = ParagraphAlignment.Right;


            riga = table.AddRow();
            riga.Style = "rigaBold";
            riga.Cells[0].AddParagraph("Monete");
            riga.Cells[0].MergeRight = 1;
            riga.Cells[2].AddParagraph(String.Format("{0:0,0.00}", monete));
            riga.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            riga.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            riga.Cells[3].MergeDown = 3;

            riga = table.AddRow();
            riga.Style = "rigaBold";
            riga.Cells[0].AddParagraph("Carta");
            riga.Cells[0].MergeRight = 1;
            riga.Cells[2].AddParagraph(String.Format("{0:0,0.00}", carta));
            riga.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            riga.Cells[2].Format.Alignment = ParagraphAlignment.Right;

            riga = table.AddRow();
            riga.Style = "rigaBold";
            riga.Cells[0].AddParagraph("Tot. cassa");
            riga.Cells[0].MergeRight = 1;
            riga.Cells[2].AddParagraph(String.Format("{0:0,0.00}", monete+carta+ recupero));
            riga.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            riga.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            riga.Cells[0].Style = "testoNero";
            riga.Cells[2].Style = "testoNero";
            
            riga = table.AddRow();
            riga.Style = "rigaBold";
            riga.Cells[0].AddParagraph("\nFlusso di cassa\n\n");
            riga.Cells[0].MergeRight = 1;
            riga.Cells[2].AddParagraph("\n" + String.Format("{0:0,0.00}", monete + carta + recupero - daRiportare));
            riga.Cells[3].AddParagraph("\n");
            riga.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            riga.Cells[2].Format.Alignment = ParagraphAlignment.Right;
            riga.Cells[3].Format.Alignment = ParagraphAlignment.Right;
            riga.Cells[0].Style = "testoNero";
            riga.Cells[2].Style = "testoRosso";
            
            page.Add(table);

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(pathFileName);

            //Process.Start(fileName);
            return fileName;
        }




    }
}