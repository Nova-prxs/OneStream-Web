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

Namespace OneStream.BusinessRule.Finance.SC_Calculates
	
	Public Class MainClass
		
		'Reference business rule to get functions
		Dim SharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.CustomCalculate
						
						#Region "Initialize"
						
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Initialize") Then

							'Get all the parameters
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							Dim sEntityAux As String = args.CustomCalculateArgs.NameValuePairs("p_entity_aux")
							
							Dim sPeriod As String = Api.Pov.Time.Name
							Dim iMonth As Integer = sPeriod.Substring(5)
	
							If (sScenario = "Forecast" AndAlso iMonth >= iForecastMonth) Then							
								
								Dim oMemberList As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(api.Dimensions.GetDim("Accounts").DimPk, "A#AL_ES_EBDA_Input.Base", Nothing)

								For Each oMember As MemberInfo In oMemberList
									
									Dim sAccountName As String = oMember.Member.Name.Replace("_Input","")																
									Me.Initialize(si, globals, api, args, sAccountName, sScenario, sYear, iForecastMonth, sEntityAux)
									
								Next
							
							End If
						End If
							
						#End Region		
						
						#Region "Recalculate"
						
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Recalculate") Then

							'Get all the parameters
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							Dim sEntityAux As String = args.CustomCalculateArgs.NameValuePairs("p_entity_aux")
							
							Dim sPeriod As String = Api.Pov.Time.Name
							Dim iMonth As Integer = sPeriod.Substring(5)
	
							If (sScenario = "Forecast" AndAlso iMonth >= iForecastMonth) Then							
								
								Dim oMemberList As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(api.Dimensions.GetDim("Accounts").DimPk, "A#AL_ES_EBDA_Input.Base", Nothing)

								For Each oMember As MemberInfo In oMemberList
									
									Dim sAccountName As String = oMember.Member.Name.Replace("_Input","")																
									Me.Recalculate(si, globals, api, args, sAccountName, sScenario, sYear, iForecastMonth, sEntityAux)
									
								Next
							
							End If
							
						#End Region					
						
						#Region "Copy Accounts Using Lookup Table"

						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyAccountsWithLookup") Then
							
							'Get parameters
							Dim ParamScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
'							Dim sEntityAux As String = args.CustomCalculateArgs.NameValuePairs("p_entity_aux")
							
							'Get source data buffer and declare destination info
							Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#{ParamScenario},F#None,U1#None,U2#None,A#AL_ES_EBDA_Input.Base)")
							Dim expDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							Dim targetDataBuffer As New DataBuffer
							'Process each data buffer cell to change the account member
							For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
								Dim targetCell As New DataBufferCell(cell)
								Dim cellAccountName As String = api.Members.GetMemberName(dimType.Account.Id, targetCell.DataBufferCellPk.AccountId)
								Dim cellAccountMapped As String = BRApi.Utilities.TransformText(si, cellAccountName, "Mapping_Structure_Accounts", False)
								If Not String.IsNullOrEmpty(cellAccountMapped) Then
									targetCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account.Id, cellAccountMapped)
									targetDataBuffer.SetCell(si, targetCell, True)
								End If
							Next
							'Set data buffer
							api.Data.SetDataBuffer(targetDataBuffer, expDestInfo,,,,,,,,,,,,, True)
						#End Region
												
						#Region "Copy Departments Using Lookup Table"

						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyDepartmentsWithLookup") Then
							
							'Get parameters
							Dim ParamScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							Dim sPeriod As String = Api.Pov.Time.Name
							Dim iMonth As Integer = sPeriod.Substring(5)
							Dim AccountsMemberFilter As String = "A#AL_ES_EBDA_Input.Base"
							Dim Accounts As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(BRApi.Finance.Dim.GetDimPk(si, "Accounts"), AccountsMemberFilter, Nothing)
							Dim Entity As String= api.Pov.Entity.Name
							Dim EntityInput As String = BRApi.Utilities.TransformText(si, Entity , "Mapping_Structure_Departments", False)							
							If (ParamScenario = "Forecast" AndAlso iMonth >= iForecastMonth) Or (ParamScenario="Budget") Then
								For Each account As MemberInfo In Accounts
									Dim NameAccountInput As String = Account.Member.Name
									Dim NameAccount As String = BRApi.Utilities.TransformText(si, NameAccountInput , "Mapping_Structure_Accounts", False)
'									BRApi.ErrorLog.LogMessage(si, "NameAccount: " & NameAccount)
'									BRApi.ErrorLog.LogMessage(si, "Entity: " & Entity)
									Api.Data.ClearCalculatedData($"A#{NameAccount}:O#Forms",True,True,True,True)			
							        Api.Data.Calculate($"A#{NameAccount}:O#Forms=A#{NameAccountInput}:E#{EntityInput}",True)	
'									BRApi.ErrorLog.LogMessage(si, $"A#{NameAccount}=A#{NameAccountInput}:E#{EntityInput}")
							   Next
						    End If 
							
                      #End Region
					  
						#Region "Copy Using Lookups"

						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyDataWithLookups") Then
							
							'Get parameters
							Dim ParamScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							Dim sPeriod As String = Api.Pov.Time.Name
							Dim iMonth As Integer = sPeriod.Substring(5)
							
							Dim AccountsMemberFilter As String = "A#AL_ES_EBDA_Input.Base, A#Headcount"
							
							Dim Accounts As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(BRApi.Finance.Dim.GetDimPk(si, "Accounts"), AccountsMemberFilter, Nothing)
							
							Dim Entity As String= api.Pov.Entity.Name
							Dim EntityInput As String = BRApi.Utilities.TransformText(si, Entity , "Mapping_Structure_Departments", False)							
							
							If (ParamScenario = "Forecast" AndAlso iMonth >= iForecastMonth) Or (ParamScenario="Budget") Then
								
								For Each account As MemberInfo In Accounts
									
									Dim NameAccountInput As String = Account.Member.Name
									Dim NameAccount As String = BRApi.Utilities.TransformText(si, NameAccountInput , "Mapping_Structure_Accounts", False)

									Api.Data.ClearCalculatedData($"A#{NameAccount}:O#Forms",True,True,True,True)			
							        Api.Data.Calculate($"A#{NameAccount}:O#Forms = A#{NameAccountInput}:E#{EntityInput}:O#Top",True)	

							  	Next
							   
						    End If 
							
						#End Region					 
			
						End If
						
				End Select
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
		
		Sub Initialize(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sScenario As String, ByVal sYear As String, ByVal iForecastMonth As Integer, ByVal sEntityAux As String)		
					
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			Dim sEntityActual As String = sEntity.Replace("_Input", "")	
			
			Dim sCalc1 As String = ""				
										
			' 1. INITIAL			
			
			'CRITERIA 1 - Average Actual Months
			
			'String builder to calculate actual average depending on the number of actual months
			Dim sCalcAvgReal As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}_Input:F#Amount_Initial_Result:O#Forms = 
	
									( S#Actual:E#{sEntityActual}:T#{sYear}M1:A#{sAccount}:F#None:O#Top:C#Aggregated * S#Forecast:T#{sYear}M1:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Actual:E#{sEntityActual}:T#{sYear}M2:A#{sAccount}:F#None:O#Top:C#Aggregated * S#Forecast:T#{sYear}M2:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Actual:E#{sEntityActual}:T#{sYear}M3:A#{sAccount}:F#None:O#Top:C#Aggregated * S#Forecast:T#{sYear}M3:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Actual:E#{sEntityActual}:T#{sYear}M4:A#{sAccount}:F#None:O#Top:C#Aggregated * S#Forecast:T#{sYear}M4:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Actual:E#{sEntityActual}:T#{sYear}M5:A#{sAccount}:F#None:O#Top:C#Aggregated * S#Forecast:T#{sYear}M5:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Actual:E#{sEntityActual}:T#{sYear}M6:A#{sAccount}:F#None:O#Top:C#Aggregated * S#Forecast:T#{sYear}M6:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Actual:E#{sEntityActual}:T#{sYear}M7:A#{sAccount}:F#None:O#Top:C#Aggregated * S#Forecast:T#{sYear}M7:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top		
									+ S#Actual:E#{sEntityActual}:T#{sYear}M8:A#{sAccount}:F#None:O#Top:C#Aggregated * S#Forecast:T#{sYear}M8:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Actual:E#{sEntityActual}:T#{sYear}M9:A#{sAccount}:F#None:O#Top:C#Aggregated * S#Forecast:T#{sYear}M9:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Actual:E#{sEntityActual}:T#{sYear}M10:A#{sAccount}:F#None:O#Top:C#Aggregated * S#Forecast:T#{sYear}M10:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Actual:E#{sEntityActual}:T#{sYear}M11:A#{sAccount}:F#None:O#Top:C#Aggregated * S#Forecast:T#{sYear}M11:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Actual:E#{sEntityActual}:T#{sYear}M12:A#{sAccount}:F#None:O#Top:C#Aggregated * S#Forecast:T#{sYear}M12:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top)
							
									/ 

									(S#Forecast:T#{sYear}M1:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top									
									+ S#Forecast:T#{sYear}M2:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Forecast:T#{sYear}M3:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Forecast:T#{sYear}M4:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Forecast:T#{sYear}M5:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Forecast:T#{sYear}M6:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Forecast:T#{sYear}M7:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top	
									+ S#Forecast:T#{sYear}M8:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Forecast:T#{sYear}M9:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Forecast:T#{sYear}M10:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Forecast:T#{sYear}M11:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top
									+ S#Forecast:T#{sYear}M12:E#{sEntityAux}:A#{sAccount}_Input:F#Selected:O#Top)

									"
						
			
			'CRITERIA 2 - Same Month Current Budget
			
			Dim sCalcBudgetSameMonth As String =  $"S#{sScenario}:A#{sAccount}_Input:F#Amount_Initial_Result:O#Forms = S#Budget:E#{sEntityActual}:A#{sAccount}:F#None:O#Top:C#Aggregated"	
			
			'CRITERIA 3 - Total Year Target Current Budget
			
			Dim sCalcBudgetObj As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}_Input:F#Amount_Initial_Result:O#Forms =
											
											(S#Budget:T#{sYear}:E#{sEntityActual}:A#{sAccount}:F#None:O#Top:C#Aggregated
											- S#Actual:T#{sYear}M{iForecastMonth - 1}:V#YTD:E#{sEntityActual}:A#{sAccount}:F#None:O#Top:C#Aggregated)
			
											/ {(12 - iForecastMonth + 1).ToString()}"
				
			Dim iCriteria As Integer = CInt(Api.Data.GetDataCell($"A#{sAccount}_Input:E#{sEntityAux}:F#Criteria_SC:T#{sYear}M12:O#Top").CellAmount)
			
			'BRAPI.ErrorLog.LogMessage(si, "Criteria: " & iCriteria.ToString())

			'If not 4 = Input File
			If iCriteria <> 4 Then
			
				Select Case iCriteria.toString
					'1 = Average Actual Months
					Case "1"  
						sCalc1 = sCalcAvgReal	
'						BRApi.ErrorLog.LogMessage(si, "Calc1: " & sCalc1)
					'2 = Same Month Current Budget
					Case "2"
						sCalc1 = sCalcBudgetSameMonth
					'3 = Total Year Target Current Budget
					Case "3"
						sCalc1 = sCalcBudgetObj
	
				End Select
				
				'BRApi.ErrorLog.LogMessage(si, "Calc1: " & sCalc1)				
	
				Api.Data.ClearCalculatedData($"A#{sAccount}_Input:F#Amount_Initial_Result:O#Forms",True,True,True,True)			
				Api.Data.Calculate(sCalc1,True)							
				
				
				
			End If
			
		End Sub 
		
		'2. FINAL
		Sub Recalculate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sScenario As String, ByVal sYear As String, ByVal iForecastMonth As Integer, ByVal sEntityAux As String)
	
				Dim sCalc2 As String = $"A#{sAccount}_Input:F#None:O#Forms = A#{sAccount}_Input:F#Amount_Initial_Result:O#Top"	
				
				Dim dPerc_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}_Input:F#Perc_Variation_Input:O#Top").CellAmount
				Dim dAmount_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}_Input:F#Amount_Variation_Input:O#Top").CellAmount
				
				If dPerc_Variation <> 0 And dAmount_Variation <> 0 Then 
					
					sCalc2 = $"A#{sAccount}_Input:F#None:O#Forms = (A#{sAccount}_Input:F#Amount_Initial_Result:O#Top * (1 + {(dPerc_Variation / 100).ToString().Replace(",",".")}))+ ({dAmount_Variation.ToString().Replace(",",".")})"
		        ElseIf dPerc_Variation <> 0 Then 
					sCalc2 = $"A#{sAccount}_Input:F#None:O#Forms = A#{sAccount}_Input:F#Amount_Initial_Result:O#Top * (1 + {(dPerc_Variation / 100).ToString().Replace(",",".")})"
				ElseIf dAmount_Variation <> 0 Then 
					sCalc2 = $"A#{sAccount}_Input:F#None:O#Forms = A#{sAccount}_Input:F#Amount_Initial_Result:O#Top + {dAmount_Variation.ToString().Replace(",",".")}"	
					
				End If
							
				Api.Data.ClearCalculatedData($"A#{sAccount}_Input:F#None:O#Forms",True,True,True,True)		
				Api.Data.Calculate(sCalc2,True)		
					
		End Sub

		#Region "Functions"
		
		#Region "Helpers"
		
		Public Sub ClearDataBuffer(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal targetDataBuffer As DataBuffer)
			'Build clean data buffer
			Dim cleanDataBuffer As New DataBuffer
			Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
			For Each targetDataBufferCell In targetDataBuffer.DataBufferCells.Values
				If Not targetDataBufferCell.CellStatus.IsNoData Then
					targetDataBufferCell.CellAmount = 0
					targetDataBufferCell.CellStatus = New DataCellStatus(targetDataBufferCell.CellStatus, DataCellExistenceType.NoData)
					cleanDataBuffer.SetCell(si, targetDataBufferCell)
				End If
			Next
			'Clean data buffer
			api.Data.SetDataBuffer(cleanDataBuffer, cleanExpDestInfo,,,,,,,,,,,,,True)
		End Sub
		
		#End Region
		
		#End Region
		
	End Class
	
End Namespace