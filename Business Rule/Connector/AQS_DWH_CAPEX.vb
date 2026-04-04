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

Namespace OneStream.BusinessRule.Connector.AQS_DWH_CAPEX
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

				ActivateLogs = "False" ' BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.LV_ActivateLog_CAPEX_AQS_CNT")
				
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
                Try
                    ConnectorName = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.LV_ListConnectors_CAPEX_AQS_CNT")
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
                Dim columnNames As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.LV_ColumNames_CAPEX_AQS_CNT")
				
				If ActivateLogs = "True" Then brapi.ErrorLog.LogMessage(si, "SAP columnNames > " & columnNames)

'                Dim columnNames As String() = {"PERIOD", "Entity", "Intercompany", "Account", "Movement", "CostCenter", "WBSElement", "UD8", "AmountLCU"}
				
				
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
                Dim wfClusterPK As WorkflowUnitClusterPk = api.WorkflowUnitPk.CreateWorkflowUnitClusterPk
                Dim iTimeID As Integer = wfClusterPK.TimeKey 'Get TimeID from Workflow Cluster
                Dim iYear As String = BRApi.Finance.Time.GetYearFromId(si, iTimeID) 'Get Year from iTimeID
                Dim iPeriod As String = BRApi.Finance.Time.GetPeriodNumFromId(si, iTimeID) 'Get Period from iTimeID
				If iPeriod.Length < 2 Then iPeriod = "0" & iPeriod ' Add 0 to the string iPeriod for the month
                Dim sPeriod As String = iYear & "M" & iPeriod 'string to store period

                If ActivateLogs = "True" Then BRApi.ErrorLog.LogMessage(si, "INFO : GetSourceDataSQL iTimeID > " & iTimeID & "   sPeriod > " & sPeriod)

                'Get a list of Entities from Text1 property on this Workflow Profile
                Dim GetWFText1 As String = api.WorkflowProfile.GetAttributeValue(ScenarioTypeId.Actual, SharedConstants.WorkflowProfileAttributeIndexes.Text1)

'                Dim elements As String() = GetWFText1.Split(New Char() {","c})
'                For i As Integer = 0 To elements.Length - 1
'                    elements(i) = "'" & elements(i) & "'"
'                Next
'                Dim sCountry As String = GetWFText1 & "%"'"Spain"String.Join(", ", elements) 'Commented out by DAGO on 20250127
                Dim elements As String() = GetWFText1.Split(New Char() {","c})
                For i As Integer = 0 To elements.Length - 1
                    elements(i) = "'" & elements(i) & "'"
                Next
                Dim sCountry As String = String.Join(", ", elements)
'                If ActivateLogs = "True" Then BRApi.ErrorLog.LogMessage(si, "EntityCodeForSQL >  " & EntityCodeForSQL)

                Dim sql As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "AQS_CNT_Connectors.LV_SQL_CAPEX_AQS_CNT")
 
'				Dim sql As String
'				Using DBConInfo, connect To SQLDB And retrieve latest TOP1 value From column SQL.
				
				If ActivateLogs = "True" Then BRApi.ErrorLog.LogMessage(si, "sql >  " & sql)
                sql = sql.Replace("{sCountry}", sCountry).Replace("{sPeriod}", sPeriod)
				If ActivateLogs = "True" Then BRApi.ErrorLog.LogMessage(si, "sql 2 >  " & sql)
#Region "Original SQL"
'	                Dim sql As String = $"
'										SELECT TOP 1000
'										       [PERIOD]
'										      ,[Entity]
'										      ,[Intercompany]
'										      ,[Account]
'										      ,[Movement]
'										      ,[CostCenter]
'										      ,[WBSElement]
'										      ,[UD8]
'										      ,[AmountLCU]
'										FROM [os].[v_SAPLineItems]
'										   WHERE [Entity] IN ('PT13', 'PT14', 'PT15', 'PT16', 'PT20') 
'										    AND [PERIOD] = '2023M12'

'                                        "

'----------------------------- Backup -----------------------------

'Column Names
'PERIOD, Entity, Intercompany, Account, Movement, UD1, UD2, UD3, UD4, UD5, UD6, UD8, Amount

'SQL Query
'Select [Country]
'     , [Period]
'     , [Commodity]
'     , [PMC]
'     , ROUND(ISNULL([Amount System LCU], 0) + ISNULL([Amount Adjustment LCU], 0), 0) As [Total_Amount_LCU]
'From [os].[v_CAPEX]
'   Where [Country] In ({sCountry}) 
'    And [PERIOD] = '{sPeriod}' 
'Order By 1, 2, 3

'----------------------------- Backup -----------------------------

                If ActivateLogs = "True" Then BRApi.ErrorLog.LogMessage(si, "sql >  " & sql)
#End Region 'Original SQL
				
'                                    Where [Entity] In ({EntityCodeForSQL})	
'                                     And [PERIOD] = '{sPeriod}'	


                Return sql

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function 'GetSourceDataSQL
#End Region 'GetSourceDataSQL

    End Class
End Namespace