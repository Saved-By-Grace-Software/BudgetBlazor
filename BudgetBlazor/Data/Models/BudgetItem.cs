namespace BudgetBlazor.Data.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Budget { get; set; }

        public List<Transaction> Transactions { get; set; }

        public Guid User { get; set; }

        public BudgetItem()
        {
            Transactions = new List<Transaction>();
            Name = "";
        }
    }
}
