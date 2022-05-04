using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class AutomationCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [StringLength(50)]
        public string Color { get; set; }

        public virtual List<Automation> Automations { get; set; }

        public Guid User { get; set; }

        public AutomationCategory(string name) : this(name, "#1ec8a54d") { }

        public AutomationCategory(string name, string color)
        {
            Automations = new List<Automation>();
            Name = name;
            Color = color;
        }
    }
}
