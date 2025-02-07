using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.IO;

namespace PdfReading
{
    public class İsbankHelper
    {
        public static List<İsbankTransaction> İsbankTransactionsFromPdf(byte[] pdfBytes)
        {
            var transactions = new List<İsbankTransaction>();

            using (MemoryStream stream = new MemoryStream(pdfBytes))
            {
                using (PdfReader reader = new PdfReader(stream))
                {
                    using (PdfDocument pdfDoc = new PdfDocument(reader))
                    {
                        for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                        {
                            // SimpleTextExtractionStrategy kullanarak metni çıkar
                            var strategy = new SimpleTextExtractionStrategy();
                            string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);

                            // Ayrıştırma işlemi
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

        private static List<İsbankTransaction> ParseTransactions(string pageText)
        {
            var transactions = new List<İsbankTransaction>();

            // Regex deseni: Tarih/Saat, Şube, Tutar, Bakiye, Ek Hesap Bakiyesi, İşlem, İşlem Tipi ve Açıklama
            string pattern = @"(?<Date>\d{2}/\d{2}/\d{4}-\d{2}:\d{2}:\d{2})\s+(?<Branch>[A-Za-zÇĞİÖŞÜçğöşü\s]+)\s+(?<Amount>-?\d{1,3}(?:\.\d{3})*,\d{2})\s+(?<Balance>-?\d{1,3}(?:\.\d{3})*,\d{2})\s+(?<AdditionalBalance>-?\d{1,3}(?:\.\d{3})*,\d{2})\s+(?<Process>[A-Z]{2})\s+(?<ProcessType>[A-Z]{3,4})\s+(?<Description>.+)";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(pageText);

            foreach (Match match in matches)
            {
                // Tarih/Saat ayrıştırma
                DateTime date = DateTime.ParseExact(match.Groups["Date"].Value, "dd/MM/yyyy-HH:mm:ss", CultureInfo.InvariantCulture);

                // Şube/Kanal ayrıştırma
                string branch = match.Groups["Branch"].Value.Trim();

                // İşlem Tipi
                string processType = match.Groups["ProcessType"].Value.Trim();

                // Açıklama
                string description = match.Groups["Description"].Value.Trim();

                // İşlem Tutarı
                string amountString = match.Groups["Amount"].Value.Replace(".", "").Replace(",", ".");
                decimal amount = decimal.Parse(amountString, CultureInfo.InvariantCulture);

                // Bakiye
                string balanceString = match.Groups["Balance"].Value.Replace(".", "").Replace(",", ".");
                decimal balance = decimal.Parse(balanceString, CultureInfo.InvariantCulture);

                // Ek Hesap Bakiyesi
                string additionalBalanceString = match.Groups["AdditionalBalance"].Value.Replace(".", "").Replace(",", ".");
                decimal additionalBalance = decimal.Parse(additionalBalanceString, CultureInfo.InvariantCulture);

                // İşlemi listeye ekleme
                transactions.Add(new İsbankTransaction
                {
                    Date = date,
                    Branch = branch,
                    ProcessType = processType,
                    Description = description,
                    Amount = amount,
                    Balance = balance,
                    AdditionalBalance = additionalBalance
                });
            }

            return transactions;
        }

    }


}
