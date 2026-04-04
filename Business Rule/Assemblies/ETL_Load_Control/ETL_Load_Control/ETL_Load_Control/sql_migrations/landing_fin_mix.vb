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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class LANDING_FIN_MIX
		
		'Declare table name
		Private Shared ReadOnly TABLE_NAME As String = "XFC_LANDING_FIN_MIX"
		Dim tableName As String = TABLE_NAME
		
		#Region "Index Definitions"
		
		''' <summary>
		''' Returns the table name for this class
		''' </summary>
		Public Shared Function GetTableName() As String
			Return TABLE_NAME
		End Function
		
		''' <summary>
		''' Returns index definitions for this table: (TableName, IndexName, Columns)
		''' Used by extender_index_maintenance for dynamic index management
		''' </summary>
		Public Shared Function GetIndexDefinitions() As List(Of Tuple(Of String, String, String))
			Dim indexes As New List(Of Tuple(Of String, String, String))()
			' Index on timestamp column (critical for DELETE and range filter operations)
			indexes.Add(Tuple.Create(TABLE_NAME, "IX_LANDING_FIN_MIX_Timestamp", "[timestamp]"))
			indexes.Add(Tuple.Create(TABLE_NAME, "IX_LANDING_FIN_MIX_wbsElement", "[wbsElement]"))
			' Composite index on companyCode and timestamp for aggregated queries
			indexes.Add(Tuple.Create(TABLE_NAME, "IX_LANDING_FIN_MIX_CompanyCode_Timestamp", "[companyCode], [timestamp]"))
			' Index on Time column (critical for DWH population queries)
			indexes.Add(Tuple.Create(TABLE_NAME, "IX_LANDING_FIN_MIX_Time", "[Time]"))
			Return indexes
		End Function
		
		#End Region
		
		#Region "Get Migration Query"

        Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
				IF OBJECT_ID(N'{tableName}', N'U') IS NULL
				BEGIN
					CREATE TABLE {tableName} (
					    [companyCode] NVARCHAR(30) NULL,
					    [referenceTransaction] NVARCHAR(30) NULL,
					    [documentType] NVARCHAR(30) NULL,
					    [postingDate] INT NULL,
					    [documentNumber] NVARCHAR(30) NULL,
					    [tradingPartner] NVARCHAR(30) NULL,
					    [customerNumber] NVARCHAR(30) NULL,
					    [vendorNumber] NVARCHAR(30) NULL,
					    [itemText] NVARCHAR(256) NULL,
					    [profitCenter] NVARCHAR(30) NULL,
					    [costCenter] NVARCHAR(30) NULL,
					    [wbsElement] NVARCHAR(30) NULL,
					    [amountInLocalCurrency] DECIMAL(18,2) NULL,
					    [postingKey] NVARCHAR(30) NULL,
					    [transactionType] NVARCHAR(30) NULL,
					    [lineItemNumber] NVARCHAR(30) NULL,
					    [ledger] NVARCHAR(30) NULL,
					    [amountInTransactionCurrency] DECIMAL(18,2) NULL,
					    [transactionCurrency] NVARCHAR(30) NULL,
					    [createdBy] NVARCHAR(30) NULL,
					    [glAccountLineItem] NVARCHAR(30) NULL,
					    [timestamp] BIGINT NULL,
					    [wbsUserField] NVARCHAR(30) NULL,
					    [projectProfile] NVARCHAR(30) NULL,
					    [balanceSheetAccountGroup] NVARCHAR(30) NULL,
					    [customerAccountGroup] NVARCHAR(30) NULL,
					    [vendorAccountGroup] NVARCHAR(30) NULL,
					    [Time] NVARCHAR(7) NULL
					);
					
					-- Index on timestamp column (critical for DELETE and range filter operations)
					CREATE NONCLUSTERED INDEX IX_LANDING_FIN_MIX_Timestamp ON {tableName} ([timestamp]);
					
					-- Index on wbsElement
					CREATE NONCLUSTERED INDEX IX_LANDING_FIN_MIX_wbsElement ON {tableName} ([wbsElement]);

					-- Composite index on companyCode and timestamp for aggregated queries
					CREATE NONCLUSTERED INDEX IX_LANDING_FIN_MIX_CompanyCode_Timestamp ON {tableName} ([companyCode], [timestamp]);
					
					-- Index on Time column (critical for DWH population queries)
					CREATE NONCLUSTERED INDEX IX_LANDING_FIN_MIX_Time ON {tableName} ([Time]);
				END;
				"
				
				'Down
				Dim downQuery As String = $"
					DROP TABLE {tableName};
				"
				
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region
		
		#Region "Get Time Column Migration Query"
		
		''' <summary>
		''' Returns the SQL to add [Time] column to existing table and populate it.
		''' Run this once to migrate existing data. Creates index after population.
		''' For 49M rows, expect ~5-10 minutes execution time.
		''' </summary>
		Public Function GetTimeColumnMigrationQuery(ByVal si As SessionInfo) As String
			Try
				Dim migrationQuery As String = $"
				-- Step 1: Add Time column if it doesn't exist
				IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'{tableName}') AND name = 'Time')
				BEGIN
					ALTER TABLE {tableName} ADD [Time] NVARCHAR(7) NULL;
					PRINT 'Column [Time] added to {tableName}';
				END
				ELSE
				BEGIN
					PRINT 'Column [Time] already exists in {tableName}';
				END
				
				-- Step 2: Populate Time column for existing rows (only where NULL)
				UPDATE {tableName}
				SET [Time] = CAST(LEFT(CAST([postingDate] AS NVARCHAR(8)), 4) AS NVARCHAR(4)) + 'M' + 
				             CAST(CAST(SUBSTRING(CAST([postingDate] AS NVARCHAR(8)), 5, 2) AS INT) AS NVARCHAR(2))
				WHERE [Time] IS NULL AND [postingDate] IS NOT NULL;
				
				PRINT 'Time column populated for ' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + ' rows';
				
				-- Step 3: Create index if it doesn't exist
				IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'{tableName}') AND name = 'IX_LANDING_FIN_MIX_Time')
				BEGIN
					CREATE NONCLUSTERED INDEX IX_LANDING_FIN_MIX_Time ON {tableName} ([Time]);
					PRINT 'Index IX_LANDING_FIN_MIX_Time created';
				END
				ELSE
				BEGIN
					PRINT 'Index IX_LANDING_FIN_MIX_Time already exists';
				END
				"
				Return migrationQuery
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
				'Up
				Dim upQuery As String = $"

				"
				
				'Down
				Dim downQuery As String = $"
					TRUNCATE TABLE {tableName};
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

		#Region "Populate Landing Table"

		Public Sub PopulateLandingTable(
			ByVal si As SessionInfo,
			ByVal stgTable As String,
			ByVal fromDate As String,
			ByVal toDate As String,
			ByVal referenceColumn As String
		)
			Try
				Dim fromTimestamp As Long = 0
				Dim toTimestamp As Long = 0
				
				If Not String.IsNullOrEmpty(fromDate) Then fromTimestamp = CLng(fromDate)
				If Not String.IsNullOrEmpty(toDate) Then toTimestamp = CLng(toDate)
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					'Delete records in the bracket
					Dim deleteQuery As String = $"DELETE FROM {tableName} WHERE [{referenceColumn}] >= {fromTimestamp} AND [{referenceColumn}] <= {toTimestamp}"
					BRApi.Database.ExecuteSql(dbConn, deleteQuery, False)
					
					'Insert from STG with calculated Time column
					Dim insertQuery As String = $"
						INSERT INTO {tableName} (
							[companyCode], [referenceTransaction], [documentType], [postingDate],
							[documentNumber], [tradingPartner], [customerNumber], [vendorNumber],
							[itemText], [profitCenter], [costCenter], [wbsElement],
							[amountInLocalCurrency], [postingKey], [transactionType], [lineItemNumber],
							[ledger], [amountInTransactionCurrency], [transactionCurrency], [createdBy],
							[glAccountLineItem], [timestamp], [wbsUserField], [projectProfile],
							[balanceSheetAccountGroup], [customerAccountGroup], [vendorAccountGroup],
							[Time]
						)
						SELECT 
							[companyCode], [referenceTransaction], [documentType], [postingDate],
							[documentNumber], [tradingPartner], [customerNumber], [vendorNumber],
							[itemText], [profitCenter], [costCenter], [wbsElement],
							[amountInLocalCurrency], [postingKey], [transactionType], [lineItemNumber],
							[ledger], [amountInTransactionCurrency], [transactionCurrency], [createdBy],
							[glAccountLineItem], [timestamp], [wbsUserField], [projectProfile],
							[balanceSheetAccountGroup], [customerAccountGroup], [vendorAccountGroup],
							CAST(LEFT(CAST([postingDate] AS NVARCHAR(8)), 4) AS NVARCHAR(4)) + 'M' + 
							CAST(CAST(SUBSTRING(CAST([postingDate] AS NVARCHAR(8)), 5, 2) AS INT) AS NVARCHAR(2)) AS [Time]
						FROM {stgTable}
					"
					BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
					
				End Using
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub

		#End Region

	End Class
End Namespace
