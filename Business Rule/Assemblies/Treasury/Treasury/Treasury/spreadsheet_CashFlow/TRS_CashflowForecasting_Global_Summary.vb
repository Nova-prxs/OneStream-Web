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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashflowForecasting_Global_Summary
	Public Class MainClass
		'Global Summary - Operating + Investment Flows by Entity (Rolling 4 Weeks)
		'Shows rolling 4-week sums of Inflows, Outflows, and Net Income for all entities (HTD + Total Group + Aurobay Sweden + HPL + Aurobay China)
		'Uses mix of Actual (past weeks) and Projection (future weeks) based on selected Scenario
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = SpreadsheetFunctionType.Unknown
						
					Case Is = SpreadsheetFunctionType.GetCustomSubstVarsInUse
						Try
							Dim list As New List(Of String)
							' Year and Week parameters needed for rolling 4-week summary
							list.Add("prm_Treasury_Year")
							list.Add("prm_Treasury_WeekNumber")
							Return list
						Catch ex As Exception
							Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						End Try
						
					Case Is = SpreadsheetFunctionType.GetTableView
						Return Me.GetGlobalSummaryReport(si, args)
						
					Case Is = SpreadsheetFunctionType.SaveTableView
						Try
							' Read-only view
							Return True
						Catch ex As Exception
							Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						End Try
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function FormatDecimalValue(ByVal value As Decimal) As String
			Return If(value = 0, "0.00", value.ToString("F2", CultureInfo.InvariantCulture))
		End Function
		#Region "Global Summary Report - Rolling 4 Weeks (Operating + Investment Flows)"

		Private Function GetGlobalSummaryReport(ByVal si As SessionInfo, ByVal args As SpreadsheetArgs) As TableView
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
				
				Dim uploadWeek As Integer = 0
				If Not Integer.TryParse(paramWeek, uploadWeek) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Week parameter must be numeric")
				End If
				
				' HTD entities (for Total Group calculation)
				Dim htdEntities As New List(Of String) From {
					"Horse Aveiro",
					"OYAK HORSE",
					"Horse Romania",
					"Horse Chile",
					"Horse Brazil",
					"Horse Argentina",
					"Horse Spain",
					"Horse Powertrain Solution"
				}
				
				' All entities including global entities
				Dim allEntities As New List(Of String) From {
					"Horse Aveiro",
					"OYAK HORSE",
					"Horse Romania",
					"Horse Chile",
					"Horse Brazil",
					"Horse Argentina",
					"Horse Spain",
					"Horse Powertrain Solution",
					"Aurobay Sweden",
					"HPL",
					"Aurobay China"
				}
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' Get maximum weeks for the selected year
					Dim maxWeeksSql As String = TRS_SQL_Repository.GetMaxWeekInYearQuery(paramYear)
					Dim maxWeeksDt As DataTable = BRApi.Database.ExecuteSql(dbConn, maxWeeksSql, False)
					Dim maxWeeks As Integer = 52 ' Default to 52 if query fails
					If maxWeeksDt IsNot Nothing AndAlso maxWeeksDt.Rows.Count > 0 AndAlso Not IsDBNull(maxWeeksDt.Rows(0)("MaxWeek")) Then
						maxWeeks = Convert.ToInt32(maxWeeksDt.Rows(0)("MaxWeek"))
					End If
					
					' Create the dynamic TableView
					Dim tv As New TableView()
					tv.CanModifyData = False ' Read-only view
					
					' ====================================
					' STEP 1: Define columns structure
					' ====================================
					' First column: Description (Entity Name, Inflow, Outflow, Net Income)
					tv.Columns.Add(New TableViewColumn() With {.Name = "Description", .Value = "Million of EUR", .IsHeader = True})
					
					' Calculate total weeks to display (from week 1 to maxWeeks)
					Dim totalWeeks As New List(Of Integer)
					For i As Integer = 1 To maxWeeks
					    totalWeeks.Add(i)
					Next
					
					' Add week columns (Week 1 through Week 52)
					For Each weekNum As Integer In totalWeeks
					    tv.Columns.Add(New TableViewColumn() With {
					        .Name = $"Week{weekNum}", 
					        .Value = $"Week {weekNum}", 
					        .IsHeader = True, 
					        .DataType = XFDataType.Decimal
					    })
					Next
					
					' ====================================
					' STEP 2: Add header rows (week numbers and dates)
					' ====================================
					' Header Row 1: Week numbers
					Dim weekNumberRow As New TableViewRow() With {.IsHeader = True}
					weekNumberRow.Items.Add("Description", New TableViewColumn() With {.Value = $"Global Summary - Scenario FW{paramWeek} (Rolling 4 Weeks)", .IsHeader = True})
					For Each weekNum As Integer In totalWeeks
						weekNumberRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
							.Value = $"Week {weekNum}", 
							.IsHeader = True,
							.DataType = XFDataType.Text
						})
					Next
					tv.Rows.Add(weekNumberRow)
					
					' Header Row 2: Monday dates for each week
					Dim mondayDateRow As New TableViewRow() With {.IsHeader = True}
					mondayDateRow.Items.Add("Description", New TableViewColumn() With {.Value = "Week Start Date", .IsHeader = True})
					For Each weekNum As Integer In totalWeeks
						Dim mondayDate As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetMondayDateForWeek(si, paramYear, weekNum.ToString())
						mondayDateRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
							.Value = mondayDate, 
							.IsHeader = True,
							.DataType = XFDataType.Text
						})
					Next
					tv.Rows.Add(mondayDateRow)
					
					' ====================================
					' STEP 3: Get individual week data (simplified approach like HTD Summary)
					' ====================================
					' Get individual week data, then calculate rolling 4-week in VB.NET
					' For weeks < uploadWeek: use Actual data (uploaded in ProjectionWeekNumber + 1)
					' For weeks >= uploadWeek: use Projection data (uploaded in uploadWeek)
					' Use centralized SQL repository
					Dim allDataSql As String = TRS_SQL_Repository.GetCashFlowSummaryGroupedQuery(paramYear, uploadWeek)
					
					Dim allDataDt As DataTable = BRApi.Database.ExecuteSql(dbConn, allDataSql, False)
					
					' Build nested dictionary: Entity -> Flow -> Week -> Amount
					' Structure: entityData(EntityName)(FlowType)(WeekNumber) = Decimal
					Dim entityData As New Dictionary(Of String, Dictionary(Of String, Dictionary(Of Integer, Decimal)))
					
					' Initialize dictionary for ALL entities
					For Each entity As String In allEntities
						entityData(entity) = New Dictionary(Of String, Dictionary(Of Integer, Decimal))
						entityData(entity)("INFLOWS") = New Dictionary(Of Integer, Decimal)
						entityData(entity)("OUTFLOWS") = New Dictionary(Of Integer, Decimal)
						
						' Initialize all weeks to 0
						For weekNum As Integer = 1 To maxWeeks
							entityData(entity)("INFLOWS")(weekNum) = 0
							entityData(entity)("OUTFLOWS")(weekNum) = 0
						Next
					Next
					
					' ====================================
					' STEP 4: Calculate Rolling 4-Week sums in VB.NET
					' ====================================
					' First, store individual week data from query
					Dim weekData As New Dictionary(Of String, Dictionary(Of String, Dictionary(Of Integer, Decimal)))
					
					For Each row As DataRow In allDataDt.Rows
						Dim entity As String = If(IsDBNull(row("Entity")), "", row("Entity").ToString())
						Dim flow As String = If(IsDBNull(row("Flow")), "", row("Flow").ToString().ToUpper())
						Dim weekNumber As Integer = If(IsDBNull(row("ProjectionWeekNumber")), 0, Convert.ToInt32(row("ProjectionWeekNumber")))
						Dim amount As Decimal = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))
						
						' Find the matching entity name (case-insensitive)
						Dim matchedEntity As String = Nothing
						For Each allEntity As String In allEntities
							If String.Equals(entity, allEntity, StringComparison.OrdinalIgnoreCase) Then
								matchedEntity = allEntity
								Exit For
							End If
						Next
						
						If matchedEntity Is Nothing Then Continue For
						If weekNumber < 1 OrElse weekNumber > maxWeeks Then Continue For
						If flow <> "INFLOWS" AndAlso flow <> "OUTFLOWS" Then Continue For
						
						' Initialize if needed
						If Not weekData.ContainsKey(matchedEntity) Then
							weekData(matchedEntity) = New Dictionary(Of String, Dictionary(Of Integer, Decimal))
							weekData(matchedEntity)("INFLOWS") = New Dictionary(Of Integer, Decimal)
							weekData(matchedEntity)("OUTFLOWS") = New Dictionary(Of Integer, Decimal)
						End If
						
						' Store individual week data
						weekData(matchedEntity)(flow)(weekNumber) = amount
					Next
					
					' Now calculate rolling 4-week sums
					For Each entity As String In allEntities
						For weekNum As Integer = 1 To maxWeeks
							Dim inflowSum As Decimal = 0
							Dim outflowSum As Decimal = 0
							
							' Sum weeks N, N+1, N+2, N+3
							For offset As Integer = 0 To 3
								Dim targetWeek As Integer = weekNum + offset
								If targetWeek <= maxWeeks Then
									If weekData.ContainsKey(entity) Then
										If weekData(entity)("INFLOWS").ContainsKey(targetWeek) Then
											inflowSum += weekData(entity)("INFLOWS")(targetWeek)
										End If
										If weekData(entity)("OUTFLOWS").ContainsKey(targetWeek) Then
											outflowSum += weekData(entity)("OUTFLOWS")(targetWeek)
										End If
									End If
								End If
							Next
							
							' Store rolling 4-week sum
							entityData(entity)("INFLOWS")(weekNum) = inflowSum
							entityData(entity)("OUTFLOWS")(weekNum) = outflowSum
						Next
					Next
					
					' ====================================
					' STEP 5: Add entity rows in order: HTD entities + Total Group + Aurobay Sweden + HPL + Aurobay China
					' ====================================
					
					' PART A: Add HTD entities (4 rows per entity: Name, Inflow, Outflow, Net Income)
					For Each entity As String In htdEntities
						
						' Row 1: Entity Name (header row)
						Dim entityNameRow As New TableViewRow() With {.IsHeader = True}
						entityNameRow.Items.Add("Description", New TableViewColumn() With {.Value = entity, .IsHeader = True})
						For Each weekNum As Integer In totalWeeks
							entityNameRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
								.Value = "",
								.IsHeader = True,
								.DataType = XFDataType.Text
							})
						Next
						tv.Rows.Add(entityNameRow)
						
						' Row 2: Inflow
						Dim inflowRow As New TableViewRow() With {.IsHeader = False}
						inflowRow.Items.Add("Description", New TableViewColumn() With {.Value = "  Inflow", .IsHeader = False})
						For Each weekNum As Integer In totalWeeks
							Dim inflowValue As Decimal = entityData(entity)("INFLOWS")(weekNum)
							Dim formattedValue As String = Me.FormatDecimalValue(inflowValue)
							inflowRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
								.Value = formattedValue,
								.IsHeader = False,
								.DataType = XFDataType.Decimal
							})
						Next
						tv.Rows.Add(inflowRow)
						
						' Row 3: Outflow
						Dim outflowRow As New TableViewRow() With {.IsHeader = False}
						outflowRow.Items.Add("Description", New TableViewColumn() With {.Value = "  Outflow", .IsHeader = False})
						For Each weekNum As Integer In totalWeeks
							Dim outflowValue As Decimal = entityData(entity)("OUTFLOWS")(weekNum)
							Dim formattedValue As String = Me.FormatDecimalValue(outflowValue)
							outflowRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
								.Value = formattedValue,
								.IsHeader = False,
								.DataType = XFDataType.Decimal
							})
						Next
						tv.Rows.Add(outflowRow)
						
						' Row 4: Net Income (Inflow + Outflow)
						Dim netIncomeRow As New TableViewRow() With {.IsHeader = False}
						netIncomeRow.Items.Add("Description", New TableViewColumn() With {.Value = "  Net Income", .IsHeader = False})
						For Each weekNum As Integer In totalWeeks
							Dim inflowValue As Decimal = entityData(entity)("INFLOWS")(weekNum)
							Dim outflowValue As Decimal = entityData(entity)("OUTFLOWS")(weekNum)
							Dim netIncome As Decimal = inflowValue + outflowValue
							Dim formattedValue As String = Me.FormatDecimalValue(netIncome)
							netIncomeRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
								.Value = formattedValue,
								.IsHeader = False,
								.DataType = XFDataType.Decimal
							})
						Next
						tv.Rows.Add(netIncomeRow)
						
					Next
					
					' ====================================
					' PART B: Add "Total Group" section (sum of all HTD entities)
					' ====================================
					
					' Row 1: Total Group Name (header row)
					Dim totalGroupNameRow As New TableViewRow() With {.IsHeader = True}
					totalGroupNameRow.Items.Add("Description", New TableViewColumn() With {.Value = "Total Group", .IsHeader = True})
					For Each weekNum As Integer In totalWeeks
						totalGroupNameRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
							.Value = "",
							.IsHeader = True,
							.DataType = XFDataType.Text
						})
					Next
					tv.Rows.Add(totalGroupNameRow)
					
					' Row 2: Total Group Inflow (sum of all HTD inflows)
					Dim totalGroupInflowRow As New TableViewRow() With {.IsHeader = False}
					totalGroupInflowRow.Items.Add("Description", New TableViewColumn() With {.Value = "  Inflow", .IsHeader = False})
					For Each weekNum As Integer In totalWeeks
						Dim totalInflow As Decimal = 0
						For Each entity As String In htdEntities
							totalInflow += entityData(entity)("INFLOWS")(weekNum)
						Next
						Dim formattedValue As String = Me.FormatDecimalValue(totalInflow)
						totalGroupInflowRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
							.Value = formattedValue,
							.IsHeader = False,
							.DataType = XFDataType.Decimal
						})
					Next
					tv.Rows.Add(totalGroupInflowRow)
					
					' Row 3: Total Group Outflow (sum of all HTD outflows)
					Dim totalGroupOutflowRow As New TableViewRow() With {.IsHeader = False}
					totalGroupOutflowRow.Items.Add("Description", New TableViewColumn() With {.Value = "  Outflow", .IsHeader = False})
					For Each weekNum As Integer In totalWeeks
						Dim totalOutflow As Decimal = 0
						For Each entity As String In htdEntities
							totalOutflow += entityData(entity)("OUTFLOWS")(weekNum)
						Next
						Dim formattedValue As String = Me.FormatDecimalValue(totalOutflow)
						totalGroupOutflowRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
							.Value = formattedValue,
							.IsHeader = False,
							.DataType = XFDataType.Decimal
						})
					Next
					tv.Rows.Add(totalGroupOutflowRow)
					
					' Row 4: Total Group Net Income (sum of all HTD net incomes)
					Dim totalGroupNetIncomeRow As New TableViewRow() With {.IsHeader = False}
					totalGroupNetIncomeRow.Items.Add("Description", New TableViewColumn() With {.Value = "  Net Income", .IsHeader = False})
					For Each weekNum As Integer In totalWeeks
						Dim totalInflow As Decimal = 0
						Dim totalOutflow As Decimal = 0
						For Each entity As String In htdEntities
							totalInflow += entityData(entity)("INFLOWS")(weekNum)
							totalOutflow += entityData(entity)("OUTFLOWS")(weekNum)
						Next
						Dim totalNetIncome As Decimal = totalInflow + totalOutflow
						Dim formattedValue As String = Me.FormatDecimalValue(totalNetIncome)
						totalGroupNetIncomeRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
							.Value = formattedValue,
							.IsHeader = False,
							.DataType = XFDataType.Decimal
						})
					Next
					tv.Rows.Add(totalGroupNetIncomeRow)
					
					' ====================================
					' PART C: Add Global entities (Aurobay Sweeden, HPL, Aurobay China)
					' ====================================
					Dim globalEntities As String() = {"Aurobay Sweden", "HPL", "Aurobay China"}
					
					For Each entity As String In globalEntities
						
						' Row 1: Entity Name (header row)
						Dim globalEntityNameRow As New TableViewRow() With {.IsHeader = True}
						globalEntityNameRow.Items.Add("Description", New TableViewColumn() With {.Value = entity, .IsHeader = True})
						For Each weekNum As Integer In totalWeeks
							globalEntityNameRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
								.Value = "",
								.IsHeader = True,
								.DataType = XFDataType.Text
							})
						Next
						tv.Rows.Add(globalEntityNameRow)
						
						' Row 2: Inflow
						Dim globalInflowRow As New TableViewRow() With {.IsHeader = False}
						globalInflowRow.Items.Add("Description", New TableViewColumn() With {.Value = "  Inflow", .IsHeader = False})
						For Each weekNum As Integer In totalWeeks
							Dim inflowValue As Decimal = entityData(entity)("INFLOWS")(weekNum)
							Dim formattedValue As String = Me.FormatDecimalValue(inflowValue)
							globalInflowRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
								.Value = formattedValue,
								.IsHeader = False,
								.DataType = XFDataType.Decimal
							})
						Next
						tv.Rows.Add(globalInflowRow)
						
						' Row 3: Outflow
						Dim globalOutflowRow As New TableViewRow() With {.IsHeader = False}
						globalOutflowRow.Items.Add("Description", New TableViewColumn() With {.Value = "  Outflow", .IsHeader = False})
						For Each weekNum As Integer In totalWeeks
							Dim outflowValue As Decimal = entityData(entity)("OUTFLOWS")(weekNum)
							Dim formattedValue As String = Me.FormatDecimalValue(outflowValue)
							globalOutflowRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
								.Value = formattedValue,
								.IsHeader = False,
								.DataType = XFDataType.Decimal
							})
						Next
						tv.Rows.Add(globalOutflowRow)
						
						' Row 4: Net Income (Inflow + Outflow)
						Dim globalNetIncomeRow As New TableViewRow() With {.IsHeader = False}
						globalNetIncomeRow.Items.Add("Description", New TableViewColumn() With {.Value = "  Net Income", .IsHeader = False})
						For Each weekNum As Integer In totalWeeks
							Dim inflowValue As Decimal = entityData(entity)("INFLOWS")(weekNum)
							Dim outflowValue As Decimal = entityData(entity)("OUTFLOWS")(weekNum)
							Dim netIncome As Decimal = inflowValue + outflowValue
							Dim formattedValue As String = Me.FormatDecimalValue(netIncome)
							globalNetIncomeRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
								.Value = formattedValue,
								.IsHeader = False,
								.DataType = XFDataType.Decimal
							})
						Next
						tv.Rows.Add(globalNetIncomeRow)
						
					Next
					
					' Configure formatting
					tv.HeaderFormat.BackgroundColor = XFColors.Black
					tv.HeaderFormat.IsBold = True
					tv.HeaderFormat.TextColor = XFColors.White
					
					Return tv
				End Using
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		#End Region

	End Class
End Namespace

