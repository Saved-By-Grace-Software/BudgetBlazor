using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Account
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string AccountNumber { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal CurrentBalance { get; set; }

        public AccountType AccountType { get; set; }

        public DateTime LastUpdated { get; set; }

        public virtual List<Transaction> Transactions { get; set; }

        public Guid User { get; set; }

        public Account(string name)
        {
            Name = name;
            AccountNumber = "";
        }
    }

    public enum AccountType
    {
        [Description("Checking")]
        Checking,
        [Description("Savings")]
        Savings,
        [Description("Credit Card")]
        CreditCard
    }
}
