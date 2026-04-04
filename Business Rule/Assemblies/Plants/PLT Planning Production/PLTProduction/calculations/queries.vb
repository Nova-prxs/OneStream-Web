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

' {Workspace.Current.PLTProduction.queries}{}{}
Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.queries

	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
		
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						'Dim names As New List(Of String)()
						'names.Add("MyDataSet")
						'Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet

#Region "Parameter Factory"
						
						If args.DataSetName.XFEqualsIgnoreCase("GetFactory") Then
							
							Dim ds As New DataSet							
							Dim dt As New DataTable("Factory")
							
							dt.Columns.Add("id", GetType(String))			
							dt.Columns.Add("description", GetType(String))																		

								
							'Query to get the event name and date
							Dim sQuery As String = $"SELECT id, description
													 FROM dbo.XFC_PLT_MASTER_Factory"
							
							'BRApi.ErrorLog.LogMessage(si, nameQuery)
							
							Dim dtFactory As DataTable
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dtFactory = BRApi.Database.ExecuteSql(dbConn, sQuery,False)	
							End Using																							
														
							For Each dr As DataRow In dtFactory.Rows									
								dt.Rows.Add(dr("id"),dr("description"))	
							Next
							
							ds.Tables.Add(dt)

							Return ds
								
#End Region									

#Region "Daily Data"
						
						#Region "Reporting"
							
						Else If args.DataSetName.XFEqualsIgnoreCase("PalmaDailyReport") Then
							
							' PARAMETROS
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim scenario As String = args.NameValuePairs("scenarioOS")
							Dim fecha As String = args.NameValuePairs("fechaOS")							
							Dim tsotaj As String = args.NameValuePairs("tajtsoOS")	
							
							' CONSULTA
							Dim dt As Datatable
							Dim sql2 As String = MainQueries(si,"Production",fecha, scenario, factory) & 
													$"														
													SELECT  
														a.Level4 as Atelier,
														a.Level3 as [group],
														a.id_costcenter as CostCenter, 
														SUM(a.actual_activity_taj) as Real, 
														SUM(a.reference_activity_taj) as Reference, 
														SUM(a.actual_activity_taj) - SUM(a.reference_activity_taj) as Difference,
														CASE 
														    WHEN SUM(a.reference_activity_taj) = 0 THEN 0 
														    ELSE (0.4 + 0.6 * (SUM(a.actual_activity_taj) / SUM(a.reference_activity_taj)))
														END AS IndiceAdjust,
														CASE 
														    WHEN SUM(a.reference_activity_taj) = 0 THEN 0 
														    ELSE (SUM(a.actual_activity_taj) / SUM(a.reference_activity_taj))
														END AS IndiceActivity
													
													FROM data a							
							
													GROUP BY a.Level4, a.id_costcenter, a.Level3
												"	
													
'							BRApi.ErrorLog.LogMessage(si, sql2)
													
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn,sql2.ToString(),False)
							End Using
							
							Return dt
							
						Else If args.DataSetName.XFEqualsIgnoreCase("ProductionBudget") Then
							' Lógica de consulta
							
						#End Region
						
						#Region "Graphs"
							
							#Region "Por Atelier"
							
						Else If args.DataSetName.XFEqualsIgnoreCase("PalmaDailyGraph") Then
							
							' PARAMETROS
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim scenario As String = args.NameValuePairs("scenarioOS")
							Dim fecha As String = args.NameValuePairs("fechaOS")		
							Dim tsotaj As String = args.NameValuePairs("tajtsoOS")
							
							' CONSULTA
							Dim dt As Datatable						
							Dim sql2 As String = MainQueries(si, "Production",fecha, scenario, factory) & $"														
													SELECT  
														a.Level4 as Atelier, 
														SUM(a.actual_activity_taj) as Real, 
														SUM(a.reference_activity_taj) as Reference, 
														SUM(a.actual_activity_taj) - SUM(a.reference_activity_taj) as Difference,
														CASE 
														    WHEN SUM(a.reference_activity_taj) = 0 THEN 0 
														    ELSE (0.4 + 0.6 * (SUM(a.actual_activity_taj) / SUM(a.reference_activity_taj)))
														END AS IndiceAdjust,
														CASE 
														    WHEN SUM(a.reference_activity_taj) = 0 THEN 0 
														    ELSE (SUM(a.actual_activity_taj) / SUM(a.reference_activity_taj))
														END AS IndiceActivity
													
													FROM data a							
							
													GROUP BY a.Level4
												"	
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn,sql2.ToString(),False)
							End Using

							' CREACIÓN GRÁFICO
							Dim oSeriesCollection As New XFSeriesCollection()
							oSeriesCollection.AddSeries(CreateSeries(si, "Real", XFChart2SeriesType.BarSideBySide,"SteelBlue",dt,"Real", "Atelier",,False,,,XFChart2ModelType.Bar2DBorderlessGradient, 0.8,"Atelier","Texto"))
							oSeriesCollection.AddSeries(CreateSeries(si, "Reference", XFChart2SeriesType.BarSideBySide,"LightBlue",dt, "Reference", "Atelier",,False,,,XFChart2ModelType.Bar2DBorderlessGradient, 0.8,"Atelier","Texto"))
							oSeriesCollection.AddSeries(CreateSeries(si, "Difference", XFChart2SeriesType.Line,"DarkGray",dt, "Difference", "Atelier",True,True,7,,,,"Atelier","Texto"))
							
							Return oSeriesCollection.CreateDataSet(si)
									
							#End Region
									
							#Region "Por CostCenter"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("PalmaDailyGraph2") Then
							' PARAMETROS
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim scenario As String = args.NameValuePairs("scenarioOS")
							Dim fecha As String = args.NameValuePairs("fechaOS")			
							Dim tsotaj As String = args.NameValuePairs("tajtsoOS")
							Dim atelier As String = args.NameValuePairs("atelierOS")
							
							' CONSULTA
							Dim dt As Datatable						
							Dim sql2 As String = MainQueries(si, "Production",fecha, scenario, factory) & $"														
													SELECT  
														a.id_costcenter as CostCenter,
														COALESCE(SUM(a.actual_activity_taj), 0) as Real, 
														COALESCE(SUM(a.reference_activity_taj), 0) as Reference, 
														COALESCE(SUM(a.actual_activity_taj) - SUM(a.reference_activity_taj),0) as Difference,
														CASE 
														    WHEN SUM(a.reference_activity_taj) = 0 THEN 0 
														    ELSE (0.4 + 0.6 * (SUM(a.actual_activity_taj) / SUM(a.reference_activity_taj)))
														END AS IndiceAdjust,
														CASE 
														    WHEN SUM(a.reference_activity_taj) = 0 THEN 0 
														    ELSE (SUM(a.actual_activity_taj) / SUM(a.reference_activity_taj))
														END AS IndiceActivity
													
													FROM data a	
							
													WHERE 1=1
														AND Level4 = '{atelier}'
							
													GROUP BY a.id_costcenter
												"	
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn,sql2.ToString(),False)
							End Using

							' CREACIÓN DEL GRÁFICO
							Dim oSeriesCollection As New XFSeriesCollection()
							oSeriesCollection.AddSeries(CreateSeries(si, "Real", XFChart2SeriesType.BarSideBySide,"SteelBlue",dt,"Real", "CostCenter",,False,,,XFChart2ModelType.Bar2DBorderlessGradient, 0.8,,))
							oSeriesCollection.AddSeries(CreateSeries(si, "Reference", XFChart2SeriesType.BarSideBySide,"LightBlue",dt, "Reference", "CostCenter",,False,,,XFChart2ModelType.Bar2DBorderlessGradient, 0.8,,))
							oSeriesCollection.AddSeries(CreateSeries(si, "Difference", XFChart2SeriesType.Line,"DarkGray",dt, "Difference", "CostCenter",True,True,7,,,,,))
							
							Return oSeriesCollection.CreateDataSet(si)
							
							#End Region
								
							#Region "Por Grupo"
								
						Else If args.DataSetName.XFEqualsIgnoreCase("PalmaDailyGraph3") Then
							
							' PARAMETROS
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim scenario As String = args.NameValuePairs("scenarioOS")
							Dim fecha As String = args.NameValuePairs("fechaOS")			
							Dim tsotaj As String = args.NameValuePairs("tajtsoOS")
							
							' CONSULTA
							Dim dt As Datatable							
							Dim sql2 As String = MainQueries(si, "Production",fecha, scenario, factory) &$"														
													SELECT  
														a.Level3 as Grupo, 
														SUM(a.actual_activity_taj) as Real, 
														SUM(a.reference_activity_taj) as Reference, 
														SUM(a.actual_activity_taj) - SUM(a.reference_activity_taj) as Difference,
														CASE 
														    WHEN SUM(a.reference_activity_taj) = 0 THEN 0 
														    ELSE (0.4 + 0.6 * (SUM(a.actual_activity_taj) / SUM(a.reference_activity_taj)))
														END AS IndiceAdjust,
														CASE 
														    WHEN SUM(a.reference_activity_taj) = 0 THEN 0 
														    ELSE (SUM(a.actual_activity_taj) / SUM(a.reference_activity_taj))
														END AS IndiceActivity
													
													FROM data a							
							
													GROUP BY a.Level3
								
													"
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn,sql2.ToString(),False)
							End Using

							' CREACIÓN DEL GRÁFICO
							Dim oSeriesCollection As New XFSeriesCollection()
'							oSeriesCollection.AddSeries(CreateSeries(si, "Real", XFChart2SeriesType.BarSideBySide,"SteelBlue",dt,"Real", "Grupo",,False,,,XFChart2ModelType.Bar2DBorderlessGradient, 0.8,,))
'							oSeriesCollection.AddSeries(CreateSeries(si, "Reference", XFChart2SeriesType.BarSideBySide,"DarkGray",dt, "Reference", "Grupo",,False,,,XFChart2ModelType.Bar2DBorderlessGradient, 0.8,,))
							oSeriesCollection.AddSeries(CreateSeries(si, "IndiceActivity", XFChart2SeriesType.Line,"DarkGray",dt, "IndiceActivity", "Grupo",False,True,7,,,,,))
							
							Return oSeriesCollection.CreateDataSet(si)
							
							#End Region								
								
						#End Region
							
						#Region "Pivot Grid"
							
						Else If args.DataSetName.XFEqualsIgnoreCase("PalmaPivotGrid") Then							
							
							' PARAMETROS
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim scenario As String = args.NameValuePairs("scenarioOS")
							Dim fecha As String = args.NameValuePairs("fechaOS")			
							
							' CONSULTA
							Dim dt As Datatable							
							Dim sql2 As String = MainQueries(si, "Production",fecha, scenario, factory) &$"														
													SELECT *
							
													FROM data a								
													"
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn,sql2.ToString(),False)
							End Using

							Return dt
							
						Else If args.DataSetName.XFEqualsIgnoreCase("PalmaPivotGrid_2") Then							
							
							' PARAMETROS
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim scenario As String = args.NameValuePairs("scenarioOS")
							Dim fecha As String = args.NameValuePairs("fechaOS")			
							
							' CONSULTA
							Dim dt As Datatable							
							Dim sql2 As String = $"														
													SELECT *
							
													FROM XFC_PLT_FACT_Production a								
													"
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn,sql2.ToString(),False)
							End Using

							Return dt							
							
						#End Region
							
#End Region

#Region "Parameter Dias Habiles"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("DiasHabiles") Then
							' PARAMETROS
							Dim fecha As String = args.NameValuePairs("fechaOS")
							Dim year As String = String.Empty
							Dim month As String = String.Empty
							Dim day As String = String.Empty
							If Len(fecha) = 8 Then
								year = Left(fecha,4)
								month = fecha.Substring(4,2)
								day = Right(fecha,2)
							Else
								year = DateTime.Now.Year
								month = DateTime.Now.Month
								day = DateTime.Now.Day
								args.CustomSubstVars.XFSetValue("PLT_prm_date", year & month & day)
							End If
							
							' CONSULTA
							Dim dt As Datatable
							Dim sql As String = $"
													-- Declarar variables para el año y la fecha actual
													DECLARE @Hoy DATE = CONVERT(date, '{fecha}', 112);
													DECLARE @MesActual INT = {month};
													DECLARE @AnioActual INT = {year};
													
													-- CTE para generar fechas
													WITH GenerarFechas AS (
													    SELECT CAST(DATEFROMPARTS(@AnioActual, 1, 1) AS DATE) AS Fecha
													    UNION ALL
													    SELECT DATEADD(DAY, 1, Fecha)
													    FROM GenerarFechas
													    WHERE Fecha < DATEFROMPARTS(@AnioActual, 12, 31)
													),
													
													DiasHabiles AS (
													    SELECT Fecha, 
													           MONTH(Fecha) AS Mes, 
													           YEAR(Fecha) AS Anio
													    FROM GenerarFechas
													    WHERE DATEPART(WEEKDAY, Fecha) NOT IN (1, 7) -- 1: Sunday, 7: Saturday
													),
													
													-- Consultar los días hábiles del mes actual hasta hoy
													SoloDiasHabiles AS (
													SELECT Fecha, Mes, Anio
													FROM DiasHabiles
													WHERE (Mes < @MesActual OR (Mes = @MesActual AND Fecha <= @Hoy))
													),
													
													DHActual as (
													SELECT Anio, Mes, COUNT(Mes) as DiasHabiles
													FROM SoloDiasHabiles
													GROUP BY Mes, Anio
													),
													
													DHMesAct as (
													SELECT Anio, Mes, COUNT(Mes) as DiasHabilesTot
													FROM DiasHabiles
													WHERE (Mes <= @MesActual)
													GROUP BY Anio, Mes
													),
													
													DHFinalTable as (
													SELECT A.Anio, A.Mes, A.DiasHabiles, M.DiasHabilesTot
													FROM DHActual as A
													LEFT JOIN DHMesAct as M ON A.Mes = M.Mes AND A.Anio = M.Anio
													)
													
													SELECT DiasHabiles, DiasHabilesTot
													FROM DHFinalTable
													WHERE Mes = {month}
																				
													OPTION (MAXRECURSION 366);
												"
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn,sql.ToString(),False)
							End Using
							
							Return dt		
							
#End Region	

#Region "Allocations"

						Else If args.DataSetName.XFEqualsIgnoreCase("Reporting_Allocations") Then
							
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sYear As String = args.NameValuePairs("year")
							Dim sFactory As String = args.NameValuePairs("factory")
							Dim sTime As String = args.NameValuePairs("time")
							
							Dim dt As DataTable							
							
							Dim sql As String = String.Empty
							
							sql = $"							
							
									SELECT
							
										CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), MONTH(A.date))), 2)) AS Month
										, A.date AS Date
										, A.id_averagegroup AS [Id GM]
										, A.id_averagegroup + ' - ' + ISNULL(G.description,'') AS GM
										, A.id_product AS [Id Product]
										, CASE WHEN A.id_product = '-1' THEN 'No Product'
											   ELSE A.id_product + ' - ' + ISNULL(P.description,'') END AS Product				
										, A.id_costcenter AS [Id Cost Center]
										, A.uotype AS Time	
										, A.volume AS Volume
										, A.allocation_taj AS Allocation
							
									FROM XFC_PLT_FACT_Production A
							
							
									LEFT JOIN XFC_PLT_MASTER_Product P
										ON A.id_product = P.id
							
									LEFT JOIN XFC_PLT_MASTER_AverageGroup G
										ON A.id_averagegroup = G.id									
							
									WHERE A.scenario = '{sScenario}'
									AND YEAR(A.date) = {sYear}						
									AND A.id_factory = '{sFactory}'	
									AND A.activity_taj <> 0 
									AND (A.uotype = '{sTime}' OR '{sTime}' = 'All')
							
									ORDER BY A.id_product, A.id_averagegroup
																	
							"
							
'							BRApi.ErrorLog.LogMessage(si, sql)
													
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using																												
							
							dt.TableName = "Production"
							
							Return dt	

#End Region
					
#Region "Activity"

						Else If args.DataSetName.XFEqualsIgnoreCase("Reporting_Activity") Then
							
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sScenarioRef As String = args.NameValuePairs("scenario_ref")
							Dim sYear As String = args.NameValuePairs("year")
							Dim sFactory As String = args.NameValuePairs("factory")
							Dim sTime As String = args.NameValuePairs("time")
							
							Dim dt As DataTable							
							
							Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							Dim sql_Activity = uti_sharedqueries.AllQueries(si, "ActivityALL", sFactory, "", sYear, sScenario, sScenarioRef, sTime)	
							
							Dim sql As String = String.Empty
							
							sql = sql_Activity & $"
							
									SELECT 
										[Id Factory]
										,[Id Cost Center]
										,[Id GM]
										,[Id GM] + ' - ' + ISNULL(G.description,'') AS GM
										,[Id Product]
										,CASE WHEN [Id Product] = '-1' THEN 'No Product'
											  ELSE [Id Product] + ' - ' + ISNULL(P.description,'') END AS Product
										,Time
										,Date
										,CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), MONTH(A.date))), 2)) AS Month
										,CONVERT(DECIMAL(18,2),SUM([Volume])) AS [Volume]
										,CONVERT(DECIMAL(18,2),SUM([Activity TAJ])) AS [Activity TAJ]
										,CONVERT(DECIMAL(18,2),SUM([Activity TSO])) AS [Activity TSO]
										,CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) AS [Activity TAJ Ref]
									
									FROM ActivityAll A
							
									LEFT JOIN XFC_PLT_MASTER_Product P
										ON A.[Id Product] = P.id
							
									LEFT JOIN XFC_PLT_MASTER_AverageGroup G
										ON A.[Id GM] = G.id							
							
									GROUP BY [Id Factory],[Id Cost Center],[Id GM],G.description,[Id Product],P.description,Time,Date
									ORDER BY Date
																	
							"
							
'							BRApi.ErrorLog.LogMessage(si, "Activity: " & sql)
													
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using																												
							
							dt.TableName = "Production"
							
							Return dt	

#End Region

#Region "Activity Detailed"

						Else If args.DataSetName.XFEqualsIgnoreCase("Reporting_Activity_Detailed") Then
							
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sScenarioRef As String = args.NameValuePairs("scenario_ref")
							Dim sYear As String = args.NameValuePairs("year")
							Dim sFactory As String = args.NameValuePairs("factory")
							Dim sTime As String = args.NameValuePairs("time")
							
							Dim sYearRef As String = If (sScenarioRef = "Actual_1",(CInt(sYear)-1).ToString,sYear)
							sYearRef = If (sScenario.StartsWith("Budget"),(CInt(sYear)-1).ToString,sYear)							
							
							Dim dt As DataTable							
							
							Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							Dim sql_Activity = uti_sharedqueries.AllQueries(si, "ActivityALL", sFactory, "", sYear, sScenario, sScenarioRef, sTime)	
							
							Dim sql As String = String.Empty
							
							sql = sql_Activity & $"
							
									, ActivityAllDetailed AS (
							
									SELECT 
										[Id Cost Center]
										,[Id GM]
										,[Id GM] + ' - ' + ISNULL(G.description,'') AS GM
										,[Id Product]
										,CASE WHEN [Id Product] = '-1' THEN 'No Product'
											  ELSE [Id Product] + ' - ' + ISNULL(P.description,'') END AS Product
										,Time
										,MONTH(A.date) AS Month
										,'M' + RIGHT('0' + CAST(MONTH(A.date) AS VARCHAR), 2) AS MonthFormat
							
										,SUM([Volume]) AS [Volume]
										,SUM([Activity TAJ]) AS [Act. TAJ]
										,SUM([Activity TSO]) AS [Act. TSO]
										,SUM([Volume Ref]) AS [Volume Ref]						
										,SUM([Activity TAJ Ref]) AS [Act. TAJ Ref]
									
									FROM ActivityAll A
							
									LEFT JOIN XFC_PLT_MASTER_Product P
										ON A.[Id Product] = P.id
							
									LEFT JOIN XFC_PLT_MASTER_AverageGroup G
										ON A.[Id GM] = G.id															
							
									GROUP BY [Id Cost Center],[Id GM],G.description,[Id Product],P.description,Time,MONTH(A.date),'M' + RIGHT('0' + CAST(MONTH(A.date) AS VARCHAR), 2)
									
									)
							
									SELECT [Id Cost Center], GM, Product, Time, A.MonthFormat AS Month
										,[Volume] AS [Volume]
										,[Act. TAJ] / NULLIF([Volume],0) AS [All. TAJ]
										,[Act. TAJ] AS [Act. TAJ]
										,[Act. TSO] / NULLIF([Volume],0) AS [All. TSO]
										,[Act. TSO] AS [Act. TSO]
										,[Volume Ref] AS [Volume Ref]
										,[Value] AS [All. TAJ Ref]
										,[Act. TAJ Ref] AS [Act. TAJ Ref]						
									
									FROM ActivityAllDetailed A
							
									LEFT JOIN (	
											SELECT DISTINCT
												MONTH(date) AS month
												, id_averagegroup
												, id_product
												, uotype
												, value
											FROM XFC_PLT_AUX_Production_Planning_Times						
											WHERE scenario = '{sScenarioRef}'
											AND YEAR(date) = {sYearRef}						
											AND id_factory = '{sFactory}'	
											AND value <> 0 ) B
									
										ON A.month = B.month
										AND A.[Id GM] = B.id_averagegroup
										AND A.[Id Product] = B.id_product
										AND A.Time = B.uotype							
							
									ORDER BY MonthFormat, [Id Cost Center], GM, Product, Time
																	
							"
							
'							BRApi.ErrorLog.LogMessage(si, "Activity: " & sql)
													
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using																												
							
							dt.TableName = "Production"
							
							Return dt	

#End Region

#Region "Coefficient TSO & Activity Index"

						Else If args.DataSetName.XFEqualsIgnoreCase("Reporting_Activity_Index") Then
							
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sScenarioRef As String = args.NameValuePairs("scenario_ref")
							Dim sYear As String = args.NameValuePairs("year")
							Dim sFactory As String = args.NameValuePairs("factory")
							Dim sTime As String = args.NameValuePairs("time")
							
							Dim dt As DataTable
							
							Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							Dim sql_Activity = uti_sharedqueries.AllQueries(si, "ActivityALL", sFactory, "", sYear, sScenario, sScenarioRef, sTime)	
							
							Dim sql As String = String.Empty
							
							sql = sql_Activity & $"												
							
									SELECT A.*
										, X.value AS [Act Index Correct]
										, X.comment AS [Comment]
							
									FROM (
							
										SELECT											
											[Id Factory]
											,[Id GM]
											,[Id GM] + ' - ' + ISNULL(G.description,'') AS GM
											,Time
											,MONTH(A.date) AS [Id Month]
											,CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), MONTH(A.date))), 2)) AS Month							
											,CONVERT(DECIMAL(18,2),SUM([Activity TAJ])) AS [Activity TAJ]
											,CASE
												WHEN CONVERT(DECIMAL(18,2),SUM([Activity TAJ])) <> 0 THEN CONVERT(DECIMAL(18,2),SUM([Activity TSO])) / CONVERT(DECIMAL(18,2),SUM([Activity TAJ])) * 100 
											END AS [Coefficient TSO]	
											,CONVERT(DECIMAL(18,2),SUM([Activity TSO])) AS [Activity TSO]										
											,CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) AS [Activity TAJ Ref]
											,CASE
												WHEN CONVERT(DECIMAL(18,2),ISNULL(SUM([Activity TAJ Ref]),0)) = 0 AND CONVERT(DECIMAL(18,2),SUM([Activity TSO])) <> 0 THEN 100
												WHEN CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) <> 0 THEN CONVERT(DECIMAL(18,2),SUM([Activity TSO])) / CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) * 100 
											END AS [Activity Index]	
											, 2 AS Orden
								
											,CONVERT(VARCHAR,MONTH(A.date)) + ':' + [Id GM] + ':' + Time AS row_selected
										
										FROM ActivityAll A		
								
										LEFT JOIN XFC_PLT_MASTER_AverageGroup G
											ON A.[Id GM] = G.id															
					
										GROUP BY [Id Factory],[Id GM], G.description, Time, MONTH(A.date)
							
										UNION ALL
							
										SELECT											
											[Id Factory]
											,'Global' AS [Id GM]
											,'Global' AS GM
											,Time
											,MONTH(A.date) AS [Id Month]
											,CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), MONTH(A.date))), 2)) AS Month							
											,CONVERT(DECIMAL(18,2),SUM([Activity TAJ])) AS [Activity TAJ]
											,CASE
												WHEN CONVERT(DECIMAL(18,2),SUM([Activity TAJ])) <> 0 THEN CONVERT(DECIMAL(18,2),SUM([Activity TSO])) / CONVERT(DECIMAL(18,2),SUM([Activity TAJ])) * 100 
											END AS [Coefficient TSO]	
											,CONVERT(DECIMAL(18,2),SUM([Activity TSO])) AS [Activity TSO]										
											,CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) AS [Activity TAJ Ref]
											,CASE
												WHEN CONVERT(DECIMAL(18,2),ISNULL(SUM([Activity TAJ Ref]),0)) = 0 AND CONVERT(DECIMAL(18,2),SUM([Activity TSO])) <> 0 THEN 100
												WHEN CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) <> 0 THEN CONVERT(DECIMAL(18,2),SUM([Activity TSO])) / CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) * 100 
											END AS [Activity Index]	
											, 1 AS Orden
								
											,CONVERT(VARCHAR,MONTH(A.date)) + ':Global:' + Time AS row_selected
										
										FROM ActivityAll A
							
										GROUP BY [Id Factory], Time, MONTH(A.date)
									
									) A 
							
									LEFT JOIN XFC_PLT_AUX_ActivityIndex_Adjust X
										ON A.[Id GM] = X.id_averagegroup
										AND A.[Id Month] = MONTH(X.date)
										AND A.[Id Factory] = X.id_factory
										AND A.Time = X.uotype
										AND X.scenario = '{sScenario}'
										AND X.scenario_ref = '{sScenarioRef}'
										AND YEAR(X.date) = {sYear}
							
									ORDER BY [Id Factory], Month, [Id GM], GM, Time
																	
							"
							
'							BRApi.ErrorLog.LogMessage(si, sql)
													
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using															
							
							Return dt	

#End Region

#Region "Volume Resume"

						Else If args.DataSetName.XFEqualsIgnoreCase("Volume_Resume") Then
							
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sYear As String = args.NameValuePairs("year")
							Dim sFactory As String = args.NameValuePairs("factory")
							
							Dim dt As DataTable
							
							Dim sql As String =  $"																			

						    SELECT 
						        F.id_costcenter AS [Cost Center], 
								F.id_averagegroup AS [Id GM], 
								F.id_averagegroup + ' - ' + ISNULL(A.description,'No Description') AS GM,
								F.id_product AS [Id Product],
								F.id_product + ' - ' + ISNULL(P.description,'No Description') AS Product,
						        'M' + RIGHT('0' + CAST(MONTH(F.date) AS VARCHAR), 2) AS Month, 
						        F.value AS Volume
					
						    FROM (	SELECT date, id_product, id_costcenter, id_averagegroup, value
									FROM XFC_PLT_AUX_Production_Planning_Volumes
									WHERE scenario = '{sScenario}'
									AND YEAR(date) = {sYear}
									AND id_factory = '{sFactory}'		
							
									UNION ALL
							
									SELECT date, id_product, id_costcenter, id_averagegroup, value 
									FROM XFC_PLT_AUX_Production_Planning_Volumes_Dist
									WHERE scenario = '{sScenario}'
									AND YEAR(date) = {sYear}
									AND id_factory = '{sFactory}'								
								) F			
					
							LEFT JOIN XFC_PLT_MASTER_Product P
								ON F.id_product = P.id
					
							LEFT JOIN XFC_PLT_MASTER_AverageGroup A
								ON F.id_averagegroup = A.id
								AND A.id_factory = '{sFactory}'
							
							"
							
'							BRApi.ErrorLog.LogMessage(si, sql)
													
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using															
							
							Return dt	

#End Region

#Region "Testing"
						Else If args.DataSetName.XFEqualsIgnoreCase("Testing") Then
						    ' 1) Crear el DataSet
						    Dim dsFinal As New DataSet("dsFinal")
						
						    ' =====================================================================
						    ' 2) TABLA HIERARCHY (Padre en la relación)
						    '    - Indica que A -> C1, C2  y  B -> C3, C4
						    ' =====================================================================
						    Dim dtHierarchy As New DataTable("Hierarchy")
						    dtHierarchy.Columns.Add("ParentID", GetType(String)) ' ej: A, B
						    dtHierarchy.Columns.Add("ChildID", GetType(String))  ' ej: C1, C2, C3, C4
						
						    ' Definimos la clave primaria compuesta, 
						    ' no imprescindible en este ejemplo simple, pero recomendable.
						    dtHierarchy.PrimaryKey = New DataColumn() {
						        dtHierarchy.Columns("ParentID"),
						        dtHierarchy.Columns("ChildID")
						    }
						
						    ' Agregamos filas: A -> C1, A -> C2, B -> C3, B -> C4
						    dtHierarchy.Rows.Add("A", "C1")
						    dtHierarchy.Rows.Add("A", "C2")
						    dtHierarchy.Rows.Add("B", "C3")
						    dtHierarchy.Rows.Add("B", "C4")
						
						    ' =====================================================================
						    ' 3) TABLA CONTENT (Hijo en la relación)
						    '    - Datos asociados a cada Child (C1, C2, C3, C4)
						    ' =====================================================================
						    Dim dtContent As New DataTable("Content")
						    dtContent.Columns.Add("Code", GetType(String))  ' Ej: C1, C2, C3, C4
						    dtContent.Columns.Add("Value", GetType(Decimal))
						
						    ' Definimos la clave primaria en "Content"
						    dtContent.PrimaryKey = New DataColumn() {dtContent.Columns("Code")}
						
						    ' Filas de ejemplo con valores
						    dtContent.Rows.Add("C1", 100D)
						    dtContent.Rows.Add("C2", 200D)
						    dtContent.Rows.Add("C3", 300D)
						    dtContent.Rows.Add("C4", 400D)
						
						    ' =====================================================================
						    ' 4) Agregar tablas al DataSet
						    ' =====================================================================
						    dsFinal.Tables.Add(dtHierarchy)
						    dsFinal.Tables.Add(dtContent)
						
						    ' =====================================================================
						    ' 5) Crear la relación "Rel_Hier_Content"
						    '    - dtHierarchy es el "padre"
						    '    - dtContent es el "hijo"
						    '    - Vinculamos dtHierarchy.ChildID con dtContent.Code
						    ' =====================================================================
						    dsFinal.Relations.Add("Rel_Hier_Content",
						                          dtHierarchy.Columns("ChildID"),  ' Parent Column
						                          dtContent.Columns("Code"))       ' Child Column
						
						    ' Listo. Ya tienes un DataSet con dos tablas y una relación padre-hijo.
							Dim dtGrid As New XFGridLayoutDefinition
							
						    Return dsFinal
#End Region


						End If
				End Select

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
	
#Region "Functions"

	#Region "Graph Function"

		Private Function CreateSeries(ByVal si As SessionInfo, _
									name As String, _
									type As XFChart2SeriesType, _ 
									color As String, _
									seriesData As DataTable, _
		                            valueField As String, _
									argumentField As String, _
									Optional useSecondaryAxis As Boolean = False, _
		                            Optional showMarkers As Boolean = False, _
									Optional markerSize As Integer = 0, _
		                            Optional lineThickness As Integer = 0, _
									Optional modelType As XFChart2ModelType = Nothing, _
		                            Optional barWidth As Double = 0,
									Optional parameterField As String = "NoIndicado", _
									Optional parameterType As String = "NoIndicado") As XFSeries
			Try
				    Dim series As New XFSeries()
    
				    series.UniqueName = name
				    series.Type = type
				    series.Color = color
				    series.UseSecondaryAxis = useSecondaryAxis
					
				    If type = XFChart2SeriesType.Line Then
				        series.SeriesAnimationType = XFChart2SeriesAnimationType.Line2DUnwrapHorizontally
				        If showMarkers Then
				            series.ShowMarkers = 1
				            series.MarkerSize = markerSize
				        End If
				        If lineThickness > 0 Then
				            series.LineThickness = lineThickness
				        End If
				    ElseIf type = XFChart2SeriesType.BarSideBySide Then
				        series.ModelType = modelType
				        series.BarWidth = barWidth
				    End If
				
				    For Each row As DataRow In seriesData.Rows
				        Dim point As New XFSeriesPoint()
				        point.Argument = row(argumentField).ToString()
				        point.Value = Convert.ToDouble(row(valueField))
						
						If type = XFChart2SeriesType.Line Then
							If point.Value >= 0 Then
								point.Color = "Green"
							Else
								point.Color = "Red"
							End If		
						End If
						
						If parameterType = "Texto" Then
							point.ParameterValue = "" & row(parameterField) & ""
						Else If parameterType = "Texto2"
							point.ParameterValue = "'" & row(parameterField) & "'"
						Else If parameterType = "Numero" Then
							point.ParameterValue = row(parameterField)
						Else
							point.ParameterValue = parameterField
						End If
						
						point.Value2 = 100
						series.AddPoint(point)
				    Next
														
				    Return series
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

	#End Region

	#Region "Dias Habiles Function"

	Private Function ObtenerDiasHabiles(ByVal si As SessionInfo, ByVal fecha As String, ByVal month As String, ByVal year As String) As DataTable
		Dim dt As Datatable
		Dim sql As String = $"
								-- Declarar variables para el año y la fecha actual
								DECLARE @Hoy DATE = CONVERT(date, '{fecha}', 112);
								DECLARE @MesActual INT = {month};
								DECLARE @AnioActual INT = {year};
								
								-- CTE para generar fechas
								WITH GenerarFechas AS (
								    SELECT CAST(DATEFROMPARTS(@AnioActual, 1, 1) AS DATE) AS Fecha
								    UNION ALL
								    SELECT DATEADD(DAY, 1, Fecha)
								    FROM GenerarFechas
								    WHERE Fecha < DATEFROMPARTS(@AnioActual, 12, 31)
								),
								
								DiasHabiles AS (
								    SELECT Fecha, 
								           MONTH(Fecha) AS Mes, 
								           YEAR(Fecha) AS Anio
								    FROM GenerarFechas
								    WHERE DATEPART(WEEKDAY, Fecha) NOT IN (1, 7) -- 1: Sunday, 7: Saturday
								),
								
								-- Consultar los días hábiles del mes actual hasta hoy
								SoloDiasHabiles AS (
								SELECT Fecha, Mes, Anio
								FROM DiasHabiles
								WHERE (Mes < @MesActual OR (Mes = @MesActual AND Fecha <= @Hoy))
								),
								
								DHActual as (
								SELECT Anio, Mes, COUNT(Mes) as DiasHabiles
								FROM SoloDiasHabiles
								GROUP BY Mes, Anio
								),
								
								DHMesAct as (
								SELECT Anio, Mes, COUNT(Mes) as DiasHabilesTot
								FROM DiasHabiles
								WHERE (Mes <= @MesActual)
								GROUP BY Anio, Mes
								),
								
								DHFinalTable as (
								SELECT A.Anio, A.Mes, A.DiasHabiles, M.DiasHabilesTot
								FROM DHActual as A
								LEFT JOIN DHMesAct as M ON A.Mes = M.Mes AND A.Anio = M.Anio
								)
								
								SELECT DiasHabiles, DiasHabilesTot
								FROM DHFinalTable
								WHERE Mes = {month}
															
								OPTION (MAXRECURSION 366);
							"
		Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			dt = BRApi.Database.ExecuteSql(dbConn,sql.ToString(),False)
		End Using
		
		Return dt						
	End Function

	#End Region

	#Region "Main Quieries"

	Private Function MainQueries(ByVal si As SessionInfo, _
								Optional query As String = "", _ 
								Optional fecha As String = "", _ 
								Optional scenario As String = "", _ 
								Optional factory As String = ""
								) As String
			' RETURN					
			Dim sqlData As String = String.Empty
			
			' VARIABLES COMUNES					
			Dim year As String = String.Empty
			Dim month As String = String.Empty
			Dim day As String = String.Empty
			If Len(fecha) = 8 Then
				year = Left(fecha,4)
				month = fecha.Substring(4,2)
				day = Right(fecha,2)
			Else
				year = DateTime.Now.Year
				month = DateTime.Now.Month
				day = DateTime.Now.Day
			End If							
																
			Dim diasHabilesTable As DataTable = ObtenerDiasHabiles(si, fecha, month, year)
			Dim diasHabilesAct As String = diasHabilesTable.Rows(0)("DiasHabiles")
			Dim diasHabilesTot As String = diasHabilesTable.Rows(0)("DiasHabilesTot")
			
			#Region "PALMA Main Query"
			If query = "Production" Then
				sqlData = $"
							WITH actual as (
								SELECT  
									scenario,
									id_factory, 
									id_costcenter, 
									id_averagegroup,
									SUM(activity_taj) as activity_taj 
	
								FROM XFC_PLT_FACT_Production
	
								WHERE 1=1
									AND id_factory = '{factory}'	
									AND scenario = 'Actual'
									AND uotype = 'UO1'
									AND YEAR(date) = {year}
									AND MONTH(date) = {month}
									AND DAY(date) <= {day}
	
								GROUP BY id_factory, id_costcenter, id_averagegroup, scenario
								), 
								
							reference as (
								SELECT 
									scenario,
									id_factory, 
									id_costcenter, 
									id_averagegroup,
									(activity_taj*{diasHabilesAct})/{diasHabilesTot} as activity_taj 
	
								FROM XFC_PLT_FACT_Production
	
								WHERE 1=1
									AND id_factory = '{factory}'	
									AND scenario = '{scenario}'
									AND YEAR(date) = {year}
									AND MONTH(date) = {month}
								), 
	
							data as (
								SELECT 
									a.scenario,
									a.id_factory, 
									a.id_costcenter, 
									h1.id AS Level4,
									h2.id AS Level3,
									h3.id AS Level2,
									h4.id AS Level1,
									a.activity_taj AS actual_activity_taj,
									b.activity_taj AS reference_activity_taj
	
								FROM actual a
	
								LEFT JOIN reference b
									ON a.id_averagegroup = b.id_averagegroup
									--Pendiente de definir si hay detalle de cost center
									--AND a.id_costcenter = b.id_costcenter
								
								LEFT JOIN XFC_PLT_MASTER_CostCenter c
									ON c.id = a.id_costcenter
	
								LEFT JOIN XFC_PLT_HIER_CostCenter h1
									ON c.parent_id = h1.id	
									AND h1.level = 4
				
								LEFT JOIN XFC_PLT_HIER_CostCenter h2
									ON (c.parent_id = h2.id	AND h2.level = 3)				
									OR h1.parent_id = h2.id
				
								LEFT JOIN XFC_PLT_HIER_CostCenter h3
									ON (c.parent_id = h3.id	AND h3.level = 2)				
									OR h2.parent_id = h3.id
				
								LEFT JOIN XFC_PLT_HIER_CostCenter h4
									ON (c.parent_id = h4.id	AND h4.level = 1)				
									OR h3.parent_id = h4.id
				
								WHERE 1=1
												
									) "
			#End Region
			
			#Region "Nueva Consulta"
			Else If query = "NuevaConsulta" Then
				'Añadir nuevas consultas
			#End Region
			
			End If

		
		Return sqlData						
	End Function

	#End Region	
	
#End Region		
		
	End Class
	
End Namespace
