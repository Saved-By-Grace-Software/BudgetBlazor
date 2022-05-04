namespace DataAccess.Models
{
    public class Automation
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public bool IsStrict { get; set; }

        public virtual List<AutomationRule> Rules { get; set; }

        public virtual BudgetItem DefaultBudgetToSet { get; set; }

        public Guid User { get; set; }

        public Automation(string name, Guid user)
        {
            Name = name;
            User = user;
            IsActive = true;
            IsStrict = false;
        }
    }
}
