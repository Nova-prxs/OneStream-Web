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
	Public Class raw_cashflowforecasting
		
		'Declare table name
		Dim tableName As String = "XFC_TRS_RAW_CashFlowForecasting"
		
		#Region "Get Migration Query"

        Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
            Try
			'Handle type input
			If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
				Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
			
			'Up - Structure matches Excel template with columns W01-W61
			Dim upQuery As String = $"
			IF OBJECT_ID(N'{tableName}', N'U') IS NULL
			BEGIN
				CREATE TABLE {tableName} (
					[Timekey] NVARCHAR(8),
					[Year] NVARCHAR(4),
					[Entity] NVARCHAR(255),
					[Account] NVARCHAR(255),
					[Flow] NVARCHAR(255),
					[W01] DECIMAL(18,2), [W02] DECIMAL(18,2), [W03] DECIMAL(18,2), [W04] DECIMAL(18,2),
					[W05] DECIMAL(18,2), [W06] DECIMAL(18,2), [W07] DECIMAL(18,2), [W08] DECIMAL(18,2),
					[W09] DECIMAL(18,2), [W10] DECIMAL(18,2), [W11] DECIMAL(18,2), [W12] DECIMAL(18,2),
					[W13] DECIMAL(18,2), [W14] DECIMAL(18,2), [W15] DECIMAL(18,2), [W16] DECIMAL(18,2),
					[W17] DECIMAL(18,2), [W18] DECIMAL(18,2), [W19] DECIMAL(18,2), [W20] DECIMAL(18,2),
					[W21] DECIMAL(18,2), [W22] DECIMAL(18,2), [W23] DECIMAL(18,2), [W24] DECIMAL(18,2),
					[W25] DECIMAL(18,2), [W26] DECIMAL(18,2), [W27] DECIMAL(18,2), [W28] DECIMAL(18,2),
					[W29] DECIMAL(18,2), [W30] DECIMAL(18,2), [W31] DECIMAL(18,2), [W32] DECIMAL(18,2),
					[W33] DECIMAL(18,2), [W34] DECIMAL(18,2), [W35] DECIMAL(18,2), [W36] DECIMAL(18,2),
					[W37] DECIMAL(18,2), [W38] DECIMAL(18,2), [W39] DECIMAL(18,2), [W40] DECIMAL(18,2),
					[W41] DECIMAL(18,2), [W42] DECIMAL(18,2), [W43] DECIMAL(18,2), [W44] DECIMAL(18,2),
					[W45] DECIMAL(18,2), [W46] DECIMAL(18,2), [W47] DECIMAL(18,2), [W48] DECIMAL(18,2),
					[W49] DECIMAL(18,2), [W50] DECIMAL(18,2), [W51] DECIMAL(18,2), [W52] DECIMAL(18,2),
					[W53] DECIMAL(18,2), [W54] DECIMAL(18,2), [W55] DECIMAL(18,2), [W56] DECIMAL(18,2),
					[W57] DECIMAL(18,2), [W58] DECIMAL(18,2), [W59] DECIMAL(18,2), [W60] DECIMAL(18,2),
					[W61] DECIMAL(18,2),
					[Actual] DECIMAL(18,2)
				)
			END;
			"
			
			'Down
			Dim downQuery As String = $"
				IF OBJECT_ID(N'{tableName}', N'U') IS NOT NULL
				BEGIN
					DROP TABLE {tableName};
				END
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
				
				'Up - This table is populated directly from Excel import
				Dim upQuery As String = $"
					-- RAW table is populated directly from Excel import via Data Adapter
					-- No population query needed here
					PRINT 'RAW_CashFlowForecasting: Data loaded from Excel file';
				"
				
				'Down
				Dim downQuery As String = $"
					TRUNCATE TABLE {tableName};
					PRINT 'RAW_CashFlowForecasting: Table truncated';
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region
		


	End Class
End Namespace