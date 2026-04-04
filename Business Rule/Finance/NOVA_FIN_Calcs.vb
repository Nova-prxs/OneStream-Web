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

Namespace OneStream.BusinessRule.Finance.NOVA_FIN_Calcs
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try				
				'Variables globales
				Dim workspaceName As String = "NOVA"
				Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, workspaceName)
				Dim nFunction As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("Function")
				
				'Variables para calculos				
				Dim entity As String = api.Pov.Entity.Name
				Dim period As String = api.Pov.Time.Name
				Dim scenario As String = api.Pov.Scenario.Name 
				Dim sSource As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("PassedScenarioSource")	
				Dim sTarget As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("PassedScenarioTarget")		
				Dim fixPeriod As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("PassedTime")
				'Dim entity As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, workspaceID,"paramEntityDM")
				
			'Calculo Precio por Cantidad	
				If nFunction = "Calc_Budget1" Then					
					Me.Calc_Budget1(si, api, entity, period, scenario, fixPeriod)
				
			'Calculo % sobre Ventas	
				Else If nFunction ="Calc_Budget2" Then
					Me.Calc_Budget2(si, api, entity, period, scenario, fixPeriod)						
						
			'Calculo Mensualización
				Else If nFunction = "Calc_Budget4" Then
					Me.Calc_Budget4(si, api, entity, period, scenario, fixPeriod)	
				
			'Copiado de escenarios		
				Else If	nFunction ="Copy_Scenario" Then					
					Me.Copy_Scenarios(si, api, entity, sSource, sTarget, period)
					
				End If
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "Calculations"

Sub Calc_Budget_Manual (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal entity As String, ByVal period As String)
		'Variables
		Dim activity As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "HoustonProducts"), "U2#Top.Base", True)
		Dim customer As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "HoustonCustomers"), "U4#TotalCustomers.Base", True)
		Dim year As String =Left(period, 4)
		Dim periodList As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "Time"), "T#[" & year & "].Base.Remove(Name StartsWith 'Q' Or Name StartsWith 'H')", True) 
		Dim yearMonthName As String = String.Empty
		Dim price As Double = 0.00
		Dim amount  As Double = 0.00
		
		For Each activityUnit In activity
			For Each customerUnit In customer	
			price = api.Data.GetDataCell("Cb#Houston:E#[Houston Heights]:C#Local:S#Actual:T#2010M12:V#Periodic:A#Price:F#None:O#Forms:I#None:U1#None:U2#[" & activityUnit.Member.Name.ToString & "]:U3#None:U4#[" & customerUnit.Member.Name.ToString & "]:U5#None:U6#None:U7#None:U8#None").CellAmount
			amount = api.Data.GetDataCell("Cb#Houston:E#[Houston Heights]:C#Local:S#Actual:T#2010M12:V#Periodic:A#Amount:F#None:O#Forms:I#None:U1#None:U2#[" & activityUnit.Member.Name.ToString & "]:U3#None:U4#[" & customerUnit.Member.Name.ToString & "]:U5#None:U6#None:U7#None:U8#None").CellAmount
			If (price <> 0 Or amount <> 0)
				api.Data.Calculate("A#2000_100:F#None:O#Forms:I#None:U1#None:U2#[" & activityUnit.Member.Name.ToString & "]:U3#None:U4#[" & customerUnit.Member.Name.ToString & "]:U5#None:U6#None:U7#None:U8#None = " & price & "*" & amount, True)
			End If
			Next
		Next		
End Sub

Sub Calc_Budget1 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal entity As String, ByVal period As String, ByVal scenario As String, ByVal fixPeriod As String)				
	
	#Region "DataBuffer con Loop"
	'Creamos databuffer
'		Dim db1 As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(A#2000_100:F#None:O#Forms:V#Periodic, U2#Top.Base, U4#TotalCustomers.Base)")	
		
'		'Ejecutamos databuffer celda por celda
'		For Each db1Cell As DataBufferCell In db1.DataBufferCells.Values		
			
'			'Obtenemos UD2-Activity y UD4-Customer en curso
'			Dim activity As String = db1Cell.DataBufferCellPk.GetUD2Name(api)
'			Dim customer As String = db1Cell.DataBufferCellPk.GetUD4Name(api)		
			
'			'Obtenemos Price y Amount
'			Dim price As Decimal = api.Data.GetDataCell("Cb#Houston:E#[" & entity & "]:C#Local:S#" & scenario & ":T#" & fixPeriod & ":V#Periodic:A#Price:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None").CellAmount
'			Dim amount As Decimal = api.Data.GetDataCell("Cb#Houston:E#[" & entity & "]:C#Local:S#" & scenario & ":T#" & fixPeriod & ":V#Periodic:A#Amount:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None").CellAmount			
		
'			If Not price = 0 And Not amount = 0 Then
'				'Ejecutamos Calculo
'				api.Data.Calculate("A#2000_100:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None = " & price & "*" & amount, True)			
'			End If	
'	Next
#End Region

	#Region "DataBuffer con SetData"
	
		'Creamos databuffer en blanco para guardar los resultados
		Dim result As New DataBuffer
		
		'Creamos databuffer
		Dim db1 As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveNoData(A#Price:T#" & fixPeriod & ":F#None:O#Forms:V#Periodic:I#None:U1#None), U2#Top.Base, U4#TotalCustomers.Base)")	
		Dim db2 As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveNoData(A#Amount:T#" & fixPeriod & ":F#None:O#Forms:V#Periodic:I#None:U1#None), U2#Top.Base, U4#TotalCustomers.Base)")
		
		'Ejecutamos databuffer celda por celda
		For Each db1Cell As DataBufferCell In db1.DataBufferCells.Values		
			For Each db2Cell As DataBufferCell In db2.DataBufferCells.Values
				
				'Obtenemos UD2-Activity y UD4-Customer en curso
				Dim activity1 As String = db1Cell.DataBufferCellPk.GetUD2Name(api)
				Dim customer1 As String = db1Cell.DataBufferCellPk.GetUD4Name(api)	
				Dim activity2 As String = db2Cell.DataBufferCellPk.GetUD2Name(api)
				Dim customer2 As String = db2Cell.DataBufferCellPk.GetUD4Name(api)
							
				If Not db1Cell.CellAmount = 0 And Not db2Cell.CellAmount = 0 And activity1 = activity2 And customer1 = customer2 Then
	
					Dim resultCell1 As New DataBufferCell(db1Cell)
					Dim resultCell2 As New DataBufferCell(db2Cell)
	
	'				Dim u3Id As Integer = api.Members.GetMemberId(DimType.UD3.Id, "None")
	'				Dim u5Id As Integer = api.Members.GetMemberId(DimType.UD5.Id, "None")
	'				Dim u6Id As Integer = api.Members.GetMemberId(DimType.UD6.Id, "None")
	'				Dim u7Id As Integer = api.Members.GetMemberId(DimType.UD7.Id, "None")
	'				Dim u8Id As Integer = api.Members.GetMemberId(DimType.UD8.Id, "None")					
	'				resultCell.DataBufferCellPk.UD3Id = u3Id
	'				resultCell.DataBufferCellPk.UD5Id = u5Id
	'				resultCell.DataBufferCellPk.UD6Id = u6Id
	'				resultCell.DataBufferCellPk.UD7Id = u7Id
	'				resultCell.DataBufferCellPk.UD8Id = u8Id
	
					resultCell1.CellAmount = resultCell1.CellAmount * resultCell2.CellAmount					
					result.SetCell(si, resultCell1, False)			
			End If	
		Next	
	Next
	
	'Guardamos los resultados del databuffer en la base de datos
	Dim exprDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("A#2000_100:F#None:O#Forms:V#Periodic:I#None:U1#None")
	api.Data.SetDataBuffer(result, exprDestInfo,,,,,,,,,,,,,True)
	
#End Region
	
End Sub

Sub Calc_Budget2 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal entity As String, ByVal period As String, ByVal scenario As String, ByVal fixPeriod As String)		
		
	#Region "DataBuffer con Loop"
	
'	'Creamos databuffer
'		Dim db1 As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(A#2000_100:F#None:O#Forms:V#Periodic, U2#Top.Base, U4#TotalCustomers.Base)")	
		
'		'Ejecutamos databuffer celda por celda
'		For Each db1Cell As DataBufferCell In db1.DataBufferCells.Values		
			
'			'Obtenemos UD2-Activity y UD4-Customer en curso
'			Dim activity As String = db1Cell.DataBufferCellPk.GetUD2Name(api)
'			Dim customer As String = db1Cell.DataBufferCellPk.GetUD4Name(api)		
			
'			'Obtenemos Price y Amount
'			Dim variation As Decimal = api.Data.GetDataCell("Cb#Houston:E#[" & entity & "]:C#Local:S#" & scenario & ":T#" & fixPeriod & ":V#Periodic:A#Variation:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None").CellAmount
'			Dim amount As Decimal = api.Data.GetDataCell("Cb#Houston:E#[" & entity & "]:C#Local:S#" & scenario & ":T#" & fixPeriod & ":V#Periodic:A#AmountVar:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None").CellAmount			
		
'			If Not variation = 0 And Not amount = 0 Then
'				'Ejecutamos Calculo
'				api.Data.Calculate("A#2000_100:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None = (" & variation & "/100) * " & amount, True)			
'			End If	
'	Next
	
	#End Region
	
	#Region "DataBuffer con SetData"
	
		'Creamos databuffer en blanco para guardar los resultados
		Dim result As New DataBuffer
		
		'Creamos databuffer
		Dim db1 As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveNoData(A#Variation:T#" & fixPeriod & ":F#None:O#Forms:V#Periodic:I#None:U1#None), U2#Top.Base, U4#TotalCustomers.Base)")	
		'Dim db2 As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveNoData(A#AmountVar:T#" & fixPeriod & ":F#None:O#Forms:V#Periodic:I#None:U1#None), U2#Top.Base, U4#TotalCustomers.Base)")
		Dim db2 As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveNoData(A#69000:T#" & fixPeriod & ":S#Actual:F#None:O#Top:V#Periodic:I#None:U1#Top:U3#Top), U2#Top.Base, U4#TotalCustomers.Base)")
		
		'T#|!prm_Time!|:U1#Top:U3#Top:O#Top:F#None:A#69000:V#YTD:C#Local
		'Ejecutamos databuffer celda por celda
		For Each db1Cell As DataBufferCell In db1.DataBufferCells.Values		
			For Each db2Cell As DataBufferCell In db2.DataBufferCells.Values
				
				'Obtenemos UD2-Activity y UD4-Customer en curso
				Dim activity1 As String = db1Cell.DataBufferCellPk.GetUD2Name(api)
				Dim customer1 As String = db1Cell.DataBufferCellPk.GetUD4Name(api)	
				Dim activity2 As String = db2Cell.DataBufferCellPk.GetUD2Name(api)
				Dim customer2 As String = db2Cell.DataBufferCellPk.GetUD4Name(api)
							
				If Not db1Cell.CellAmount = 0 And Not db2Cell.CellAmount = 0 And activity1 = activity2 And customer1 = customer2 Then
	
					Dim resultCell1 As New DataBufferCell(db1Cell)
					Dim resultCell2 As New DataBufferCell(db2Cell)
	
	'				Dim u3Id As Integer = api.Members.GetMemberId(DimType.UD3.Id, "None")
	'				Dim u5Id As Integer = api.Members.GetMemberId(DimType.UD5.Id, "None")
	'				Dim u6Id As Integer = api.Members.GetMemberId(DimType.UD6.Id, "None")
	'				Dim u7Id As Integer = api.Members.GetMemberId(DimType.UD7.Id, "None")
	'				Dim u8Id As Integer = api.Members.GetMemberId(DimType.UD8.Id, "None")					
	'				resultCell.DataBufferCellPk.UD3Id = u3Id
	'				resultCell.DataBufferCellPk.UD5Id = u5Id
	'				resultCell.DataBufferCellPk.UD6Id = u6Id
	'				resultCell.DataBufferCellPk.UD7Id = u7Id
	'				resultCell.DataBufferCellPk.UD8Id = u8Id
	
					resultCell1.CellAmount = (resultCell1.CellAmount/100) * resultCell2.CellAmount				
					result.SetCell(si, resultCell1, False)			
			End If	
		Next	
	Next
	
	'Guardamos los resultados del databuffer en la base de datos
	Dim exprDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("A#2000_100:F#None:O#Forms:V#Periodic:I#None:U1#None")
	api.Data.SetDataBuffer(result, exprDestInfo,,,,,,,,,,,,,True)
	
#End Region
	
End Sub

Sub Calc_Budget4 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal entity As String, ByVal period As String, ByVal scenario As String, ByVal fixPeriod As String)		
		
	#Region "DataBuffer con Loop"
	
		'Creamos databuffer
		Dim db1 As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(A#2000_100:F#None:O#Forms:V#Periodic, U2#Top.Base, U4#TotalCustomers.Base)")	
		
		'Ejecutamos databuffer celda por celda
		For Each db1Cell As DataBufferCell In db1.DataBufferCells.Values		
			
			'Obtenemos UD2-Activity y UD4-Customer en curso
			Dim activity As String = db1Cell.DataBufferCellPk.GetUD2Name(api)
			Dim customer As String = db1Cell.DataBufferCellPk.GetUD4Name(api)		
			
			'Obtenemos Price y Amount
			Dim amount As Decimal = api.Data.GetDataCell("Cb#Houston:E#[" & entity & "]:C#Local:S#" & scenario & ":T#" & fixPeriod & ":V#Periodic:A#AmountM:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None").CellAmount			
		
			'Driver para mensualizar el reparto de manera proporcional y respetando los ajustes hechos
				'Dividir YearTotal entre cada mes
			'Dim year As String = period.Substring(0,4)
	'EL AÑO ESTA CAMBIANDO CADA MES PORQUE EL TOTAL VARIA SEGUN CALCULO LOS MESES, SACAR TOTAL AÑO FUERA DEL BUFFER		
			'Dim yearTotal As Decimal = api.Data.GetDataCell("Cb#Houston:E#[" & entity & "]:C#Local:S#" & scenario & ":T#" & year & ":V#Periodic:A#2000_100:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None").CellAmount
			'Dim monthTotal As Decimal = api.Data.GetDataCell("Cb#Houston:E#[" & entity & "]:C#Local:S#" & scenario & ":V#Periodic:A#2000_100:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None").CellAmount
			'Dim driver As Decimal
			
			'If yearTotal <> 0 And monthTotal <> 0 Then 'yeartotal no puede ser 0
			'	driver = monthTotal / yearTotal
			'BRApi.ErrorLog.LogMessage(si, "CALCULANDO... "& period &" month:" & monthTotal & " - año "  & yeartotal & " = " & driver)
				'Else 
				'driver = 1
			'End If
			'BRApi.ErrorLog.LogMessage(si, activity & " - " & customer & " = " & driver)
			
			If Not amount = 0 Then
				'If scenario.Contains("BudgetV1") Or scenario.Contains("BudgetV2") Or scenario.Contains("BudgetV3") Then 
				'Ejecutamos Calculo para las otras versiones del Budget (V1, V2, V3) respetando los ajustes realizados (en versiones anteriores?)
					'api.Data.Calculate("A#2000_100:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None = " & amount & "*" & driver, True)			
				'BRApi.ErrorLog.LogMessage(si, "CALCULANDO... " & scenario & " - "  & activity & " - " & customer & " = " & driver)
				'Else
				'Ejecutamos Calculo para S#Budget - Proyección Lineal
					api.Data.Calculate("A#2000_100:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None = " & amount & "/12", True)			
					
				'End If
			End If	
	Next
	
	#End Region
	
End Sub
	
Sub Copy_Scenarios (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal entity As String, ByVal sSource As String, ByVal sTarget As String, ByVal period As String)
		
	#Region "DataBuffer con SetBuffer"	

	Dim results As New DataBuffer	
	Dim dataToCopy As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveNoData(S#" & sSource & ":A#2000_100:F#None:O#Forms:V#Periodic), U2#Top.Base, U4#TotalCustomers.Base)")
	
	For Each db1Cell As DataBufferCell In dataToCopy.DataBufferCells.Values
		Dim resultCell As New DataBufferCell(db1Cell)				
		results.SetCell(si, resultCell, False)			

	Next
	
	Dim exprDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("S#" & sTarget & ":A#2000_100:F#None:O#Forms:V#Periodic")
	api.Data.SetDataBuffer(results, exprDestInfo,,,,,,,,,,,,,True)
	
#End Region	
	
	#Region "DataBuffer con Loop"

'	Dim dataToCopy As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveNoData(S#" & sSource & ":A#2000_100:F#None:O#Forms:V#Periodic), U2#Top.Base, U4#TotalCustomers.Base)")
	
'	If Not dataToCopy Is Nothing Then
'		For Each dtcell As DataBufferCell In dataToCopy.DataBufferCells.Values
'			'Obtenemos UD2-Activity y UD4-Customer en curso
'			Dim activity As String = dtCell.DataBufferCellPk.GetUD2Name(api)
'			Dim customer As String = dtCell.DataBufferCellPk.GetUD4Name(api)
			
'			'Execute the Copy process if the DM job has the "Process=[Copy]"
'			'Need the IsDurableCalculatedData is True so that the data does not clear after a Calculate
'			api.Data.Calculate("A#2000_100:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None = " & dtcell.CellAmount, True)
			
'		Next
'	End If

#End Region

	#Region "Example Data Copy - Buffer"
		'COPIADO DE ESCENARIOS
	'			If Process.XFEqualsIgnoreCase("Copy") Then																	
	'				'COPY from the Source Scenario (ie: TXP_Actual) into the WF Scenario (ie: TXP_FCST_M5)
	'				'PV620 SV300: Replaced "RemoveZeros" with "RemoveNoData"
	'				Dim CopyDataFCST02 As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveNoData(S#" & SourceScenario & ":O#" & SourceOriginName & ":I#None:U5#None:U6#None:U8#None)" & _
	'		                 ",[A#" & AccountName & "],[F#" & FlowName & "],[U1#" & UD1Name & "],[U2#" & UD2Name & "],[U3#" & UD3Name & "],[U4#" & UD4Name & "],[U7#" & UD7Name & "])")
		 
	'				If Not CopyDataFCST02 Is Nothing Then
	'					'loop through the source buffer cells and filter out cells that do no have data and are equal to zero
	'					For Each varDataCell As DataBufferCell In CopyDataFCST02.DataBufferCells.Values
	'						Dim varDataCellPk As New DataBufferCellPk(varDataCell.DataBufferCellPk)
	'						Dim curAcct As String = varDataCellPk.GetAccountName(api)
	'						Dim curFlow As String = varDataCellPk.GetFlowName(api)
	'						Dim curUD1 As String = varDataCellPk.GetUD1Name(api)
	'						Dim curUD2 As String = varDataCellPk.GetUD2Name(api)
	'						Dim curUD3 As String = varDataCellPk.GetUD3Name(api)
	'						Dim curUD4 As String = varDataCellPk.GetUD4Name(api)
	'						Dim curUD7 As String = varDataCellPk.GetUD7Name(api)
		
	'						'-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------		
	'						'Execute the Copy process if the DM job has the "Process=[Copy]"
	'						'Need the IsDurableCalculatedData is True so that the data does not clear after a Calculate
	'						api.Data.Calculate("O#" & DestOriginName & ":A#" & curAcct & ":F#" & curFlow & ":I#None:U1#" & curUD1 & ":U2#" & curUD2 & ":U3#" & curUD3 & ":U4#" & curUD4 & _
	'																   ":U5#None:U6#None:U7#" & curUD7 & ":U8#None = " & varDataCell.CellAmount, True)
	'						'----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------									
	'					Next
	'				End If
	#End Region

End Sub

#End Region

#Region "Drivers"

'Function getYearTotal (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal entity As String, ByVal scenario As String, ByVal fixperiod As String, ByVal activity As String, ByVal customer As String)				
	
''	Dim calc As Decimal
''	'Obtenemos Price y Amount
''	Dim price As Decimal = api.Data.GetDataCell("Cb#Houston:E#[" & entity & "]:C#Local:S#" & scenario & ":T#" & fixperiod & ":V#Periodic:A#Price:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None").CellAmount
''	Dim amount As Decimal = api.Data.GetDataCell("Cb#Houston:E#[" & entity & "]:C#Local:S#" & scenario & ":T#" & fixperiod & ":V#Periodic:A#Amount:F#None:O#Forms:I#None:U1#None:U2#[" & activity.ToString & "]:U3#None:U4#[" & customer.ToString & "]:U5#None:U6#None:U7#None:U8#None").CellAmount			
	
''	If Not price = 0 And Not amount = 0 Then
''		'Ejecutamos Calculo
''		calc = price * amount			
''	End If	

''	Return calc
	
'End Function


#End Region

	End Class
End Namespace