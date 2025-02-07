using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PdfReading.Services
{
    public class PdfProcessingService : IHostedService, IDisposable
    {
        private Timer _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Timer'ı 5 dakikada bir çalışacak şekilde ayarlıyoruz
            _timer = new Timer(ProcessPdf, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }

        private void ProcessPdf(object state)
        {
            Console.WriteLine("PDF işleme başladı...");

            // Örnek PDF verisi (bu veri genellikle bir URL'den indirilir veya başka bir kaynaktan sağlanır)
            string pdfPath = "path_to_your_pdf.pdf"; // Gerçek PDF dosyasını buraya ekleyin

            if (File.Exists(pdfPath))
            {
                byte[] pdfBytes = File.ReadAllBytes(pdfPath);

                // İsbankTransactionsFromPdf metodunu çalıştırarak işlemleri çekiyoruz
                var transactions = İsbankHelper.İsbankTransactionsFromPdf(pdfBytes);

                // İşlemleri console'a yazdırıyoruz (gerçek senaryoda başka bir işlem yapılabilir)
                foreach (var transaction in transactions)
                {
                    Console.WriteLine($"{transaction.Date} - {transaction.Branch} - {transaction.Amount}");
                }
            }
            else
            {
                Console.WriteLine("PDF dosyası bulunamadı.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0); // Timer'ı durdur
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose(); // Timer'ı yok et
        }
    }
}
