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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	
	''' <summary>
	''' Manages the XFC_ETL_RULES table for metadata-driven ETL transformations.
	''' Allows dynamic CASE logic configuration without code changes.
	''' </summary>
	Public Class ETL_RULES
		
		'Declare table name
		Private Shared ReadOnly TABLE_NAME As String = "XFC_ETL_RULES"
		
		#Region "Get Table Name"
		
		''' <summary>
		''' Returns the table name for this class
		''' </summary>
		Public Shared Function GetTableName() As String
			Return TABLE_NAME
		End Function
		
		#End Region
		
		#Region "Rule Configuration Class"
		
		''' <summary>
		''' Configuration object for ETL transformation rules
		''' </summary>
		Public Class EtlRuleConfig
			Public Property RuleId As Integer = 0
			Public Property RuleGroup As String = String.Empty
			Public Property SourceTable As String = String.Empty
			Public Property TableMapping As String = String.Empty
			Public Property TargetColumn As String = String.Empty
			Public Property Priority As Integer = 99
			Public Property ConditionSQL As String = String.Empty
			Public Property TargetValue As String = String.Empty
			Public Property Description As String = String.Empty
			Public Property IsActive As Boolean = True
		End Class
		
		#End Region
		
		#Region "Get Migration Query"

		Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
			Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				
				'Up
				Dim upQuery As String = $"
				IF OBJECT_ID(N'{TABLE_NAME}', N'U') IS NULL
				BEGIN
					CREATE TABLE {TABLE_NAME} (
						RuleId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
						RuleGroup NVARCHAR(100) NOT NULL,
						SourceTable NVARCHAR(128) NOT NULL,
						TableMapping NVARCHAR(128) NOT NULL,
						TargetColumn NVARCHAR(50) NOT NULL,
						Priority INT NOT NULL DEFAULT 99,
						ConditionColumn NVARCHAR(50) NULL,
						ConditionSQL NVARCHAR(MAX) NOT NULL,
						TargetValue NVARCHAR(100) NOT NULL,
						Description NVARCHAR(255) NULL,
						IsActive BIT NOT NULL DEFAULT 1
					);
					
					-- Index on RuleGroup, TableMapping, TargetColumn and Priority for efficient rule retrieval
					CREATE NONCLUSTERED INDEX IX_ETL_RULES_RuleGroup_Table_Column_Priority 
					ON {TABLE_NAME} (RuleGroup, TableMapping, TargetColumn, Priority) 
					INCLUDE (SourceTable, ConditionColumn, ConditionSQL, TargetValue, IsActive);
				END;
				"
				
				'Down
				Dim downQuery As String = $"
					DROP TABLE {TABLE_NAME};
				"
				
				Return If(type.ToLower = "up", upQuery, downQuery)
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		#End Region
		
		#Region "Get Population Query"

		Public Function GetPopulationQuery(ByVal si As SessionInfo, type As String) As String
			Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Population type must be 'up' or 'down'"))
				
				'Up - Seed initial UD8 mapping rules (migrated from hardcoded CASE in dwh_fin_mix.vb)
				Dim upQuery As String = $"
				-- Seed UD8 mapping rules (originally hardcoded in dwh_fin_mix.vb)
				IF NOT EXISTS (SELECT 1 FROM {TABLE_NAME} WHERE RuleGroup = 'XFC_LANDING_FIN_MIX__XFC_DWH_FIN_MIX__UD8' AND TableMapping = 'XFC_DWH_FIN_MIX')
				BEGIN
					INSERT INTO {TABLE_NAME} (RuleGroup, SourceTable, TableMapping, TargetColumn, Priority, ConditionSQL, TargetValue, Description, IsActive)
					VALUES 
						('XFC_LANDING_FIN_MIX__XFC_DWH_FIN_MIX__UD8', 'XFC_LANDING_FIN_MIX', 'XFC_DWH_FIN_MIX', 'UD8', 10, 'ledger = ''2L''', 'Statutory', 'Local/Statutory ledger mapping', 1),
						('XFC_LANDING_FIN_MIX__XFC_DWH_FIN_MIX__UD8', 'XFC_LANDING_FIN_MIX', 'XFC_DWH_FIN_MIX', 'UD8', 20, 'ledger = ''0L'' AND transactionType = ''REACI''', 'IFRS16', 'IFRS 16 lease adjustments', 1),
						('XFC_LANDING_FIN_MIX__XFC_DWH_FIN_MIX__UD8', 'XFC_LANDING_FIN_MIX', 'XFC_DWH_FIN_MIX', 'UD8', 99, '1=1', 'Total_IFRS_ledger', 'Default catch-all rule', 1);
				END;
				"
				
				'Down
				Dim downQuery As String = $"
					TRUNCATE TABLE {TABLE_NAME};
				"
				
				Return If(type.ToLower = "up", upQuery, downQuery)
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		#End Region
		
		#Region "Get Rules By Group"
		
		''' <summary>
		''' Gets all active rules for a specific RuleGroup, optionally filtered by TableMapping and TargetColumn, ordered by Priority.
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="ruleGroup">The rule group name (e.g., 'UD8_Map')</param>
		''' <param name="tableMapping">Optional: Target table name to filter rules (e.g., 'XFC_DWH_FIN_MIX')</param>
		''' <param name="targetColumn">Optional: Target column name to filter rules (e.g., 'UD8')</param>
		''' <returns>List of EtlRuleConfig ordered by Priority ASC</returns>
		Public Shared Function GetRulesByGroup(ByVal si As SessionInfo, ByVal ruleGroup As String, Optional ByVal tableMapping As String = "", Optional ByVal targetColumn As String = "") As List(Of EtlRuleConfig)
			Dim rules As New List(Of EtlRuleConfig)()
			
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim filters As New StringBuilder()
					If Not String.IsNullOrEmpty(tableMapping) Then
						filters.Append($" AND TableMapping = '{HelperFunctions.EscapeSql(tableMapping)}'")
					End If
					If Not String.IsNullOrEmpty(targetColumn) Then
						filters.Append($" AND TargetColumn = '{HelperFunctions.EscapeSql(targetColumn)}'")
					End If
					
					Dim query As String = $"
						SELECT RuleId, RuleGroup, SourceTable, TableMapping, TargetColumn, Priority, ConditionSQL, TargetValue, Description, IsActive
						FROM {TABLE_NAME}
						WHERE RuleGroup = '{HelperFunctions.EscapeSql(ruleGroup)}'
						  AND IsActive = 1{filters.ToString()}
						ORDER BY Priority ASC
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing Then
						For Each row As DataRow In dt.Rows
							rules.Add(ParseRuleRow(row))
						Next
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get ETL rules for group '{ruleGroup}': {ex.Message}")
			End Try
			
			Return rules
		End Function
		
		''' <summary>
		''' Parses a DataRow into EtlRuleConfig object
		''' </summary>
		Private Shared Function ParseRuleRow(ByVal row As DataRow) As EtlRuleConfig
			Dim config As New EtlRuleConfig()
			config.RuleId = CInt(row("RuleId"))
			config.RuleGroup = If(IsDBNull(row("RuleGroup")), "", row("RuleGroup").ToString())
			config.SourceTable = If(IsDBNull(row("SourceTable")), "", row("SourceTable").ToString())
			config.TableMapping = If(IsDBNull(row("TableMapping")), "", row("TableMapping").ToString())
			config.TargetColumn = If(IsDBNull(row("TargetColumn")), "", row("TargetColumn").ToString())
			config.Priority = If(IsDBNull(row("Priority")), 99, CInt(row("Priority")))
			config.ConditionSQL = If(IsDBNull(row("ConditionSQL")), "", row("ConditionSQL").ToString())
			config.TargetValue = If(IsDBNull(row("TargetValue")), "", row("TargetValue").ToString())
			config.Description = If(IsDBNull(row("Description")), "", row("Description").ToString())
			config.IsActive = Not IsDBNull(row("IsActive")) AndAlso CBool(row("IsActive"))
			Return config
		End Function
		
		#End Region
		
		#Region "Build Dynamic CASE"
		
		''' <summary>
		''' Builds a dynamic SQL CASE statement from rules in XFC_ETL_RULES table.
		''' Returns a CASE WHEN...THEN...ELSE...END block ready to embed in SQL queries.
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="ruleGroup">The rule group name (e.g., 'UD8_Map')</param>
		''' <param name="defaultValue">Fallback value if no rules found or as ELSE clause</param>
		''' <param name="tableMapping">Optional: Target table name to filter rules (e.g., 'XFC_DWH_FIN_MIX')</param>
		''' <param name="targetColumn">Optional: Target column name to filter rules (e.g., 'UD8')</param>
		''' <returns>SQL CASE statement string</returns>
		''' <example>
		''' Dim ud8Logic As String = ETL_RULES.BuildDynamicCase(si, "UD8_Map", "Total_IFRS_ledger", "XFC_DWH_FIN_MIX", "UD8")
		''' ' Returns:
		''' ' CASE
		''' '     WHEN [ledger] = '2L' THEN 'Statutory'
		''' '     WHEN [ledger] = '0L' AND [transactionType] = 'REACI' THEN 'IFRS16'
		''' '     ELSE 'Total_IFRS_ledger'
		''' ' END
		''' 
		''' ' For column references (TargetValue starts with '['):
		''' ' CASE WHEN [vendorAccountGroup] IS NULL THEN [customerAccountGroup] ELSE [vendorAccountGroup] END
		''' </example>
		Public Shared Function BuildDynamicCase(
			ByVal si As SessionInfo, 
			ByVal ruleGroup As String, 
			ByVal defaultValue As String,
			Optional ByVal tableMapping As String = "",
			Optional ByVal targetColumn As String = ""
		) As String
			
			Dim rules As List(Of EtlRuleConfig) = GetRulesByGroup(si, ruleGroup, tableMapping, targetColumn)
			
			' If no rules found, return simple default value
			If rules Is Nothing OrElse rules.Count = 0 Then
				Return FormatTargetValue(defaultValue)
			End If
			
			' Check if there are any non-catch-all rules (rules with actual conditions, not just "1=1")
			Dim hasNonCatchAllRules As Boolean = rules.Any(Function(r) r.ConditionSQL.Trim() <> "1=1")
			
			' If ALL rules are catch-all (1=1), don't generate CASE - just return the catch-all value
			If Not hasNonCatchAllRules Then
				Dim catchAllRule As EtlRuleConfig = rules.FirstOrDefault(Function(r) r.ConditionSQL.Trim() = "1=1")
				Dim directValue As String = If(catchAllRule IsNot Nothing, catchAllRule.TargetValue, defaultValue)
				Return FormatTargetValue(directValue)
			End If
			
			Dim sb As New StringBuilder()
			sb.AppendLine("CASE")
			
			For Each rule As EtlRuleConfig In rules
				' Skip the catch-all rule (1=1) - we'll handle it as ELSE
				If rule.ConditionSQL.Trim() = "1=1" Then
					Continue For
				End If
				
				' Build WHEN clause - handle both literal values and column references
				Dim formattedValue As String = FormatTargetValue(rule.TargetValue)
				sb.AppendLine($"    WHEN {rule.ConditionSQL} THEN {formattedValue}")
			Next
			
			' Find the catch-all rule (1=1) for ELSE, or use defaultValue
			Dim catchAllElseRule As EtlRuleConfig = rules.FirstOrDefault(Function(r) r.ConditionSQL.Trim() = "1=1")
			Dim elseValue As String = If(catchAllElseRule IsNot Nothing, catchAllElseRule.TargetValue, defaultValue)
			
			sb.AppendLine($"    ELSE {FormatTargetValue(elseValue)}")
			sb.Append("END")
			
			Return sb.ToString()
		End Function
		
		''' <summary>
		''' Formats a target value for use in SQL.
		''' - If value starts with '[' and ends with ']', it's a column reference - return as-is
		''' - If value contains SQL functions or expressions (like COALESCE, ISNULL), return as-is
		''' - If value contains concatenation operator (+) with column references, return as-is
		''' - Otherwise, wrap in single quotes as a literal string
		''' </summary>
		''' <param name="value">The target value to format</param>
		''' <returns>Formatted SQL value</returns>
		Private Shared Function FormatTargetValue(ByVal value As String) As String
			If String.IsNullOrEmpty(value) Then
				Return "''"
			End If
			
			Dim trimmedValue As String = value.Trim()
			
			' Check if it's a column reference [columnName]
			If trimmedValue.StartsWith("[") AndAlso trimmedValue.EndsWith("]") Then
				Return trimmedValue
			End If
			
			' Check if it looks like a SQL expression (contains functions, brackets, or concatenation)
			If trimmedValue.ToUpper().Contains("COALESCE(") OrElse 
			   trimmedValue.ToUpper().Contains("ISNULL(") OrElse
			   trimmedValue.ToUpper().Contains("NULLIF(") OrElse
			   trimmedValue.ToUpper().Contains("TRY_CAST(") OrElse
			   trimmedValue.ToUpper().Contains("CASE ") OrElse
			   trimmedValue.ToUpper().Contains("CAST(") OrElse
			   trimmedValue.ToUpper().Contains("CONVERT(") OrElse
			   trimmedValue.Contains("[") OrElse
			   (trimmedValue.Contains("+") AndAlso (trimmedValue.Contains("[") OrElse trimmedValue.Contains("'"))) Then
				Return trimmedValue
			End If
			
			' It's a literal value - wrap in quotes
			Return $"'{HelperFunctions.EscapeSql(trimmedValue)}'"
		End Function
		
		#End Region
		
		#Region "CRUD Operations"
		
		''' <summary>
		''' Inserts a new rule into XFC_ETL_RULES
		''' </summary>
		Public Shared Function InsertRule(
			ByVal si As SessionInfo,
			ByVal sourceTable As String,
			ByVal tableMapping As String,
			ByVal targetColumn As String,
			ByVal priority As Integer,
			ByVal conditionSQL As String,
			ByVal targetValue As String,
			Optional ByVal description As String = "",
			Optional ByVal isActive As Boolean = True
		) As Integer
			Try
				' Auto-generate RuleGroup from SourceTable, TableMapping and TargetColumn
				Dim ruleGroup As String = GenerateRuleGroup(sourceTable, tableMapping, targetColumn)
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						INSERT INTO {TABLE_NAME} (RuleGroup, SourceTable, TableMapping, TargetColumn, Priority, ConditionSQL, TargetValue, Description, IsActive)
						OUTPUT INSERTED.RuleId
						VALUES (
							'{HelperFunctions.EscapeSql(ruleGroup)}',
							'{HelperFunctions.EscapeSql(sourceTable)}',
							'{HelperFunctions.EscapeSql(tableMapping)}',
							'{HelperFunctions.EscapeSql(targetColumn)}',
							{priority},
							'{HelperFunctions.EscapeSql(conditionSQL)}',
							'{HelperFunctions.EscapeSql(targetValue)}',
							'{HelperFunctions.EscapeSql(description)}',
							{If(isActive, 1, 0)}
						)
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						Return CInt(dt.Rows(0)(0))
					End If
				End Using
				Return 0
			Catch ex As Exception
				Throw New XFException(si, New Exception($"Failed to insert ETL rule: {ex.Message}", ex))
			End Try
		End Function
		
		''' <summary>
		''' Updates an existing rule
		''' </summary>
		Public Shared Sub UpdateRule(
			ByVal si As SessionInfo,
			ByVal ruleId As Integer,
			ByVal priority As Integer,
			ByVal conditionSQL As String,
			ByVal targetValue As String,
			Optional ByVal description As String = "",
			Optional ByVal isActive As Boolean = True
		)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						UPDATE {TABLE_NAME}
						SET Priority = {priority},
							ConditionSQL = '{HelperFunctions.EscapeSql(conditionSQL)}',
							TargetValue = '{HelperFunctions.EscapeSql(targetValue)}',
							Description = '{HelperFunctions.EscapeSql(description)}',
							IsActive = {If(isActive, 1, 0)}
						WHERE RuleId = {ruleId}
					"
					BRApi.Database.ExecuteSql(dbConn, query, False)
				End Using
			Catch ex As Exception
				Throw New XFException(si, New Exception($"Failed to update ETL rule {ruleId}: {ex.Message}", ex))
			End Try
		End Sub
		
		''' <summary>
		''' Deletes a rule by RuleId
		''' </summary>
		Public Shared Sub DeleteRule(ByVal si As SessionInfo, ByVal ruleId As Integer)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"DELETE FROM {TABLE_NAME} WHERE RuleId = {ruleId}"
					BRApi.Database.ExecuteSql(dbConn, query, False)
				End Using
			Catch ex As Exception
				Throw New XFException(si, New Exception($"Failed to delete ETL rule {ruleId}: {ex.Message}", ex))
			End Try
		End Sub
		
		''' <summary>
		''' Toggles the IsActive flag for a rule
		''' </summary>
		Public Shared Sub ToggleRuleActive(ByVal si As SessionInfo, ByVal ruleId As Integer, ByVal isActive As Boolean)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"UPDATE {TABLE_NAME} SET IsActive = {If(isActive, 1, 0)} WHERE RuleId = {ruleId}"
					BRApi.Database.ExecuteSql(dbConn, query, False)
				End Using
			Catch ex As Exception
				Throw New XFException(si, New Exception($"Failed to toggle ETL rule {ruleId}: {ex.Message}", ex))
			End Try
		End Sub
		
		#End Region
		
		#Region "Generate Rule Group"
		
		''' <summary>
		''' Generates a consistent RuleGroup identifier from SourceTable, TableMapping and TargetColumn.
		''' Format: {SourceTable}__{TableMapping}__{TargetColumn}
		''' </summary>
		''' <param name="sourceTable">Source table name (e.g., 'XFC_LANDING_FIN_MIX')</param>
		''' <param name="tableMapping">Target table name (e.g., 'XFC_DWH_FIN_MIX')</param>
		''' <param name="targetColumn">Target column name (e.g., 'UD8')</param>
		''' <returns>Generated RuleGroup string</returns>
		Public Shared Function GenerateRuleGroup(ByVal sourceTable As String, ByVal tableMapping As String, ByVal targetColumn As String) As String
			Return $"{sourceTable}__{tableMapping}__{targetColumn}"
		End Function
		
		''' <summary>
		''' Gets all unique RuleGroups defined in the system
		''' </summary>
		Public Shared Function GetAllRuleGroups(ByVal si As SessionInfo) As List(Of String)
			Dim groups As New List(Of String)()
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"SELECT DISTINCT RuleGroup FROM {TABLE_NAME} ORDER BY RuleGroup"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					If dt IsNot Nothing Then
						For Each row As DataRow In dt.Rows
							If Not IsDBNull(row("RuleGroup")) Then
								groups.Add(row("RuleGroup").ToString())
							End If
						Next
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get rule groups: {ex.Message}")
			End Try
			Return groups
		End Function
		
		''' <summary>
		''' Gets all rules (including inactive) for management UI
		''' </summary>
		Public Shared Function GetAllRules(ByVal si As SessionInfo, Optional ByVal includeInactive As Boolean = True) As List(Of EtlRuleConfig)
			Dim rules As New List(Of EtlRuleConfig)()
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim activeFilter As String = If(includeInactive, "", " WHERE IsActive = 1")
					Dim query As String = $"
						SELECT RuleId, RuleGroup, SourceTable, TableMapping, TargetColumn, Priority, ConditionSQL, TargetValue, Description, IsActive
						FROM {TABLE_NAME}{activeFilter}
						ORDER BY RuleGroup, Priority ASC
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					If dt IsNot Nothing Then
						For Each row As DataRow In dt.Rows
							rules.Add(ParseRuleRow(row))
						Next
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get all ETL rules: {ex.Message}")
			End Try
			Return rules
		End Function
		
		#End Region

	End Class
End Namespace
