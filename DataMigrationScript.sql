DECLARE @DestinationUserId AS NVARCHAR(50) = 'F3D3781A-461C-4AA0-AE9F-C59560C4A6D4'
DECLARE @SourceUserId AS NVARCHAR(50) = 'f946d0eb-f463-4c72-86fa-32ae69561a84'

-- Get the offset for the month ID
DECLARE @NextMonthId AS INT
SELECT @NextMonthId = IDENT_CURRENT('BudgetBlazor.dbo.BudgetMonths');

-- Get the offset for the category ID
DECLARE @NextCatId AS INT
SELECT @NextCatId = IDENT_CURRENT('BudgetBlazor.dbo.BudgetCategory');

-- Get the offset for the budget item ID
DECLARE @NextItemId AS INT
SELECT @NextItemId = IDENT_CURRENT('BudgetBlazor.dbo.BudgetItem');

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
Set IDENTITY_INSERT [BudgetBlazor].[dbo].[BudgetMonths] On
INSERT INTO [BudgetBlazor].[dbo].[BudgetMonths] ([Id], [Month], [Year], [ExpectedIncome], [User], [ActualIncome], [TotalBudgeted], [TotalSpent])
SELECT ([MonthId] + @NextMonthId) as [Id], [Name] as [Month], [Year], [ExpectedIncome], @DestinationUserId as [User], [ActualIncome], [TotalBudgeted], [Spent] as [TotalSpent]
FROM [Budget].[dbo].[BudgetMonths]
WHERE [Budget].[dbo].[BudgetMonths].[UserID] = @SourceUserId AND ([Year] < 2022 OR [Name] < 4)
Set Identity_Insert [BudgetBlazor].[dbo].[BudgetMonths] Off

-- Copy over the budget categories
Set IDENTITY_INSERT [BudgetBlazor].[dbo].[BudgetCategory] On
INSERT INTO [BudgetBlazor].[dbo].[BudgetCategory] ([Id], [Name], [Color], [Budgeted], [Spent], [Remaining], [BudgetMonthId])
SELECT ([CategoryID] + @NextCatId) as [Id], [#CategoryEnum].[CatName] as [Name], '#85858657' as [Color], [Budgeted], [Spent], [Remaining], ([MonthId] + @NextMonthId) as [BudgetMonthId]
FROM [Budget].[dbo].[BudgetCategories]
INNER JOIN #CategoryEnum on #CategoryEnum.ID = [BudgetCategories].Type
INNER JOIN [BudgetBlazor].[dbo].[BudgetMonths] on [BudgetBlazor].[dbo].[BudgetMonths].[Id] = ([MonthId] + @NextMonthId)
Set IDENTITY_INSERT [BudgetBlazor].[dbo].[BudgetCategory] Off

-- Copy over the budget items
Set IDENTITY_INSERT [BudgetBlazor].[dbo].[BudgetItem] On
INSERT INTO [BudgetBlazor].[dbo].[BudgetItem] ([Id], [Name], [Budget], [Spent], [Remaining], [BudgetCategoryId])
SELECT ([BudgetItemDataID] + @NextItemId) as [Id], [BID].[Name], [BID].[Budgeted] as [Budget], [BID].[Spent], [BID].[Remaining], ([CategoryID] + @NextCatId) as [BudgetCategoryId]
FROM [Budget].[dbo].[BudgetItemDatas] as [BID]
INNER JOIN [BudgetBlazor].[dbo].[BudgetCategory] on [BudgetBlazor].[dbo].[BudgetCategory].[Id] = ([CategoryID] + @NextCatId)
Set IDENTITY_INSERT [BudgetBlazor].[dbo].[BudgetItem] Off

-- Copy over the transactions
INSERT INTO [BudgetBlazor].[dbo].[Transactions] ([Name], [FITransactionId], [Amount], [CheckNumber], [TransactionDate], [IsSplit], [IsPartial], [User], [BudgetId], [AccountId], [IsIncome])
SELECT [Budget].[dbo].[Transactions].[Name], [TransactionID] as [FITransactionId], [Amount], [CheckNumber], [TransactionDate], [IsSplit], [IsPartial],
	@DestinationUserId as [User],
	(SELECT IIF(BudgetItemDataID < 1, NULL, ([BudgetItemDataID] + @NextItemId)) FROM [Budget].[dbo].[Transactions] as [InTran] WHERE [Budget].[dbo].[Transactions].[ID] = [InTran].[ID]) as [BudgetId],
	(SELECT [DestAccount] FROM #AccountMap WHERE [SourceAccount] = [TransactionsAccountId]) as [AccountId],
	0 as [IsIncome]
FROM [Budget].[dbo].[Transactions]
WHERE [Budget].[dbo].[Transactions].[UserID] = @SourceUserId

-- Clear the temp tables
DROP TABLE #CategoryEnum
DROP TABLE #AccountMap