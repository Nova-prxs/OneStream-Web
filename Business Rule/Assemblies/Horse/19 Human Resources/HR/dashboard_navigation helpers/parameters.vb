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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.parameters
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						'Dim names As New List(Of String)()
						'names.Add("MyDataSet")
						'Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet						
						' VARIABLES
						Dim dt As DataTable = Nothing
						Dim sql As String = String.Empty
						
						If args.DataSetName.XFEqualsIgnoreCase("UD1_LaborType") Then

							sql = "
								WITH ALL_TR as (
									SELECT 
										  SRG.RuleGroupName
										, SR.RuleName
										, SR.OutputValue
										, M.Description
									FROM StageRuleGroups SRG
									LEFT JOIN StageRules SR
										ON SR.RulesGroupKey = SRG.UniqueId
									LEFT JOIN Member M
										ON M.Name = SR.OutputValue
									WHERE 1=1
										AND SRG.RuleGroupName = 'TR_Customer_Labor_Type'	
								)
							
								SELECT 
									  CASE WHEN RuleName='LaborType' THEN 'All' ELSE RuleName END as id
									, CASE WHEN RuleName='LaborType' THEN 'All' ELSE RuleName END as description
								FROM ALL_TR
								GROUP BY
									  RuleName									
							"												
							Return ExecuteSQL(si, sql)
						Else If args.DataSetName.XFEqualsIgnoreCase("UD2_CostTypology") Then
							sql = "
								WITH ALL_TR as (
									SELECT 
										  SRG.RuleGroupName
										, SR.RuleName
										, SR.OutputValue
										, M.Description
									FROM StageRuleGroups SRG
									LEFT JOIN StageRules SR
										ON SR.RulesGroupKey = SRG.UniqueId
									LEFT JOIN Member M
										ON M.Name = SR.OutputValue
									WHERE 1=1
										AND SRG.RuleGroupName = 'TR_Product_Labor_Cost_Type'	
								)
							
								SELECT 
									  OutputValue as id
									, OutputValue as description
								FROM ALL_TR
								GROUP BY
									  OutputValue									
							"	
							
						Else If args.DataSetName.XFEqualsIgnoreCase("UD3_Function") Then
							sql = "
								WITH ALL_TR as (
									SELECT 
										  SRG.RuleGroupName
										, SR.RuleName
										, SR.OutputValue
										, M.Description
									FROM StageRuleGroups SRG
									LEFT JOIN StageRules SR
										ON SR.RulesGroupKey = SRG.UniqueId
									LEFT JOIN Member M
										ON M.Name = SR.OutputValue
									WHERE 1=1
										AND SRG.RuleGroupName = 'TR_CostCenter'	
								)
							
								SELECT 
									  Description as id
									, Description as description
								FROM ALL_TR
								GROUP BY
									  Description									
							"		
						Else If args.DataSetName.XFEqualsIgnoreCase("Scenarios") Then
							sql="
								SELECT 
									  Name as id
									, Description as description
								FROM Member
								WHERE 1=1 
									AND DimId = 0
									AND (	Name LIKE '%Bu%'
											OR Name LIKE '%F%'
											OR Name LIKE '%RF%'
											OR Name ='Actual'
										)
							
								UNION ALL 
							
								SELECT 'Actual-1' as id, 'Actual-1' as description
							
								ORDER BY id Asc
							"
							
						Else If args.DataSetName.XFEqualsIgnoreCase("Entity") Then
							sql="
																						
								SELECT 'All' as id, 'All' as description, 1 as orden
							
								UNION ALL 
							
								SELECT 
									  REPLACE(Name, 'R','') as id
									, Description as description
									, 2 as orden
								FROM Member
								WHERE 1=1 
									AND DimId = 6
									AND LEN(Name) = 5 
										
								ORDER BY orden, id Asc
							"						
						Else If args.DataSetName.XFEqualsIgnoreCase("FunctionGroup") Then
							sql = $"
								SELECT 
									function_group as id
									, function_group as description
								FROM XFC_HR_MASTER_Function
								GROUP BY function_group
							"
						End If
						dt = ExecuteSQL(si, sql)
						dt.TableName = args.DataSetName
						Return dt
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		
		#Region "Helper Functions"
		
		Private Function ExecuteSQL(ByVal si As SessionInfo, ByVal sqlCmd As String)        
			Dim dt As DataTable = Nothing
	        Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)                           
	            dt = BRAPi.Database.ExecuteSql(dbConnApp, sqlCmd, True)			
	        End Using   
			Return dt	
	    End Function
		
		#End Region
						
	End Class
End Namespace
