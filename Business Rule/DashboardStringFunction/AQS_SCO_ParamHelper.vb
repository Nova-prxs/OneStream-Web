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

Namespace OneStream.BusinessRule.DashboardStringFunction.AQS_SCO_ParamHelper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try

				If args.FunctionName.XFEqualsIgnoreCase("GetUD4Base") Then
					Dim prm_INP_EntitySelection As String = args.NameValuePairs.XFGetValue("INP_EntitySelection")
					Dim functionName As String = args.NameValuePairs.XFGetValue("Function",String.Empty)
					Dim result As String = Nothing
					If prm_INP_EntitySelection.Equals("Bosnia_Input_OP") And FunctionName.Equals("Cube") Then
						Result = "Cube_200_Bosnia"
						'brapi.ErrorLog.LogMessage(si, Result & " " & "Result")
					End If
					
					Return Result
'					brapi.ErrorLog.LogMessage(si, Result & " " & "Result")
'					Dim functionName As String = args.NameValuePairs.XFGetValue("Function",String.Empty)
'					If FunctionName.Equals("Cube") Then
						
'						brapi.ErrorLog.LogMessage(si, prm_INP_EntitySelection & " " & "Return entity Cube")
					
'					End If

'					brapi.ErrorLog.LogMessage(si, prm_INP_EntitySelection & "Test if reach rule")
					
					End If
				
				If args.FunctionName.XFEqualsIgnoreCase("GetSourceScenario") Then
					'XFBR(AQS_SCO_ParamHelper,GetSourceScenario,MS_BL_SourceWFKey=|!MS_BL_SourceWFKey!|,everyOtherXValues=2)
					Dim sSourceWFKey As String = args.NameValuePairs.XFGetValue("MS_BL_SourceWFKey_AQS_SCO")
					Dim sEveryOtherXValues As String = args.NameValuePairs.XFGetValue("everyOtherXValues_AQS_SCO")

					If String.IsNullOrEmpty(sSourceWFKey) Then
						sSourceWFKey = "8487ffbe-c586-4aae-8bdf-8ff3041170b5"
					End If

					If String.IsNullOrEmpty(sEveryOtherXValues) Then
						sEveryOtherXValues = 1
					End If

					'IMPROVEMENT : 

					'This calls the Table
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Dim OLDsqlStatement As String = $"
							SELECT
							  vStageSourceAndTargetData.Wsk
							 ,Member.Name
							FROM dbo.vStageSourceAndTargetData
							INNER JOIN dbo.WorkflowProfileHierarchy
							  ON vStageSourceAndTargetData.Wfk = WorkflowProfileHierarchy.ProfileKey
							INNER JOIN dbo.Member
							  ON vStageSourceAndTargetData.Wsk = Member.MemberId
							WHERE WorkflowProfileHierarchy.ProfileName = (SELECT TOP 1 ProfileName FROM dbo.WorkflowProfileHierarchy WHERE ProfileKey = '{sSourceWFKey}')
							And Member.Name = Member.Name
							AND Member.DimTypeId = 2
							GROUP BY Member.Name
							        ,vStageSourceAndTargetData.Wsk
							 "
'brapi.ErrorLog.LogMessage(si,"sSourceWFKey>> " & sSourceWFKey & "    sEveryOtherXValues>>> " & sEveryOtherXValues )						
							Dim sqlStatement As String = $"
														
							SELECT
							  s.Wsk,
							  m.Name
							FROM dbo.StageSourceData s
							INNER JOIN dbo.WorkflowProfileHierarchy w
							  ON s.Wfk = w.ProfileKey
							INNER JOIN dbo.Member m
							  ON s.Wsk = m.MemberId
							WHERE w.ProfileName = (
							    SELECT TOP 1 ProfileName
							    FROM dbo.WorkflowProfileHierarchy
							    WHERE ProfileKey = '{sSourceWFKey}'
							)
							AND m.DimTypeId = 2
							GROUP BY m.Name,
							         s.Wsk;

							 "
						
						
						
						
						Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, sqlStatement.ToString, True)
DataTableToString(si, dt, 100)
						Dim sWSK As String
						Dim sName As String
						Dim sList As String = ""

						'This writes the values in the Table to a string
						For Each row As DataRow In dt.Rows
							sWSK = row.Item("wsk")
							sName = row.Item("Name")
							sList = sList & "" & sWSK & "," & "" & sName & ","
						Next
						sList = sList.TrimEnd(New Char() {","c}) ' Remove the trailing comma
						BRApi.ErrorLog.LogMessage(si, "Original List " & sList)
						sList = GetEveryXthValue(si, sList, sEveryOtherXValues)
BRApi.ErrorLog.LogMessage(si, "sqlStatement " & vbCrLf &  sqlStatement)
						BRApi.ErrorLog.LogMessage(si, "New List " & sList)

						Return sList
					End Using
				End If

				If args.FunctionName.XFEqualsIgnoreCase("GetSourceTime") Then
					'XFBR(AQS_SCO_ParamHelper,GetSourceTime,MS_BL_SourceWFKey=|!MS_BL_SourceWFKey!|,everyOtherXValues=2)
					Dim sSourceWFKey As String = args.NameValuePairs.XFGetValue("MS_BL_SourceWFKey_AQS_SCO")
					Dim sEveryOtherXValues As String = args.NameValuePairs.XFGetValue("everyOtherXValues_AQS_SCO")

					If String.IsNullOrEmpty(sSourceWFKey) Then
						sSourceWFKey = "8487ffbe-c586-4aae-8bdf-8ff3041170b5"
					End If

					If String.IsNullOrEmpty(sEveryOtherXValues) Then
						sEveryOtherXValues = 1
					End If

					'IMPROVEMENT : 

					'This calls the Table
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Dim SlowsqlStatement As String = $"
						SELECT
						vStageSourceAndTargetData.Wtk ,
						  LEFT(CAST(vStageSourceAndTargetData.Wtk AS VARCHAR(10)), 4) + 'M' +
						  CASE SUBSTRING(CAST(vStageSourceAndTargetData.Wtk AS VARCHAR(10)), 6, 2)
							WHEN '03' THEN '1'   -- January
							WHEN '04' THEN '2'   -- February
							WHEN '05' THEN '3'   -- March
							WHEN '07' THEN '4'   -- April
							WHEN '08' THEN '5'   -- May
							WHEN '09' THEN '6'   -- June
							WHEN '12' THEN '7'   -- July
							WHEN '13' THEN '8'   -- August
							WHEN '14' THEN '9'   -- September
							WHEN '16' THEN '10'  -- October
							WHEN '17' THEN '11'  -- November
							WHEN '18' THEN '12'  -- December
						  END AS FormattedDate
						FROM dbo.vStageSourceAndTargetData
						INNER JOIN dbo.WorkflowProfileHierarchy
						  ON vStageSourceAndTargetData.Wfk = WorkflowProfileHierarchy.ProfileKey
						INNER JOIN dbo.Member
						  ON vStageSourceAndTargetData.Wsk = Member.MemberId
						WHERE WorkflowProfileHierarchy.ProfileName = (SELECT TOP 1 ProfileName FROM dbo.WorkflowProfileHierarchy WHERE ProfileKey = '{sSourceWFKey}')
						And Member.Name = Member.Name
						AND Member.DimTypeId = 2
						GROUP BY vStageSourceAndTargetData.Wtk
							 "
						
						
												
						Dim sqlStatement As String = $"
							SELECT
							  s.Wtk,
							  LEFT(CAST(s.Wtk AS VARCHAR(10)), 4) + 'M' +
							  CASE SUBSTRING(CAST(s.Wtk AS VARCHAR(10)), 6, 2)
							    WHEN '03' THEN '1'   -- January
							    WHEN '04' THEN '2'   -- February
							    WHEN '05' THEN '3'   -- March
							    WHEN '07' THEN '4'   -- April
							    WHEN '08' THEN '5'   -- May
							    WHEN '09' THEN '6'   -- June
							    WHEN '12' THEN '7'   -- July
							    WHEN '13' THEN '8'   -- August
							    WHEN '14' THEN '9'   -- September
							    WHEN '16' THEN '10'  -- October
							    WHEN '17' THEN '11'  -- November
							    WHEN '18' THEN '12'  -- December
							  END AS FormattedDate
							FROM dbo.StageSourceData s
							INNER JOIN dbo.WorkflowProfileHierarchy w
							  ON s.Wfk = w.ProfileKey
							INNER JOIN dbo.Member m
							  ON s.Wsk = m.MemberId
							WHERE w.ProfileName = (
							  SELECT TOP 1 ProfileName
							  FROM dbo.WorkflowProfileHierarchy
							  WHERE ProfileKey = '{sSourceWFKey}'
							)
							AND m.DimTypeId = 2
							GROUP BY s.Wtk;
							 "
						
						
						
						
						Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, sqlStatement.ToString, True)
						Dim sWTK As String
						Dim sName As String
						Dim sList As String = ""

						'This writes the values in the Table to a string
						For Each row As DataRow In dt.Rows
							sWTK = row.Item("wtk")
							sName = row.Item("FormattedDate")
							sList = sList & "" & sWTK & "," & "" & sName & ","
						Next
						sList = sList.TrimEnd(New Char() {","c}) ' Remove the trailing comma
						BRApi.ErrorLog.LogMessage(si, "Original List " & sList)
						sList = GetEveryXthValue(si, sList, sEveryOtherXValues)

						BRApi.ErrorLog.LogMessage(si, "New List " & sList)

						Return sList
					End Using
				End If


				If args.FunctionName.XFEqualsIgnoreCase("GetDestinationScenario") Then
					'XFBR(AQS_SCO_ParamHelper,GetDestinationScenario,MS_BL_DestinationFKey=|!MS_BL_DestinationWFKey!|,everyOtherXValues=2)
					Dim sDestinationWFKey As String = args.NameValuePairs.XFGetValue("MS_BL_DestinationFKey_AQS_SCO")
					Dim sEveryOtherXValues As String = args.NameValuePairs.XFGetValue("everyOtherXValues_AQS_SCO")

					If String.IsNullOrEmpty(sDestinationWFKey) Then
						sDestinationWFKey = "8487ffbe-c586-4aae-8bdf-8ff3041170b5"
					End If

					If String.IsNullOrEmpty(sEveryOtherXValues) Then
						sEveryOtherXValues = 1
					End If

					'IMPROVEMENT : 

					'This calls the Table
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Dim SlowsqlStatement As String = $"
							
							SELECT
							  vStageSourceAndTargetData.Wsk
							 ,Member.Name
							FROM dbo.vStageSourceAndTargetData
							INNER JOIN dbo.WorkflowProfileHierarchy
							  ON vStageSourceAndTargetData.Wfk = WorkflowProfileHierarchy.ProfileKey
							INNER JOIN dbo.Member
							  ON vStageSourceAndTargetData.Wsk = Member.MemberId
							WHERE WorkflowProfileHierarchy.ProfileName = (SELECT TOP 1 ProfileName FROM dbo.WorkflowProfileHierarchy WHERE ProfileKey = '{sDestinationWFKey}')
							AND Member.Name = Member.Name
							AND Member.DimTypeId = 2
							GROUP BY Member.Name
									,vStageSourceAndTargetData.Wsk


							 "
						
						Dim sqlStatement As String = $"
														
							SELECT
							  s.Wsk,
							  m.Name
							FROM dbo.StageSourceData s
							INNER JOIN dbo.WorkflowProfileHierarchy w
							  ON s.Wfk = w.ProfileKey
							INNER JOIN dbo.Member m
							  ON s.Wsk = m.MemberId
							WHERE w.ProfileName = (
							    SELECT TOP 1 ProfileName
							    FROM dbo.WorkflowProfileHierarchy
							    WHERE ProfileKey = '{sDestinationWFKey}'
							)
							AND m.DimTypeId = 2
							GROUP BY m.Name,
							         s.Wsk;

							 "
						
						
						Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, sqlStatement.ToString, True)
						Dim sWSK As String
						Dim sName As String
						Dim sList As String = ""

						'This writes the values in the Table to a string
						For Each row As DataRow In dt.Rows
							sWSK = row.Item("wsk")
							sName = row.Item("Name")
							sList = sList & "" & sWSK & "," & "" & sName & ","
						Next
						sList = sList.TrimEnd(New Char() {","c}) ' Remove the trailing comma
						BRApi.ErrorLog.LogMessage(si, "Original List " & sList)
						sList = GetEveryXthValue(si, sList, sEveryOtherXValues)

						BRApi.ErrorLog.LogMessage(si, "New List " & sList)

						Return sList
					End Using
				End If

				If args.FunctionName.XFEqualsIgnoreCase("GetDestinationTime") Then
					'XFBR(AQS_SCO_ParamHelper,GetDestinationTime,MS_BL_DestinationWFKey=|!MS_BL_DestinationWFKey!|,everyOtherXValues=2)
					Dim sDestinationWFKey As String = args.NameValuePairs.XFGetValue("MS_BL_DestinationWFKey_AQS_SCO")
					Dim sEveryOtherXValues As String = args.NameValuePairs.XFGetValue("everyOtherXValues_AQS_SCO")

					If String.IsNullOrEmpty(sDestinationWFKey) Then
						sDestinationWFKey = "8487ffbe-c586-4aae-8bdf-8ff3041170b5"
					End If

					If String.IsNullOrEmpty(sEveryOtherXValues) Then
						sEveryOtherXValues = 1
					End If

					'IMPROVEMENT : 

					'This calls the Table
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Dim SlowsqlStatement As String = $"

						SELECT
						vStageSourceAndTargetData.Wtk ,
						  LEFT(CAST(vStageSourceAndTargetData.Wtk AS VARCHAR(10)), 4) + 'M' +
						  CASE SUBSTRING(CAST(vStageSourceAndTargetData.Wtk AS VARCHAR(10)), 6, 2)
							WHEN '03' THEN '1'   -- January
							WHEN '04' THEN '2'   -- February
							WHEN '05' THEN '3'   -- March
							WHEN '07' THEN '4'   -- April
							WHEN '08' THEN '5'   -- May
							WHEN '09' THEN '6'   -- June
							WHEN '12' THEN '7'   -- July
							WHEN '13' THEN '8'   -- August
							WHEN '14' THEN '9'   -- September
							WHEN '16' THEN '10'  -- October
							WHEN '17' THEN '11'  -- November
							WHEN '18' THEN '12'  -- December
						  END AS FormattedDate
						FROM dbo.vStageSourceAndTargetData
						INNER JOIN dbo.WorkflowProfileHierarchy
						  ON vStageSourceAndTargetData.Wfk = WorkflowProfileHierarchy.ProfileKey
						INNER JOIN dbo.Member
						  ON vStageSourceAndTargetData.Wsk = Member.MemberId
						WHERE WorkflowProfileHierarchy.ProfileName =  (SELECT TOP 1 ProfileName FROM dbo.WorkflowProfileHierarchy WHERE ProfileKey = '{sDestinationWFKey}')
						AND Member.Name = Member.Name
						AND Member.DimTypeId = 2
						GROUP BY vStageSourceAndTargetData.Wtk
							 "
						
							Dim sqlStatement As String = $"
							SELECT
							  s.Wtk,
							  LEFT(CAST(s.Wtk AS VARCHAR(10)), 4) + 'M' +
							  CASE SUBSTRING(CAST(s.Wtk AS VARCHAR(10)), 6, 2)
							    WHEN '03' THEN '1'   -- January
							    WHEN '04' THEN '2'   -- February
							    WHEN '05' THEN '3'   -- March
							    WHEN '07' THEN '4'   -- April
							    WHEN '08' THEN '5'   -- May
							    WHEN '09' THEN '6'   -- June
							    WHEN '12' THEN '7'   -- July
							    WHEN '13' THEN '8'   -- August
							    WHEN '14' THEN '9'   -- September
							    WHEN '16' THEN '10'  -- October
							    WHEN '17' THEN '11'  -- November
							    WHEN '18' THEN '12'  -- December
							  END AS FormattedDate
							FROM dbo.StageSourceData s
							INNER JOIN dbo.WorkflowProfileHierarchy w
							  ON s.Wfk = w.ProfileKey
							INNER JOIN dbo.Member m
							  ON s.Wsk = m.MemberId
							WHERE w.ProfileName = (
							  SELECT TOP 1 ProfileName
							  FROM dbo.WorkflowProfileHierarchy
							  WHERE ProfileKey = '{sDestinationWFKey}'
							)
							AND m.DimTypeId = 2
							GROUP BY s.Wtk;
							 "
						
						
						Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, sqlStatement.ToString, True)

						Dim sWTK As String
						Dim sName As String
						Dim sList As String = ""

						'This writes the values in the Table to a string
						For Each row As DataRow In dt.Rows
							sWTK = row.Item("wtk")
							sName = row.Item("FormattedDate")
							sList = sList & "" & sWTK & "," & "" & sName & ","
						Next
						sList = sList.TrimEnd(New Char() {","c}) ' Remove the trailing comma
						BRApi.ErrorLog.LogMessage(si, "Original List " & sList)
						sList = GetEveryXthValue(si, sList, sEveryOtherXValues)

						BRApi.ErrorLog.LogMessage(si, "New List " & sList)

						Return sList
					End Using
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetEveryXthValue(ByVal si As SessionInfo, ByVal inputString As String, ByVal x As Integer) As String
			' Check if the input string is empty or x is less than 1
			If String.IsNullOrEmpty(inputString) OrElse x < 1 Then
				Return String.Empty
			End If

			' Split the string into an array based on the comma separator
			Dim valuesArray() As String = inputString.Split(New Char() {","c})

			' List to hold the selected values
			Dim stridedValues As New List(Of String)

			' Handle the case for x = 1 separately
			If x = 1 Then
				For i As Integer = 0 To valuesArray.Length - 1 Step 2
					stridedValues.Add(valuesArray(i))
				Next
			Else
				' For x > 1, iterate through the array and add every xth value to the list
				For i As Integer = x - 1 To valuesArray.Length - 1 Step x
					stridedValues.Add(valuesArray(i))
				Next
			End If

			' Join the selected values into a single string with a comma separator and return
			Return String.Join(",", stridedValues)
		End Function


		#Region "DataTableToString"


		''Use with options like this 
		'DataTableToString(si, dt, 100)

		Function DataTableToString(ByVal si As SessionInfo, dt As DataTable, Optional totalRows As Integer = 10, Optional delimiter As String = ";") As String
			Dim output As String = ""

			output += "This is the information of the table: " & dt.TableName & vbCrLf & vbCrLf

			output += "Column Count: " & dt.Columns.Count & vbCrLf
			output += "Row Count: " & dt.Rows.Count & vbCrLf & vbCrLf
			output += String.Join(delimiter, dt.Columns.Cast(Of DataColumn).ToList()) & vbCrLf

			If totalRows > 0 Then
				output += String.Join(vbCrLf, dt.Rows.Cast(Of DataRow).Select(Function(r) String.Join(delimiter, r.ItemArray())).Take(totalRows))
			Else
				output += String.Join(vbCrLf, dt.Rows.Cast(Of DataRow).Select(Function(r) String.Join(delimiter, r.ItemArray())))
			End If

			BRApi.ErrorLog.LogMessage(si, output)
			Return Nothing
		End Function

#End Region '"DataTableToString"



	End Class
End Namespace