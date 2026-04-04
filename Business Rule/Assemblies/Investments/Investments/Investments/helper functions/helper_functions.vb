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
	Public Class helper_functions
		
		#Region "Get Company IDs for User"
		
		Public Function GetCompanyIDsForUser(ByVal si As SessionInfo) As List(Of Integer)
			'Declare a dictionary of security groups and it's company id and a company id list
			Dim securityGroupToCompanyIDDict As New Dictionary(Of String, Integer) From {
				{"Horse Group", 0},
				{"Horse Holding", 1300},
				{"Horse Argentina", 592},
				{"Horse Chile", 585},
				{"Horse Portugal", 671},
				{"Horse Romania", 611},
				{"Horse Spain", 1301},
				{"Horse Turkey", 1302},
				{"Horse_Brazil", 1303}
			}
			Dim companyIDList As New List(Of Integer)
			
			'Check user security groups and add company ids to the list
			If BRApi.Security.Authorization.IsUserInAdminGroup(si) Then
				companyIDList.Add(0)
			Else
				For Each securityGroupToCompanyID As KeyValuePair(Of String, Integer) In securityGroupToCompanyIDDict
					If BRApi.Security.Authorization.IsUserInGroup(si, securityGroupToCompanyID.Key) Then companyIDList.Add(securityGroupToCompanyID.value)
				Next
			End If
			
			Return companyIDList
			
		End Function
		
		#End Region

        #Region "Get Confirmed Months"
		
		Public Function GetConfirmedMonths(ByVal si As SessionInfo, ByVal type As String,
			ByVal company As Integer, ByVal scenario As String, ByVal year As String
		) As DataTable
			'Declare db param infos and dt
			Dim dbParamInfos As New List(Of DbParamInfo) From {
				New DbParamInfo("paramType", type),
				New DbParamInfo("paramCompany", company),
				New DbParamInfo("paramScenario", scenario),
				New DbParamInfo("paramYear", year)
			}
			Dim dt As New DataTable()
			'Query confirmed months
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				dt = BRApi.Database.ExecuteSql(
					dbConn,
					"
					SELECT *
					FROM XFC_INV_AUX_confirmed_month WITH(NOLOCK)
					WHERE type = @paramType
						AND company_id = @paramCompany
						AND scenario = @paramScenario
						AND year = @paramYear
					",
					dbParamInfos, False
				)
			End Using
			
			Return dt
			
		End Function
		
		#End Region

        #Region "Set Month Confirmation"
		
		Public Sub SetMonthConfirmation(ByVal si As SessionInfo, ByVal type As String,
			ByVal company As Integer, ByVal scenario As String, ByVal year As String,
			ByVal month As Integer, ByVal confirm As Boolean)
			'Declare db param infos
			Dim dbParamInfos As New List(Of DbParamInfo) From {
				New DbParamInfo("paramType", type),
				New DbParamInfo("paramCompany", company),
				New DbParamInfo("paramScenario", scenario),
				New DbParamInfo("paramYear", year),
				New DbParamInfo("paramMonth", month),
				New DbParamInfo("paramConfirm", If(confirm, 1, 0))
			}
			'Query confirmed months
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				BRApi.Database.ExecuteSql(
					dbConn,
					$"
					MERGE INTO XFC_INV_AUX_confirmed_month AS target
					USING (
						SELECT
							@paramType AS type,
							@paramCompany AS company_id,
							@paramScenario AS scenario,
							@paramYear AS year
					) AS source
					ON target.type = source.type
					AND target.company_id = source.company_id
					AND target.scenario = source.scenario
					AND target.year = source.year
					WHEN MATCHED THEN
					    UPDATE SET
							M{month} = @paramConfirm
					WHEN NOT MATCHED THEN
					    INSERT (
					        type, company_id, scenario, year, M{month}
					    )
					    VALUES (
					        source.type, source.company_id, source.scenario, source.year, @paramConfirm
					    );
					",
					dbParamInfos, False
				)
			End Using
			
		End Sub
		
		#End Region
		
		#Region "Get Last RF"
		
		Public Function GetLastRF( ByVal si As SessionInfo, ByVal type As String,
			ByVal company As Integer, ByVal scenario As String, ByVal year As Integer
		) As String
			'Declare lastRF, db param infos and table name
			Dim lastRF As String = String.Empty
			Dim dbParamInfos As New List(Of DbParamInfo) From {
				New DbParamInfo("paramType", type),
				New DbParamInfo("paramCompany", company),
				New DbParamInfo("paramScenario", scenario),
				New DbParamInfo("paramYear", year)
			}
			Dim tableName As String = If(
				type.ToLower = "cash",
				"
					XFC_INV_FACT_project_cash pc WITH(NOLOCK)
					LEFT JOIN XFC_INV_MASTER_project p WITH(NOLOCK) ON pc.project_id = p.project
				",
				"
					XFC_INV_FACT_asset_depreciation ad WITH(NOLOCK)
					LEFT JOIN XFC_INV_MASTER_asset a WITH(NOLOCK) ON ad.asset_id = a.id
					LEFT JOIN XFC_INV_MASTER_project p WITH(NOLOCK) ON a.project_id = p.project
				"
			)
			
			'Get the last rf
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim dt As DataTable = BRApi.Database.ExecuteSql(
					dbConn,
					$"
					SELECT TOP(1) scenario
					FROM {tableName}
					WHERE
						year = @paramYear - 1
						AND scenario LIKE 'RF%'
						AND (
							@paramCompany = 0
							OR p.company_id = @paramCompany
						)
					ORDER BY CAST(REPLACE(LEFT(scenario, 4), 'RF', '') AS INTEGER) DESC
					",
					dbParamInfos, False
				)
				lastRF = If(
					dt.Rows.Count > 0,
					dt.Rows(0)("scenario"),
					"No RF in previous year"
				)
			End Using
			
			Return lastRF
		End Function
		
		#End Region
		
		#Region "Get Forecast Month"
		
		Public Function GetForecastMonth(ByVal si As SessionInfo, ByVal scenario As String) As Integer
			'Get first 4 characters
			Dim scenarioShorted As String = Left(scenario, 4).ToLower
			Return If(scenarioShorted.StartsWith("rf"), CInt(scenarioShorted.Replace("rf", "")) - 1, 0)
		End Function
		
		#End Region
		
		#Region "Get Base Entity"
		
		Public Function GetBaseEntity(ByVal si As SessionInfo, ByVal entityId As String) As String
			'Get parent entity member
			Dim parentEntityMemberName As String = Me.GetParentEntity(si, entityId)
			
			'Get children member names
			Dim baseEntityNames As String = String.Join(
				",",
				BRApi.Finance.Metadata.GetMembersUsingFilter(
					si,
					"Entities",
					$"E#{parentEntityMemberName}.Base",
					True
				).Select(Function(member) member.Member.Name).ToArray()
			)
			
			Return baseEntityNames
		End Function
		
		#End Region
		
		#Region "Get Parent Entity"
		
		Public Function GetParentEntity(ByVal si As SessionInfo, ByVal entityId As String) As String
			'Declare parent to base entity dict
			Dim parentToBaseEntityDict As New Dictionary(Of String, String) From {
				{"0", "Horse_Group"},
				{"1300", "R1300"},
				{"1301", "R1301"},
				{"1302", "R1302"},
				{"1303", "R1303"},
				{"611", "R0611"},
				{"671", "R0671"},
				{"592", "R0592"},
				{"585", "R0585"}
			}
			
			Return parentToBaseEntityDict(entityId)
		End Function
		
		#End Region
		
		#Region "Get Time Filter From Forecast Month"
		
		Public Function GetTimeFilterFromForecastMonth(ByVal si As SessionInfo, ByVal year As Integer, ByVal forecastMonth As Integer) As String
			'Declare time filter string
			Dim timeFilter As String = String.Empty
			
			'Loop through each month to be processed
			For i As Integer = forecastMonth + 1 To 12
				timeFilter = If(String.IsNullOrEmpty(timeFilter), $"T#{year}M{i}", $"{timeFilter}, T#{year}M{i}")
			Next
			
			Return timeFilter
		End Function
		
		#End Region
		
		#Region "Check Scenario Year Confirmation"
		
		Public Function CheckScenarioYearConfirmation(
			ByVal si As SessionInfo, ByVal entityId As String,
			ByVal scenario As String, ByVal year As Integer
		) As Boolean
			'Declare db param infos
			Dim dbParamInfos As New List(Of DbParamInfo) From {
				New DbParamInfo("paramCompany", entityId),
				New DbParamInfo("paramYear", year),
				New DbParamInfo("paramScenario", scenario)
			}
			
			'Get confirmation
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim dt As DataTable = BRApi.Database.ExecuteSql(
					dbConn,
					"
						SELECT is_confirmed
						FROM XFC_INV_AUX_confirmed_scenario_year
						WHERE company_id = @paramCompany
							AND year = @paramYear
							AND scenario = @paramScenario
					",
					dbParamInfos,
					False
				)
				If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
					Return If(dt.Rows(0)("is_confirmed") = "Completed", True, False)
				Else
					Return False
				End If
			End Using
			
			Return False
		End Function
		
		#End Region

	End Class
End Namespace
