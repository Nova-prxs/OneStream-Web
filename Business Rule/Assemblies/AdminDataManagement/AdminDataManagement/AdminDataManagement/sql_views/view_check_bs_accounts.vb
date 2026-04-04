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
	Public Class view_check_bs_accounts

        'Declare view and table name
		Dim viewName As String = "XFC_MAIN_VIEW_CHECK_BSAccounts"
		Dim tableName As String = "XFC_MAIN_MASTER_Accounts"

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
							WITH filtered_accounts AS (
								SELECT name, description
								FROM XFC_MAIN_MASTER_Accounts
								WHERE name NOT LIKE ''6%'' AND name NOT LIKE ''7%''
							), filtered_account_rubrics AS (
								SELECT account_name
								FROM XFC_MAIN_MASTER_AccountRubrics
								WHERE cost_center_class_id = ''none''
							)

							SELECT name AS account_name, description AS account_description, '''' AS conso_rubric
							FROM filtered_accounts fa
							WHERE NOT EXISTS (
								SELECT 1
								FROM filtered_account_rubrics far
								WHERE fa.name = far.account_name
							)
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
