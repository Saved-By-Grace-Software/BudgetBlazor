using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetBlazor.DataAccess.Models
{
    public class Account
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [StringLength(50)]
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
            Transactions = new List<Transaction>();
        }

        public override string ToString()
        {
            return Name;
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
