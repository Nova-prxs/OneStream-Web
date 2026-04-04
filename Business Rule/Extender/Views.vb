Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace OneStream.BusinessRule.Extender.Views
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
					
				Dim sViewDrop As String = String.Empty
				Dim sViewCreate As String = String.Empty
				Dim prm_SQL_FinalSales As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si,False, "prm_SQL_FinalSales")
				
				
				#Region "VIEW_Daily_Sales"
				
					sViewDrop = "
					IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'VIEW_Daily_Sales')
						DROP VIEW IF EXISTS VIEW_Daily_Sales"
					ExecuteSql(si, sViewDrop)
					
'					sViewCreate = $"
'						CREATE VIEW VIEW_Daily_Sales As
					
'						SELECT PivotTable.*, C.Description AS [Unit Description]
'						FROM (
'					           SELECT 
'							    D.brand AS Brand,
'					            D.scenario AS Scenario,
'							    D.unit_code AS [Unit Code],
'							    YEAR(D.[date]) AS [Year],
'							    'M' + RIGHT('0' + CAST(MONTH(D.date) AS VARCHAR), 2)+ '_FS' AS month,
'							    SUM(
'							        {prm_SQL_FinalSales} - ISNULL(month_sales_unit_adj,0)
					    
'							    )  AS [value]
							
'							FROM XFC_DailySales D
'							GROUP BY 
'							    D.brand,
'					            D.scenario,
'							    D.unit_code,
'					            D.unit_description,
'							    YEAR(D.[date]),
'							    MONTH(D.[date])
							
'                         UNION ALL
					  
'					         SELECT 
'							    D.brand AS Brand,
'					            D.scenario AS Scenario,
'							    D.unit_code AS [Unit Code],
'							    YEAR(D.[date]) AS [Year],
'							    'M' + RIGHT('0' + CAST(MONTH(D.date) AS VARCHAR), 2)+ '_AD' AS month,
'							    value AS [value]
							
'							FROM XFC_AUX_DailySales_MonthlyAdjustments D
		
'					UNION ALL
					  
'					         SELECT 
'							    D.brand AS Brand,
'					            D.scenario AS Scenario,
'							    D.unit_code AS [Unit Code],
'							    D.year AS [Year],
'							    'M' + RIGHT('0' + CAST(D.month AS VARCHAR), 2)+ '_WA' AS month,
'							    ISNULL(sales,0) + ISNULL(value,0) AS [value]
							
'							FROM (SELECT brand, scenario, unit_code, YEAR(date) AS [Year], MONTH(date)  AS month,
'									SUM({prm_SQL_FinalSales} - ISNULL(month_sales_unit_adj,0)) AS sales
'								  FROM XFC_DailySales
'								  GROUP BY brand, scenario, unit_code, YEAR(date), MONTH(date)) D
'							LEFT JOIN XFC_AUX_DailySales_MonthlyAdjustments A
'								ON D.brand = A.brand
'								AND D.scenario = A.scenario
'								AND D.unit_code = A.unit_code
'								AND Year = YEAR(A.[date])
'					            AND Month = MONTH(A.[date])
	
'						) AS SourceTable
'						PIVOT (
'						    SUM(value) 
'						    FOR month IN ([M01_FS], [M02_FS], [M03_FS], [M04_FS], [M05_FS], [M06_FS], [M07_FS], [M08_FS], [M09_FS], [M10_FS], [M11_FS], [M12_FS],
'					           [M01_AD], [M02_AD], [M03_AD], [M04_AD], [M05_AD], [M06_AD], [M07_AD], [M08_AD], [M09_AD], [M10_AD], [M11_AD], [M12_AD],
'					[M01_WA], [M02_WA], [M03_WA], [M04_WA], [M05_WA], [M06_WA], [M07_WA], [M08_WA], [M09_WA], [M10_WA], [M11_WA], [M12_WA])
'						) AS PivotTable
'						LEFT JOIN XFC_CEBES C
'					ON PivotTable.[Unit Code] = C.Cebe  ;
'					"
					
					sViewCreate = $"
						CREATE VIEW VIEW_Daily_Sales AS
					
						WITH original_sales AS (
							SELECT 
					            D.scenario,
							    D.unit_code,
							    YEAR(D.date) AS year,
							    MONTH(D.date) AS month,
							    SUM(
							        {prm_SQL_FinalSales} - ISNULL(month_sales_unit_adj,0)
							    )  AS sales
							FROM XFC_DailySales D
							GROUP BY 
					            D.scenario,
							    D.unit_code,
					            D.unit_description,
							    YEAR(D.date),
							    MONTH(D.date)
						), monthly_adjustments AS (
					         SELECT 
					            D.scenario,
							    D.unit_code,
							    YEAR(D.[date]) AS year,
							    MONTH(D.date) AS month,
							    value AS sales_adjustment
							FROM XFC_AUX_DailySales_MonthlyAdjustments D
						), final_sales AS (
					         SELECT 
					            os.scenario,
							    os.unit_code,
							    os.year,
							    os.month,
								os.sales AS original_sales,
								COALESCE(ma.sales_adjustment, 0) AS sales_adjustment,
							    COALESCE(os.sales, 0) + COALESCE(ma.sales_adjustment, 0) AS final_sales
							FROM original_sales os
							LEFT JOIN monthly_adjustments ma ON os.scenario = ma.scenario
								AND os.unit_code = ma.unit_code
								AND os.year = ma.year
								AND os.month = ma.month
						)
					
						SELECT pt.*, c.description AS [Unit Description], c.sales_brand AS [Brand]
						FROM (
							SELECT
								scenario AS Scenario,
								unit_code AS [Unit Code],
								year AS [Year],
								SUM(CASE WHEN month = 1 THEN original_sales END) AS M01_FS,
								SUM(CASE WHEN month = 2 THEN original_sales END) AS M02_FS,
								SUM(CASE WHEN month = 3 THEN original_sales END) AS M03_FS,
								SUM(CASE WHEN month = 4 THEN original_sales END) AS M04_FS,
								SUM(CASE WHEN month = 5 THEN original_sales END) AS M05_FS,
								SUM(CASE WHEN month = 6 THEN original_sales END) AS M06_FS,
								SUM(CASE WHEN month = 7 THEN original_sales END) AS M07_FS,
								SUM(CASE WHEN month = 8 THEN original_sales END) AS M08_FS,
								SUM(CASE WHEN month = 9 THEN original_sales END) AS M09_FS,
								SUM(CASE WHEN month = 10 THEN original_sales END) AS M10_FS,
								SUM(CASE WHEN month = 11 THEN original_sales END) AS M11_FS,
								SUM(CASE WHEN month = 12 THEN original_sales END) AS M12_FS,
								SUM(CASE WHEN month = 1 THEN sales_adjustment END) AS M01_AD,
								SUM(CASE WHEN month = 2 THEN sales_adjustment END) AS M02_AD,
								SUM(CASE WHEN month = 3 THEN sales_adjustment END) AS M03_AD,
								SUM(CASE WHEN month = 4 THEN sales_adjustment END) AS M04_AD,
								SUM(CASE WHEN month = 5 THEN sales_adjustment END) AS M05_AD,
								SUM(CASE WHEN month = 6 THEN sales_adjustment END) AS M06_AD,
								SUM(CASE WHEN month = 7 THEN sales_adjustment END) AS M07_AD,
								SUM(CASE WHEN month = 8 THEN sales_adjustment END) AS M08_AD,
								SUM(CASE WHEN month = 9 THEN sales_adjustment END) AS M09_AD,
								SUM(CASE WHEN month = 10 THEN sales_adjustment END) AS M10_AD,
								SUM(CASE WHEN month = 11 THEN sales_adjustment END) AS M11_AD,
								SUM(CASE WHEN month = 12 THEN sales_adjustment END) AS M12_AD,
								SUM(CASE WHEN month = 1 THEN final_sales END) AS M01_WA,
								SUM(CASE WHEN month = 2 THEN final_sales END) AS M02_WA,
								SUM(CASE WHEN month = 3 THEN final_sales END) AS M03_WA,
								SUM(CASE WHEN month = 4 THEN final_sales END) AS M04_WA,
								SUM(CASE WHEN month = 5 THEN final_sales END) AS M05_WA,
								SUM(CASE WHEN month = 6 THEN final_sales END) AS M06_WA,
								SUM(CASE WHEN month = 7 THEN final_sales END) AS M07_WA,
								SUM(CASE WHEN month = 8 THEN final_sales END) AS M08_WA,
								SUM(CASE WHEN month = 9 THEN final_sales END) AS M09_WA,
								SUM(CASE WHEN month = 10 THEN final_sales END) AS M10_WA,
								SUM(CASE WHEN month = 11 THEN final_sales END) AS M11_WA,
								SUM(CASE WHEN month = 12 THEN final_sales END) AS M12_WA
							FROM final_sales
							GROUP BY
								scenario,
								unit_code,
								year
						) AS pt
						LEFT JOIN XFC_CEBES c ON pt.[Unit Code] = c.cebe;
					"
				

					ExecuteSql(si, sViewCreate)



				
				#End Region
				

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

        Private Sub ExecuteSql(ByVal si As SessionInfo, ByVal sqlCmd As String)        
		
            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                               
                BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, True)
				
            End Using   
				
        End Sub						
		
	End Class
	
End Namespace