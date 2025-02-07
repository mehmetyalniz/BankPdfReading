using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PdfReading.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly HttpClient _httpClient;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [HttpGet("pdftransactions")]
        public async Task<IActionResult> ExtractTransactions([FromQuery] string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("URL gereklidir.");
            }

            try
            {
                // 1. PDF'yi indir
                byte[] pdfBytes = await _httpClient.GetByteArrayAsync(url);

                // 2. PDF içeriğini belirli bir sayfadan al (örneğin ilk sayfa)
                string firstPageText;
                using (MemoryStream stream = new MemoryStream(pdfBytes))
                {
                    using (PdfReader reader = new PdfReader(stream))
                    {
                        using (PdfDocument pdfDoc = new PdfDocument(reader))
                        {
                            firstPageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(1));
                        }
                    }
                }

                // 3. PDF'nin hangi bankaya ait olduğunu belirle
                List<object> transactions = null;


                if (firstPageText.Contains("Ziraat Bankası") || firstPageText.Contains("ziraat"))
                {
                    string pdfText = Helper.ExtractTextFromPdf(url);
                    // Ziraat Bankası PDF'si
                    var classifiedTransactions = Helper.ClassifyPdfContent(pdfText); // Metni sınıflandırıyoruz
                    transactions = classifiedTransactions.Cast<object>().ToList(); // Cast işlemi
                }
                else if (firstPageText.Contains("www.vakifbank.com.tr") || firstPageText.Contains("Türkiye Vakiflar T.A.O Büyük Mükellefler"))
                {
                    // Vakıfbank PDF'si
                    var classifiedTransactions = VakifHelper.ExtractTransactionsFromPdf(pdfBytes);
                    transactions = classifiedTransactions.Cast<object>().ToList(); // Cast işlemi
                }
                else if (firstPageText.Contains("www.kuveytturk.com.tr") || firstPageText.Contains("444 0 123"))
                {
                    // Kuveytturk PDF'si
                    var classifiedTransactions = KuveytHelper.KuveytTransactionsFromPdf(pdfBytes);
                    transactions = classifiedTransactions.Cast<object>().ToList(); // Cast işlemi
                }
                else if (firstPageText.Contains("481 005 8590") || firstPageText.Contains("Büyük Mükellefler V. D. Başkanlığı"))
                {
                    // İsbank PDF'si
                    var classifiedTransactions = İsbankHelper.İsbankTransactionsFromPdf(pdfBytes);
                    transactions = classifiedTransactions.Cast<object>().ToList(); // Cast işlemi
                }
                else
                {
                    return BadRequest("Desteklenmeyen PDF formatı.");
                }

                // 4. İşlemleri geri döndür
                if (transactions == null || transactions.Count == 0)
                {
                    return NotFound("Herhangi bir hesap hareketi bulunamadı.");
                }

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }
    }


}