using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class AccountHistory
    {
        public int Id { get; set; }

        public virtual Account Account { get; set; }

        public DateTime BalanceDate { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Balance { get; set; }
    }
}
