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
	Public Class fact_pnl_transactions
		
		'Declare table name
		Dim tableName As String = "XFC_MAIN_FACT_PnLTransactions"
		
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
					    year INTEGER NOT NULL,
					    month INTEGER NOT NULL,
					    company_id VARCHAR(50) NOT NULL,
					    account_name VARCHAR(50) NOT NULL,
					    cost_center_id VARCHAR(50) NOT NULL,
					    profit_center_id VARCHAR(50) NOT NULL,
					    business_partner_id VARCHAR(50) NOT NULL,
					    order_id VARCHAR(50) NOT NULL,
					    product_id VARCHAR(50) NOT NULL,
					    amount DECIMAL(18,2) NOT NULL,
						CONSTRAINT PK_{tableName} PRIMARY KEY (
							year, month, company_id, account_name, cost_center_id, profit_center_id, business_partner_id, order_id, product_id
						)
					)
				
					CREATE INDEX idx_year_month_company_id ON {tableName} (year, month, company_id);
					CREATE INDEX idx_account_name_cost_center_id ON {tableName} (account_name, cost_center_id);
				
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
