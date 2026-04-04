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
	Public Class rnd_view_project_internal_expense_proportion

        'Declare view and table name
		Dim viewName As String = "XFC_RND_VIEW_project_internal_expense_proportion"
		Dim tableName As String = "XFC_RND_FACT_project_internal_expense_proportion"

		#Region "Get Migration Query"
		
        Public Function GetMigrationQuery(ByVal si As SessionInfo, type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
					IF OBJECT_ID(N'{viewName}', N'V') IS NULL
					BEGIN
					    EXEC('CREATE VIEW {viewName} AS
					        SELECT
					            year,
								scenario,
					            CONVERT(INT, COALESCE([1300], 0)) AS [1300],
					            CONVERT(INT, COALESCE([1301], 0)) AS [1301],
					            CONVERT(INT, COALESCE([1302], 0)) AS [1302],
					            CONVERT(INT, COALESCE([1303], 0)) AS [1303],
					            CONVERT(INT, COALESCE([585], 0)) AS [585],
					            CONVERT(INT, COALESCE([592], 0)) AS [592],
					            CONVERT(INT, COALESCE([611], 0)) AS [611],
					            CONVERT(INT, COALESCE([671], 0)) AS [671]
					        FROM 
					        (
					            SELECT year, scenario, company_id, proportion
					            FROM {tableName}
					        ) AS SourceTable
					        PIVOT 
					        (
					            SUM(proportion)
					            FOR company_id IN ([1300], [1301], [1302], [1303], [585], [592], [611], [671])
					        ) AS PivotTable
					    ')
					END;
				"
				
				'Down
				Dim downQuery As String = $"
					DROP VIEW IF EXISTS {viewName};
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
