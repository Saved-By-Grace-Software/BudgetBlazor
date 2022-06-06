using System.ComponentModel.DataAnnotations;

namespace BudgetBlazor.DataAccess.Models
{
    public class AutomationCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [StringLength(50)]
        public string Color { get; set; }

        public virtual List<Automation> Automations { get; set; }

        public Guid User { get; set; }

        public AutomationCategory(string name, Guid user) : this(name, user, "#9d9d9d41") { }

        public AutomationCategory(string name, Guid user, string color)
        {
            Automations = new List<Automation>();
            Name = name;
            User = user;
            Color = color;
        }
    }
}
