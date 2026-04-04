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

Namespace OneStream.BusinessRule.Finance.ALL_AllocationCalc_inge
	
	Public Class MainClass
			
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
		
			Try
				
				'Variables Globales
				Dim sScenario As String = api.Pov.Scenario.Name	
				Dim sScenarioType As String = api.Pov.ScenarioType.Name							
				Dim sTime As String = api.Pov.Time.Name		
				
				Dim sItemID As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("ItemID")				
				Dim sFunction As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("Function")
				
				'Brapi.ErrorLog.LogMessage(si, "ScenarioType:" & sScenarioType)
				'Brapi.ErrorLog.LogMessage(si, "Time:" & sTime)
				
				If (Not api.Entity.HasChildren) And api.Cons.IsLocalCurrencyForEntity() Then

					Dim sEntity As String = api.Pov.Entity.Name
					'Brapi.ErrorLog.LogMessage(si, "Entity: " & sEntity)				
					
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

#Region "Consultas"

		Sub ClearAllocationRulesAll (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sTime As String, sScenario As String)
		
			api.Data.ClearCalculatedData(True, True, True, True,,,,,,,,,"U5#Rules.Base","U6#Net_Change.Base",, )	
	
		End Sub	

		Sub ClearAllocationRulesOne (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sTime As String, sScenario As String, sItemID As String)
		
			Dim sRuleName As String = String.Empty				
		
			Dim sbSQL As New Text.StringBuilder()
						
			sbSQL.Append(" SELECT RuleName")			
			sbSQL.Append(" FROM XFC_ALL_Allocation_Rules")																				
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
			
			sbSQL.Append(" , Accounts")	
			sbSQL.Append(" , Entities")
			
			sbSQL.Append(" , Source_Product")
			sbSQL.Append(" , Source_Customer")	
			sbSQL.Append(" , Target_Product")
			sbSQL.Append(" , Target_Customer")				
			
			sbSQL.Append(" , driver")								
				
			sbSQL.Append(" FROM XFC_ALL_Allocation_Rules")																	
			
			sbSQL.Append(" WHERE Scenario = '" & sScenario & "'")
			sbSQL.Append(" AND Time = '" & sTime & "'")
			sbSQL.Append(" AND Enabled = 1")			
			
			sbSQL.Append(" ORDER BY Sequence")
							
			'Brapi.ErrorLog.LogMessage(si, sbSQL.ToString)
			
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)											
					
					'Calculamos cuentas que forman parte de los drivers de EBITDA (CR_P) o MARGEN BRUTO (CR_I)
					Calc_CR_Accounts(si, api, globals, args)
					
					For Each dr As DataRow In dt.Rows
						
						'Ejecución de repartos
						Me.CalculateAllocationRules(si, api, dr, sScenario)	
						
						'Calculamos cuentas que forman parte de los drivers de EBITDA (CR_P) o MARGEN BRUTO (CR_I)
						Calc_CR_Accounts(si, api, globals, args)											
						
					Next
					
				End Using
			End Using
					
			
		End Sub	

		Sub ExecuteAllocationRulesOne (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, sTime As String, sScenario As String, sItemID As String)				
		
			Dim sbSQL As New Text.StringBuilder()
						
			sbSQL.Append(" SELECT RuleName")
			
			sbSQL.Append(" , Accounts")	
			sbSQL.Append(" , Entities")
			
			sbSQL.Append(" , Source_Product")
			sbSQL.Append(" , Source_Customer")	
			sbSQL.Append(" , Target_Product")
			sbSQL.Append(" , Target_Customer")				
			
			sbSQL.Append(" , driver")								
				
			sbSQL.Append(" FROM XFC_ALL_Allocation_Rules")																	
			
			sbSQL.Append(" WHERE Scenario = '" & sScenario & "'")
			sbSQL.Append(" AND Time = '" & sTime & "'")
			sbSQL.Append(" AND Enabled = 1")		
			sbSQL.Append(" AND ItemID = '" & sItemID & "'")			
			
			sbSQL.Append(" ORDER BY Sequence")			
							
			'Brapi.ErrorLog.LogMessage(si, sbSQL.ToString)
			
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)											
					
					'Calculamos cuentas que forman parte de los drivers de EBITDA (CR_P) o MARGEN BRUTO (CR_I)
					Calc_CR_Accounts(si, api, globals, args)
					
					For Each dr As DataRow In dt.Rows
						
						'Ejecución de repartos
						Me.CalculateAllocationRules(si, api, dr, sScenario)	
						
						'Calculamos cuentas que forman parte de los drivers de EBITDA (CR_P) o MARGEN BRUTO (CR_I)
						Calc_CR_Accounts(si, api, globals, args)
					Next
					
				End Using
			End Using
								
		End Sub	
		
#End Region

#Region "Calculos"

		Sub ExecutePreallocation (ByVal si As SessionInfo, ByVal api As FinanceRulesApi)		

			ClearPreallocation(si, api)
			
			CalculatePreallocationDN(si, api, "DN_EMP")
			CalculatePreallocationDN(si, api, "DN_PEC")
			CalculatePreallocationDN(si, api, "DN_SER")
		
		End Sub

		Sub ClearPreallocation (ByVal si As SessionInfo, ByVal api As FinanceRulesApi)		

			' 0. Eliminación del preasignado	
		
			api.Data.ClearCalculatedData(True, True, True, True,,,,,,,,,"U5#Preallocation",,, )	
	
		
		End Sub
	
		Sub CalculatePreallocationDN (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal sDN As String)		

			' 1. Delimitación del cruce de miembros a los que afecta la preasignación
			
			' Account
			
			'Dim listAccount_MemberInfo As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Accounts", "A#CP_CG.Base", True)									
			'20/12/2023
			Dim listAccount_MemberInfo As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Accounts", "A#CDR.Base,A#Calc_CRJ.Base,A#Calc_CRG.Base", True)									
			'BRApi.ErrorLog.LogMessage(si, "Número miembros Account: " & listAccount_MemberInfo.Count.ToString)	
			
			'UD1
			Dim listUD1_MemberInfo As List(Of MemberInfo) = New List(Of MemberInfo)
			For Each miMemberUD1 As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_BusinessUnits", "U1#" & sDN & ".Base", True)														
				If Not miMemberUD1.Member.Name.Equals("HD01") And Not miMemberUD1.Member.Name.EndsWith("99")
					listUD1_MemberInfo.Add(miMemberUD1)
				End If
			Next
			'BRApi.ErrorLog.LogMessage(si, "Número miembros UD1: " & listUD1_MemberInfo.Count.ToString)	
			
			'UD7
			Dim listUD7_MemberInfo As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Sites", "U7#SG.Base", True)								
			'BRApi.ErrorLog.LogMessage(si, "Número miembros UD7: " & listUD7_MemberInfo.Count.ToString)	
			
			
			' 2. Mapping DN - Segmento
			Dim sSegmento As String = String.Empty
			Select Case sDN
				
				Case "DN_EMP"
					sSegmento = "SM99"
				
				Case "DN_PEC"
					sSegmento = "SC99"
					
				Case "DN_SER"
					sSegmento = "SO99"
				
			End Select			
			
			' 3. Creación del Data Buffer
			Dim diDestinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
			Dim dbSource As DataBuffer
			dbSource = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, "U5#None:U6#None", diDestinationInfo)
				
			' 4. Creación de filtros con los cruces de miembros		
			Dim filtersAccount As Dictionary(Of Integer, Object) = GetFilter(listAccount_MemberInfo)
			Dim filtersUD1 As Dictionary(Of Integer, Object) = GetFilter(listUD1_MemberInfo)
			Dim filtersUD7 As Dictionary(Of Integer, Object) = GetFilter(listUD7_MemberInfo)

			' 5. Aplicación de filtros en Data Buffer
			dbSource = dbSource.GetFilteredDataBuffer(DimType.Account, filtersAccount)
			dbSource = dbSource.GetFilteredDataBuffer(DimType.UD1, filtersUD1)
			dbSource = dbSource.GetFilteredDataBuffer(DimType.UD7, filtersUD7)
			
			' 6. Recorremos cada registro del fichero de carga y por cada uno creamos 4 registros nuevos
			If Not dbSource Is Nothing Then
									
				Dim dbPreallocation As DataBuffer = New DataBuffer()					
					
				Dim i As Integer = 0					
				For Each dbcSourceCell As DataBufferCell In dbSource.DataBufferCells.Values
					
					Brapi.ErrorLog.LogMessage(si, "Entra")					
					
				' Nuevo registro 1 - Anulación registo UD1 que viene en el fichero
				
					Dim dbcAllocation1 As New DataBufferCell(dbcSourceCell)
					dbcAllocation1.DataBufferCellPk.UD5Id = api.Members.GetMemberId(DimType.UD5.Id,"Preallocation")
					dbcAllocation1.DataBufferCellPk.UD6Id = api.Members.GetMemberId(DimType.UD6.Id,"None")
					dbcAllocation1.CellAmount = dbcSourceCell.CellAmount * -1
					
					'ICP (Si tiene ICP le ponemos E997)
					Dim sIc As String = BRApi.Finance.Metadata.GetMember(si, DimType.IC.Id,	dbcAllocation1.DataBufferCellPk.IcId).Member.Name
					If Not sIc = "None"
						dbcAllocation1.DataBufferCellPk.IcId = api.Members.GetMemberId(DimType.Ic.Id,"E997")
					End If

					'UD2 (Si estamos distribuyendo por UD1 y en función de la ICP)					
					Dim sUD1Name As String = String.Empty
					If sIc = "None"
						sUD1Name = api.Members.GetMember(DimType.UD1.Id, dbcAllocation1.DataBufferCellPk.UD1Id).Name
						dbcAllocation1.DataBufferCellPk.UD2Id = api.Members.GetMemberId(DimType.UD2.Id, sUD1Name)
					End If							
					
					
				' Nuevo registro 2 - Añadir registro Segmento como si viniese de origen
				
					Dim dbcAllocation2 As New DataBufferCell(dbcSourceCell)
					dbcAllocation2.DataBufferCellPk.UD1Id = api.Members.GetMemberId(DimType.UD1.Id,sSegmento)
					dbcAllocation2.DataBufferCellPk.UD5Id = api.Members.GetMemberId(DimType.UD5.Id,"Preallocation")
					dbcAllocation2.DataBufferCellPk.UD6Id = api.Members.GetMemberId(DimType.UD6.Id,"None")
					dbcAllocation2.CellAmount = dbcSourceCell.CellAmount
					
					'ICP (Si tiene ICP le ponemos E997)
					sIc = BRApi.Finance.Metadata.GetMember(si, DimType.IC.Id,	dbcAllocation2.DataBufferCellPk.IcId).Member.Name
					If Not sIc = "None"
						dbcAllocation2.DataBufferCellPk.IcId = api.Members.GetMemberId(DimType.Ic.Id,"E997")
					End If

					'UD2 (Si estamos distribuyendo por UD1 y en función de la ICP)					
					sUD1Name = String.Empty
					If sIc = "None"
						sUD1Name = api.Members.GetMember(DimType.UD1.Id, dbcAllocation2.DataBufferCellPk.UD1Id).Name
						dbcAllocation2.DataBufferCellPk.UD2Id = api.Members.GetMemberId(DimType.UD2.Id, sUD1Name)
					End If							
					
					
				' Nuevo registro 3 - Reparto a UD1 del registro del segmento
				
					Dim dbcAllocation3 As New DataBufferCell(dbcSourceCell)
					dbcAllocation3.DataBufferCellPk.UD5Id = api.Members.GetMemberId(DimType.UD5.Id,"Preallocation")
					dbcAllocation3.DataBufferCellPk.UD6Id = api.Members.GetMemberId(DimType.UD6.Id,"Allocation_In")
					dbcAllocation3.CellAmount = dbcSourceCell.CellAmount
				
					'ICP (Si tiene ICP le ponemos E997)
					sIc = BRApi.Finance.Metadata.GetMember(si, DimType.IC.Id,	dbcAllocation3.DataBufferCellPk.IcId).Member.Name
					If Not sIc = "None"
						dbcAllocation3.DataBufferCellPk.IcId = api.Members.GetMemberId(DimType.Ic.Id,"E997")
					End If

					'UD2 (Si estamos distribuyendo por UD1 y en función de la ICP)					
					sUD1Name = String.Empty
					If sIc = "None"
						sUD1Name = api.Members.GetMember(DimType.UD1.Id, dbcAllocation3.DataBufferCellPk.UD1Id).Name
						dbcAllocation3.DataBufferCellPk.UD2Id = api.Members.GetMemberId(DimType.UD2.Id, sUD1Name)
					End If						
					
				' Nuevo registro 4 - Anulación registro segmento
				
					Dim dbcAllocation4 As New DataBufferCell(dbcSourceCell)
					dbcAllocation4.DataBufferCellPk.UD1Id = api.Members.GetMemberId(DimType.UD1.Id,sSegmento)
					dbcAllocation4.DataBufferCellPk.UD5Id = api.Members.GetMemberId(DimType.UD5.Id,"Preallocation")
					dbcAllocation4.DataBufferCellPk.UD6Id = api.Members.GetMemberId(DimType.UD6.Id,"Allocation_Out")
					dbcAllocation4.CellAmount = dbcSourceCell.CellAmount * -1							
					
					'ICP (Si tiene ICP le ponemos E997)
					sIc = BRApi.Finance.Metadata.GetMember(si, DimType.IC.Id,	dbcAllocation4.DataBufferCellPk.IcId).Member.Name
					If Not sIc = "None"
						dbcAllocation4.DataBufferCellPk.IcId = api.Members.GetMemberId(DimType.Ic.Id,"E997")
					End If

					'UD2 (Si estamos distribuyendo por UD1 y en función de la ICP)					
					sUD1Name = String.Empty
					If sIc = "None"
						sUD1Name = api.Members.GetMember(DimType.UD1.Id, dbcAllocation4.DataBufferCellPk.UD1Id).Name
						dbcAllocation4.DataBufferCellPk.UD2Id = api.Members.GetMemberId(DimType.UD2.Id, sUD1Name)
					End If						
					
					
					
				' Añadimos los registros al Data Buffer de Preasignaciones
				
					dbPreallocation.SetCell(si, dbcAllocation1,True)
					dbPreallocation.SetCell(si, dbcAllocation2,True)
					dbPreallocation.SetCell(si, dbcAllocation3,True)
					dbPreallocation.SetCell(si, dbcAllocation4,True)
				
				Next
			
				' Enviamos a BBDD
				api.Data.SetDataBuffer(dbPreallocation, diDestinationInfo,,,,,,,,,,,,,True)					
			
			End If
			
		End Sub

		Sub CalculateAllocationRules (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal dr As DataRow, ByVal sScenario As String)		

			'Entidad que estamos recorriendo
			Dim sEntity_MemberInfo As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & api.Pov.Entity.Name, True)									
			Dim sEntity = Api.Pov.Entity.Name		

			Dim sRuleName As String = String.Empty
			
			Dim sSourceUD1 As String = String.Empty
			Dim sSourceUD3 As String = String.Empty
			Dim sSourceAccount As String = String.Empty	
			Dim sSourceEntity As String = String.Empty
			Dim sSourceSite As String = String.Empty			
			
			Dim sTargetUD1 As String = String.Empty
			Dim sTargetUD3 As String = String.Empty
			Dim sTargetAccount As String = String.Empty	
			Dim sTargetUD7 As String = String.Empty	
			
			Dim sDriverUD1 As String = String.Empty
			Dim sDriverUD3 As String = String.Empty
			Dim sDriverEntity As String = String.Empty
			Dim sDriverUD7 As String = String.Empty
			
			Dim sDriverAccount1, sDriverAccount2, sDriverAccount3, sDriverAccount4, sDriverAccount5 As String
			Dim sDriverAccount1Pond, sDriverAccount2Pond, sDriverAccount3Pond, sDriverAccount4Pond, sDriverAccount5Pond As String		
			
			sRuleName =  dr(0).ToString

'			If Api.Pov.Entity.Name = "E122"
'				BRApi.ErrorLog.LogMessage(si, sRuleName & " - Entra")
'			End If				
			
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
				sTargetUD7 =  			dr(9).ToString	
				
			'Driver
				sDriverUD1 = 			dr(10).ToString
				sDriverUD3 = 			dr(11).ToString	
				sDriverEntity = 		dr(12).ToString
				sDriverUD7 = 			dr(13).ToString
				
				sDriverAccount1 = 		dr(14).ToString		
				sDriverAccount1Pond = 	dr(15).ToString		
									
				sDriverAccount2 = 		dr(16).ToString		
				sDriverAccount2Pond = 	dr(17).ToString	
				
				sDriverAccount3 = 		dr(18).ToString		
				sDriverAccount3Pond = 	dr(19).ToString	
				
				sDriverAccount4 = 		dr(20).ToString		
				sDriverAccount4Pond = 	dr(21).ToString						
				
				sDriverAccount5 = 		dr(22).ToString		
				sDriverAccount5Pond = 	dr(23).ToString						

			Dim sSourceEntity_List As List(Of MemberInfo) = GetMemberBaseSource(si, sSourceEntity, "E#", "Entities")
					
'			If sEntity = "E107" And sRuleName = "Rule_013"
'				BRApi.ErrorLog.LogMessage(si, "CR_I: " & api.Data.GetDataCell("A#CR_I:U7#Top:C#Local:O#Top:F#Top:I#Top:V#YTD:U1#GR01:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U8#None").CellAmount.ToString())
'				BRApi.ErrorLog.LogMessage(si, "CR_A_D_E: " & api.Data.GetDataCell("A#CR_A_D_E:U7#Top:C#Local:O#Top:F#Top:I#Top:V#YTD:U1#GR01:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U8#None").CellAmount.ToString())
'				BRApi.ErrorLog.LogMessage(si, "RH024: " & api.Data.GetDataCell("A#RH024:U7#Top:C#Local:O#Top:F#Top:I#Top:V#YTD:U1#GR01:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U8#None").CellAmount.ToString())
'				BRApi.ErrorLog.LogMessage(si, "Parchis_Site_BU: " & api.Data.GetDataCell("A#Parchis_Site_BU:U7#Top:C#Local:O#Top:F#Top:I#Top:V#YTD:U1#GR01:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U8#None").CellAmount.ToString())
'			End If
			
			'BRApi.ErrorLog.LogMessage(si, "Entra")
			
			If (sSourceEntity_List.Contains(sEntity_MemberInfo(0)))																	
					
				'Obtenemos miembros ORIGEN a recorrer			
				Dim sSourceAccount_List As List(Of MemberInfo) = GetMemberBaseAccount(si, sSourceAccount, "A#", "CONS_Accounts")		
				Dim sSourceUD1_List As List(Of MemberInfo) = GetMemberBaseSource(si, sSourceUD1, "U1#", "CONS_BusinessUnits")
				Dim sSourceUD3_List As List(Of MemberInfo) = GetMemberBaseSource(si, sSourceUD3, "U3#", "CONS_CostCenters")					
				Dim sSourceSite_List As List(Of MemberInfo) = GetMemberBaseSource(si, sSourceSite, "U7#", "CONS_Sites")			
				
				'Obtenemos miembros TARGET a recorrer
				Dim sTargetAccount_List As List(Of MemberInfo)
				Dim sTargetUD1_List As List(Of MemberInfo)
				Dim sTargetUD3_List As List(Of MemberInfo)
				Dim sTargetUD7_List As New List(Of MemberInfo)
				
				Dim sUD1SameAsSource As String = String.Empty
				Dim sUD3SameAsSource As String = String.Empty
								
				If sTargetAccount = "Same as source" Then
					sTargetAccount = sSourceAccount
					sTargetAccount_List = sSourceAccount_List				
				Else
					sTargetAccount_List = GetMemberBaseAccount(si, sTargetAccount, "A#", "CONS_Accounts")		
				End If
				
				If sTargetUD1 = "Same as source" Then
					sUD1SameAsSource = sTargetUD1
					sTargetUD1 = sSourceUD1
					sTargetUD1_List = sSourceUD1_List				
				Else
					sTargetUD1_List = GetMemberBaseTarget(si, sTargetUD1, "U1#", "CONS_BusinessUnits", sSourceUD1_List)
				End If
				
				If sTargetUD3 = "Same as source" Then
					sUD3SameAsSource = sTargetUD3				
					sTargetUD3 = sSourceUD3
					sTargetUD3_List = sSourceUD3_List
				Else
					sTargetUD3_List = GetMemberBaseTarget(si, sTargetUD3, "U3#", "CONS_CostCenters", sSourceUD3_List)
				End If	
							
				If sTargetUD7 <> "Same as source" Then
					sTargetUD7_List = GetMemberBaseTargetSite(si, sTargetUD7, "U7#", "CONS_Sites")
				End If							
				
				'Obtenemos miembros TARGET para repartos de segmentos
				Dim sTargetUD1_List_SM99 As List(Of MemberInfo) = GetMemberBaseTargetSegment(si, "DN_EMP", "U1#", "CONS_BusinessUnits", sSourceUD1_List)
				Dim sTargetUD1_List_SC99 As List(Of MemberInfo) = GetMemberBaseTargetSegment(si, "DN_PEC", "U1#", "CONS_BusinessUnits", sSourceUD1_List)
				Dim sTargetUD1_List_SO99 As List(Of MemberInfo) = GetMemberBaseTargetSegment(si, "DN_SER", "U1#", "CONS_BusinessUnits", sSourceUD1_List)
							
				'Obtenemos el escenario y periodo para obtener los drivers de la E502 si corresponde
				If (sEntity.Equals("E502") )
					Me.GetDriverScenarioPeriod(si, api, sScenario, "IPT")
				ElseIf (sDriverEntity <> "Same as target")
					Me.GetDriverScenarioPeriod(si, api, sScenario, sDriverEntity)
				Else
					sCalc_DriverScenarioPeriod = String.Empty
					sCalc_DriverScenarioPeriod_Parchis = String.Empty
				End If				

				Dim diDestinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
				Dim dbSourceDataBuffer As DataBuffer
				dbSourceDataBuffer = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, "U5#Top:U6#Top", diDestinationInfo)
				
				Dim sSourceUD4_List As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Natures", "U4#Gestion.Base", True)
				
				'Dim sSourceUD5_List As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_AdditionalInfo", "U5#None, U5#Rules.Base", True)
				'BRApi.ErrorLog.LogMessage(si, "Número miembros UD5: " & sSourceUD5_List.Count.ToString)	
				
				Dim filtersAccount As Dictionary(Of Integer, Object) = GetFilter(sSourceAccount_List)
				Dim filtersUD1 As Dictionary(Of Integer, Object) = GetFilter(sSourceUD1_List)
				Dim filtersUD3 As Dictionary(Of Integer, Object) = GetFilter(sSourceUD3_List)
				Dim filtersUD4 As Dictionary(Of Integer, Object) = GetFilter(sSourceUD4_List)	
				'Dim filtersUD5 As Dictionary(Of Integer, Object) = GetFilter(sSourceUD5_List)
				Dim filtersSite As Dictionary(Of Integer, Object) = GetFilter(sSourceSite_List)

				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.Account, filtersAccount)
				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.UD1, filtersUD1)
				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.UD3, filtersUD3)
				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.UD4, filtersUd4)
				'dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.UD5, filtersUd5)
				dbSourceDataBuffer = dbSourceDataBuffer.GetFilteredDataBuffer(DimType.UD7, filtersSite)
				
				Dim sDriverScenario = GetScenarioDriver(api, sScenario)
				
				Dim dtDriver = Me.GetDriverDataTable(si, api, sDriverScenario _
									, sDriverUD1, sTargetUD1_List, sDriverUD3, sTargetUD3_List, sDriverEntity, sDriverUD7 _
									, sDriverAccount1, sDriverAccount2, sDriverAccount3, sDriverAccount4, sDriverAccount5, sRuleName)
									
				If Not dbSourceDataBuffer Is Nothing Then
									
					Dim dbAllocationIn As DataBuffer = New DataBuffer()
					Dim dbAllocationOut As DataBuffer = New DataBuffer()					
					
					Dim i As Integer = 0					
					For Each dbcSourceCell As DataBufferCell In dbSourceDataBuffer.DataBufferCells.Values						
						
						'LogWriteCellData(si, api, 0, dbcSourceCell)
						
						Dim sSourceUD5Name As String = api.Members.GetMember(DimType.UD5.Id, dbcSourceCell.DataBufferCellPk.UD5Id).Name
						If (Not dbcSourceCell.CellStatus.IsNoData And dbcSourceCell.CellAmount <> 0 And sSourceUD5Name <> "Preallocation")	
						
							i = i + 1
							'LogWriteCellData(si, api, i, dbcSourceCell)
																			
							If(sTargetAccount = "Same as source" Or sTargetAccount = sSourceAccount)
								sTargetAccount_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Accounts", "A#" & api.Members.GetMember(DimType.Account.Id, dbcSourceCell.DataBufferCellPk.AccountId).Name, True)	
							End If
							
							Dim sSourceUD1Name As String = api.Members.GetMember(DimType.UD1.Id, dbcSourceCell.DataBufferCellPk.UD1Id).Name
							Dim sSegment As String = String.Empty
							
							If sSourceUD1Name = "SM99" And sDriverUD1 = "Allocation"
								sTargetUD1_List = sTargetUD1_List_SM99
								sSegment = "DN_EMP"
							Else If sSourceUD1Name = "SC99" And sDriverUD1 = "Allocation"
								sTargetUD1_List = sTargetUD1_List_SC99
								sSegment = "DN_PEC"
							Else If sSourceUD1Name = "SO99" And sDriverUD1 = "Allocation"
								sTargetUD1_List = sTargetUD1_List_SO99										
								sSegment = "DN_SER"
							Else If(sTargetUD1 = "Same as source" Or sTargetUD1 = sSourceUD1)
								sTargetUD1_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_BusinessUnits", "U1#" & api.Members.GetMember(DimType.UD1.Id, dbcSourceCell.DataBufferCellPk.UD1Id).Name, True)	
							End If
							
							If(sTargetUD3 = "Same as source" Or sTargetUD3 = sSourceUD3)
								sTargetUD3_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_CostCenters", "U3#" & api.Members.GetMember(DimType.UD3.Id, dbcSourceCell.DataBufferCellPk.UD3Id).Name, True)	
							End If	
							
							Dim bAllocationIn As Boolean = False
							
							'No hay reparto por Site, se mantiene site de origen
							If (sDriverUD7 <> "Allocation" Or sTargetUD7 = "Same as source")
								sTargetUD7_List = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Sites", "U7#" & api.Members.GetMember(DimType.UD7.Id, dbcSourceCell.DataBufferCellPk.UD7Id).Name, True)	
							End If							
							
							For Each sTargetAccount_Member As MemberInfo In sTargetAccount_List
								For Each sTargetUD1_Member As MemberInfo In sTargetUD1_List
									For Each sTargetUD3_Member As MemberInfo In sTargetUD3_List																		
										For Each sTargetUD7_Member As MemberInfo In sTargetUD7_List	
											
'											BRApi.ErrorLog.LogMessage(si, sTargetUD1_Member.Member.Name)
											
											Dim dDataDriver As Double = GetDriverData(si, api, dtDriver, sEntity _ 											
												, sDriverUD1, sTargetUD1_Member.Member.Name, sSegment _
												, sDriverUD3, sTargetUD3_Member.Member.Name _
												, sDriverUD7, sTargetUD7_Member.Member.Name _
												, sDriverAccount1, sDriverAccount2, sDriverAccount3, sDriverAccount4, sDriverAccount5 _
												, sDriverAccount1Pond, sDriverAccount2Pond, sDriverAccount3Pond, sDriverAccount4Pond, sDriverAccount5Pond)																																	
												
											If dDataDriver <> 0											
												
												Dim dbcAllocationInCell As New DataBufferCell(dbcSourceCell)
												
												'MODIFICACIÓN registros de Allocation In
												dbcAllocationInCell.DataBufferCellPk.UD5Id = api.Members.GetMemberId(DimType.UD5.Id,sRuleName)
												dbcAllocationInCell.DataBufferCellPk.UD6Id = api.Members.GetMemberId(DimType.UD6.Id,"Allocation_In")
												
												dbcAllocationInCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account.Id,sTargetAccount_Member.Member.Name)
												dbcAllocationInCell.DataBufferCellPk.UD1Id = api.Members.GetMemberId(DimType.UD1.Id,sTargetUD1_Member.Member.Name)
												dbcAllocationInCell.DataBufferCellPk.UD3Id = api.Members.GetMemberId(DimType.UD3.Id,sTargetUD3_Member.Member.Name)
												dbcAllocationInCell.DataBufferCellPk.UD7Id = api.Members.GetMemberId(DimType.UD7.Id,sTargetUD7_Member.Member.Name)	
												
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
												
'												If sEntity = "E105" And sTargetUD3_Member.Member.Name = "1010"
'													LogWriteCellData(si, api, i, dbcAllocationInCell)
'												End If												
												
'												If sEntity = "E105" And sTargetUD1_Member.Member.Name = "TR01" And sTargetUD3_Member.Member.Name = "1010"
'													LogWriteCellData(si, api, i, dbcAllocationInCell)
'												End If
												
												'GUARDAMOS el registro de ALLOCATION IN en el BUFFER
												dbAllocationIn.SetCell(si, dbcAllocationInCell,True)
												
												bAllocationIn = True
												
											End If
										Next	
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
					
					'Cálculo de CR_Q3
					If sRuleName = "Rule_001" Or sRuleName = "Rule_000" 
						Calc_CR_Q3(si, api)
					End If
					
				End If		
				
'				If sEntity = "E122"
'					BRApi.ErrorLog.LogMessage(si, sRuleName & " - Sale")
'				End If					
				
			End If				
		
		End Sub		

		Sub GetDriverScenarioPeriod (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal sScenario As String, ByVal sPerimeter As String)		
			
			If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then			
	
				Dim iMonth As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Month
				Dim iYear As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Year
				
				Dim sScenario_Driver As String = String.Empty
				Dim sPeriod_Driver As String = String.Empty
				
				sPeriod_Driver = iYear.ToString() & "M12"
				
				Dim sScenarioType As String = api.Pov.ScenarioType.Name
				
				Select Case sScenarioType
					
					Case "Actual"
				
						Select Case iMonth
							
							Case "1", "2", "3"
								sScenario_Driver = "O1"
							
							Case  "4", "5", "6", "7"
								sScenario_Driver = "P03"
								
							Case  "8", "9"
								sScenario_Driver = "P06"
								
							Case "10","11"
								sScenario_Driver = "P09"
								
							Case "12"
								sScenario_Driver = "P11"
								
						End Select
				
					Case "Forecast"
						
						Select Case sScenario
							Case "P03"
								sScenario_Driver = "O1"
							
							Case "P06"
								sScenario_Driver = "P03"
							
							Case "P09"
								sScenario_Driver = "P06"
								
							Case "P11"
								sScenario_Driver = "P06"
						End Select
					
					Case "Budget"
								'sScenario_Driver = "R"					
								'FAS 2023-10-30
								sScenario_Driver = "P09"					
								'FAS 2023-10-30
								sPeriod_Driver = (iYear - 1).ToString() & "M12"
					
						
				End Select		
				
				sCalc_DriverScenarioPeriod = ":S#" & sScenario_Driver & ":T#" & sPeriod_Driver & ":E#" & sPerimeter
				sCalc_DriverScenarioPeriod_Parchis = ":S#" & sScenario_Driver & ":T#" & sPeriod_Driver
				'Brapi.ErrorLog.LogMessage(si, "Filter E502: " & sCalc_E502)
				
			End If
		
		End Sub		

#End Region		
		
#Region "Miembros"

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
							Else If miMember.Member.Name.Equals("HD01")
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
		
		Function GetMemberBaseTargetSite (ByVal si As SessionInfo, ByVal sMemberTarget As String, ByVal sTag As String, ByVal sDimension As String) As List(Of MemberInfo)
			
			Dim MemberList As List(Of MemberInfo) = New List(Of MemberInfo)
			
			If(sMemberTarget.Contains(","))
				For Each sMember As String In sMemberTarget.Split(",")
					For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMember.Trim & ".Base", True)																
						MemberList.Add(miMember)
					Next
				Next
				
			Else

				For Each miMember As MemberInfo In  BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMemberTarget & ".Base", True)
					MemberList.Add(miMember)
				Next				
				
			End If		
			
			Return MemberList
			
		End Function						
				
#End Region		

#Region "Drivers"

		Function GetDriverDataTable(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal sDriverScenario As String, ByVal sDriverUD1 As String, ByVal sTargetUD1_List As List(Of MemberInfo), ByVal sDriverUD3 As String, ByVal sTargetUD3_List As List(Of MemberInfo), ByVal sDriverEntity As String, ByVal sDriverUD7 As String, ByVal sDriverAccount1 As String, ByVal sDriverAccount2 As String, ByVal sDriverAccount3 As String, ByVal sDriverAccount4 As String, ByVal sDriverAccount5 As String, ByVal sRuleName As String)	As DataTable
					
			Dim dtDriver As New DataTable("Driver_Data")
		
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
			Dim sSites_List_All As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Sites", "U7#Top.Base", True)	
			Dim sSites_List As New List(Of MemberInfo)
			
			For Each sSite As MemberInfo In sSites_List_All
				If api.Data.GetDataCell("A#Parchis_Entity_Site:U7#" & sSite.Member.Name & ":C#Local:O#Forms:F#None:I#None:V#YTD:U1#None:U2#HD01:U3#None:U4#None:U5#None:U6#None:U8#None").CellAmount <> 0 Then
					sSites_List.add(sSite)
				End If
			Next
				
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
					
					For Each sSite_Member As MemberInfo In sSites_List					
						
						'Recuperar driver a nivel de Site o Top
						Dim sSite As String = sSite_Member.Member.Name
						If sDriverUD7.Equals("Top") Then
							sSite = sDriverUD7
						End If	
						
						'Está la UD1 en este site?
						If api.Data.GetDataCell("A#Parchis_Site_BU:U1#" & sTargetUD1_Member.Member.Name & ":U7#" & sSite_Member.Member.Name & ":C#Local:O#Forms:F#None:I#None:V#YTD:U2#HD01:U3#None:U4#None:U5#None:U6#None:U8#None").CellAmount <> 0 Then

'						If sEntity.Equals("E107") And sTargetUD1_Member.Member.Name.Equals("SC99")
'							brapi.ErrorLog.LogMessage(si, "A#Parchis_Site_BU:U1#" & sTargetUD1_Member.Member.Name & ":U7#" & sSite_Member.Member.Name & ":C#Local:O#Forms:F#None:I#None:V#YTD:U2#HD01:U3#None:U4#None:U5#None:U6#None:U8#None")									
'						End If
							
							If sDriverAccount1 <> String.Empty
															
								'brapi.ErrorLog.LogMessage(si, "1")											
								row = dtDriver.NewRow									
								row("Account") = sDriverAccount1
								row("Segment") = sSegment
								row("UD1") = sTargetUD1_Member.Member.Name												
								row("UD3") = sTargetUD3_Member.Member.Name	
								row("UD7") = sSite_Member.Member.Name														
																											
								sCalc = GetDriverCalc(api, sDriverAccount1, sDriverScenario, row("UD1"), row("UD3"), sSite)									
								
								dData = api.Data.GetDataCell(sCalc).CellAmount
								If (sDriverAccount1.Equals("Parchis_Site_BU") And dData > 1)
									dData = 1
								End If	
								
								'Comentar esta parte
								
								'If (Not sDriverAccount1.Equals("CR_P") And dData <> 0) _ 
								'Or (sDriverAccount1.Equals("CR_P") And dData < 0)								
								
								'Descomentar esta parte
								
								If (Not sDriverAccount1.Equals("CR_P") And Not sDriverAccount1.Equals("CR_I") And dData <> 0) _ 
								Or (sDriverAccount1.Equals("CR_P") And dData < 0) _
								Or (sDriverAccount1.Equals("CR_I") And dData > 0)
								
								'Fin de descomentar
								
									row("Amount") = dData
									dtDriver.Rows.Add(row.ItemArray)						
								End If	
								
'								If sEntity.Equals("E107") And sTargetUD1_Member.Member.Name.Equals("EO03") And sRuleName = "Rule_013"
'									brapi.ErrorLog.LogMessage(si, "calc: " & sCalc)	
'									brapi.ErrorLog.LogMessage(si, "Account: " & sDriverAccount1)	
'									brapi.ErrorLog.LogMessage(si, "Segment: " & sSegment)	
'									brapi.ErrorLog.LogMessage(si, "UD1: " & sTargetUD1_Member.Member.Name)	
'									brapi.ErrorLog.LogMessage(si, "UD3: " & sTargetUD3_Member.Member.Name)	
'									brapi.ErrorLog.LogMessage(si, "UD7: " & sSite_Member.Member.Name)	
'									brapi.ErrorLog.LogMessage(si, "data: " & dData.ToString())									
'								End If
								
'								If sEntity.Equals("E105")
'									brapi.ErrorLog.LogMessage(si, api.Pov.Entity.Name & " - " & sCalc)	
'									brapi.ErrorLog.LogMessage(si, "data: " & dData.ToString())
'								End If

'									If sEntity.Equals("E123") 'And sTargetUD1_Member.Member.Name	 = "EO04"
'										Brapi.ErrorLog.LogMessage(si, "Calc: " & sCalc.ToString)										
'										Brapi.ErrorLog.LogMessage(si, "Data: " & dData.ToString)										
'									End If	

							End If

							If sDriverAccount2 <> String.Empty
								row = dtDriver.NewRow									
								row("Account") = sDriverAccount2
								row("Segment") = sSegment
								row("UD1") = sTargetUD1_Member.Member.Name												
								row("UD3") = sTargetUD3_Member.Member.Name																							
								row("UD7") = sSite_Member.Member.Name							
								
								sCalc = GetDriverCalc(api, sDriverAccount2, sDriverScenario, row("UD1"), row("UD3"), sSite)		
								
								dData = api.Data.GetDataCell(sCalc).CellAmount
								If (sDriverAccount2.Equals("Parchis_Site_BU") And dData > 1)
									dData = 1
								End If	
								
								'Comentar esta parte
								
								'If (Not sDriverAccount2.Equals("CR_P") And dData <> 0) _ 
								'Or (sDriverAccount2.Equals("CR_P") And dData < 0)	
								
								'Descomentar esta parte
								
								If (Not sDriverAccount2.Equals("CR_P") And Not sDriverAccount2.Equals("CR_I") And dData <> 0) _ 
								Or (sDriverAccount2.Equals("CR_P") And dData < 0) _
								Or (sDriverAccount2.Equals("CR_I") And dData > 0)
								
								'Fin de descomentar
								
									row("Amount") = dData
									dtDriver.Rows.Add(row.ItemArray)
									
									'If sEntity.Equals("E122")
										'Brapi.ErrorLog.LogMessage(si, "Calc: " & sCalc.ToString)
										'Brapi.ErrorLog.LogMessage(si, "UD1: " & sTargetUD1_Member.Member.Name	)
										'Brapi.ErrorLog.LogMessage(si, "Data: " & dData.ToString)
										
									'End If									
								End If
								
'									If sEntity.Equals("E123") And sTargetUD1_Member.Member.Name	 = "EO04"
'										Brapi.ErrorLog.LogMessage(si, "Calc: " & sCalc.ToString)										
'										Brapi.ErrorLog.LogMessage(si, "Data: " & dData.ToString)										
'									End If	
															
							End If

							If sDriverAccount3 <> String.Empty
								row = dtDriver.NewRow									
								row("Account") = sDriverAccount3
								row("Segment") = sSegment
								row("UD1") = sTargetUD1_Member.Member.Name												
								row("UD3") = sTargetUD3_Member.Member.Name																							
								row("UD7") = sSite_Member.Member.Name
								
								sCalc = GetDriverCalc(api, sDriverAccount3, sDriverScenario, row("UD1"), row("UD3"), sSite)	
														
								dData = api.Data.GetDataCell(sCalc).CellAmount
								If (sDriverAccount3.Equals("Parchis_Site_BU") And dData > 1)
									dData = 1
								End If	
								
								'Comentar esta parte
								
								'If (Not sDriverAccount3.Equals("CR_P") And dData <> 0) _ 
								'Or (sDriverAccount3.Equals("CR_P") And dData < 0)
								
								'Descomentar esta parte
								
								If (Not sDriverAccount3.Equals("CR_P") And Not sDriverAccount3.Equals("CR_I") And dData <> 0) _ 
								Or (sDriverAccount3.Equals("CR_P") And dData < 0) _
								Or (sDriverAccount3.Equals("CR_I") And dData > 0)
								
								'Fin de descomentar
								
									row("Amount") = dData
									dtDriver.Rows.Add(row.ItemArray)								
								End If
								
'									If sEntity.Equals("E123") And sTargetUD1_Member.Member.Name	 = "EO04"
'										Brapi.ErrorLog.LogMessage(si, "Calc: " & sCalc.ToString)										
'										Brapi.ErrorLog.LogMessage(si, "Data: " & dData.ToString)										
'									End If									
							End If

							If sDriverAccount4 <> String.Empty
								row = dtDriver.NewRow									
								row("Account") = sDriverAccount4
								row("Segment") = sSegment
								row("UD1") = sTargetUD1_Member.Member.Name												
								row("UD3") = sTargetUD3_Member.Member.Name																							
								row("UD7") = sSite_Member.Member.Name								
								
								sCalc = GetDriverCalc(api, sDriverAccount4, sDriverScenario, row("UD1"), row("UD3"), sSite)	
																								
								dData = api.Data.GetDataCell(sCalc).CellAmount
								If (sDriverAccount4.Equals("Parchis_Site_BU") And dData > 1)
									dData = 1
								End If									
									
								'Comentar esta parte
								
								'If (Not sDriverAccount4.Equals("CR_P") And dData <> 0) _ 
								'Or (sDriverAccount4.Equals("CR_P") And dData < 0)
								
								'Descomentar esta parte
								
								If (Not sDriverAccount4.Equals("CR_P") And Not sDriverAccount4.Equals("CR_I") And dData <> 0) _ 
								Or (sDriverAccount4.Equals("CR_P") And dData < 0) _
								Or (sDriverAccount4.Equals("CR_I") And dData > 0)
								
								'Fin de descomentar
								
									row("Amount") = dData			
									dtDriver.Rows.Add(row.ItemArray)
								End If
								
'									If sEntity.Equals("E123") And sTargetUD1_Member.Member.Name	 = "EO04"
'										Brapi.ErrorLog.LogMessage(si, "Calc: " & sCalc.ToString)										
'										Brapi.ErrorLog.LogMessage(si, "Data: " & dData.ToString)										
'									End If	
								
							End If

							If sDriverAccount5 <> String.Empty
								row = dtDriver.NewRow
								row("Account") = sDriverAccount5
								row("Segment") = sSegment
								row("UD1") = sTargetUD1_Member.Member.Name												
								row("UD3") = sTargetUD3_Member.Member.Name																							
								row("UD7") = sSite_Member.Member.Name								
								
								sCalc = GetDriverCalc(api, sDriverAccount5, sDriverScenario, row("UD1"), row("UD3"), sSite)
													
								dData = api.Data.GetDataCell(sCalc).CellAmount
								If (sDriverAccount5.Equals("Parchis_Site_BU") And dData > 1)
									dData = 1
								End If	
								
								'Comentar esta parte
								
								'If (Not sDriverAccount5.Equals("CR_P") And dData <> 0) _ 
								'Or (sDriverAccount5.Equals("CR_P") And dData < 0)
								
								'Descomentar esta parte
								
								If (Not sDriverAccount5.Equals("CR_P") And Not sDriverAccount5.Equals("CR_I") And dData <> 0) _ 
								Or (sDriverAccount5.Equals("CR_P") And dData < 0) _
								Or (sDriverAccount5.Equals("CR_I") And dData > 0)
								
								'Fin de descomentar
								
									row("Amount") = dData
									dtDriver.Rows.Add(row.ItemArray)
								End If					
							End If			
						End If
					Next
				Next
			Next
			
			'brapi.ErrorLog.LogMessage(si, "6")	
			
			'Dim objeto As Object = dt.Compute("SUM(Amount)","Account = 'RH024'")
			'brapi.ErrorLog.LogMessage(si, sEntity & ": " & dt.Rows.Count.ToString)
			'brapi.ErrorLog.LogMessage(si, sEntity & ": " & objeto.ToString)				
			
			Return dtDriver
			
		End Function

		Function GetDriverData(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal dtDriver As Datatable, ByVal sEntity As String, ByVal sDriverUD1 As String, ByVal sTargetUD1Base As String, ByVal sSegment As String, ByVal sDriverUD3 As String, ByVal sTargetUD3Base As String, ByVal sDriverUD7 As String, ByVal sTargetUD7Base As String, ByVal sDriverAccount1 As String, ByVal sDriverAccount2 As String, ByVal sDriverAccount3 As String, ByVal sDriverAccount4 As String, ByVal sDriverAccount5 As String, ByVal sDriverAccount1Pond As String, ByVal sDriverAccount2Pond As String, ByVal sDriverAccount3Pond As String, ByVal sDriverAccount4Pond As String, ByVal sDriverAccount5Pond As String) As Double
									
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
			
			If sSegment <> String.Empty And sDriverUD1 <> "Top"
				sFilterDen = sFilterDen & " AND Segment = '" & sSegment & "'"
			End If			
			
			'Filtros UD7
			If sDriverUD7 = ("Allocation") 
				sFilterNum = sFilterNum & " AND UD7 = '" & sTargetUD7Base & "'"	
			Else 'If sDriverUD7 = ("Same as target")
				sFilterNum = sFilterNum & " AND UD7 = '" & sTargetUD7Base & "'"	
				sFilterDen = sFilterDen & " AND UD7 = '" & sTargetUD7Base & "'"
			End If					
						
			Dim oDataDriverNum As Object
			Dim oDataDriverDen As Object									
			
			'Driver 1
			Dim dDataDriver1 As Double = 0
			If sDriverAccount1Pond <> "0"				
				
				oDataDriverNum = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount1 & "'" & sFilterNum)		
				oDataDriverDen = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount1 & "'" & sFilterDen)
				
'				If sEntity = "E107" And sTargetUD1Base = "EO03" Then
'					Brapi.ErrorLog.LogMessage(si, "Num: Account = '" & sDriverAccount1 & "'" & sFilterNum)
'					Brapi.ErrorLog.LogMessage(si, "Den: Account = '" & sDriverAccount1 & "'" & sFilterDen)								
'				End If
				
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
								
				oDataDriverNum = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount2 & "'" & sFilterNum)		
				oDataDriverDen = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount2 & "'" & sFilterDen)										
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
				
				oDataDriverNum = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount3 & "'" & sFilterNum)		
				oDataDriverDen = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount3 & "'" & sFilterDen)											
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
								
				oDataDriverNum = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount4 & "'" & sFilterNum)		
				oDataDriverDen = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount4 & "'" & sFilterDen)											
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
								
				oDataDriverNum = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount5 & "'" & sFilterNum)		
				oDataDriverDen = dtDriver.Compute("SUM(Amount)", "Account = '" & sDriverAccount5 & "'" & sFilterDen)											
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
			
						
'			If sEntity = "E107" And sTargetUD1Base = "EO03" Then
'				Brapi.ErrorLog.LogMessage(si, "Data1: " & (dDataDriver1 * sDriverAccount1PondFinal).ToString)
'				Brapi.ErrorLog.LogMessage(si, "Data2: " & (dDataDriver2 * sDriverAccount2PondFinal).ToString)
'				Brapi.ErrorLog.LogMessage(si, "Data3: " & (dDataDriver3 * sDriverAccount3PondFinal).ToString)
'				Brapi.ErrorLog.LogMessage(si, "Data4: " & (dDataDriver4 * sDriverAccount4PondFinal).ToString)
'				Brapi.ErrorLog.LogMessage(si, "Data5: " & (dDataDriver5 * sDriverAccount5PondFinal).ToString)				
'			End If	
			
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

		Function GetDriverSite (ByVal si As SessionInfo, ByVal sDriverUD7 As String) As List(Of MemberInfo)
			
			Dim sSites_List As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Sites", "U7#Top.Base", True)			
			Dim sSites_ListTop As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "CONS_Sites", "U7#Top", True)			
				
			Dim sSites_ListDriver As List(Of MemberInfo) = sSites_ListTop
			If Not sDriverUD7.Equals("Top")
				sSites_ListDriver = sSites_List
			End If
			
			Return sSites_ListDriver
			
		End Function
		
		Function GetDriverCalc(ByVal api As FinanceRulesApi, ByVal sDriverAccount As String, ByVal sDriverScenario As String, ByVal sUD1 As String, ByVal sUD3 As String, ByVal sUD7 As String) As String
					
			Dim sCalc As String						
			
			Select Case sDriverAccount
			
				'Drivers CONTR
				Case "Pres_Aprob_Año_N","Valor_Bruto"
					sCalc = "A#" & sDriverAccount & ":U1#" & sUD1 & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#" & sUD3 & sCalc_DriverScenarioPeriod & ":Cb#CONTR"
			
				'Drivers PARCHIS & PARCHIS SSGG
				Case "Parchis","N_UP"
					sCalc = "A#" & sDriverAccount & ":U1#" & sUD1 & ":C#Local:O#Forms:F#None:I#None:V#YTD:U2#HD01:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"' & sCalc_DriverScenarioPeriod_Parchis		
	
				'Drivers PARCHIS SITE
				Case "Parchis_Site_BU"
					sCalc = "A#" & sDriverAccount & ":U1#" & sUD1 & ":U7#" & sUD7 & ":C#Local:O#Forms:F#None:I#None:V#YTD:U2#HD01:U3#None:U4#None:U5#None:U6#None:U8#None"' & sCalc_DriverScenarioPeriod_Parchis								
				
				'Drivers CONSO
				Case Else					
					sCalc = "A#" & sDriverAccount & ":U1#" & sUD1 & ":U3#" & sUD3 & ":U7#" & sUD7 & ":C#Local:O#Top:F#Top:I#Top:V#YTD:U2#Top:U4#Gestion:U5#Top:U6#Top:U8#None" & sCalc_DriverScenarioPeriod													
					
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

#End Region

#Region "Otros"

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
			
			'BRApi.ErrorLog.LogMessage(si,"Entra2")
		
			api.Data.ClearCalculatedData("A#CR_Q3",True,True,True)
			
			'Rule 001
			api.Data.Calculate("A#CR_Q3:U3#8080:U6#Allocation_IN:U5#Rule_001 = (A#Calc_CRJ:U3#8080:U6#Allocation_IN:U5#Rule_001 * (-1)) - A#623200:U3#8080:U6#Allocation_IN:U5#Rule_001 + A#682100:U3#8080:U6#Allocation_IN:U5#Rule_001",True)		
			api.Data.Calculate("A#CR_Q3:U3#8080:U6#Allocation_IN:U5#Rule_001:I#None = A#CR_Q3:U3#8080:I#None:U6#Allocation_IN:U5#Rule_001 + A#681100:U3#8080:I#None:U6#Allocation_IN:U5#Rule_001 + A#680100:U3#8080:I#None:U6#Allocation_IN:U5#Rule_001 + A#682100:U3#8080:I#None:U6#Allocation_IN:U5#Rule_001",True)
		
			'Rule 000
			api.Data.Calculate("A#CR_Q3:U3#8080:U6#Allocation_IN:U5#Rule_000 = (A#Calc_CRJ:U3#8080:U6#Allocation_IN:U5#Rule_000 * (-1)) - A#623200:U3#8080:U6#Allocation_IN:U5#Rule_000 + A#682100:U3#8080:U6#Allocation_IN:U5#Rule_000",True)		
			api.Data.Calculate("A#CR_Q3:U3#8080:U6#Allocation_IN:U5#Rule_000:I#None = A#CR_Q3:U3#8080:I#None:U6#Allocation_IN:U5#Rule_000 + A#681100:U3#8080:I#None:U6#Allocation_IN:U5#Rule_000 + A#680100:U3#8080:I#None:U6#Allocation_IN:U5#Rule_000 + A#682100:U3#8080:I#None:U6#Allocation_IN:U5#Rule_000",True)
									
			
		End Sub			
			
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