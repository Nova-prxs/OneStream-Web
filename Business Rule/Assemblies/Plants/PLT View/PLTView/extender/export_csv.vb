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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.export_csv
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType

					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
																		
						Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
						Dim queries As Object = New Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.queries.MainClass
						Dim folderPath As String = "Documents/Public/Plants/Exports"
						Dim fileName As String = "Export.csv"
						Dim destPath As String = ""
						Dim fileType As String =  args.NameValuePairs.GetValueOrEmpty("fileType")
						
						Dim dt As New DataTable
						Dim dtMeta As New DataTable
						Dim args2 As New DashboardDataSetArgs
						Dim args3 As New DashboardDataSetArgs
						
						' Incluir todos los parámetros posibles
						args2.FunctionType = DashboardDataSetFunctionType.GetDataSet
						args2.DataSetName = args.NameValuePairs.GetValueOrEmpty("dataSetName")
						args2.NameValuePairs("scenario") = args.NameValuePairs.GetValueOrEmpty("scenario")
						args2.NameValuePairs("scenario_ref") = args.NameValuePairs.GetValueOrEmpty("scenario_ref")
						args2.NameValuePairs("scenarioRef") = args.NameValuePairs.GetValueOrEmpty("scenario_ref")
						args2.NameValuePairs("year") = args.NameValuePairs.GetValueOrEmpty("year")
						args2.NameValuePairs("month") = args.NameValuePairs.GetValueOrEmpty("month")
						args2.NameValuePairs("currency") = args.NameValuePairs.GetValueOrEmpty("currency")
						args2.NameValuePairs("factory") = args.NameValuePairs.GetValueOrEmpty("factory")
						args2.NameValuePairs("account") = args.NameValuePairs.GetValueOrEmpty("account")
						args2.NameValuePairs("account_type") = args.NameValuePairs.GetValueOrEmpty("account_type")
						args2.NameValuePairs("view") = args.NameValuePairs.GetValueOrEmpty("view")
						args2.NameValuePairs("product") = args.NameValuePairs.GetValueOrEmpty("product")
						args2.NameValuePairs("GM_Function") = args.NameValuePairs.GetValueOrEmpty("GM_Function")						
						args2.NameValuePairs("indicator") = args.NameValuePairs.GetValueOrEmpty("indicator")
						args2.NameValuePairs("reportDetail") = args.NameValuePairs.GetValueOrEmpty("reportDetail")
						args2.NameValuePairs("reportDetailProduction") = args.NameValuePairs.GetValueOrEmpty("reportDetailProduction")
						args2.NameValuePairs("reportDetailCosts") = args.NameValuePairs.GetValueOrEmpty("reportDetailCosts")
						
						Dim factory As String = args.NameValuePairs.GetValueOrEmpty("factory")
						
'						args2.NameValuePairs("factory") = factory
						' brapi.ErrorLog.LogMessage(si, "CUENTAS "& args.NameValuePairs.GetValueOrEmpty("account"))
						' brapi.ErrorLog.LogMessage(si, "PRODUCT "& args.NameValuePairs.GetValueOrEmpty("product"))
						' Obtener Metadatos
						args3.FunctionType = DashboardDataSetFunctionType.GetDataSet
						args3.DataSetName = "FileInfo"
						args3.NameValuePairs("dataSetName") = args.NameValuePairs.GetValueOrEmpty("dataSetName")
						
						
						' Generación de DT y fichero csv
						dt = queries.Main(si, globals, api, args2) 	
						dtMeta = queries.Main(si, globals, api, args3)
						
''						shared_queries.CreateCSV(si,folderPath,fileName,destPath,dt)						
						
''						shared_queries.CreateExcel_New(si _ 
''													, args.NameValuePairs.GetValueOrEmpty("factory") _ 
''													, args.NameValuePairs.GetValueOrEmpty("year") _
''													, args.NameValuePairs.GetValueOrEmpty("month") _
''													, args.NameValuePairs.GetValueOrEmpty("scenario") _
''													, _
''													, args.NameValuePairs.GetValueOrEmpty("scenario_ref") _
''													, args.NameValuePairs.GetValueOrEmpty("view") _
''													, args.NameValuePairs.GetValueOrEmpty("currency") _
''													, args.NameValuePairs.GetValueOrEmpty("product") _
''													, args.NameValuePairs.GetValueOrEmpty("indicator") _
''													, _
''													, "Export.xlsx" _
''													, _
''													, "" _
''													, dt _
''													, dtMeta)

						If fileType = "csv" Then
							If dt.Rows.Count <= 1400000 Then
								shared_queries.CreateCSV(si,folderPath,fileName,destPath,dt)
							Else
								shared_queries.CreateTXT(si,folderPath,fileName,destPath,dt)
							End If
						Else 
							If dt.Rows.Count <= 1400000 Then
								shared_queries.CreateExcel_New(si _ 
															, args.NameValuePairs.GetValueOrEmpty("factory") _ 
															, args.NameValuePairs.GetValueOrEmpty("year") _
															, args.NameValuePairs.GetValueOrEmpty("month") _
															, args.NameValuePairs.GetValueOrEmpty("scenario") _
															, _
															, args.NameValuePairs.GetValueOrEmpty("scenario_ref") _
															, args.NameValuePairs.GetValueOrEmpty("view") _
															, args.NameValuePairs.GetValueOrEmpty("currency") _
															, args.NameValuePairs.GetValueOrEmpty("product") _
															, args.NameValuePairs.GetValueOrEmpty("indicator") _
															, _
															, "Export.xlsx" _
															, _
															, "" _
															, dt _
															, dtMeta)
							Else
								shared_queries.CreateTXT(si,folderPath,fileName,destPath,dt)								
							End If
						End If
						
						Return Nothing

				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function generatePath(ByVal si As SessionInfo, ByVal parameter As String, ByVal fileNameExport As String)
			
			'Set current dashboard
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
			selectionChangedTaskResult.ModifiedCustomSubstVars(parameter) = fileNameExport
			brapi.Dashboards.Parameters.SetLiteralParameterValue(si, True, parameter, fileNameExport)
			Return selectionChangedTaskResult
			
		End Function
	End Class
End Namespace
