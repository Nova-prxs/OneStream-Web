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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_export_excel
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
																		
						Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
						Dim queries As Object = New Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.queries.MainClass
						
						Dim fileName As String = args.NameValuePairs.GetValueOrDefault("fileName", "Export.csv")
						
						'Create data table depending on the table parameter
						Dim dt As New DataTable
						Dim args2 As New DashboardDataSetArgs
						
						args2.FunctionType = DashboardDataSetFunctionType.GetDataSet
						args2.DataSetName = args.NameValuePairs.GetValueOrEmpty("dataSetName")
						args2.NameValuePairs("scenario") = args.NameValuePairs.GetValueOrEmpty("scenario")
						args2.NameValuePairs("scenario_ref") = args.NameValuePairs.GetValueOrEmpty("scenario_ref")
						args2.NameValuePairs("scenarioRef") = args.NameValuePairs.GetValueOrEmpty("scenario_ref")
						args2.NameValuePairs("year") = args.NameValuePairs.GetValueOrEmpty("year")
						args2.NameValuePairs("month") = args.NameValuePairs.GetValueOrEmpty("month")
						args2.NameValuePairs("currency") = args.NameValuePairs.GetValueOrEmpty("currency")
						args2.NameValuePairs("factory") = args.NameValuePairs.GetValueOrEmpty("factory")
						args2.NameValuePairs("account_type") = args.NameValuePairs.GetValueOrEmpty("account_type")
						args2.NameValuePairs("view") = args.NameValuePairs.GetValueOrEmpty("view")
						args2.NameValuePairs("product") = args.NameValuePairs.GetValueOrEmpty("product")
						
						dt = queries.Main(si, globals, api, args2) 	
'						Dim sql As String = "
'								SELECT *
'								FROM XFC_PLT_RAW_CostsMonthly
'								--WHERE entity IN ('001','009')
'								--WHERE entity IN ('002')
'								--WHERE entity IN ('STE')
'								--WHERE entity IN ('003')
'						"
'						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
'						End Using	
						
						If fileName.Contains("xlsx") Then

							shared_queries.CreateExcel(si, _
														args2.NameValuePairs("factory"), _
														args2.NameValuePairs("year"), _
														args2.NameValuePairs("month"), _
														args2.NameValuePairs("scenario"), _
														, _
														, fileName, _
														, _
														"",dt)
						Else
							
							shared_queries.CreateCSV(si,,,,dt)
							
						End If

						
						Return Nothing

							
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource

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
