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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.maintenance_backup_helper
    Public Class MainClass
        
        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
            Try
                Select Case args.FunctionType
                    
                    Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
                        Me.ExecuteMaintenanceAction(si, args)
                        
                End Select
                
                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
        
        Private Sub ExecuteMaintenanceAction(ByVal si As SessionInfo, ByVal args As ExtenderArgs)
            Dim p_Function As String = String.Empty
            If args.NameValuePairs.ContainsKey("p_Function") Then
                p_Function = args.NameValuePairs("p_Function")
            End If
            
            ' List of tables to manage
            Dim tables As New List(Of String) From {
                "XFC_TRS_MASTER_Treasury_Monitoring",
                "XFC_TRS_Master_CashflowForecasting",
                "XFC_TRS_Master_CashDebtPosition",
                "XFC_TRS_AUX_TreasuryWeekConfirm",
                "XFC_TRS_Master_Banks"
            }
            
            Try
                Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    
                    If p_Function.XFEqualsIgnoreCase("Create") Then
                        ' Create: Create a copy of the tables adding _BackUp (Structure only)
                        For Each table As String In tables
                            ' Drop if exists
                            Dim dropSql As String = $"IF OBJECT_ID('{table}_BackUp', 'U') IS NOT NULL DROP TABLE {table}_BackUp"
                            BRApi.Database.ExecuteSql(dbConn, dropSql, False)
                            
                            ' Create empty copy (preserves Identity property)
                            Dim createSql As String = $"SELECT * INTO {table}_BackUp FROM {table} WHERE 1=0"
                            BRApi.Database.ExecuteSql(dbConn, createSql, False)
                        Next
                        BRApi.ErrorLog.LogMessage(si, "Backup tables created successfully (structure only).")
                        
                    ElseIf p_Function.XFEqualsIgnoreCase("PopulateBackup") Then
                        ' PopulateBackup: Clear backup tables and insert all records from Original tables
                        ' We use DROP + SELECT INTO to ensure clean state and schema match
                        For Each table As String In tables
                            ' Drop if exists
                            Dim dropSql As String = $"IF OBJECT_ID('{table}_BackUp', 'U') IS NOT NULL DROP TABLE {table}_BackUp"
                            BRApi.Database.ExecuteSql(dbConn, dropSql, False)
                            
                            ' Create and Populate
                            Dim populateSql As String = $"SELECT * INTO {table}_BackUp FROM {table}"
                            BRApi.Database.ExecuteSql(dbConn, populateSql, False)
                        Next
                        BRApi.ErrorLog.LogMessage(si, "Backup tables populated from Original tables successfully.")
                        
                    ElseIf p_Function.XFEqualsIgnoreCase("PopulateOriginal") Then
                        ' PopulateOriginal: Clear original tables and insert all records from Backup tables
                        For Each table As String In tables
                            ' Check if Backup exists
                            Dim checkSql As String = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{table}_BackUp'"
                            Dim countDt As DataTable = BRApi.Database.ExecuteSql(dbConn, checkSql, False)
                            If countDt.Rows.Count = 0 OrElse Convert.ToInt32(countDt.Rows(0)(0)) = 0 Then
                                Throw New Exception($"Backup table {table}_BackUp does not exist.")
                            End If
                            
                            ' Truncate Original
                            Dim truncateSql As String = $"TRUNCATE TABLE {table}"
                            BRApi.Database.ExecuteSql(dbConn, truncateSql, False)
                            
                            ' Check Identity
                            Dim identityCheckSql As String = $"SELECT OBJECTPROPERTY(OBJECT_ID('{table}'), 'TableHasIdentity')"
                            Dim identityDt As DataTable = BRApi.Database.ExecuteSql(dbConn, identityCheckSql, False)
                            Dim hasIdentity As Boolean = (identityDt.Rows.Count > 0 AndAlso Convert.ToInt32(identityDt.Rows(0)(0)) = 1)
                            
                            If hasIdentity Then
                                ' Get Columns to build INSERT statement
                                Dim colSql As String = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{table}'"
                                Dim colDt As DataTable = BRApi.Database.ExecuteSql(dbConn, colSql, False)
                                Dim columns As New List(Of String)
                                For Each row As DataRow In colDt.Rows
                                    columns.Add($"[{row("COLUMN_NAME")}]")
                                Next
                                Dim colList As String = String.Join(", ", columns)
                                
                                ' Insert with Identity
                                Dim insertSql As String = $"
                                    SET IDENTITY_INSERT {table} ON;
                                    INSERT INTO {table} ({colList}) SELECT {colList} FROM {table}_BackUp;
                                    SET IDENTITY_INSERT {table} OFF;
                                "
                                BRApi.Database.ExecuteSql(dbConn, insertSql, False)
                            Else
                                ' Simple Insert
                                Dim insertSql As String = $"INSERT INTO {table} SELECT * FROM {table}_BackUp"
                                BRApi.Database.ExecuteSql(dbConn, insertSql, False)
                            End If
                        Next
                        BRApi.ErrorLog.LogMessage(si, "Original tables restored from Backup tables successfully.")
                        
                    Else
                        Throw New Exception($"Unknown function: {p_Function}. Valid values: Create, PopulateBackup, PopulateOriginal")
                    End If
                    
                End Using
                
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Sub
        
    End Class
End Namespace
