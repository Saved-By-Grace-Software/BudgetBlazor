using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetBlazor.DataAccess.Models
{
    [Index(nameof(Name), nameof(Amount), nameof(TransactionDate), nameof(User), nameof(FITransactionId), IsUnique = true)]
    public class Transaction
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [StringLength(50)]
        public string? FITransactionId { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string? CheckNumber { get; set; }

        public DateTime TransactionDate { get; set; }

        public bool IsSplit { get; set; }

        public bool IsPartial { get; set; }

        public bool IsIncome { get; set; }

        public string? Notes { get; set; }

        public virtual BudgetItem? Budget { get; set; }

        public virtual List<Transaction> Splits { get; set; }

        public Guid User { get; set; }

        public Transaction(string name, Guid user) : this(name, user, DateTime.Now) { }

        public Transaction(string name, Guid user, DateTime transactionDate)
        {
            Name = name;
            TransactionDate = transactionDate;
            User = user;
            IsPartial = false;
            IsSplit = false;
            IsIncome = false;
            Splits = new List<Transaction>();
        }

        public override bool Equals(object? obj)
        {
            //Check for null 
            if (obj == null)
            {
                return false;
            }
            else
            {
                // Try cast and then evaluate
                try
                {
                    Transaction t = (Transaction)obj;
                    return (Name == t.Name) && (Amount == t.Amount) && (TransactionDate == t.TransactionDate) && (FITransactionId == t.FITransactionId);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns a new transaction with the same values as the current transaction
        /// </summary>
        /// <returns></returns>
        public Transaction DuplicateTransaction()
        {
            // Create a new transaction
            Transaction newTransaction = new Transaction(this.Name, this.User, this.TransactionDate);

            // Duplicate the "standard" parameters
            newTransaction.Amount = this.Amount;
            newTransaction.Budget = this.Budget;
            newTransaction.CheckNumber = this.CheckNumber;
            newTransaction.FITransactionId = this.FITransactionId;
            newTransaction.IsIncome = this.IsIncome;
            newTransaction.IsPartial = this.IsPartial;
            newTransaction.IsSplit = this.IsSplit;
            newTransaction.Notes = this.Notes;

            // Duplicate the splits
            foreach(Transaction split in this.Splits)
            {
                newTransaction.Splits.Add(split.DuplicateTransaction());
            }

            return newTransaction;
        }

        public void ResetTransaction(Transaction originalValues)
        {
            // Reset the "standard" parameters
            this.Name = originalValues.Name;
            this.User = originalValues.User;
            this.TransactionDate = originalValues.TransactionDate;
            this.Amount = originalValues.Amount;
            this.Budget = originalValues.Budget;
            this.CheckNumber = originalValues.CheckNumber;
            this.FITransactionId = originalValues.FITransactionId;
            this.IsIncome = originalValues.IsIncome;
            this.IsPartial = originalValues.IsPartial;
            this.IsSplit = originalValues.IsSplit;
            this.Notes = originalValues.Notes;

            // Reset the splits
            this.Splits = new List<Transaction>();
            foreach (Transaction split in originalValues.Splits)
            {
                this.Splits.Add(split.DuplicateTransaction());
            }
        }
    }
}
