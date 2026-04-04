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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_update_transformation_rules
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						' Get table name
						Dim paramTableName As String = args.NameValuePairs("p_table_name")
						
						Select Case paramTableName
							
						#Region "Master Cost Centers Old to OneStream Member"
						
						Case "XFC_MAIN_MASTER_CostCentersOldToOneStreamMember"
							
							' Get mappings
							Dim dt As DataTable
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(
									dbConn,
									$"
									SELECT id_old, onestream_member_name
									FROM {paramTableName};
									",
									False
								)
							End Using
							
							If dt Is Nothing OrElse dt.Rows.Count = 0 Then Return Nothing
								
							' Delete all rules in group and add new ones
							Dim ruleGroup = BRApi.Import.Metadata.GetRuleGroup(si, "TR_CostCenter", True)
							
							For Each rule In ruleGroup.Rules
								If rule.RuleType <> 1 Then Continue For
								BRApi.Import.Metadata.DeleteRule(si, rule.UniqueID)
							Next
							
							For Each mapping In dt.Rows
								Dim newRule As New TransformRuleInfo(
									guid.NewGuid(),
									ruleGroup.UniqueID,
									mapping("id_old"),
									1,
									"NA",
									mapping("onestream_member_name"),
									False,
									"",
									Nothing,
									Nothing,
									Nothing,
									If(mapping("id_old") = "*", 1, 0),
									0
								)		
								BRApi.Import.Metadata.CreateRule(si, newRule)
							Next
							
						#End Region
						
						#Region "Raw Customers to UD1 IC"
							
						Case "XFC_MAIN_RAW_CustomersToUD1IC"
							
							' Get mappings
							Dim dt As DataTable
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(
									dbConn,
									$"
									SELECT id, customer_member_name, ic_member_name
									FROM XFC_MAIN_MASTER_BusinessPartners;
									",
									False
								)
							End Using
							
							If dt Is Nothing OrElse dt.Rows.Count = 0 Then Return Nothing
								
							' Delete all rules in group and add new ones
							Dim customerRuleGroup = BRApi.Import.Metadata.GetRuleGroup(si, "S4_Customer_UD1", True)
							
							For Each rule In customerRuleGroup.Rules
								If rule.RuleType <> 1 Then Continue For
								BRApi.Import.Metadata.DeleteRule(si, rule.UniqueID)
							Next
							
							Dim ICRuleGroup = BRApi.Import.Metadata.GetRuleGroup(si, "S4_Customer_IC", True)
							
							For Each rule In ICRuleGroup.Rules
								If rule.RuleType <> 1 Then Continue For
								BRApi.Import.Metadata.DeleteRule(si, rule.UniqueID)
							Next
							
							For Each mapping In dt.Rows
								
								Dim newRule As New TransformRuleInfo
								
								' If customer map, add mapping
								If String.IsNullOrEmpty(mapping("customer_member_name").ToString) Then
									newRule = New TransformRuleInfo(
										guid.NewGuid(),
										customerRuleGroup.UniqueID,
										mapping("id"),
										1,
										"NA",
										mapping("customer_member_name"),
										False,
										"",
										Nothing,
										Nothing,
										Nothing,
										If(mapping("id") = "*", 1, 0),
										0
									)		
									BRApi.Import.Metadata.CreateRule(si, newRule)
								End If
								
								' If no IC map, continue for
								If String.IsNullOrEmpty(mapping("ic_member_name").ToString) OrElse mapping("ic_member_name").ToString.ToLower = "none" Then _
									Continue For
							
								newRule = New TransformRuleInfo(
									guid.NewGuid(),
									ICRuleGroup.UniqueID,
									mapping("id"),
									1,
									"NA",
									mapping("ic_member_name"),
									False,
									"",
									Nothing,
									Nothing,
									Nothing,
									If(mapping("id") = "*", 1, 0),
									0
								)		
								BRApi.Import.Metadata.CreateRule(si, newRule)
								
							Next
							
						#End Region
						
						#Region "Raw Account BS to Conso"
							
						Case "XFC_MAIN_RAW_AccountBSToConso"
							
							' Get mappings
							Dim dt As DataTable
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(
									dbConn,
									$"
									SELECT account_name, conso_rubric
									FROM XFC_MAIN_MASTER_AccountRubrics
									WHERE cost_center_class_id = 'none';
									",
									False
								)
							End Using
							
							If dt Is Nothing OrElse dt.Rows.Count = 0 Then Return Nothing
								
							' Delete all rules in group and add new ones
							Dim ruleGroup = BRApi.Import.Metadata.GetRuleGroup(si, "S4_BSGL_Account", True)
							
							For Each rule In ruleGroup.Rules
								If rule.RuleType <> 1 Then Continue For
								BRApi.Import.Metadata.DeleteRule(si, rule.UniqueID)
							Next
							
							For Each mapping In dt.Rows
								Dim newRule As New TransformRuleInfo(
									guid.NewGuid(),
									ruleGroup.UniqueID,
									mapping("account_name"),
									1,
									"NA",
									mapping("conso_rubric"),
									If(
										mapping("conso_rubric").ToString.StartsWith("2") OrElse
										mapping("conso_rubric").ToString.StartsWith("3") OrElse
										mapping("conso_rubric").ToString.Contains("1VDIV"),
										True,
										False
									),
									"",
									Nothing,
									Nothing,
									Nothing,
									If(mapping("account_name") = "*", 1, 0),
									0
								)		
								BRApi.Import.Metadata.CreateRule(si, newRule)
							Next
							
						#End Region
						
						#Region "Master Flow Mappings"
							
						Case "XFC_MAIN_MASTER_FlowMappings"
							
							' Get mappings
							Dim dt As DataTable
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(
									dbConn,
									$"
									SELECT sap_flow, flow
									FROM XFC_MAIN_MASTER_FlowMappings;
									",
									False
								)
							End Using
							
							If dt Is Nothing OrElse dt.Rows.Count = 0 Then Return Nothing
								
							' Delete all rules in group and add new ones
							Dim ruleGroup = BRApi.Import.Metadata.GetRuleGroup(si, "S4_CIM_Flow", True)
							
							For Each rule In ruleGroup.Rules
								If rule.RuleType <> 1 Then Continue For
								BRApi.Import.Metadata.DeleteRule(si, rule.UniqueID)
							Next
							
							For Each mapping In dt.Rows
								Dim newRule As New TransformRuleInfo(
									guid.NewGuid(),
									ruleGroup.UniqueID,
									mapping("sap_flow"),
									1,
									"NA",
									mapping("flow"),
									False,
									"",
									Nothing,
									Nothing,
									Nothing,
									If(mapping("sap_flow") = "*", 1, 0),
									0
								)		
								BRApi.Import.Metadata.CreateRule(si, newRule)
							Next
							
							#End Region
								
						End Select
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
