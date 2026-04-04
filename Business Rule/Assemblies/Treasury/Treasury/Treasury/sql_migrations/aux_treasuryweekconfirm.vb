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
	Public Class aux_treasuryweekconfirm
		
		'Declare table name
		Dim tableName As String = "XFC_TRS_AUX_TreasuryWeekConfirm"
		
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
						[Entity] NVARCHAR(255) NOT NULL,
						[Week] INT NOT NULL,
						[Year] INT NOT NULL,
						[CashDebt] BIT DEFAULT 0,
						[CashFlow] BIT DEFAULT 0,
						CONSTRAINT PK_{tableName} PRIMARY KEY (Entity, Week, Year)
					)
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
		
		#Region "Get Population Query"

        Public Function GetPopulationQuery(ByVal si As SessionInfo, type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Population type must be 'up' or 'down'"))
				
				'Up - Initialize with default values (all weeks set to 0/false)
				Dim upQuery As String = $"
					-- This table tracks confirmation status for Cash Debt Position and Cash Flow Forecasting reports
					-- Each row represents a specific Entity + Week + Year combination
					-- CashDebt and CashFlow bits indicate if that week's data has been confirmed/validated
					
				-- Populate from CashDebtPosition RAW data (if available)
				IF OBJECT_ID('XFC_TRS_RAW_CashDebtPosition', 'U') IS NOT NULL
				BEGIN
					INSERT INTO {tableName} (Entity, Week, Year, CashDebt, CashFlow)
					SELECT DISTINCT
						raw.Entity,
						auxDate.weekNumber AS Week,
						CAST(auxDate.year AS INT) AS Year,
						0 AS CashDebt,
						0 AS CashFlow
					FROM XFC_TRS_RAW_CashDebtPosition raw
					INNER JOIN XFC_TRS_AUX_Date auxDate 
						ON CONVERT(VARCHAR(8), auxDate.weekStartDate, 112) = raw.Timekey
						AND auxDate.fulldate = auxDate.weekStartDate
					WHERE raw.Entity IS NOT NULL
						AND auxDate.year IS NOT NULL
						-- Only insert if the combination Entity+Week+Year does NOT exist yet
						AND NOT EXISTS (
							SELECT 1 
							FROM {tableName} existing
							WHERE existing.Entity = raw.Entity
							AND existing.Week = auxDate.weekNumber
							AND existing.Year = CAST(auxDate.year AS INT)
						);
					
					DECLARE @RowsAffectedCashDebt INT = @@ROWCOUNT;
					PRINT 'TreasuryWeekConfirm population from CashDebtPosition: ' + CAST(@RowsAffectedCashDebt AS VARCHAR(10)) + ' new rows inserted.';
				END

				-- Populate from CashFlowForecasting RAW data (if available)
				IF OBJECT_ID('XFC_TRS_RAW_CashFlowForecasting', 'U') IS NOT NULL
				BEGIN
					INSERT INTO {tableName} (Entity, Week, Year, CashDebt, CashFlow)
					SELECT DISTINCT
						raw.Entity,
						auxDate.weekNumber AS Week,
						CAST(auxDate.year AS INT) AS Year,
						0 AS CashDebt,
						0 AS CashFlow
					FROM XFC_TRS_RAW_CashFlowForecasting raw
					INNER JOIN XFC_TRS_AUX_Date auxDate 
						ON CONVERT(VARCHAR(8), auxDate.weekStartDate, 112) = raw.Timekey
						AND auxDate.fulldate = auxDate.weekStartDate
					WHERE raw.Entity IS NOT NULL
						AND auxDate.year IS NOT NULL
						-- Only insert if the combination Entity+Week+Year does NOT exist yet
						AND NOT EXISTS (
							SELECT 1 
							FROM {tableName} existing
							WHERE existing.Entity = raw.Entity
							AND existing.Week = auxDate.weekNumber
							AND existing.Year = CAST(auxDate.year AS INT)
						);
					
					DECLARE @RowsAffectedCashFlow INT = @@ROWCOUNT;
					PRINT 'TreasuryWeekConfirm population from CashFlowForecasting: ' + CAST(@RowsAffectedCashFlow AS VARCHAR(10)) + ' new rows inserted.';
				END
					
					PRINT 'TreasuryWeekConfirm population completed successfully.';
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

	End Class
End Namespace
