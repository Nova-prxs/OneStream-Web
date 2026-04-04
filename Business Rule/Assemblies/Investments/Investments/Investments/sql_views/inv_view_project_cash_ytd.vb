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
	Public Class inv_view_project_cash_ytd

        'Declare view and table name
		Dim viewName As String = "XFC_INV_VIEW_project_cash_ytd"

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
						        WITH CumulativeSums AS (
						            SELECT
										project_id,
										scenario,
						                year,
						                month,
						                SUM(amount) OVER (
						                    PARTITION BY project_id, scenario, year
						                    ORDER BY month
						                ) AS cumulative_amount
						            FROM 
						                XFC_INV_FACT_project_cash
						           
						        )
						        SELECT
						            year,
									scenario, 
									project_id,
						            COALESCE([1], 0) AS cash_m1,
						            COALESCE([2], [1], 0) AS cash_m2,
						            COALESCE([3], [2], [1], 0) AS cash_m3,
						            COALESCE([4], [3], [2], [1], 0) AS cash_m4,
						            COALESCE([5], [4], [3], [2], [1], 0) AS cash_m5,
						            COALESCE([6], [5], [4], [3], [2], [1], 0) AS cash_m6,
						            COALESCE([7], [6], [5], [4], [3], [2], [1], 0) AS cash_m7,
						            COALESCE([8], [7], [6], [5], [4], [3], [2], [1], 0) AS cash_m8,
						            COALESCE([9], [8], [7], [6], [5], [4], [3], [2], [1], 0) AS cash_m9,
						            COALESCE([10], [9], [8], [7], [6], [5], [4], [3], [2], [1], 0) AS cash_m10,
						            COALESCE([11], [10], [9], [8], [7], [6], [5], [4], [3], [2], [1], 0) AS cash_m11,
						            COALESCE([12], [11], [10], [9], [8], [7], [6], [5], [4], [3], [2], [1], 0) AS cash_m12
						        FROM (
						            SELECT
						                year,
										scenario,
										project_id,
						                month,
						                cumulative_amount
						            FROM CumulativeSums
						        ) AS Aggregated
						        PIVOT (
						            MAX(cumulative_amount)
						            FOR month IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])
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