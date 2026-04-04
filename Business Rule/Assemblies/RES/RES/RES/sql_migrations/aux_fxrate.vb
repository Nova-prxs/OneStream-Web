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
	Public Class aux_fxrate
		
		'Declare table name
		Dim tableName As String = "XFC_RES_AUX_fxrate"
		
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
            	CREATE TABLE XFC_RES_AUX_fxrate (
                    year INT NOT NULL,
                    month INT NOT NULL,
                    type VARCHAR(255) NOT NULL,
                    source VARCHAR(18) NOT NULL,
                    target VARCHAR(18) NOT NULL,
                    rate DEC(18, 9) NOT NULL,
                    CONSTRAINT PK_XFC_RES_AUX_fxrate PRIMARY KEY (year, month, type, source, target)
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
