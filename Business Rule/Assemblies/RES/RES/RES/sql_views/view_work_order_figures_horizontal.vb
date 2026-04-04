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
	Public Class view_work_order_figures_horizontal
		
		'Declare table name
		Dim tableName As String = "XFC_RES_FACT_work_order_figures"
		Dim viewName As String = "XFC_RES_VIEW_work_order_figures_horizontal"
		
		#Region "Get Migration Query"

        Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
				IF OBJECT_ID(N'{viewName}', N'V') IS NULL
				BEGIN
					EXEC('
						CREATE VIEW {viewName} AS
						SELECT 
						    entity,
						    wo_id,
						    scenario,
						    year,
						    account,
						    intercompany,
						    ud3,
						    COALESCE([1], 0) AS m1,
						    COALESCE([2], 0) AS m2,
						    COALESCE([3], 0) AS m3,
						    COALESCE([4], 0) AS m4,
						    COALESCE([5], 0) AS m5,
						    COALESCE([6], 0) AS m6,
						    COALESCE([7], 0) AS m7,
						    COALESCE([8], 0) AS m8,
						    COALESCE([9], 0) AS m9,
						    COALESCE([10], 0) AS m10,
						    COALESCE([11], 0) AS m11,
						    COALESCE([12], 0) AS m12
						FROM
						(
						    SELECT
						        entity,
						        wo_id,
						        scenario,
						        year,
						        month,
						        account,
						        intercompany,
						        ud3,
						        amount
						    FROM {tableName}
						) AS SourceTable
						PIVOT
						(
						    SUM(amount)
						    FOR month IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])
						) AS PivotTable;
					');
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
