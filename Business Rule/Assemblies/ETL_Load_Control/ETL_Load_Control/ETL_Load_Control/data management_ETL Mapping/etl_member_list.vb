Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.etl_member_list
	
	''' <summary>
	''' Dashboard Data Adapter for ETL Mapping Manager Member Lists.
	''' Provides dropdown/list data for Source Tables, Target Tables, Columns, Operators, etc.
	''' </summary>
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						Dim names As New List(Of String)()
						names.Add("GetSourceTables")
						names.Add("GetTargetTables")
						names.Add("GetAllETLTables")
						names.Add("GetTableColumns")
						names.Add("GetDistinctValues")
						names.Add("GetOperators")
						names.Add("GetRuleGroups")
						names.Add("GetSourceTargetPairs")
						Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet
						
						'-------------------------------------------------------------------
						' GetSourceTables: Returns Landing tables for Source dropdown
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("GetSourceTables") Then
							Return Me.GetSourceTablesDataTable(si)
						End If
						
						'-------------------------------------------------------------------
						' GetTargetTables: Returns DWH tables for Target dropdown
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("GetTargetTables") Then
							Return Me.GetTargetTablesDataTable(si)
						End If
						
						'-------------------------------------------------------------------
						' GetAllETLTables: Returns all ETL tables (Landing + DWH + STG)
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("GetAllETLTables") Then
							Return Me.GetAllETLTablesDataTable(si)
						End If
						
						'-------------------------------------------------------------------
						' GetTableColumns: Returns columns for a specific table
						' Parameter: p_table - Table name (direct value or parameter name to resolve)
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("GetTableColumns") Then
							Dim tableName As String = args.NameValuePairs.XFGetValue("p_table", "")
							
							' Handle parameter reference that wasn't substituted by OneStream
							' If it looks like a parameter reference, try to resolve it from Dashboard context
							If tableName.Contains("|!") OrElse tableName.Contains("!|") OrElse String.IsNullOrEmpty(tableName) Then
								' Try to get from Dashboard parameter directly
								Dim paramName As String = args.NameValuePairs.XFGetValue("p_param_name", "prm_ETL_Load_Control_Target_Table")
								tableName = Me.GetDashboardParameterValue(si, args, paramName)
								BRApi.ErrorLog.LogMessage(si, $"[DEBUG] GetTableColumns - Resolved from Dashboard param '{paramName}': '{tableName}'")
							Else
								BRApi.ErrorLog.LogMessage(si, $"[DEBUG] GetTableColumns - Direct p_table: '{tableName}'")
							End If
							
							Return Me.GetTableColumnsDataTable(si, tableName)
						End If
						
						'-------------------------------------------------------------------
						' GetDistinctValues: Returns distinct values for a column
						' Parameters: p_table - Table name, p_column - Column name
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("GetDistinctValues") Then
							Dim tableName As String = args.NameValuePairs.XFGetValue("p_table", "")
							Dim columnName As String = args.NameValuePairs.XFGetValue("p_column", "")
							Return Me.GetDistinctValuesDataTable(si, tableName, columnName)
						End If
						
						'-------------------------------------------------------------------
						' GetOperators: Returns available comparison operators
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("GetOperators") Then
							Return Me.GetOperatorsDataTable()
						End If
						
						'-------------------------------------------------------------------
						' GetRuleGroups: Returns all defined rule groups
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("GetRuleGroups") Then
							Return Me.GetRuleGroupsDataTable(si)
						End If
						
						'-------------------------------------------------------------------
						' GetSourceTargetPairs: Returns recommended source->target mappings
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("GetSourceTargetPairs") Then
							Return Me.GetSourceTargetPairsDataTable(si)
						End If
					
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Data Table Generators"
		
		''' <summary>
		''' Gets source (Landing) tables as DataTable
		''' Format optimized for ComboBox binding
		''' </summary>
		Private Function GetSourceTablesDataTable(ByVal si As SessionInfo) As DataTable
			Dim dt As New DataTable("SourceTables")
			' Single column format - simplest for ComboBox
			dt.Columns.Add("Name", GetType(String))
			dt.Columns.Add("Value", GetType(String))
			
			' Add empty option first
			dt.Rows.Add("-- Select Source Table --", "")
			
			Dim tables As List(Of ETL_METADATA.TableInfo) = ETL_METADATA.GetLandingTables(si)
			For Each table As ETL_METADATA.TableInfo In tables
				dt.Rows.Add(table.TableName, table.TableName)
			Next
			
			Return dt
		End Function
		
		''' <summary>
		''' Gets target (DWH) tables as DataTable
		''' Format optimized for ComboBox binding
		''' </summary>
		Private Function GetTargetTablesDataTable(ByVal si As SessionInfo) As DataTable
			Dim dt As New DataTable("TargetTables")
			' Single column format - simplest for ComboBox
			dt.Columns.Add("Name", GetType(String))
			dt.Columns.Add("Value", GetType(String))
			
			' Add empty option first
			dt.Rows.Add("-- Select Target Table --", "")
			
			Dim tables As List(Of ETL_METADATA.TableInfo) = ETL_METADATA.GetDWHTables(si)
			For Each table As ETL_METADATA.TableInfo In tables
				dt.Rows.Add(table.TableName, table.TableName)
			Next
			
			Return dt
		End Function
		
		''' <summary>
		''' Gets all ETL tables as DataTable
		''' </summary>
		Private Function GetAllETLTablesDataTable(ByVal si As SessionInfo) As DataTable
			Dim dt As New DataTable("AllETLTables")
			dt.Columns.Add("TableName", GetType(String))
			dt.Columns.Add("TableType", GetType(String))
			dt.Columns.Add("ColumnCount", GetType(Integer))
			dt.Columns.Add("Description", GetType(String))
			
			Dim tables As List(Of ETL_METADATA.TableInfo) = ETL_METADATA.GetAllETLTables(si)
			For Each table As ETL_METADATA.TableInfo In tables
				dt.Rows.Add(table.TableName, table.TableType, table.ColumnCount, table.Description)
			Next
			
			Return dt
		End Function
		
		''' <summary>
		''' Gets columns for a table as DataTable
		''' </summary>
		Private Function GetTableColumnsDataTable(ByVal si As SessionInfo, ByVal tableName As String) As DataTable
			Dim dt As New DataTable("TableColumns")
			dt.Columns.Add("ColumnName", GetType(String))
			dt.Columns.Add("DataType", GetType(String))
			dt.Columns.Add("MaxLength", GetType(Integer))
			dt.Columns.Add("IsNullable", GetType(Boolean))
			dt.Columns.Add("Position", GetType(Integer))
			
			' Add empty option first for ComboBox
			dt.Rows.Add("-- Select Column --", "", 0, False, 0)
			
			If Not String.IsNullOrEmpty(tableName) AndAlso Not tableName.StartsWith("--") Then
				Dim columns As List(Of ETL_METADATA.ColumnInfo) = ETL_METADATA.GetTableColumns(si, tableName)
				For Each col As ETL_METADATA.ColumnInfo In columns
					dt.Rows.Add(col.ColumnName, col.DataType, col.MaxLength, col.IsNullable, col.OrdinalPosition)
				Next
			End If
			
			Return dt
		End Function
		
		''' <summary>
		''' Gets distinct column values as DataTable
		''' </summary>
		Private Function GetDistinctValuesDataTable(ByVal si As SessionInfo, ByVal tableName As String, ByVal columnName As String) As DataTable
			Dim dt As New DataTable("DistinctValues")
			dt.Columns.Add("Value", GetType(String))
			
			If Not String.IsNullOrEmpty(tableName) AndAlso Not String.IsNullOrEmpty(columnName) Then
				Dim values As List(Of String) = ETL_METADATA.GetDistinctColumnValues(si, tableName, columnName)
				For Each val As String In values
					dt.Rows.Add(val)
				Next
			End If
			
			Return dt
		End Function
		
		''' <summary>
		''' Gets comparison operators as DataTable
		''' </summary>
		Private Function GetOperatorsDataTable() As DataTable
			Dim dt As New DataTable("Operators")
			dt.Columns.Add("OperatorId", GetType(String))
			dt.Columns.Add("DisplayName", GetType(String))
			
			Dim operators As List(Of KeyValuePair(Of String, String)) = CONDITION_BUILDER.GetAllOperators()
			For Each op As KeyValuePair(Of String, String) In operators
				dt.Rows.Add(op.Key, op.Value)
			Next
			
			Return dt
		End Function
		
		''' <summary>
		''' Gets rule groups as DataTable
		''' </summary>
		Private Function GetRuleGroupsDataTable(ByVal si As SessionInfo) As DataTable
			Dim dt As New DataTable("RuleGroups")
			dt.Columns.Add("RuleGroup", GetType(String))
			
			Dim groups As List(Of String) = ETL_RULES.GetAllRuleGroups(si)
			For Each grp As String In groups
				dt.Rows.Add(grp)
			Next
			
			Return dt
		End Function
		
		''' <summary>
		''' Gets source->target table pairs as DataTable
		''' </summary>
		Private Function GetSourceTargetPairsDataTable(ByVal si As SessionInfo) As DataTable
			Dim dt As New DataTable("SourceTargetPairs")
			dt.Columns.Add("SourceTable", GetType(String))
			dt.Columns.Add("TargetTable", GetType(String))
			
			Dim pairs As Dictionary(Of String, String) = ETL_METADATA.GetSourceTargetPairs(si)
			For Each pair As KeyValuePair(Of String, String) In pairs
				dt.Rows.Add(pair.Key, pair.Value)
			Next
			
			Return dt
		End Function
		
		#End Region
		
		#Region "Dashboard Parameter Helpers"
		
		''' <summary>
		''' Resolves a Dashboard parameter value from the current Dashboard context.
		''' This is needed when parameter substitution in Method Query doesn't work.
		''' </summary>
		Private Function GetDashboardParameterValue(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs, ByVal paramName As String) As String
			Try
				' Method 1: Try to get from NameValuePairs (sometimes passed as additional param)
				Dim directValue As String = args.NameValuePairs.XFGetValue(paramName, "")
				If Not String.IsNullOrEmpty(directValue) AndAlso Not directValue.Contains("!") Then
					Return directValue
				End If
				
				' Method 2: Try to get from Dashboard context via CustomSubstVars
				If args.CustomSubstVars IsNot Nothing AndAlso args.CustomSubstVars.ContainsKey(paramName) Then
					Return args.CustomSubstVars(paramName).ToString()
				End If
				
				' Method 3: Check if there's a clean parameter name in NameValuePairs
				For Each kvp As KeyValuePair(Of String, String) In args.NameValuePairs
					If kvp.Key.XFEqualsIgnoreCase(paramName) Then
						Return If(kvp.Value, "")
					End If
				Next
				
				Return ""
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to resolve parameter '{paramName}': {ex.Message}")
				Return ""
			End Try
		End Function
		
		#End Region
		
	End Class
End Namespace
