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
	Public Class Migrations
		
		'Get all the table classes
		Dim tables As New List(Of Object) From {
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.main_master_cost_center_group
		}

		#Region "Get Migration Queries"
		
        Public Function GetMigrationQueries(ByVal si As SessionInfo, ByVal type As String) As List(Of String)
            Try
				'Declare list of queries
				Dim queries As New List(Of String)
				
				'Loop through all the tables to get the queries
				For Each table In tables
					queries.Add(table.GetMigrationQuery(si, type))
				Next
				
                Return queries
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

		#Region "Get Population Queries"
		
        Public Function GetPopulationQueries(ByVal si As SessionInfo, ByVal type As String) As List(Of String)
            Try
				'Declare list of queries
				Dim queries As New List(Of String)
				
				'Loop through all the tables to get the queries
				For Each table In tables
					'Usamos AddRange para añadir la LISTA de queries, no el objeto lista en sí
					queries.AddRange(table.GetPopulationQuery(si, type))
				Next
				
                Return queries
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function
		
		#End Region

	End Class
End Namespace