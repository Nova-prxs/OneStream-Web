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

#Region "Version Control"
	
	' ------------------------------------------------------------------------------------------------------------
	' Reference Code: 	PT_Export_CubeView_CONSO_simplificado.vb
	' 
	' Description:		Performance Tuning test for dynamic calc using FDX adapter for cube views
	' 
	' Created By:		inlumi
	' Date Created:		30-08-2021
	' Last Update:		30-08-2021
	' ------------------------------------------------------------------------------------------------------------
	' Function			: 		
	' Change			: 		
	' Comments			:              
	' Created By		:		inlumi
	' Date Created		:		30-08-2021
	' ------------------------------------------------------------------------------------------------------------


#End Region

#Region "Process Logic"
	' ------------------------------------------------------------------------------------------------------------
	' Use FDX adapters to export dynamic calc dada from Cube Views
	' 1) Set static filters for CVs
	' 2) Get list of dimension members to be processed
	' 3) Loop each combination of Entity-BU
	' 4) If there is data in IC-Cost Center intersection, extract data from 4 cube views in to 2 files (Step 5)
	'    Cube View TOP is used for this check
    ' 5) Export data from EUROS CVs (Management and Legal) to EUROS file
	' 6) Export data from LOCAL CVs (Management and Legal) to LOCAL file
	' ------------------------------------------------------------------------------------------------------------

#End Region

#Region "Code Best Practices"
	' ------------------------------------------------------------------------------------------------------------
	' 1) Do not use hard coded values for Id (ex: cecoDimTypeId). There are APIs to get the internal ids
	' 2) List that can be dynamically defined by user (UNs) could be defined using member filters which can be 
	'	 then queried with API using method
	' ------------------------------------------------------------------------------------------------------------


#End Region

Namespace OneStream.BusinessRule.Extender.Export_CubeView_CONSO_simplificado
	Public Class MainClass
		
		#Region "class Variables"
			Private m_LogMsgDetail As New Text.StringBuilder
		#End Region
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ExtenderArgs) As Object 'ByVal api As Transformer
			' Log StringBuilder
			Dim logMsgEntityBu As New Text.StringBuilder
				
			Try
				Me.AppendMsgToLogDetail(si, "Declaring variables - Start")
			
				' *************************************
				' Declare Variables
				' *************************************
				#Region "General Variables"
				
				' DataTable objects
				Dim dt As New DataTable
				Dim dtTop As New DataTable
				
				#End Region
				
				' Cube 				
				Dim cubo As String = "CONSO"
				
				#Region "General Variables"
				
				' Cube Views (Performance Tuning)
				Dim cubeViewNameEuros As String = "CONS_Extract_Euros"
				Dim cubeViewNameLocal As String = "CONS_Extract_Local"				
				Dim cubeViewNameEurosLegal As String = "CONS_Extract_Euros_Legal"
				Dim cubeViewNameLocalLegal As String = "CONS_Extract_Local_Legal"
				Dim cubeViewNameTop As String = "CONS_Top"
				Me.AppendMsgToLogDetail(si, "-------------------------------------------")
				Me.AppendMsgToLogDetail(si, "Cube Views:")
				Me.AppendMsgToLogDetail(si, "- " & cubeViewNameEuros)	
				Me.AppendMsgToLogDetail(si, "- " & cubeViewNameLocal)	
				Me.AppendMsgToLogDetail(si, "- " & cubeViewNameEurosLegal)	
				Me.AppendMsgToLogDetail(si, "- " & cubeViewNameLocalLegal)	
				Me.AppendMsgToLogDetail(si, "- " & cubeViewNameTop)	
				
				#End Region				
				
				#Region "FDX Adapter Settings"
					
				Dim parallelQueryCount As Integer = 8
				Dim logStatistics As Boolean = False
				Me.AppendMsgToLogDetail(si, "-------------------------------------------")
				Me.AppendMsgToLogDetail(si, "FDX Settings:")
				Me.AppendMsgToLogDetail(si, "Parallel Query Count: " & parallelQueryCount)	
				Me.AppendMsgToLogDetail(si, "Log Statistics: " & logStatistics)					
				
				#End Region
				
				#Region "CSV Writer Settings"
					' Buffer size for Streamwriter WFP file
					Dim bufferSizeWfpWriter As Integer = 1000000
				#End Region
				
				#Region "Dashboard Parameters"
				
				Dim scenarioParameter As String = "Analytics_Scenario"
				Dim scenarioMember As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, scenarioParameter)
				Dim entityParameter As String = "Analytics_Entity"
				Dim currencyParameter As String = "Analytics_MonedaLocal"
				Dim icParameter As String = "Analytics_IC"
				Dim costCenterParameter As String = "Analytics_Ceco"		
				Dim buParameter As String = "Analytics_UN"
				Dim accountsDynParameter As String = "Analytics_CtasDinamicas"
				Dim accountsStoreParameter As String = "Analytics_CtasAlmacenadas"
				Dim accountsDynLegalParameter As String = "Analytics_CtasDinamicas_Legal"
				
				Me.AppendMsgToLogDetail(si, "-------------------------------------------")
				Me.AppendMsgToLogDetail(si, "Dashboard Parameter values (before process starts):")
				Me.AppendMsgToLogDetail(si, "Analytics_Scenario :" & BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, scenarioParameter))			
				Me.AppendMsgToLogDetail(si, "Analytics_Entity :" & BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, entityParameter))			
				Me.AppendMsgToLogDetail(si, "Analytics_MonedaLocal :" & BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, currencyParameter))			
				Me.AppendMsgToLogDetail(si, "Analytics_IC :" & BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, icParameter))			
				Me.AppendMsgToLogDetail(si, "Analytics_Ceco :" & BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, costCenterParameter))			
				Me.AppendMsgToLogDetail(si, "Analytics_UN :" & BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, buParameter))			
				Me.AppendMsgToLogDetail(si, "Analytics_CtasDinamicas :" & BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, accountsDynParameter))			
				Me.AppendMsgToLogDetail(si, "Analytics_CtasDinamicas_Legal :" & BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, accountsDynLegalParameter))			
				Me.AppendMsgToLogDetail(si, "Analytics_CtasAlmacenadas :" & BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, accountsStoreParameter))					
				
				#End Region
				
				#Region "Set Dimension Filters"
				
				' *************************************
				' Set dimensions and filters for CV
				' *************************************
				
				' Dimension names
				Dim accountDimName As String = "CONS_Accounts"
				Dim scenarioDimName As String = "CONS_Scenarios"
				Dim entityDimName As String = "Entities"
				Dim costCenterDimName As String = "CONS_CostCenters"
				Dim buDimName As String = "CONS_BusinessUnits"
				
				' Scenario				
				Dim scenarioMemFilter As String = "R"
				
				' Entity (sociedad)				
				Dim entityMemFilter As String = "E#E101"
				
				' IC
				Dim icTopMember As String = "Top" '"LEGAL"
				
				' Time
				Dim timeMemFilter As String = "2020M3"
				
				' Log					
				Me.AppendMsgToLogDetail(si, "-------------------------------------------")
				Me.AppendMsgToLogDetail(si, "Initial CV Filters:")
				Me.AppendMsgToLogDetail(si, "Scenario:" & scenarioMemFilter)
				Me.AppendMsgToLogDetail(si, "Entity:" & entityMemFilter)				
				Me.AppendMsgToLogDetail(si, "Time:" & timeMemFilter)
				Me.AppendMsgToLogDetail(si, "ICP Top Member:" & icTopMember)
				Me.AppendMsgToLogDetail(si, "Declaring variables - End")
				Me.AppendMsgToLogDetail(si, "-------------------------------------------")
				Me.AppendMsgToLogDetail(si, "Building list of members:")
				
				' *************************************
				' Get list of Entities (Sociedades)
				' *************************************	
				Me.AppendMsgToLogDetail(si, "Entity List - Start")
				Dim entityTopMember As String = "LEGAL"
				Dim entityDimTypeId As Integer = 0
				Dim dimPkEntity As DimPk = BRApi.Finance.dim.GetDimPk(si, entityDimName)
				Dim parentIdEntity As Object = BRApi.Finance.Members.GetMemberId(si, entityDimTypeId, entityTopMember)
				' Get base members
				Dim listEntities = brapi.Finance.members.GetBaseMembers(si, dimPkEntity, parentIdEntity)
				' Add Top member
				listEntities.Add(BRApi.Finance.Members.GetMember(si, entityDimTypeId, entityTopMember))
				Me.AppendMsgToLogDetail(si, "Entity List: " & String.Join(", ", listEntities))
				Me.AppendMsgToLogDetail(si, "Entity List - End")
				
				' *************************************
				' Get list of Cost Centers (Cecos)		
				' *************************************
				Me.AppendMsgToLogDetail(si, "Cost Center List - Start")
				' UD3				
				Dim costCenterTopMember As String = "Top"
				Dim costCenterDimTypeId As Integer = 11						
				Dim dimPkCeco As DimPk = BRApi.Finance.dim.GetDimPk(si, costCenterDimName)
				Dim parentIdCeco As Object = BRApi.Finance.Members.GetMemberId(si, costCenterDimTypeId, costCenterTopMember)
				Dim listCostCenters = brapi.Finance.members.GetBaseMembers(si, dimPkCeco, parentIdCeco)				
				Me.AppendMsgToLogDetail(si, "Cost Center List: " & String.Join(", ", listCostCenters))
				Me.AppendMsgToLogDetail(si, "Cost Center List - End")
				
				' *************************************
				' Get list of Business Units (UNs)	
				' *************************************	
				Me.AppendMsgToLogDetail(si, "Business Unit List - Start")
				' UD1
				Dim buTopMember As String = "Top"
				Dim buDimTypeId As Integer = 9
				
				'Para añadir un nuevo UN hay que descomentar las lineas con *** e incluir el valor del buTopMember2
				Dim buTopMember2 As String = "ENTES"
				Dim buTopMember3 As String = "MC" '"SECTORES"
				'Dim buTopMember4 As String = "BEX"
				'TOP UD1
				Dim dimPkBU As DimPk = BRApi.Finance.dim.GetDimPk(si, buDimName)
				Dim parentIdBU As Object = BRApi.Finance.Members.GetMemberId(si, buDimTypeId, buTopMember)
				Dim listBusinessUnits = brapi.Finance.members.GetDescendants(si, dimPkBU, parentIdBU)
				listBusinessUnits.Add(BRApi.Finance.Members.GetMember(si, buDimTypeId, buTopMember))
				'ENTES
				Dim parentIdBU2 As Object = BRApi.Finance.Members.GetMemberId(si, buDimTypeId, buTopMember2)
				listBusinessUnits.addRange(brapi.Finance.members.GetChildren(si, dimPkBU, parentIdBU2))
				listBusinessUnits.Add(BRApi.Finance.Members.GetMember(si, buDimTypeId, buTopMember2))
				'SECTORES
				Dim parentIdBU3 As Object = BRApi.Finance.Members.GetMemberId(si, buDimTypeId, buTopMember3)
				listBusinessUnits.addRange(brapi.Finance.members.GetChildren(si, dimPkBU, parentIdBU3))
				listBusinessUnits.Add(BRApi.Finance.Members.GetMember(si, buDimTypeId, buTopMember3))
				'BEX
				'listBusinessUnits.Add(BRApi.Finance.Members.GetMember(si, buDimTypeId, buTopMember4))
				
				Me.AppendMsgToLogDetail(si, "Business Unit List: " & String.Join(", ", listBusinessUnits))
				Me.AppendMsgToLogDetail(si, "Business Unit List - End")
				
				' *************************************
				' Get list of Accounts
				' *************************************
				Me.AppendMsgToLogDetail(si, "Accounts List - Start")
				'INDICES+RH Dynamic and Stored	
				Dim accountDimTypeId As Integer = 5								
				Dim dimPkAccount As DimPk = BRApi.Finance.dim.GetDimPk(si, accountDimName)
				
				' Get parent account "INDICES"
				Dim indiceTopMember As String = "INDICES"
				' Add base members to list
				Dim parentIdIndice As Object = BRApi.Finance.Members.GetMemberId(si, accountDimTypeId, indiceTopMember)
				Dim listAccounts As List(Of Member) = brapi.Finance.members.GetBaseMembers(si, dimPkAccount, parentIdIndice)
				
				' Get parent account "RH"
				Dim rhTopMember As String = "RH"
				' Add base members to list
				Dim parentIdRh As Object = BRApi.Finance.Members.GetMemberId(si, accountDimTypeId, rhTopMember)				
				listAccounts.AddRange(brapi.Finance.Members.GetBaseMembers(si, dimPkAccount, parentIdRh))
				
				Me.AppendMsgToLogDetail(si, "Account List: " & String.Join(", ", listAccounts))
				Me.AppendMsgToLogDetail(si, "Account List - End")
				
				' Create empty lists
				Dim listDynamicCalcAccts As New List(Of Member)
				Dim listStoredAccts As New List(Of Member)
				Dim listDynamicCalcAcctsLegal As New List(Of Member)				
				
				Dim accountsDynString As New Text.StringBuilder()
				Dim accountsStoredString As New Text.StringBuilder()
				Dim accountsDynLegalString As New Text.StringBuilder()
				
				Me.AppendMsgToLogDetail(si, "Classify Accounts - Start")				
				
				' Loop accounts to build lists
				For Each member In listAccounts
					' List of Dynamic Accounts (Legal)
					If brapi.Finance.Account.GetAccountType(si, brapi.Finance.Members.GetMemberId(si, accountDimTypeId, member.Name)).Name = "DynamicCalc" And 
						( member.Name.StartsWith("BAL") Or member.Name.StartsWith("PM") )
					
						listDynamicCalcAcctsLegal.Add(member)
						accountsDynLegalString.Append (",A#" &member.Name)
					
					' List of Dynamic Accounts 
					ElseIf brapi.Finance.Account.GetAccountType(si, brapi.Finance.Members.GetMemberId(si, accountDimTypeId, member.Name)).Name = "DynamicCalc"
						listDynamicCalcAccts.Add(member)
						accountsDynString.Append (",A#" &member.Name)
						'system.IO.file.WriteAllText(filePath,",A#" &member.Name)
					' List of Stored Accounts
					Else
						listStoredAccts.Add(member)
						accountsStoredString.Append (",A#" &member.Name)
					End If
				Next
				
				Me.AppendMsgToLogDetail(si, "DynAccount Legal Account List: " & String.Join(", ", listDynamicCalcAcctsLegal))
				Me.AppendMsgToLogDetail(si, "DynAccount Other Account List: " & String.Join(", ", listDynamicCalcAccts))
				Me.AppendMsgToLogDetail(si, "Stored Account List: " & String.Join(", ", listStoredAccts))
				Me.AppendMsgToLogDetail(si, "Classify Accounts - End")	
				
				' Set list to parameter values
				'vk
				'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, accountsDynParameter, accountsDynString.ToString)
				brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, accountsStoreParameter, accountsStoredString.ToString)
				brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, accountsDynLegalParameter, accountsDynLegalString.ToString)
				
				Me.AppendMsgToLogDetail(si, "Dashboard Parameters Updated: ")
				Me.AppendMsgToLogDetail(si, accountsDynParameter & " : " & accountsDynString.ToString)
				Me.AppendMsgToLogDetail(si, accountsDynLegalParameter & " : " & accountsDynLegalString.ToString)
				Me.AppendMsgToLogDetail(si, accountsStoreParameter & " : " & accountsStoredString.ToString)
				
				#End Region
				
				#Region "Create Empty Files"
				
				' *************************************
				' Create Empty Files
				' *************************************
				Me.AppendMsgToLogDetail(si, "-------------------------------------------")
				Me.AppendMsgToLogDetail(si, "Creating files - Start")	
				
				' Prefix file names
				Dim filenamePrefixEuros As String = cubeViewNameEuros & "_" & scenarioMember & "_simplificado_"
				Dim filenamePrefixLocal As String = cubeViewNameLocal & "_" & scenarioMember & "_simplificado_"
				
				' Get DM export User name folder
				Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)
		        Dim folderPath As String = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName) & "\Analytics"
		        If Not Directory.Exists(folderPath) Then Directory.CreateDirectory(folderPath)
				
				' Full path for EUROS file
		        Dim filePathEuros As String = folderPath & "\" & filenamePrefixEuros & DateTime.UtcNow.ToString("yyyyMMdd") & ".csv"
				If File.Exists(filePathEuros) Then File.Delete(filePathEuros)	
				Me.AppendMsgToLogDetail(si, "Path for EUROS file: " & filePathEuros)	
				
				' Full path for LOCAL file
				Dim filePathLocal As String = folderPath & "\" & filenamePrefixLocal & DateTime.UtcNow.ToString("yyyyMMdd") & ".csv"
		        If File.Exists(filePathLocal) Then File.Delete(filePathLocal)	
				Me.AppendMsgToLogDetail(si, "Path for EUROS file: " & filePathEuros)	
								
				Dim csvStringEuros As New Text.StringBuilder()
				Dim csvStringLocal As New Text.StringBuilder()
				
				#End Region
				
				#Region "Export data from CV to CSV using FDX Adapter"
				
				' *************************************
				' Export Cube Views > Files
				' *************************************
				' Create one file for each Entity and Business Unit combination	
				' Loop the different dimensions
				' Entity > Business Unit
				
				' Counter
				Dim counter As Integer = 1
				
				
				' Loop Entity	
				For Each memberEntity In listEntities
					Me.AppendMsgToLogDetail(si, "*****************************************")
					Me.AppendMsgToLogDetail(si, String.Format("Processing Entity {0} - Start", memberEntity.Name))
					' ----------------------------
					' Set Dashboard parameters
					' ----------------------------
					' the cube view will use those params
					
					' Get local currency
					Dim localEntCurrency As String = BRApi.Finance.Entity.GetLocalCurrency(si, memberEntity.MemberId).Name
					Me.AppendMsgToLogDetail(si, "Local Currency: " & localEntCurrency)
					
					' Currency
					brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, currencyParameter, localEntCurrency)
					
					' Entity
					brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, entityParameter, memberEntity.Name)
					
					' IC	
					brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, icParameter, icTopMember)
					
					' Cost center
					brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, costCenterParameter, costCenterTopMember)
					
					Me.AppendMsgToLogDetail(si, "Dashboards Parameter Updated : ")
					Me.AppendMsgToLogDetail(si, currencyParameter & " : " &  BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, currencyParameter))
					Me.AppendMsgToLogDetail(si, entityParameter & " : " &  BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, entityParameter))
					Me.AppendMsgToLogDetail(si, icParameter & " : " &  BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, icParameter))
					Me.AppendMsgToLogDetail(si, costCenterParameter & " : "  & BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, costCenterParameter))
												
					' Business Unit	
					For Each memberBU In listBusinessUnits
						Me.AppendMsgToLogDetail(si, String.Format("BU {0} - Start", memberBU.Name))
						
						' Variable for time
						Dim dtStartExtractTop As DateTime
						Dim dtStartExtractEuros As DateTime
						Dim dtStartExtractEurosLegal As DateTime
						Dim dtStartExtractLocal As DateTime
						Dim dtStartExtractLocalLegal As DateTime
						Dim dtStartWriteEuros As DateTime
						Dim dtStartWriteEurosLegal As DateTime
						Dim dtStartWriteLocal As DateTime
						Dim dtStartWriteLocalLegal As DateTime
						Dim durationExtractTop As Integer = 0
						Dim durationExtractEuros As Integer = 0
						Dim durationExtractEurosLegal As Integer = 0
						Dim durationExtractLocal As Integer = 0
						Dim durationExtractLocalLegal As Integer = 0
						Dim durationWriteEuros As Integer = 0
						Dim durationWriteEurosLegal As Integer = 0
						Dim durationWriteLocal As Integer = 0
						Dim durationWriteLocalLegal As Integer = 0
						Dim numRecsEuros As Integer = 0
						Dim numRecsEurosLegal As Integer = 0
						Dim numRecsLocal As Integer = 0
						Dim numRecsLocalLegal As Integer = 0
					
						' ----------------------------
						' Extract CV TOP
						' ----------------------------	
						' This cube view is used as a trigger for the other CV extracts
						
						Dim buName As String = memberBU.Name
						' Set BU to dashboard parameter
						brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, buParameter, buName)
						Me.AppendMsgToLogDetail(si, "Dashboard Parameter updated: ")
						Me.AppendMsgToLogDetail(si, buParameter & " : "  & BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, buParameter))
						
						' FDX
						Me.AppendMsgToLogDetail(si, String.Format("FDX Extract {0} - Start", cubeViewNameTop))
						dtStartExtractTop = DateTime.Now
						dtTop = BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewNameTop, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, timeMemFilter, Nothing, True, False, Nothing, parallelQueryCount, logStatistics) 'nvbParams, includeCellTextCols, useStandardFactTableFields, filter
						durationExtractTop = GetDurationInSeconds(dtStartExtractTop)
						Me.AppendMsgToLogDetail(si, String.Format("FDX Extract {0} - End", cubeViewNameTop))
						
						' If there is data in IC-Cost CEnter combination, then we extract data from the other Cube Views
						' File EUROS
						' CV Euros Mgmt
						' CV Euros Legal
						' File LOCAL
						' CV Local Mgmt
						' CV Local Legal						
						
						If dtTop IsNot Nothing Then 
							#Region "CV Euros Mgmt"
							' ----------------------------
							' Extract CV EUROS Mgmt
							' ----------------------------	
							
							'csvstringEuros.Append("cruce top valido - Entity: " & memberEntity.Name & " - IC: " & memberIC.Name & " - Ceco: " & memberCeco.Name & " - UN: " & memberBU.Name  & vbCrLf)
							
							' Execute FDX
							Me.AppendMsgToLogDetail(si, String.Format("FDX Extract {0} - Start", cubeViewNameEuros))
							dtStartExtractEuros = DateTime.Now
							dt = BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewNameEuros, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, timeMemFilter, Nothing, True, False, Nothing, parallelQueryCount, logStatistics) 'nvbParams, includeCellTextCols, useStandardFactTableFields, filter
							durationExtractEuros = GetDurationInSeconds(dtStartExtractEuros)
							Me.AppendMsgToLogDetail(si, String.Format("FDX Extract {0} - End", cubeViewNameEuros))
							
							' DataTable to CSV File											
							If dt IsNot Nothing Then	
								Me.AppendMsgToLogDetail(si, String.Format("CSV Write {0} - Start", cubeViewNameEuros))
								numRecsEuros = dt.Rows.Count								
								dtStartWriteEuros = DateTime.Now
'								Dim csvRows As DataRow() = dt.select()   
								
'								'csvstring.Append (counter & " - " & nombreEntity & " - " & nombreCeco & vbCrLf)
								
'								For Each row As DataRow In csvRows
'									For Each element As Object In row.ItemArray 
'										csvstringEuros.Append (element.tostring & ",")
'									Next
'									csvstringEuros.Append (vbCrLf)
'								Next
					
'								system.IO.file.WriteAllText(filePathEuros,csvstringEuros.tostring)

								' =========================	
								' StreamWriter with Buffer
								' =========================
								Using sw As StreamWriter = New StreamWriter(filePathEuros, True, System.Text.Encoding.UTF8, bufferSizeWfpWriter)
								
									' Write lines to file -> Get lines from ItemArray
								    For Each row As DataRow In dt.Rows					        
								        sw.WriteLine(String.Join(",", row.ItemArray))
								        'sw.Flush()
								    Next
								End Using

								durationWriteEuros = GetDurationInSeconds(dtStartWriteEuros)
								Me.AppendMsgToLogDetail(si, String.Format("CSV Write {0} - End", cubeViewNameEuros))
			
							End If
							#End Region
							
							#Region "CV Euros Legal"
							' ----------------------------
							' Extract CV EUROS LEGAL
							' ----------------------------	
				
							'csvstringEuros.Append("cruce top valido - Entity: " & memberEntity.Name & " - IC: " & memberIC.Name & " - Ceco: " & memberCeco.Name & " - UN: " & memberBU.Name  & vbCrLf)
							
							' Execute FDX
							Me.AppendMsgToLogDetail(si, String.Format("FDX Extract {0} - Start", cubeViewNameEurosLegal))
							dtStartExtractEurosLegal = DateTime.Now
							dt = BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewNameEurosLegal, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, timeMemFilter, Nothing, True, False, Nothing, parallelQueryCount, logStatistics) 'nvbParams, includeCellTextCols, useStandardFactTableFields, filter
							durationExtractEurosLegal = GetDurationInSeconds(dtStartExtractEurosLegal)
							Me.AppendMsgToLogDetail(si, String.Format("FDX Extract {0} - End", cubeViewNameEurosLegal))
										
							' DataTable to CSV File		
							If dt IsNot Nothing Then	
								Me.AppendMsgToLogDetail(si, String.Format("CSV Write {0} - Start", cubeViewNameEurosLegal))
								numRecsEurosLegal = dt.Rows.Count	
								dtStartWriteEurosLegal = DateTime.Now
'								Dim csvRows As DataRow() = dt.select()   
								
'								'csvstring.Append (counter & " - " & nombreEntity & " - " & nombreCeco & vbCrLf)
								
'								For Each row As DataRow In csvRows
'									For Each element As Object In row.ItemArray 
'										csvstringEuros.Append (element.tostring & ",")
'									Next
'									csvstringEuros.Append (vbCrLf)
'								Next
					
'								system.IO.file.WriteAllText(filePathEuros,csvstringEuros.tostring)
								
								' =========================	
								' StreamWriter with Buffer
								' =========================
								Using sw As StreamWriter = New StreamWriter(filePathEuros, True, System.Text.Encoding.UTF8, bufferSizeWfpWriter)
								
									' Write lines to file -> Get lines from ItemArray
								    For Each row As DataRow In dt.Rows					        
								        sw.WriteLine(String.Join(",", row.ItemArray))
								        'sw.Flush()
								    Next
								End Using
								durationWriteEurosLegal = GetDurationInSeconds(dtStartWriteEurosLegal)
								Me.AppendMsgToLogDetail(si, String.Format("CSV Write {0} - End", cubeViewNameEurosLegal))
							End If							
							#End Region
							
							#Region "CV Local Mgmt"
							' ----------------------------
							' Extract CV LOCAL Mgmt
							' ----------------------------	
					
							'csvstringLocal.Append("cruce top valido - Entity: " & memberEntity.Name & " - IC: " & memberIC.Name & " - Ceco: " & memberCeco.Name & " - UN: " & memberBU.Name  & vbCrLf)
							
							' Execute FDX
							Me.AppendMsgToLogDetail(si, String.Format("FDX Extract {0} - Start", cubeViewNameLocal))
							dtStartExtractLocal = DateTime.Now
							dt = BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewNameLocal, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, timeMemFilter, Nothing, True, False, Nothing, parallelQueryCount, logStatistics) 'nvbParams, includeCellTextCols, useStandardFactTableFields, filter
							durationExtractLocal = GetDurationInSeconds(dtStartExtractLocal)
							Me.AppendMsgToLogDetail(si, String.Format("FDX Extract {0} - End", cubeViewNameLocal))
										
							' DataTable to CSV File		
							If dt IsNot Nothing Then	
								Me.AppendMsgToLogDetail(si, String.Format("CSV Write {0} - Start", cubeViewNameLocal))
								numRecsLocal = dt.Rows.Count	
								dtStartWriteLocal = DateTime.Now
'								Dim csvRows As DataRow() = dt.select()   
								
'								'csvstring.Append (counter & " - " & nombreEntity & " - " & nombreCeco & vbCrLf)
								
'								For Each row As DataRow In csvRows
'									For Each element As Object In row.ItemArray 
'										csvstringLocal.Append (element.tostring & ",")
'									Next
'									csvstringLocal.Append (vbCrLf)
'								Next
					
'								system.IO.file.WriteAllText(filePathLocal,csvstringLocal.tostring)
'								durationWriteLocal = GetDurationInSeconds(dtStartWriteLocal)
								
								' =========================	
								' StreamWriter with Buffer
								' =========================
								Using sw As StreamWriter = New StreamWriter(filePathLocal, True, System.Text.Encoding.UTF8, bufferSizeWfpWriter)
								
									' Write lines to file -> Get lines from ItemArray
								    For Each row As DataRow In dt.Rows					        
								        sw.WriteLine(String.Join(",", row.ItemArray))
								        'sw.Flush()
								    Next
								End Using

								Me.AppendMsgToLogDetail(si, String.Format("CSV Write {0} - End", cubeViewNameLocal))
							End If
							#End Region
							
							#Region "CV Local Legal"
							' ----------------------------
							' Extract CV LOCAL LEGAL
							' ----------------------------	
								
							'csvstringLocal.Append("cruce top valido - Entity: " & memberEntity.Name & " - IC: " & memberIC.Name & " - Ceco: " & memberCeco.Name & " - UN: " & memberBU.Name  & vbCrLf)
						
							' Execute FDX
							Me.AppendMsgToLogDetail(si, String.Format("FDX Extract {0} - Start", cubeViewNameLocalLegal))
							dtStartExtractLocalLegal = DateTime.Now
							dt = BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewNameLocalLegal, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, timeMemFilter, Nothing, True, False, Nothing, parallelQueryCount, logStatistics) 'nvbParams, includeCellTextCols, useStandardFactTableFields, filter
							durationExtractLocalLegal = GetDurationInSeconds(dtStartExtractLocalLegal)
							Me.AppendMsgToLogDetail(si, String.Format("FDX Extract {0} - End", cubeViewNameLocalLegal))
											
							' DataTable to CSV File		
							If dt IsNot Nothing Then	
								Me.AppendMsgToLogDetail(si, String.Format("CSV Write {0} - Start", cubeViewNameLocalLegal))
								numRecsLocalLegal = dt.Rows.Count	
								dtStartWriteLocalLegal = DateTime.Now
'								Dim csvRows As DataRow() = dt.select()   
								
'								'csvstring.Append (counter & " - " & nombreEntity & " - " & nombreCeco & vbCrLf)
								
'								For Each row As DataRow In csvRows
'									For Each element As Object In row.ItemArray 
'										csvstringLocal.Append (element.tostring & ",")
'									Next
'									csvstringLocal.Append (vbCrLf)
'								Next
					
'								system.IO.file.WriteAllText(filePathLocal,csvstringLocal.tostring)

								' =========================	
								' StreamWriter with Buffer
								' =========================
								Using sw As StreamWriter = New StreamWriter(filePathLocal, True, System.Text.Encoding.UTF8, bufferSizeWfpWriter)
								
									' Write lines to file -> Get lines from ItemArray
								    For Each row As DataRow In dt.Rows					        
								        sw.WriteLine(String.Join(",", row.ItemArray))
								        'sw.Flush()
								    Next
								End Using

								durationWriteLocal = GetDurationInSeconds(dtStartWriteLocalLegal)
								Me.AppendMsgToLogDetail(si, String.Format("CSV Write {0} - End", cubeViewNameLocalLegal))
							End If
							
							#End Region
							
							' Log Statistics
							logMsgEntityBu.AppendLine(String.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14}", memberEntity.Name, memberBU.Name,durationExtractTop, _
																													durationExtractEuros, durationWriteEuros, numRecsEuros, _
																													durationExtractEurosLegal, durationWriteEurosLegal, numRecsEurosLegal, _
																													durationExtractLocal, durationWriteLocal, numRecsLocal, _
																													durationExtractLocalLegal, durationWriteLocalLegal, numRecsLocalLegal _
																													))
																												
						Else
							logMsgEntityBu.AppendLine(String.Format("{0};{1};NO DATA", memberEntity.Name, memberBU.Name))
						End If
						
						Me.AppendMsgToLogDetail(si, String.Format("BU {0} - End", memberBU.Name))
					Next
					
					counter = counter + 1
					Me.AppendMsgToLogDetail(si, String.Format("Processing Entity {0} - End", memberEntity.Name))	
				Next 
				
				Me.AppendMsgToLogDetail(si, "Creating files - End")	
				
				#End Region
					
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			Finally
				#Region "Create Process Log"
				
				' Write final log
				Me.AppendMsgToLogDetail(si, "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~")
				Me.AppendMsgToLogDetail(si, "STATISTICS")
				Me.AppendMsgToLogDetail(si, "Entity;BusinessUnit;DurationExtractTop,DurationExtractEuros,DurationWriteEuros,NumRecordsEuros;DurationExtractEurosLegal,DurationWriteEurosLegal,NumRecordsEurosLegal;DurationExtractLocal,DurationWriteLocal,NumRecordsLocal;DurationExtractLocalLegal,DurationWriteLocalLegal,NumRecordsLocalLegal")
				Me.AppendMsgToLogDetail(si, logMsgEntityBu.ToString)
				IntegrationWriteLog(si, "Results for Dynamic Calc extract with FDX-Cube View (see below)")
				' Write to log file
				'Network File to create
				Dim logFolder As String = Path.Combine(FileShareFolderHelper.GetOutgoingFolderForApp(si, True, FileSystemHelper.GetOSFileShareRootPath(si), si.AppName), "Integration_Logs")
				' Create log folder if it does not exist
				If Not system.IO.Directory.Exists(logFolder) Then
					system.IO.Directory.CreateDirectory(logFolder)
				End If
				' Create log
				Dim logFile As String = Path.Combine(logFolder, "Integration_Log_" & DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) & ".txt")
				Dim textToWrite As String = "Execution results for Performance Tuning (see below)" & vbNewLine & Me.m_LogMsgDetail.ToString
				   
				If Not system.IO.File.Exists(logFile) Then
				    'Create network file
				    System.IO.File.Create(logFile).Close
				End If
				   
				'Write content to file
				System.IO.File.WriteAllText(logFile, textToWrite)
				
				#End Region
			End Try
		End Function
		
		#Region "Logger Helpers"
			Function GetDurationInSeconds(ByVal startTime As DateTime) As Integer
				Dim result As TimeSpan = DateTime.Now() - startTime
				Return result.TotalSeconds
				
			End Function
			' ------------------------------------------------------------------------------------------------------------
			' Reference Code:   IntegrationWriteLog
			' 
			' Description:		Log message in System Error Log
			' 
			' Logic:			Write a log entry with prefix "INTEGRATION HELPER"
			' 					Log entries will be logged if:
			' 					 * Global Debug Mode is enabled (Integration Helper)
			' 					 * Business Rule Debug Mode is enabled (Business Rule)
			' 					Fatal errors will always raise an exception
			' 
			' 					Global Debug Mode
			' 						Module variable m_GlobalDebugMode = True -> write log entries For all integration processes
			' 					Business Rule Debug Mode
			' 						Input parameter debugModeBr = True -> write log entries for BR calling the method even if 
			' 						Global Debug mode is disabled (False)
			' 
			' Notes:				
			' 
			' Created By:		inlumi
			' Date Created:		27-10-2020
			' ------------------------------------------------------------------------------------------------------------
			''' <summary>
			''' Log message in System Error Log
			''' </summary>
			''' <param name="msgSummary">Message summary displayed on the top</param>
			''' <param name="msgDetail">Message detail displayed on the bottom</param>
			''' <param name="msgType">Integer (Info=1, Debug=2, Error=3, Fatal=4, Warning = 5)</param>
			''' <param name="debugModeBr">Debug mode status for the BR calling the function (True/False)</param>
			''' <returns>N/A</returns>
			Public Sub IntegrationWriteLog(ByVal si As SessionInfo, ByVal msgSummary As String)				
								
				' =================================
				' Check DetailedL Log setting
				' =================================
				' Get environmental settings
				Dim xfEnvSettings As XFEnvironmentSettings = AppServerConfig.GetSettings(si).EnvironmentSettings
				Dim useDetailedErrorLogging As Boolean = xfEnvSettings.UseDetailedErrorLogging
				
				' =================================
				' Write log entry
				' =================================				
				' Log messages		
				If Me.m_LogMsgDetail IsNot Nothing AndAlso useDetailedErrorLogging Then
						BRAPi.ErrorLog.LogMessage(si, XFErrorLevel.Information, String.Format("EXTENDED LOGGER: {0}", msgSummary), String.Format("Message Details:{0}{1}", vbNewLine, Me.m_LogMsgDetail))			
				Else If Me.m_LogMsgDetail IsNot Nothing Then
					BRAPi.ErrorLog.LogMessage(si, XFErrorLevel.Information, String.Format("EXTENDED LOGGER: {0}", msgSummary) & vbNewLine & String.Format("Message Details:{0}{1}", vbNewLine, Me.m_LogMsgDetail))			
				Else
					' No Message Details
					BRAPi.ErrorLog.LogMessage(si, XFErrorLevel.Information, String.Format("EXTENDED LOGGER: {0}", msgSummary))			
				End If								
				
			End Sub  		

			
			' ------------------------------------------------------------------------------------------------------------
			' Reference Code: 	AppendMsgToLogDetail
			' 
			' Description:		Append line to string builder
			' 
			' Logic:						
			' 					
			' Notes:            
			' 
			' Created By:		inlumi
			' Date Created:		17-06-2021
			' ------------------------------------------------------------------------------------------------------------
			Public Sub AppendMsgToLogDetail(ByVal si As sessionInfo, ByVal msg As String)				
				Try
					Me.m_LogMsgDetail.AppendLine(DateTime.Now.ToString & vbTab & msg)
			
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(ex))
				End Try	
			End Sub			
			
		#End Region
	
	End Class
End Namespace