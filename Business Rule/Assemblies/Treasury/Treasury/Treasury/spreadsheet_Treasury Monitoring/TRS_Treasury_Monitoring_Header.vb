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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_Treasury_Monitoring_Header
	Public Class MainClass
		
		' Get Treasury Monitoring Header with dynamic title
		Private Function GetTreasuryMonitoringHeader(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As TableView
			Try
				' Get entity ID parameter
				Dim paramEntityId As String = ""
				Dim entityName As String = "HORSE SPAIN"
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_CompanyNames") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_CompanyNames")) Then
					paramEntityId = args.CustSubstVarsAlreadyResolved("prm_Treasury_CompanyNames")
					
					' Convert entity ID to entity name using helper function
					Dim paramEntity As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, paramEntityId)
					
					' If entity is HTD or empty, use "Horse Global", otherwise use the actual name
					If String.IsNullOrEmpty(paramEntity) OrElse paramEntity.Trim() = "HTD" Then
						entityName = "Horse Global"
					Else
						entityName = paramEntity.Trim().ToUpper()
					End If
				End If
				
				' Create TableView for header
				Dim tv As New TableView()
				tv.CanModifyData = False ' Read-only
				
				' Add single column
				tv.Columns.Add(New TableViewColumn() With {.Name = "Title", .Value = "", .IsHeader = True})
				
				' Add title row
				Dim titleRow As New TableViewRow() With {.IsHeader = True}
				Dim titleText As String = $"{entityName} - Treasury Monitoring - Actions performed"
				
				titleRow.Items.Add("Title", New TableViewColumn() With {
					.Value = titleText,
					.IsHeader = True,
					.DataType = XFDataType.Text
				})
				
				tv.Rows.Add(titleRow)
				
				' Configure formatting
				tv.HeaderFormat.BackgroundColor = XFColors.White
				tv.HeaderFormat.IsBold = True
				tv.HeaderFormat.TextColor = XFColors.Black
				
				Return tv
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = SpreadsheetFunctionType.Unknown
						Return Nothing
						
					Case Is = SpreadsheetFunctionType.GetCustomSubstVarsInUse
						Try
							Dim list As New List(Of String)
							list.Add("prm_Treasury_CompanyNames")
							Return list
						Catch ex As Exception
							Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						End Try
						
					Case Is = SpreadsheetFunctionType.GetTableView
						Return GetTreasuryMonitoringHeader(si, globals, api, args)
						
					Case Is = SpreadsheetFunctionType.SaveTableView
						Return Nothing
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
