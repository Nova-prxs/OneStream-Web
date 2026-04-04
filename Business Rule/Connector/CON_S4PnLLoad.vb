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

Namespace OneStream.BusinessRule.Connector.CON_S4PnLLoad
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As Object
			Try
				
				'Get the Field name list or load the data    
                Select Case args.ActionType
                Case Is = ConnectorActionTypes.GetFieldList
						
                    'Return Field Name List
					Dim fieldList As New List(Of String)
					fieldList.Add("time")
					fieldList.Add("conso_rubric")
					fieldList.Add("entity")
					fieldList.Add("cost_center_id")
					fieldList.Add("profit_center_id")
					fieldList.Add("business_partner_id")
					fieldList.Add("amount")
					
					Return fieldList
                        
                Case Is = ConnectorActionTypes.GetData
				
					' Get Workflow information
					Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si, api.WorkflowUnitPk.TimeKey)
					Dim oTime As TimeMemberSubComponents = BRApi.Finance.Time.GetSubComponentsFromName(si, wfTime)
					Dim wfYear As Integer = oTime.Year
					Dim wfMonth As Integer = oTime.Month
					Dim wfScenario As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Scenario, api.WorkflowUnitPk.ScenarioKey)
					Dim wfProfileName As String = api.WorkflowProfile.Name
					
					Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, wfProfileName, wfScenario, wfTime)
					Dim wfScenarioTypeId As Integer = BRApi.Workflow.General.GetScenarioTypeId(si, wfClusterPk)
					
					' Get profit center first letters based on the parent workflow description and the workflow channel
					Dim entityName As String = BRApi.Workflow.Metadata.GetProfile(si, api.WorkflowProfile.ParentProfileKey).Description
					Dim wfChannel As String = api.WorkflowProfile.GetAttributeValue(
						wfScenarioTypeId,
						SharedConstants.WorkflowProfileAttributeIndexes.WorkflowChannel,
						WorkflowChannel.Standard.UniqueID.ToString
					)
					
					' Prepare profit center filter based on wfChannel
					Dim paramAreProfitCentersFiltered As Boolean = False
					Dim paramProfitCenters As String = "''"
					If wfChannel <> WorkflowChannel.Standard.UniqueID.ToString AndAlso wfChannel <> WorkflowChannel.AllChannelInput.UniqueID.ToString Then
						paramAreProfitCentersFiltered = True
						' Get entityName base members and filter based on workflow channel
						Dim memberList = BRApi.Finance.Metadata.GetMembersUsingFilter(
							si, "ProfitCenters", $"U5#{entityName}.Base", True,
							, New MemberDisplayOptions(False, "en-US", False, True, True, False, False, 0)
						)
						For Each member In memberList
							If wfChannel = member.GetUDProperties.WorkflowChannel.GetPropertyItem(wfScenarioTypeId).TextValue Then _
								paramProfitCenters = If(
									paramProfitCenters = "''",
									$"'{member.Member.Name}'",
									paramProfitCenters & $", '{member.Member.Name}'"
								)
						Next
					End If
					
					' Get and process data
					Dim dt As New DataTable
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Dim dbParamInfos As New List(Of DbParamInfo) From {
							New DbParamInfo("paramYear", wfYear),
							New DbParamInfo("paramMonth", wfMonth),
							New DbParamInfo("paramAreProfitCentersFiltered", paramAreProfitCentersFiltered),
							New DbParamInfo("paramEntityName", entityName)
						}
						dt = BRApi.Database.ExecuteSql(
							dbConn,
							$"
							WITH filtered_cost_centers AS (
								SELECT *
								FROM XFC_MAIN_MASTER_CostCenters WITH(NOLOCK)
								WHERE DATEFROMPARTS(@paramYear, @paramMonth, 1) BETWEEN start_date AND end_date AND is_blocked <> 1
							), filtered_profit_centers AS (
								SELECT id
								FROM XFC_MAIN_MASTER_ProfitCenters WITH(NOLOCK)
								WHERE (
										@paramAreProfitCentersFiltered = 1
										AND entity_id IN ({paramProfitCenters})
									) OR (
										@paramAreProfitCentersFiltered <> 1
										AND entity_id LIKE '{entityName}%'
									)
							), filtered_account_rubrics AS (
								SELECT
									cc.id AS cost_center_id, ccon.id_old AS cost_center_id_old, ar.cost_center_class_id,
									ar.account_name, ar.conso_rubric
								FROM filtered_cost_centers cc
								LEFT JOIN XFC_MAIN_MASTER_CostCentersOldToNew ccon WITH(NOLOCK)
								ON cc.id = ccon.id
								LEFT JOIN XFC_MAIN_MASTER_AccountRubrics ar WITH(NOLOCK)
								ON cc.class_id = ar.cost_center_class_id
							), transactions_mapped_by_cost_center_class AS (
								-- First we check if the account has a rubric mapping with cost center class
								SELECT 
									CONCAT(ft.year, 'M', ft.month) AS time, ft.account_name, far.conso_rubric, far.cost_center_id_old AS cost_center_id,
									ft.profit_center_id,
									CASE
									    WHEN LEFT(ft.account_name, 2) IN ('71', '61') THEN 'none'
									    ELSE ft.business_partner_id
									END AS business_partner_id,
									ft.amount
								FROM (
									SELECT year, month, account_name, cost_center_id, profit_center_id, business_partner_id, amount
									FROM XFC_MAIN_FACT_PnLTransactions pnlt WITH(NOLOCK)
									WHERE year = @paramYear AND month = @paramMonth AND EXISTS (
										SELECT 1
										FROM filtered_profit_centers fpc
										WHERE pnlt.profit_center_id = fpc.id
									)
								) AS ft
								LEFT JOIN filtered_account_rubrics AS far
								ON ft.account_name = far.account_name AND ft.cost_center_id = far.cost_center_id
							)
							
							-- We check if the account has a rubric mapping with 'none' as cost center class
							SELECT
								time, conso_rubric, @paramEntityName As entity, cost_center_id, profit_center_id, business_partner_id, SUM(amount) AS amount
							FROM (
								SELECT 
									tmbccc.time, COALESCE(tmbccc.conso_rubric, ar.conso_rubric) AS conso_rubric,
									tmbccc.cost_center_id,
									tmbccc.profit_center_id, tmbccc.business_partner_id, tmbccc.amount
								FROM transactions_mapped_by_cost_center_class tmbccc
								LEFT JOIN XFC_MAIN_MASTER_AccountRubrics AS ar WITH(NOLOCK)
								ON tmbccc.account_name = ar.account_name AND tmbccc.conso_rubric IS NULL AND ar.cost_center_class_id = 'none'
							) AS subquery
							GROUP BY time, conso_rubric, cost_center_id ,profit_center_id, business_partner_id;
							",
							dbParamInfos,
							False
						)
					End Using
					api.Parser.ProcessDataTable(si, dt, False, api.ProcessInfo)
					
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace