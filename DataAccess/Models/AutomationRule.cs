namespace DataAccess.Models
{
    public class AutomationRule
    {
        public int Id { get; set; }

        public string ContainsText { get; set; }

        public bool StopAfterTrigger { get; set; }

        public AutomationRule(string containsText)
        {
            ContainsText = containsText;
            StopAfterTrigger = false;
        }
    }
}
