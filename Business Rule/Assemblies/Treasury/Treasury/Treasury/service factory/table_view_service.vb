Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Reflection
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

    #Region "TableView Configuration"
    ''' <summary>
    ''' Configuration class for TableView definitions
    ''' </summary>
    Public Class TableViewConfig
        Public Property BusinessRuleTypeName As String
        Public Property SubstitutionVars As List(Of String)
        Public Property IsSaveable As Boolean
        
        Public Sub New(brTypeName As String, substVars As List(Of String), Optional saveable As Boolean = False)
            Me.BusinessRuleTypeName = brTypeName
            Me.SubstitutionVars = substVars
            Me.IsSaveable = saveable
        End Sub
    End Class
    #End Region

    Public Class table_view_service
        Implements IWsasTableViewV800

        #Region "Static Configuration"
        ' Common substitution variable patterns
        Private Shared ReadOnly VARS_YEAR_WEEK As List(Of String) = New List(Of String)(New String() {"prm_Treasury_Year", "prm_Treasury_WeekNumber"})
        Private Shared ReadOnly VARS_COMPANY_YEAR_WEEK As List(Of String) = New List(Of String)(New String() {"prm_Treasury_CompanyNames", "prm_Treasury_Year", "prm_Treasury_WeekNumber"})
        Private Shared ReadOnly VARS_COMPANY_YEAR_WEEK_RANGE As List(Of String) = New List(Of String)(New String() {"prm_Treasury_CompanyNames", "prm_Treasury_Year", "prm_Treasury_WeekNumber_From", "prm_Treasury_WeekNumber_To"})
        Private Shared ReadOnly VARS_COMPANY_ONLY As List(Of String) = New List(Of String)(New String() {"prm_Treasury_CompanyNames"})

        ''' <summary>
        ''' Master configuration dictionary mapping TableView names to their Business Rule types and substitution variables
        ''' Initialized in shared constructor for VB.NET compatibility
        ''' </summary>
        Private Shared ReadOnly TableViewRegistry As Dictionary(Of String, TableViewConfig) = InitializeTableViewRegistry()
        
        Private Shared Function InitializeTableViewRegistry() As Dictionary(Of String, TableViewConfig)
            Dim registry As New Dictionary(Of String, TableViewConfig)(StringComparer.OrdinalIgnoreCase)
            
            ' CashDebtPosition Reports - Global/HTD (Year + Week only)
            registry.Add("CashDebtPosition_Actual_Global", New TableViewConfig("TRS_CashDebtPosition_CashFinanceBalance_Global", VARS_YEAR_WEEK))
            registry.Add("CashDebtPosition_EOM_Global", New TableViewConfig("TRS_CashDebtPosition_CashFinanceBalance_Global", VARS_YEAR_WEEK))
            registry.Add("CashDebtPosition_EndWeek_Global", New TableViewConfig("TRS_CashDebtPosition_CashFinanceBalance_Global", VARS_YEAR_WEEK))
            registry.Add("CashDebtPosition_HTD", New TableViewConfig("TRS_CashDebtPosition_CashFinanceBalance_HTD", VARS_YEAR_WEEK))
            registry.Add("CashDebtPosition_CashFinanceBalance_HTD_StartWeek", New TableViewConfig("TRS_CashDebtPosition_CashFinanceBalance_HTD_StartWeek", VARS_YEAR_WEEK))
            registry.Add("CashDebtPosition_CashFinanceBalance_HTD_EOW", New TableViewConfig("TRS_CashDebtPosition_CashFinanceBalance_HTD_EOW", VARS_YEAR_WEEK))
            registry.Add("CashDebtPosition_CashFinanceBalance_HTD_EOM", New TableViewConfig("TRS_CashDebtPosition_CashFinanceBalance_HTD_EOM", VARS_YEAR_WEEK))
            registry.Add("CashDebtPositionGlobal", New TableViewConfig("TRS_CashDebtPosition_CashFinanceBalance_Global", VARS_YEAR_WEEK))
            registry.Add("CashDebtPositionHeader_Global", New TableViewConfig("TRS_CashDebtPositionHeader_Global", VARS_YEAR_WEEK))
            registry.Add("CashDebtPositionHeader_HTD", New TableViewConfig("TRS_CashDebtPositionHeader_HTD", VARS_YEAR_WEEK))
            
            ' CashDebtPosition Reports - Entity-specific (Company + Year + Week)
            registry.Add("CashDebtPosition_CashFinancingBalance", New TableViewConfig("TRS_CashDebtPosition_CashFinancingBalance", VARS_COMPANY_YEAR_WEEK, True))
            registry.Add("CashDebtPosition_CashFinancingBalance_StartWeek", New TableViewConfig("TRS_CashDebtPosition_CashFinanceBalance_StartWeek", VARS_COMPANY_YEAR_WEEK))
            registry.Add("CashDebtPosition_CashFinancingBalance_EOM", New TableViewConfig("TRS_CashDebtPosition_CashFinanceBalance_EOM", VARS_COMPANY_YEAR_WEEK))
            registry.Add("CashDebtPositionHeader", New TableViewConfig("TRS_CashDebtPositionHeader", VARS_COMPANY_YEAR_WEEK))
            
            ' CashDebtPosition Reports - Week range
            registry.Add("CashDebtPosition_Weeks", New TableViewConfig("TRS_CashDebtPosition_Weeks", VARS_COMPANY_YEAR_WEEK_RANGE))
            
            ' CashFlowForecasting Reports - Global/HTD (Year + Week only)
            registry.Add("CashFlowForecasting_CF_CB_Global", New TableViewConfig("TRS_CashFlowForecasting_CF_CB_Global", VARS_YEAR_WEEK))
            registry.Add("CashFlowForecasting_CF_CB_Global_Average", New TableViewConfig("TRS_CashFlowForecasting_CF_CB_Global_Average", VARS_YEAR_WEEK))
            registry.Add("CashFlowForecasting_OF_IF_Global", New TableViewConfig("TRS_CashFlowForecasting_OF_IF_Global", VARS_YEAR_WEEK))
            registry.Add("CashFlowForecasting_OF_IF_Global_Average", New TableViewConfig("TRS_CashFlowForecasting_OF_IF_Global_Average", VARS_YEAR_WEEK))
            registry.Add("CashFlowForecasting_CF_CB_HTD", New TableViewConfig("TRS_CashFlowForecasting_CF_CB_HTD", VARS_YEAR_WEEK))
            registry.Add("CashFlowForecasting_CF_CB_HTD_Average", New TableViewConfig("TRS_CashFlowForecasting_CF_CB_HTD_Average", VARS_YEAR_WEEK))
            registry.Add("CashFlowForecasting_OF_IF_HTD", New TableViewConfig("TRS_CashFlowForecasting_OF_IF_HTD", VARS_YEAR_WEEK))
            registry.Add("CashFlowForecasting_OF_IF_HTD_Average", New TableViewConfig("TRS_CashFlowForecasting_OF_IF_HTD_Average", VARS_YEAR_WEEK))
            registry.Add("CashFlowForecasting_HTD_Summary", New TableViewConfig("TRS_CashflowForecasting_HTD_Summary", VARS_YEAR_WEEK))
            registry.Add("CashFlowForecasting_Global_Summary", New TableViewConfig("TRS_CashflowForecasting_Global_Summary", VARS_YEAR_WEEK))
            
            ' CashFlowForecasting Reports - Entity-specific (Company + Year + Week)
            registry.Add("CashFlowForecasting", New TableViewConfig("TRS_CashFlowForecasting", VARS_COMPANY_YEAR_WEEK, True))
            registry.Add("CashFlowForecasting_4Week", New TableViewConfig("TRS_CashFlowForecasting4Week", VARS_COMPANY_YEAR_WEEK))
            registry.Add("CashFlowForecastingHeader", New TableViewConfig("TRS_CashFlowForecastingHeader", VARS_COMPANY_YEAR_WEEK))
            
            ' Treasury Monitoring Reports
            registry.Add("TreasuryMonitoring", New TableViewConfig("TRS_Treasury_Monitoring", VARS_COMPANY_YEAR_WEEK, True))
            registry.Add("TreasuryMonitoring_report", New TableViewConfig("TRS_Treasury_Monitoring_Report", VARS_YEAR_WEEK, True))
            registry.Add("TreasuryMonitoringHeader", New TableViewConfig("TRS_Treasury_Monitoring_Header", VARS_COMPANY_ONLY))
            
            Return registry
        End Function
        #End Region
        ''' <summary>
        ''' Returns the custom substitution variables needed for the specified TableView
        ''' Uses dictionary-based lookup for maintainability
        ''' </summary>
        Public Function TableViewGetCustomSubstVarsInUse(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal workspace As DashboardWorkspace, _
            ByVal tableViewName As String, ByVal custSubstVarsAlreadyResolved As Dictionary(Of String, String)) _
            As List(Of String) Implements IWsasTableViewV800.TableViewGetCustomSubstVarsInUse
            Try
                If (globals IsNot Nothing) AndAlso (workspace IsNot Nothing) Then
                    ' Look up configuration from registry
                    Dim config As TableViewConfig = Nothing
                    If TableViewRegistry.TryGetValue(tableViewName, config) Then
                        Return New List(Of String)(config.SubstitutionVars)
                    End If
                End If
                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function
        ''' <summary>
        ''' Gets a TableView by looking up the configuration and invoking the appropriate Business Rule
        ''' Uses dictionary-based lookup for maintainability
        ''' </summary>
        Public Function GetTableView(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal workspace As DashboardWorkspace, _
            ByVal tableViewName As String, ByVal custSubstVarsAlreadyResolved As Dictionary(Of String, String), _
            ByVal nameValuePairs As Dictionary(Of String, String)) As TableView Implements IWsasTableViewV800.GetTableView
            Try
                If (globals IsNot Nothing) AndAlso (workspace IsNot Nothing) Then
                    ' Look up configuration from registry
                    Dim config As TableViewConfig = Nothing
                    If TableViewRegistry.TryGetValue(tableViewName, config) Then
                        Return Me.GetTableViewByConfig(si, globals, config.BusinessRuleTypeName, custSubstVarsAlreadyResolved)
                    End If
                End If
                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function
        ''' <summary>
        ''' Saves TableView changes by looking up the configuration and invoking the appropriate Business Rule
        ''' Uses dictionary-based lookup for maintainability
        ''' </summary>
        Public Function SaveTableView(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal workspace As DashboardWorkspace, _
            ByVal tableViewName As String, ByVal tableView As TableView) As Boolean Implements IWsasTableViewV800.SaveTableView
            Try
                If (globals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (tableView IsNot Nothing) Then
                    ' Look up configuration from registry
                    Dim config As TableViewConfig = Nothing
                    If TableViewRegistry.TryGetValue(tableViewName, config) Then
                        If config.IsSaveable Then
                            Return Me.SaveTableViewByConfig(si, globals, config.BusinessRuleTypeName, tableView)
                        End If
                    End If
                End If                
                Return False
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function
        #Region "Generic TableView Handlers"
        ''' <summary>
        ''' Generic method to get a TableView by invoking the appropriate Business Rule
        ''' Uses reflection to instantiate the Business Rule type dynamically
        ''' </summary>
        ''' <param name="si">Session information</param>
        ''' <param name="globals">BR globals</param>
        ''' <param name="brTypeName">Business Rule type name (e.g., "TRS_CashDebtPositionHeader")</param>
        ''' <param name="custSubstVarsAlreadyResolved">Custom substitution variables</param>
        ''' <returns>TableView from the Business Rule, or empty TableView on error</returns>
        Private Function GetTableViewByConfig(ByVal si As SessionInfo, ByVal globals As BRGlobals, _
            ByVal brTypeName As String, ByVal custSubstVarsAlreadyResolved As Dictionary(Of String, String)) As TableView
            Try
                ' Build the full type name for the spreadsheet business rule
                Dim fullTypeName As String = $"Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.{brTypeName}.MainClass"
                
                ' Get the type using reflection
                Dim brType As Type = Type.GetType(fullTypeName, throwOnError:=False)
                If brType Is Nothing Then
                    Return Me.CreateEmptyTableView($"Business Rule type not found: {brTypeName}")
                End If
                
                ' Create an instance of the Business Rule
                Dim brInstance As Object = Activator.CreateInstance(brType)
                If brInstance Is Nothing Then
                    Return Me.CreateEmptyTableView($"Failed to instantiate Business Rule: {brTypeName}")
                End If
                
                ' Create SpreadsheetArgs to call the business rule
                Dim spreadsheetArgs As New SpreadsheetArgs()
                spreadsheetArgs.FunctionType = SpreadsheetFunctionType.GetTableView
                spreadsheetArgs.CustSubstVarsAlreadyResolved = custSubstVarsAlreadyResolved
                
                ' Get the Main method
                Dim mainMethod As MethodInfo = brType.GetMethod("Main")
                If mainMethod Is Nothing Then
                    Return Me.CreateEmptyTableView($"Main method not found in Business Rule: {brTypeName}")
                End If
                
                ' Invoke the Main method
                Dim result As Object = mainMethod.Invoke(brInstance, New Object() {si, globals, Nothing, spreadsheetArgs})
                
                ' Cast and return the TableView
                If TypeOf result Is TableView Then
                    Return DirectCast(result, TableView)
                Else
                    Return Me.CreateEmptyTableView("No data available")
                End If
            Catch ex As Exception
                Return Me.CreateEmptyTableView($"Error: {ex.Message}")
            End Try
        End Function

        ''' <summary>
        ''' Generic method to save a TableView by invoking the appropriate Business Rule
        ''' Uses reflection to instantiate the Business Rule type dynamically
        ''' </summary>
        ''' <param name="si">Session information</param>
        ''' <param name="globals">BR globals</param>
        ''' <param name="brTypeName">Business Rule type name (e.g., "TRS_CashFlowForecasting")</param>
        ''' <param name="tableView">The TableView to save</param>
        ''' <returns>True if save was successful, False otherwise</returns>
        Private Function SaveTableViewByConfig(ByVal si As SessionInfo, ByVal globals As BRGlobals, _
            ByVal brTypeName As String, ByVal tableView As TableView) As Boolean
            Try
                ' Build the full type name for the spreadsheet business rule
                Dim fullTypeName As String = $"Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.{brTypeName}.MainClass"
                
                ' Get the type using reflection
                Dim brType As Type = Type.GetType(fullTypeName, throwOnError:=False)
                If brType Is Nothing Then
                    Return False
                End If
                
                ' Create an instance of the Business Rule
                Dim brInstance As Object = Activator.CreateInstance(brType)
                If brInstance Is Nothing Then
                    Return False
                End If
                
                ' Create SpreadsheetArgs to call the save function
                Dim spreadsheetArgs As New SpreadsheetArgs()
                spreadsheetArgs.FunctionType = SpreadsheetFunctionType.SaveTableView
                spreadsheetArgs.TableView = tableView
                
                ' Get the Main method
                Dim mainMethod As MethodInfo = brType.GetMethod("Main")
                If mainMethod Is Nothing Then
                    Return False
                End If
                
                ' Invoke the Main method
                Dim result As Object = mainMethod.Invoke(brInstance, New Object() {si, globals, Nothing, spreadsheetArgs})
                
                ' Return the boolean result
                If TypeOf result Is Boolean Then
                    Return DirectCast(result, Boolean)
                Else
                    Return False
                End If
            Catch ex As Exception
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Creates an empty table view with an error message
        ''' </summary>
        Private Function CreateEmptyTableView(ByVal message As String) As TableView
            Try
                Dim tv As New TableView()
                tv.CanModifyData = False
                ' Add a single column
                tv.Columns.Add(New TableViewColumn() With {.Name = "Message", .Value = "Message", .IsHeader = True})
                ' Add the error message row
                Dim row As New TableViewRow()
                row.Items.Add("Message", New TableViewColumn() With {.Value = message, .IsHeader = False})
                tv.Rows.Add(row)
                Return tv
            Catch ex As Exception
                ' If we can't even create an empty table view, throw the exception
                Throw ex
            End Try
        End Function
        #End Region
    End Class
End Namespace
