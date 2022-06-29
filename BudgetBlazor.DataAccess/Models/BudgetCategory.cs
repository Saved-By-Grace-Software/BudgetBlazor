using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetBlazor.DataAccess.Models
{
    public class BudgetCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [StringLength(50)]
        public string Color { get; set; }

        public virtual List<BudgetItem> BudgetItems { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Budgeted { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Spent { get; set; }

        [NotMapped]
        public decimal Remaining 
        { 
            get
            {
                return Budgeted + Spent;
            }
        }

        public BudgetCategory(string name) : this(name, "#1ec8a54d") { }

        public BudgetCategory(string name, string color)
        {
            BudgetItems = new List<BudgetItem>();
            Name = name;
            Color = color;
        }
    }
}
