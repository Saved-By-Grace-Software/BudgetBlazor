-- Get the offset for the month ID
DECLARE @NextMonthId AS INT
SELECT @NextMonthId = IDENT_CURRENT('BudgetBlazor.dbo.BudgetMonths');

-- Get the offset for the category ID
DECLARE @NextCatId AS INT
SELECT @NextCatId = IDENT_CURRENT('BudgetBlazor.dbo.BudgetCategory');

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
--INSERT INTO [BudgetBlazor].[dbo].[BudgetMonths] ([Id], [Month], [Year], [ExpectedIncome], [User], [ActualIncome], [TotalBudgeted], [TotalSpent])
SELECT ([MonthId] + @NextMonthId) as [Id], [Name] as [Month], [Year], [ExpectedIncome], 'F3D3781A-461C-4AA0-AE9F-C59560C4A6D4' as [User], [ActualIncome], [TotalBudgeted], [Spent] as [TotalSpent]
FROM [Budget].[dbo].[BudgetMonths]
WHERE [Name] < 4 AND [Year] < 2022
Set Identity_Insert [BudgetBlazor].[dbo].[BudgetMonths] Off

------ Copy over the budget categories
--Set IDENTITY_INSERT [BudgetBlazor].[dbo].[BudgetCategory] On
--INSERT INTO [BudgetBlazor].[dbo].[BudgetCategory] ([Id], [Name], [Color], [Budgeted], [Spent], [Remaining], [BudgetMonthId])
--SELECT ([CategoryID] + @NextCatId) as [Id], [#CategoryEnum].[CatName] as [Name], '#85858657' as [Color], [Budgeted], [Spent], [Remaining], ([MonthId] + @NextMonthId) as [BudgetMonthId]
--FROM [Budget].[dbo].[BudgetCategories]
--INNER JOIN #CategoryEnum on #CategoryEnum.ID = [BudgetCategories].Type
--Set IDENTITY_INSERT [BudgetBlazor].[dbo].[BudgetCategory] Off

------ Copy over the budget items
--INSERT INTO [BudgetBlazor].[dbo].[BudgetItem]
--SELECT [Name], [Budgeted] as [Budget], [Spent], [Remaining], ([CategoryID] + @NextCatId) as [BudgetCategoryId]
--FROM [Budget].[dbo].[BudgetItemDatas]

-- Clear the temp tables
DROP TABLE #CategoryEnum