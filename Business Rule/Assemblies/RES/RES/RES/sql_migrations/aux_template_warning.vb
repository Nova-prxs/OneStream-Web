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
	Public Class aux_template_warning

		'Declare table name
		Dim tableName As String = "XFC_RES_AUX_template_warning"

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
	                id INT IDENTITY(1,1),
	                import_date DATETIME DEFAULT GETDATE(),
	                template_type VARCHAR(10),
	                import_type VARCHAR(20),
	                entity VARCHAR(255),
	                scenario VARCHAR(50),
	                year INTEGER,
	                warning_type VARCHAR(20),
	                source_value VARCHAR(255),
	                row_detail NVARCHAR(MAX),
	                CONSTRAINT PK_{tableName} PRIMARY KEY (id)
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
