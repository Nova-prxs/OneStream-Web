Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports System.Windows.Forms
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database

Namespace OneStream.BusinessRule.Connector.AQS_DWH_iScala
    Public Class MainClass
        '--------------------------------------------------------------------------------------------------------------------
        'Connector rule to retrieve data from DWH
        '
        'This rule creates the connector and SQL query
        '
        'Initial version created by Nicolas ARGENTE (AIQOS) in 2023
        '
        'Updates after go live:
        '<dd-mm-yyyy> - <name> - <Change description> - <line numbers>
        '--------------------------------------------------------------------------------------------------------------------
        Dim ActivateLogs As String 

        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As Object
            Try
				
			
'				ActivateLogs = args.NameValuePairs.XFGetValue("MS_DL_ActivateLog_iScala_AQS_CNT")
				ActivateLogs = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.LV_ActivateLog_iScala_AQS_CNT")
ActivateLogs = "False" 'update temporary				
				
                'Get the query information
                Dim connectionString As String = GetConnectionString(si, globals, api)

                'Get the Field name list or load the data        
                Select Case args.ActionType
                    Case Is = ConnectorActionTypes.GetFieldList
                        'Return Field Name List
                        Return GetFieldListSQL(si)
                    Case Is = ConnectorActionTypes.GetData
                        'Process data (stored procedure)
                        Dim SourceDataSQL As String = GetSourceDataSQL(si, globals, api)
						
						Dim retry As Integer = 0
						Dim retryTotal As Integer = 3
						'SIC seems not to work fine when using multiple connections. The first time you launche it it fails and the second time it works.
						'By using the retry we are launching it a second or third time if it fails.
						While retry <= retryTotal
							Try 
								api.Parser.ProcessSQLQuery(si, DbProviderType.OLEDB, connectionString, True, SourceDataSQL, False, api.ProcessInfo)
								If ActivateLogs = "True" Then brapi.ErrorLog.LogMessage(si, "BR END ProcessSQLQuery")
								Exit While
							Catch 
								retry+= 1
							End Try
						End While
                        Return Nothing
                End Select
                Return Nothing

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function 'Main

#Region "GetConnectionString"
        'Create a Connection string to the External Database
        Private Function GetConnectionString(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer) As String
            Try
                'Named External Connection
                '-----------------------------------------------------------
                'Connection Name
                Dim ConnectorName As String = "CPCLONEU_FINSTGDB01"
                Try
                    ConnectorName = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.LV_ListConnectors_iScala_AQS_CNT")
                Catch
                    'Continue with rest of code. Don't error out.
                End Try
                If ActivateLogs = "True" Then BRApi.ErrorLog.LogMessage(si, "INFO: Connector name is " & ConnectorName)

                Return ConnectorName
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function 'GetConnectionString
#End Region 'GetConnectionString


#Region "GetFieldListSQL"
        'Create the field list SQL Statement

        Private Function GetFieldListSQL(ByVal si As SessionInfo) As List(Of String)
            Try

                Dim fields As New List(Of String)

                'Manually create the list of Fields
                Dim columnNames As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.LV_ColumNames_iScala_AQS_CNT")
				 If ActivateLogs = "True" Then brapi.ErrorLog.LogMessage(si, "iScala columnNames > " & columnNames)

                'Dim columnNames As String() = {"PERIOD", "Entity", "Intercompany", "Account", "Movement", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD8", "Amount"}
				
				' Split the string into an array of strings.
					Dim columnNamesArray As String()
					If Not String.IsNullOrEmpty(columnNames) Then
					    columnNamesArray = columnNames.Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)
					    For i As Integer = 0 To columnNamesArray.Length - 1
					        ' Trim whitespace and remove double quotes from each element
					        columnNamesArray(i) = columnNamesArray(i).Trim().Trim(New Char() {""""c})
					    Next
					Else
					    columnNamesArray = New String() {}  ' or handle the case where the parameter is empty/null
					End If	

                For Each cn In columnNamesArray
                    fields.Add(cn)
                Next

                Return fields

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

#End Region 'GetFieldListSQL


#Region "GetSourceDataSQL"
        'Create the data load SQL Statement
        Private Function GetSourceDataSQL(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer) As String
            'With Optimization 
            Try
				
'				brapi.ErrorLog.LogMessage(si, "iScala api.WorkflowUnitPk.TimeKey >>>>>>>>>>>>" & api.WorkflowUnitPk.TimeKey)
                'Create the SQL Statement 
                Dim wfClusterPK As WorkflowUnitClusterPk = api.WorkflowUnitPk.CreateWorkflowUnitClusterPk 'si.WorkflowClusterPk 'Get current WorkFlow Cluster 
                Dim iTimeID As Integer = wfClusterPK.TimeKey 'Get TimeID from Workflow Cluster
                Dim iYear As String = BRApi.Finance.Time.GetYearFromId(si, iTimeID) 'Get Year from iTimeID
                Dim iPeriod As String = BRApi.Finance.Time.GetPeriodNumFromId(si, iTimeID) 'Get Period from iTimeID
				If iPeriod.Length < 2 Then iPeriod = "0" & iPeriod ' Add 0 to the string iPeriod for the month
                Dim sPeriod As String = iYear & "M" & iPeriod 'string to store period
				
'				Dim wfTime As String = BRApi.Workflow.General.GetGlobalTime(si)
'				Dim sPeriod As String = If(wfTime.Length = 6, wfTime.Insert(5, "0"), str)
				
'				Dim sPeriodAutoload As String =  brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, gWorkspaceID, "LV_ListOfMenuSubDescriptions_AQS_ACC")

                If ActivateLogs = "True" Then BRApi.ErrorLog.LogMessage(si, "INFO : GetSourceDataSQL iScala iTimeID > " & iTimeID & "   sPeriod > " & sPeriod)

                'Get a list of Entities from Text1 property on this Workflow Profile
                Dim GetWFText1 As String = api.WorkflowProfile.GetAttributeValue(ScenarioTypeId.Actual, SharedConstants.WorkflowProfileAttributeIndexes.Text1)

                Dim elements As String() = GetWFText1.Split(New Char() {","c})
                For i As Integer = 0 To elements.Length - 1
                    elements(i) = "'" & elements(i) & "'"
                Next
                Dim EntityCodeForSQL As String = String.Join(", ", elements)
                If ActivateLogs = "True" Then BRApi.ErrorLog.LogMessage(si, "EntityCodeForSQL >  " & EntityCodeForSQL)
				
				
										
						Dim workspaceName As String = "AQS_CNT_Connectors"
						Dim isSystemLevel As Boolean = False
						Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, isSystemLevel, workspaceName)
						
				'If it is the Autoload then it should select a different time potentially and this time is in the parameter
				Dim sPeriodAutoload As String =  brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, guid.Empty, "AQS_100_Consolidation_Parameters.LV_ListOfMenuSubDescriptions_AQS_ACC")		
				If Not String.IsNullOrEmpty(sPeriodAutoload) Then 
					If sPeriodAutoload.Length = 6 Then sPeriodAutoload = sPeriodAutoload.Insert(5, "0")	'In SQL we need M01 and not M1 so we add a 0			
					
'					BRApi.ErrorLog.LogMessage(si, "INFO BEFORE sPeriod >  " & sPeriod & "  sPeriodAutoload >" & sPeriodAutoload)
'					sPeriod = GetEarliestMonth(sPeriodAutoload, sPeriod)
'					BRApi.ErrorLog.LogMessage(si, "INFO AFTER sPeriod >  " & sPeriod & "  sPeriodAutoload >" & sPeriodAutoload)
				End If
					
				Dim sql As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, isSystemLevel, workspaceID, "LV_SQL_iScala_AQS_CNT")

'                Dim sql As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.LV_SQL_iScala_AQS_CNT")
				

                sql = sql.Replace("{EntityCodeForSQL}", EntityCodeForSQL).Replace("{sPeriod}", sPeriod)

				
'                Dim sql As String = $"
'                                    Select TOP 1000
'                                      [PERIOD]
'                                     ,[Entity]
'                                     ,[Intercompany]
'                                     ,[Account]
'                                     ,[Movement]
'                                     ,[UD1]
'                                     ,[UD2]
'                                     ,[UD3]
'                                     ,[UD4]
'                                     ,[UD5]
'                                     ,[UD6]
'                                     ,[UD8]
'                                     ,[Amount]
'                                    From [os].[v_iScalaLineItems]

'                                    Where [Entity] In ({EntityCodeForSQL})	
'                                     And [PERIOD] = '{sPeriod}'	
'                                        "

'----------------------------- Backup -----------------------------
'Column Names:
'PERIOD, Entity, Intercompany, Account, Movement, UD1, UD2, UD3, UD4, UD5, UD6, UD8, Amount, LocalAccount, TransactionNo

'SQL query:
'Select 
'     [PERIOD]
'    ,[Entity]
'    ,[Intercompany]
'    ,[Account]
'    ,[Movement]
'    ,[UD1]
'    ,[UD2]
'    ,[UD3]
'    ,[UD4]
'    ,[UD5]
'    ,[UD6]
'    ,[UD8]
'    ,[LocalAccount]
'    ,[TransactionNo]
'    ,[Amount]
'   From [os].[v_iScalaLineItems]

'   Where [Entity] In ({EntityCodeForSQL}) 
'    And [PERIOD] = '{sPeriod}' 

'----------------------------- Backup -----------------------------



                If ActivateLogs = "True" Then BRApi.ErrorLog.LogMessage(si, "INFO : iScala sql >  " & sql)

				Return sql

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function 'GetSourceDataSQL
#End Region 'GetSourceDataSQL


    End Class
End Namespace