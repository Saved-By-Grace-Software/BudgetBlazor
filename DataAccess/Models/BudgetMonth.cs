using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class BudgetMonth
    {
        public int Id { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal ExpectedIncome { get; set; }

        public virtual List<BudgetCategory> BudgetCategories { get; set; }

        public Guid User { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal ActualIncome { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalBudgeted { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalSpent { get; set; }

        [NotMapped]
        public int PercentBudgeted
        {
            get
            {
                if (ExpectedIncome == 0)
                    return 0;
                else
                    return (int)(TotalBudgeted / ExpectedIncome * 100);
            }
        }

        [NotMapped]
        public int PercentSpent
        {
            get
            {
                if (TotalSpent == 0)
                    return 0;
                else if (ActualIncome == 0)
                    return 100;
                else
                    return (int)(-TotalSpent / ActualIncome * 100);
            }
        }

        public BudgetMonth(int year, int month, Guid user)
        {
            BudgetCategories = new List<BudgetCategory>();
            Year = year;
            Month = month;
            User = user;
        }
    }
}
