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

Namespace OneStream.BusinessRule.Extender.CAL_DataMgmt
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
'						TruncateActAndBud(si)
'						CreateActual(si)
'						CreateBudget(si)
'						UpdateHolidaysAmount(si)
'						UpdateNonHolidayDaysAmount(si)

						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						If args.DataMgmtArgs.Sequence.Name.Equals("TruncateActAndBud") Then
							TruncateBud(si)
						Else If args.DataMgmtArgs.Sequence.Name.Equals("Reset") Then
							TruncateActAndBud(si)
						Else If args.DataMgmtArgs.Sequence.Name.Equals("CreateActual") Then
							CreateActual(si)
						Else If args.DataMgmtArgs.Sequence.Name.Equals("CreateBudget") Then
							CreateBudget(si)
						Else If args.DataMgmtArgs.Sequence.Name.Equals("UpdateHolidaysAmount") Then
							UpdateHolidaysAmount(si) 
						Else If args.DataMgmtArgs.Sequence.Name.Equals("UpdateNonHolidayDaysAmount") Then
							UpdateNonHolidayDaysAmount(si) 
						Else If args.DataMgmtArgs.Sequence.Name.Equals("RunAllBudget") Then
							CreateBudget(si)
							UpdateHolidaysAmount(si) 
							UpdateNonHolidayDaysAmount(si) 
														
						End If
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						'Add External Members
						Dim externalMembers As New List(Of NameValuePair)
						externalMembers.Add(New NameValuePair("YourMember1Name","YourMember1Value"))
						externalMembers.Add(New NameValuePair("YourMember2Name","YourMember2Value"))
						Return externalMembers
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		
		#Region "UpdateNonHolidayDaysAmount"
		Public Function UpdateNonHolidayDaysAmount(ByVal si As SessionInfo)
			Dim sql As New Text.StringBuilder()
			
			Dim dtDays As New DataTable()
			'Dim listStore As New List(Of String) ({"Barcelona Carrer del Freser 45-47", "Madrid C. de Alberto Aguilera 5"})
			Using dbc = Brapi.Database.CreateApplicationDbConnInfo(si)
				dtDays = BRApi.Database.ExecuteSqlUsingReader(dbc, "Select * from XFA_CAL_2023 WHERE Amount = 0", False)

				For Each drDays As DataRow In dtDays.Rows
					Dim dayDate As Date = Date.Parse(drDays("EventDate"))
					Dim dayDatePrevYear As Date = dayDate.AddYears(-1)
					Dim dtPrevYear As DataTable = BRApi.Database.ExecuteSqlUsingReader(dbc, $"Select top(1) * from XFA_CAL_2022 WHERE Store = '{drDays("Store")}' AND EventDay = '{drDays("EventDay")}' AND EventDate >= '{dayDatePrevYear.ToString("yyyy-MM-dd")}' ORDER BY EventDate", False)
					If dtPrevYear.Rows.Count > 0 Then
						Dim drPrevYear As DataRow = dtPrevYear.Rows(0)
						Dim dayDatePrevYearR As Date = Date.Parse(drPrevYear("EventDate"))
						sql.AppendLine($"UPDATE [dbo].[XFA_CAL_2023] SET [Amount] = '{drPrevYear("Amount")}', [Note] = 'From Actual {dayDatePrevYearR.ToString("dd/MM/yyyy")} - {drPrevYear("EventDay")} - {drPrevYear("Store")}' WHERE Store = '{drDays("Store")}' AND EventDate = '{dayDate.ToString("yyyy-MM-dd")}'")
					End If
				Next
'				BRApi.ErrorLog.LogMessage(si, sql.ToString)				
				BRApi.Database.ExecuteActionQuery(dbc, sql.ToString(), False, True)
			End Using
			Return Nothing
			
		End Function
		#End Region
		
		
		
		
		#Region "UpdateHolidaysAmount"
		Public Function UpdateHolidaysAmount(ByVal si As SessionInfo)
			Dim sql As New Text.StringBuilder()
			
			Dim dtEvent As New DataTable()
			Dim listStore As New List(Of String) ({"Barcelona Carrer del Freser 45-47", "Madrid C. de Alberto Aguilera 5"})
			Using dbc = Brapi.Database.CreateApplicationDbConnInfo(si)
				dtEvent = BRApi.Database.ExecuteSqlUsingReader(dbc, "Select * from XFA_CAL_2023_Events", False)
				For Each drEvent As DataRow In dtEvent.Rows
					For Each store As String In listStore
						Dim dtPrevYear As DataTable = BRApi.Database.ExecuteSqlUsingReader(dbc, $"Select top(1) * from XFA_CAL_2022 WHERE EventName = '{drEvent("EventName")}' AND Store = '{store}'", False)
						If dtPrevYear.Rows.Count > 0 Then
							Dim drPrevYear As DataRow = dtPrevYear.Rows(0)
							Dim dayDatePrevYear As Date = Date.Parse(drPrevYear("EventDate"))
							sql.AppendLine($"UPDATE [dbo].[XFA_CAL_2023] SET [Amount] = '{drPrevYear("Amount")}', [Note] = 'From Actual {dayDatePrevYear.ToString("dd/MM/yyyy")} - {drPrevYear("EventDay")} - {drEvent("EventName")} - {store}' WHERE EventName = '{drEvent("EventName")}'  AND Store = '{store}'")
						End If
					Next
				Next
				BRApi.Database.ExecuteActionQuery(dbc, sql.ToString(), False, True)

				
			End Using
			
			Return Nothing
			
		End Function
		#End Region
		
		
		
		#Region "CreateBudget"
		Public Function CreateBudget(ByVal si As SessionInfo)
			Dim sql As New Text.StringBuilder()
			
			Dim startP As DateTime = New DateTime(2023, 1, 1)
			Dim endP As DateTime = New DateTime(startP.Year, 12, 31)
'			Dim CurrD As DateTime = startP
			
			sql.AppendLine($"TRUNCATE TABLE [XFA_CAL_2023]")
			
			Dim listStore As New List(Of String) ({"Barcelona Carrer del Freser 45-47", "Madrid C. de Alberto Aguilera 5"})
			For Each store As String In listStore
				Dim CurrD As DateTime = startP
			
				Dim query As String = String.Empty
				If store.Contains("Barcelona") Then
					query = "Select * from XFA_CAL_2023_Events Where EventType in ('State', 'Catalunya', 'Barcelona','Custom')"
				Else
					query = "Select * from XFA_CAL_2023_Events Where EventType in ('State', 'Madrid', 'Madrid','Custom')"
				End If			
			
				Dim dtEvent As New DataTable()
				Using dbc = Brapi.Database.CreateApplicationDbConnInfo(si)
					dtEvent = BRApi.Database.ExecuteSqlUsingReader(dbc, query, False)
				End Using
				
			
			
				While (CurrD <= endP)        
					Dim random As New Random
					Dim amount As Decimal = 0
					
					Dim EventType As String = String.Empty
					Dim EventName As String = String.Empty
					
					If dtEvent.Select($"EventDate = '{CurrD.ToString("yyyy-MM-dd")}'").Count > 0 Then
						Dim drEvent = dtEvent.Select($"EventDate = '{CurrD.ToString("yyyy-MM-dd")}'").First()
					
						EventType = drEvent("EventType")
						EventName = drEvent("EventName")
					
					End If
				
				
			    	sql.AppendLine($"INSERT INTO [dbo].[XFA_CAL_2023] ([ID],[Entity],[Scenario],[Store],[EventDate],[EventDay],[Amount],[EventType],[EventName],[Note]) VALUES (NEWID(),'Domino','Budget','{store}','{CurrD.ToString("yyyy-MM-dd")}','{CurrD.DayOfWeek.ToString()}',{amount.ToString("G2")},'{EventType}','{EventName}', '')")
			    	CurrD = CurrD.AddDays(1)
				End While
			Next
			
			Using dbc = Brapi.Database.CreateApplicationDbConnInfo(si)
				BRApi.Database.ExecuteActionQuery(dbc, sql.ToString(), False, True)
			End Using
			
			Return Nothing
			
		End Function
		#End Region
		
		
		
		#Region "CreateActual"
		Public Function CreateActual(ByVal si As SessionInfo)
			Dim sql As New Text.StringBuilder()
			
			Dim startP As DateTime = New DateTime(2022, 1, 1)
			Dim endP As DateTime = New DateTime(startP.Year, 12, 31)
'			Dim CurrD As DateTime = startP
			
			sql.AppendLine($"TRUNCATE TABLE [XFA_CAL_2022]")
			
			Dim listStore As New List(Of String) ({"Barcelona Carrer del Freser 45-47", "Madrid C. de Alberto Aguilera 5"})
			For Each store As String In listStore
				Dim CurrD As DateTime = startP
			
				Dim query As String = String.Empty
				If store.Contains("Barcelona") Then
					query = "Select * from XFA_CAL_2022_Events Where EventType in ('State', 'Catalunya', 'Barcelona')"
				Else
					query = "Select * from XFA_CAL_2022_Events Where EventType in ('State', 'Madrid', 'Madrid')"
				End If
				
				Dim dtEvent As New DataTable()
				Using dbc = Brapi.Database.CreateApplicationDbConnInfo(si)
					dtEvent = BRApi.Database.ExecuteSqlUsingReader(dbc, query, False)
				End Using
			
			
			
				While (CurrD <= endP)        
					Dim random As New Random
					Dim amount As Decimal = GetRandom(8600, 15000)
					
					Dim EventType As String = String.Empty
					Dim EventName As String = String.Empty
					
					If dtEvent.Select($"EventDate = '{CurrD.ToString("yyyy-MM-dd")}' ").Count > 0 Then
						Dim drEvent = dtEvent.Select($"EventDate = '{CurrD.ToString("yyyy-MM-dd")}'").First()
						
						EventType = drEvent("EventType")
						EventName = drEvent("EventName")
						
					End If
					
					
				    sql.AppendLine($"INSERT INTO [dbo].[XFA_CAL_2022] ([ID],[Entity],[Scenario],[Store],[EventDate],[EventDay],[Amount],[EventType],[EventName]) VALUES (NEWID(),'Domino','Actual','{store}','{CurrD.ToString("yyyy-MM-dd")}','{CurrD.DayOfWeek.ToString()}',{amount.ToString("G2")},'{EventType}','{EventName}')")
				    CurrD = CurrD.AddDays(1)
				End While
				
				
			Next
'			BRApi.ErrorLog.LogMessage(si, sql.ToString)
			
			Using dbc = Brapi.Database.CreateApplicationDbConnInfo(si)
				BRApi.Database.ExecuteActionQuery(dbc, sql.ToString(), False, True)
			End Using
			
			Return Nothing
			
		End Function
		#End Region
		
		
		
		Public Function GetRandom(ByVal Min As Integer, ByVal Max As Integer) As Integer
		    Static Generator As System.Random = New System.Random()
		    Return Generator.Next(Min, Max)
		End Function
		
		
		
		
		#Region "TruncateActAndBud"
		Public Function TruncateActAndBud(ByVal si As SessionInfo)
			Dim sql As New Text.StringBuilder()
			
			sql.AppendLine($"TRUNCATE TABLE [XFA_CAL_2023];")
			sql.AppendLine($"TRUNCATE TABLE [XFA_CAL_2022];")
			sql.AppendLine($"DROP TABLE [XFA_CAL_2022_Events];")
			sql.AppendLine($"DROP TABLE [XFA_CAL_2023_Events];")
			
'			BRApi.ErrorLog.LogMessage(si, sql.ToString)
			
			Using dbc = Brapi.Database.CreateApplicationDbConnInfo(si)
				BRApi.Database.ExecuteActionQuery(dbc, sql.ToString(), False, True)
			End Using
			
			Return Nothing
			
		End Function
		#End Region
		
		
			#Region "TruncateBud"
		Public Function TruncateBud(ByVal si As SessionInfo)
			Dim sql As New Text.StringBuilder()
			
			sql.AppendLine($"TRUNCATE TABLE [XFA_CAL_2023];")
			Using dbc = Brapi.Database.CreateApplicationDbConnInfo(si)
				BRApi.Database.ExecuteActionQuery(dbc, sql.ToString(), False, True)
			End Using
			
			Return Nothing
			
		End Function
		#End Region
		
		
	
		
		
		
		
		
		
	End Class
End Namespace