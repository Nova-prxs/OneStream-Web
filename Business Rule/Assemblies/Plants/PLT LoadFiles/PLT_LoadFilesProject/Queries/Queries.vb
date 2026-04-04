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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.Queries
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						'Dim names As New List(Of String)()
						'names.Add("MyDataSet")
						'Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet
						
#Region "Raw Data"		
	
					If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_Common") Then
						
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim year As String = args.NameValuePairs("yearOS")
							Dim month As String = args.NameValuePairs("monthOS")
							Dim scenario As String = args.NameValuePairs("scenarioOS")
							Dim table As String = args.NameValuePairs("tableOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM {table}
							"
							If table.Contains("MASTER_Effects") Or  table.Contains("MASTER_NatureCost") Or table.Contains("MAPPING_Accounts") Or table.Contains("MASTER_Product")Then
								' Def.
							Else If table.Contains("MASTER") Or table.Contains("HIER") Then
								sql = sql &
									$"
									WHERE 1=1
										AND id_factory = '{factory}'
								"								
							Else
								sql = sql & 
									$"
									WHERE 1=1
										AND id_factory = '{factory}'								
										AND YEAR(date) = '{year}'
										AND scenario = '{scenario}'
								"
							End If
							
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt	
							
						#Region "PRUDCTION"
					Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_FACT_Production") Then
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim year As String = args.NameValuePairs("yearOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_FACT_Production
							
								WHERE 1=1
									AND id_factory = '{factory}'
									AND YEAR(date) = '{year}'
									AND scenario = 'Actual'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt
							
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_FACT_Production_Budget") Then
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim year As String = args.NameValuePairs("yearOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_FACT_Production
							
								WHERE 1=1
									AND id_factory = '{factory}'
									AND YEAR(date) = '{year}'
									AND scenario = 'Budget'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_AUX_Production_TopFabBudget_Allocations") Then
							
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim year As String = args.NameValuePairs("yearOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_AUX_Production_TopFabBudget_Allocations
							
								WHERE 1=1
									AND id_factory = '{factory}'
									AND YEAR(date) = '{year}'
									-- AND scenario = 'Actual'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt		
							
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_AUX_Production_TopFabBudget_Volumes") Then
							
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim year As String = args.NameValuePairs("yearOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_AUX_Production_TopFabBudget_Volumes
							
								WHERE 1=1
									AND id_factory = '{factory}'
									AND YEAR(date) = '{year}'
									-- AND scenario = 'Actual'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt								
						#End Region
						
						#Region "COSTS"
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_FACT_Costs_Monthly") Then
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim year As String = args.NameValuePairs("yearOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_FACT_Costs
							
								WHERE 1=1
									AND id_factory = '{factory}'
									AND YEAR(date) = '{year}'
									-- AND scenario = 'Actual'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt							
						
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_FACT_Costs_Daily") Then
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim year As String = args.NameValuePairs("yearOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_FACT_CostsDaily
							
								WHERE 1=1
									AND id_factory = '{factory}'
									AND YEAR(date) = '{year}'
									AND scenario = 'Actual'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt
							
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_AUX_AllocationKeys") Then
							
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim year As String = args.NameValuePairs("yearOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_AUX_AllocationKeys
							
								WHERE 1=1
									AND id_factory = '{factory}'
									AND YEAR(date) = '{year}'
									-- AND scenario = 'Actual'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt	
							
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_AUX_FixedVariableCosts") Then
							
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim year As String = args.NameValuePairs("yearOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_AUX_FixedVariableCosts
							
								WHERE 1=1
									AND id_factory = '{factory}'
									AND YEAR(date) = '{year}'
									-- AND scenario = 'Actual'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt								
							
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_FACT_TopFabBudget_Costs") Then
							
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim year As String = args.NameValuePairs("yearOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_FACT_Costs
							
								WHERE 1=1
									AND id_factory = '{factory}'
									AND YEAR(date) = '{year}'
									AND scenario = 'Budget'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt
						#End Region	
						
						#Region "VA & EV"
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_AUX_EnergyVariance") Then
							
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim year As String = args.NameValuePairs("yearOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_AUX_EnergyVariance
							
								WHERE 1=1
									AND id_factory = '{factory}'
									AND YEAR(date) = '{year}'
									-- AND scenario = 'Actual'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt								
							
							
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_AUX_EffectsAnalysis") Then
							
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim year As String = args.NameValuePairs("yearOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_AUX_EffectsAnalysis
							
								WHERE 1=1
									AND id_factory = '{factory}'
									AND YEAR(date) = '{year}'
									-- AND scenario = 'Actual'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt	
						#End Region
						
#End Region	

#Region "Master Data"							
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_MASTER_CostCenter") Then
							Dim factory As String = args.NameValuePairs("factoryOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_MASTER_CostCenter
							
								WHERE 1=1
									AND factory = '{factory}'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt	
							
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_MASTER_CostCenter_Budget") Then
							' Añadir la columna de factory y de naturaleza y propiedades que falten
							Dim factory As String = args.NameValuePairs("factoryOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_MASTER_CostCenterBudget
							
								WHERE 1=1
									-- AND factory = '{factory}'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using							
							Return dt
							
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_HIER_CostCenter") Then
							Dim factory As String = args.NameValuePairs("factoryOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_HIER_CostCenter
							
								WHERE 1=1
									AND factory = '{factory}'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt	
							
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_HIER_Nomenclature") Then
							Dim factory As String = args.NameValuePairs("factoryOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_HIER_Nomenclature
							
								WHERE 1=1
									AND id_factory = '{factory}'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt	
							
						Else If args.DataSetName.XFEqualsIgnoreCase("XFC_PLT_MASTER_AverageGroup") Then
							Dim factory As String = args.NameValuePairs("factoryOS")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								SELECT *
							
								FROM XFC_PLT_MASTER_AverageGroup
							
								WHERE 1=1
									AND id_factory = '{factory}'
							"
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)
							
							End Using
							
							Return dt	
							
#End Region

#Region "SHARED QUERIES"
						Else If args.DataSetName.XFEqualsIgnoreCase("Shared") Then
							Dim query As String = "RepartoCostes"
							
							brapi.Errorlog.LogMessage(si, "1")
							Dim hola As Object = New Workspace.__WsNamespacePrefix.__WsAssemblyName.sharedClass
							
							query = hola.queriesCommon(si, "DistribucionCostes")
							
							 brapi.ErrorLog.LogMessage(si, query)
							Dim dt As DataTable
							
							return dt
							
							
#End Region

#Region "File Info"
						Else If args.DataSetName.XFEqualsIgnoreCase("UpdateFileInfo") Then
							
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim fileName As String = args.NameValuePairs("fileNameOS")
							Dim year As String = args.NameValuePairs("Year")
							Dim month As String = args.NameValuePairs("Month")
							Dim scenario As String = args.NameValuePairs("scenario")
							
							Dim fileNameMonth As String = String.Empty
							
							If fileName.Contains(".") And (scenario = "Actual" or fileName.Contains("ADMIN"))Then
								fileNameMonth = $"{fileName.Split(".")(0)}_M{month}.{fileName.Split(".")(1)}"
							Else
								fileNameMonth = fileName
							End If
								
							Dim filePath As String = If(fileName.Contains("ADMIN") _
							, $"Documents/Public/Plants/Admin/{fileNameMonth}" _
							, $"Documents/Public/Plants/Import/{factory}/{year}/{scenario}/{fileNameMonth}")
														
							Dim dt As New DataTable		
							dt.Columns.Add("LastUpdate")
							
							If BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, filePath)							
								
								Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase,filePath, True, False, False, SharedConstants.Unknown, Nothing, True)
								Dim info As String = fileInfo.XFFile.FileInfo.TimeModified.ToString

								dt.Rows.Add(info)
							Else
								dt.Rows.Add("No Info")
							End If
							
							Return dt								
#End Region			

#Region "Last update S4H"

						Else If args.DataSetName.XFEqualsIgnoreCase("LastUpdateS4H") Then
							
							Dim factory As String = args.NameValuePairs("factoryOS")
							Dim year As String = args.NameValuePairs("year")
							Dim month As String = args.NameValuePairs("month")
							
							Dim dt As DataTable
							Dim sql As String = String.Empty
							
							sql = $"
								--SELECT MAX(date) AS LastUpdate
								SELECT TOP 1 date AS LastUpdate
							
								FROM XFC_PLT_AUX_Log
							
								WHERE id_factory = '{factory}'
								AND object = 'XFC_MAIN_FACT_PnLTransactions'
								AND action = 'Import'
								AND YEAR(date) = {year}
								AND MONTH(date) = {month} + 1		
							
								ORDER BY date DESC
							"
							
'							BRApi.ErrorLog.LogMessage(si, sql)
							
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							 	dt = BRApi.Database.ExecuteSql(dbcon, sql, True)								
							
							End Using
							
							Return dt	

#End Region
							
						End If
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
