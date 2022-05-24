-- Create local temp table for BudgetMonths
CREATE TABLE #TransferBudgetMonths
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
	[Month] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[ExpectedIncome] [decimal](12, 2) NOT NULL,
	[User] [uniqueidentifier] NOT NULL,
	[ActualIncome] [decimal](12, 2) NOT NULL,
	[TotalBudgeted] [decimal](12, 2) NOT NULL,
	[TotalSpent] [decimal](12, 2) NOT NULL
)

-- Create local temp table for BudgetCategory
CREATE TABLE #TransferBudgetCategory
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Color] [nvarchar](255) NOT NULL,
	[Budgeted] [decimal](12, 2) NOT NULL,
	[Spent] [decimal](12, 2) NOT NULL,
	[Remaining] [decimal](12, 2) NOT NULL,
	[BudgetMonthId] [int] NULL
)

-- Create local temp table for BudgetItem
CREATE TABLE #TransferBudgetItem
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Budget] [decimal](12, 2) NOT NULL,
	[Spent] [decimal](12, 2) NOT NULL,
	[Remaining] [decimal](12, 2) NOT NULL,
	[BudgetCategoryId] [int] NULL
)

-- Create local temp table for Transactions
CREATE TABLE #TransferTransactions
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[FITransactionId] [nvarchar](50) NULL,
	[Amount] [decimal](12, 2) NOT NULL,
	[CheckNumber] [nvarchar](50) NULL,
	[TransactionDate] [datetime2](7) NOT NULL,
	[IsSplit] [bit] NOT NULL,
	[IsPartial] [bit] NOT NULL,
	[User] [uniqueidentifier] NOT NULL,
	[BudgetId] [int] NULL,
	[AccountId] [int] NULL,
	[IsIncome] [bit] NOT NULL,
	[TransactionId] [int] NULL,
	[Notes] [nvarchar](255) NULL
)

DECLARE @DestinationUserId AS NVARCHAR(50) = 'ab029536-0775-48fb-a711-6bd619e2e933'
DECLARE @SourceUserId AS NVARCHAR(50) = 'f946d0eb-f463-4c72-86fa-32ae69561a84'

-- Get the offset for the month ID
DECLARE @NextMonthId AS INT = 11
--SELECT @NextMonthId = IDENT_CURRENT('[dbo].[BudgetBlazor_BudgetMonths]');

-- Get the offset for the category ID
DECLARE @NextCatId AS INT = 64
--SELECT @NextCatId = IDENT_CURRENT('BudgetBlazor.dbo.BudgetCategory');

-- Get the offset for the budget item ID
DECLARE @NextItemId AS INT = 181
--SELECT @NextItemId = IDENT_CURRENT('BudgetBlazor.dbo.BudgetItem');

-- Create temp mapping table for account mapping
CREATE TABLE #AccountMap
(
	SourceAccount INT,
	DestAccount INT
)
INSERT INTO #AccountMap VALUES (1, 3)
INSERT INTO #AccountMap VALUES (2, 2)
INSERT INTO #AccountMap VALUES (3, 1)
INSERT INTO #AccountMap VALUES (6, 1)

-- Create temp lookup table for the category enum
CREATE TABLE #CategoryEnum
(
	ID INT,
	CatName NVarChar(50)
)
INSERT INTO #CategoryEnum VALUES (0, 'Charity')
INSERT INTO #CategoryEnum VALUES (1, 'Savings')
INSERT INTO #CategoryEnum VALUES (2, 'Housing')
INSERT INTO #CategoryEnum VALUES (3, 'Food')
INSERT INTO #CategoryEnum VALUES (4, 'Clothing')
INSERT INTO #CategoryEnum VALUES (5, 'Transportation')
INSERT INTO #CategoryEnum VALUES (6, 'Utilities')
INSERT INTO #CategoryEnum VALUES (7, 'Medical')
INSERT INTO #CategoryEnum VALUES (8, 'Insurance')
INSERT INTO #CategoryEnum VALUES (9, 'Recreation')
INSERT INTO #CategoryEnum VALUES (10, 'Personal')
INSERT INTO #CategoryEnum VALUES (11, 'Debts')
INSERT INTO #CategoryEnum VALUES (20, 'Income')
INSERT INTO #CategoryEnum VALUES (100, 'Staff')
INSERT INTO #CategoryEnum VALUES (101, 'BuildingAndOperatingExpenses')
INSERT INTO #CategoryEnum VALUES (102, 'Ministries')
INSERT INTO #CategoryEnum VALUES (103, 'LongTermSavings')

-- Copy over the budget months (prior to April 2022)
Set IDENTITY_INSERT #TransferBudgetMonths On
INSERT INTO #TransferBudgetMonths ([Id], [Month], [Year], [ExpectedIncome], [User], [ActualIncome], [TotalBudgeted], [TotalSpent])
SELECT ([MonthId] + @NextMonthId) as [Id], [Name] as [Month], [Year], [ExpectedIncome], @DestinationUserId as [User], [ActualIncome], [TotalBudgeted], [Spent] as [TotalSpent]
FROM [Budget].[dbo].[BudgetMonths]
WHERE [Budget].[dbo].[BudgetMonths].[UserID] = @SourceUserId AND ([Year] < 2022 OR [Name] < 4)
Set Identity_Insert #TransferBudgetMonths Off

-- Copy over the budget categories
Set IDENTITY_INSERT #TransferBudgetCategory On
INSERT INTO #TransferBudgetCategory ([Id], [Name], [Color], [Budgeted], [Spent], [Remaining], [BudgetMonthId])
SELECT ([CategoryID] + @NextCatId) as [Id], [#CategoryEnum].[CatName] as [Name], '#85858657' as [Color], [Budgeted], [Spent], [Remaining], ([MonthId] + @NextMonthId) as [BudgetMonthId]
FROM [Budget].[dbo].[BudgetCategories]
INNER JOIN #CategoryEnum on #CategoryEnum.ID = [BudgetCategories].Type
INNER JOIN #TransferBudgetMonths on #TransferBudgetMonths.[Id] = ([MonthId] + @NextMonthId)
Set IDENTITY_INSERT #TransferBudgetCategory Off

-- Copy over the budget items
Set IDENTITY_INSERT #TransferBudgetItem On
INSERT INTO #TransferBudgetItem ([Id], [Name], [Budget], [Spent], [Remaining], [BudgetCategoryId])
SELECT ([BudgetItemDataID] + @NextItemId) as [Id], [BID].[Name], [BID].[Budgeted] as [Budget], [BID].[Spent], [BID].[Remaining], ([CategoryID] + @NextCatId) as [BudgetCategoryId]
FROM [Budget].[dbo].[BudgetItemDatas] as [BID]
INNER JOIN #TransferBudgetCategory on #TransferBudgetCategory.[Id] = ([CategoryID] + @NextCatId)
Set IDENTITY_INSERT #TransferBudgetItem Off

-- Copy over the transactions
INSERT INTO #TransferTransactions ([Name], [FITransactionId], [Amount], [CheckNumber], [TransactionDate], [IsSplit], [IsPartial], [User], [BudgetId], [AccountId], [IsIncome])
SELECT [Budget].[dbo].[Transactions].[Name], [TransactionID] as [FITransactionId], [Amount], [CheckNumber], [TransactionDate], [IsSplit], [IsPartial],
	@DestinationUserId as [User],
	(SELECT IIF(BudgetItemDataID < 1, NULL, ([BudgetItemDataID] + @NextItemId)) FROM [Budget].[dbo].[Transactions] as [InTran] WHERE [Budget].[dbo].[Transactions].[ID] = [InTran].[ID]) as [BudgetId],
	(SELECT [DestAccount] FROM #AccountMap WHERE [SourceAccount] = [TransactionsAccountId]) as [AccountId],
	0 as [IsIncome]
FROM [Budget].[dbo].[Transactions]
WHERE [Budget].[dbo].[Transactions].[UserID] = @SourceUserId

--SELECT * FROM #TransferBudgetMonths
--SELECT * FROM #TransferBudgetCategory
--SELECT * FROM #TransferBudgetItem
--SELECT * FROM #TransferTransactions

-- Clear the temp tables etc.
DROP TABLE #CategoryEnum
DROP TABLE #AccountMap
DROP TABLE #TransferBudgetMonths
DROP TABLE #TransferBudgetCategory
DROP TABLE #TransferBudgetItem
DROP TABLE #TransferTransactions