using System;
namespace PdfReading
{
	public class İsbankTransaction
	{
        public DateTime Date { get; set; }
        public string Branch { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public decimal AdditionalBalance { get; set; }
        public string ProcessType { get; set; }
        public string Description { get; set; } 
    }
}

