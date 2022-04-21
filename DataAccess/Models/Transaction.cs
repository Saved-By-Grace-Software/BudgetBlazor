namespace DataAccess.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int TransactionId { get; set; }

        public decimal Amount { get; set; }

        public string? CheckNumber { get; set; }

        public DateTime TransactionDate { get; set; }

        public bool IsSplit { get; set; }

        public bool IsPartial { get; set; }

        public Guid User { get; set; }

        public Transaction()
        {
            Name = "";
        }
    }
}
