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

Namespace OneStream.BusinessRule.Extender.WF_PlanningHelper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						' Get the function name
						Dim functionName As String = args.NameValuePairs("p_function")
						
						#Region "Import Sales To Cube"
						
						If functionName.XFEqualsIgnoreCase("ImportSalesToCube") Then
								
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							
							Try
								'Get parameters
								Dim ParamBrand As String = args.NameValuePairs("p_brand")
								Dim ParamScenario As String = args.NameValuePairs("p_scenario")
								Dim actualYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Actual_Year")
								Dim budgetYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Budget_Year")
								Dim forecastMonth As Integer = CInt(BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Forecast_Month"))
								Dim scenarioYear As String = If(ParamScenario = "Forecast", actualYear, budgetYear)
								
								'Build a dictionary to get the brand entity ancestor
								Dim brandAncestorDict As New Dictionary(Of String, String) From {
									{"Burger King", "M_BK"},
									{"Fridays", "M_FR"},
									{"Domino''s Pizza", "M_DP"},
									{"Foster''s Hollywood", "M_FH"},
									{"Foster''s Hollywood Valencia", "M_FH"},
									{"Starbucks", "M_SB"},
									{"Starbucks Portugal", "M_SB"},
									{"Starbucks Bélgica", "M_SB"},
									{"Starbucks Francia", "M_SB"},
									{"Starbucks Holanda", "M_SB"},
									{"Ginos", "M_GI"},
									{"Ginos Portugal", "M_GI"},
									{"VIPS", "M_VI"}
								}
								
								'Get current Workflow Cluster Pk
								Dim profileName As String = $"{BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk).Name} Load"
								Dim profileKey As Guid = BRApi.Workflow.Metadata.GetProfile(si, profileName).ProfileKey
								Dim scenarioKey As Integer = si.WorkflowClusterPk.ScenarioKey
								Dim timeKey As Integer = si.WorkflowClusterPk.TimeKey
								
								Dim wfClusterPK As New WorkflowUnitClusterPk(profileKey, scenarioKey, timeKey)
								
								'Set Brand value to use it in the load process
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_BrandFilter_Value", ParamBrand)
								
								'Update Task Activity and check if it's cancelled
								Dim isDMCancelled As Boolean = BRApi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(
									si,
									args,
									"Loading data to model, this can take a while." & vbCrLf & "You can close this window while loading.",
									0
								)
								If isDMCancelled Then Return Nothing
								
								'Clear Stage Data
								BRApi.Import.Process.ClearStageData(si, wfClusterPK, "")
				
								'Execute extraction
								Dim piLoadTransform As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteParseAndTransform(si, wfClusterPK, Nothing, Nothing, TransformLoadMethodTypes.Replace, SourceDataOriginTypes.FromDirectConnection, False)
				
								'Execute validations
								Dim piValidateTransformation As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, wfClusterPK, True)
								Dim piValidateIntersections As ValidateIntersectionProcessInfo = BRApi.Import.Process.ValidateIntersections(si, wfClusterPK, True)
				
								'Execute Load & Process
								Dim piLoadCube As LoadCubeProcessInfo = BRApi.Import.Process.LoadCube(si, wfClusterPK)
								
								'Declare variables to execute sales copy from aux to main account
								Dim unitCodes As String
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									'Get the non franchise unit codes
									Dim selectQuery As String = $"
										SELECT DISTINCT c.cebe
										FROM XFC_DailySales ds
										JOIN XFC_CEBES c ON ds.unit_code = c.cebe
										WHERE
										    (
												ds.scenario = '{ParamScenario}'
												OR
												(
													ds.scenario = 'Actual'
													AND MONTH(ds.date) = {forecastMonth}
												)
											)
											AND YEAR(ds.date) = {scenarioYear}
											AND ds.brand = '{ParamBrand}'
											AND c.unit_type IN ('Unidades Propias', 'Unidades ECI')
									"
									Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
									'Iterate through each cebe to copy data from sales aux account to sales account
									If dt.Rows.Count > 0 Then
										For Each row As DataRow In dt.Rows
											unitCodes += If(String.IsNullOrEmpty(unitCodes), $"E#{row("cebe")}", $", E#{row("cebe")}")
										Next
									End If
									
									'Generate a time filter for only open months
									Dim timeFilter As String
									
									For i As Integer = If(ParamScenario = "Budget", 1, forecastMonth) To 12
										timeFilter += If(String.IsNullOrEmpty(timeFilter), $"T#{scenarioYear}M{i}", $", T#{scenarioYear}M{i}")
									Next
									'Create a dictionary with the data management sequence parameters
									Dim customSubstVars As New Dictionary(Of String, String) From {
										{"p_entity_filter", unitCodes},
										{"p_scenario", ParamScenario},
										{"p_year", scenarioYear},
										{"p_time_filter", timeFilter},
										{"p_brand", brandAncestorDict(ParamBrand)}
									}
									BRApi.Utilities.ExecuteDataMgmtSequence(si, guid.Empty, "SALES_LoadSalesToOwnUnits", customSubstVars)
'									BRApi.Utilities.ExecuteDataMgmtSequence(si, guid.Empty, "ALL_Aggregate", customSubstVars)
								End Using
							
								selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
								
								selectionChangedTaskResult.IsOK = True
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = "Data has been imported to cube."
								Return selectionChangedTaskResult
							
							Catch Ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								
							End Try
							
						#End Region
						
						End If
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace