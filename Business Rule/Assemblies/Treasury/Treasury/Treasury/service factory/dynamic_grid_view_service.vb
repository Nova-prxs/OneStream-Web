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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Imports System.Reflection

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class dynamic_grid_view_service
		Implements IWsasDynamicGridV800

        Public Function GetDynamicGridData(ByVal si As SessionInfo, ByVal brGlobals As BRGlobals, ByVal workspace As DashboardWorkspace, ByVal args As DashboardDynamicGridArgs) As XFDynamicGridGetDataResult Implements IWsasDynamicGridV800.GetDynamicGridData
            Try
                If (brGlobals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
					
					' Get parameters with safe access
					Dim paramYear As String = If(args.CustomSubstVars.ContainsKey("prm_Year"), args.CustomSubstVars("prm_Year"), String.Empty)
					Dim paramScenario As String = If(args.CustomSubstVars.ContainsKey("prm_Scenario"), args.CustomSubstVars("prm_Scenario"), String.Empty)
					Dim paramEntity As String = If(args.CustomSubstVars.ContainsKey("prm_Entity"), args.CustomSubstVars("prm_Entity"), String.Empty)
					
					' Define Columns
					Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition)
                    ' Example of defining columns explicitly if needed, otherwise they are inferred from DataTable
                    ' To make a column editable, you would typically set properties here if the API supports it, 
                    ' or rely on the grid's behavior with ReadWrite access.
                    ' For example:
                    ' columnDefinitions.Add(New XFDynamicGridColumnDefinition With {.ColumnName = "Amount", .IsReadOnly = False})

					Dim dt As New DataTable()
					Dim countDt As New DataTable()
					
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						
                        ' Calculate pagination parameters with safe defaults
                        Dim rowsPerPage As Integer = If(args.DynamicGridDefinition.RowsPerPage > 0, args.DynamicGridDefinition.RowsPerPage, 50) ' Default to 50 if not set
                        Dim pageNumber As Integer = If(rowsPerPage > 0, CInt(Math.Ceiling((args.GetDataArgs.StartRowIndex + 1) / rowsPerPage)), 1)
                        If pageNumber < 1 Then pageNumber = 1 ' Ensure page number is at least 1

						Dim dbParamInfos As New List(Of DbParamInfo) From {
							New DbParamInfo("PageNumber", pageNumber),
							New DbParamInfo("RowsPerPage", rowsPerPage),
							New DbParamInfo("paramYear", paramYear),
							New DbParamInfo("paramScenario", paramScenario)
						}

                        ' Build the WHERE clause dynamically based on optional parameters
                        Dim whereClause As String = "WHERE 1=1"
                        
                        If Not String.IsNullOrEmpty(paramYear) Then
                            whereClause &= " AND UploadYear = @paramYear"
                        End If
                        
                        If Not String.IsNullOrEmpty(paramScenario) Then
                            whereClause &= " AND Scenario = @paramScenario"
                        End If
                        
                        If Not String.IsNullOrEmpty(paramEntity) AndAlso paramEntity <> "HTD" Then
                            whereClause &= " AND Entity = @paramEntity"
                            dbParamInfos.Add(New DbParamInfo("paramEntity", paramEntity))
                        End If

                        ' Main Data Query
						dt = BRApi.Database.ExecuteSql(
							dbConn, $"
							SELECT
                                RowId,
                                UploadTimekey,
                                UploadYear,
                                UploadWeekNumber,
                                Entity,
                                Scenario,
                                Account,
                                Flow,
                                Bank,
                                ProjectionType,
                                ProjectionTimekey,
                                ProjectionDate,
                                Amount
							FROM
							    XFC_TRS_Master_CashDebtPosition
                            {whereClause}
							ORDER BY
							    UploadTimekey DESC, Entity, Account
							OFFSET (@PageNumber - 1) * @RowsPerPage ROWS
							FETCH NEXT @RowsPerPage ROWS ONLY;
							",
							dbParamInfos,
							False
						)

                        ' Count Query for Pagination
						countDt = BRApi.Database.ExecuteSql(
							dbConn, $"
							SELECT COUNT(*) AS count
							FROM XFC_TRS_Master_CashDebtPosition
                            {whereClause}
							",
							dbParamInfos,
							False
						)

					End Using

					Dim gridData As New XFDataTable(
						si,
						dt,
						Nothing,
						args.DynamicGridDefinition.RowsPerPage,
						1
					)
					gridData.TotalNumRowsInOriginalDataTable = countDt.Rows(0)("count")

					Return New XFDynamicGridGetDataResult With {
						.DataTable = gridData,
						.ColumnDefinitions = columnDefinitions,
						.AccessLevel = DataAccessLevel.AllAccess ' This allows potential editing if the grid is configured for it
					}
					
                End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function

        Public Function SaveDynamicGridData(ByVal si As SessionInfo, ByVal brGlobals As BRGlobals, ByVal workspace As DashboardWorkspace, ByVal args As DashboardDynamicGridArgs) As XFDynamicGridSaveDataResult Implements IWsasDynamicGridV800.SaveDynamicGridData
            Try
                If (brGlobals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
                    
                    ' Use BRApi.Database.SaveDataTableRows to handle the save operation
                    If args.SaveDataArgs IsNot Nothing AndAlso args.SaveDataArgs.EditedDataRows IsNot Nothing Then
                        
                        ' Create database connection
                        Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                            
                            ' Define the columns for SaveDataTableRows
                            Dim columns As New List(Of XFDataColumn) From {
                                New XFDataColumn(si, New DataColumn("RowId", GetType(Integer)), True, True), ' PK, not editable
                                New XFDataColumn(si, New DataColumn("UploadTimekey", GetType(Integer)), False, False),
                                New XFDataColumn(si, New DataColumn("UploadYear", GetType(Integer)), False, False),
                                New XFDataColumn(si, New DataColumn("UploadWeekNumber", GetType(Integer)), False, False),
                                New XFDataColumn(si, New DataColumn("Entity", GetType(String)), False, False),
                                New XFDataColumn(si, New DataColumn("Scenario", GetType(String)), False, False),
                                New XFDataColumn(si, New DataColumn("Account", GetType(String)), False, False),
                                New XFDataColumn(si, New DataColumn("Flow", GetType(String)), False, False),
                                New XFDataColumn(si, New DataColumn("Bank", GetType(String)), False, False),
                                New XFDataColumn(si, New DataColumn("ProjectionType", GetType(String)), False, False),
                                New XFDataColumn(si, New DataColumn("ProjectionTimekey", GetType(Integer)), False, False),
                                New XFDataColumn(si, New DataColumn("ProjectionDate", GetType(DateTime)), False, False),
                                New XFDataColumn(si, New DataColumn("Amount", GetType(Decimal)), False, False)
                            }
                            
                            ' Use SaveDataTableRows to persist the changes
                            ' This method handles INSERT/UPDATE/DELETE automatically based on row state
                            BRApi.Database.SaveDataTableRows(
                                dbConn,
                                "XFC_TRS_Master_CashDebtPosition",
                                columns,
                                True, ' hasPrimaryKeyColumns
                                args.SaveDataArgs.EditedDataRows,
                                True, ' commitTransaction
                                False, ' includeAddedRows
                                False  ' includeDeletedRows
                            )
                            
                        End Using
                    End If
                    
                    Return New XFDynamicGridSaveDataResult()
                End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function
	End Class
End Namespace
