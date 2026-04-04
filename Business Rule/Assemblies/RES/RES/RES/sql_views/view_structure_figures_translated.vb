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
	Public Class view_structure_figures_translated
		
		'Declare table name
		Dim tableName As String = "XFC_RES_FACT_structure_figures"
		Dim viewName As String = "XFC_RES_VIEW_structure_figures_translated"
		
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
						WITH ytd_gbp AS (
							SELECT
								pfytd.entity,
								pfytd.ud1,
								pfytd.scenario,
								pfytd.year,
								pfytd.month,
								pfytd.account,
								pfytd.intercompany,
								pfytd.ud3,
								ec.currency,
								fxrt.fxratetype_rev,
								pfytd.amount_ytd AS amount_ytd_local,
								dbo.TranslateAmount(
									pfytd.year,
									pfytd.month,
									fxrt.fxratetype_rev,
									ec.currency,
									''GBP'',
									pfytd.amount_ytd
								) AS amount_ytd_gbp
							FROM XFC_RES_VIEW_structure_figures_ytd pfytd
							INNER JOIN XFC_RES_AUX_entity_currency ec ON pfytd.entity = ec.entity
							INNER JOIN XFC_RES_AUX_scenario_fxratetype fxrt ON pfytd.scenario = fxrt.scenario
						), ytd_eur AS (
							SELECT
								entity,
								ud1,
								scenario,
								year,
								month,
								account,
								intercompany,
								ud3,
								currency,
								amount_ytd_local,
								amount_ytd_gbp,
								dbo.TranslateAmount(
									year,
									month,
									fxratetype_rev,
									''GBP'',
									''EUR'',
									amount_ytd_gbp
								) AS amount_ytd_eur
							FROM ytd_gbp
						)
				
						SELECT
							entity,
							ud1,
							scenario,
							year,
							month,
							account,
							intercompany,
							ud3,
							currency,
							amount_ytd_local,
							amount_ytd_gbp,
							amount_ytd_eur,
							amount_ytd_local - COALESCE(
								LAG(amount_ytd_local) OVER (
									PARTITION BY
										entity,
										ud1,
										scenario,
										year,
										account,
										intercompany,
										ud3
									ORDER BY month ASC
								),
								0
							) AS amount_periodic_local,
							amount_ytd_gbp - COALESCE(
								LAG(amount_ytd_gbp) OVER (
									PARTITION BY
										entity,
										ud1,
										scenario,
										year,
										account,
										intercompany,
										ud3
									ORDER BY month ASC
								),
								0
							) AS amount_periodic_gbp,
							amount_ytd_eur - COALESCE(
								LAG(amount_ytd_eur) OVER (
									PARTITION BY
										entity,
										ud1,
										scenario,
										year,
										account,
										intercompany,
										ud3
									ORDER BY month ASC
								),
								0
							) AS amount_periodic_eur
						FROM ytd_eur
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
