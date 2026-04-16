using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using POSSystem.Data;
using POSSystem.Models;

using System.Collections.ObjectModel;

namespace POSSystem.Services
{
    // Interface for transaction service
    public interface ITransactionService
    {
        Transaction CreateTransaction();
        void HoldTransaction(Transaction transaction);
        ObservableCollection<Transaction> GetHeldTransactions();
        ObservableCollection<Transaction> GetTransactionsHistory();
        Transaction ResumeTransaction(string transactionId);
        void AbortTransaction(Transaction transaction);
        void CompleteTransaction(Transaction transaction);
        int GetHeldTransactionCount();
    }

    // Transaction service implementation
    public class TransactionService : ITransactionService
    {
        private ObservableCollection<Transaction> _heldTransactions;
        private readonly LocalView<Transaction> _completedTransactions;
        private readonly POSDbContext _dbContext;

        public TransactionService(POSDbContext context)
        {
            _heldTransactions = [];
            _dbContext = context;
            _dbContext.Transactions.Include(t => t.Cart).Load();
            _completedTransactions = _dbContext.Transactions.Local;
        }

        public Transaction CreateTransaction()
        {
            return new Transaction();
        }

        public void HoldTransaction(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (transaction.Cart.Count == 0)
                throw new InvalidOperationException("Cannot hold an empty transaction");

            transaction.Status = "Held";
            
            // Create a clone to prevent reference issues
            var heldTransaction = transaction.Clone();
            _heldTransactions.Add(heldTransaction);
        }

        public ObservableCollection<Transaction> GetHeldTransactions()
        {
            return _heldTransactions;
        }

        public Transaction ResumeTransaction(string transactionId)
        {
            var heldTransaction = _heldTransactions.FirstOrDefault(t => t.Id == transactionId);
            
            if (heldTransaction == null)
                throw new InvalidOperationException("Transaction not found");

            // Remove from held list
            _heldTransactions.Remove(heldTransaction);

            // Clone and set as active
            var resumedTransaction = heldTransaction.Clone();
            resumedTransaction.Status = "Active";
            
            return resumedTransaction;
        }

        public void AbortTransaction(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            transaction.Status = "Aborted";
            transaction.Cart.Clear();
        }

        public void CompleteTransaction(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (transaction.Cart.Count == 0)
                throw new InvalidOperationException("Cannot complete an empty transaction");

            transaction.Status = "Completed";
            transaction.CalculateTotals();
            
            _completedTransactions.Add(transaction);
            _dbContext.SaveChanges();
        }

        public int GetHeldTransactionCount()
        {
            return _heldTransactions.Count;
        }

        public ObservableCollection<Transaction> GetTransactionsHistory()
        {
            return _completedTransactions.ToObservableCollection();
        }
    }
}
