

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace PdfReading
{
    public class KuveytHelper
    {
        public static List<KuveytTransaction> KuveytTransactionsFromPdf(byte[] pdfBytes)
        {
            var transactions = new List<KuveytTransaction>();

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

        private static List<KuveytTransaction> ParseTransactions(string pageText)
        {
            var transactions = new List<KuveytTransaction>();

            // Regex deseni tarih, saat, işlem numarası, miktar, bakiye ve işlem adı için.
            string pattern = @"(?<ProcessDate>\d{2}\.\d{2}\.\d{4})\s+(?<ReferanceCode>[A-Z0-9]{5})\s+(?<Description>[A-Za-z0-9\s,:\(\)]+?)\s+(?<Amount>-?\d{1,3}(?:\.\d{3}),\d{2})\s+(?<Balance>-?\d{1,3}(?:\.\d{3}),\d{2})\s+(?<Branch>[A-Za-z\s]+)";


            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(pageText);

            foreach (Match match in matches)
            {
                DateTime processDate = DateTime.ParseExact(match.Groups["ProcessDate"].Value, "dd.MM.yyyy", null);

                string referanceCode = match.Groups["ReferanceCode"].Value;

                string description = match.Groups["Description"].Value.Trim();

                string amountString = match.Groups["Amount"].Value.Replace(".", "").Replace(",", ".");
                decimal amount = decimal.Parse(amountString, CultureInfo.InvariantCulture);

                string balanceString = match.Groups["Balance"].Value.Replace(".", "").Replace(",", ".");
                decimal balance = decimal.Parse(balanceString, CultureInfo.InvariantCulture);

                string branch = match.Groups["Branch"].Value.Trim();

                transactions.Add(new KuveytTransaction
                {
                    ProcessDate = processDate.ToString("dd.MM.yyyy"),
                    ReferanceCode = referanceCode,
                    Description = description,
                    Amount = amount,
                    Balance = balance,
                    Branch = branch
                });
            }

            return transactions;
        }
    }
}
