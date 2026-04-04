Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace OneStream.BusinessRule.DashboardStringFunction.AQS_AutoConso_CurrentPeriod
    Public Class MainClass
        ' Helper function to calculate the number of working days between two dates.
        Public Function GetWorkingDays(startDate As DateTime, endDate As DateTime) As Integer
            Dim totalDays As Integer = endDate.Subtract(startDate).TotalDays
            Dim totalWeekends As Integer = Math.Floor((totalDays + startDate.DayOfWeek - DayOfWeek.Saturday) / 7) * 2
            Return CInt(totalDays - totalWeekends)
        End Function

        ' Helper function to get the nth working day of a month.
        Public Function GetNthWorkingDayOfMonth(year As Integer, month As Integer, n As Integer) As DateTime
            Dim targetDate As DateTime = New DateTime(year, month, 1)
            While n > 0
                If targetDate.DayOfWeek <> DayOfWeek.Saturday And targetDate.DayOfWeek <> DayOfWeek.Sunday Then
                    n -= 1
                End If
                targetDate = targetDate.AddDays(1)
            End While
            Return targetDate.AddDays(-1)
        End Function

        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
            Try
                ' Check if the function name is "AQS_Get_TimeFilter".
                If args.FunctionName.XFEqualsIgnoreCase("AQS_Get_TimeFilter") Then
                ' Check if the function name is "AQS_Get_TimeFilter_Counter".
                ElseIf args.FunctionName.XFEqualsIgnoreCase("AQS_Get_TimeFilter_Counter") Then
                ' Add a new function name check for "AQS_Get_TimeFilter_CurrentWorkingDay".
                ElseIf args.FunctionName.XFEqualsIgnoreCase("AQS_Get_TimeFilter_CurrentWorkingDay") Then
                    ' Get current date.
                    Dim currentDate As DateTime = DateTime.Now
                    ' Get current month and year.
                    Dim currentYear As Integer = currentDate.Year
                    Dim currentMonth As Integer = currentDate.Month
                    ' Get 6th working day of current month.
                    'Dim sixthWorkingDay As DateTime = GetNthWorkingDayOfMonth(currentYear, currentMonth, 6) 'Original Line. 
					Dim sixthWorkingDay As DateTime = GetNthWorkingDayOfMonth(currentYear, currentMonth, 21) 'New line to extend the closing of the previous month while it still open 08.01.2025
                    ' Get 5th working day of next month.
                    Dim nextMonth As Integer = If(currentMonth = 12, 1, currentMonth + 1)
                    Dim nextYear As Integer = If(currentMonth = 12, currentYear + 1, currentYear)
                    'Dim fifthWorkingDayNextMonth As DateTime = GetNthWorkingDayOfMonth(nextYear, nextMonth, 5) 'Original line
					Dim fifthWorkingDayNextMonth As DateTime = GetNthWorkingDayOfMonth(nextYear, nextMonth, 20) 'New line to extend the closing of the previous month while it still open 08.01.2025
                    ' Determine which month to use based on working day range.
                    Dim workingDay As Integer = GetWorkingDays(New DateTime(currentYear, currentMonth, 1), currentDate)
                    Dim wfTime As String
                    If workingDay >= 21 AndAlso workingDay <= GetWorkingDays(sixthWorkingDay, fifthWorkingDayNextMonth) Then
                        wfTime = currentYear & "M" & currentMonth
                    Else
                        ' Use previous month if not within working day range.
                        currentMonth -= 1
                        If currentMonth = 0 Then
                            currentMonth = 12
                            currentYear -= 1
                        End If
                        wfTime = currentYear & "M" & currentMonth.ToString("00")
                    End If
                    ' Return the calculated time filter.
                    Return wfTime
                End If

                ' If the function name does not match any of the above, return nothing.
                Return Nothing
            Catch ex As Exception
                ' If an exception occurs, log it and re-throw it.
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
    End Class
End Namespace