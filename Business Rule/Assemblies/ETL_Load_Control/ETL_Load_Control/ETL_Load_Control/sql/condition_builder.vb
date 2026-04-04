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
	''' SQL Condition Builder for ETL Mapping Manager.
	''' Provides structured condition creation and parsing for non-technical users.
	''' Supports AND/OR grouping with GUI-friendly object representation.
	''' </summary>
	Public Class CONDITION_BUILDER
		
		#Region "Condition Classes"
		
		''' <summary>
		''' Supported comparison operators
		''' </summary>
		Public Enum ComparisonOperator
			Equals              ' = 
			NotEquals           ' <>
			GreaterThan         ' >
			LessThan            ' <
			GreaterOrEqual      ' >=
			LessOrEqual         ' <=
			Contains            ' LIKE '%value%'
			StartsWith          ' LIKE 'value%'
			EndsWith            ' LIKE '%value'
			IsNull              ' IS NULL
			IsNotNull           ' IS NOT NULL
			InList              ' IN (...)
		End Enum
		
		''' <summary>
		''' Logical operators for condition grouping
		''' </summary>
		Public Enum LogicalOperator
			LogicalAnd
			LogicalOr
		End Enum
		
		''' <summary>
		''' Represents a single condition (e.g., ledger = '2L')
		''' </summary>
		Public Class SingleCondition
			Public Property ColumnName As String = String.Empty
			Public Property CompareOp As ComparisonOperator = ComparisonOperator.Equals
			Public Property Value As String = String.Empty
			Public Property Values As List(Of String) = New List(Of String)()  ' For IN operator
		End Class
		
		''' <summary>
		''' Represents a group of conditions with a logical operator
		''' </summary>
		Public Class ConditionGroup
			Public Property Conditions As List(Of SingleCondition) = New List(Of SingleCondition)()
			Public Property LogicalOp As LogicalOperator = LogicalOperator.LogicalAnd
		End Class
		
		''' <summary>
		''' Complete rule condition structure (supports multiple groups)
		''' </summary>
		Public Class RuleCondition
			Public Property Groups As List(Of ConditionGroup) = New List(Of ConditionGroup)()
			Public Property GroupLogicalOp As LogicalOperator = LogicalOperator.LogicalAnd  ' AND/OR between groups
		End Class
		
		#End Region
		
		#Region "Operator Mappings"
		
		''' <summary>
		''' Maps operator enum to SQL operator string
		''' </summary>
		Private Shared ReadOnly OperatorToSQL As New Dictionary(Of ComparisonOperator, String) From {
			{ComparisonOperator.Equals, "="},
			{ComparisonOperator.NotEquals, "<>"},
			{ComparisonOperator.GreaterThan, ">"},
			{ComparisonOperator.LessThan, "<"},
			{ComparisonOperator.GreaterOrEqual, ">="},
			{ComparisonOperator.LessOrEqual, "<="},
			{ComparisonOperator.Contains, "LIKE"},
			{ComparisonOperator.StartsWith, "LIKE"},
			{ComparisonOperator.EndsWith, "LIKE"},
			{ComparisonOperator.IsNull, "IS NULL"},
			{ComparisonOperator.IsNotNull, "IS NOT NULL"},
			{ComparisonOperator.InList, "IN"}
		}
		
		''' <summary>
		''' Gets display name for operator (UI-friendly)
		''' </summary>
		Public Shared Function GetOperatorDisplayName(ByVal op As ComparisonOperator) As String
			Select Case op
				Case ComparisonOperator.Equals : Return "equals"
				Case ComparisonOperator.NotEquals : Return "not equals"
				Case ComparisonOperator.GreaterThan : Return "greater than"
				Case ComparisonOperator.LessThan : Return "less than"
				Case ComparisonOperator.GreaterOrEqual : Return "greater or equal"
				Case ComparisonOperator.LessOrEqual : Return "less or equal"
				Case ComparisonOperator.Contains : Return "contains"
				Case ComparisonOperator.StartsWith : Return "starts with"
				Case ComparisonOperator.EndsWith : Return "ends with"
				Case ComparisonOperator.IsNull : Return "is empty"
				Case ComparisonOperator.IsNotNull : Return "is not empty"
				Case ComparisonOperator.InList : Return "is one of"
				Case Else : Return "equals"
			End Select
		End Function
		
		''' <summary>
		''' Gets all operators as list for dropdown population
		''' </summary>
		Public Shared Function GetAllOperators() As List(Of KeyValuePair(Of String, String))
			Dim operators As New List(Of KeyValuePair(Of String, String))()
			For Each op As ComparisonOperator In [Enum].GetValues(GetType(ComparisonOperator))
				operators.Add(New KeyValuePair(Of String, String)(op.ToString(), GetOperatorDisplayName(op)))
			Next
			Return operators
		End Function
		
		#End Region
		
		#Region "Build SQL from Condition"
		
		''' <summary>
		''' Builds SQL WHERE clause from a SingleCondition object
		''' </summary>
		Public Shared Function BuildSingleConditionSQL(ByVal condition As SingleCondition) As String
			If condition Is Nothing OrElse String.IsNullOrEmpty(condition.ColumnName) Then
				Return ""
			End If
			
			Dim columnName As String = condition.ColumnName
			Dim value As String = HelperFunctions.EscapeSql(condition.Value)
			
			Select Case condition.CompareOp
				Case ComparisonOperator.Equals
					Return $"{columnName} = '{value}'"
					
				Case ComparisonOperator.NotEquals
					Return $"{columnName} <> '{value}'"
					
				Case ComparisonOperator.GreaterThan
					Return $"{columnName} > '{value}'"
					
				Case ComparisonOperator.LessThan
					Return $"{columnName} < '{value}'"
					
				Case ComparisonOperator.GreaterOrEqual
					Return $"{columnName} >= '{value}'"
					
				Case ComparisonOperator.LessOrEqual
					Return $"{columnName} <= '{value}'"
					
				Case ComparisonOperator.Contains
					Return $"{columnName} LIKE '%{value}%'"
					
				Case ComparisonOperator.StartsWith
					Return $"{columnName} LIKE '{value}%'"
					
				Case ComparisonOperator.EndsWith
					Return $"{columnName} LIKE '%{value}'"
					
				Case ComparisonOperator.IsNull
					Return $"{columnName} IS NULL"
					
				Case ComparisonOperator.IsNotNull
					Return $"{columnName} IS NOT NULL"
					
				Case ComparisonOperator.InList
					If condition.Values IsNot Nothing AndAlso condition.Values.Count > 0 Then
						Dim escaped As List(Of String) = condition.Values.Select(Function(v) $"'{HelperFunctions.EscapeSql(v)}'").ToList()
						Return $"{columnName} IN ({String.Join(", ", escaped)})"
					End If
					Return ""
					
				Case Else
					Return $"{columnName} = '{value}'"
			End Select
		End Function
		
		''' <summary>
		''' Builds SQL from a ConditionGroup (multiple conditions with AND/OR)
		''' </summary>
		Public Shared Function BuildConditionGroupSQL(ByVal Group As ConditionGroup) As String
			If Group Is Nothing OrElse Group.Conditions Is Nothing OrElse Group.Conditions.Count = 0 Then
				Return ""
			End If
			
			Dim sqlParts As New List(Of String)()
			For Each condition As SingleCondition In Group.Conditions
				Dim sql As String = BuildSingleConditionSQL(condition)
				If Not String.IsNullOrEmpty(sql) Then
					sqlParts.Add(sql)
				End If
			Next
			
			If sqlParts.Count = 0 Then Return ""
			If sqlParts.Count = 1 Then Return sqlParts(0)
			
			Dim logicalOp As String = If(Group.LogicalOp = LogicalOperator.LogicalAnd, " AND ", " OR ")
			Return $"({String.Join(logicalOp, sqlParts)})"
		End Function
		
		''' <summary>
		''' Builds complete SQL condition from RuleCondition object
		''' </summary>
		''' <param name="ruleCondition">RuleCondition with groups</param>
		''' <returns>Complete SQL condition string</returns>
		Public Shared Function BuildConditionSQL(ByVal ruleCondition As RuleCondition) As String
			If ruleCondition Is Nothing OrElse ruleCondition.Groups Is Nothing OrElse ruleCondition.Groups.Count = 0 Then
				Return "1=1"  ' Default catch-all
			End If
			
			Dim groupSQLs As New List(Of String)()
			For Each Group As ConditionGroup In ruleCondition.Groups
				Dim sql As String = BuildConditionGroupSQL(Group)
				If Not String.IsNullOrEmpty(sql) Then
					groupSQLs.Add(sql)
				End If
			Next
			
			If groupSQLs.Count = 0 Then Return "1=1"
			If groupSQLs.Count = 1 Then Return groupSQLs(0)
			
			Dim groupLogicalOp As String = If(ruleCondition.GroupLogicalOp = LogicalOperator.LogicalAnd, " AND ", " OR ")
			Return String.Join(groupLogicalOp, groupSQLs)
		End Function
		
		#End Region
		
		#Region "Parse SQL to Condition"
		
		''' <summary>
		''' Parses a simple SQL condition into a SingleCondition object.
		''' Handles common patterns like: column = 'value', column LIKE 'value%', etc.
		''' </summary>
		''' <param name="sql">SQL condition string</param>
		''' <returns>SingleCondition object or Nothing if parsing fails</returns>
		Public Shared Function ParseSingleConditionSQL(ByVal sql As String) As SingleCondition
			If String.IsNullOrWhiteSpace(sql) Then Return Nothing
			
			sql = sql.Trim()
			Dim condition As New SingleCondition()
			
			Try
				' Handle IS NULL / IS NOT NULL
				If sql.ToUpper().EndsWith(" IS NULL") Then
					condition.ColumnName = sql.Substring(0, sql.Length - 8).Trim()
					condition.CompareOp = ComparisonOperator.IsNull
					Return condition
				End If
				
				If sql.ToUpper().EndsWith(" IS NOT NULL") Then
					condition.ColumnName = sql.Substring(0, sql.Length - 12).Trim()
					condition.CompareOp = ComparisonOperator.IsNotNull
					Return condition
				End If
				
				' Handle IN (...)
				Dim inMatch As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(sql, "^(.+?)\s+IN\s*\((.+)\)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
				If inMatch.Success Then
					condition.ColumnName = inMatch.Groups(1).Value.Trim()
					condition.CompareOp = ComparisonOperator.InList
					Dim valuesPart As String = inMatch.Groups(2).Value
					' Extract values between quotes
					Dim valueMatches As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(valuesPart, "'([^']*)'")
					For Each vm As System.Text.RegularExpressions.Match In valueMatches
						condition.Values.Add(vm.Groups(1).Value)
					Next
					Return condition
				End If
				
				' Handle LIKE patterns
				Dim likeMatch As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(sql, "^(.+?)\s+LIKE\s+'(.+)'$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
				If likeMatch.Success Then
					condition.ColumnName = likeMatch.Groups(1).Value.Trim()
					Dim pattern As String = likeMatch.Groups(2).Value
					
					If pattern.StartsWith("%") AndAlso pattern.EndsWith("%") Then
						condition.CompareOp = ComparisonOperator.Contains
						condition.Value = pattern.Substring(1, pattern.Length - 2)
					ElseIf pattern.EndsWith("%") Then
						condition.CompareOp = ComparisonOperator.StartsWith
						condition.Value = pattern.Substring(0, pattern.Length - 1)
					ElseIf pattern.StartsWith("%") Then
						condition.CompareOp = ComparisonOperator.EndsWith
						condition.Value = pattern.Substring(1)
					Else
						condition.CompareOp = ComparisonOperator.Contains
						condition.Value = pattern
					End If
					Return condition
				End If
				
				' Handle comparison operators (=, <>, >, <, >=, <=)
				Dim compMatch As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(sql, "^(.+?)\s*(=|<>|>=|<=|>|<)\s*'?([^']*)'?$")
				If compMatch.Success Then
					condition.ColumnName = compMatch.Groups(1).Value.Trim()
					Dim opStr As String = compMatch.Groups(2).Value
					condition.Value = compMatch.Groups(3).Value.Trim()
					
					Select Case opStr
						Case "=" : condition.CompareOp = ComparisonOperator.Equals
						Case "<>" : condition.CompareOp = ComparisonOperator.NotEquals
						Case ">" : condition.CompareOp = ComparisonOperator.GreaterThan
						Case "<" : condition.CompareOp = ComparisonOperator.LessThan
						Case ">=" : condition.CompareOp = ComparisonOperator.GreaterOrEqual
						Case "<=" : condition.CompareOp = ComparisonOperator.LessOrEqual
					End Select
					Return condition
				End If
				
			Catch ex As Exception
				' Parsing failed, return nothing
			End Try
			
			Return Nothing
		End Function
		
		''' <summary>
		''' Parses a complete SQL condition into a RuleCondition object.
		''' Handles simple AND/OR combinations.
		''' </summary>
		''' <param name="sql">Complete SQL condition</param>
		''' <returns>RuleCondition object</returns>
		Public Shared Function ParseConditionSQL(ByVal sql As String) As RuleCondition
			Dim result As New RuleCondition()
			
			If String.IsNullOrWhiteSpace(sql) OrElse sql.Trim() = "1=1" Then
				Return result  ' Empty/catch-all condition
			End If
			
			Try
				' Simple parsing: split by AND, treat as single group
				' More complex parsing would need a proper SQL parser
				Dim Group As New ConditionGroup()
				Group.LogicalOp = LogicalOperator.LogicalAnd
				
				' Try to split by AND (outside parentheses)
				Dim parts As String() = SplitByLogicalOperator(sql, " AND ")
				
				If parts.Length = 1 Then
					' Try OR
					parts = SplitByLogicalOperator(sql, " OR ")
					If parts.Length > 1 Then
						Group.LogicalOp = LogicalOperator.LogicalOr
					End If
				End If
				
				For Each part As String In parts
					Dim condition As SingleCondition = ParseSingleConditionSQL(part.Trim())
					If condition IsNot Nothing Then
						Group.Conditions.Add(condition)
					End If
				Next
				
				If Group.Conditions.Count > 0 Then
					result.Groups.Add(Group)
				End If
				
			Catch ex As Exception
				' Parsing failed, return empty result
			End Try
			
			Return result
		End Function
		
		''' <summary>
		''' Splits SQL by logical operator, respecting parentheses
		''' </summary>
		Private Shared Function SplitByLogicalOperator(ByVal sql As String, ByVal op As String) As String()
			' Simple implementation - split by operator ignoring case
			Dim pattern As String = System.Text.RegularExpressions.Regex.Escape(op)
			Return System.Text.RegularExpressions.Regex.Split(sql, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
		End Function
		
		#End Region
		
		#Region "Validation"
		
		''' <summary>
		''' Validates a condition has required fields
		''' </summary>
		Public Shared Function ValidateCondition(ByVal condition As SingleCondition, ByVal availableColumns As List(Of String)) As List(Of String)
			Dim errors As New List(Of String)()
			
			If condition Is Nothing Then
				errors.Add("Condition is null")
				Return errors
			End If
			
			If String.IsNullOrEmpty(condition.ColumnName) Then
				errors.Add("Column name is required")
			ElseIf availableColumns IsNot Nothing AndAlso Not availableColumns.Contains(condition.ColumnName) Then
				errors.Add($"Column '{condition.ColumnName}' does not exist in the source table")
			End If
			
			' Check if value is required for this operator
			Dim valueRequired As Boolean = condition.CompareOp <> ComparisonOperator.IsNull AndAlso condition.CompareOp <> ComparisonOperator.IsNotNull
			
			If valueRequired Then
				If condition.CompareOp = ComparisonOperator.InList Then
					If condition.Values Is Nothing OrElse condition.Values.Count = 0 Then
						errors.Add("At least one value is required for 'is one of' operator")
					End If
				Else
					If String.IsNullOrEmpty(condition.Value) Then
						errors.Add("Value is required for this operator")
					End If
				End If
			End If
			
			Return errors
		End Function
		
		''' <summary>
		''' Validates a complete RuleCondition
		''' </summary>
		Public Shared Function ValidateRuleCondition(ByVal ruleCondition As RuleCondition, ByVal availableColumns As List(Of String)) As List(Of String)
			Dim errors As New List(Of String)()
			
			If ruleCondition Is Nothing OrElse ruleCondition.Groups Is Nothing OrElse ruleCondition.Groups.Count = 0 Then
				errors.Add("At least one condition is required")
				Return errors
			End If
			
			Dim groupIndex As Integer = 1
			For Each Group As ConditionGroup In ruleCondition.Groups
				If Group.Conditions Is Nothing OrElse Group.Conditions.Count = 0 Then
					errors.Add($"Group {groupIndex}: At least one condition is required")
				Else
					Dim condIndex As Integer = 1
					For Each condition As SingleCondition In Group.Conditions
						Dim condErrors As List(Of String) = ValidateCondition(condition, availableColumns)
						For Each err As String In condErrors
							errors.Add($"Group {groupIndex}, Condition {condIndex}: {err}")
						Next
						condIndex += 1
					Next
				End If
				groupIndex += 1
			Next
			
			Return errors
		End Function
		
		#End Region
		
		#Region "Helper Methods"
		
		''' <summary>
		''' Creates a simple equals condition
		''' </summary>
		Public Shared Function CreateEqualsCondition(ByVal columnName As String, ByVal value As String) As SingleCondition
			Return New SingleCondition() With {
				.ColumnName = columnName,
				.CompareOp = ComparisonOperator.Equals,
				.Value = value
			}
		End Function
		
		''' <summary>
		''' Creates a catch-all condition (1=1)
		''' </summary>
		Public Shared Function CreateCatchAllCondition() As RuleCondition
			Return New RuleCondition()  ' Empty groups = 1=1
		End Function
		
		''' <summary>
		''' Creates a simple AND condition from multiple column=value pairs
		''' </summary>
		Public Shared Function CreateAndCondition(ByVal ParamArray pairs() As KeyValuePair(Of String, String)) As RuleCondition
			Dim result As New RuleCondition()
			Dim Group As New ConditionGroup()
			Group.LogicalOp = LogicalOperator.LogicalAnd
			
			For Each pair As KeyValuePair(Of String, String) In pairs
				Group.Conditions.Add(CreateEqualsCondition(pair.Key, pair.Value))
			Next
			
			result.Groups.Add(Group)
			Return result
		End Function
		
		''' <summary>
		''' Gets human-readable description of a condition
		''' </summary>
		Public Shared Function GetConditionDescription(ByVal condition As SingleCondition) As String
			If condition Is Nothing Then Return ""
			
			Dim opDisplay As String = GetOperatorDisplayName(condition.CompareOp)
			
			Select Case condition.CompareOp
				Case ComparisonOperator.IsNull, ComparisonOperator.IsNotNull
					Return $"{condition.ColumnName} {opDisplay}"
				Case ComparisonOperator.InList
					Dim valuesStr As String = String.Join(", ", condition.Values.Take(3))
					If condition.Values.Count > 3 Then valuesStr &= ", ..."
					Return $"{condition.ColumnName} {opDisplay} ({valuesStr})"
				Case Else
					Return $"{condition.ColumnName} {opDisplay} '{condition.Value}'"
			End Select
		End Function
		
		''' <summary>
		''' Gets human-readable description of a RuleCondition
		''' </summary>
		Public Shared Function GetRuleConditionDescription(ByVal ruleCondition As RuleCondition) As String
			If ruleCondition Is Nothing OrElse ruleCondition.Groups Is Nothing OrElse ruleCondition.Groups.Count = 0 Then
				Return "Always (default rule)"
			End If
			
			Dim groupDescs As New List(Of String)()
			
			For Each Group As ConditionGroup In ruleCondition.Groups
				Dim condDescs As New List(Of String)()
				For Each condition As SingleCondition In Group.Conditions
					condDescs.Add(GetConditionDescription(condition))
				Next
				
				Dim groupOp As String = If(Group.LogicalOp = LogicalOperator.LogicalAnd, " AND ", " OR ")
				groupDescs.Add(String.Join(groupOp, condDescs))
			Next
			
			Dim mainOp As String = If(ruleCondition.GroupLogicalOp = LogicalOperator.LogicalAnd, " AND ", " OR ")
			Return String.Join(mainOp, groupDescs)
		End Function
		
		#End Region
		
	End Class
End Namespace
