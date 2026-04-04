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

Namespace OneStream.BusinessRule.Connector.AQS_DWH
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
        Dim ActivateLogs = True

        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As Object
            Try
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
                        BRApi.ErrorLog.LogMessage(si, "SourceDataSQL >>  " & SourceDataSQL)
                        api.Parser.ProcessSQLQuery(si, DbProviderType.OLEDB, connectionString, True, SourceDataSQL, False, api.ProcessInfo)
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
'                Try
'                    ConnectorName = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.MS_BL_ListConnectors_AQS_CNT")
'                Catch
'                    'Continue with rest of code. Don't error out.
'                End Try
                If ActivateLogs = True Then BRApi.ErrorLog.LogMessage(si, "INFO: Connector name is " & ConnectorName)

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
                Dim columnNames As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.IV_ColumNames_iScala_AQS_CNT")
                'Dim columnNames As String() = {"PERIOD", "Entity", "Intercompany", "Account", "Movement", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD8", "Amount"}
                If ActivateLogs = True Then BRApi.ErrorLog.LogMessage(si, columnNames.ToString)
                For Each cn In columnNames
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
                'Create the SQL Statement 
                Dim wfClusterPK As WorkflowUnitClusterPk = si.WorkflowClusterPk 'Get current WorkFlow Cluster 
                Dim iTimeID As Integer = wfClusterPK.TimeKey 'Get TimeID from Workflow Cluster
                Dim iYear As String = BRApi.Finance.Time.GetYearFromId(si, iTimeID) 'Get Year from iTimeID
                Dim iPeriod As String = BRApi.Finance.Time.GetPeriodNumFromId(si, iTimeID) 'Get Period from iTimeID
                Dim sPeriod As String = iYear & "M" & iPeriod 'string to store period

                If ActivateLogs = True Then BRApi.ErrorLog.LogMessage(si, "iTimeID > " & iTimeID & "   sPeriod > " & sPeriod)

                'Get a list of Entities from Text1 property on this Workflow Profile
                Dim GetWFText1 As String = api.WorkflowProfile.GetAttributeValue(ScenarioTypeId.Actual, SharedConstants.WorkflowProfileAttributeIndexes.Text1)

                Dim elements As String() = GetWFText1.Split(New Char() {","c})
                For i As Integer = 0 To elements.Length - 1
                    elements(i) = "'" & elements(i) & "'"
                Next
                Dim EntityCodeForSQL As String = String.Join(", ", elements)
                If ActivateLogs = True Then BRApi.ErrorLog.LogMessage(si, "EntityCodeForSQL >  " & EntityCodeForSQL)

'                Dim sql As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.IV_SQL_iScala_AQS_CNT")

'                sql = sql.Replace("{EntityCodeForSQL}", EntityCodeForSQL).Replace("{sPeriod}", sPeriod)

				
                Dim sql As String = $"
                                    Select TOP 1000
                                      [PERIOD]
                                     ,[Entity]
                                     ,[Intercompany]
                                     ,[Account]
                                     ,[Movement]
                                     ,[UD1]
                                     ,[UD2]
                                     ,[UD3]
                                     ,[UD4]
                                     ,[UD5]
                                     ,[UD6]
                                     ,[UD8]
                                     ,[Amount]
                                    From [os].[v_iScalaLineItems]

                                    Where [Entity] In ({EntityCodeForSQL})	
                                     And [PERIOD] = '{sPeriod}'	
                                        "
                If ActivateLogs = True Then BRApi.ErrorLog.LogMessage(si, "sql >  " & sql)

#Region "Helper Code"
                                If ActivateLogs = True Then
                                    BRApi.ErrorLog.LogMessage(si, sql)

                                    Using dbConn As DbConnInfo = BRApi.Database.CreateExternalDbConnInfo(si, "CPCLONEU_FINSTGDB01")
                                        Dim dt As DataTable = DbSql.GetDataTableUsingReader(dbConn, sql.ToString, Nothing, True)


                                        '----------------------------------------------------
                                        'HELPER CODE!!!
                                        '----------------------------------------------------
                                        'Write data table data To log.

                                        Dim sRow4 As Object = "dt Rows: "
                                        Dim rowCount As Integer = 2
                                        Dim i As Integer = 1
                                        For Each row As DataRow In dt.Rows
                                            For Each cell As Object In row.ItemArray
                                                sRow4 += cell.ToString & ","
                                            Next
                                            sRow4 += vbCrLf
                                            If i = rowCount Then
                                                Exit For
                                            End If
                                        Next
                                        BRApi.ErrorLog.LogMessage(si, sRow4.TrimEnd(","))



                                        'Write column names To log.
                                        Dim sCol4 As Object = "dt Columns: "
                                        For Each column As DataColumn In dt.Columns
                                            sCol4 += column.ColumnName & ","
                                        Next
                                        BRApi.ErrorLog.LogMessage(si, sCol4.TrimEnd(","))

                                        ' Check if the DataTable is not Nothing and has rows
                                        If dt.Rows.Count < 1 Then
                                            ' DataTable is initialized and has data
                                            brapi.ErrorLog.LogMessage(si,"ERROR : There are no data in the source for this entity")
                                        End If

                                    End Using
                                End If
#End Region

                Return sql

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function 'GetSourceDataSQL
#End Region 'GetSourceDataSQL



    End Class
End Namespace