﻿using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int FITransactionId { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        public string? CheckNumber { get; set; }

        public DateTime TransactionDate { get; set; }

        public bool IsSplit { get; set; }

        public bool IsPartial { get; set; }

        public virtual BudgetItem? Budget { get; set; }

        public Guid User { get; set; }

        public Transaction(string name)
        {
            Name = name;
            TransactionDate = DateTime.Now;
        }

        public Transaction(string name, Guid user)
        {
            Name = name;
            TransactionDate = DateTime.Now;
            User = user;
        }
    }
}
