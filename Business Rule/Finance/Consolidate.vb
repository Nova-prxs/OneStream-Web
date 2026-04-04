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

Namespace OneStream.BusinessRule.Finance.Consolidate
	Public Class MainClass
		
		#Region "Constants"
		'================================================================================
		' CONSTANTES DE CONFIGURACIÓN
		' Centralizar fechas y valores críticos para facilitar mantenimiento
		'================================================================================
		Private Const TURNING_YEAR As Integer = 2024
		Private Const TURNING_MONTH As Integer = 10
		Private Const BUDGET_CUTOFF_YEAR As Integer = 2025
		Private Const ENABLE_PERF_LOGGING As Boolean = False
		Private Const REGARDLESS_IC As String = "RegardlessIC"
		#End Region
		
		#Region "Cache Fields"
		'================================================================================
		' CAMPOS DE CACHÉ
		' Pre-computar valores costosos fuera de los loops de procesamiento
		'================================================================================
		' Caché de plug accounts por AccountId
		Private _plugAccountCache As Dictionary(Of Integer, Member)
		' Caché de textos de cuenta (Text1, Text2, Text5)
		Private _accountText1Cache As Dictionary(Of Integer, String)
		Private _accountText2Cache As Dictionary(Of Integer, String)
		Private _accountText5Cache As Dictionary(Of Integer, String)
		' Caché de TransformText para PlugAccounts_2024M10
		Private _transformCache As Dictionary(Of Integer, Integer)
		' Set de descendants válidos del grupo de consolidación
		Private _descendantsCache As HashSet(Of Integer)
		' Caché de member IDs frecuentemente usados
		Private _cachedMemberIds As Dictionary(Of String, Integer)
		' Caché de AccountType
		Private _accountTypeCache As Dictionary(Of Integer, AccountType)
		#End Region
		
		#Region "Main"
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					'================================================================================
					' MÉTODOS DE CONSOLIDACIÓN SOPORTADOS:
					' - FULLCONSOLIDATION: Consolidación completa
					' - HOLDING: Método de participación
					' - EQUITY: Método de puesta en equivalencia
					' - JV: Joint ventures (relationshiptext1 = "JV")
					' - NONCONTROLLINGINTEREST (NCI): Intereses minoritarios
					' - CUSTOM1: Entidades fusionadas
					' - CUSTOM5: Cambio de método o porcentaje durante el año
					'================================================================================
				
					Case Is = FinanceFunctionType.ConsolidateShare	
						' Verificar si la entidad está en uso antes de cualquier procesamiento
						If Not api.Entity.InUse(api.Pov.Entity.MemberId, api.Pov.Scenario.MemberId, api.Pov.Time.MemberId) Then Return Nothing
						api.Data.ClearCalculatedData(True, True, True, True)
						
						' Ejecutar consolidación de participación optimizada
						ConsolidateShareNormal_Optimized(si, api)
						
					Case Is = FinanceFunctionType.ConsolidateElimination
						' Verificar si la entidad está en uso antes de cualquier procesamiento
						If Not api.Entity.InUse(api.Pov.Entity.MemberId, api.Pov.Scenario.MemberId, api.Pov.Time.MemberId) Then Return Nothing
						api.Data.ClearCalculatedData(True, True, True, True)
						
						' Determinar si aplicar lógica de eliminación desde 2023
						Dim onlyfrom2023dataelim As Boolean = DetermineElimDataFilter(api)
						
						' Verificar método de consolidación
						Dim Method As String = UCase(api.Entity.OwnershipType(api.Pov.Entity.MemberId, api.Pov.Parent.MemberId, api.Pov.Scenario.MemberId, api.Pov.Time.MemberId).ToString)
						Dim strEnt_Text3 As String = api.Entity.Text(3)
						
						If Not Method.Equals("CUSTOM5", StringComparison.InvariantCultureIgnoreCase) AndAlso _
						   Not strEnt_Text3.Equals("DISPOSED", StringComparison.InvariantCultureIgnoreCase) Then
							' Inicializar cachés una sola vez
							InitializeCaches(si, api)
							' Ejecutar eliminaciones IC optimizadas
							IntercompanyElimination_Optimized(si, api, onlyfrom2023dataelim)
							' Procesar NCI P&L
							NCI_PL_Optimized(si, api)
						End If
						
				End Select
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		#End Region
		
		#Region "Cache Initialization"
		'================================================================================
		' INICIALIZACIÓN DE CACHÉS
		' Pre-cargar todos los metadatos necesarios antes del loop principal
		' Esto convierte operaciones O(n) en O(1) dentro del loop
		'================================================================================
		Private Sub InitializeCaches(ByVal si As SessionInfo, ByVal api As FinanceRulesApi)
			' Inicializar diccionarios
			_plugAccountCache = New Dictionary(Of Integer, Member)()
			_accountText1Cache = New Dictionary(Of Integer, String)()
			_accountText2Cache = New Dictionary(Of Integer, String)()
			_accountText5Cache = New Dictionary(Of Integer, String)()
			_transformCache = New Dictionary(Of Integer, Integer)()
			_descendantsCache = New HashSet(Of Integer)()
			_cachedMemberIds = New Dictionary(Of String, Integer)()
			_accountTypeCache = New Dictionary(Of Integer, AccountType)()
			
			' Pre-cachear member IDs frecuentemente usados
			CacheMemberId(api, "Flow", "F_OPE")
			CacheMemberId(api, "Flow", "F_NCI_Delta")
			CacheMemberId(api, "UD8", "OB")
			CacheMemberId(api, "UD8", "NCI_Elim")
			CacheMemberId(api, "Account", "630000")
			CacheMemberId(api, "Account", "0")
			
			' Pre-cargar descendants del grupo de consolidación para validación IC
			Dim entityDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Ent_100_Group")
			Dim parentId As Integer = api.Pov.Parent.MemberId
			Try
				Dim descendants As List(Of Member) = api.Members.GetDescendants(entityDimPk, parentId)
				For Each desc As Member In descendants
					_descendantsCache.Add(desc.MemberId)
				Next
			Catch
				' Si falla, continuar sin caché de descendants
			End Try
		End Sub
		
		Private Sub CacheMemberId(ByVal api As FinanceRulesApi, ByVal dimName As String, ByVal memberName As String)
			Dim key As String = dimName & ":" & memberName
			Dim dimTypeId As Integer
			Select Case dimName.ToUpper()
				Case "FLOW" : dimTypeId = DimType.Flow.Id
				Case "UD8" : dimTypeId = DimType.UD8.Id
				Case "ACCOUNT" : dimTypeId = DimType.Account.Id
				Case "IC" : dimTypeId = DimType.IC.Id
				Case Else : Return
			End Select
			_cachedMemberIds(key) = api.Members.GetMemberId(dimTypeId, memberName)
		End Sub
		
		Private Function GetCachedMemberId(ByVal dimName As String, ByVal memberName As String) As Integer
			Dim key As String = dimName & ":" & memberName
			If _cachedMemberIds IsNot Nothing AndAlso _cachedMemberIds.ContainsKey(key) Then
				Return _cachedMemberIds(key)
			End If
			Return -1
		End Function
		#End Region
		
		#Region "Helper Functions"
		'================================================================================
		' FUNCIONES AUXILIARES OPTIMIZADAS
		'================================================================================
		
		''' <summary>
		''' Determina si aplicar el filtro de datos de eliminación desde 2023
		''' </summary>
		Private Function DetermineElimDataFilter(ByVal api As FinanceRulesApi) As Boolean
			Dim scenarioName As String = api.Pov.Scenario.Name
			If scenarioName.XFEqualsIgnoreCase("Actual_at_op") OrElse scenarioName.XFEqualsIgnoreCase("Actual") Then
				If api.Time.GetYearFromId < TURNING_YEAR Then
					Return False
				End If
			End If
			Return True
		End Function
		
		''' <summary>
		''' Obtiene el plug account cacheado para una cuenta
		''' </summary>
		Private Function GetCachedPlugAccount(ByVal api As FinanceRulesApi, ByVal accountId As Integer) As Member
			If _plugAccountCache.ContainsKey(accountId) Then
				Return _plugAccountCache(accountId)
			End If
			Dim plugAccount As Member = api.Account.GetPlugAccount(accountId)
			_plugAccountCache(accountId) = plugAccount
			Return plugAccount
		End Function
		
		''' <summary>
		''' Obtiene el texto de cuenta cacheado (Text1, Text2 o Text5)
		''' </summary>
		Private Function GetCachedAccountText(ByVal api As FinanceRulesApi, ByVal accountId As Integer, ByVal textIndex As Integer) As String
			Dim cache As Dictionary(Of Integer, String) = Nothing
			Select Case textIndex
				Case 1 : cache = _accountText1Cache
				Case 2 : cache = _accountText2Cache
				Case 5 : cache = _accountText5Cache
				Case Else : Return api.Account.Text(accountId, textIndex)
			End Select
			
			If cache.ContainsKey(accountId) Then
				Return cache(accountId)
			End If
			Dim textValue As String = api.Account.Text(accountId, textIndex)
			cache(accountId) = textValue
			Return textValue
		End Function
		
		''' <summary>
		''' Obtiene el AccountType cacheado
		''' </summary>
		Private Function GetCachedAccountType(ByVal api As FinanceRulesApi, ByVal accountId As Integer) As AccountType
			If _accountTypeCache.ContainsKey(accountId) Then
				Return _accountTypeCache(accountId)
			End If
			Dim accType As AccountType = api.Account.GetAccountType(accountId)
			_accountTypeCache(accountId) = accType
			Return accType
		End Function
		
		''' <summary>
		''' Obtiene el plug account transformado cacheado para PlugAccounts_2024M10
		''' </summary>
		Private Function GetCachedTransformPlugAccount(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal accountId As Integer) As Integer
			If _transformCache.ContainsKey(accountId) Then
				Return _transformCache(accountId)
			End If
			
			Dim accountName As String = api.Members.GetMemberName(DimType.Account.Id, accountId)
			Dim transformedName As String = BRApi.Utilities.TransformText(si, accountName, "PlugAccounts_2024M10", False)
			Dim transformedId As Integer = api.Members.GetMemberId(DimType.Account.Id, transformedName)
			
			_transformCache(accountId) = transformedId
			Return transformedId
		End Function
		
		''' <summary>
		''' Valida si una celda es una eliminación IC válida
		''' </summary>
		Private Function IsValidICCell(ByVal api As FinanceRulesApi, ByVal accountId As Integer, ByVal icEntityId As Integer, _
									   ByVal plugAccount As Member, ByVal accountTxt5 As String) As Boolean
			' Verificar si la cuenta es IC
			If api.Account.IsIC(accountId) <> 1 Then Return False
			
			' Verificar si hay contraparte IC
			If icEntityId = DimConstants.None Then Return False
			
			' Verificar si el miembro IC es válido para la cuenta
			If Not api.Account.IsICMemberValidForAccount(accountId, icEntityId) Then Return False
			
			' Verificar si hay plug account válido
			If plugAccount Is Nothing OrElse plugAccount.IsUnknown() Then Return False
			
			' Verificar que la contraparte IC sea descendant del grupo
			' IMPORTANTE: Si el caché está vacío, rechazar (comportamiento conservador igual que original)
			If _descendantsCache.Count = 0 OrElse Not _descendantsCache.Contains(icEntityId) Then Return False
			
			' Verificar flag RegardlessIC
			If accountTxt5.XFEqualsIgnoreCase(REGARDLESS_IC) Then Return False
			
			Return True
		End Function
		
		''' <summary>
		''' Determina el plug account según el período y escenario
		''' </summary>
		Private Function DeterminePlugAccountByPeriod(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, _
													  ByVal accountId As Integer, ByVal basePlugId As Integer, _
													  ByVal currentYear As Integer, ByVal currentMonth As Integer, _
													  ByVal isBudget As Boolean, ByRef plugAccountOld As Integer) As Integer
			plugAccountOld = -1
			Dim transformedId As Integer
			
			If isBudget AndAlso currentYear <= BUDGET_CUTOFF_YEAR Then
				transformedId = GetCachedTransformPlugAccount(si, api, accountId)
				If transformedId <> -1 Then Return transformedId
			ElseIf currentYear < TURNING_YEAR OrElse (currentYear = TURNING_YEAR AndAlso currentMonth < TURNING_MONTH) Then
				transformedId = GetCachedTransformPlugAccount(si, api, accountId)
				If transformedId <> -1 Then
					plugAccountOld = basePlugId
					Return transformedId
				End If
			Else
				transformedId = GetCachedTransformPlugAccount(si, api, accountId)
				If transformedId <> -1 Then
					plugAccountOld = transformedId
				End If
			End If
			
			Return basePlugId
		End Function
		#End Region
		
		#Region "SetDataToDataBuffer Unified"
		'================================================================================
		' FUNCIÓN UNIFICADA PARA ESCRIBIR EN DATABUFFER
		' Consolida SetDataToDataBuffer, NCI_SetDataToDataBuffer_ToFlow, NCI_SetDataToDataBuffer_UD8_Specific
		'================================================================================
		Private Sub SetDataToDataBufferOptimized(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, _
				ByRef dataBuffer As DataBuffer, ByVal cell As DataBufferCell, ByVal amount As Decimal, _
				ByVal accountId As Integer, _
				Optional ByVal setToNone As Boolean = False, _
				Optional ByVal flowOverride As Integer = -1, _
				Optional ByVal ud8Override As Integer = -1)
			
			cell.DataBufferCellPk.AccountId = accountId
			cell.CellAmount = amount
			
			' Aplicar override de Flow si se especifica
			If flowOverride <> -1 Then
				cell.DataBufferCellPk.FlowId = flowOverride
			End If
			
			' Aplicar override de UD8 si se especifica
			If ud8Override <> -1 Then
				cell.DataBufferCellPk.UD8Id = ud8Override
			End If
			
			' Resetear dimensiones UD a None si se solicita
			If setToNone Then
				cell.DataBufferCellPk.UD1Id = DimConstants.None
				cell.DataBufferCellPk.UD2Id = DimConstants.None
				cell.DataBufferCellPk.UD3Id = DimConstants.None
				cell.DataBufferCellPk.UD4Id = DimConstants.None
				cell.DataBufferCellPk.UD5Id = DimConstants.None
				cell.DataBufferCellPk.UD6Id = DimConstants.None
				cell.DataBufferCellPk.UD7Id = DimConstants.None
			End If
			
			dataBuffer.SetCell(si, cell, True)
		End Sub
		#End Region
		
		#Region "ConsolidateShareNormal Optimized"
		'================================================================================
		' CONSOLIDATE SHARE - VERSIÓN OPTIMIZADA
		'================================================================================
		Private Sub ConsolidateShareNormal_Optimized(ByVal si As SessionInfo, ByVal api As FinanceRulesApi)
			Try
				' Pre-computar valores fuera del loop
				Dim myTimeMember As Integer = api.Pov.Time.MemberPk.MemberId
				Dim myTimeMemberLY As Integer = api.Time.GetLastPeriodInPriorYear(myTimeMember)
				Dim myScenarioTypeMember As Integer = api.Pov.ScenarioTypeId
				Dim ActiveEntity As Boolean = api.Entity.InUse(api.Pov.Entity.MemberId, myScenarioTypeMember, myTimeMember)
				
				' Early exit si entidad no está activa
				If Not ActiveEntity Then Return
				
				' Calcular porcentajes de consolidación
				Dim PCon As Decimal = CDec(1/100) * api.Entity.PercentConsolidation(api.Pov.Entity.MemberId, api.Pov.Parent.MemberId, myScenarioTypeMember, myTimeMember)
				
				' Early exit si porcentaje es 0
				If PCon = 0 Then Return
				
				Dim PConLY As Decimal = CDec(1/100) * api.Entity.PercentConsolidation(api.Pov.Entity.MemberId, api.Pov.Parent.MemberId, myScenarioTypeMember, myTimeMemberLY)
				
				' Pre-cachear member ID de F_OPE
				Dim F_OPE As Integer = api.Members.GetMemberId(DimType.Flow.Id, "F_OPE")
				
				' Determinar si aplicar mejora de performance
				Dim ImprovePerformance As Boolean = api.Pov.Parent.Name.XFEqualsIgnoreCase("SubCons_Affidea_Group_BV")
				
				' Obtener data buffer de origen
				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferForCustomShareCalculation(viewid:=DimConstants.Periodic)
				
				' Early exit si no hay datos
				If sourceDataBuffer Is Nothing OrElse sourceDataBuffer.DataBufferCells.Count = 0 Then Return
				
				Dim resultDataBuffer As DataBuffer = New DataBuffer()
				
				' Procesar cada celda
				For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
					' OPTIMIZACIÓN: Filtro temprano de celdas vacías
					If cell.CellAmount = 0 OrElse cell.CellStatus.IsNoData Then Continue For
					
					Dim shareCell As New DataBufferCell(cell)
					
					' Aplicar reducción de dimensiones para mejorar performance
					If ImprovePerformance Then
						shareCell.DataBufferCellPk.UD2Id = DimConstants.None
						shareCell.DataBufferCellPk.UD3Id = DimConstants.None
						shareCell.DataBufferCellPk.UD5Id = DimConstants.None
						shareCell.DataBufferCellPk.UD6Id = DimConstants.None
						shareCell.DataBufferCellPk.UD7Id = DimConstants.None
					End If
					
					' Aplicar porcentaje según tipo de flujo
					If shareCell.DataBufferCellPk.FlowId = F_OPE Then
						If PConLY <> 0 Then
							shareCell.CellAmount = cell.CellAmount * PConLY
						End If
					Else
						shareCell.CellAmount = cell.CellAmount * PCon
					End If
					
					resultDataBuffer.SetCell(si, shareCell, True)
				Next cell
				
				' Escribir resultado
				Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
				api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub
		#End Region
		
		#Region "IntercompanyElimination Optimized"
		'================================================================================
		' INTERCOMPANY ELIMINATION - VERSIÓN OPTIMIZADA
		'================================================================================
		Private Sub IntercompanyElimination_Optimized(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal onlyfrom2023dataelim As Boolean)
			Try
				' Pre-computar valores constantes fuera del loop
				Dim ConsolidationMethod As String = UCase(api.Entity.OwnershipType(api.Pov.Entity.MemberId, api.Pov.Parent.MemberId, api.Pov.Scenario.MemberId, api.Pov.Time.MemberId).ToString)
				Dim myTimeMember As Integer = api.Pov.Time.MemberId
				Dim currentYear As Integer = api.Time.GetYearFromId
				Dim currentMonth As Integer = api.Time.GetPeriodNumFromId
				
				' Obtener member IDs cacheados
				Dim F_OPE As Integer = GetCachedMemberId("Flow", "F_OPE")
				Dim intUD8_OB As Integer = GetCachedMemberId("UD8", "OB")
				Dim intNCI_Elim As Integer = GetCachedMemberId("UD8", "NCI_Elim")
				Dim intF_NCI_Delta As Integer = GetCachedMemberId("Flow", "F_NCI_Delta")
				Dim int630000 As Integer = GetCachedMemberId("Account", "630000")
				Dim int0Account As Integer = GetCachedMemberId("Account", "0")
				
				' Porcentajes de consolidación y ownership
				Dim PCON As Decimal = api.Entity.PercentConsolidation * CDec(1/100)
				Dim POWN As Decimal = api.Entity.PercentOwnership * CDec(1/100)
				Dim POWNLY As Decimal = api.Entity.PercentOwnership(api.Pov.Entity.MemberId,,, api.Time.GetLastPeriodInPriorYear) * CDec(1/100)
				
				' Flags de escenario
				Dim isBudget As Boolean = api.Pov.Scenario.Name.XFContainsIgnoreCase("Budget")
				Dim isHolding As Boolean = ConsolidationMethod.XFEqualsIgnoreCase("HOLDING")
				
				' Obtener data buffer de origen
				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferForCustomElimCalculation( _
					includeICNone := True, _
					includeICPartners := True, _
					combineImportFormsAndAdjConsolidatedIntoElim := True, _
					viewID := DimConstants.YTD)
				
				' Early exit si no hay datos
				If sourceDataBuffer Is Nothing OrElse sourceDataBuffer.DataBufferCells.Count = 0 Then Return
				
				Dim resultDataBuffer As DataBuffer = New DataBuffer()
				
				' Procesar cada celda
				For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
					Dim accountId As Integer = cell.DataBufferCellPk.AccountId
					Dim icEntityId As Integer = cell.DataBufferCellPk.ICId
					
					' Obtener plug account cacheado
					Dim plugAccount As Member = GetCachedPlugAccount(api, accountId)
					Dim plugAccountMemID As Integer = If(plugAccount IsNot Nothing, plugAccount.MemberId, -1)
					Dim plugAccountMemID_OLD As Integer = -1
					
					' Obtener textos de cuenta cacheados
					Dim strPlugtext1 As String = GetCachedAccountText(api, accountId, 1)
					Dim strtext2 As String = GetCachedAccountText(api, accountId, 2)
					Dim accountTxt5 As String = GetCachedAccountText(api, accountId, 5)
					
					' Determinar plug account según período
					plugAccountMemID = DeterminePlugAccountByPeriod(si, api, accountId, plugAccountMemID, currentYear, currentMonth, isBudget, plugAccountMemID_OLD)
					
					' Validar si es eliminación IC válida
					Dim IS_IC As Boolean = IsValidICCell(api, accountId, icEntityId, plugAccount, accountTxt5)
					
					' Determinar plug de Goodwill si aplica
					Dim intPlugtext1 As Integer = -1
					If intUD8_OB = cell.DataBufferCellPk.UD8Id Then
						If strPlugtext1.XFContainsIgnoreCase("#") AndAlso strPlugtext1.XFContainsIgnoreCase("Goodwill") Then
							Dim goodwillAccountName As String = strPlugtext1.Split("#"c)(1)
							intPlugtext1 = api.Members.GetMemberId(DimType.Account.Id, goodwillAccountName)
						ElseIf api.Members.IsBase(api.Pov.AccountDim.DimPk, int0Account, accountId) Then
							intPlugtext1 = int630000
						End If
						
						If intPlugtext1 <> -1 Then
							plugAccountMemID = intPlugtext1
						End If
					End If
					
					' Determinar cuenta NCI de eliminación
					Dim intTxt2Ac As Integer = -1
					If strtext2.XFContainsIgnoreCase("#") AndAlso strtext2.XFContainsIgnoreCase("NCI") Then
						Dim nciAccountName As String = strtext2.Split("#"c)(1)
						intTxt2Ac = api.Members.GetMemberId(DimType.Account.Id, nciAccountName)
					End If
					
					'====================================================================
					' LÓGICA DE ELIMINACIÓN
					'====================================================================
					
					' 1. ELIMINACIONES NCI (Non-Controlling Interest)
					If PCON <> POWN AndAlso intTxt2Ac <> -1 AndAlso onlyfrom2023dataelim Then
						ProcessNCIElimination(si, api, resultDataBuffer, cell, accountId, intTxt2Ac, _
											  plugAccountMemID, F_OPE, PCON, POWN, POWNLY, _
											  intNCI_Elim, intF_NCI_Delta, accountTxt5, isHolding, IS_IC)
						
					' 2. ELIMINACIONES FULL CONSOLIDATION (RegardlessIC)
					ElseIf accountTxt5.XFEqualsIgnoreCase(REGARDLESS_IC) AndAlso Not isHolding AndAlso plugAccountMemID <> -1 Then
						SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), -cell.CellAmount, accountId, False)
						SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), cell.CellAmount, plugAccountMemID, False)
						
					' 3. ELIMINACIONES IC SIN LÓGICA NCI
					ElseIf plugAccountMemID <> -1 AndAlso IS_IC Then
						SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), -cell.CellAmount, accountId, False)
						SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), cell.CellAmount, plugAccountMemID, True)
						
						' Limpiar plug account antiguo para 2024M10
						If plugAccountMemID_OLD <> -1 AndAlso api.Pov.Time.Name.XFEqualsIgnoreCase("2024M10") Then
							SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), 0, plugAccountMemID_OLD, True)
						End If
					End If
				Next cell
				
				' Escribir resultado
				Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#YTD")
				api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub
		
		''' <summary>
		''' Procesa las eliminaciones de NCI (Non-Controlling Interest)
		''' </summary>
		Private Sub ProcessNCIElimination(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, _
										  ByRef resultDataBuffer As DataBuffer, ByVal cell As DataBufferCell, _
										  ByVal accountId As Integer, ByVal intTxt2Ac As Integer, _
										  ByVal plugAccountMemID As Integer, ByVal F_OPE As Integer, _
										  ByVal PCON As Decimal, ByVal POWN As Decimal, ByVal POWNLY As Decimal, _
										  ByVal intNCI_Elim As Integer, ByVal intF_NCI_Delta As Integer, _
										  ByVal accountTxt5 As String, ByVal isHolding As Boolean, ByVal IS_IC As Boolean)
			
			Dim amount As Decimal = cell.CellAmount
			
			If cell.DataBufferCellPk.FlowId = F_OPE Then
				' Opening Balance: basado en POWNLY
				SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), -amount * (PCON - POWNLY), accountId, False, -1, intNCI_Elim)
				SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), amount * (PCON - POWNLY), intTxt2Ac, True, -1, intNCI_Elim)
				
				' Delta NCI si hay diferencia entre POWNLY y POWN
				If POWNLY - POWN <> 0 Then
					SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), -amount * (POWNLY - POWN), accountId, False, intF_NCI_Delta, intNCI_Elim)
					SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), amount * (POWNLY - POWN), intTxt2Ac, True, intF_NCI_Delta, intNCI_Elim)
				End If
			Else
				' Otros flujos: basado en diferencia PCON-POWN
				Dim accType As AccountType = GetCachedAccountType(api, accountId)
				
				If accType.ToString() = "Expense" Then
					SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), amount * (PCON - POWN), intTxt2Ac, True, -1, intNCI_Elim)
				Else
					SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), -amount * (PCON - POWN), accountId, False, -1, intNCI_Elim)
					SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), amount * (PCON - POWN), intTxt2Ac, True, -1, intNCI_Elim)
				End If
			End If
			
			' Full Consolidation para cuentas con RegardlessIC y NCI
			If accountTxt5.XFEqualsIgnoreCase(REGARDLESS_IC) AndAlso Not isHolding AndAlso plugAccountMemID <> -1 Then
				SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), -amount * POWN, accountId, False)
				SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), amount * POWN, plugAccountMemID, False)
			End If
			
			' Eliminación IC con NCI
			If IS_IC AndAlso plugAccountMemID <> -1 Then
				If cell.DataBufferCellPk.FlowId = F_OPE Then
					SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), -amount * (1 - (PCON - POWNLY)), accountId, False)
					SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), amount * (1 - (PCON - POWNLY)), plugAccountMemID, True)
				Else
					SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), -amount * (1 - (PCON - POWN)), accountId, False)
					SetDataToDataBufferOptimized(si, api, resultDataBuffer, New DataBufferCell(cell), amount * (1 - (PCON - POWN)), plugAccountMemID, True)
				End If
			End If
		End Sub
		#End Region
		
		#Region "NCI_PL Optimized"
		'================================================================================
		' NCI P&L - VERSIÓN OPTIMIZADA
		'================================================================================
		Private Sub NCI_PL_Optimized(ByVal si As SessionInfo, ByVal api As FinanceRulesApi)
			Try
				' Pre-computar valores fuera del loop
				Dim PCon As Decimal = CDec(1/100) * api.Entity.PercentConsolidation
				Dim POWN As Decimal = CDec(1/100) * api.Entity.PercentOwnership
				
				' Early exit si no hay diferencia
				If PCon = POWN Then Return
				
				Dim nciMultiplier As Decimal = POWN - PCon
				
				' Pre-cachear member ID de NCI_Elim
				Dim intNCI_Elim As Integer = GetCachedMemberId("UD8", "NCI_Elim")
				If intNCI_Elim = -1 Then
					intNCI_Elim = api.Members.GetMemberId(DimType.UD8.Id, "NCI_Elim")
				End If
				
				Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic:A#NCI_PL")
				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, "C#Share:V#Periodic:A#4_1", destinationInfo)
				
				' Early exit si no hay datos
				If sourceDataBuffer Is Nothing OrElse sourceDataBuffer.DataBufferCells.Count = 0 Then Return
				
				Dim resultDataBuffer As DataBuffer = New DataBuffer()
				
				For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
					cell.DataBufferCellPk.OriginId = DimConstants.OriginElimination
					cell.DataBufferCellPk.UD8Id = intNCI_Elim
					cell.CellAmount = cell.CellAmount * nciMultiplier
					resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
				Next
				
				api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub
		#End Region
		
	End Class
End Namespace