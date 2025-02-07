using System;
using System.Security.Cryptography.Xml;

namespace PdfReading
{
	public class BankTransaction
	{
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string Reference { get; set; }
    }
}

