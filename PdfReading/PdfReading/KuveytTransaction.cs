using System;
namespace PdfReading
{
	public class KuveytTransaction
	{
        public string ProcessDate { get; set; }
        public string ReferanceCode { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string Branch { get; set; }
    }
}

