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
	Public Class master_companies
		
		'Declare table name
		Dim tableName As String = "XFC_TRS_MASTER_Companies"
		
		#Region "Get Migration Query"

        Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				
			'Up - Create table with all company master data
			Dim upQuery As String = $"
			IF OBJECT_ID(N'{tableName}', N'U') IS NULL
			BEGIN
				CREATE TABLE {tableName} (
					[id] INT PRIMARY KEY,
					[name] NVARCHAR(255) NOT NULL,
					[tax_id] NVARCHAR(50) NULL,
					[currency_code] NVARCHAR(10) NULL,
					[region] NVARCHAR(50) NULL,
					[country] NVARCHAR(100) NULL
				)
			END
			"				'Down - Drop table
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
				
			'Up - Insert all company master data from existing dictionaries
			Dim upQuery As String = $"
			
			TRUNCATE TABLE {tableName};
			
			INSERT INTO {tableName} (id, name, tax_id, currency_code, region, country)
			VALUES

				(0, 'HTD', NULL, NULL, 'HTD', 'HTD'),
				
				-- Horse Group Companies (Europe)
				(1300, 'Horse Powertrain Solution', NULL, 'EUR', 'Europe', 'Spain'),
				(1301, 'Horse Spain', NULL, 'EUR', 'Europe', 'Spain'),
				(1302, 'OYAK HORSE', NULL, 'TRY', 'Europe', 'Turkey'),
				(611, 'Horse Romania', NULL, 'RON', 'Europe', 'Romania'),
				(671, 'Horse Aveiro', NULL, 'EUR', 'Europe', 'Portugal'),
				
				-- Horse Group Companies (LATAM)
				(1303, 'Horse Brazil', NULL, 'BRL', 'LATAM', 'Brazil'),
				(585, 'Horse Chile', NULL, 'CLP', 'LATAM', 'Chile'),
				(592, 'Horse Argentina', NULL, 'ARS', 'LATAM', 'Argentina'),
			
			"
				Dim downQuery As String = $"
				-- Clear Companies Master Table
				TRUNCATE TABLE {tableName};
				PRINT 'Companies Master Table cleared successfully.';
				"
				
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

	End Class
End Namespace
