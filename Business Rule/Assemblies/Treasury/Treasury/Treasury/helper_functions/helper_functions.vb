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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class helper_functions
		
		#Region "Constants"
		
		''' <summary>
		''' HTD (Horse Treasury Division) - Represents "all companies" / no entity filter
		''' When company ID = 0 or company name = HTD, no entity filtering should be applied
		''' </summary>
		Public Const TRS_GLOBAL_COMPANY As String = "HTD"
		Public Const TRS_GLOBAL_COMPANY_ID As Integer = 0
		
		#End Region
		
		#Region "Get Company IDs for User"
		
		Public Function GetCompanyIDsForUser(ByVal si As SessionInfo) As List(Of Integer)
			'Declare a dictionary of security groups and it's company id and a company id list
			Dim securityGroupToCompanyIDDict As New Dictionary(Of String, Integer) From {
				{"Horse Group", 0},
				{"Horse Holding", 1300},
				{"Horse HPL", 997},
				{"Horse Argentina", 592},
				{"Horse Chile", 585},
				{"Horse Portugal", 671},
				{"Horse Romania", 611},
				{"Horse Spain", 1301},
				{"Horse Turkey", 1302},
				{"Horse Brazil", 1303}
			}
			Dim companyIDList As New List(Of Integer)
			
			'Check user security groups and add company ids to the list
			If BRApi.Security.Authorization.IsUserInAdminGroup(si) OrElse BRApi.Security.Authorization.IsUserInGroup(si, "F_TRS_User") Then
				companyIDList.Add(0)
			Else
				For Each securityGroupToCompanyID As KeyValuePair(Of String, Integer) In securityGroupToCompanyIDDict
					If BRApi.Security.Authorization.IsUserInGroup(si, securityGroupToCompanyID.Key) Then companyIDList.Add(securityGroupToCompanyID.value)
				Next
			End If
			
			Return companyIDList
			
		End Function
		
		#End Region
		
		#Region "Treasury Shared Functions"
		
		''' <summary>
		''' Creates a simple error table view
		''' </summary>
		Public Shared Function CreateErrorTableView(ByVal errorMessage As String) As TableView
			Try
				Dim tv As New TableView()
				tv.CanModifyData = False
				
				tv.Columns.Add(New TableViewColumn() With {.Name = "Error", .Value = "Error", .IsHeader = True})
				
				Dim errorRow As New TableViewRow()
				errorRow.Items.Add("Error", New TableViewColumn() With {.Value = errorMessage, .IsHeader = False})
				tv.Rows.Add(errorRow)
				
				Return tv
			Catch ex As Exception
				Throw ex
			End Try
		End Function
		
		''' <summary>
		''' Gets the company name based on company ID from XFC_TRS_MASTER_Companies table
		''' </summary>
		Public Shared Function GetCompanyNameFromId(ByVal si As SessionInfo, ByVal companyIdStr As String) As String
			Try
				' Handle HTD (global/no filter) case
				If String.IsNullOrEmpty(companyIdStr) OrElse _
				   companyIdStr.Trim().Equals(TRS_GLOBAL_COMPANY, StringComparison.OrdinalIgnoreCase) OrElse _
				   companyIdStr.Trim() = TRS_GLOBAL_COMPANY_ID.ToString() Then
					Return TRS_GLOBAL_COMPANY
				End If
				
				Dim companyId As Integer
				If Not Integer.TryParse(companyIdStr, companyId) Then
					Return companyIdStr ' Return original if not a valid integer
				End If
				
				' Query XFC_TRS_MASTER_Companies table
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT name 
						FROM XFC_TRS_MASTER_Companies 
						WHERE id = {companyId}
					"
					
					Dim result As DataTable = BRApi.Database.ExecuteSql(dbConn, query, False)
					
					If result.Rows.Count > 0 AndAlso Not IsDBNull(result.Rows(0)("name")) Then
						Return result.Rows(0)("name").ToString()
					Else
						Return companyIdStr ' Fallback to ID if not found
					End If
				End Using
				
			Catch ex As Exception
				' If there's any error, just return the company ID
				Return companyIdStr
			End Try
		End Function
		
		''' <summary>
		''' Gets the company region based on company ID from XFC_TRS_MASTER_Companies table
		''' Falls back to "Unknown" if not found
		''' </summary>
		Public Shared Function GetCompanyRegionFromId(ByVal si As SessionInfo, ByVal companyIdStr As String) As String
			Try
				' Handle HTD (global/no filter) case
				If String.IsNullOrEmpty(companyIdStr) OrElse _
				   companyIdStr.Trim().Equals(TRS_GLOBAL_COMPANY, StringComparison.OrdinalIgnoreCase) OrElse _
				   companyIdStr.Trim() = TRS_GLOBAL_COMPANY_ID.ToString() Then
					Return TRS_GLOBAL_COMPANY
				End If
				
				Dim companyId As Integer
				If Not Integer.TryParse(companyIdStr, companyId) Then
					Return "Unknown" ' Return Unknown if not a valid integer
				End If
				
				' Query XFC_TRS_MASTER_Companies table for region
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT region 
						FROM XFC_TRS_MASTER_Companies 
						WHERE id = {companyId}
					"
					
					Dim result As DataTable = BRApi.Database.ExecuteSql(dbConn, query, False)
					
					If result.Rows.Count > 0 AndAlso Not IsDBNull(result.Rows(0)("region")) Then
						Return result.Rows(0)("region").ToString()
					Else
						Return "Unknown" ' Fallback if not found
					End If
				End Using
				
			Catch ex As Exception
				' If there's any error, return Unknown
				Return "Unknown"
			End Try
		End Function

		''' <summary>
		''' Gets the company country based on company ID from XFC_TRS_MASTER_Companies table
		''' Falls back to "Unknown" if not found
		''' </summary>
		Public Shared Function GetCompanyCountryFromId(ByVal si As SessionInfo, ByVal companyIdStr As String) As String
			Try
				' Handle HTD (global/no filter) case
				If String.IsNullOrEmpty(companyIdStr) OrElse _
				   companyIdStr.Trim().Equals(TRS_GLOBAL_COMPANY, StringComparison.OrdinalIgnoreCase) OrElse _
				   companyIdStr.Trim() = TRS_GLOBAL_COMPANY_ID.ToString() Then
					Return TRS_GLOBAL_COMPANY
				End If
				
				Dim companyId As Integer
				If Not Integer.TryParse(companyIdStr, companyId) Then
					Return "Unknown" ' Return Unknown if not a valid integer
				End If
				
				' Query XFC_TRS_MASTER_Companies table for country
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT country 
						FROM XFC_TRS_MASTER_Companies 
						WHERE id = {companyId}
					"
					
					Dim result As DataTable = BRApi.Database.ExecuteSql(dbConn, query, False)
					
					If result.Rows.Count > 0 AndAlso Not IsDBNull(result.Rows(0)("country")) Then
						Return result.Rows(0)("country").ToString()
					Else
						Return "Unknown" ' Fallback if not found
					End If
				End Using
				
			Catch ex As Exception
				' If there's any error, return Unknown
				Return "Unknown"
			End Try
		End Function
		
		''' <summary>
		''' Gets the TimeKey for the Monday of a specific week (as String parameter)
		''' Week 53 is treated as Week 1 of the next year (for cross-year support)
		''' </summary>
		Public Shared Function GetWeekMondayTimeKeyString(ByVal si As SessionInfo, ByVal year As String, ByVal week As String) As String
			Try
				' Validate inputs before conversion
				Dim yearInt As Integer
				Dim weekInt As Integer
				
				If Not Integer.TryParse(year, yearInt) Then
					yearInt = DateTime.Now.Year
				End If
				
				If Not Integer.TryParse(week, weekInt) Then
					weekInt = 1
				End If
				
				' CROSS-YEAR SUPPORT: Week 53 = Week 1 of next year
				If weekInt = 53 Then
					yearInt = yearInt + 1
					weekInt = 1
				End If
				
				' Calculate Monday of the specified week using the correct column names from aux_date table
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim sql As String = $"
						SELECT timekey 
						FROM XFC_TRS_AUX_Date 
						WHERE year = {yearInt} 
						AND weekNumber = {weekInt} 
						AND weekStartDate = fulldate
					"
					
					Dim result As DataTable = BRApi.Database.ExecuteSql(dbConn, sql, False)
					If result.Rows.Count > 0 Then
						Dim timeKey As String = result.Rows(0)("timekey").ToString()
						Return timeKey
					Else
						' Try alternative: get the first day of the specified week
						Dim sql2 As String = $"
							SELECT MIN(timekey) as timekey
							FROM XFC_TRS_AUX_Date 
							WHERE year = {yearInt} 
							AND weekNumber = {weekInt}
						"
						
						Dim result2 As DataTable = BRApi.Database.ExecuteSql(dbConn, sql2, False)
						If result2.Rows.Count > 0 AndAlso Not IsDBNull(result2.Rows(0)("timekey")) Then
							Dim timeKey As String = result2.Rows(0)("timekey").ToString()
							Return timeKey
						Else
							' Fallback calculation 
							Dim jan1 As DateTime = New DateTime(yearInt, 1, 1)
							Dim daysToAdd As Integer = (weekInt - 1) * 7
							Dim targetWeek As DateTime = jan1.AddDays(daysToAdd)
							Dim monday As DateTime = targetWeek.AddDays(-(CInt(targetWeek.DayOfWeek) - 1))
							Dim fallbackKey As String = monday.ToString("yyyyMMdd")
							Return fallbackKey
						End If
					End If
				End Using
			Catch ex As Exception
				' Fallback to simple format - use current year if yearInt is not available
				Dim fallbackYear As Integer = DateTime.Now.Year
				If Not String.IsNullOrEmpty(year) Then
					Integer.TryParse(year, fallbackYear)
				End If
				Return $"{fallbackYear}0101"
			End Try
		End Function
		
		''' <summary>
		''' Gets the Monday date of a specific week and year (returns DateTime)
		''' </summary>
		Public Shared Function GetWeekMondayDate(ByVal si As SessionInfo, ByVal year As String, ByVal week As String) As DateTime
			Try
				Dim yearInt As Integer
				Dim weekInt As Integer
				
				If Not Integer.TryParse(year, yearInt) Then
					yearInt = DateTime.Now.Year
				End If
				
				If Not Integer.TryParse(week, weekInt) Then
					weekInt = 1
				End If
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim sql As String = $"
						SELECT fulldate 
						FROM XFC_TRS_AUX_Date 
						WHERE year = {yearInt} 
						AND weekNumber = {weekInt} 
						AND weekStartDate = fulldate
					"
					
					Dim result As DataTable = BRApi.Database.ExecuteSql(dbConn, sql, False)
					If result.Rows.Count > 0 Then
						Dim dateValue As DateTime = Convert.ToDateTime(result.Rows(0)("fulldate"))
						Return dateValue
					Else
						' Fallback: Calculate ISO week Monday
						Dim jan1 As DateTime = New DateTime(yearInt, 1, 1)
						Dim daysToAdd As Integer = (weekInt - 1) * 7
						Dim targetWeek As DateTime = jan1.AddDays(daysToAdd)
						
						' Get Monday of that week
						Dim dayOfWeek As Integer = CInt(targetWeek.DayOfWeek)
						If dayOfWeek = 0 Then dayOfWeek = 7 ' Sunday = 7
						Dim monday As DateTime = targetWeek.AddDays(-(dayOfWeek - 1))
						
						Return monday
					End If
				End Using
			Catch ex As Exception
				Return New DateTime(DateTime.Now.Year, 1, 1)
			End Try
		End Function
		
		''' <summary>
		''' Gets the Monday date 4 weeks after a specific week and year
		''' </summary>
		Public Shared Function GetEndingWeekMondayDate(ByVal si As SessionInfo, ByVal year As String, ByVal week As String) As DateTime
			Try
				Dim startingMonday As DateTime = GetWeekMondayDate(si, year, week)
				Return startingMonday.AddDays(28) ' 4 weeks = 28 days
			Catch ex As Exception
				Return New DateTime(DateTime.Now.Year, 1, 1)
			End Try
		End Function
		
		''' <summary>
		''' Gets the last day of the month for a specific week and year
		''' </summary>
		Public Shared Function GetEndOfCurrentMonth(ByVal si As SessionInfo, ByVal year As String, ByVal week As String) As DateTime
			Try
				Dim startingMonday As DateTime = GetWeekMondayDate(si, year, week)
				Dim lastDayOfMonth As DateTime = New DateTime(startingMonday.Year, startingMonday.Month, DateTime.DaysInMonth(startingMonday.Year, startingMonday.Month))
				Return lastDayOfMonth
			Catch ex As Exception
				Return New DateTime(DateTime.Now.Year, 1, 31)
			End Try
		End Function
		
		''' <summary>
		''' Gets the month number based on the week number using AUX_Date table
		''' Returns the month of the first day (weekStartDate) of the specified week
		''' </summary>
		Public Shared Function GetMonthFromWeek(ByVal si As SessionInfo, ByVal year As String, ByVal week As Integer) As Integer
			Try
				' Validate year input
				Dim yearInt As Integer
				If Not Integer.TryParse(year, yearInt) Then
					yearInt = DateTime.Now.Year
				End If
				
				' Query AUX_Date to get month from the week start date (first day of the week)
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim sql As String = $"
						SELECT TOP 1 monthNumber 
						FROM XFC_TRS_AUX_Date 
						WHERE year = {yearInt} 
						AND weekNumber = {week}
						AND weekStartDate = fulldate
						ORDER BY fulldate
					"
					
					Dim result As DataTable = BRApi.Database.ExecuteSql(dbConn, sql, False)
					If result.Rows.Count > 0 AndAlso Not IsDBNull(result.Rows(0)("monthNumber")) Then
						Dim monthNum As Integer = Convert.ToInt32(result.Rows(0)("monthNumber"))
						Return monthNum
					Else
						' Fallback: estimate month based on week number
						Dim estimatedMonth As Integer = Math.Min(12, Math.Max(1, CInt(Math.Ceiling(week / 4.33))))
						Return estimatedMonth
					End If
				End Using
				
			Catch ex As Exception
				' Fallback to estimated month
				Dim estimatedMonth As Integer = Math.Min(12, Math.Max(1, CInt(Math.Ceiling(week / 4.33))))
				Return estimatedMonth
			End Try
		End Function
		
		''' <summary>
		''' Gets the Monday date for a specific week in day-month format (e.g., "30-dic")
		''' </summary>
		Public Shared Function GetMondayDateForWeek(ByVal si As SessionInfo, ByVal year As String, ByVal week As String) As String
			Try
				' Validate inputs
				Dim yearInt As Integer
				Dim weekInt As Integer
				
				If Not Integer.TryParse(year, yearInt) Then
					yearInt = DateTime.Now.Year
				End If
				
				If Not Integer.TryParse(week, weekInt) Then
					weekInt = 1
				End If
				
				' Try to get Monday date from the aux_date table first
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim sql As String = $"
						SELECT fulldate 
						FROM XFC_TRS_AUX_Date 
						WHERE year = {yearInt} 
						AND weekNumber = {weekInt} 
						AND weekStartDate = fulldate
					"
					
					Dim result As DataTable = BRApi.Database.ExecuteSql(dbConn, sql, False)
					If result.Rows.Count > 0 Then
						Dim mondayDate As DateTime = Convert.ToDateTime(result.Rows(0)("fulldate"))
						Return FormatDateToDayMonth(mondayDate)
					Else
						' Fallback: calculate Monday manually
						Dim jan1 As DateTime = New DateTime(yearInt, 1, 1)
						Dim daysToAdd As Integer = (weekInt - 1) * 7
						Dim targetWeek As DateTime = jan1.AddDays(daysToAdd)
						
						' Get Monday of that week
						Dim daysFromMonday As Integer = CInt(targetWeek.DayOfWeek) - 1
						If daysFromMonday < 0 Then daysFromMonday = 6 ' Sunday case
						Dim monday As DateTime = targetWeek.AddDays(-daysFromMonday)
						
						Return FormatDateToDayMonth(monday)
					End If
				End Using
			Catch ex As Exception
				' Fallback to current date if all else fails
				Return FormatDateToDayMonth(DateTime.Now)
			End Try
		End Function
		
		''' <summary>
		''' Formats a date to day-month format (e.g., "30-dic", "06-ene")
		''' </summary>
		Public Shared Function FormatDateToDayMonth(ByVal dateValue As DateTime) As String
			Try
				' Spanish month abbreviations
				Dim monthNames As String() = {
					"ene", "feb", "mar", "abr", "may", "jun",
					"jul", "ago", "sep", "oct", "nov", "dic"
				}
				
				Dim day As String = dateValue.Day.ToString().PadLeft(2, "0"c)
				Dim month As String = monthNames(dateValue.Month - 1)
				
				Return $"{day}-{month}"
			Catch ex As Exception
				Return "01-ene"
			End Try
		End Function
		
		''' <summary>
		''' Calculates accumulated cashflow values from all previous years (starting from 2023) up to the year before currentYear.
		''' For example, if currentYear = 2026, it will sum accumulated values from 2023, 2024, and 2025.
		''' This is used for year-over-year carryforward of accumulated cashflow totals.
		''' </summary>
		''' <param name="si">SessionInfo</param>
		''' <param name="currentYear">The current year as string (e.g., "2026")</param>
		''' <param name="entity">Entity filter (e.g., company name or TRS_GLOBAL_COMPANY for no filter)</param>
		''' <param name="getYearAccumulatedFunc">Function delegate that calculates accumulated values for a single year</param>
		''' <returns>Dictionary with "AccumulatedAll" and "AccumulatedOperInv" keys containing the sum of all previous years</returns>
		Public Shared Function GetHistoricalAccumulatedValues(
			ByVal si As SessionInfo, 
			ByVal currentYear As String, 
			ByVal entity As String,
			ByVal getYearAccumulatedFunc As Func(Of SessionInfo, DbConnInfo, String, String, Dictionary(Of String, Decimal))
		) As Dictionary(Of String, Decimal)
			Try
				Dim result As New Dictionary(Of String, Decimal)
				result("AccumulatedAll") = 0
				result("AccumulatedOperInv") = 0
				
				' Validate current year
				Dim currentYearInt As Integer
				If Not Integer.TryParse(currentYear, currentYearInt) Then
					Return result
				End If
				
				' Base year - start accumulation from 2023
				Const BASE_YEAR As Integer = 2023
				
				' If current year is 2023 or earlier, no previous years to accumulate
				If currentYearInt <= BASE_YEAR Then
					Return result
				End If
				
				Dim totalAccumulatedAll As Decimal = 0
				Dim totalAccumulatedOperInv As Decimal = 0
				
				' Dictionary to store per-entity accumulated values (for multi-entity queries)
				Dim entityAccumulated As New Dictionary(Of String, Decimal)
				
				' Sum accumulated values from BASE_YEAR to (currentYear - 1)
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					For yearToProcess As Integer = BASE_YEAR To (currentYearInt - 1)
						Dim yearStr As String = yearToProcess.ToString()
						
						' Call the provided function to get accumulated values for this specific year
						Dim yearValues As Dictionary(Of String, Decimal) = getYearAccumulatedFunc(si, dbConn, entity, yearStr)
						
						' Check if we're dealing with single entity or multi-entity
						If entity <> TRS_GLOBAL_COMPANY Then
							' Single entity mode: sum using "AccumulatedAll" and "AccumulatedOperInv" keys
							Dim yearAccumAll As Decimal = If(yearValues.ContainsKey("AccumulatedAll"), yearValues("AccumulatedAll"), 0)
							Dim yearAccumOperInv As Decimal = If(yearValues.ContainsKey("AccumulatedOperInv"), yearValues("AccumulatedOperInv"), 0)
							
							totalAccumulatedAll += yearAccumAll
							totalAccumulatedOperInv += yearAccumOperInv
						Else
							' Multi-entity mode: sum using "EntityName_All" and "EntityName_OperInv" keys
							For Each kvp In yearValues
								Dim key As String = kvp.Key
								Dim value As Decimal = kvp.Value
								
								' Accumulate per-entity values across years
								If entityAccumulated.ContainsKey(key) Then
									entityAccumulated(key) += value
								Else
									entityAccumulated(key) = value
								End If
							Next
						End If
					Next
				End Using
				
				' Return appropriate format based on query mode
				If entity <> TRS_GLOBAL_COMPANY Then
					' Single entity: return simple keys
					result("AccumulatedAll") = totalAccumulatedAll
					result("AccumulatedOperInv") = totalAccumulatedOperInv
				Else
					' Multi-entity: return per-entity keys
					result = entityAccumulated
				End If
				
				Return result
				
			Catch ex As Exception
				Return New Dictionary(Of String, Decimal) From {
					{"AccumulatedAll", 0},
					{"AccumulatedOperInv", 0}
				}
			End Try
		End Function
		
		''' <summary>
		''' Calculates accumulated values from previous years for a SINGLE entity (used in OF_IF reports).
		''' Returns the accumulated value for the specific entity with key format "{entity}_OF_IF".
		''' For example, if currentYear = 2026 and entity = "Horse Aveiro", it will sum accumulated values from 2023, 2024, and 2025.
		''' </summary>
		''' <param name="si">SessionInfo</param>
		''' <param name="currentYear">The current year as string (e.g., "2026")</param>
		''' <param name="entity">Entity name (e.g., "Horse Aveiro")</param>
		''' <param name="getYearAccumulatedFunc">Function delegate that calculates accumulated values for a single year</param>
		''' <returns>Dictionary with "{entity}_OF_IF" key containing the sum of all previous years for that entity</returns>
		Public Shared Function GetHistoricalAccumulatedValuesForEntity(
			ByVal si As SessionInfo, 
			ByVal currentYear As String, 
			ByVal entity As String,
			ByVal getYearAccumulatedFunc As Func(Of SessionInfo, DbConnInfo, String, String, Dictionary(Of String, Decimal))
		) As Dictionary(Of String, Decimal)
			Try
				Dim result As New Dictionary(Of String, Decimal)
				Dim entityKey As String = $"{entity}_OF_IF"
				result(entityKey) = 0
				
				' Validate current year
				Dim currentYearInt As Integer
				If Not Integer.TryParse(currentYear, currentYearInt) Then
					Return result
				End If
				
				' Base year - start accumulation from 2023
				Const BASE_YEAR As Integer = 2023
				
				' If current year is 2023 or earlier, no previous years to accumulate
				If currentYearInt <= BASE_YEAR Then
					Return result
				End If
				
				Dim totalAccumulated As Decimal = 0
				
				' Sum accumulated values from BASE_YEAR to (currentYear - 1)
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					For yearToProcess As Integer = BASE_YEAR To (currentYearInt - 1)
						Dim yearStr As String = yearToProcess.ToString()
						
						' Call the provided function to get accumulated values for this specific year
						Dim yearValues As Dictionary(Of String, Decimal) = getYearAccumulatedFunc(si, dbConn, entity, yearStr)
						
						' Extract the value for this entity
						If yearValues.ContainsKey(entityKey) Then
							Dim yearValue As Decimal = yearValues(entityKey)
							totalAccumulated += yearValue
						End If
					Next
				End Using
				
				result(entityKey) = totalAccumulated
				
				Return result
				
			Catch ex As Exception
				Dim entityKey As String = $"{entity}_OF_IF"
				Return New Dictionary(Of String, Decimal) From {{entityKey, 0}}
			End Try
		End Function
		
		#End Region

		#Region "LPW Functions - Copied from helper_functions_LPW"
		
		''' <summary>
		''' Get confirmed months from auxiliary table
		''' </summary>
		Public Function GetConfirmedMonths(ByVal si As SessionInfo, ByVal type As String,
			ByVal company As Integer, ByVal scenario As String, ByVal year As String
		) As DataTable
			'Declare db param infos and dt
			Dim dbParamInfos As New List(Of DbParamInfo) From {
				New DbParamInfo("paramType", type),
				New DbParamInfo("paramCompany", company),
				New DbParamInfo("paramScenario", scenario),
				New DbParamInfo("paramYear", year)
			}
			Dim dt As New DataTable()
			'Query confirmed months
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				dt = BRApi.Database.ExecuteSql(
					dbConn,
					"
					SELECT *
					FROM XFC_INV_AUX_confirmed_month WITH(NOLOCK)
					WHERE type = @paramType
						AND company_id = @paramCompany
						AND scenario = @paramScenario
						AND year = @paramYear
					",
					dbParamInfos, False
				)
			End Using
			
			Return dt
			
		End Function
		
		''' <summary>
		''' Set month confirmation in auxiliary table
		''' </summary>
		Public Sub SetMonthConfirmation(ByVal si As SessionInfo, ByVal type As String,
			ByVal company As Integer, ByVal scenario As String, ByVal year As String,
			ByVal month As Integer, ByVal confirm As Boolean)
			'Declare db param infos
			Dim dbParamInfos As New List(Of DbParamInfo) From {
				New DbParamInfo("paramType", type),
				New DbParamInfo("paramCompany", company),
				New DbParamInfo("paramScenario", scenario),
				New DbParamInfo("paramYear", year),
				New DbParamInfo("paramMonth", month),
				New DbParamInfo("paramConfirm", If(confirm, 1, 0))
			}
			'Query confirmed months
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				BRApi.Database.ExecuteSql(
					dbConn,
					$"
					MERGE INTO XFC_INV_AUX_confirmed_month AS target
					USING (
						SELECT
							@paramType AS type,
							@paramCompany AS company_id,
							@paramScenario AS scenario,
							@paramYear AS year
					) AS source
					ON target.type = source.type
					AND target.company_id = source.company_id
					AND target.scenario = source.scenario
					AND target.year = source.year
					WHEN MATCHED THEN
					    UPDATE SET
							M{month} = @paramConfirm
					WHEN NOT MATCHED THEN
					    INSERT (
					        type, company_id, scenario, year, M{month}
					    )
					    VALUES (
					        source.type, source.company_id, source.scenario, source.year, @paramConfirm
					    );
					",
					dbParamInfos, False
				)
			End Using
			
		End Sub
		
		''' <summary>
		''' Get the last RF scenario from previous year
		''' </summary>
		Public Function GetLastRF( ByVal si As SessionInfo, ByVal type As String,
			ByVal company As Integer, ByVal scenario As String, ByVal year As Integer
		) As String
			'Declare lastRF, db param infos and table name
			Dim lastRF As String = String.Empty
			Dim dbParamInfos As New List(Of DbParamInfo) From {
				New DbParamInfo("paramType", type),
				New DbParamInfo("paramCompany", company),
				New DbParamInfo("paramScenario", scenario),
				New DbParamInfo("paramYear", year)
			}
			Dim tableName As String = If(
				type.ToLower = "cash",
				"
					XFC_INV_FACT_project_cash pc WITH(NOLOCK)
					LEFT JOIN XFC_INV_MASTER_project p WITH(NOLOCK) ON pc.project_id = p.project
				",
				"
					XFC_INV_FACT_asset_depreciation ad WITH(NOLOCK)
					LEFT JOIN XFC_INV_MASTER_asset a WITH(NOLOCK) ON ad.asset_id = a.id
					LEFT JOIN XFC_INV_MASTER_project p WITH(NOLOCK) ON a.project_id = p.project
				"
			)
			
			'Get the last rf
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim dt As DataTable = BRApi.Database.ExecuteSql(
					dbConn,
					$"
					SELECT TOP(1) scenario
					FROM {tableName}
					WHERE
						year = @paramYear - 1
						AND scenario LIKE 'RF%'
						AND (
							@paramCompany = 0
							OR p.company_id = @paramCompany
						)
					ORDER BY CAST(REPLACE(LEFT(scenario, 4), 'RF', '') AS INTEGER) DESC
					",
					dbParamInfos, False
				)
				lastRF = If(
					dt.Rows.Count > 0,
					dt.Rows(0)("scenario"),
					"No RF in previous year"
				)
			End Using
			
			Return lastRF
		End Function
		
		''' <summary>
		''' Get forecast month from scenario name (e.g., "RF01" returns 0)
		''' </summary>
		Public Function GetForecastMonth(ByVal si As SessionInfo, ByVal scenario As String) As Integer
			'Get first 4 characters
			Dim scenarioShorted As String = Left(scenario, 4).ToLower
			Return If(scenarioShorted.StartsWith("rf"), CInt(scenarioShorted.Replace("rf", "")) - 1, 0)
		End Function
		
		''' <summary>
		''' Get base entity from parent entity ID
		''' </summary>
		Public Function GetBaseEntity(ByVal si As SessionInfo, ByVal entityId As String) As String
			'Declare parent to base entity dict
			Dim parentToBaseEntityDict As New Dictionary(Of String, String) From {
				{"1300", "R1300001"},
				{"1301", "R1301001"},
				{"1302", "R1302001"},
				{"1303", "R1303001"},
				{"611", "R0611001"},
				{"671", "R0671001"},
				{"592", "R0592001"},
				{"585", "R0585001"}
			}
			
			If parentToBaseEntityDict.ContainsKey(entityId) Then
				Return parentToBaseEntityDict(entityId)
			Else
				Return ""
			End If
		End Function
		
		''' <summary>
		''' Get parent entity from entity ID
		''' </summary>
		Public Function GetParentEntity(ByVal si As SessionInfo, ByVal entityId As String) As String
			'Declare parent to base entity dict
			Dim parentToBaseEntityDict As New Dictionary(Of String, String) From {
				{"0", "Horse_Group"},
				{"1300", "R1300"},
				{"1301", "R1301"},
				{"1302", "R1302"},
				{"1303", "R1303"},
				{"611", "R0611"},
				{"671", "R0671"},
				{"592", "R0592"},
				{"585", "R0585"}
			}
			
			If parentToBaseEntityDict.ContainsKey(entityId) Then
				Return parentToBaseEntityDict(entityId)
			Else
				Return ""
			End If
		End Function
		
		''' <summary>
		''' Get time filter from forecast month for data processing
		''' </summary>
		Public Function GetTimeFilterFromForecastMonth(ByVal si As SessionInfo, ByVal year As Integer, ByVal forecastMonth As Integer) As String
			'Declare time filter string
			Dim timeFilter As String = String.Empty
			
			'Loop through each month to be processed
			For i As Integer = forecastMonth + 1 To 12
				timeFilter = If(String.IsNullOrEmpty(timeFilter), $"T#{year}M{i}", $"{timeFilter}, T#{year}M{i}")
			Next
			
			Return timeFilter
		End Function
		
		''' <summary>
		''' Check if a scenario year is confirmed for a specific company
		''' </summary>
		Public Function CheckScenarioYearConfirmation(
			ByVal si As SessionInfo, ByVal entityId As String,
			ByVal scenario As String, ByVal year As Integer
		) As Boolean
			'Declare db param infos
			Dim dbParamInfos As New List(Of DbParamInfo) From {
				New DbParamInfo("paramCompany", entityId),
				New DbParamInfo("paramYear", year),
				New DbParamInfo("paramScenario", scenario)
			}
			
			'Get confirmation
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim dt As DataTable = BRApi.Database.ExecuteSql(
					dbConn,
					"
						SELECT is_confirmed
						FROM XFC_INV_AUX_confirmed_scenario_year
						WHERE company_id = @paramCompany
							AND year = @paramYear
							AND scenario = @paramScenario
					",
					dbParamInfos,
					False
				)
				If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
					Return If(dt.Rows(0)("is_confirmed") = "Completed", True, False)
				Else
					Return False
				End If
			End Using
			
			Return False
		End Function
		
		#End Region

	End Class
End Namespace
