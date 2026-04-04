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
	Public Class raw_cashdebtposition
		
		'Declare table name
		Dim tableName As String = "XFC_TRS_RAW_CashDebtPosition"
		
		#Region "Get Migration Query"

        Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
            Try
			'Handle type input
			If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
				Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
			
			'Up - Structure matches Excel template with StartWeek, EndWeek, and single EOM column
			Dim upQuery As String = $"
			IF OBJECT_ID(N'{tableName}', N'U') IS NULL
			BEGIN
				CREATE TABLE {tableName} (
					[Timekey] NVARCHAR(8),
					[Year] NVARCHAR(10),
					[Entity] NVARCHAR(255),
					[Account] NVARCHAR(255),
					[Flow] NVARCHAR(255),
					[Bank] NVARCHAR(255),
					[StartWeek] DECIMAL(18,2),
					[EndWeek] DECIMAL(18,2),
					[EOM] DECIMAL(18,2)
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
					PRINT 'RAW_CashDebtPosition: Data loaded from Excel file';
				"
				
				'Down
				Dim downQuery As String = $"
					TRUNCATE TABLE {tableName};
					PRINT 'RAW_CashDebtPosition: Table truncated';
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region
		


	End Class
End Namespace