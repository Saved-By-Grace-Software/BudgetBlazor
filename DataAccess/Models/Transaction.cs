﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    [Index(nameof(Name), nameof(Amount), nameof(TransactionDate), nameof(User), nameof(FITransactionId), IsUnique = true)]
    public class Transaction
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [StringLength(50)]
        public string FITransactionId { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string? CheckNumber { get; set; }

        public DateTime TransactionDate { get; set; }

        public bool IsSplit { get; set; }

        public bool IsPartial { get; set; }

        public virtual BudgetItem? Budget { get; set; }

        public Guid User { get; set; }

        public Transaction(string name, Guid user)
        {
            Name = name;
            TransactionDate = DateTime.Now;
            User = user;
            IsPartial = false;
            IsSplit = false;
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
    }
}
