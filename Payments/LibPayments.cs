namespace LibPayments
{
    public class Payment
    {
        public DateTimeOffset PaidOn { get; set; }
        public decimal amount { get; set; }
    }

    public class AllPayments
    {
        public List<Payment> Payments { set; get; }
    }
}