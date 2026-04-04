Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports System.Windows.Forms
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database

Namespace OneStream.BusinessRule.Finance.ALL_AllocationCalc
	
	Public Class MainClass
			
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
		
			Try
				
				'Variables Globales
				Dim sScenario As String = api.Pov.Scenario.Name	
				Dim sScenarioType As String = api.Pov.ScenarioType.Name							
				Dim sTime As String = api.Pov.Time.Name	
				Dim sYear As String = sTime.Substring(0, 4)
			'	Dim sYear As String = sTime
				Dim MesFcstNumber As Integer = api.Scenario.GetWorkflowNumNoInputTimePeriods
				Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))	
				
				Dim sItemID As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("ItemID")				
				Dim sFunction As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("Function")
				
				'Brapi.ErrorLog.LogMessage(si, "ScenarioType:" & sScenarioType)
				'Brapi.ErrorLog.LogMessage(si, "Time:" & sTime)
				
				If (Not api.Entity.HasChildren) And api.Cons.IsLocalCurrencyForEntity() Then

					Dim sEntity As String = api.Pov.Entity.Name
					'Brapi.ErrorLog.LogMessage(si, "Entity: " & sEntity)				
					
					'BORRAR Repartos TODAS Reglas
					If sFunction = "Clear_Allocation_Rules_All" Then
						
						Me.TransformToCalculateAllocationRulesAll(si, api, globals, args, sTime, sScenario)
						Me.ClearAllocationRulesAll(si, api, globals, args, sTime, sScenario)
						
					'BORRAR Repartos UNA Regla
					Else If sFunction = "Clear_Allocation_Rules_One" Then
						
						Me.TransformToCalculateAllocationRulesAll(si, api, globals, args, sTime, sScenario)
						Me.ClearAllocationRulesOne(si, api, globals, args, sTime, sScenario, sItemID)		
					
					'EJECUTAR Repatos TODAS Reglas
					Else If sFunction = "Execute_Allocation_Rules_All"
						If (sScenarioType = "Actual") Or (sScenarioType = "Budget") Or  (sScenarioType = "Forecast" And sMes > MesFcstNumber) Then
						'	Me.Brazil_002_Volumes_Copy(si, api, globals, args, sEntity, sScenario)
							Me.CopyData_ADJ_to_Import(si, api, globals, args)
							Me.TransformToCalculateAllocationRulesAll(si, api, globals, args, sTime, sScenario)
							Me.ClearAllocationRulesAll(si, api, globals, args, sTime, sScenario)
							Me.ExecuteAllocationRulesAll(si, api, globals, args, sTime, sScenario, sYear)	
							Me.CopyData_ImportU7Temp_to_ADJU7None(si, api, globals, args)	
						'	Me.Brazil_002_Volumes_Delete(si, api, globals, args, sEntity, sScenario)
						End If
					'EJECUTAR Repatos UNA Regla
					Else If sFunction = "Execute_Allocation_Rules_One"							
						
						Me.TransformToCalculateAllocationRulesAll(si, api, globals, args, sTime, sScenario)
						Me.ClearAllocationRulesOne(si, api, globals, args, sTime, sScenario, sItemID)
						Me.ExecuteAllocationRulesOne(si, api, globals, args, sTime, sScenario, sItemID, sYear)	
						
					End If							
				End If
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
'---------------------------------------------------------------------------------------------------------------------------------		
		
		Sub Brazil_002_Volumes_Copy (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sEntity As String, sScenario As String)	

			If sScenario = "Actual"  Then
			
				If sEntity = "R1303002"  Then
				
					Dim Driver_AccountsList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account_Details", "A#Drivers_Actual.base", True)
					Dim Scenario As String = api.Pov.Scenario.Name
					
					For Each Driver_Account As MemberInfo In Driver_AccountsList
					
						' Eliminar dato destino
						api.Data.ClearCalculatedData("S#" & Scenario & ":E#R1303002:O#Import:A#" & Driver_Account.Member.Name,True,True,True,True)
		
						' Copia de Volumenes de R1303001 a R1303002
						api.Data.Calculate( "S#" & Scenario & ":E#R1303002:O#Import:A#" & Driver_Account.Member.Name & " = S#" & Scenario & ":E#R1303001:O#Import:A#" & Driver_Account.Member.Name, True)
	
					Next
						
				End If
				
			Else If LEFT(sScenario, 6) = "Budget" Then 
			
				If sEntity = "R1303002"  Then
				
					Dim Driver_AccountsList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account_Details", "A#Drivers_Budget.base", True)
					Dim Scenario As String = api.Pov.Scenario.Name
					
					For Each Driver_Account As MemberInfo In Driver_AccountsList
					
						' Eliminar dato destino
						api.Data.ClearCalculatedData("S#" & Scenario & ":E#R1303002:O#Import:A#" & Driver_Account.Member.Name,True,True,True,True)
		
						' Copia de Volumenes de R1303001 a R1303002
						api.Data.Calculate( "S#" & Scenario & ":E#R1303002:O#Import:A#" & Driver_Account.Member.Name & " = S#" & Scenario & ":E#R1303001:O#Import:A#" & Driver_Account.Member.Name, True)
	
					Next
						
				End If
				
			End If	
		
		End Sub		
		
'-----------------------------------------------------------------------------------------------------------------------------------			
		
		Sub Brazil_002_Volumes_Delete (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sEntity As String, sScenario As String)	

			If LEFT(sScenario, 6) = "Budget" Then 
				If sEntity = "R1303002"  Then
			
					Dim Driver_AccountsList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account_Details", "A#Drivers_Budget.base", True)
					Dim Scenario As String = api.Pov.Scenario.Name	
					
					For Each Driver_Account As MemberInfo In Driver_AccountsList
					
						' Eliminar dato destino
						api.Data.ClearCalculatedData("S#" & Scenario & ":E#R1303002:O#Import:A#" & Driver_Account.Member.Name,True,True,True,True)
					
					Next	
						
						api.Data.ClearCalculatedData("S#" & Scenario & ":E#R1303002:O#Import:O#Import:O#Import:A#Vol_Sales",True,True,True,True)
						api.Data.ClearCalculatedData("S#" & Scenario & ":E#R1303002:O#Import:O#Import:A#Vol_Prod",True,True,True,True)
						api.Data.ClearCalculatedData("S#" & Scenario & ":E#R1303002:O#Import:A#Volume",True,True,True,True)
						
				End If	
			End If	
				
		End Sub					
		
'---------------------------------------------------------------------------------------------------------------------------------		
		
		Sub CopyData_ADJ_to_Import (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)	

		Dim Scenario As String = api.Pov.Scenario.Name
		Dim calculationExpression As String = "S#" & Scenario & ":O#Import:U7#Temp:V#Periodic = S#" & Scenario & ":O#AdjInput:U7#None:V#YTD"
		
		' Eliminar dato destino en UD7 = Temp
		api.Data.ClearCalculatedData("S#" & Scenario & ":O#Import:U7#Temp",True,True,True,True)
		' Copia de AdjInput a Import en UD7 = Temp
		api.Data.Calculate(calculationExpression, True)
	
		End Sub		
		
'-----------------------------------------------------------------------------------------------------------------------------------			

		Sub CopyData_ImportU7Temp_to_ADJU7None (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)	


		Dim Scenario As String = api.Pov.Scenario.Name
		Dim calculationExpression As String = "S#" & Scenario & ":O#AdjInput:U7#None:V#YTD = S#" & Scenario & ":O#Import:U7#Temp:V#Periodic"
		
		' Eliminar dato destino en Origin = AdjInput, UD7 = None
		api.Data.ClearCalculatedData("S#" & Scenario & ":O#AdjInput:U7#None",True,True,True,True)
		' Copia de Import a AdjInput en UD7 = None
		api.Data.Calculate(calculationExpression, True)
		
		' Eliminar todos los datos en UD7 = Temp
		api.Data.ClearCalculatedData("S#" & Scenario & ":U7#Temp",True,True,True,True)
		
		End Sub			
		
'---------------------------------------------------------------------------------------------------------------------------------		

#Region "Consultas"


		Sub TransformToCalculateAllocationRulesAll (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sTime As String, sScenario As String)
			
			api.data.calculate("U6#Allocation_In = 0 * U6#Allocation_In")
			api.data.calculate("U6#Allocation_Out = 0 * U6#Allocation_Out")
		End Sub


		Sub ClearAllocationRulesAll (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sTime As String, sScenario As String)
		
			api.Data.ClearCalculatedData(True, True, True, True,,,,,,,,,"U5#Rules.Base","U6#Net_Change.Base",, )	
	
		End Sub	

		Sub ClearAllocationRulesOne (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sTime As String, sScenario As String, sItemID As String)
		
			Dim sRuleName As String = String.Empty				
		
			Dim sbSQL As New Text.StringBuilder()
						
			sbSQL.Append(" SELECT Rule_Name")			
			sbSQL.Append(" FROM XFC_ALL_AllocationRules")																				
			sbSQL.Append(" WHERE ItemID = '" & sItemID & "'")			
							
			'Brapi.ErrorLog.LogMessage(si, sbSQL.ToString)
			
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)											
						
					sRuleName =  dt.Rows(0)(0).ToString
					
					'(clearCalculatedData, clearTranslatedData, clearConsolidatedData, clearDurableCalculatedData,A#,F#,O#,I#,UD1#,UD2#,UD3#,UD4#,UD5#,UD6#,UD7#,UD8#)
					api.Data.ClearCalculatedData(True, True, True, True,,,,,,,,,"U5#" & sRuleName & "","U6#Net_Change.Base",, )	
					
				End Using
			End Using
		
		
		End Sub

		Sub ExecuteAllocationRulesAll (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sTime As String, sScenario As String, sYear As String)		

			Dim sbSQL As New Text.StringBuilder()
						
			sbSQL.Append(" SELECT Rule_Name")
	
			sbSQL.Append(" , Source_Entity")			
			sbSQL.Append(" , Source_Account")		
			sbSQL.Append(" , Source_CostCenter")				
			sbSQL.Append(" , Source_Product")
			sbSQL.Append(" , Source_Customer")	
			
			sbSQL.Append(" , Target_CostCenter")			
			sbSQL.Append(" , Target_Product")
			sbSQL.Append(" , Target_Customer")				
			
			sbSQL.Append(" , Driver_CostCenter")			
			sbSQL.Append(" , Driver_Product")
			sbSQL.Append(" , Driver_Customer")				
			sbSQL.Append(" , Driver_Account")								
				
			sbSQL.Append(" FROM XFC_ALL_AllocationRules")																	
			
			sbSQL.Append(" WHERE POV_Scenario = '" & sScenario & "'")
			sbSQL.Append(" AND POV_Time = '" & sYear & "'")
			'sbSQL.Append(" AND POV_Time = '2025'")
			sbSQL.Append(" AND Rule_Enabled = 1")			
			
			sbSQL.Append(" ORDER BY Rule_Sequence")
							
			'Brapi.ErrorLog.LogMessage(si, sbSQL.ToString)
			
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)															
					
					For Each dr As DataRow In dt.Rows
						
						'Ejecución de repartos
						Me.CalculateAllocationRules(si, api, dr, sScenario)																
						
					Next
					
				End Using
			End Using
					
			
		End Sub	

		Sub ExecuteAllocationRulesOne (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sTime As String, sScenario As String, sItemID As String, sYear As String)				
		
			Dim sbSQL As New Text.StringBuilder()

			sbSQL.Append(" SELECT Rule_Name")
			
			sbSQL.Append(" , Source_Entity")			
			sbSQL.Append(" , Source_Account")	
			sbSQL.Append(" , Source_CostCenter")				
			sbSQL.Append(" , Source_Product")
			sbSQL.Append(" , Source_Customer")	
			
			sbSQL.Append(" , Target_CostCenter")			
			sbSQL.Append(" , Target_Product")
			sbSQL.Append(" , Target_Customer")				
			
			sbSQL.Append(" , Driver_CostCenter")			
			sbSQL.Append(" , Driver_Product")
			sbSQL.Append(" , Driver_Customer")				
			sbSQL.Append(" , Driver_Account")									
				
			sbSQL.Append(" FROM XFC_ALL_AllocationRules")																	
			
			sbSQL.Append(" WHERE POV_Scenario = '" & sScenario & "'")
			sbSQL.Append(" AND POV_Time = '" & sYear & "'")
			'sbSQL.Append(" AND POV_Time = '2025'")
			sbSQL.Append(" AND Rule_Enabled = 1")			
			sbSQL.Append(" AND ItemID = '" & sItemID & "'")
			
			sbSQL.Append(" ORDER BY Rule_Sequence")			
							
			'Brapi.ErrorLog.LogMessage(si, sbSQL.ToString)
			
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)																
					
					For Each dr As DataRow In dt.Rows
						
						'Ejecución de repartos
						Me.CalculateAllocationRules(si, api, dr, sScenario)	
						
					Next
					
				End Using
			End Using
								
		End Sub	
		
#End Region

#Region "Calculos"

		Sub CalculateAllocationRules (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal dr As DataRow, ByVal sScenario As String)		
		
			'Entidad que estamos recorriendo
			Dim sEntity_MemberInfo As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & api.Pov.Entity.Name, True)									
			Dim sEntity = Api.Pov.Entity.Name		
			
			Dim sRule_Name As String = String.Empty

			Dim sSource_Entity As String = String.Empty				
			Dim sSource_Account As String = String.Empty							
			Dim sSource_Type As String = String.Empty
			Dim sSource_Product As String = String.Empty			
			Dim sSource_Customer As String = String.Empty		
			
			Dim sTarget_Type As String = String.Empty
			Dim sTarget_Product As String = String.Empty
			Dim sTarget_Customer As String = String.Empty
	
			Dim sDriver_Type As String = String.Empty
			Dim sDriver_Product As String = String.Empty
			Dim sDriver_Customer As String = String.Empty			
			Dim sDriver_Account As String = String.Empty
			
			sRule_Name = 			dr(0).ToString		
	
			sSource_Entity =		dr(1).ToString			
			sSource_Account =		dr(2).ToString
			sSource_Type =			If(dr(3).ToString = "Top", "Type", dr(3).ToString)
			sSource_Product = 		dr(4).ToString
			sSource_Customer = 		dr(5).ToString
			
			sTarget_Type =			If(dr(6).ToString = "Top", "Type", dr(6).ToString)
			sTarget_Product = 		dr(7).ToString
			sTarget_Customer = 		dr(8).ToString	
				
			sDriver_Type =			If(dr(9).ToString = "Top", "Type", dr(9).ToString)
			sDriver_Product = 		dr(10).ToString
			sDriver_Customer = 		dr(11).ToString				
			sDriver_Account = 		dr(12).ToString						
	
			Dim sSource_Entity_List As List(Of MemberInfo) = GetMembers(si, sSource_Entity, "E#", "Entities", "")													
			
			If (sSource_Entity_List.Contains(sEntity_MemberInfo(0)))																	
					
				'1. OBTENCIÓN MIEMBROS SOURCE & TARGET
				
				'Obtenemos miembros ORIGEN a recorrer			
				Dim sSource_Account_List As List(Of MemberInfo) = GetMembers(si, sSource_Account, "A#", "Account_Details", "")														
				Dim sSource_Customer_List As List(Of MemberInfo) = GetMembers(si, sSource_Customer, "U1#", "Customer", "")
				Dim sSource_Product_List As List(Of MemberInfo) = GetMembers(si, sSource_Product, "U2#", "Product", "")	
				Dim sSource_Type_List As List(Of MemberInfo) = GetMembers(si, sSource_Type, "U7#", "CF_Conso", "")				
				
				'Obtenemos miembros TARGET a recorrer
				Dim sTarget_Customer_List As New List(Of MemberInfo)
				Dim sTarget_Product_List As New List(Of MemberInfo)
				Dim sTarget_Type_List As New List(Of MemberInfo)
				
				Dim sUD1SameAsSource As String = String.Empty
				Dim sUD7SameAsSource As String = String.Empty
				
				'Customer
				If sTarget_Customer = "Same as source" Then
					sTarget_Customer_List = sSource_Customer_List				
				Else
					Dim sTarget_Customer_ListFiltered As New List(Of MemberInfo) 
					sTarget_Customer_ListFiltered = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Customer", "U1#"& sTarget_Customer &".Base", True)
					' sTarget_Customer_List = GetMembers(si, sTarget_Customer, "U1#", "Customer", "")	
					
					For Each customer As MemberInfo In sTarget_Customer_ListFiltered
						
						Dim customerName As String = customer.Member.Name
						Dim value As Double = api.Data.GetDataCell("A#"& sDriver_Account &":U1#" & customerName &":V#YTD:I#Top:O#Top:F#Top:U2#Top:U3#Top:U4#Top:U7#Type").CellAmount
						
						If value <> 0 Then
							sTarget_Customer_List.Add(customer)
						End If
						
					Next
					
				End If
				
				'Product
				If sTarget_Product = "Same as source" Then
					sTarget_Product_List = sSource_Product_List				
				Else
					sTarget_Product_List = GetMembers(si, sTarget_Product, "U2#", "Product", "")	
					
'					For Each product As MemberInfo In sSource_Product_List
						
'						Dim productName As String = product.Member.Name
'						Dim value As Double = api.Data.GetDataCell("A#"& sDriver_Account &":U2#" & productName &":I#Top:O#Top:F#Top:U1#Top:U3#Top:U4#Top").CellAmount
						
'						If value <> 0 Then
'							sTarget_Product_List.Add(product)
'						End If
						
'					Next

				End If
				
				'Type
				If sTarget_Type = "Same as source" Then
					sTarget_Type_List = sSource_Type_List				
				Else
					sTarget_Type_List = GetMembers(si, sTarget_Type, "U7#", "CF_Conso", "")		
				End If						
				
				'2. DEFINICIÓN DATABUFFER
				
				Dim diDestinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#YTD")
				Dim dbSourceDataBuffer As DataBuffer
				'dbSourceDataBuffer = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, "RemoveNoData(U5#Top:U6#Top)" , diDestinationInfo)
				'dbSourceDataBuffer = api.Data.GetDataBufferUsingFormula("RemoveNoData(U5#Top:U6#Top)",DataApiScriptMethodType.Calculate,,diDestinationInfo,,)
				dbSourceDataBuffer = api.Data.GetDataBufferUsingFormula("RemoveNoData(V#YTD:U5#Top:U6#Top)",DataApiScriptMethodType.Calculate,False,diDestinationInfo)
				
				Dim filters_Account As Dictionary(Of Integer, Object) = GetFilter(sSource_Account_List)
				Dim filters_Customer As Dictionary(Of Integer, Object) = GetFilter(sSource_Customer_List)
				Dim filters_Product As Dictionary(Of Integer, Object) = GetFilter(sSource_Product_List)
				Dim filters_Type As Dictionary(Of Integer, Object) = GetFilter(sSource_Type_List)	

				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.Account, filters_Account)
				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.UD1, filters_Customer)
				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.UD2, filters_Product)
				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.UD7, filters_Type)
				
				'3. OBTENCIÓN DATATABLE DRIVERS
				Dim dtDriver = Me.GetDriverDataTable(si, api, sDriver_Account _
									, sDriver_Type, sTarget_Type_List _
									, sDriver_Product, sTarget_Product_List _
									, sDriver_Customer, sTarget_Customer_List)
									
				If Not dbSourceDataBuffer Is Nothing Then														
					
					Dim dbAllocationIn As DataBuffer = New DataBuffer()
					Dim dbAllocationOut As DataBuffer = New DataBuffer()					
					
					Dim i As Integer = 0					
					For Each dbcSourceCell As DataBufferCell In dbSourceDataBuffer.DataBufferCells.Values						
						
	
						
						'LogWriteCellData(si, api, 0, dbcSourceCell)					
						If (Not dbcSourceCell.CellStatus.IsNoData And dbcSourceCell.CellAmount <> 0)	
							'Brapi.ErrorLog.LogMessage(si,"Pasa por aqui")							
							i = i + 1
							'LogWriteCellData(si, api, i, dbcSourceCell)																
							
							If(sTarget_Customer = "Same as source" Or sTarget_Customer = sSource_Customer)
								sTarget_Customer_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Customer", "U1#" & api.Members.GetMember(DimType.UD1.Id, dbcSourceCell.DataBufferCellPk.UD1Id).Name, True)	
							End If
							
							If(sTarget_Product = "Same as source" Or sTarget_Product = sSource_Product)
								sTarget_Product_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Product", "U2#" & api.Members.GetMember(DimType.UD2.Id, dbcSourceCell.DataBufferCellPk.UD2Id).Name, True)	
							End If
							
							If(sTarget_Type = "Same as source" Or sTarget_Type = sSource_Type)
								sTarget_Type_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CF_Conso", "U7#" & api.Members.GetMember(DimType.UD7.Id, dbcSourceCell.DataBufferCellPk.UD7Id).Name, True)	
							End If			
								
'							If (sTarget_Type_List(0).Member.Name = "ENG_5.3")
'								Brapi.ErrorLog.LogMessage(si,"CC:" & api.Members.GetMemberName(DimType.UD3.Id,dbcSourceCell.DataBufferCellPk.UD3Id))
'								Brapi.ErrorLog.LogMessage(si,"CC2:" & sTarget_Type_List(0).Member.Name)
'							End If								
							
							Dim bAllocationIn As Boolean = False												
							
							For Each sTarget_Customer_Member As MemberInfo In sTarget_Customer_List
								
								For Each sTarget_Product_Member As MemberInfo In sTarget_Product_List
									
									For Each sTarget_Type_Member As MemberInfo In sTarget_Type_List																																																		
									
'							If (sTarget_Type_Member.Member.Name = "ENG_5.3")
'								Brapi.ErrorLog.LogMessage(si,"Account:" & api.Members.GetMemberName(DimType.Account.Id,dbcSourceCell.DataBufferCellPk.AccountId))
'								BRApi.ErrorLog.LogMessage(si,"Amount: " & dbcSourceCell.CellAmount.ToString())
'							End If												

										Dim dDataDriver As Double = GetDriverData(si, api, dtDriver, sDriver_Account _ 											
											, sDriver_Customer, sTarget_Customer_Member.Member.Name _
											, sDriver_Product, sTarget_Product_Member.Member.Name _
											, sDriver_Type, sTarget_Type_Member.Member.Name )																						
											
											'BRAPI.ErrorLog.LogMessage(si,"Driver Data: " & dDataDriver.ToString)
											
										If dDataDriver <> 0																																
											
											'BRAPI.ErrorLog.LogMessage(si,"Product: " & sTarget_Product_Member.Member.Name)
											
											Dim dbcAllocationInCell As New DataBufferCell(dbcSourceCell)										
											
											'MODIFICACIÓN registros de Allocation In
											dbcAllocationInCell.DataBufferCellPk.UD1Id = api.Members.GetMemberId(DimType.UD1.Id,sTarget_Customer_Member.Member.Name)
											dbcAllocationInCell.DataBufferCellPk.UD2Id = api.Members.GetMemberId(DimType.UD2.Id,sTarget_Product_Member.Member.Name)
											dbcAllocationInCell.DataBufferCellPk.UD7Id = api.Members.GetMemberId(DimType.UD7.Id,sTarget_Type_Member.Member.Name)																															
											dbcAllocationInCell.DataBufferCellPk.UD5Id = api.Members.GetMemberId(DimType.UD5.Id,sRule_Name)
											dbcAllocationInCell.DataBufferCellPk.UD6Id = api.Members.GetMemberId(DimType.UD6.Id,"Allocation_IN")	
											
											'DATO = DATO ORIGEN * PESO
											dbcAllocationInCell.CellAmount = dbcAllocationInCell.CellAmount * dDataDriver.ToString		
																																	
											'LogWriteCellData(si, api, i, dbcAllocationInCell)
											
											'GUARDAMOS el registro de ALLOCATION IN en el BUFFER
											dbAllocationIn.SetCell(si, dbcAllocationInCell,True)
											
											bAllocationIn = True
											
										End If
										
									Next	
									
								Next	
								
							Next
							
							'ALLOCATION OUT
							If (bAllocationIn)
								
								Dim dbcAllocationOutCell As New DataBufferCell(dbcSourceCell)
															
								dbcAllocationOutCell.DataBufferCellPk.UD5Id = api.Members.GetMemberId(DimType.UD5.Id,sRule_Name)
								dbcAllocationOutCell.DataBufferCellPk.UD6Id = api.Members.GetMemberId(DimType.UD6.Id,"Allocation_OUT")						
								dbcAllocationOutCell.CellAmount = dbcAllocationOutCell.CellAmount * -1
							
								'LogWriteCellData(si, api, i, dbcAllocationOutCell)
								
								'GUARDAMOS el registro de ALLOCATION OUT en el BUFFER
								dbAllocationOut.SetCell(si, dbcAllocationOutCell,True)
							End If
						End If
					Next
					
					'ENVIAMOS los buffer de ALLOCATION IN y ALLOCATION OUT para guardar los cambios
					api.Data.SetDataBuffer(dbAllocationIn, diDestinationInfo,,,,,,,,,,,,,True)
					api.Data.SetDataBuffer(dbAllocationOut, diDestinationInfo,,,,,,,,,,,,,True)					
					
				End If										
				
			End If				
		
		End Sub		

#End Region		
		
#Region "Miembros"

		Function GetMembers (ByVal si As SessionInfo, ByVal sMemberSource As String, ByVal sTag As String, ByVal sDimension As String, ByVal sMember_NoAllocation_Filter As String) As List(Of MemberInfo)
			
			Dim MemberList As List(Of MemberInfo) = New List(Of MemberInfo)
			Dim MemberList_NoAllocation As List(Of MemberInfo) = New List(Of MemberInfo)
			
			If (sMember_NoAllocation_Filter <> String.Empty)
				BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sMember_NoAllocation_Filter, True)
			End If
			
			For Each sMember As String In sMemberSource.Split(",")
				For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMember.Trim & ".Base", True)
					'Brapi.ErrorLog.LogMessage(si, "Member: " & miMember.Member.Name)
					
					If Not MemberList.Contains(miMember) And Not MemberList_NoAllocation.Contains(miMember)
						MemberList.Add(miMember)
					End If
				Next
			Next						
			
			Return MemberList
			
		End Function		
				
#End Region		

#Region "Drivers"

		Function GetDriverDataTable(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal sDriver_Account As String, ByVal sDriver_Type As String, ByVal sTarget_Type_List As List(Of MemberInfo), ByVal sDriver_Product As String, ByVal sTarget_Product_List As List(Of MemberInfo), ByVal sDriver_Customer As String, ByVal sTarget_Customer_List As List(Of MemberInfo))	As DataTable
					
			Dim dtDriver As New DataTable("Driver_Data")
		
			Dim objCol = New DataColumn
            objCol.ColumnName = "Account"
            objCol.DataType = GetType(String)
            objCol.DefaultValue = ""
            objCol.AllowDBNull = False
			dtDriver.Columns.Add(objCol)
			
			objCol = New DataColumn
            objCol.ColumnName = "Customer"
            objCol.DataType = GetType(String)
            objCol.DefaultValue = ""
            objCol.AllowDBNull = False
			dtDriver.Columns.Add(objCol)			
			
			objCol = New DataColumn
            objCol.ColumnName = "Product"
            objCol.DataType = GetType(String)
            objCol.DefaultValue = ""
            objCol.AllowDBNull = False
			dtDriver.Columns.Add(objCol)
			
			objCol = New DataColumn
            objCol.ColumnName = "Type"
            objCol.DataType = GetType(String)
            objCol.DefaultValue = ""
            objCol.AllowDBNull = False
			dtDriver.Columns.Add(objCol)				
			
			objCol = New DataColumn
			objCol.ColumnName = "Amount"
			objCol.DataType = GetType(Double)
            objCol.DefaultValue = 0.00
            objCol.AllowDBNull = True
            dtDriver.Columns.Add(objCol)
			
			' Early return if sDriver Account is empty
			If String.IsNullOrEmpty(sDriver_Account) Then Return dtDriver
						
			If Not sDriver_Customer = "Allocation" And Not sDriver_Type = "Same as target"
				sTarget_Customer_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Customer", "U1#" & sDriver_Customer, True)
			End If		
				
			If Not sDriver_Product = "Allocation" And Not sDriver_Product = "Same as target"
				sTarget_Product_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Product", "U2#" & sDriver_Product, True)
			End If	
			
			If Not sDriver_Type = "Allocation" And Not sDriver_Type = "Same as target"
				sTarget_Type_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CF_Conso", "U7#" & sDriver_Type, True)
			End If				
						
			Dim sCalc As String = String.Empty
			Dim dData As Decimal = 0
			Dim row As DataRow		
			
			For Each sTarget_Customer_Member As MemberInfo In sTarget_Customer_List														
				
				For Each sTarget_Product_Member As MemberInfo In sTarget_Product_List
										
					For Each sTarget_Type_Member As MemberInfo In sTarget_Type_List	
														
						'brapi.ErrorLog.LogMessage(si, "1")											
						row = dtDriver.NewRow									
						row("Account") = sDriver_Account
						row("Customer") = sTarget_Customer_Member.Member.Name												
						row("Product") = sTarget_Product_Member.Member.Name	
						row("Type") = sTarget_Type_Member.Member.Name														
																									
						'sCalc = "A#" & row("Account") & ":C#Local:O#Top:F#Top:I#Top:V#Periodic:U1#" & row("Customer") & ":U2#" & row("Product") & ":U3#" & row("Type") & ":U4#None:U5#Top:U6#Top:U8#None"															
						sCalc = "A#" & row("Account") & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U1#[" & row("Customer") & "]:U2#[" & row("Product") & "]:U3#Top:U4#None:U5#Top:U6#Top:U7#[" & row("Type") & "]:U8#None"															
						dData = api.Data.GetDataCell(sCalc).CellAmount
					
						row("Amount") = dData
						
						'If dData > 0 And sTarget_Product_Member.Member.Name = "5841088" Then
						'If sTarget_Product_Member.Member.Name = "5841088" Then
							'BRApi.ErrorLog.LogMessage(si,"dData: " & dData.ToString())
						'End If
						
						dtDriver.Rows.Add(row.ItemArray)
						
					Next
				Next
			Next						
			
			Return dtDriver
			
		End Function

		Function GetDriverData(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal dtDriver As Datatable, ByVal sDriver_Account As String, ByVal sDriver_Customer As String, ByVal sTarget_Customer_Base As String, ByVal sDriver_Product As String, ByVal sTarget_Product_Base As String, ByVal sDriver_Type As String, ByVal sTarget_Type_Base As String) As Double
									
			Dim sFilterNum As String = String.Empty
			Dim sFilterDen As String = String.Empty			
			
			'Filtros Customer
			If sDriver_Customer = ("Allocation") 
				sFilterNum = sFilterNum & " AND Customer = '" & sTarget_Customer_Base & "'"	
			Else If sDriver_Customer = ("Same as target")
				sFilterNum = sFilterNum & " AND Customer = '" & sTarget_Customer_Base & "'"	
				sFilterDen = sFilterDen & " AND Customer = '" & sTarget_Customer_Base & "'"
			End If					
			
			'Filtros UD3
			If sDriver_Product = ("Allocation") 
				sFilterNum = sFilterNum & " AND Product = '" & sTarget_Product_Base & "'"	
			Else If sDriver_Product = ("Same as target")
				sFilterNum = sFilterNum & " AND Product = '" & sTarget_Product_Base & "'"	
				sFilterDen = sFilterDen & " AND Product = '" & sTarget_Product_Base & "'"
			End If								
			
			If sDriver_Type = ("Allocation") 
				sFilterNum = sFilterNum & " AND Type = '" & sTarget_Type_Base & "'"	
			Else If sDriver_Type = ("Same as target")
				sFilterNum = sFilterNum & " AND Type = '" & sTarget_Type_Base & "'"	
				sFilterDen = sFilterDen & " AND Type = '" & sTarget_Type_Base & "'"
			End If					
						
			Dim oDataDriverNum As Object
			Dim oDataDriverDen As Object									
			
			'Driver 1
			Dim dDataDriver1 As Double = 0		
				
			oDataDriverNum = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriver_Account & "'" & sFilterNum)		
			oDataDriverDen = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriver_Account & "'" & sFilterDen)
							
			If Not oDataDriverDen Is DBNull.Value 
				If Convert.ToDouble(oDataDriverDen) <> 0 					
					If Not oDataDriverNum Is DBNull.Value 
						dDataDriver1 = Convert.ToDouble(oDataDriverNum) / Convert.ToDouble(oDataDriverDen)																		
					End If												
				End If					
			End If	
						
			Return dDataDriver1
			
		End Function	

#End Region

#Region "Otros"
		
		Function GetFilter (ByVal MemberInfoList As List(Of MemberInfo)) As Dictionary(Of Integer, Object)
			
				Dim filters As New Dictionary(Of Integer, Object)
				For Each MemberI As MemberInfo In MemberInfoList
					filters.Add(MemberI.Member.MemberId, MemberI.Member.Name)
				Next
			
			Return filters
			
		End Function	
					
		Sub LogWriteCellData (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal i As Integer, ByVal dbcCell As DataBufferCell)
			
			Brapi.ErrorLog.LogMessage(si, "-----------------------------")
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID Account:" & api.Members.GetMemberName(DimType.Account.Id,dbcCell.DataBufferCellPk.AccountId))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID UD1:" & api.Members.GetMemberName(DimType.UD1.Id,dbcCell.DataBufferCellPk.UD1Id))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID UD2:" & api.Members.GetMemberName(DimType.UD2.Id,dbcCell.DataBufferCellPk.UD2Id))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID UD3:" & api.Members.GetMemberName(DimType.UD3.Id,dbcCell.DataBufferCellPk.UD3Id))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID UD4:" & api.Members.GetMemberName(DimType.UD4.Id,dbcCell.DataBufferCellPk.UD4Id))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID UD5:" & api.Members.GetMemberName(DimType.UD5.Id,dbcCell.DataBufferCellPk.UD5Id))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID UD6:" & api.Members.GetMemberName(DimType.UD6.Id,dbcCell.DataBufferCellPk.UD6Id))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID UD7:" & api.Members.GetMemberName(DimType.UD7.Id,dbcCell.DataBufferCellPk.UD7Id))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID Flow:" & api.Members.GetMemberName(DimType.Flow.Id,dbcCell.DataBufferCellPk.FlowId))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID IC:" & api.Members.GetMemberName(DimType.IC.Id,dbcCell.DataBufferCellPk.ICId))						
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID Origin:" & api.Members.GetMemberName(DimType.Origin.Id,dbcCell.DataBufferCellPk.OriginId))	
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - Value:" & dbcCell.CellAmount.ToString)								
		
		End Sub

#End Region		
		
	End Class
	
End Namespace