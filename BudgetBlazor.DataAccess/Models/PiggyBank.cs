using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetBlazor.DataAccess.Models
{
    public class PiggyBank
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal CurrentAmount { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TargetAmount { get; set; }

        public DateTime TargetDate { get; set; }

        public Guid User { get; set; }

        public virtual Account? SavingsAccount { get; set; }

        [NotMapped]
        public decimal RemainingAmount
        {
            get
            {
                return TargetAmount - CurrentAmount;
            }
        }

        [NotMapped]
        public decimal PerMonthAmount
        {
            get
            {
                int monthsApart = (12 * (TargetDate.Year - DateTime.Now.Year)) + (TargetDate.Month - DateTime.Now.Month);
                if (monthsApart == 0)
                    return RemainingAmount;
                else
                    return RemainingAmount / Math.Abs(monthsApart);
            }
        }

        [NotMapped]
        public int PercentSaved
        {
            get
            {
                if (TargetAmount == 0)
                    return 0;
                else
                    return (int)(CurrentAmount / TargetAmount * 100);
            }
        }

        public PiggyBank(string name, Guid user)
        {
            Name = name;
            User = user;
            TargetDate = DateTime.Now;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
