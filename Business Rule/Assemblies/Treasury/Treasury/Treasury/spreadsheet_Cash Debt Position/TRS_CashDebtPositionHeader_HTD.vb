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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashDebtPositionHeader_HTD
	Public Class MainClass
		'Reference Solution Helper Business Rule
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = SpreadsheetFunctionType.Unknown
						
					Case Is = SpreadsheetFunctionType.GetCustomSubstVarsInUse
						Try
							Dim list As New List(Of String)
							' Only Year and Week parameters needed for global header
							list.Add("prm_Treasury_Year")
							list.Add("prm_Treasury_WeekNumber")
							Return list
						Catch ex As Exception
							Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						End Try
						
					Case Is = SpreadsheetFunctionType.GetTableView
						Return Me.GetCashDebtPositionHeaderGlobalReport(si, args)
						
					Case Is = SpreadsheetFunctionType.SaveTableView
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		#Region "Cash Debt Position Header Global Report"

		Private Function GetCashDebtPositionHeaderGlobalReport(ByVal si As SessionInfo, ByVal args As SpreadsheetArgs) As TableView
			Try
				' Validate input parameters
				If args Is Nothing OrElse args.CustSubstVarsAlreadyResolved Is Nothing Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: No parameters provided")
				End If
				
				' Get parameters from spreadsheet args
				Dim paramYear As String = ""
				Dim paramWeek As String = ""
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_Year") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_Year")) Then
					paramYear = args.CustSubstVarsAlreadyResolved("prm_Treasury_Year")
				End If
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_WeekNumber") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_WeekNumber")) Then
					paramWeek = args.CustSubstVarsAlreadyResolved("prm_Treasury_WeekNumber")
				End If
				
				' Validate required parameters
				If String.IsNullOrEmpty(paramYear) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Year parameter is missing")
				End If
				
				If String.IsNullOrEmpty(paramWeek) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Week parameter is missing")
				End If
				
				' Entity is always "HORSE GLOBAL" for global view
				Dim paramEntity As String = "HORSE HTD"
				
				' Calculate dates
				Dim startingWeekDate As DateTime = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayDate(si, paramYear, paramWeek)
				Dim endingWeekDate As DateTime = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetEndingWeekMondayDate(si, paramYear, paramWeek)
				Dim endOfMonthDate As DateTime = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetEndOfCurrentMonth(si, paramYear, paramWeek)
				
				' Create table view using correct OneStream API
				Dim tv As New TableView()
				tv.CanModifyData = False
				
				' Add columns for the header information table
				tv.Columns.Add(New TableViewColumn() With {.Name = "Field", .Value = "Field", .IsHeader = True})
				tv.Columns.Add(New TableViewColumn() With {.Name = "Value", .Value = "Value", .IsHeader = True})
				
				' Add main title row
				Dim titleRow As New TableViewRow() With {.IsHeader = True}
				titleRow.Items.Add("Field", New TableViewColumn() With {.Value = $"{paramEntity} - Cash and Debt position report", .IsHeader = True})
				titleRow.Items.Add("Value", New TableViewColumn() With {.Value = "", .IsHeader = True})
				tv.Rows.Add(titleRow)
				
				' Add blank row for separation
				Dim blankRow1 As New TableViewRow()
				blankRow1.Items.Add("Field", New TableViewColumn() With {.Value = "", .IsHeader = False})
				blankRow1.Items.Add("Value", New TableViewColumn() With {.Value = "", .IsHeader = False})
				tv.Rows.Add(blankRow1)
				
				' Add Entity row
				Dim entityRow As New TableViewRow()
				entityRow.Items.Add("Field", New TableViewColumn() With {.Value = "ENTITY", .IsHeader = False})
				entityRow.Items.Add("Value", New TableViewColumn() With {.Value = paramEntity, .IsHeader = False})
				tv.Rows.Add(entityRow)
				
				' Add Starting week row
				Dim startingWeekRow As New TableViewRow()
				startingWeekRow.Items.Add("Field", New TableViewColumn() With {.Value = "STARTING week of", .IsHeader = False})
				startingWeekRow.Items.Add("Value", New TableViewColumn() With {.Value = startingWeekDate.ToString("dd/MM/yy"), .IsHeader = False})
				tv.Rows.Add(startingWeekRow)
				
				' Add Ending week row
				Dim endingWeekRow As New TableViewRow()
				endingWeekRow.Items.Add("Field", New TableViewColumn() With {.Value = "ENDING week of", .IsHeader = False})
				endingWeekRow.Items.Add("Value", New TableViewColumn() With {.Value = endingWeekDate.ToString("dd/MM/yy"), .IsHeader = False})
				tv.Rows.Add(endingWeekRow)
				
				' Add End of current month row
				Dim endOfMonthRow As New TableViewRow()
				endOfMonthRow.Items.Add("Field", New TableViewColumn() With {.Value = "END OF CURRENT MONTH", .IsHeader = False})
				endOfMonthRow.Items.Add("Value", New TableViewColumn() With {.Value = endOfMonthDate.ToString("dd/MM/yy"), .IsHeader = False})
				tv.Rows.Add(endOfMonthRow)
				
				' Add Status row
				Dim statusRow As New TableViewRow()
				statusRow.Items.Add("Field", New TableViewColumn() With {.Value = "STATUS", .IsHeader = False})
				statusRow.Items.Add("Value", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = False})
				tv.Rows.Add(statusRow)
				
				' Add Mill EUR row
				Dim millEurRow As New TableViewRow()
				millEurRow.Items.Add("Field", New TableViewColumn() With {.Value = "Mill EUR", .IsHeader = False})
				millEurRow.Items.Add("Value", New TableViewColumn() With {.Value = "", .IsHeader = False})
				tv.Rows.Add(millEurRow)
				
				' Configure formatting
				tv.HeaderFormat.BackgroundColor = XFColors.Black
				tv.HeaderFormat.IsBold = True
				tv.HeaderFormat.TextColor = XFColors.White
				
				Return tv
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		#End Region

	End Class
End Namespace
