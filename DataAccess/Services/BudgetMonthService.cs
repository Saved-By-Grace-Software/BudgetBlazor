using DataAccess.Data;
using DataAccess.Models;

namespace DataAccess.Services
{
    public class BudgetMonthService : IBudgetMonthService
    {
        private readonly ApplicationDbContext _db;

        public BudgetMonthService(ApplicationDbContext db)
        {
            _db = db;
        }

        public BudgetMonth Create(int year, int month)
        {
            // Create a new budget month
            BudgetMonth budgetMonth = new BudgetMonth(year, month);

            // Add it to the database
            _db.BudgetMonths.Add(budgetMonth);

            // Return the new month
            return budgetMonth;
        }

        public BudgetMonth Get(int budgetMonthId)
        {
            throw new NotImplementedException();
        }

        public BudgetMonth Get(int year, int month)
        {
            return _db.BudgetMonths.FirstOrDefault(x => x.Year == year && x.Month == month);
        }

        public BudgetMonth GetOrCreate(int year, int month)
        {
            BudgetMonth m = _db.BudgetMonths.FirstOrDefault(x => x.Year == year && x.Month == month);

            if (m == default(BudgetMonth))
            {
                // Current month doesn't exist, create a new one
                m = Create(year, month);
            }

            return m;
        }

        public List<BudgetMonth> GetAll()
        {
            return _db.BudgetMonths.ToList();
        }

        public BudgetMonth Update(BudgetMonth budgetMonth)
        {
            throw new NotImplementedException();
        }

        public void Delete(int budgetMonthId)
        {
            throw new NotImplementedException();
        }
    }
}
