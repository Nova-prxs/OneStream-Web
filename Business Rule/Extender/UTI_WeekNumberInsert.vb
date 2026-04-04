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

Namespace OneStream.BusinessRule.Extender.UTI_WeekNumberInsert
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
					
				Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				
					Dim Query As String = $"TRUNCATE TABLE DateWeekNumber"

					BRApi.Database.ExecuteSql(dbcon,Query,False)
					
					Dim firstDate As DateTime = New DateTime(2020, 1, 1)
					Dim lastDate As DateTime = New DateTime(2030, 12, 31)

					Dim currentDate As DateTime = firstDate

				    While currentDate <= lastDate
						
						Dim result = GetWeekNumberAndYear(currentDate)
						
						Dim insertDate As String = currentDate.ToString("yyyy-MM-dd")
						Dim insertYear As Integer
						Dim insertWeek As Integer
						Dim insertMonth As Integer
						
						Dim lastDayOfYear As DateTime = New DateTime(currentDate.Year, 12, 31)
						Dim lastDayOfYearWeekdayNumber As Integer = CType(lastDayOfYear.DayOfWeek, Integer)
						Dim daysToLastMonday As Integer = (lastDayOfYear.DayOfWeek - DayOfWeek.Monday + 7) Mod 7
    					Dim lastMondayOfYear As DateTime = lastDayOfYear.AddDays(-daysToLastMonday)
						
						insertYear = result.Year
						insertWeek = result.WeekNumber
						insertMonth = result.Month
						
						If currentDate >= lastMondayOfYear AndAlso lastDayOfYearWeekdayNumber < 4 AndAlso lastDayOfYearWeekdayNumber <> 0 Then
							
							insertYear = result.Year + 1
							insertMonth = 1
							insertWeek = 1
							
						End If
						
						Query = $"	INSERT INTO DateWeekNumber (Date, CustomWeekNumber, WeekYear, MonthNumber)
									VALUES ('{insertDate}', {insertWeek.ToString}, {insertYear.ToString}, {insertMonth.ToString})"
	
'						BRAPI.ErrorLog.LogMessage(si, "Query: " & Query)
						BRApi.Database.ExecuteSql(dbcon,Query,False)
						
				        currentDate = currentDate.AddDays(1)
						
				    End While

				End Using					

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
			
		Function GetWeekNumberAndYear(ByVal dateValue As DateTime) As (WeekNumber As Integer, Year As Integer, Month As Integer)
	        Dim cultureInfo As CultureInfo = CultureInfo.CurrentCulture
	        Dim calendar As Calendar = cultureInfo.Calendar
	        Dim firstDayOfWeek As DayOfWeek = cultureInfo.DateTimeFormat.FirstDayOfWeek
	
	        ' Use the FirstFourDayWeek rule
	        Dim weekNumber As Integer = calendar.GetWeekOfYear(dateValue, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek)
	
	        ' Determine the year based on the week number and the date
	        Dim year As Integer = dateValue.Year
			
			' Determine the month of the date
			Dim month As Integer = dateValue.Month
	
	        ' Check if the date is in the last week of the year
	        If weekNumber = 1 AndAlso dateValue.Month = 12 Then
	            year += 1
				month = 1
	        End If
	
	        ' Check if the date is in the first week of the year but belongs to the last week of the previous year
	        If weekNumber >= 52 AndAlso dateValue.Month = 1 Then
	            year -= 1
				month = 12
	        End If
	
	        Return (weekNumber, year, month)
	    End Function
		
	End Class
	
End Namespace