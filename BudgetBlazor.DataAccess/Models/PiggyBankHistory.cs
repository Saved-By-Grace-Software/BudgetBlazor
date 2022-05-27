using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetBlazor.DataAccess.Models
{
    public class PiggyBankHistory
    {
        public int Id { get; set; }

        public virtual PiggyBank PiggyBank { get; set; }

        public DateTime SavedAmountDate { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal SavedAmount { get; set; }
    }
}
