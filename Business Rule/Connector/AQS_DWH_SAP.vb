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
Imports System.Text.RegularExpressions

Namespace OneStream.BusinessRule.Connector.AQS_DWH_SAP
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

                ActivateLogs = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.LV_ActivateLog_SAP_AQS_CNT")
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
                        'SIC seems not to work fine when using multiple connections. The first time you launch it, it fails and the second time it works.
                        'By using the retry we are launching it a second or third time if it fails.
                        While retry <= retryTotal
                            Try
                                api.Parser.ProcessSQLQuery(si, DbProviderType.OLEDB, connectionString, True, SourceDataSQL, False, api.ProcessInfo)
                                Exit While
                            Catch
                                retry += 1
                            End Try
                        End While
                        Return Nothing
                    Case Is = ConnectorActionTypes.GetDrillBackTypes
                        Return Me.GetDrillBackTypeList(si, globals, api, args)

                    Case Is = ConnectorActionTypes.GetDrillBack
                        Return Me.GetDrillBack(si, globals, api, args)
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
                    ConnectorName = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.LV_ListConnectors_SAP_AQS_CNT")
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
                'Dim columnNames As String() = {"iScalaLineItemsID","PERIOD","Entity","RunningCount","Intercompany","Account","Movement","UD1","UD2","UD3","UD4","UD5","UD6","UD8","Amount","TransactionCurrency","TransactionNo","TransactionText","PLReference","CompanyCode","SourceCompany","InvoiceNumber","Supplier","SupplierName","EntryDate","EntryUser","TransactionDate","LocalAccount","GroupCodeDescription","CompanyTypeDescription","SourceCompanyTypeDescription","CostCenterAllocation","Customer","CustomerName"}

                'Manually create the list of Fields
                Dim columnNames As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.LV_ColumNames_SAP_AQS_CNT")

                If ActivateLogs = "True" Then BRApi.ErrorLog.LogMessage(si, "SAP columnNames > " & columnNames)

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

                'Create the SQL Statement 
                Dim wfClusterPK As WorkflowUnitClusterPk = api.WorkflowUnitPk.CreateWorkflowUnitClusterPk 'Get current WorkFlow Cluster of the user
                Dim iTimeID As Integer = wfClusterPK.TimeKey 'Get TimeID from Workflow Cluster
                Dim iYear As String = BRApi.Finance.Time.GetYearFromId(si, iTimeID) 'Get Year from iTimeID
                Dim iPeriod As String = BRApi.Finance.Time.GetPeriodNumFromId(si, iTimeID) 'Get Period from iTimeID
                If iPeriod.Length < 2 Then iPeriod = "0" & iPeriod ' Add 0 to the string iPeriod for the month
                Dim sPeriod As String = iYear & "M" & iPeriod 'string to store period

                If ActivateLogs = "True" Then BRApi.ErrorLog.LogMessage(si, "INFO : SAP GetSourceDataSQL iTimeID > " & iTimeID & "   sPeriod > " & sPeriod)

                'Get a list of Entities from Text1 property on this Workflow Profile
                Dim GetWFText1 As String = api.WorkflowProfile.GetAttributeValue(ScenarioTypeId.Actual, SharedConstants.WorkflowProfileAttributeIndexes.Text1)

                Dim elements As String() = GetWFText1.Split(New Char() {","c})
                For i As Integer = 0 To elements.Length - 1
                    elements(i) = "'" & elements(i) & "'"
                Next
                Dim EntityCodeForSQL As String = String.Join(", ", elements)
                If ActivateLogs = "True" Then BRApi.ErrorLog.LogMessage(si, "EntityCodeForSQL >  " & EntityCodeForSQL)

                Dim sql As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.LV_SQL_SAP_AQS_CNT")

                sql = sql.Replace("{EntityCodeForSQL}", EntityCodeForSQL).Replace("{sPeriod}", sPeriod)

                If ActivateLogs = "True" Then BRApi.ErrorLog.LogMessage(si, "INFO : SAP sql >  " & sql)

                Return sql

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function 'GetSourceDataSQL
		
'----------------------------- Backup -----------------------------
'Column Names:
'PERIOD, Entity, Intercompany, Account, Movement, CostCenter, WBSElement, UD8, AmountLCU

'SQL Query:
'Select 
'       [PERIOD],
'       [Entity],
'       [Intercompany],
'       [Account],
'       [Movement],
'       [CostCenter],
'       [WBSElement],
'       [UD8],
'       SUM([AmountLCU]) As [AmountLCU]
'From [os].[v_SAPLineItems]
'Where [Entity] In ({EntityCodeForSQL}) 
'  And [PERIOD] = '{sPeriod}'
'Group By 
'       [PERIOD],
'       [Entity],
'       [Intercompany],
'       [Account],
'       [Movement],
'       [CostCenter],
'       [WBSElement],
'       [UD8]
'----------------------------- Backup -----------------------------

#End Region 'GetSourceDataSQL

#Region "GetDrillBackTypeList"
        ' This is a function that populates the drill back types that will be available to the users. At the moment only one type for transactional details.
        Private Function GetDrillBackTypeList(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As List(Of DrillBackTypeInfo)
            Dim drillTypes As New List(Of DrillBackTypeInfo)

            ' Corrected the usage of String.Equals
            If String.Equals(args.DrillCode, StageConstants.TransformationGeneral.DrillCodeDefaultValue, StringComparison.InvariantCultureIgnoreCase) Then
                drillTypes.Add(New DrillBackTypeInfo(ConnectorDrillBackDisplayTypes.DataGrid, New NameAndDesc("TransactionDetail", "Transaction Detail")))
            End If

            Return drillTypes
        End Function
#End Region

#Region "GetDrillBack"        
        ' Function that actually triggers the drill back to source system to pull more data to the stage area
        Private Function GetDrillBack(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As DrillBackResultInfo

		     'Get the query information
             Dim connectionString As String = GetConnectionString(si, globals, api)	
		
            ' Corrected the usage of String.Equals
            If String.Equals(args.DrillBackType.NameAndDescription.Name, "TransactionDetail", StringComparison.InvariantCultureIgnoreCase) Then
                Dim drillBackSQL As String = GetSourceDataSQL_DrillBack(si, globals, api, args)
                Dim drillBackInfo As New DrillBackResultInfo
                drillBackInfo.DisplayType = ConnectorDrillBackDisplayTypes.DataGrid
                drillBackInfo.DataTable = api.Parser.GetXFDataTableForSQLQuery(si,DbProviderType.OLEDB,connectionString,True,drillBackSQL,True,args.PageSize,args.PageNumber)
'				api.Parser.ProcessSQLQuery(si, DbProviderType.OLEDB, connectionString, True, SourceDataSQL, False, api.ProcessInfo)
                Return drillBackInfo
            Else
                Return Nothing
            End If

        End Function
#End Region

#Region "GetSourceDataSQL_DrillBack"
    'Create the data load SQL Statement
    Private Function GetSourceDataSQL_DrillBackOLD(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As String
        Try
            ' Remove everything after GROUP BY (if it exists)
            Dim sourceValues As Dictionary(Of String, Object) = api.Parser.GetFieldValuesForSourceDataRow(si, args.RowID)
            Dim SQL_E As String = noneToEmpty(sourceValues.Item(StageConstants.MasterDimensionNames.Entity).ToString) 'Entity
            Dim SQL_A As String = noneToEmpty(sourceValues.Item(StageConstants.MasterDimensionNames.Attribute7).ToString) 'ACCOUNT
            Dim SQL_F As String = noneToEmpty(sourceValues.Item(StageConstants.MasterDimensionNames.Flow).ToString) 'Movement
            Dim SQL_4 As String = noneToEmpty(sourceValues.Item(StageConstants.MasterDimensionNames.Attribute1).ToString) 'Costcenter
            Dim SQL_5 As String = noneToEmpty(sourceValues.Item(StageConstants.MasterDimensionNames.Attribute5).ToString) 'WBSElement
            Dim SQL_8 As String = noneToEmpty(sourceValues.Item(StageConstants.MasterDimensionNames.Attribute8).ToString) 'UD8

            'Convert the OneStream time member to the BIReports time member for the WHERE Clause (BIREPORTS Format = '201401')
            Dim year As String = TimeDimHelper.GetYearFromId(sourceValues.Item(StageTableFields.StageSourceData.DimWorkflowTimeKey).ToString)
            Dim month As String = TimeDimHelper.GetSubComponentsFromId(sourceValues.Item(StageTableFields.StageSourceData.DimWorkflowTimeKey)).Month.ToString
            If month.Length = 1 Then month = "0" & month
            Dim sPeriod As String = year & "M" & month

            ' Define the base SQL statement
            Dim sql As String = $"
SELECT 
       [PERIOD],
       [Entity],
       [Intercompany],
       [Account],
       [Movement],
       [CostCenter],
       [WBSElement],
       [UD8],
       [AmountLCU]
FROM [os].[v_SAPLineItems]"

            ' Initialize the WHERE clause with mandatory conditions
            Dim whereClause As String = $"WHERE [Entity] = '{SQL_E}' AND [PERIOD] = '{sPeriod}'"



            ' Combine the SQL statement with the dynamically built WHERE clause
            sql &= vbCrLf & whereClause
			brapi.ErrorLog.LogMessage(si, "DrillBack sql:: " & sql)
            Return sql

        Catch ex As Exception
            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
        End Try
    End Function 'GetSourceDataSQL_DrillBack
#End Region 'GetSourceDataSQL_DrillBack
 
#Region "GetSourceDataSQL_DrillBack"
''' <summary>
''' Creates the data load SQL statement.
''' </summary>
''' <param name="si">Session information.</param>
''' <param name="globals">Global configurations.</param>
''' <param name="api">Transformer API.</param>
''' <param name="args">Connector arguments.</param>
''' <returns>SQL string with dynamic WHERE conditions.</returns>
Private Function GetSourceDataSQL_DrillBack(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As String
    Try
        ' Retrieve field values for the source data row
        Dim sourceValues As Dictionary(Of String, Object) = api.Parser.GetFieldValuesForSourceDataRow(si, args.RowID)
        Dim SQL_E As String = sourceValues.Item(StageConstants.MasterDimensionNames.Entity).ToString ' Entity
        Dim SQL_A As String = sourceValues.Item(StageConstants.MasterDimensionNames.Attribute7).ToString ' Account
        Dim SQL_F As String = sourceValues.Item(StageConstants.MasterDimensionNames.Flow).ToString ' Movement
        Dim SQL_4 As String = sourceValues.Item(StageConstants.MasterDimensionNames.Attribute1).ToString ' CostCenter
        Dim SQL_5 As String = sourceValues.Item(StageConstants.MasterDimensionNames.Attribute5).ToString ' WBSElement
        Dim SQL_8 As String = sourceValues.Item(StageConstants.MasterDimensionNames.Attribute8).ToString ' UD8

        ' Convert time member to BIReports format
        Dim year As String = TimeDimHelper.GetYearFromId(sourceValues.Item(StageTableFields.StageSourceData.DimWorkflowTimeKey).ToString)
        Dim month As String = TimeDimHelper.GetSubComponentsFromId(sourceValues.Item(StageTableFields.StageSourceData.DimWorkflowTimeKey)).Month.ToString
        If month.Length = 1 Then month = "0" & month
        Dim sPeriod As String = year & "M" & month

        ' Define the base SQL statement
        Dim sql As String = $"
SELECT 
       [PERIOD],
       [Entity],
       [Intercompany],
       [Account],
       [Movement],
       [CostCenter],
       [WBSElement],
       [UD8],
       [JournalEntry],
       [PostingKey],
       [NumberofItems],
       [AmountLCU]
FROM [os].[v_SAPLineItems]"

        ' Build the WHERE clause
        Dim whereClause As String = $"
WHERE 
       [Entity] = '{SQL_E}' 
       AND [PERIOD] = '{sPeriod}' 
       AND ISNULL(NULLIF([Account], ''), '') = '{SQL_A}' 
       AND ISNULL(NULLIF([Movement], ''), '') = '{SQL_F}' 
       AND ISNULL(NULLIF([CostCenter], ''), '') = '{SQL_4}' 
       AND ISNULL(NULLIF([WBSElement], ''), '') = '{SQL_5}' 
       AND ISNULL(NULLIF([UD8], ''), '') = '{SQL_8}'"

        ' Combine the SQL statement with the WHERE clause
        sql &= vbCrLf & whereClause

        ' Log the generated SQL for debugging
        ' brapi.ErrorLog.LogMessage(si, $"DrillBack SQL: {sql}")
        Return sql

    Catch ex As Exception
        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
    End Try
End Function
#End Region 'GetSourceDataSQL_DrillBack



        Private Function noneToEmpty(ByVal incomingValue As String) As String
            If incomingValue = "None" Then
                incomingValue = ""
            Else
                incomingValue = incomingValue
            End If
            Return incomingValue
        End Function

    End Class
End Namespace