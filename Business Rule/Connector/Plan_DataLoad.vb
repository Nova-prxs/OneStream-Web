Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace OneStream.BusinessRule.Connector.Plan_DataLoad
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As Object
			Try

				
				
					Dim sWorkflowName As String = BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk.ProfileKey).Name
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim mbrTimeMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Time, si.WorkflowClusterPk.TimeKey)				
					Dim scountry As String = sWorkflowName.Split("_")(0)
					If scountry = "Czech" Then
						scountry= "Czech_Republic"
					Else If scountry = "Northern" Then
						scountry= "Northern_Ireland"
					End If
					
			#Region "GetFieldList"
					If args.ActionType = ConnectorActionTypes.GetFieldList Then
					
						Dim lstFields As New List(Of String)({"Entity", "Scenario", "Time", "Account","Type of Lease","Asset Type","Contractual obligation for MAPEX", "Flow", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD7", "UD8","Month","Amount"})
'						Dim lstFields As New List(Of String)({"Entity", "Scenario", "Time", "Account","Type of Lease","Asset Type", "Flow", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD7", "UD8","Month","Amount"})
'						Dim lstFields As New List(Of String)({"Entity", "Scenario", "Time", "Account","Type Of Lease","Asset Type", "Flow", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD7", "UD8", "SourceTable"})
						Return lstFields
					
			#End Region 'GetFieldList
			#Region "GetData"
					ElseIf args.ActionType = ConnectorActionTypes.GetData Then
						
				#Region "SQL"
						Dim SQLQuery As New Text.StringBuilder	
						#Region "BUDGET"
						SqlQuery.Append($"	
						
							-----------------------------------------------------------------------------------------------
													--         Depreciation
							-----------------------------------------------------------------------------------------------

						
						
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M1' as Month,
													sum(COALESCE(c.Depreciation_Jan, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
										Union 
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M2' as Month,
												   sum( COALESCE(c.Depreciation_Feb, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
									union
											Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M3' as Month,
												   sum( COALESCE(c.Depreciation_Mar, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
										Union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M4' as Month,
												    sum(COALESCE(c.Depreciation_Apr, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
										union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M5' as Month,
												   sum( COALESCE(c.Depreciation_May, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											Union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M6' as Month,
												    sum(COALESCE(c.Depreciation_Jun, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M7' as Month,
												    sum(COALESCE(c.Depreciation_Jul, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M8' as Month,
												    sum(COALESCE(c.Depreciation_Aug, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M9' as Month,
												   sum(COALESCE(c.Depreciation_Sep, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.Depreciation_Oct, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.Depreciation_Nov, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.Depreciation_Dec, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--         Purchase InclVAT
							-----------------------------------------------------------------------------------------------

										Union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M1' as Month,
													sum(COALESCE(c.[Purchase inclVAT_Jan], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
										Union 
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M2' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Feb], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
										union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M3' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Mar], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
										Union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M4' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Apr], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M5' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_May], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											Union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M6' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Jun], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M7' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Jul], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M8' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Aug], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.Asset_type,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M9' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Sep], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Oct], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Nov], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--          Payment InclVAT
						    -----------------------------------------------------------------------------------------------
											
											Union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M1' as Month,
													sum(COALESCE(c.[Payment inclVAT_Jan], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											Union 
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M2' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Feb], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
										union
											Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M3' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Mar], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											Union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M4' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Apr], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M5' as Month,
												    sum(COALESCE(c.[Payment inclVAT_May], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											Union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M6' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Jun], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M7' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Jul], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M8' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Aug], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M9' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Sep], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Oct], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Nov], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
															")
						
						#End Region 'BUDGET
						#Region "Q_FCST"
						If mbrScenarioMember.Name.XFContainsIgnoreCase("Q_FCST_6_6") Then
							SQLQuery.Clear()
							SqlQuery.Append($"	
						
							-----------------------------------------------------------------------------------------------
													--         Depreciation
							-----------------------------------------------------------------------------------------------

						
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M7' as Month,
												    sum(COALESCE(c.Depreciation_Jul, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M8' as Month,
												    sum(COALESCE(c.Depreciation_Aug, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M9' as Month,
												    sum(COALESCE(c.Depreciation_Sep, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.Depreciation_Oct, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.Depreciation_Nov, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.Depreciation_Dec, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--         Purchase InclVAT
							-----------------------------------------------------------------------------------------------

										Union
												
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M7' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Jul], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M8' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Aug], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M9' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Sep], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Oct], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Nov], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--          Payment InclVAT
						    -----------------------------------------------------------------------------------------------
											
											Union
												
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M7' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Jul], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M8' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Aug], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M9' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Sep], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Oct], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Nov], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
															")
							Else If mbrScenarioMember.Name.XFContainsIgnoreCase("Q_FCST_7_5") Then
								SQLQuery.Clear()
								SqlQuery.Append($"	
						
							-----------------------------------------------------------------------------------------------
													--         Depreciation
							-----------------------------------------------------------------------------------------------

						
												Select
														
													
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M8' as Month,
												    sum(COALESCE(c.Depreciation_Aug, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M9' as Month,
												    sum(COALESCE(c.Depreciation_Sep, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.Depreciation_Oct, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.Depreciation_Nov, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.Depreciation_Dec, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--         Purchase InclVAT
							-----------------------------------------------------------------------------------------------

										Union
												
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M8' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Aug], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M9' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Sep], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Oct], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Nov], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--          Payment InclVAT
						    -----------------------------------------------------------------------------------------------
											
											Union
												
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M8' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Aug], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M9' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Sep], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Oct], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Nov], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
															")
								Else If mbrScenarioMember.Name.XFContainsIgnoreCase("Q_FCST_8_4") Then
									SQLQuery.Clear()
									SqlQuery.Append($"	
						
							-----------------------------------------------------------------------------------------------
													--         Depreciation
							-----------------------------------------------------------------------------------------------

						
												Select
													
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M9' as Month,
												    sum(COALESCE(c.Depreciation_Sep, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.Depreciation_Oct, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.Depreciation_Nov, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.Depreciation_Dec, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--         Purchase InclVAT
							-----------------------------------------------------------------------------------------------

										Union
												
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M9' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Sep], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Oct], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Nov], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--          Payment InclVAT
						    -----------------------------------------------------------------------------------------------
											
											Union
												
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M9' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Sep], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Oct], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Nov], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
															")
								Else If mbrScenarioMember.Name.XFContainsIgnoreCase("Q_FCST_9_3") Then
									SQLQuery.Clear()
									SqlQuery.Append($"	
						
							-----------------------------------------------------------------------------------------------
													--         Depreciation
							-----------------------------------------------------------------------------------------------

						
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.Depreciation_Oct, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.Depreciation_Nov, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.Depreciation_Dec, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--         Purchase InclVAT
							-----------------------------------------------------------------------------------------------

										Union
												
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Oct], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Nov], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--          Payment InclVAT
						    -----------------------------------------------------------------------------------------------
											
											Union
												
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M10' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Oct], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Nov], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
															")
								Else If mbrScenarioMember.Name.XFContainsIgnoreCase("Q_FCST_10_2") Then
									SQLQuery.Clear()
									SqlQuery.Append($"	
						
							-----------------------------------------------------------------------------------------------
													--         Depreciation
							-----------------------------------------------------------------------------------------------

						
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.Depreciation_Nov, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.Depreciation_Dec, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--         Purchase InclVAT
							-----------------------------------------------------------------------------------------------

										Union
												
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Nov], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--          Payment InclVAT
						    -----------------------------------------------------------------------------------------------
											
											Union
												
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M11' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Nov], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
											union
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
															")
								Else If mbrScenarioMember.Name.XFContainsIgnoreCase("Q_FCST_11_1") Then
									SQLQuery.Clear()
									SqlQuery.Append($"	
						
							-----------------------------------------------------------------------------------------------
													--         Depreciation
							-----------------------------------------------------------------------------------------------

						
												Select
														
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Depreciation' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.Depreciation_Dec, 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--         Purchase InclVAT
							-----------------------------------------------------------------------------------------------

										Union
												
												Select
												
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Purchase' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Purchase inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
							-----------------------------------------------------------------------------------------------
													--          Payment InclVAT
						    -----------------------------------------------------------------------------------------------
											
											Union
												
												Select
												
													c.Country + '_Input_OP' as [Entity],  -----> Country_Input
												    c.WFScenario as [Scenario],
												    c.WFTime as [Time],	
													'Payment' as [Account],   				
													c.Type_of_Leasing as [Type of Lease],
													c.Asset_type as [Asset Type],c.Contractual_obligation_for_MAPEX_YES_NO as [Contractual obligation for MAPEX],
													--case 
													 		--when c.Asset_type = 'PP&E' then 'F_Property_plant_and_equipment_Mov_1'
															--else 'F_Intangibles_Mov_2'
													--END AS [Flow],       				----> Needs to be here or in the TR (-)
													'None' AS [Flow],
						    						c.center_code as [UD1],              ----> UD1 Name in OS not Description 
													c.Modality as [UD2],
													'None' as [UD3],
													'None' as [UD4],
													c.PMC as [UD5], 					----> |UD1Detault|
													c.Project_code as [UD6],			----> UD6 Name in OS not Description 
													c.Commodity as [UD7],					----> |UD1Detault|
													 case 
													 		when c.Type_of_Leasing = 'Operating Lease' then 'IFRS16_Plan'
															else 'Pre_IFRS16_Plan'
													END AS [UD8],
						
													  CAST(MAX({mbrTimeMember.Name}) AS VARCHAR(10))+'M12' as Month,
												    sum(COALESCE(c.[Payment inclVAT_Dec], 0)) AS Amount

						
													From XFC_Capex_Plan As c
														where	
															c.Country = '{scountry}'
															AND c.WFScenario = '{mbrScenarioMember.Name}'
															AND c.WFTime = {mbrTimeMember.Name}
															group by
																 c.Country+ '_Input_OP',
																    c.WFScenario,
																    c.WFTime,
																    c.Type_of_Leasing,
																    c.Asset_type,
																	c.Contractual_obligation_for_MAPEX_YES_NO,
																    c.center_code,
																    c.Modality,
																    c.PMC,
																    c.Project_code,
																    c.Commodity
						
															")
							End If
						#End Region 'Q_FCST
'						  brapi.ErrorLog.LogMessage(si, "sql  " &  SQLQuery.ToString)
					#End Region 'SQL
						  				Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
											api.Parser.ProcessSQLQuery(si, DbProviderType.SqlServer, dbConnApp.ConnectionString, False, SQLQuery.ToString, False, api.ProcessInfo)
										End Using
					
					End If 
				#End Region 'GetData
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace