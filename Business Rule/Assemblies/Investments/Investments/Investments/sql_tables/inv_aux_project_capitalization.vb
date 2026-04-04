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
	Public Class inv_aux_project_capitalization
		
		'Declare table name
		Dim tableName As String = "XFC_INV_AUX_project_capitalization"

		#Region "Get Migration Query"
		
        Public Function GetMigrationQuery(ByVal si As SessionInfo, type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
					IF OBJECT_ID(N'{tableName}', N'U') IS NULL
					BEGIN
						CREATE TABLE {tableName} (	
							project_id VARCHAR(255) NOT NULL,
							scenario VARCHAR(255) NOT NULL,
							year INTEGER NOT NULL,
							amount_m1 DEC(18,2),
							amount_m2 DEC(18,2),
							amount_m3 DEC(18,2),
							amount_m4 DEC(18,2),
							amount_m5 DEC(18,2),
							amount_m6 DEC(18,2),
							amount_m7 DEC(18,2),
							amount_m8 DEC(18,2),
							amount_m9 DEC(18,2),
							amount_m10 DEC(18,2),
							amount_m11 DEC(18,2),
							amount_m12 DEC(18,2),
							CONSTRAINT PK_{tableName} PRIMARY KEY (project_id, scenario, year)
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
