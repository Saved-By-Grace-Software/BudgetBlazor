using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetBlazor.DataAccess.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Budget { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Spent { get; set; }

        [NotMapped]
        public decimal Remaining 
        { 
            get
            {
                return Budget + Spent;
            }
        }

        public BudgetItem(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
