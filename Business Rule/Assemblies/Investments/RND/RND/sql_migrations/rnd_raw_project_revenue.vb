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
	Public Class rnd_raw_project_revenue
		
		'Declare table name
		Dim tableName As String = "XFC_RND_RAW_project_revenue"
		
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
					    year INT,
					    company_name VARCHAR(255),
					    responsible_dir VARCHAR(100),
					    job VARCHAR(100),
					    activity VARCHAR(100),
					    project VARCHAR(50),
						product_family VARCHAR(255),
					    client VARCHAR(50),
						account VARCHAR(50),
						info_provider VARCHAR(255),
					    scenario VARCHAR(50),
						m1 DEC(18, 2),
						m2 DEC(18, 2),
						m3 DEC(18, 2),
						m4 DEC(18, 2),
						m5 DEC(18, 2),
						m6 DEC(18, 2),
						m7 DEC(18, 2),
						m8 DEC(18, 2),
						m9 DEC(18, 2),
						m10 DEC(18, 2),
						m11 DEC(18, 2),
						m12 DEC(18, 2)
					)
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
					TRUNCATE TABLE {tableName}
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

	End Class
End Namespace
