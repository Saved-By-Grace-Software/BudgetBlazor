using BudgetBlazor.Helpers;
using BudgetBlazor.Pages.Page_Components;
using BudgetBlazor.DataAccess.Models;
using BudgetBlazor.DataAccess.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Plotly.Blazor;
using Title = Plotly.Blazor.LayoutLib.Title;
using YAxis = Plotly.Blazor.LayoutLib.YAxis;

namespace BudgetBlazor.Pages
{
    public class AccountTransactionsBase : ComponentBase
    {
        #region Dependency Injection & Cascading Parameters
        [Inject]
        protected ISnackbar Snackbar { get; set; }

        [Inject]
        protected IDialogService DialogService { get; set; }

        [Inject]
        protected IBudgetDataService BudgetDataService { get; set; }

        [Inject]
        protected AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        [CascadingParameter]
        protected MudTheme CurrentTheme { get; set; }
        #endregion

        [Parameter] public string AccountId { get; set; }
        protected Account? Account { get; set; }
        protected Guid _currentUserId;
        protected string searchString = "";
        protected List<BreadcrumbItem> _items = new List<BreadcrumbItem>();
        protected Variant _filterButtonVariant = Variant.Outlined;
        private bool _showOnlyUnbudgeted = false;
        protected bool _isAccountLoading = true;
        protected PlotlyChart chart;
        protected Config config { get; set; }
        protected Layout layout { get; set; }
        protected IList<ITrace> data { get; set; }
        private Transaction _transactionBeforeEdit;
        protected MudTable<Transaction> table;
        protected bool _isNextPage = true;

        /// <summary>
        /// Lifecycle method called when the page is initialized
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var authstate = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _currentUserId = Guid.Parse(authstate.User.Claims.First().Value);

            Account = new Account("Loading Account...");

            BudgetDataService.AccountDataChanged += BudgetDataService_AccountDataChanged;
        }

        /// <summary>
        /// Lifecycle method called after the page is rendered
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && int.TryParse(AccountId, out int id))
            {
                Account = BudgetDataService.GetAccount(id, _currentUserId);

                if (Account != default(Account))
                {
                    _items = new List<BreadcrumbItem>
                    {
                        new BreadcrumbItem("Accounts", href: "Accounts", icon: Icons.Material.Filled.CreditCard),
                        new BreadcrumbItem(Account == null ? "Account" : Account.Name, href: null, disabled: true)
                    };

                    // Add the history data for the chart
                    data = new List<ITrace>();
                    data.Add(ChartHelpers.GetScatterDataForAccount(Account.Id, Account.Name, BudgetDataService));

                    // Configure the chart
                    config = new()
                    {
                        Responsive = true,
                        DisplayModeBar = Plotly.Blazor.ConfigLib.DisplayModeBarEnum.False
                    };

                    layout = new()
                    {
                        Title = new Title
                        {
                            Text = String.Format("Balance - {0:C}", Account.CurrentBalance),
                            Font = new Plotly.Blazor.LayoutLib.TitleLib.Font()
                            {
                                Size = 30
                            }
                        },
                        YAxis = new List<YAxis>
                        {
                            new()
                            {
                                Title = new Plotly.Blazor.LayoutLib.YAxisLib.Title { Text = "Account Balance" }
                            }
                        }
                    };

                    // Done loading data, show the data
                    _isAccountLoading = false;
                    StateHasChanged();
                }
            }
        }

        /// <summary>
        /// Opens the dialog to add a new transaction
        /// </summary>
        /// <returns></returns>
        protected async Task OpenAddTransactionDialog()
        {
            // Open the dialog
            var parameters = new DialogParameters { ["Transaction"] = new Transaction("", _currentUserId) };
            var dialogRef = DialogService.Show<EditTransactionDialog>("Add New Transaction", parameters);

            // Wait for a response and add the Account
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                // Add the new transaction
                Transaction transaction = (Transaction)res.Data;
                Account.Transactions.Add(transaction);
                BudgetDataService.Update(Account);
            }
        }

        /// <summary>
        /// Opens the dialog to edit a transaction
        /// </summary>
        /// <returns></returns>
        protected async Task OpenEditTransactionDialog(Transaction transactionToEdit)
        {
            // Backup the transaction to edit
            _transactionBeforeEdit = transactionToEdit.DuplicateTransaction();

            // Open the dialog
            var parameters = new DialogParameters { ["Transaction"] = transactionToEdit };
            var dialogRef = DialogService.Show<EditTransactionDialog>("Edit Transaction", parameters);

            // Wait for a response and add the Account
            var res = await dialogRef.Result;
            if (!res.Cancelled)
            {
                // Update the transaction
                Transaction transaction = (Transaction)res.Data;
                int index = Account.Transactions.FindIndex(t => t.Id == transaction.Id);
                if (index != -1)
                {
                    Account.Transactions[index] = transaction;
                    BudgetDataService.Update(Account);
                }
            }
            else
            {
                // Edit was cancelled, revert the transaction to the before transaction
                transactionToEdit.ResetTransaction(_transactionBeforeEdit);

                // Reset the before edit transaction
                _transactionBeforeEdit = null;
            }
        }

        /// <summary>
        /// Shows a confirmation dialog and then deletes the budget
        /// </summary>
        protected async Task DeleteTransaction(Transaction transactionToDelete)
        {
            bool? result = await DialogService.ShowMessageBox(
                "Warning",
                "Deleting a transaction cannot be undone!",
                yesText: "Delete!", cancelText: "Cancel");

            if (result != null && result == true)
            {
                // Delete the transaction 
                BudgetDataService.Delete(transactionToDelete);
            }
        }

        #region Table Filter Functions
        protected bool FilterFunc(Transaction transaction, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;
            if (transaction.Name.Contains(searchString, StringComparison.CurrentCultureIgnoreCase))
                return true;
            if (transaction.Budget != null && transaction.Budget.Name.Contains(searchString, StringComparison.CurrentCultureIgnoreCase))
                return true;
            if (transaction.Amount.ToString().Contains(searchString, StringComparison.CurrentCultureIgnoreCase))
                return true;

            // Check within splits
            if (transaction.Splits.Count > 0)
            {
                if (transaction.Splits.Where(s => s.Budget != null &&
                                s.Budget.Name.Contains(searchString, StringComparison.CurrentCultureIgnoreCase)).Count() > 0)
                {
                    return true;
                }

                if (transaction.Splits.Where(s => s.Amount.ToString().Contains(searchString, StringComparison.CurrentCultureIgnoreCase)).Count() > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<TableData<Transaction>> LoadTransactions(TableState state)
        {
            IEnumerable<Transaction> filteredData;

            // Apply the sorting
            switch(state.SortLabel)
            {
                case "Amount":
                    if (state.SortDirection == SortDirection.Ascending)
                    {
                        filteredData = Account.Transactions.OrderBy(t => t.Amount);
                    }
                    else
                    {
                        filteredData = Account.Transactions.OrderByDescending(t => t.Amount);
                    }
                    break;
                case "Description":
                    if (state.SortDirection == SortDirection.Ascending)
                    {
                        filteredData = Account.Transactions.OrderBy(t => t.Name);
                    }
                    else
                    {
                        filteredData = Account.Transactions.OrderByDescending(t => t.Name);
                    }
                    break;
                case "Date":
                default:
                    if (state.SortDirection == SortDirection.Ascending)
                    {
                        filteredData = Account.Transactions.OrderBy(t => t.TransactionDate);
                    }
                    else
                    {
                        filteredData = Account.Transactions.OrderByDescending(t => t.TransactionDate);
                    }
                    break;
            }

            // Filter unbudgeted
            if (_showOnlyUnbudgeted)
            {
                filteredData = filteredData.Where(d => d.Budget == default(BudgetItem) && !d.IsIncome);
            }

            // Filter by search string
            filteredData = filteredData.Where(t => FilterFunc(t, searchString));

            // Check for the end
            IEnumerable<Transaction> dataToShow = filteredData.Skip(state.Page * state.PageSize).Take(state.PageSize);
            _isNextPage = dataToShow.Count() >= state.PageSize;

            return new TableData<Transaction>() { Items = filteredData.Skip(state.Page * state.PageSize).Take(state.PageSize), TotalItems = Account.Transactions.Count };
        }

        protected async Task FilterUnbudgeted()
        {
            // Toggle the unbudgeted filter
            _showOnlyUnbudgeted = !_showOnlyUnbudgeted;
            if (_showOnlyUnbudgeted)
                _filterButtonVariant = Variant.Filled;
            else
                _filterButtonVariant = Variant.Outlined;

            // Set the table back to page 0
            table.CurrentPage = 0;

            // Trigger the table to reload data
            table.ReloadServerData();
        }

        protected void OnSearch(string text)
        {
            searchString = text;
            table.ReloadServerData();
        }

        protected async Task BackPage()
        {
            if (table.CurrentPage > 0)
            {
                table.CurrentPage--;
                table.ReloadServerData();
            }
        }

        protected async Task FirstPage()
        {
            table.CurrentPage = 0;
            table.ReloadServerData();
        }

        protected async Task NextPage()
        {
            table.CurrentPage++;
            table.ReloadServerData();
        }
        #endregion

        #region Event Functions
        /// <summary>
        /// Subscriber to BudgetDataService account changed event, updates UI when account data has changed
        /// </summary>
        private void BudgetDataService_AccountDataChanged()
        {
            StateHasChanged();
        }
        #endregion

        #region Import Transactions Functions
        protected async Task UploadFiles(InputFileChangeEventArgs e)
        {
            Snackbar.Add("Transaction import has started, please do not exit this page until it has completed.");

            // Copy the uploaded file into a string buffer
            string transactionsFile = await new StreamReader(e.File.OpenReadStream()).ReadToEndAsync();

            // Check the file type
            if (e.File.ContentType == "text/csv")
            {
                // CSV file uploaded, use CSV parser
                if (Account != default(Account) && await TransactionImporter.ImportCSVTransactions(transactionsFile, (Account)Account, BudgetDataService))
                {
                    Snackbar.Add("Successfully imported transactions!");
                }
                else
                {
                    Snackbar.Add("Error importing transactions!");
                }
            }
            else
            {
                // OFX file uploaded, use OFX parser
                if (Account != default(Account) && await TransactionImporter.ImportOFXTransactions(transactionsFile, (Account)Account, BudgetDataService))
                {
                    Snackbar.Add("Successfully imported transactions!");
                }
                else
                {
                    Snackbar.Add("Error importing transactions!");
                }
            }
        }
        #endregion
    }
}
