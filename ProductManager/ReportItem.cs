namespace ProductManager
{
    public class ReportItem
    {
        public int Id { get; set; }   

        public string Time { get; set; }

        public string Date { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public double TotalPrice { get; set; }

        public string PaymentType { get; set; }
    }
}
