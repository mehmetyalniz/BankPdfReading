using System;
namespace PdfReading
{
	public class VakifTransaction
	{
        public string Date { get; set; }
        public string Time { get; set; }
        public string ProcessNo { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string ProcessName { get; set; }
    }
}

