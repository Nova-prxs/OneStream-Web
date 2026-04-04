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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashFlowForecastingHeader
	Public Class MainClass
		'Reference Solution Helper Business Rule
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = SpreadsheetFunctionType.Unknown
						
					Case Is = SpreadsheetFunctionType.GetCustomSubstVarsInUse
						Try
							Dim list As New List(Of String)
							list.Add("prm_Treasury_CompanyNames")
							list.Add("prm_Treasury_Year")
							list.Add("prm_Treasury_WeekNumber")
							Return list
						Catch ex As Exception
							Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						End Try
						
					Case Is = SpreadsheetFunctionType.GetTableView
						Return Me.GetCashFlowForecastingHeaderReport(si, args)
						
					Case Is = SpreadsheetFunctionType.SaveTableView
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		#Region "Cash Flow Forecasting Header Report"

		Private Function GetCashFlowForecastingHeaderReport(ByVal si As SessionInfo, ByVal args As SpreadsheetArgs) As TableView
			Try
				' Validate input parameters
				If args Is Nothing OrElse args.CustSubstVarsAlreadyResolved Is Nothing Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: No parameters provided")
				End If
				
				' Get parameters from spreadsheet args
				Dim paramEntityId As String = ""
				Dim paramYear As String = ""
				Dim paramWeek As String = ""
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_CompanyNames") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_CompanyNames")) Then
					paramEntityId = args.CustSubstVarsAlreadyResolved("prm_Treasury_CompanyNames")
				End If
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_Year") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_Year")) Then
					paramYear = args.CustSubstVarsAlreadyResolved("prm_Treasury_Year")
				End If
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_WeekNumber") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_WeekNumber")) Then
					paramWeek = args.CustSubstVarsAlreadyResolved("prm_Treasury_WeekNumber")
				End If
				
				' Validate required parameters
				If String.IsNullOrEmpty(paramEntityId) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Company parameter is missing")
				End If
				
				If String.IsNullOrEmpty(paramYear) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Year parameter is missing")
				End If
				
				If String.IsNullOrEmpty(paramWeek) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Week parameter is missing")
				End If
				
				' Convert company ID to company name and region
				Dim paramEntity As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, paramEntityId)
				Dim paramRegion As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyRegionFromId(si, paramEntityId)
				Dim paramCountry As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyCountryFromId(si, paramEntityId)
				
				' Calculate date
				Dim weekDate As DateTime = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayDate(si, paramYear, paramWeek)
				
				' Create table view using correct OneStream API
				Dim tv As New TableView()
				tv.CanModifyData = False
				
				' Add columns for the header information table
				tv.Columns.Add(New TableViewColumn() With {.Name = "Field", .Value = "Field", .IsHeader = True})
				tv.Columns.Add(New TableViewColumn() With {.Name = "Value", .Value = "Value", .IsHeader = True})
				tv.Columns.Add(New TableViewColumn() With {.Name = "Spacer", .Value = "", .IsHeader = True})
				tv.Columns.Add(New TableViewColumn() With {.Name = "Status", .Value = "Status", .IsHeader = True})
				
				' Get confirmation status from database
				Dim confirmationStatus As String = "Not uploaded"
				Try
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Dim checkSql As String = TRS_SQL_Repository.GetConfirmationStatusQuery("CashFlow")
						
						Dim checkParams As New List(Of DbParamInfo) From {
							New DbParamInfo("entity", paramEntity),
							New DbParamInfo("week", Integer.Parse(paramWeek)),
							New DbParamInfo("year", Integer.Parse(paramYear))
						}
						
						Dim checkDt As DataTable = BRApi.Database.ExecuteSql(dbConn, checkSql, checkParams, False)
						
						If checkDt.Rows.Count > 0 Then
							' Record exists - check if confirmed
							Dim cashFlowBit As Object = checkDt.Rows(0)("CashFlow")
							If Not IsDBNull(cashFlowBit) AndAlso Convert.ToBoolean(cashFlowBit) Then
								confirmationStatus = "Confirmed"
							Else
								confirmationStatus = "Unconfirmed"
							End If
						End If
						' If Rows.Count = 0, status remains "Not uploaded"
					End Using
				Catch ex As Exception
					' If error, keep default "Not uploaded"
				End Try
				
				' Add main title row
				Dim titleRow As New TableViewRow() With {.IsHeader = True}
				titleRow.Items.Add("Field", New TableViewColumn() With {.Value = $"{paramEntity.ToUpper()} - Cashflow forecasting report", .IsHeader = True})
				titleRow.Items.Add("Value", New TableViewColumn() With {.Value = "", .IsHeader = True})
				titleRow.Items.Add("Spacer", New TableViewColumn() With {.Value = "", .IsHeader = True})
				titleRow.Items.Add("Status", New TableViewColumn() With {.Value = "STATUS", .IsHeader = True})
				tv.Rows.Add(titleRow)
				
				' Add subtitle row
				Dim subtitleRow As New TableViewRow() With {.IsHeader = True}
				subtitleRow.Items.Add("Field", New TableViewColumn() With {.Value = "Million of EUR", .IsHeader = True})
				subtitleRow.Items.Add("Value", New TableViewColumn() With {.Value = "", .IsHeader = True})
				subtitleRow.Items.Add("Spacer", New TableViewColumn() With {.Value = "", .IsHeader = True})
				subtitleRow.Items.Add("Status", New TableViewColumn() With {.Value = confirmationStatus, .IsHeader = True})
				tv.Rows.Add(subtitleRow)
				
				' Add blank row for separation
				Dim blankRow1 As New TableViewRow()
				blankRow1.Items.Add("Field", New TableViewColumn() With {.Value = "", .IsHeader = False})
				blankRow1.Items.Add("Value", New TableViewColumn() With {.Value = "", .IsHeader = False})
				blankRow1.Items.Add("Spacer", New TableViewColumn() With {.Value = "", .IsHeader = False})
				blankRow1.Items.Add("Status", New TableViewColumn() With {.Value = "", .IsHeader = False})
				tv.Rows.Add(blankRow1)
				
				' Add Date row
				Dim dateRow As New TableViewRow()
				dateRow.Items.Add("Field", New TableViewColumn() With {.Value = "Date", .IsHeader = False})
				dateRow.Items.Add("Value", New TableViewColumn() With {.Value = weekDate.ToString("dd-MMM-yy", CultureInfo.CreateSpecificCulture("en-US")), .IsHeader = False})
				dateRow.Items.Add("Spacer", New TableViewColumn() With {.Value = "", .IsHeader = False})
				dateRow.Items.Add("Status", New TableViewColumn() With {.Value = "", .IsHeader = False})
				tv.Rows.Add(dateRow)
				
				' Add Region row
				Dim regionRow As New TableViewRow()
				regionRow.Items.Add("Field", New TableViewColumn() With {.Value = "Region", .IsHeader = False})
				regionRow.Items.Add("Value", New TableViewColumn() With {.Value = paramRegion.ToUpper(), .IsHeader = False})
				regionRow.Items.Add("Spacer", New TableViewColumn() With {.Value = "", .IsHeader = False})
				regionRow.Items.Add("Status", New TableViewColumn() With {.Value = "", .IsHeader = False})
				tv.Rows.Add(regionRow)
				
				' Add Country row
				Dim countryRow As New TableViewRow()
				countryRow.Items.Add("Field", New TableViewColumn() With {.Value = "Country", .IsHeader = False})
				countryRow.Items.Add("Value", New TableViewColumn() With {.Value = paramCountry, .IsHeader = False})
				countryRow.Items.Add("Spacer", New TableViewColumn() With {.Value = "", .IsHeader = False})
				countryRow.Items.Add("Status", New TableViewColumn() With {.Value = "", .IsHeader = False})
				tv.Rows.Add(countryRow)
				
				' Add Legal entity row
				Dim legalEntityRow As New TableViewRow()
				legalEntityRow.Items.Add("Field", New TableViewColumn() With {.Value = "Legal entity", .IsHeader = False})
				legalEntityRow.Items.Add("Value", New TableViewColumn() With {.Value = paramEntity.ToUpper(), .IsHeader = False})
				legalEntityRow.Items.Add("Spacer", New TableViewColumn() With {.Value = "", .IsHeader = False})
				legalEntityRow.Items.Add("Status", New TableViewColumn() With {.Value = "", .IsHeader = False})
				tv.Rows.Add(legalEntityRow)
				
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
