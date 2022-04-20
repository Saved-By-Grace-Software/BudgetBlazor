using MudBlazor;

namespace BudgetBlazor.Data.Models
{
    public class BudgetCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public List<BudgetItem> BudgetItems { get; set; }

        public BudgetCategory(string name)
        {
            BudgetItems = new List<BudgetItem>();
            Name = name;
            Color = "rgb(200, 150, 100, 0.3)";

            // DEBUG - Fix when real data in EF
            Random random = new Random();
            int numItems = random.Next(10);
            for (int i=0; i < numItems; i++)
            {
                BudgetItem item = new BudgetItem("Budget " + i);
                BudgetItems.Add(item);
            }
            // END DEBUG
        }
    }
}
