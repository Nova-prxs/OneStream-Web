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

Namespace OneStream.BusinessRule.Finance.CONS_Allocation_Data_20221121
	
	Public Class MainClass
		
		Public dtDriver As New DataTable("Driver_Data")
		Public sCalc_E502 As String = String.Empty
		Public sCalc_E502_Parchis As String = String.Empty
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
		
			Try
				
				'Variables Globales
				Dim sScenario As String = api.Pov.Scenario.Name	
				Dim sTime As String = api.Pov.Time.Name			
				Dim sItemID As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("ItemID")				
				Dim sFunction As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("Function")
				
'				Brapi.ErrorLog.LogMessage(si, "Scenario:" & sScenario)
'				Brapi.ErrorLog.LogMessage(si, "Time:" & sTime)
				
				If (Not api.Entity.HasChildren) And api.Cons.IsLocalCurrencyForEntity() Then

					Dim sEntity As String = api.Pov.Entity.Name
					'Brapi.ErrorLog.LogMessage(si, "Entity: " & sEntity)
				
					'Obtenemos el escenario y periodo para obtener los drivers de la E502 si corresponde
					If sEntity.Equals("E502")
						Me.GetCalcE502(si, api, sScenario)
					End If
					
					'BORRAR Repartos TODAS Reglas
					If sFunction = "Clear_Allocation_Rules_All" Then
						
						Me.ClearAllocationRulesAll(si, api, globals, args, sTime, sScenario)
						
					'BORRAR Repartos UNA Regla
					Else If sFunction = "Clear_Allocation_Rules_One" Then
																		
						Me.ClearAllocationRulesOne(si, api, globals, args, sTime, sScenario, sItemID)		
					
					'EJECUTAR Repatos TODAS Reglas
					Else If sFunction = "Execute_Allocation_Rules_All"		

						Me.ClearAllocationRulesAll(si, api, globals, args, sTime, sScenario)
						Me.ExecuteAllocationRulesAll(si, api, globals, args, sTime, sScenario)	
					
					'EJECUTAR Repatos UNA Regla
					Else If sFunction = "Execute_Allocation_Rules_One"		
					
						Me.ClearAllocationRulesOne(si, api, globals, args, sTime, sScenario, sItemID)
						Me.ExecuteAllocationRulesOne(si, api, globals, args, sTime, sScenario, sItemID)	
						
					End If							
				End If
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
'---------------------------------------------------------------------------------------------------------------------------------		
		
		Sub ClearAllocationRulesAll (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sTime As String, sScenario As String)
		
			'(clearCalculatedData, clearTranslatedData, clearConsolidatedData, clearDurableCalculatedData,A#,F#,O#,I#,UD1#,UD2#,UD3#,UD4#,UD5#,UD6#,UD7#,UD8#)
			api.Data.ClearCalculatedData(True, True, True, True,,,,,,,,,,"U6#Net_Change.Base",, )	
	
		End Sub	

		Sub ClearAllocationRulesOne (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sTime As String, sScenario As String, sItemID As String)
		
			Dim sRuleName As String = String.Empty				
		
			Dim sbSQL As New Text.StringBuilder()
						
			sbSQL.Append(" SELECT RuleName")			
			sbSQL.Append(" FROM Allocation_Rules")																				
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

		Sub ExecuteAllocationRulesAll (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sTime As String, sScenario As String)		

			Dim sbSQL As New Text.StringBuilder()
						
			sbSQL.Append(" SELECT RuleName")
			
			sbSQL.Append(" , sUD1")
			sbSQL.Append(" , sUD3")
			sbSQL.Append(" , sAccount")	
			sbSQL.Append(" , sEntity")
			sbSQL.Append(" , sSite")
			
			sbSQL.Append(" , tUD1")
			sbSQL.Append(" , tUD3")	
			sbSQL.Append(" , tAccount")	
			sbSQL.Append(" , tAccountOut")
			
			sbSQL.Append(" , dUD1")
			sbSQL.Append(" , dUD3")
			
			sbSQL.Append(" , dAccount1")			
			sbSQL.Append(" , dAccount1Pond")	
			sbSQL.Append(" , dSite1")			
			
			sbSQL.Append(" , dAccount2")			
			sbSQL.Append(" , dAccount2Pond")
			sbSQL.Append(" , dSite2")			
				
			sbSQL.Append(" , dAccount3")			
			sbSQL.Append(" , dAccount3Pond")
			sbSQL.Append(" , dSite3")			
				
			sbSQL.Append(" , dAccount4")			
			sbSQL.Append(" , dAccount4Pond")
			sbSQL.Append(" , dSite4")			
				
			sbSQL.Append(" , dAccount5")			
			sbSQL.Append(" , dAccount5Pond")			
			sbSQL.Append(" , dSite5")			
				
			sbSQL.Append(" FROM Allocation_Rules")																	
			
			sbSQL.Append(" WHERE POVScenario = '" & sScenario & "'")
			sbSQL.Append(" AND POVTime = '" & sTime & "'")
			sbSQL.Append(" AND RuleEnabled = 1")
			
			sbSQL.Append(" ORDER BY RuleSequence")
							
			'Brapi.ErrorLog.LogMessage(si, sbSQL.ToString)
			
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)											
						
					'Calculamos cuentas que forman parte de los drivers de EBITDA (CR_P) o MARGEN BRUTO (CR_I)
					Calc_CR_Accounts(si, api, globals, args)
					
					For Each dr As DataRow In dt.Rows
						
						'Ejecución de repartos
						Me.CalculateAllocationAllRules(si, api, dr, sScenario)	
						
						'Calculamos cuentas que forman parte de los drivers de EBITDA (CR_P) o MARGEN BRUTO (CR_I)
						Calc_CR_Accounts(si, api, globals, args)
					Next
					
				End Using
			End Using
					
			
		End Sub	

		Sub ExecuteAllocationRulesOne (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sTime As String, sScenario As String, sItemID As String)				
		
			Dim sbSQL As New Text.StringBuilder()
						
			sbSQL.Append(" SELECT RuleName")
			
			sbSQL.Append(" , sUD1")
			sbSQL.Append(" , sUD3")
			sbSQL.Append(" , sAccount")	
			sbSQL.Append(" , sEntity")
			sbSQL.Append(" , sSite")
			
			sbSQL.Append(" , tUD1")
			sbSQL.Append(" , tUD3")	
			sbSQL.Append(" , tAccount")	
			sbSQL.Append(" , tAccountOut")
			
			sbSQL.Append(" , dUD1")
			sbSQL.Append(" , dUD3")
			
			sbSQL.Append(" , dAccount1")			
			sbSQL.Append(" , dAccount1Pond")	
			sbSQL.Append(" , dSite1")			
			
			sbSQL.Append(" , dAccount2")			
			sbSQL.Append(" , dAccount2Pond")
			sbSQL.Append(" , dSite2")			
				
			sbSQL.Append(" , dAccount3")			
			sbSQL.Append(" , dAccount3Pond")
			sbSQL.Append(" , dSite3")			
				
			sbSQL.Append(" , dAccount4")			
			sbSQL.Append(" , dAccount4Pond")
			sbSQL.Append(" , dSite4")			
				
			sbSQL.Append(" , dAccount5")			
			sbSQL.Append(" , dAccount5Pond")			
			sbSQL.Append(" , dSite5")		
			
			sbSQL.Append(" FROM Allocation_Rules")																	
			
			sbSQL.Append(" WHERE POVScenario = '" & sScenario & "'")
			sbSQL.Append(" AND POVTime = '" & sTime & "'")
			sbSQL.Append(" AND RuleEnabled = 1")
			sbSQL.Append(" AND ItemID = '" & sItemID & "'")
			
			sbSQL.Append(" ORDER BY RuleSequence")
							
			'Brapi.ErrorLog.LogMessage(si, sbSQL.ToString)
			
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)											
					
					'Calculamos cuentas que forman parte de los drivers de EBITDA (CR_P) o MARGEN BRUTO (CR_I)
					Calc_CR_Accounts(si, api, globals, args)
					
					For Each dr As DataRow In dt.Rows
						
						'Ejecución de repartos
						Me.CalculateAllocationAllRules(si, api, dr, sScenario)	
						
						'Calculamos cuentas que forman parte de los drivers de EBITDA (CR_P) o MARGEN BRUTO (CR_I)
						Calc_CR_Accounts(si, api, globals, args)
					Next
					
				End Using
			End Using
								
		End Sub	
		
		Sub CalculateAllocationAllRules (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal dr As DataRow, ByVal sScenario As String)		

			Dim sRuleName As String = String.Empty
			
			Dim sSourceUD1 As String = String.Empty
			Dim sSourceUD3 As String = String.Empty
			Dim sSourceAccount As String = String.Empty	
			Dim sSourceEntity As String = String.Empty
			Dim sSourceSite As String = String.Empty			
			
			Dim sTargetUD1 As String = String.Empty
			Dim sTargetUD3 As String = String.Empty
			Dim sTargetAccount As String = String.Empty	
			Dim sTargetAccountOut As String = String.Empty	
			
			Dim sDriverUD1 As String = String.Empty
			Dim sDriverUD3 As String = String.Empty
			
			Dim sDriverAccount1, sDriverAccount2, sDriverAccount3, sDriverAccount4, sDriverAccount5 As String
			Dim sDriverAccount1Pond, sDriverAccount2Pond, sDriverAccount3Pond, sDriverAccount4Pond, sDriverAccount5Pond As String		
			Dim sDriverSite1, sDriverSite2, sDriverSite3, sDriverSite4, sDriverSite5 As String
			
			sRuleName =  dr(0).ToString
			
			'Origen
				sSourceUD1 =  			dr(1).ToString
				sSourceUD3 =  			dr(2).ToString	
				sSourceAccount =  		dr(3).ToString	
				sSourceEntity =  		dr(4).ToString
				sSourceSite =  			dr(5).ToString	
				
			'Destino
				sTargetUD1 =	 		dr(6).ToString
				sTargetUD3 = 			dr(7).ToString	
				sTargetAccount =  		dr(8).ToString
				sTargetAccountOut =  	dr(9).ToString	
				
			'Driver
				sDriverUD1 = 			dr(10).ToString
				sDriverUD3 = 			dr(11).ToString	
				
				sDriverAccount1 = 		dr(12).ToString		
				sDriverAccount1Pond = 	dr(13).ToString		
				sDriverSite1 = 			dr(14).ToString	
				
				sDriverAccount2 = 		dr(15).ToString		
				sDriverAccount2Pond = 	dr(16).ToString	
				sDriverSite2 = 			dr(17).ToString	
				
				sDriverAccount3 = 		dr(18).ToString		
				sDriverAccount3Pond = 	dr(19).ToString	
				sDriverSite3 = 			dr(20).ToString
				
				sDriverAccount4 = 		dr(21).ToString		
				sDriverAccount4Pond = 	dr(22).ToString	
				sDriverSite4 = 			dr(23).ToString					
				
				sDriverAccount5 = 		dr(24).ToString		
				sDriverAccount5Pond = 	dr(25).ToString	
				sDriverSite5 = 			dr(26).ToString	

			'Obtenemos miembros ORIGEN a recorrer			
			Dim sSourceAccount_List As List(Of MemberInfo) = GetMemberBaseAccount(si, sSourceAccount, "A#", "CONS_Accounts")					
			
			Dim sSourceUD1_List As List(Of MemberInfo) = GetMemberBaseSource(si, sSourceUD1, "U1#", "CONS_BusinessUnits")
			Dim sSourceUD3_List As List(Of MemberInfo) = GetMemberBaseSource(si, sSourceUD3, "U3#", "CONS_CostCenters")	
			Dim sSourceEntity_List As List(Of MemberInfo) = GetMemberBaseSource(si, sSourceEntity, "E#", "Entities")
			Dim sSourceSite_List As List(Of MemberInfo) = GetMemberBaseSource(si, sSourceSite, "U7#", "CONS_Sites")			
			
			'Obtenemos miembros TARGET a recorrer
			Dim sTargetAccount_List As List(Of MemberInfo)
			Dim sTargetUD1_List As List(Of MemberInfo)
			Dim sTargetUD3_List As List(Of MemberInfo)
			
			If sTargetAccount = "Same as source" Then
				sTargetAccount = sSourceAccount
				sTargetAccount_List = sSourceAccount_List
			Else
				sTargetAccount_List = GetMemberBaseAccount(si, sTargetAccount, "A#", "CONS_Accounts")		
			End If
			
			If sTargetUD1 = "Same as source" Then
				sTargetUD1 = sSourceUD1
				sTargetUD1_List = sSourceUD1_List
			Else
				sTargetUD1_List = GetMemberBaseTarget(si, sTargetUD1, "U1#", "CONS_BusinessUnits", sSourceUD1_List)
			End If
			
			If sTargetUD3 = "Same as source" Then
				sTargetUD3 = sSourceUD3
				sTargetUD3_List = sSourceUD3_List
			Else
				sTargetUD3_List = GetMemberBaseTarget(si, sTargetUD3, "U3#", "CONS_CostCenters", sSourceUD3_List)
			End If				
			
			'Obtenemos miembros TARGET para repartos de segmentos
			Dim sTargetUD1_List_SM99 As List(Of MemberInfo) = GetMemberBaseTargetSegment(si, "DN_EMP", "U1#", "CONS_BusinessUnits", sSourceUD1_List)
			Dim sTargetUD1_List_SC99 As List(Of MemberInfo) = GetMemberBaseTargetSegment(si, "DN_PEC", "U1#", "CONS_BusinessUnits", sSourceUD1_List)
			Dim sTargetUD1_List_SO99 As List(Of MemberInfo) = GetMemberBaseTargetSegment(si, "DN_SER", "U1#", "CONS_BusinessUnits", sSourceUD1_List)
			
			'Entidad que estamos recorriendo
			Dim sEntity_MemberInfo As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & api.Pov.Entity.Name, True)									
			Dim sEntity = sEntity_MemberInfo(0).Member.Name			
			
			If (sSourceEntity_List.Contains(sEntity_MemberInfo(0)))						
				
				Dim diDestinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
				Dim dbSourceDataBuffer As DataBuffer
				dbSourceDataBuffer = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, "U5#Top:U6#Top", diDestinationInfo)
				
				Dim sSourceUD4_List As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Natures", "U4#Gestion.Base", True)
								
				Dim filtersAccount As Dictionary(Of Integer, Object) = GetFilter(sSourceAccount_List)
				Dim filtersUD1 As Dictionary(Of Integer, Object) = GetFilter(sSourceUD1_List)
				Dim filtersUD3 As Dictionary(Of Integer, Object) = GetFilter(sSourceUD3_List)
				Dim filtersUD4 As Dictionary(Of Integer, Object) = GetFilter(sSourceUD4_List)				
				Dim filtersSite As Dictionary(Of Integer, Object) = GetFilter(sSourceSite_List)

				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.Account, filtersAccount)
				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.UD1, filtersUD1)
				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.UD3, filtersUD3)
				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.UD4, filtersUd4)
				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.UD7, filtersSite)
				
				Dim sDriverScenario = GetScenarioDriver(api, sScenario)
				
				Me.GetDriverDataTable(si, api, sDriverScenario, sEntity _
									, sDriverUD1, sTargetUD1_List, sDriverUD3, sTargetUD3_List _
									, sDriverAccount1, sDriverAccount2, sDriverAccount3, sDriverAccount4, sDriverAccount5 _	
									, sDriverSite1, sDriverSite2, sDriverSite3, sDriverSite4, sDriverSite5)
									
				If Not dbSourceDataBuffer Is Nothing Then
									
					Dim dbAllocationIn As DataBuffer = New DataBuffer()
					Dim dbAllocationOut As DataBuffer = New DataBuffer()					
					
					Dim i As Integer = 0					
					For Each dbcSourceCell As DataBufferCell In dbSourceDataBuffer.DataBufferCells.Values						
						
						'LogWriteCellData(si, api, 0, dbcSourceCell)
						
						If (Not dbcSourceCell.CellStatus.IsNoData And dbcSourceCell.CellAmount <> 0)	
						
							i = i + 1
							'LogWriteCellData(si, api, i, dbcSourceCell)
																			
							If(sTargetAccount = "Same as source" Or sTargetAccount = sSourceAccount)
								sTargetAccount_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Accounts", "A#" & api.Members.GetMember(DimType.Account.Id, dbcSourceCell.DataBufferCellPk.AccountId).Name, True)	
							End If
							
							Dim sSourceUD1Name As String = api.Members.GetMember(DimType.UD1.Id, dbcSourceCell.DataBufferCellPk.UD1Id).Name
							Dim sSegment As String = String.Empty
							
							If sSourceUD1Name = "SM99"
								sTargetUD1_List = sTargetUD1_List_SM99
								sSegment = "DN_EMP"
							Else If sSourceUD1Name = "SC99"
								sTargetUD1_List = sTargetUD1_List_SC99
								sSegment = "DN_PEC"
							Else If sSourceUD1Name = "SO99"
								sTargetUD1_List = sTargetUD1_List_SO99										
								sSegment = "DN_SER"
							Else If(sTargetUD1 = "Same as source" Or sTargetUD1 = sSourceUD1)
								sTargetUD1_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_BusinessUnits", "U1#" & api.Members.GetMember(DimType.UD1.Id, dbcSourceCell.DataBufferCellPk.UD1Id).Name, True)	
							End If
							
							If(sTargetUD3 = "Same as source" Or sTargetUD3 = sSourceUD3)
								sTargetUD3_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_CostCenters", "U3#" & api.Members.GetMember(DimType.UD3.Id, dbcSourceCell.DataBufferCellPk.UD3Id).Name, True)	
							End If	
							
							Dim bAllocationIn As Boolean = False
							
							For Each sTargetAccount_Member As MemberInfo In sTargetAccount_List
								For Each sTargetUD1_Member As MemberInfo In sTargetUD1_List
									For Each sTargetUD3_Member As MemberInfo In sTargetUD3_List																		
									
										Dim sSite As String = BRApi.Finance.Metadata.GetMember(si, DimType.UD7.Id,	dbcSourceCell.DataBufferCellPk.UD7Id).Member.Name
										
										Dim dDataDriver As Double = GetDriverData(si, api, sEntity, sSite _ 											
											, sDriverUD1, sTargetUD1_Member.Member.Name, sSegment _
											, sDriverUD3, sTargetUD3_Member.Member.Name _
											, sDriverAccount1, sDriverAccount2, sDriverAccount3, sDriverAccount4, sDriverAccount5 _
											, sDriverAccount1Pond, sDriverAccount2Pond, sDriverAccount3Pond, sDriverAccount4Pond, sDriverAccount5Pond _
											, sDriverSite1, sDriverSite2, sDriverSite3, sDriverSite4, sDriverSite5)
										
										If dDataDriver <> 0											
											
											Dim dbcAllocationInCell As New DataBufferCell(dbcSourceCell)
											
											'MODIFICACIÓN registros de Allocation In
											dbcAllocationInCell.DataBufferCellPk.UD5Id = api.Members.GetMemberId(DimType.UD5.Id,sRuleName)
											dbcAllocationInCell.DataBufferCellPk.UD6Id = api.Members.GetMemberId(DimType.UD6.Id,"Allocation_In")
											
											dbcAllocationInCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account.Id,sTargetAccount_Member.Member.Name)
											dbcAllocationInCell.DataBufferCellPk.UD1Id = api.Members.GetMemberId(DimType.UD1.Id,sTargetUD1_Member.Member.Name)
											dbcAllocationInCell.DataBufferCellPk.UD3Id = api.Members.GetMemberId(DimType.UD3.Id,sTargetUD3_Member.Member.Name)
												
											'ICP (Si tiene ICP y estamos distribuyendo por UD1)
											Dim sIc As String = BRApi.Finance.Metadata.GetMember(si, DimType.IC.Id,	dbcAllocationInCell.DataBufferCellPk.IcId).Member.Name
											If Not sIc = "None" And sDriverUD1 = "Allocation"
												dbcAllocationInCell.DataBufferCellPk.IcId = api.Members.GetMemberId(DimType.Ic.Id,"E997")
											End If
											
											'UD2 (Si estamos distribuyendo por UD1 y en función de la ICP)
'											17/10/11 - Comentamos el cálculo de UD2, se mantiene la UD2 de origen
											If (sDriverUD1 = "Allocation")	
												Dim sUD1Name As String = String.Empty
												If sIc = "None"
													sUD1Name = api.Members.GetMember(DimType.UD1.Id, dbcAllocationInCell.DataBufferCellPk.UD1Id).Name
													dbcAllocationInCell.DataBufferCellPk.UD2Id = api.Members.GetMemberId(DimType.UD2.Id, sUD1Name)
												End If
																																			
											End If																					
											
											'DATO = DATO ORIGEN * PESO
											dbcAllocationInCell.CellAmount = dbcAllocationInCell.CellAmount * dDataDriver.ToString											
											
'											If sEntity = "E122"
'												LogWriteCellData(si, api, i, dbcAllocationInCell)
'											End If
											
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
								
								Dim sIc As String = BRApi.Finance.Metadata.GetMember(si, DimType.IC.Id,	dbcAllocationOutCell.DataBufferCellPk.IcId).Member.Name
								If Not sIc = "None" And sDriverUD1 = "Allocation"
									dbcAllocationOutCell.DataBufferCellPk.IcId = api.Members.GetMemberId(DimType.Ic.Id,"E997")
								End If
								
								If sTargetUD1.StartsWith("Elim_DN")
									dbcAllocationOutCell.DataBufferCellPk.UD1Id = api.Members.GetMemberId(DimType.UD1.Id,"Elim_DN")
								End If
								
								dbcAllocationOutCell.DataBufferCellPk.UD5Id = api.Members.GetMemberId(DimType.UD5.Id,sRuleName)
								dbcAllocationOutCell.DataBufferCellPk.UD6Id = api.Members.GetMemberId(DimType.UD6.Id,"Allocation_OUT")						
								If sTargetAccountOut <> "Same as source"
									dbcAllocationOutCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account.Id,sTargetAccountOut)
								End If
								dbcAllocationOutCell.CellAmount = dbcAllocationOutCell.CellAmount * -1
								
'								If(sEntity = "E122")
'									LogWriteCellData(si, api, i, dbcAllocationOutCell)
'								End If	
								
								'GUARDAMOS el registro de ALLOCATION OUT en el BUFFER
								dbAllocationOut.SetCell(si, dbcAllocationOutCell,True)
							End If
						End If
					Next
	
'					If(sEntity = "E122")
'						Brapi.ErrorLog.LogMessage(si, "Previo Allocation In")
'					End If					
					
					'ENVIAMOS los buffer de ALLOCATION IN y ALLOCATION OUT para guardar los cambios
					api.Data.SetDataBuffer(dbAllocationIn, diDestinationInfo,,,,,,,,,,,,,True)					
					api.Data.SetDataBuffer(dbAllocationOut, diDestinationInfo,,,,,,,,,,,,,True)
					
					'Cálculo de CR_Q3
					If sRuleName = "Rule_001"
						Calc_CR_Q3(si, api)
					End If
					
				End If				
				
			End If
		
		End Sub		

		Sub GetCalcE502 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal sScenario As String)		
			
			If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then			
	
				Dim iMonth As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Month
				Dim iYear As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Year
				
				Dim sScenario_E502 As String = String.Empty
				Dim sPeriod_E502 As String = String.Empty
				
				sPeriod_E502 = iYear.ToString() & "M12"
				
				Select Case sScenario
					
					Case "R"
				
						Select Case iMonth
							
							Case "1", "2", "3", "4", "5", "6"
								sScenario_E502 = "O1"
								
							Case "7", "8", "9", "10", "11"
								sScenario_E502 = "P06"
								
							Case "12"
								sScenario_E502 = "P11"
								
						End Select
				
					Case "P03"
						sScenario_E502 = "O1"
					
					Case "P06"
						sScenario_E502 = "P03"
						
					Case "P11"
						sScenario_E502 = "P09"
				
					Case "O1"
						sScenario_E502 = "P09"					
						sPeriod_E502 = (iYear - 1).ToString() & "M12"
						
				End Select		
				
				sCalc_E502 = ":S#" & sScenario_E502 & ":T#" & sPeriod_E502 & ":E#IPT"
				sCalc_E502_Parchis = ":S#" & sScenario_E502 & ":T#" & sPeriod_E502
				'Brapi.ErrorLog.LogMessage(si, "Filter E502: " & sCalc_E502)
				
			End If
		
		End Sub		
					
		Function GetMemberBaseSource (ByVal si As SessionInfo, ByVal sMemberSource As String, ByVal sTag As String, ByVal sDimension As String) As List(Of MemberInfo)
			
			Dim MemberList As List(Of MemberInfo) = New List(Of MemberInfo)
			
			If(sMemberSource.Contains(","))
				For Each sMember As String In sMemberSource.Split(",")
					For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMember.Trim & ".Base", True)
						'Brapi.ErrorLog.LogMessage(si, "Member: " & miMember.Member.Name)
						
						If Not MemberList.Contains(miMember)
							MemberList.Add(miMember)
						End If
					Next
				Next
				
			Else
				MemberList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMemberSource & ".Base", True)
				
			End If					
			
			Return MemberList
			
		End Function

		Function GetMemberBaseAccount (ByVal si As SessionInfo, ByVal sMemberSource As String, ByVal sTag As String, ByVal sDimension As String) As List(Of MemberInfo)
			
			Dim MemberList As List(Of MemberInfo) = New List(Of MemberInfo)
			Dim MemberListAccNoDist As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Accounts", "A#CR_J1,A#CR_J2,A#CR_J3,A#CR_J4,A#CR_G1,A#CR_G2,A#CR_G3", True)
			
			If(sMemberSource.Contains(","))
				For Each sMember As String In sMemberSource.Split(",")
					For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMember.Trim & ".Base", True)
						'Brapi.ErrorLog.LogMessage(si, "Member: " & miMember.Member.Name)
						
						If Not MemberList.Contains(miMember) And Not MemberListAccNoDist.Contains(miMember)
							MemberList.Add(miMember)
						End If
					Next
				Next
				
			Else
				MemberList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMemberSource & ".Base", True)
				
			End If					
			
			Return MemberList
			
		End Function		
		
		Function GetMemberBaseTarget (ByVal si As SessionInfo, ByVal sMemberTarget As String, ByVal sTag As String, ByVal sDimension As String, ByVal sSource_List As List(Of MemberInfo)) As List(Of MemberInfo)
			
			Dim MemberList As List(Of MemberInfo) = New List(Of MemberInfo)
			
			If(sMemberTarget.Contains(","))
				For Each sMember As String In sMemberTarget.Split(",")
					If sMember.StartsWith("Elim")
						MemberList.Add(BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMember, True)(0))
					Else
						For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMember.Trim & ".Base", True)									
							
							If Not miMember.Member.Name.StartsWith("Elim") And Not sSource_List.Contains(miMember)
								MemberList.Add(miMember)
							End If
						Next
					End If
				Next
				
			Else

				For Each miMember As MemberInfo In  BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMemberTarget & ".Base", True)
					
					If Not miMember.Member.Name.StartsWith("Elim") And Not sSource_List.Contains(miMember)
						MemberList.Add(miMember)
					End If
				Next				
'				If sMemberTarget = "DN_PEC"
'					For Each sMember As MemberInfo In MemberList
'						Brapi.ErrorLog.LogMessage(si, "Member: " & sMember.Member.Name)
'					Next
'				End If
				
			End If		
			
			Return MemberList
			
		End Function						
		
		Function GetMemberBaseTargetSegment (ByVal si As SessionInfo, ByVal sMemberTarget As String, ByVal sTag As String, ByVal sDimension As String, ByVal sSource_List As List(Of MemberInfo)) As List(Of MemberInfo)
			
			Dim MemberList As List(Of MemberInfo) = New List(Of MemberInfo)
			
			If(sMemberTarget.Contains(","))
				For Each sMember As String In sMemberTarget.Split(",")
					
					If (sMember.Trim().StartsWith("Elim_DN"))
						MemberList.Add(BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMember.Trim, True)(0))
					Else
						For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMember.Trim & ".Base", True)
							
							If Not miMember.Member.Name.StartsWith("Elim") And Not sSource_List.Contains(miMember)
								MemberList.Add(miMember)
								'Brapi.ErrorLog.LogMessage(si, "Member: " & miMember.Member.Name)
							End If
						Next
					End If
				Next
				
			Else
				For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMemberTarget & ".Base", True)					
					If Not miMember.Member.Name.StartsWith("Elim") And Not miMember.Member.Name.EndsWith("99")
						MemberList.Add(miMember)
						'Brapi.ErrorLog.LogMessage(si, "Member: " & miMember.Member.Name)
					End If
				Next				
			End If		
			
			Return MemberList
			
		End Function				
		
		Function GetMemberBaseTargetEntity (ByVal si As SessionInfo, ByVal sMemberTarget As String, ByVal sMemberSource As String, ByVal sTag As String, ByVal sDimension As String) As List(Of MemberInfo)
			
			Dim MemberList As List(Of MemberInfo)
			
			If(sMemberTarget.Contains(","))
				MemberList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMemberTarget, True)
			Else
				MemberList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMemberTarget & ".Base", True)
			End If		
			
			Return MemberList
			
		End Function		

		Function GetFilter (ByVal MemberInfoList As List(Of MemberInfo)) As Dictionary(Of Integer, Object)
			
				Dim filters As New Dictionary(Of Integer, Object)
				For Each MemberI As MemberInfo In MemberInfoList
					filters.Add(MemberI.Member.MemberId, MemberI.Member.Name)
				Next
			
			Return filters
			
		End Function	
	
		Sub GetDriverDataTable(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal sDriverScenario As String, ByVal sDriverEntity As String, ByVal sDriverUD1 As String, ByVal sTargetUD1_List As List(Of MemberInfo), ByVal sDriverUD3 As String, ByVal sTargetUD3_List As List(Of MemberInfo), ByVal sDriverAccount1 As String, ByVal sDriverAccount2 As String, ByVal sDriverAccount3 As String, ByVal sDriverAccount4 As String, ByVal sDriverAccount5 As String, ByVal sDriverSite1 As String, ByVal sDriverSite2 As String, ByVal sDriverSite3 As String, ByVal sDriverSite4 As String, ByVal sDriverSite5 As String)	
		
			dtDriver = New DataTable("Driver_Data")
		
			Dim objCol = New DataColumn
            objCol.ColumnName = "UD1"
            objCol.DataType = GetType(String)
            objCol.DefaultValue = ""
            objCol.AllowDBNull = False
			dtDriver.Columns.Add(objCol)
			
			objCol = New DataColumn
            objCol.ColumnName = "Segment"
            objCol.DataType = GetType(String)
            objCol.DefaultValue = ""
            objCol.AllowDBNull = False
			dtDriver.Columns.Add(objCol)			
			
			objCol = New DataColumn
            objCol.ColumnName = "UD3"
            objCol.DataType = GetType(String)
            objCol.DefaultValue = ""
            objCol.AllowDBNull = False
			dtDriver.Columns.Add(objCol)
			
			objCol = New DataColumn
            objCol.ColumnName = "Account"
            objCol.DataType = GetType(String)
            objCol.DefaultValue = ""
            objCol.AllowDBNull = False
			dtDriver.Columns.Add(objCol)	
			
			objCol = New DataColumn
            objCol.ColumnName = "UD7"
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
					
			'Segmentos
			Dim sUD1_DN_EMP_List As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_BusinessUnits", "U1#DN_EMP.Base", True)
			Dim sUD1_DN_PEC_List As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_BusinessUnits", "U1#DN_PEC.Base", True)
			Dim sUD1_DN_SER_List As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_BusinessUnits", "U1#DN_SER.Base", True)
			
			If Not sDriverUD1 = "Allocation" And Not sDriverUD1 = "Same as target"
				sTargetUD1_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_BusinessUnits", "U1#" & sDriverUD1, True)
			End If		
			
			If Not sDriverUD3 = "Allocation" And Not sDriverUD3 = "Same as target"
				sTargetUD3_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_CostCenters", "U3#" & sDriverUD3, True)
			End If		
		
			'Sites
			Dim sSites_List1 As List(Of MemberInfo) = GetDriverSite(si, sDriverSite1)
			Dim sSites_List2 As List(Of MemberInfo) = GetDriverSite(si, sDriverSite2)
			Dim sSites_List3 As List(Of MemberInfo) = GetDriverSite(si, sDriverSite3)
			Dim sSites_List4 As List(Of MemberInfo) = GetDriverSite(si, sDriverSite4)
			Dim sSites_List5 As List(Of MemberInfo) = GetDriverSite(si, sDriverSite5)
			
			Dim sDriver1Cube As String = GetCube(sDriverAccount1)	
			Dim sDriver2Cube As String = GetCube(sDriverAccount2)
			Dim sDriver3Cube As String = GetCube(sDriverAccount3)
			Dim sDriver4Cube As String = GetCube(sDriverAccount4)
			Dim sDriver5Cube As String = GetCube(sDriverAccount5)
			
			Dim sSegment As String = String.Empty
			
			'brapi.ErrorLog.LogMessage(si, "2")								
			For Each sTargetUD1_Member As MemberInfo In sTargetUD1_List							
				
				Dim sEntity As String = api.Pov.Entity.Name
				
'				If sEntity.Equals("E122")
'					brapi.ErrorLog.LogMessage(si, "UD1: " & sTargetUD1_Member.Member.Name)	
'				End If
				
				If sUD1_DN_EMP_List.Contains(sTargetUD1_Member)
					sSegment = "DN_EMP"
				ElseIf sUD1_DN_PEC_List.Contains(sTargetUD1_Member)
					sSegment = "DN_PEC"
				ElseIf sUD1_DN_SER_List.Contains(sTargetUD1_Member)
					sSegment = "DN_SER"
				End If
				
				'brapi.ErrorLog.LogMessage(si, "UD1: " & sTargetUD1_Member.Member.Name & " Segmento: " & sSegment)	
				
				For Each sTargetUD3_Member As MemberInfo In sTargetUD3_List
						
					Dim sCalc As String = String.Empty
					Dim dData As Decimal = 0
					Dim row As DataRow
					
					For Each sSite_Member As MemberInfo In sSites_List1
						If sDriverAccount1 <> String.Empty
							'brapi.ErrorLog.LogMessage(si, "1")											
							row = dtDriver.NewRow									
							row("Account") = sDriverAccount1
							row("Segment") = sSegment
							row("UD1") = sTargetUD1_Member.Member.Name												
							row("UD3") = sTargetUD3_Member.Member.Name																							
							row("UD7") = sSite_Member.Member.Name
							
							sCalc = GetDriverCalc(api, sDriverAccount1, sDriverScenario, row("UD1"), row("UD3"), row("UD7"))	
							
							'brapi.ErrorLog.LogMessage(si, sCalc)	
							
							dData = api.Data.GetDataCell(sCalc).CellAmount										
							If (Not sDriverAccount1.Equals("CR_P") And dData <> 0) _ 
							Or (sDriverAccount1.Equals("CR_P") And dData < 0)
								row("Amount") = dData
								dtDriver.Rows.Add(row.ItemArray)						
							End If	
							
'							If sEntity.Equals("E502") And sTargetUD1_Member.Member.Name.Equals("NV05")
'								brapi.ErrorLog.LogMessage(si, "calc: " & sCalc)	
'								brapi.ErrorLog.LogMessage(si, "data: " & dData.ToString())									
'							End If
							
						End If
					Next
					
'					brapi.ErrorLog.LogMessage(si, "Dato: " & dData.ToString)		
					
					For Each sSite_Member As MemberInfo In sSites_List2
						If sDriverAccount2 <> String.Empty
							row = dtDriver.NewRow									
							row("Account") = sDriverAccount2
							row("Segment") = sSegment
							row("UD1") = sTargetUD1_Member.Member.Name												
							row("UD3") = sTargetUD3_Member.Member.Name																							
							row("UD7") = sSite_Member.Member.Name
							
							sCalc = GetDriverCalc(api, sDriverAccount2, sDriverScenario, row("UD1"), row("UD3"), row("UD7"))		
							
							dData = api.Data.GetDataCell(sCalc).CellAmount
							If (Not sDriverAccount2.Equals("CR_P") And dData <> 0) _ 
							Or (sDriverAccount2.Equals("CR_P") And dData < 0)							
								row("Amount") = dData
								dtDriver.Rows.Add(row.ItemArray)
								
								'If sEntity.Equals("E122")
									'Brapi.ErrorLog.LogMessage(si, "Calc: " & sCalc.ToString)
									'Brapi.ErrorLog.LogMessage(si, "UD1: " & sTargetUD1_Member.Member.Name	)
									'Brapi.ErrorLog.LogMessage(si, "Data: " & dData.ToString)
									
								'End If									
							End If
														
						End If
					Next
					
					'brapi.ErrorLog.LogMessage(si, "3")	
					For Each sSite_Member As MemberInfo In sSites_List3
						If sDriverAccount3 <> String.Empty
							row = dtDriver.NewRow									
							row("Account") = sDriverAccount3
							row("Segment") = sSegment
							row("UD1") = sTargetUD1_Member.Member.Name												
							row("UD3") = sTargetUD3_Member.Member.Name																							
							row("UD7") = sSite_Member.Member.Name
							
							sCalc = GetDriverCalc(api, sDriverAccount3, sDriverScenario, row("UD1"), row("UD3"), row("UD7"))	
													
							dData = api.Data.GetDataCell(sCalc).CellAmount
							If (Not sDriverAccount3.Equals("CR_P") And dData <> 0) _ 
							Or (sDriverAccount3.Equals("CR_P") And dData < 0)
								row("Amount") = dData
								dtDriver.Rows.Add(row.ItemArray)								
							End If
						End If
					Next
					
					'brapi.ErrorLog.LogMessage(si, "4")	
					For Each sSite_Member As MemberInfo In sSites_List4
						If sDriverAccount4 <> String.Empty
							row = dtDriver.NewRow									
							row("Account") = sDriverAccount4
							row("Segment") = sSegment
							row("UD1") = sTargetUD1_Member.Member.Name												
							row("UD3") = sTargetUD3_Member.Member.Name																							
							row("UD7") = sSite_Member.Member.Name
							
							sCalc = GetDriverCalc(api, sDriverAccount4, sDriverScenario, row("UD1"), row("UD3"), row("UD7"))	
													
							dData = api.Data.GetDataCell(sCalc).CellAmount
							If (Not sDriverAccount4.Equals("CR_P") And dData <> 0) _ 
							Or (sDriverAccount4.Equals("CR_P") And dData < 0)
								row("Amount") = dData
								dtDriver.Rows.Add(row.ItemArray)
							End If
						End If
					Next
					
					'brapi.ErrorLog.LogMessage(si, "5")	
					For Each sSite_Member As MemberInfo In sSites_List5
						If sDriverAccount5 <> String.Empty
							row = dtDriver.NewRow
							row("Account") = sDriverAccount5
							row("Segment") = sSegment
							row("UD1") = sTargetUD1_Member.Member.Name												
							row("UD3") = sTargetUD3_Member.Member.Name																							
							row("UD7") = sSite_Member.Member.Name
							
							sCalc = GetDriverCalc(api, sDriverAccount5, sDriverScenario, row("UD1"), row("UD3"), row("UD7"))
												
							dData = api.Data.GetDataCell(sCalc).CellAmount
							If (Not sDriverAccount5.Equals("CR_P") And dData <> 0) _ 
							Or (sDriverAccount5.Equals("CR_P") And dData < 0)
								row("Amount") = dData
								dtDriver.Rows.Add(row.ItemArray)
							End If					
						End If									
					Next
				Next
			Next
			
			'brapi.ErrorLog.LogMessage(si, "6")	
			
			'Dim objeto As Object = dt.Compute("SUM(Amount)","Account = 'RH024'")
			'brapi.ErrorLog.LogMessage(si, sEntity & ": " & dt.Rows.Count.ToString)
			'brapi.ErrorLog.LogMessage(si, sEntity & ": " & objeto.ToString)				
			
		End Sub

		Sub GetDriverDataTableSSGG(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal sDriverScenario As String, ByVal sDriverTime As String, ByVal sDriverEntity As String, ByVal sDriverUD1 As String, ByVal sTargetUD1_List As List(Of MemberInfo), ByVal sDriverAccount1 As String, ByVal sDriverAccount2 As String, ByVal sDriverAccount3 As String, ByVal sDriverAccount4 As String, ByVal sDriverAccount5 As String)	
		
			Dim objCol = New DataColumn
            objCol.ColumnName = "UD1"
            objCol.DataType = GetType(String)
            objCol.DefaultValue = ""
            objCol.AllowDBNull = False
			dtDriver.Columns.Add(objCol)									
			
			objCol = New DataColumn
            objCol.ColumnName = "Account"
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
			
			If Not sDriverEntity = "Same as target"
				sDriverEntity = ":E#" & sDriverEntity
			Else
				sDriverEntity = String.Empty
			End If					
						
			Dim sDriver1Cube As String = GetCube(sDriverAccount1)	
			Dim sDriver2Cube As String = GetCube(sDriverAccount2)
			Dim sDriver3Cube As String = GetCube(sDriverAccount3)
			Dim sDriver4Cube As String = GetCube(sDriverAccount4)
			Dim sDriver5Cube As String = GetCube(sDriverAccount5)
			
			Dim sSegment As String = String.Empty
			
			'brapi.ErrorLog.LogMessage(si, "2")								
			For Each sTargetUD1_Member As MemberInfo In sTargetUD1_List												
				
				Dim sCalc As String = String.Empty
				Dim dData As Decimal = 0
				Dim row As DataRow		
				'brapi.ErrorLog.LogMessage(si, "1")	
				
				If sDriverAccount1 <> String.Empty
					row = dtDriver.NewRow	
					row("UD1") = sTargetUD1_Member.Member.Name									
					row("Account") = sDriverAccount1									
					If sDriver1Cube = ":Cb#CONTR"
						sCalc = "A#" & row("Account") & ":S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#None" & sDriverEntity & sDriver1Cube
					Else If sDriverAccount1 = "Por_Estar"
						sCalc = "A#N_UP:S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Forms:F#None:I#None:V#YTD:U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Else
						sCalc = "A#" & row("Account") & ":S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None" & sDriverEntity
					End If
					dData = api.Data.GetDataCell(sCalc).CellAmount							
					If dData.ToString <> "0"
						row("Amount") = dData
						dtDriver.Rows.Add(row.ItemArray)
					End If
				End If			
								
				If sDriverAccount2 <> String.Empty
					row = dtDriver.NewRow									
					row("UD1") = sTargetUD1_Member.Member.Name						
					row("Account") = sDriverAccount2										
					If sDriver2Cube = ":Cb#CONTR"
						sCalc = "A#" & row("Account") & ":S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#None" & sDriverEntity & sDriver2Cube
					Else If sDriverAccount2 = "Por_Estar"
						sCalc = "A#N_UP:S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Forms:F#None:I#None:V#YTD:U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Else
						sCalc = "A#" & row("Account") & ":S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None" & sDriverEntity
					End If					
					dData = api.Data.GetDataCell(sCalc).CellAmount
					If dData.ToString <> "0"
						row("Amount") = dData
						dtDriver.Rows.Add(row.ItemArray)
					End If
				End If
				
				'brapi.ErrorLog.LogMessage(si, "3")					
				If sDriverAccount3 <> String.Empty
					row = dtDriver.NewRow									
					row("UD1") = sTargetUD1_Member.Member.Name										
					row("Account") = sDriverAccount3										
					If sDriver3Cube = ":Cb#CONTR"
						sCalc = "A#" & row("Account") & ":S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#None" & sDriverEntity & sDriver3Cube
					Else If sDriverAccount3 = "Por_Estar"
						sCalc = "A#N_UP:S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Forms:F#None:I#None:V#YTD:U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Else
						sCalc = "A#" & row("Account") & ":S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None" & sDriverEntity
					End If								
					dData = api.Data.GetDataCell(sCalc).CellAmount
					If dData.ToString <> "0"
						row("Amount") = dData
						dtDriver.Rows.Add(row.ItemArray)
					End If
				End If
				
				'brapi.ErrorLog.LogMessage(si, "4")						
				If sDriverAccount4 <> String.Empty
					row = dtDriver.NewRow														
					row("Account") = sDriverAccount4										
					If sDriver4Cube = ":Cb#CONTR"
						sCalc = "A#" & row("Account") & ":S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#None" & sDriverEntity & sDriver4Cube
					Else If sDriverAccount4 = "Por_Estar"
						sCalc = "A#N_UP:S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Forms:F#None:I#None:V#YTD:U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Else
						sCalc = "A#" & row("Account") & ":S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None" & sDriverEntity
					End If								
					dData = api.Data.GetDataCell(sCalc).CellAmount
					If dData.ToString <> "0"
						row("Amount") = dData
						dtDriver.Rows.Add(row.ItemArray)
					End If
				End If
				
				'brapi.ErrorLog.LogMessage(si, "5")						
				If sDriverAccount5 <> String.Empty
					row = dtDriver.NewRow									
					row("UD1") = sTargetUD1_Member.Member.Name										
					row("Account") = sDriverAccount5										
					If sDriver5Cube = ":Cb#CONTR"
						sCalc = "A#" & row("Account") & ":S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#None" & sDriverEntity & sDriver5Cube
					Else If sDriverAccount5 = "Por_Estar"
						sCalc = "A#N_UP:S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Forms:F#None:I#None:V#YTD:U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Else
						sCalc = "A#" & row("Account") & ":S#" & sDriverScenario & ":T#" & sDriverTime & ":U1#" & row("UD1") & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None" & sDriverEntity
					End If								
					dData = api.Data.GetDataCell(sCalc).CellAmount
					If dData.ToString <> "0"
						row("Amount") = dData
						dtDriver.Rows.Add(row.ItemArray)
					End If					
				End If									
					
			Next
			
			'brapi.ErrorLog.LogMessage(si, "6")	
			
			'Dim objeto As Object = dt.Compute("SUM(Amount)","Account = 'RH024'")
			'brapi.ErrorLog.LogMessage(si, sEntity & ": " & dt.Rows.Count.ToString)
			'brapi.ErrorLog.LogMessage(si, sEntity & ": " & objeto.ToString)				
			
		End Sub
	
		Function GetDriverData(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal sEntity As String, ByVal sUD7 As String, ByVal sDriverUD1 As String, ByVal sTargetUD1Base As String, ByVal sSegment As String, ByVal sDriverUD3 As String, ByVal sTargetUD3Base As String, ByVal sDriverAccount1 As String, ByVal sDriverAccount2 As String, ByVal sDriverAccount3 As String, ByVal sDriverAccount4 As String, ByVal sDriverAccount5 As String, ByVal sDriverAccount1Pond As String, ByVal sDriverAccount2Pond As String, ByVal sDriverAccount3Pond As String, ByVal sDriverAccount4Pond As String, ByVal sDriverAccount5Pond As String, ByVal sDriverSite1 As String, ByVal sDriverSite2 As String, ByVal sDriverSite3 As String, ByVal sDriverSite4 As String, ByVal sDriverSite5 As String) As Double
									
			Dim sFilterNum As String = String.Empty
			Dim sFilterDen As String = String.Empty			
			
			'Filtros UD1
			If sDriverUD1 = ("Allocation") 
				sFilterNum = sFilterNum & " AND UD1 = '" & sTargetUD1Base & "'"	
			Else If sDriverUD1 = ("Same as target")
				sFilterNum = sFilterNum & " AND UD1 = '" & sTargetUD1Base & "'"	
				sFilterDen = sFilterDen & " AND UD1 = '" & sTargetUD1Base & "'"
			End If					
			
			'Filtros UD3
			If sDriverUD3 = ("Allocation") 
				sFilterNum = sFilterNum & " AND UD3 = '" & sTargetUD3Base & "'"	
			Else If sDriverUD3 = ("Same as target")
				sFilterNum = sFilterNum & " AND UD3 = '" & sTargetUD3Base & "'"	
				sFilterDen = sFilterDen & " AND UD3 = '" & sTargetUD3Base & "'"
			End If							
			
			If sSegment <> String.Empty
				sFilterDen = sFilterDen & " AND Segment = '" & sSegment & "'"
			End If
			
			'Filtro UD7
			Dim sFilterUD7 As String = String.Empty			
							
			Dim oDataDriverNum As Object
			Dim oDataDriverDen As Object									
			
			'Driver 1
			Dim dDataDriver1 As Double = 0
			If sDriverAccount1Pond <> "0"
				
				If Not sDriverSite1.Equals("Top")
					sFilterUD7 = " AND UD7 = '" & sUD7 & "'"
				End If
				
				oDataDriverNum = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount1 & "'" & sFilterNum & sFilterUD7)		
				oDataDriverDen = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount1 & "'" & sFilterDen & sFilterUD7)
				
				If Not oDataDriverDen Is DBNull.Value 
					If Convert.ToDouble(oDataDriverDen) <> 0 					
						If Not oDataDriverNum Is DBNull.Value 
							dDataDriver1 = Convert.ToDouble(oDataDriverNum) / Convert.ToDouble(oDataDriverDen)
						End If
					Else
						sDriverAccount1Pond = "0"
					End If	
				Else
					sDriverAccount1Pond = "0"											
				End If					
				
			End If
									
			'Driver 2
			Dim dDataDriver2 As Double = 0
			If sDriverAccount2Pond <> "0"
				
				If Not sDriverSite2.Equals("Top")
					sFilterUD7 = " AND UD7 = '" & sUD7 & "'"
				End If
								
				oDataDriverNum = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount2 & "'" & sFilterNum & sFilterUD7)		
				oDataDriverDen = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount2 & "'" & sFilterDen & sFilterUD7)										
				If Not oDataDriverDen Is DBNull.Value 
					If Convert.ToDouble(oDataDriverDen) <> 0 					
						If Not oDataDriverNum Is DBNull.Value 
							dDataDriver2 = Convert.ToDouble(oDataDriverNum) / Convert.ToDouble(oDataDriverDen)
						End If
					Else
						sDriverAccount2Pond = "0"
					End If	
				Else
					sDriverAccount2Pond = "0"											
				End If		
				

			End If	
									
			'Driver 3
			Dim dDataDriver3 As Double = 0
			If sDriverAccount3Pond <> "0"
				
				If Not sDriverSite3.Equals("Top")
					sFilterUD7 = " AND UD7 = '" & sUD7 & "'"
				End If
				
				oDataDriverNum = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount3 & "'" & sFilterNum & sFilterUD7)		
				oDataDriverDen = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount3 & "'" & sFilterDen & sFilterUD7)											
				If Not oDataDriverDen Is DBNull.Value 
					If Convert.ToDouble(oDataDriverDen) <> 0 					
						If Not oDataDriverNum Is DBNull.Value 
							dDataDriver3 = Convert.ToDouble(oDataDriverNum) / Convert.ToDouble(oDataDriverDen)
						End If
					Else
						sDriverAccount3Pond = "0"
					End If	
				Else
					sDriverAccount3Pond = "0"											
				End If			
			End If	
			
			'Driver 4
			Dim dDataDriver4 As Double = 0
			If sDriverAccount4Pond <> "0"
				
				If Not sDriverSite4.Equals("Top")
					sFilterUD7 = " AND UD7 = '" & sUD7 & "'"
				End If
								
				oDataDriverNum = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount4 & "'" & sFilterNum & sFilterUD7)		
				oDataDriverDen = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount4 & "'" & sFilterDen & sFilterUD7)											
				If Not oDataDriverDen Is DBNull.Value 
					If Convert.ToDouble(oDataDriverDen) <> 0 					
						If Not oDataDriverNum Is DBNull.Value 
							dDataDriver4 = Convert.ToDouble(oDataDriverNum) / Convert.ToDouble(oDataDriverDen)
						End If
					Else
						sDriverAccount4Pond = "0"
					End If	
				Else
					sDriverAccount4Pond = "0"											
				End If				
			End If	
			
			'Driver 5
			Dim dDataDriver5 As Double = 0
			If sDriverAccount5Pond <> "0"
				
				If Not sDriverSite5.Equals("Top")
					sFilterUD7 = " AND UD7 = '" & sUD7 & "'"
				End If
								
				oDataDriverNum = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount5 & "'" & sFilterNum & sFilterUD7)		
				oDataDriverDen = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount5 & "'" & sFilterDen & sFilterUD7)											
				If Not oDataDriverDen Is DBNull.Value 
					If Convert.ToDouble(oDataDriverDen) <> 0 					
						If Not oDataDriverNum Is DBNull.Value 
							dDataDriver5 = Convert.ToDouble(oDataDriverNum) / Convert.ToDouble(oDataDriverDen)
						End If
					Else
						sDriverAccount5Pond = "0"
					End If	
				Else
					sDriverAccount5Pond = "0"											
				End If			
			End If					
			
'			If sEntity = "E502" And sTargetUD1Base = "NV05" Then
'				Brapi.ErrorLog.LogMessage(si, "Data1: " & dDataDriver1.ToString)
'				Brapi.ErrorLog.LogMessage(si, "Data2: " & dDataDriver2.ToString)
'				Brapi.ErrorLog.LogMessage(si, "Data3: " & dDataDriver3.ToString)
'				Brapi.ErrorLog.LogMessage(si, "Data4: " & dDataDriver4.ToString)
'				Brapi.ErrorLog.LogMessage(si, "Data5: " & dDataDriver5.ToString)				
'			End If	
			
			Dim sDriverAccount1PondFinal As Double = 0
			Dim sDriverAccount2PondFinal As Double = 0
			Dim sDriverAccount3PondFinal As Double = 0
			Dim sDriverAccount4PondFinal As Double = 0
			Dim sDriverAccount5PondFinal As Double = 0
			
			Dim sDriverPondTotal As Double = CDbl(sDriverAccount1Pond) + CDbl(sDriverAccount2Pond) + CDbl(sDriverAccount3Pond) + CDbl(sDriverAccount4Pond) + CDbl(sDriverAccount5Pond)
			If sDriverPondTotal <> 0
				sDriverAccount1PondFinal = CDbl(sDriverAccount1Pond) / sDriverPondTotal
				sDriverAccount2PondFinal = CDbl(sDriverAccount2Pond) / sDriverPondTotal
				sDriverAccount3PondFinal = CDbl(sDriverAccount3Pond) / sDriverPondTotal
				sDriverAccount4PondFinal = CDbl(sDriverAccount4Pond) / sDriverPondTotal
				sDriverAccount5PondFinal = CDbl(sDriverAccount5Pond) / sDriverPondTotal
			End If						
			
			Dim dDataDriverResult As Double = (dDataDriver1 * sDriverAccount1PondFinal) _
											 + (dDataDriver2 * sDriverAccount2PondFinal) _
											 + (dDataDriver3 * sDriverAccount3PondFinal) _
											 + (dDataDriver4 * sDriverAccount4PondFinal) _
											 + (dDataDriver5 * sDriverAccount5PondFinal) 																		
											 
			'Brapi.ErrorLog.LogMessage(si, "Driver: " & dDataDriverResult.tostring)
										
			Return dDataDriverResult
			
		End Function
		
		Function GetCube (ByVal sDriverAccount As String) As String
			
			Dim sDriverCube As String = String.Empty
			If sDriverAccount = "Pres_Aprob_Año_N" Or sDriverAccount = "Valor_Bruto" 
				sDriverCube = ":Cb#CONTR"
			End If
			Return sDriverCube
			
		End Function		

		Function GetDriverSite (ByVal si As SessionInfo, ByVal sDriverSite As String) As List(Of MemberInfo)
			
			Dim sSites_List As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Sites", "U7#Top.Base", True)			
			Dim sSites_ListTop As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Sites", "U7#Top", True)			
				
			Dim sSites_ListDriver As List(Of MemberInfo) = sSites_ListTop
			If Not sDriverSite.Equals("Top")
				sSites_ListDriver = sSites_List
			End If
			
			Return sSites_ListDriver
			
		End Function
		
		Function GetDriverCalc(ByVal api As FinanceRulesApi, ByVal sDriverAccount As String, ByVal sDriverScenario As String, ByVal sUD1 As String, ByVal sUD3 As String, ByVal sUD7 As String) As String
					
			Dim sCalc As String						
			
			Select Case sDriverAccount
			
				'Drivers CONTR
				Case "Pres_Aprob_Año_N","Valor_Bruto"
					sCalc = "A#" & sDriverAccount & ":U1#" & sUD1 & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#None" & sCalc_E502 & ":Cb#CONTR"
			
				'Drivers PARCHIS & PARCHIS SSGG
				Case "Parchis","N_UP"
					sCalc = "A#" & sDriverAccount & ":U1#" & sUD1 & ":C#Local:O#Forms:F#None:I#None:V#YTD:U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None" & sCalc_E502_Parchis		
	
				'Drivers PARCHIS SITE
				Case "Parchis_Site_BU"
					sCalc = "A#" & sDriverAccount & ":U1#" & sUD1 & ":U7#" & sUD7 & ":C#Local:O#Forms:F#None:I#None:V#YTD:U2#HD01:U3#None:U4#None:U5#None:U6#None:U8#None" & sCalc_E502_Parchis								
				
				'Drivers CONSO
				Case Else					
					sCalc = "A#" & sDriverAccount & ":U1#" & sUD1 & ":U3#" & sUD3 & ":U7#" & sUD7 & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U4#Gestion:U5#Top:U6#Top:U8#None" & sCalc_E502													
					
			End Select		
			
			Return sCalc
			
		End Function

		Function GetScenarioDriver (ByVal api As FinanceRulesApi, ByVal sScenario As String) As String
		
			Dim sDriverScenario As String = sScenario
			Dim periodNumber As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Month
				
			If sScenario = "R" Then	
			
				Select Case periodNumber
					Case "1", "2", "3", "4", "5"
						sDriverScenario = "O1"
					Case "6", "7", "8", "9", "10", "11"
						sDriverScenario = "P06"
					Case Else
						sDriverScenario = "R"
				End Select
			
			Else If sScenario = "P03" Then
				sDriverScenario = "O1"
				
			Else If (sScenario = "P06" Or sScenario = "P09" Or sScenario = "P11") Then
				sDriverScenario = "P06"
			
			End If	
			
			Return sDriverScenario
			
		End Function

		Sub Calc_CR_Accounts (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		
			
			Dim FSK As New OneStream.BusinessRule.Finance.XFW_FSK_MemberFormulas.MainClass
			
			FSK.GtosGrales_CRJ1(si, api, globals, args)
			FSK.GtosGrales_CRJ2(si, api, globals, args)
			FSK.GtosGrales_CRJ3(si, api, globals, args)
			FSK.GtosGrales_CRJ4(si, api, globals, args)
			FSK.GtosGrales_CRJ5(si, api, globals, args)
			
			FSK.GtosGrales_CRG1(si, api, globals, args)
			FSK.GtosGrales_CRG2(si, api, globals, args)
			FSK.GtosGrales_CRG3(si, api, globals, args)
			
			FSK.GtosGrales_Q21(si, api, globals, args)
			
			FSK.GtosGrales_Q3i(si, api, globals, args)
			
		End Sub							
		
		Sub Calc_CR_Q3 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi)		
			
			api.Data.ClearCalculatedData("A#CR_Q3",True,True,True)
			
			api.Data.Calculate("A#CR_Q3:U3#8080:U6#Allocation_IN:U5#Rule_001 = (A#Calc_CRJ:U3#8080:U6#Allocation_IN:U5#Rule_001 * (-1)) - A#623200:U3#8080:U6#Allocation_IN:U5#Rule_001 + A#682100:U3#8080:U6#Allocation_IN:U5#Rule_001",True)		
			api.Data.Calculate("A#CR_Q3:U3#8080:U6#Allocation_IN:U5#Rule_001:I#None = A#CR_Q3:U3#8080:I#None:U6#Allocation_IN:U5#Rule_001 + A#681100:U3#8080:I#None:U6#Allocation_IN:U5#Rule_001 + A#680100:U3#8080:I#None:U6#Allocation_IN:U5#Rule_001 + A#682100:U3#8080:I#None:U6#Allocation_IN:U5#Rule_001",True)
										
		End Sub			
			
		Sub LogWriteCellData (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal i As Integer, ByVal dbcCell As DataBufferCell)
			
			Brapi.ErrorLog.LogMessage(si, "-----------------------------")
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID Account:" & api.Members.GetMemberName(DimType.Account.Id,dbcCell.DataBufferCellPk.AccountId))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID UD1:" & api.Members.GetMemberName(DimType.UD1.Id,dbcCell.DataBufferCellPk.UD1Id))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID UD2:" & api.Members.GetMemberName(DimType.UD2.Id,dbcCell.DataBufferCellPk.UD2Id))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID UD3:" & api.Members.GetMemberName(DimType.UD3.Id,dbcCell.DataBufferCellPk.UD3Id))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID UD4:" & api.Members.GetMemberName(DimType.UD4.Id,dbcCell.DataBufferCellPk.UD4Id))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID UD5:" & api.Members.GetMemberName(DimType.UD5.Id,dbcCell.DataBufferCellPk.UD5Id))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID UD6:" & api.Members.GetMemberName(DimType.UD6.Id,dbcCell.DataBufferCellPk.UD6Id))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID Flow:" & api.Members.GetMemberName(DimType.Flow.Id,dbcCell.DataBufferCellPk.FlowId))
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID IC:" & api.Members.GetMemberName(DimType.IC.Id,dbcCell.DataBufferCellPk.ICId))						
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - ID Origin:" & api.Members.GetMemberName(DimType.Origin.Id,dbcCell.DataBufferCellPk.OriginId))	
			Brapi.ErrorLog.LogMessage(si, i.ToString & " - Value:" & dbcCell.CellAmount.ToString)								
		
		End Sub
		
		Sub OLD_CalculateAllocation_ (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal dr As DataRow)		

			Dim sRuleName As String = String.Empty
			
			Dim sSourceEntity As String = String.Empty
			Dim sSourceAccount As String = String.Empty
			Dim sSourceUD1 As String = String.Empty
			Dim sSourceUD3 As String = String.Empty
			
			Dim sTargetEntity As String = String.Empty
			Dim sTargetAccount As String = String.Empty
			Dim sTargetUD1 As String = String.Empty
			Dim sTargetUD3 As String = String.Empty
			
			Dim sDriverEntity As String = String.Empty
			Dim sDriverAccount As String = String.Empty
			Dim sDriverUD1 As String = String.Empty
			Dim sDriverUD3 As String = String.Empty			
																		
			sRuleName =  dr(0).ToString
			
			'Origen
				sSourceEntity =  	dr(1).ToString
				sSourceAccount =  	dr(2).ToString
				sSourceUD1 =  		dr(3).ToString
				sSourceUD3 =  		dr(4).ToString						
			'Destino
				sTargetEntity =  	dr(5).ToString
				sTargetAccount =  	dr(6).ToString
				sTargetUD1 =	 	dr(7).ToString
				sTargetUD3 = 		dr(8).ToString	
			'Driver
				sDriverEntity = 	dr(9).ToString
				sDriverAccount = 	dr(10).ToString
				sDriverUD1 = 		dr(11).ToString
				sDriverUD3 = 		dr(12).ToString							
	
			'Destino igual que origen
			If sTargetEntity = "Same as source" Then
				sTargetEntity = sSourceEntity
			End If
			
			If sTargetAccount = "Same as source" Then
				sTargetAccount = sSourceAccount
			End If
			
			If sTargetUD1 = "Same as source" Then
				sTargetUD1 = sSourceUD1
			End If
			
			If sTargetUD3 = "Same as source" Then
				sTargetUD3 = sSourceUD3
			End If														
				
			'Obtenemos miembros a recorrer
			Dim sTargetEntity_List As List(Of MemberInfo)
			If(sTargetEntity.Contains(","))
				sTargetEntity_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sTargetEntity, True)
			Else
				sTargetEntity_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sTargetEntity & ".Base", True)
			End If
			
			Dim sTargetAccount_List As List(Of MemberInfo) 
			If(sTargetAccount.Contains(","))
				sTargetAccount_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Accounts", "A#" & sTargetAccount, True)
			Else
				sTargetAccount_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Accounts", "A#" & sTargetAccount & ".Base", True)
			End If
			
			Dim sTargetUD1_List As List(Of MemberInfo)
			If(sTargetUD1.Contains(","))
				sTargetUD1_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_BusinessUnits", "U1#" & sTargetUD1, True)
			Else
				sTargetUD1_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_BusinessUnits", "U1#" & sTargetUD1 & ".Base.Where((Name DoesNotContain Elim) And (Name DoesNotContain 9909))", True)
			End If
			
			Dim sTargetUD3_List As List(Of MemberInfo)
			If(sTargetUD3.Contains(","))
				sTargetUD3_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_CostCenters", "U3#" & sTargetUD3, True)
			Else
				sTargetUD3_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_CostCenters", "U3#" & sTargetUD3 & ".Base", True)
			End If		
			
			Dim sTargetEntity_MemberInfo As List(Of MemberInfo)
			sTargetEntity_MemberInfo = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & api.Pov.Entity.Name, True)									
			
			If (sTargetEntity_List.Contains(sTargetEntity_MemberInfo(0)))
				
				For Each sTargetAccount_Member As MemberInfo In sTargetAccount_List
					For Each sTargetUD1_Member As MemberInfo In sTargetUD1_List
						For Each sTargetUD3_Member As MemberInfo In sTargetUD3_List								
															
							Dim sTargetEntity_Name As String = sTargetEntity_MemberInfo(0).Member.Name
							Dim sTargetAccount_Name As String = sTargetAccount_Member.Member.Name
							Dim sTargetUD1_Name As String = sTargetUD1_Member.Member.Name
							Dim sTargetUD3_Name As String = sTargetUD3_Member.Member.Name

							Dim sSourceEntity_Name As String = sTargetEntity
							Dim sSourceAccount_Name As String = sTargetAccount
							Dim sSourceUD1_Name As String = sSourceUD1
							Dim sSourceUD3_Name As String = sSourceUD3
							
							Dim sDriverEntity_Num As String = sDriverEntity
							Dim sDriverAccount_Num As String = sDriverAccount
							Dim sDriverUD1_Num As String = sDriverUD1
							Dim sDriverUD3_Num As String = sDriverUD3							
																
							Dim sDriverEntity_Den As String = sDriverEntity
							Dim sDriverAccount_Den As String = sDriverAccount
							Dim sDriverUD1_Den As String = sDriverUD1
							Dim sDriverUD3_Den As String = sDriverUD3									
							
							'Destino igual que origen
							If sTargetEntity = sSourceEntity Then
								sSourceEntity_Name = sTargetEntity_Name
							End If
							
							If sTargetAccount = sSourceAccount Then
								sSourceAccount_Name = sTargetAccount_Name
							End If
							
							If sTargetUD1 = sSourceUD1 Then
								sSourceUD1_Name = sTargetUD1_Name
							End If
							
							If sTargetUD3 = sSourceUD3 Then
								sSourceUD3_Name = sTargetUD3_Name
							End If
							
							'Driver igual que destino
							If sDriverEntity = "Same as target" Then
								sDriverEntity_Num = sTargetEntity_Name
								sDriverEntity_Den = sTargetEntity_Name
							End If								
							
							If sDriverAccount = "Allocation" Then
								sDriverAccount_Num = sTargetAccount_Name
								sDriverAccount_Den = sTargetAccount
							End If
							
							If sDriverUD1 = "Allocation" Then
								sDriverUD1_Num = sTargetUD1_Name
								sDriverUD1_Den = sTargetUD1
							End If
							
							If sDriverUD3 = "Allocation" Then
								sDriverUD3_Num = sTargetUD3_Name
								sDriverUD3_Den = sTargetUD3
							End If	
							
							Dim sPesoDriverCalculo As New Text.StringBuilder()
							Dim sPesoDriverDato As String
							sPesoDriverCalculo.Append("   E#" & sDriverEntity_Num & ":A#" & sDriverAccount_Num & ":U1#" & sDriverUD1_Num & ":U3#" & sDriverUD3_Num & ":O#Top:U2#Top:U4#Gestion:F#Top:I#Top:U5#None:U6#None")
							sPesoDriverCalculo.Append(" / E#" & sDriverEntity_Den & ":A#" & sDriverAccount_Den & ":U1#" & sDriverUD1_Den & ":U3#" & sDriverUD3_Den & ":O#Top:U2#Top:U4#Gestion:F#Top:I#Top:U5#None:U6#None")								
							sPesoDriverDato = api.Data.GetDataCell(sPesoDriverCalculo.ToString).CellAmount.ToString
							

							'Brapi.ErrorLog.LogMessage(si, sPesoDriverDato)
							
							If sPesoDriverDato <> "0"
							
								'Calculo para repartir (Allocation_IN)
								Dim sCalculo_IN As New Text.StringBuilder()
								
								'Target (IN)
								'sCalculo_IN.Append("O#AdjInput:U2#None:U4#None:F#None:I#None:E#" & sTargetEntity_Name & ":A#" & sTargetAccount_Name & ":U1#" & sTargetUD1_Name & ":U3#" & sTargetUD3_Name & ":U5#" & sRuleName & ":U6#Allocation_IN")
								'Source
								'sCalculo_IN.Append(" = O#AdjInput:U2#Top:U4#Gestion:F#Top:I#Top:E#" & sSourceEntity_Name & ":A#" & sSourceAccount_Name & ":U1#" & sSourceUD1_Name & ":U3#" & sSourceUD3_Name & ":U5#None:U6#None")
								'Driver
								'sCalculo_IN.Append(" * O#Top:U2#Top:U4#Gestion:F#Top:I#Top:E#" & sTargetEntity_Name & ":A#" & sDriverAccount_Name & ":U1#" & sDriverUD1_Name & ":U3#" & sDriverUD3_Name & ":U5#None:U6#None")
								'sCalculo_IN.Append(" / O#Top:U2#Top:U4#Gestion:F#Top:I#Top:E#" & sTargetEntity & ":A#" & sDriverAccount & ":U1#" & sDriverUD1 & ":U3#" & sDriverUD3 & ":U5#None:U6#None")								
								
								'Target (IN)
								sCalculo_IN.Append("E#" & sTargetEntity_Name & ":A#" & sTargetAccount_Name & ":U1#" & sTargetUD1_Name & ":U3#" & sTargetUD3_Name & ":U5#" & sRuleName & ":U6#Allocation_IN")
								'Source
								sCalculo_IN.Append(" = E#" & sSourceEntity_Name & ":A#" & sSourceAccount_Name & ":U1#" & sSourceUD1_Name & ":U3#" & sSourceUD3_Name & ":U5#None:U6#None")
								'Driver						
								sCalculo_IN.Append(" * " & sPesoDriverDato)									
								
								'Calculo para repartir (Allocation_OUT)
								Dim sCalculo_OUT As New Text.StringBuilder()
								'Source (OUT)
								sCalculo_OUT.Append("E#" & sTargetEntity_Name & ":A#" & sSourceAccount_Name & ":U1#" & sSourceUD1_Name & ":U3#" & sSourceUD3_Name & ":U5#" & sRuleName & ":U6#Allocation_OUT")
								'Source
								sCalculo_OUT.Append(" = -(E#" & sTargetEntity_Name & ":A#" & sSourceAccount_Name & ":U1#" & sSourceUD1_Name & ":U3#" & sSourceUD3_Name & ":U5#None:U6#None) ")							
								
								api.Data.Calculate(sCalculo_IN.ToString)
								api.Data.Calculate(sCalculo_OUT.ToString)
								'Brapi.ErrorLog.LogMessage(si, sCalculo_IN.ToString)
								'Brapi.ErrorLog.LogMessage(si, sCalculo_OUT.ToString)

							End If
							
						Next
					Next
				Next
			End If					
			
		End Sub			
		
		Sub OLD_CalculateAllocationSSGG (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal dr As DataRow, ByVal sScenario As String, ByVal sTime As String)		

			Dim sRuleName As String = String.Empty
			
			Dim sDriverEntity As String = String.Empty
			Dim sDriverUD1 As String = String.Empty
			Dim sDriverUD3 As String = String.Empty
			Dim sDriverAccount1, sDriverAccount2, sDriverAccount3, sDriverAccount4, sDriverAccount5 As String
			Dim sDriverAccount1Pond, sDriverAccount2Pond, sDriverAccount3Pond, sDriverAccount4Pond, sDriverAccount5Pond As String		
										
			sRuleName =  dr(0).ToString
			
			sDriverEntity = 		dr(10).ToString
			sDriverUD1 = 			dr(11).ToString
			sDriverUD3 = 			dr(12).ToString	
			sDriverAccount1 = 		dr(13).ToString		
			sDriverAccount1Pond = 	dr(14).ToString		
			sDriverAccount2 = 		dr(15).ToString		
			sDriverAccount2Pond = 	dr(16).ToString	
			sDriverAccount3 = 		dr(17).ToString		
			sDriverAccount3Pond = 	dr(18).ToString	
			sDriverAccount4 = 		dr(19).ToString		
			sDriverAccount4Pond = 	dr(20).ToString	
			sDriverAccount5 = 		dr(21).ToString		
			sDriverAccount5Pond = 	dr(22).ToString	
			
			Dim sEntity As String = api.Pov.Entity.Name
'			Brapi.ErrorLog.LogMessage(si, "Entity: " & sEntity)	
			
			If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then			
	
				Dim sDriverScenario As String = sScenario
				Dim periodNumber As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Month
				
				If sScenario = "R" Then	
				
					Select Case periodNumber
						Case "1", "2", "3", "4", "5"
							sDriverScenario = "O1"
						Case "6", "7", "8", "9", "10", "11"
							sDriverScenario = "P06"
						Case Else
							sDriverScenario = "R"
					End Select
				
				Else If sScenario = "P03" Then
					sDriverScenario = "O1"
					
				Else If (sScenario = "P06" Or sScenario = "P09" Or sScenario = "P11") Then
					sDriverScenario = "P06"
				
				End If							
				
				Dim DatoHolding = api.Data.GetDataCell("A#N_UP:S#" & sDriverScenario & ":T#" & Left(sTime,4) & ":C#Local:O#Forms:I#None:V#YTD:F#None:U1#HD01:U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount				
				If DatoHolding > 0 Then	

					'Listado de UD1 Parchis SSGG
					Dim sTargetUD1_List As List(Of MemberInfo) = New List(Of MemberInfo)
					For Each mUD1 As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_BusinessUnits", "U1#DN.Base", True)
						
						Dim DatoUP = api.Data.GetDataCell("A#N_UP:S#" & sDriverScenario & ":T#" & Left(sTime,4) & "M12" & ":C#Local:O#Forms:I#None:V#YTD:F#None:U1#" & mUD1.Member.Name & ":U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
						If DatoUP > 0 Then
							If Not mUD1.Member.Name <> "HD01"
								sTargetUD1_List.Add(mUD1)
							End If			
						End If
										
					Next
					
					'Obtener Tabla de Drivers
					Me.GetDriverDataTableSSGG(si, api, _
						sDriverScenario, Left(sTime,4) & "M12", sDriverEntity, sDriverUD1, sTargetUD1_List, _ 
						sDriverAccount1, sDriverAccount2, sDriverAccount3, sDriverAccount4, sDriverAccount5)					
					
					If sTargetUD1_List.Count > 0
						For Each mUD1 As MemberInfo In sTargetUD1_List 						
							
							Dim sUD1 As String = mUD1.Member.Name.ToString
							
							Dim dDataDriver As Double = GetDriverData(si, api, sEntity, "" _ 											
								, sDriverUD1, sUD1, String.Empty _
								, String.Empty, String.Empty _
								, sDriverAccount1, sDriverAccount2, sDriverAccount3, sDriverAccount4, sDriverAccount5 _
								, sDriverAccount1Pond, sDriverAccount2Pond, sDriverAccount3Pond, sDriverAccount4Pond, sDriverAccount5Pond, _
								"", "", "", "", "")		
								
							'Brapi.ErrorLog.LogMessage(si, "Entity: " & sEntity & " UD1: " & sUD1 & " DriverData: " & dDataDriver.ToString)
										
							'Reparto CR_V a CR_W2J9
							Dim sCalculo As String = "A#CR_W2J9:I#" & sEntity & ":U1#" & sUD1 & ":U2#HD01:U3#All:U4#Gestion:U5#" & sRuleName & ":U6#Allocation_In " _
										  & " = A#CR_V:I#Top:U1#HD01:U2#Top:U3#All:U4#Gestion:U5#Top:U6#Top * " & dDataDriver.ToString & " * -1 " _
										  & " + (A#623200:I#E502:U1#HD01:U2#Top:U3#All:U4#Gestion:U5#Top:U6#Top * " & dDataDriver.ToString & ")"							
							api.Data.Calculate(sCalculo)
						
							'Brapi.ErrorLog.LogMessage(si, "Calculate 1: " & sCalculo)
										  
							'Neteo
							sCalculo = "A#CR_W2J9:I#" & sEntity & ":U1#9909:U2#9909:U4#Gestion:U5#" & sRuleName & ":U6#Allocation_Out " _ 
										  & " = A#CR_V:I#Top:U1#HD01:U2#Top:U4#Gestion:U5#Top:U6#Top" _ 
										  & " - A#623200:I#E502:U1#HD01:U2#Top:U4#Gestion:U5#Top:U6#Top"
							api.Data.Calculate(sCalculo)
		
							'Brapi.ErrorLog.LogMessage(si, "Calculate 2: " & sCalculo)									  
										  
							'NEW Ingreso CR_W1
							sCalculo = "A#CR_W1:I#" & sEntity & ":U1#HD01:U2#" & sUD1 & ":U3#All:U4#Gestion:U5#" & sRuleName & ":U6#Allocation_Out " _
										  & " = A#CR_W1:I#" & sEntity & ":U1#HD01:U2#" & sUD1 & ":U3#All:U4#Gestion:U5#Top:U6#Top" _
										  & " + A#CR_W2J9:I#" & sEntity & ":U1#" & sUD1 & ":U2#HD01:U3#All:U4#Gestion:U5#Top:U6#Top"
							api.Data.Calculate(sCalculo)

							'Brapi.ErrorLog.LogMessage(si, "Calculate 3: " & sCalculo)									  									  
										  
							'Reparto 623200 IC E101
							sCalculo = "A#623200:I#" & sEntity & ":U1#" & sUD1 & ":U2#HD01:U3#All:U4#Gestion:U5#" & sRuleName & ":U6#Allocation_In " _
										  & " = A#623200:I#E101:U1#HD01:U2#Top:U3#All:U4#Gestion:U5#Top:U6#Top * " & dDataDriver.ToString & ""
							api.Data.Calculate(sCalculo)
					
							'Brapi.ErrorLog.LogMessage(si, "Calculate 4: " & sCalculo)										  
										  
							'NEW Neteo
							sCalculo = "A#623200:I#" & sEntity & ":U1#9909:U2#9909:U4#Gestion:U5#" & sRuleName & ":U6#Allocation_Out " _
										  & " = A#623200:I#E101:U1#HD01:U2#Top:U4#Gestion:U5#Top:U6#Top * -1"
							api.Data.Calculate(sCalculo)
								
							'Brapi.ErrorLog.LogMessage(si, "Calculate 5: " & sCalculo)										  
										  
							'NEW Ingreso CR_W1
							sCalculo = "A#CR_W1:I#" & sEntity & ":U1#HD01:U2#" & sUD1 & ":U3#All:U4#Gestion:U5#" & sRuleName & ":U6#Allocation_Out " _
										  & " = A#CR_W1:I#" & sEntity & ":U1#HD01:U2#" & sUD1 & ":U3#All:U4#Gestion:U5#Top:U6#Top" _
										  & " + A#623200:I#" & sEntity & ":U1#" & sUD1 & ":U2#HD01:U3#All:U4#Gestion:U5#Top:U6#Top"
							api.Data.Calculate(sCalculo)
											
							'Brapi.ErrorLog.LogMessage(si, "Calculate 6: " & sCalculo)										  
										  
							'Vaciado de CR_W1
							sCalculo = "A#CR_W1:I#" & sEntity & ":U1#9909:U2#9909:U4#Repartir_SSGG:U5#" & sRuleName & ":U6#Allocation_Out " _
										  & " = A#CR_V:I#Top:U1#HD01:U2#Top:U4#Gestion:U5#Top:U6#Top" _
										  & " - A#623200:I#E101:U1#HD01:U2#Top:U4#Gestion:U5#Top:U6#Top" _
										  & " - A#623200:I#E502:U1#HD01:U2#Top:U4#Gestion:U5#Top:U6#Top"
							api.Data.Calculate(sCalculo)

							'Brapi.ErrorLog.LogMessage(si, "Calculate 7: " & sCalculo)							
							
						Next
					End If
					
				End If
				
			End If
		
		End Sub		
		
		
	End Class
	
End Namespace