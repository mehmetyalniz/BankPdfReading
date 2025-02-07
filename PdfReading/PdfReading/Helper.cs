using System;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;

namespace PdfReading
{
    public static class Helper
    {
        // PDF'den metni çıkaran metod
        public static string ExtractTextFromPdf(string url)
        {
            using var pdfReader = new PdfReader(url);
            using var pdfDoc = new PdfDocument(pdfReader);

            var text = new StringBuilder();
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i));
                text.Append(pageText);
            }
            return text.ToString();
        }

        // PDF metnini sınıflandıran metod
        public static List<BankTransaction> ClassifyPdfContent(string pdfText)
        {
            List<BankTransaction> transactions = new List<BankTransaction>();

            // Regex ile tarih, açıklama ve miktarı ayıklıyoruz
            string pattern = @"(?<Date>\d{2}\.\d{2}\.\d{4})\s+(?<Reference>[A-Z]\d{5})\s+(?<Amount>-?\d{1,3}(?:\.\d{3}),\d{2})\s+(?<Balance>-?\d{1,3}(?:\.\d{3}),\d{2})\s+(?<Description>[\s\S]+?(?=\d{2}\.\d{2}\.\d{4}|Sayfa|Toplam))";
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(pdfText);

            CultureInfo turkishCulture = new CultureInfo("tr-TR");

            foreach (Match match in matches)
            {
                var date = DateTime.ParseExact(match.Groups["Date"].Value, "dd.MM.yyyy", null);
                string reference = match.Groups["Reference"].Value;
                string amountString = match.Groups["Amount"].Value;
                string balanceString = match.Groups["Balance"].Value;

                // Miktar ve bakiye üzerindeki nokta ve virgül değişimleri
                amountString = amountString.Replace(".", "").Replace(",", ".");
                balanceString = balanceString.Replace(".", "").Replace(",", ".");

                if (amountString.StartsWith(","))
                {
                    amountString = "0" + amountString;
                }

                if (balanceString.StartsWith(","))
                {
                    balanceString = "0" + balanceString;
                }

                decimal amount = Decimal.Parse(amountString, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                decimal balance = Decimal.Parse(balanceString, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                string description = match.Groups["Description"].Value.Trim();

                transactions.Add(new BankTransaction
                {
                    Date = date,
                    Reference = reference,
                    Amount = amount,
                    Balance = balance,
                    Description = description
                });
            }

            return transactions;
        }
    }
}
