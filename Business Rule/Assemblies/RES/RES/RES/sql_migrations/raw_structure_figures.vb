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
	Public Class raw_structure_figures
		
		'Declare table name
		Dim tableName As String = "XFC_RES_RAW_structure_figures"
		
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
						entity VARCHAR(255),
						ud3 VARCHAR(255),
						ud1 VARCHAR(255),
						intercompany VARCHAR(255),
						account VARCHAR(255),
						ref_scenario VARCHAR(255),
						m1 DECIMAL(18, 2),
						m2 DECIMAL(18, 2),
						m3 DECIMAL(18, 2),
						m4 DECIMAL(18, 2),
						m5 DECIMAL(18, 2),
						m6 DECIMAL(18, 2),
						m7 DECIMAL(18, 2),
						m8 DECIMAL(18, 2),
						m9 DECIMAL(18, 2),
						m10 DECIMAL(18, 2),
						m11 DECIMAL(18, 2),
						m12 DECIMAL(18, 2),
						bm1 DECIMAL(18, 4),
						bm2 DECIMAL(18, 4),
						bm3 DECIMAL(18, 4),
						bm4 DECIMAL(18, 4),
						bm5 DECIMAL(18, 4),
						bm6 DECIMAL(18, 4),
						bm7 DECIMAL(18, 4),
						bm8 DECIMAL(18, 4),
						bm9 DECIMAL(18, 4),
						bm10 DECIMAL(18, 4),
						bm11 DECIMAL(18, 4),
						bm12 DECIMAL(18, 4),
						p1m1 DECIMAL(18, 4),
						p1m2 DECIMAL(18, 4),
						p1m3 DECIMAL(18, 4),
						p1m4 DECIMAL(18, 4),
						p1m5 DECIMAL(18, 4),
						p1m6 DECIMAL(18, 4),
						p1m7 DECIMAL(18, 4),
						p1m8 DECIMAL(18, 4),
						p1m9 DECIMAL(18, 4),
						p1m10 DECIMAL(18, 4),
						p1m11 DECIMAL(18, 4),
						p1m12 DECIMAL(18, 4),
						p2m1 DECIMAL(18, 4),
						p2m2 DECIMAL(18, 4),
						p2m3 DECIMAL(18, 4),
						p2m4 DECIMAL(18, 4),
						p2m5 DECIMAL(18, 4),
						p2m6 DECIMAL(18, 4),
						p2m7 DECIMAL(18, 4),
						p2m8 DECIMAL(18, 4),
						p2m9 DECIMAL(18, 4),
						p2m10 DECIMAL(18, 4),
						p2m11 DECIMAL(18, 4),
						p2m12 DECIMAL(18, 4)
					);
				END;
				"
				
				'Down
				Dim downQuery As String = $"
					DROP TABLE IF EXISTS {tableName};
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
				'Up
				Dim upQuery As String = $"
					
				"
				
				'Down
				Dim downQuery As String = $"
					
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

	End Class
End Namespace
