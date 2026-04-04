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

Namespace OneStream.BusinessRule.Finance.Copy_Allocations
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				
				Dim sFunction As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("Function")

				If sFunction = "Copy_Allocations_0_Clear_Data" Then	
				
					Me.Copy_Allocations_0_Clear_Data(si, api, globals, args)				
				
				Else If sFunction = "Copy_Allocations_Drivers_Brazil" Then	
				
					Me.Copy_Allocations_Drivers_Brazil(si, api, globals, args)	
					
				Else If sFunction = "Copy_Allocations_1_Source_Data" Then	
				
					Me.Copy_Allocations_1_Source_Data(si, api, globals, args)	
				
				Else If sFunction = "Copy_Allocations_2_1_Drivers" Then	
				
					Me.Copy_Allocations_2_1_Drivers(si, api, globals, args)	
					
				Else If sFunction = "Copy_Allocations_2_2_Drivers" Then	
				
					Me.Copy_Allocations_2_2_Drivers(si, api, globals, args)						
					
				Else If sFunction = "Copy_Allocations_3_Target_Data" Then	
				
					Me.Copy_Allocations_3_Target_Data(si, api, globals, args)					

				Else If sFunction = "Copy_Allocations_4_Perimeter" Then	
				
					Me.Copy_Allocations_4_Perimeter(si, api, globals, args)						
					
				Else If sFunction = "Copy_Allocations_0_Copy_Drivers_Test" Then	
				
					Me.Copy_Allocations_0_Copy_Drivers_Test(si, api, globals, args)						
					
				End If	
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

'__________________________________________________________________________________________________________________________________________________________________________________________________
	
		Sub Copy_Allocations_0_Clear_Data (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
			Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Analytics:S#RF06)")
			
			UTISharedFunctionsBR.ClearDataBuffer(si,api,targetDataBuffer)
	
		End Sub
		
'--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------		
			
		Sub Copy_Allocations_Drivers_Brazil (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
		
				'Clear scenario calculated data
				Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Analytics:E#" & api.Pov.Entity.Name & ", A#Drivers_Unified.Base)")
				
				UTISharedFunctionsBR.ClearDataBuffer(si,api,targetDataBuffer)
				
				'Declare target and source data buffers
				Dim siDestinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"RemoveZeros(FilterMembers(Cb#Analytics:E#R1303001, A#Drivers_Unified.Base))")
				
				'Process data buffer
				api.Data.SetDataBuffer(sourceDataBuffer,siDestinationInfo,,,,,,,,,,,,,True)	
				
				
		End Sub		
		
'--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------		
		
		Sub Copy_Allocations_1_Source_Data (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
			Dim sourceCopyScenario As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("SourceScenario")
		
			'Clear scenario calculated data
			Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Analytics:S#Allocations, A#PL.Base)")
			
			UTISharedFunctionsBR.ClearDataBuffer(si,api,targetDataBuffer)
			
			'Declare target and source data buffers
			Dim siDestinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("Cb#Analytics:S#Allocations:U1#None:U2#None")
			Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"RemoveZeros(FilterMembers(Cb#Horse:S#" & sourceCopyScenario  & ":U1#Top:U2#Top:U5#None:U6#None, A#PL.Base))")
			
			'Process data buffer
			api.Data.SetDataBuffer(sourceDataBuffer,siDestinationInfo,,,,,,,,,,,,,True)
	
		End Sub
		
'--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------		
			
		Sub Copy_Allocations_2_1_Drivers (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
			Dim sourceCopyScenario As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("SourceScenario")
		
			
			Dim MesFcstNumber As Integer = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Scenario, sourceCopyScenario, True).GetScenarioProperties.WorkflowNumNoInputTimePeriods.GetStoredValue
			Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
			Dim sScenarioType As String = api.Scenario.GetScenarioType(api.Members.GetMemberId(dimTypeId.Scenario, sourceCopyScenario)).Name
			
			'Clear scenario calculated data
			Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Analytics:S#" & api.Pov.Scenario.Name  & ", A#Drivers_Unified.Base, A#Vol_Sales, A#Vol_Prod)")
			
			UTISharedFunctionsBR.ClearDataBuffer(si,api,targetDataBuffer)
		
			If (sScenarioType = "Forecast" And sMes <= MesFcstNumber) Then
				
				'Declare target and source data buffers
				Dim siDestinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Analytics:S#Actual, A#Drivers_Unified.Base, A#Vol_Sales, A#Vol_Prod)")
				
				'Process data buffer
				api.Data.SetDataBuffer(sourceDataBuffer,siDestinationInfo,,,,,,,,,,,,,True)	
				
			End If
		
		End Sub		
			
'------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------	
				
		Sub Copy_Allocations_2_2_Drivers (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
			Dim sourceCopyScenario As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("SourceScenario")
		
			
			Dim MesFcstNumber As Integer = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Scenario, sourceCopyScenario, True).GetScenarioProperties.WorkflowNumNoInputTimePeriods.GetStoredValue
			Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
			Dim sScenarioType As String = api.Scenario.GetScenarioType(api.Members.GetMemberId(dimTypeId.Scenario, sourceCopyScenario)).Name
					
		
			'If (sScenarioType = "Actual") Or (sScenarioType = "Budget") Or (sScenarioType = "Forecast" And sMes > MesFcstNumber) Then
				
				'Clear scenario calculated data
				Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#" & api.Pov.Scenario.Name  & ", A#Drivers_Unified.Base)")
				
				UTISharedFunctionsBR.ClearDataBuffer(si,api,targetDataBuffer)
				
				'Declare target And source data buffers
				Dim siDestinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic:S#" & api.Pov.Scenario.Name)
				Dim sourceDataBufferCurrentMonth As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#" & sourceCopyScenario  & ", A#Drivers_Unified.Base)")
				Dim sourceDataBufferPriorMonth As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(T#PovPrior1:S#" & sourceCopyScenario  & ", A#Drivers_Unified.Base)")
				
				'Process data buffer
				Dim sourceDataBuffer As DataBuffer = sourceDataBufferCurrentMonth - sourceDataBufferPriorMonth
				
				api.Data.SetDataBuffer(sourceDataBuffer,siDestinationInfo,,,,,,,,,,,,,True)	
				
			'End If
		
		End Sub
		
'--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------		
	
		Sub Copy_Allocations_3_Target_Data (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		

				'Clear scenario calculated data
				Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Analytics:S#" & api.Pov.Scenario.Name  & ", A#PL.Base, U5#Top.Base, U6#Top.Base)")
				
				UTISharedFunctionsBR.ClearDataBuffer(si,api,targetDataBuffer)
				
				'Declare target and source data buffers
				Dim siDestinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("S#" & api.Pov.Scenario.Name)
				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"RemoveZeros(FilterMembers(S#Allocations, A#PL.Base, U5#Top.Base, U6#Top.Base))")
				
				'Process data buffer
				api.Data.SetDataBuffer(sourceDataBuffer,siDestinationInfo,,,,,,,,,,,,,True)
			
				
		End Sub			

'--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------	
	
		Sub Copy_Allocations_4_Perimeter (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		

				'Clear scenario calculated data
				Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Analytics:S#" & api.Pov.Scenario.Name  & ", A#PerimeterAcc.Base)")
				
				UTISharedFunctionsBR.ClearDataBuffer(si,api,targetDataBuffer)
				
				'Declare target and source data buffers
				Dim siDestinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"RemoveZeros(FilterMembers(Cb#Horse:S#" & api.Pov.Scenario.Name & ", A#PerimeterAcc.Base))")
				
				'Process data buffer
				api.Data.SetDataBuffer(sourceDataBuffer,siDestinationInfo,,,,,,,,,,,,,True)
			
				
		End Sub			

'--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------		
		
		Sub Copy_Allocations_0_Copy_Drivers_Test (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		

				'Clear scenario calculated data
				Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Analytics:S#" & api.Pov.Scenario.Name  & ", A#Drivers_Unified.Base)")
				
				UTISharedFunctionsBR.ClearDataBuffer(si,api,targetDataBuffer)
				
				'Declare target and source data buffers
				Dim siDestinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"RemoveZeros(FilterMembers(Cb#Horse:S#" & api.Pov.Scenario.Name  & ", A#Drivers_Unified.Base))")
				
				'Process data buffer
				api.Data.SetDataBuffer(sourceDataBuffer,siDestinationInfo,,,,,,,,,,,,,True)
			
				
		End Sub			

	End Class
End Namespace