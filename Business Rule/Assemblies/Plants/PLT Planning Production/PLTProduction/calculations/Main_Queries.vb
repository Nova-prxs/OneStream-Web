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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.Main_Queries
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
		
			Try
				
				Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						'Dim names As New List(Of String)()
						'names.Add("MyDataSet")
						'Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet

	#Region "PRODUCTION"
						
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
							
							Dim dtFactory As DataTable = BRApi.Database.ExecuteSql(dbcon, sQuery,False)																
														
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

	#End Region
	
	#Region "COSTS"
						
						#Region "Cube Import Data"
						Else If args.DataSetName.XFEqualsIgnoreCase("DatosCostes") Then
							
							'Create Session Info for another app
							Dim oar As New OpenAppResult
							Dim osi As SessionInfo = BRApi.Security.Authorization.CreateSessionInfoForAnotherApp(si, "Production", oar)
							
							'Get table name and declare dt
							Dim dt As New DataTable
							
							If oar = OpenAppResult.Success Then
								
								Using mainDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(osi)
								
									'Get all the rows from external table
									Dim selectQuery As String = $"								
									WITH aux as (	
										SELECT 
											A.U3 as id_costcenter, 
											A.Sn as scenario,
											CASE 
												WHEN A.Tm LIKE '%.%' THEN
													CONVERT(DATE, CONCAT(LEFT(Tm, 4), '-', RIGHT('0' + CAST(CAST(PARSENAME(Tm, 1) AS INT) AS VARCHAR), 2), '-01'))
												WHEN A.Tm LIKE '%M%' THEN
													 CONVERT(DATE, CONCAT(LEFT(Tm, 4), '-', RIGHT('0' + REPLACE(Tm, LEFT(Tm, 5), ''), 2), '-01'))
												ELSE NULL
												END AS date,
											CASE
												WHEN A.Et LIKE '%R0611%' THEN 'R0045106'
												WHEN A.Et LIKE '%R1303%' THEN 'R0483003'
												WHEN A.Et LIKE '%R1302%' THEN 'R0529002'
												WHEN A.Et = 'R1301003' THEN 'R0548913'
												WHEN A.Et = 'R1301002' THEN 'R0548914'
												WHEN A.Et LIKE '%R0585%' THEN 'R0585'
												WHEN A.Et LIKE '%R0671%' THEN 'R0671'
												WHEN A.Et LIKE '%R0592%' THEN 'R0592'
											ELSE 'TBD'
											END as id_factory,
											A.Et,
											A.Ac as id_account,
											SUM(A.Am) as Am
									
										FROM StageSourceData A
									
										INNER JOIN WorkflowProfileHierarchy B
											ON A.Wfk = B.ProfileKey
																											
										WHERE 1=1
											AND A.Sn = 'Actual'
											AND B.ProfileName LIKE '%Import PL%'
									
										GROUP BY A.U3, A.Sn, A.Tm, A.Et, A.Ac
									)
									
									SELECT scenario, Et, id_factory, date, id_costcenter, id_account, SUM(Am) as amount
									
									FROM aux
									
									WHERE 1 = 1
										-- AND id_factory <> 'TBD'
										-- AND id_costcenter is null or id_costcenter = ''
									
									GROUP BY scenario, Et, id_costcenter, date, id_account, id_factory
									"
									dt = BRApi.Database.ExecuteSql(oDbConn, selectQuery, False)
								
								End Using
								End Using
						
							End If
						Return dt
						#End Region

						#Region "Monthly Data"
						Else If args.DataSetName.XFEqualsIgnoreCase("MonthlyReporting") Then
							
						#End Region
						
	#End Region						
					End If
				End Select
				
				End Using	

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
									SUM(activity_taj) as activity_taj 
	
								FROM XFC_PLT_FACT_Production
	
								WHERE 1=1
									AND id_factory = '{factory}'	
									AND scenario = 'Actual'
									AND uotype = 'HRS'
									AND YEAR(date) = {year}
									AND MONTH(date) = {month}
									AND DAY(date) <= {day}
	
								GROUP BY id_factory, id_costcenter, scenario
								), 
								
							reference as (
								SELECT 
									scenario,
									id_factory, 
									id_costcenter, 
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
									ON a.id_costcenter = b.id_costcenter
								
								INNER JOIN XFC_PLT_MASTER_CostCenter c
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
