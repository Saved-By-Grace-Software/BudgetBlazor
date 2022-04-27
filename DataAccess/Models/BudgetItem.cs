using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Budget { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Spent { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Remaining { get; set; }

        public BudgetItem(string name)
        {
            Name = name;
        }
    }
}
