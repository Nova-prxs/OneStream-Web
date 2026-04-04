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
	Public Class stg_fin_mix
		
		'Declare table name
		Dim tableName As String = "XFC_STG_FIN_MIX"
		
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
					    [companyCode] NVARCHAR(30) NULL,
					    [referenceTransaction] NVARCHAR(30) NULL,
					    [documentType] NVARCHAR(30) NULL,
					    [postingDate] INT NULL,
					    [documentNumber] NVARCHAR(30) NULL,
					    [tradingPartner] NVARCHAR(30) NULL,
					    [customerNumber] NVARCHAR(30) NULL,
					    [vendorNumber] NVARCHAR(30) NULL,
					    [itemText] NVARCHAR(256) NULL,
					    [profitCenter] NVARCHAR(30) NULL,
					    [costCenter] NVARCHAR(30) NULL,
					    [wbsElement] NVARCHAR(30) NULL,
					    [amountInLocalCurrency] DECIMAL(18,2) NULL,
					    [postingKey] NVARCHAR(30) NULL,
					    [transactionType] NVARCHAR(30) NULL,
					    [lineItemNumber] NVARCHAR(30) NULL,
					    [ledger] NVARCHAR(30) NULL,
					    [amountInTransactionCurrency] DECIMAL(18,2) NULL,
					    [transactionCurrency] NVARCHAR(30) NULL,
					    [createdBy] NVARCHAR(30) NULL,
					    [glAccountLineItem] NVARCHAR(30) NULL,
					    [timestamp] BIGINT NULL,
					    [wbsUserField] NVARCHAR(30) NULL,
					    [projectProfile] NVARCHAR(30) NULL,
					    [balanceSheetAccountGroup] NVARCHAR(30) NULL,
					    [customerAccountGroup] NVARCHAR(30) NULL,
					    [vendorAccountGroup] NVARCHAR(30) NULL
					)
				END;
				"
				
				'Down
				Dim downQuery As String = $"
					DROP TABLE {tableName};
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
