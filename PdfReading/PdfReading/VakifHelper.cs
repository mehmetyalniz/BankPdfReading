using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Xobject;

namespace PdfReading
{
    public class VakifHelper
    {
        public static List<VakifTransaction> ExtractTransactionsFromPdf(byte[] pdfBytes)
        {
            var transactions = new List<VakifTransaction>();

            using (MemoryStream stream = new MemoryStream(pdfBytes))
            {
                using (PdfReader reader = new PdfReader(stream))
                {
                    using (PdfDocument pdfDoc = new PdfDocument(reader))
                    {
                        for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                        {
                            string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page));
                            var pageTransactions = ParseTransactions(pageText);
                            if (pageTransactions != null)
                            {
                                transactions.AddRange(pageTransactions);
                            }
                        }
                    }
                }
            }
            return transactions;
        }

        private static List<VakifTransaction> ParseTransactions(string pageText)
        {
            var transactions = new List<VakifTransaction>();

            // Regex deseni tarih, saat, işlem numarası, miktar, bakiye ve işlem adı için.
            string pattern = @"(?<Date>\d{2}\.\d{2}\.\d{4})\s+(?<Time>[01]?[0-9]|2[0-3]):[0-5][0-9]\s+(?<ProcessNo>\d{16})\s+(?<Amount>-?\d{1,3}(?:\.\d{3}),\d{2})\s+(?<Balance>-?\d{1,3}(?:\.\d{3}),\d{2})\s+(?<ProcessName>[\s\S]+?(?=\d{2}\.\d{2}\.\d{4}|Sayfa|Toplam))";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(pageText);

            foreach (Match match in matches)
            {
                DateTime date = DateTime.ParseExact(match.Groups["Date"].Value, "dd.MM.yyyy", null);
                string time = match.Groups["Time"].Value;
                string processno = match.Groups["ProcessNo"].Value;

                string amountString = match.Groups["Amount"].Value.Replace(".", "").Replace(",", ".");
                decimal amount = decimal.Parse(amountString, CultureInfo.InvariantCulture);

                string balanceString = match.Groups["Balance"].Value.Replace(".", "").Replace(",", ".");
                decimal balance = decimal.Parse(balanceString, CultureInfo.InvariantCulture);

                string processname = match.Groups["ProcessName"].Value.Trim();

                transactions.Add(new VakifTransaction
                {
                    Date = date.ToString("dd.MM.yyyy"),
                    Time = time,
                    ProcessNo = processno,
                    Amount = amount,
                    Balance = balance,
                    ProcessName = processname
                });
            }

            return transactions;
        }
    }
}
