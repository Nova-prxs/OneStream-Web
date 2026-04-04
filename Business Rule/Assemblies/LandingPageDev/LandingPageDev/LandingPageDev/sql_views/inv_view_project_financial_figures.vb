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
	Public Class inv_view_project_financial_figures

        'Declare view and table name
		Dim viewName As String = "XFC_INV_VIEW_project_financial_figures"
		Dim tableName As String = "XFC_INV_FACT_project_financial_figures"

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
						            project_id,
						            scenario,
						            year,
						            Decided AS decided_ytd,
									Delivered AS delivered_ytd,
									Ordered AS ordered_ytd,
									Requirement AS requirement_ytd
						        FROM 
						            (
										SELECT project_id, scenario, year, type, amount_ytd
										FROM (
											SELECT
												project_id,
												scenario,
												year,
												type,
												amount_ytd,
												ROW_NUMBER() OVER(
													PARTITION BY project_id, scenario, year, type
													ORDER BY month DESC
												) AS rn
											FROM {tableName}
										) AS subquery
										WHERE
											rn = 1
									) AS SourceTable
						        PIVOT
						        (
						            MAX(amount_ytd)
						            FOR type IN (Decided, Delivered, Ordered, Requirement)
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
