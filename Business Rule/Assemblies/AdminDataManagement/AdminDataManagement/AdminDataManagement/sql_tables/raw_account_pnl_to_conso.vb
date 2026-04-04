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
	Public Class raw_account_pnl_to_conso
		
		'Declare table name
		Dim tableName As String = "XFC_MAIN_RAW_AccountPnLToConso"
		
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
					    account_name VARCHAR(50),
					    [A] VARCHAR(50),
					    [B] VARCHAR(50),
					    [C] VARCHAR(50),
					    [D] VARCHAR(50),
					    [E] VARCHAR(50),
					    [F] VARCHAR(50),
					    [G] VARCHAR(50),
					    [H] VARCHAR(50),
					    [I] VARCHAR(50),
					    [J] VARCHAR(50),
					    [K] VARCHAR(50),
					    [L] VARCHAR(50),
					    [M] VARCHAR(50),
					    [N] VARCHAR(50),
					    [O] VARCHAR(50),
					    [P] VARCHAR(50),
					    [Q] VARCHAR(50),
					    [0] VARCHAR(50),
					    [R] VARCHAR(50),
					    [S] VARCHAR(50),
					    [T] VARCHAR(50),
					    [U] VARCHAR(50),
					    [V] VARCHAR(50),
					    [W] VARCHAR(50),
					    [X] VARCHAR(50),
					    [Y] VARCHAR(50),
					    [1] VARCHAR(50),
					    [2] VARCHAR(50),
					    [Z] VARCHAR(50),
					    [PD] VARCHAR(50),
					    [PC] VARCHAR(50),
					    [none] VARCHAR(50)
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
					
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

	End Class
End Namespace
