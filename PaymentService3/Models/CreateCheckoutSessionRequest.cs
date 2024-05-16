namespace PaymentService3.Models
{
    public class CreateCheckoutSessionRequest
    {
        public string ProductName { get; set; }
        public int ProductPrice { get; set; }
        public int Quantity { get; set; }

        public int CustomerId { get; set; }
    }
}
