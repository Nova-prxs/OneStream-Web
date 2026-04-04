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

Namespace OneStream.BusinessRule.DashboardStringFunction.CFX_ReportingHelper
	Public Class MainClass
'------------------------------------------------------------------------------------------------------------
'Reference Code: 		CFX_ReportingHelper
'
'Description:			Used to return a "TRUE" for each CF member with data on it (Reporting Book, do not display emtpy CVs) 
'
'Use Examples:	        XFBR(CFX_ReportingHelper, checkValue, UD1Member=|Loop1Variable|,ScenarioMember=|!Scenario_Members_CFX!|, _ 
'							 TimeMember=|!Time_Members_CFX!|,EntityMember=|!Entity_Members_CFX!|,ConsMember=|!Cons_Members_CFX!|)
'
'Created By:			Henning Windhagen
'
'Date Created:			07-09-2020
'------------------------------------------------------------------------------------------------------------	
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				
				'initialize VariableRepository
				Dim rep As New OneStream.BusinessRule.Finance.VariableRepository.MainClass
				
				If args.FunctionName.XFEqualsIgnoreCase("checkValue") Then
					
					'get POV members
					Dim cfMember As String = args.NameValuePairs.XFGetValue("UD1Member")
					globals.SetStringValue("povScenario", args.NameValuePairs.XFGetValue("ScenarioMember"))
					globals.SetStringValue("povTime", args.NameValuePairs.XFGetValue("timeMember"))
					globals.SetStringValue("povEntity", args.NameValuePairs.XFGetValue("entityMember"))
					globals.SetStringValue("povParent", Me.GetParent(si, globals, args, rep, args.NameValuePairs.XFGetValue("entityMember")))
					globals.SetStringValue("povCons", args.NameValuePairs.XFGetValue("consMember"))

					'Create Global object to return True/False from (to hide empty cube views in report)
					Dim cfCheck As Boolean = Me.CashFlowCheck(si, globals, args, rep, cfMember)
'BRAPI.ErrorLog.LogMessage(si, "CHECK 1")
					'True/False
					Return cfCheck
					
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "Cash Flow Amount Check Boolean"
		Public Function CashFlowCheck(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal args As DashboardStringFunctionArgs, ByVal rep As Object, ByVal cfMember As String) As Boolean
					
			Try
				
				'Helper method to check if a certain Cash Flow member has data to return a boolean to suppress cube view if False
				'------------------------------------------------
				Dim oCFCheck As Dictionary(Of String, Boolean) 'dictionary that holds the return key and value
				Dim bCFValueCheck As Boolean
				Dim cfMemberId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD1, cfMember)
				
				'Get mapping from Global object 
				Dim oCFMap As Object = Me.mappingDictionary(si, globals, rep) 
				
				If BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, globals.GetStringValue("povEntity")).Equals(dimconstants.Unknown) Then
					Return False
				Else	
					Dim sGlobalObjectName As String = "CFMapCheck_" & globals.GetStringValue("povEntity") & "_" & globals.GetStringValue("povScenario") & "_" & globals.GetStringValue("povTime")  & "_" & globals.GetStringValue("povCons") 
					oCFCheck = globals.GetObject(sGlobalObjectName)

					If oCFCheck Is Nothing Then
						
						oCFCheck = New Dictionary(Of String, Boolean)
						
						'define source data buffer details
						Dim dataCubeId, dataEntityId, dataParentId, dataConsId, dataScenarioId, dataTimeId, dataViewId As Integer
						dataCubeId = BRApi.Finance.Cubes.GetCubeInfo(si, rep.mainReportingCube).Cube.CubeId
						dataEntityId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.entity, globals.GetStringValue("povEntity"))
						dataParentId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.entity, globals.GetStringValue("povParent"))
						dataConsId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Consolidation, globals.GetStringValue("povCons"))
						dataScenarioId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, globals.GetStringValue("povScenario")) 
						dataTimeId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, globals.GetStringValue("povTime"))
						dataViewId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.view, "YTD")
						
						'define cellPk as basis for data buffer
						Dim cellPk As New DataBufferCellPk()
						cellPk.AccountId = DimConstants.All
						cellPk.FlowId = DimConstants.All
						cellPk.OriginId = DimConstants.Top
						cellPk.IcId = DimConstants.Top
'						cellPk.UD1Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD1, rep.topUD1)
						cellPk.UD1Id = DimConstants.All 'all, ie. for each CF member, because CF adjs is potentially saved on each member individually ' --> changed for HUH
						cellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, rep.topUD2)
						cellPk.UD3Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD3, rep.topUD3)
						cellPk.UD4Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD4, rep.topUD4)
						cellPk.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, rep.topUD5)
						cellPk.UD6Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD6, rep.topUD6)
'						cellPk.UD7Id = DimConstants.All 'all, ie. for each CF member, because CF adjs is potentially saved on each member individually ' --> changed for HUH
						cellPk.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, rep.topUD7)
						cellPk.UD8Id = DimConstants.All 'all, ie. for each technical CF member, because CF adjs is saved here
												
						Dim objDataUnitPk As New DataUnitPk(dataCubeId, dataEntityId, dataParentId, dataConsId, dataScenarioId, dataTimeId)
						Dim objList As List(Of DataCell) = BRApi.Finance.Data.GetDataBufferDataCells(si, objDataUnitPk, dataViewId, cellPk, False, True)
						Dim dimensionPK As DimPk = BRApi.Finance.Dim.GetDimPk(si,rep.mainAccountDimName)
                    	Dim hierarchyToUse As String = rep.CFTopAccount
                   		Dim ancestorID As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, hierarchyToUse)
						Dim iTargetMember As Integer
						Dim targetID As Integer
												
						'loop through each data buffer cell and check for corresponding mapping 
						For Each oMyDataBufferCell As dataCell In objList
							If (Not oMyDataBufferCell.CellStatus.IsNoData) Then
								If BRApi.Finance.Members.IsDescendant(si, dimensionPK, ancestorID, oMyDataBufferCell.DataCellPk.AccountId, Nothing) Then
									
									'add exception for manually adjusted data and add this to the list, even though this is not derived from the CF mapping itself
									If BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD8, oMyDataBufferCell.DataCellPk.UD8Id) = "CashFlowAdjs" Then
										iTargetMember = oMyDataBufferCell.DataCellPk.UD1Id
									End If 
									
									'Check if Account-Flow combination of data point is in mapping; if YES, add to list
									If oCFMap.TryGetValue(oMyDataBufferCell.DataCellPk.AccountId & "_" & oMyDataBufferCell.DataCellPk.FlowId, targetID) Then
										iTargetMember = math.abs(targetID)
									End If
									
									'if  data <> 0 then add to dictionary to later return "True" based on this
									' --> we cannot jump to the next CF member, as this is a Global object and all data cells have to be looped through here for a complete dictionary
									If iTargetMember <> 0 Then 	
										'### Add value to target cash flow member ###
										If oMyDataBufferCell.CellStatus.IsRealData Then 'only add real data in order to have a correct CF also in planning scenarios
											If oCFCheck.TryGetValue(iTargetMember, bCFValueCheck) Then
												oCFCheck.item(iTargetMember) = bCFValueCheck
											Else
												oCFCheck.Add(iTargetMember,  bCFValueCheck)
											End If
										End If
									End If
								End If
							End If
						Next
						
						'set Global object with all CF accounts that have data on it, derived from the mapping
						globals.SetObject(sGlobalObjectName, oCFCheck)
						
					End If	
					
					'return result that is checked in the Global object with all CF accounts that have data on it
					Dim result As Boolean = False
					
					If oCFCheck.TryGetValue(cfMemberId, bCFValueCheck) Then
						Result = True
					End If
					
					Return result
					
				End If
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function		
#End Region

#Region "Create Global Mapping Object"
		Public Function mappingDictionary(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal rep As Object) As Object
			Try
				Dim sGlobalObjectName_Mapping As String = "CFMap" 
				Dim oCFMap As Dictionary(Of String, Integer) 'dictionary that holds the return key and value
				Dim dCFTargetID As Integer
				
				SyncLock globals
					oCFMap = globals.GetObject(sGlobalObjectName_Mapping)
					
					If oCFMap Is Nothing Then
						oCFMap = New Dictionary(Of String, Integer)

						Dim iTargetMember As Integer 
						Dim iSourceSign As Integer 
						Dim targetCombination As String										
	
						'### Retrieve rows from SQL annotation table ("CashFlowMapping") ###

						'get mapping table sql statement
						Dim sql As New Text.StringBuilder()
						sql.AppendLine("SELECT SourceAccount, SourceAccountID, ActiveAccount, SourceFlow, SourceFlowID, ActiveFlow, FlowInAccountConstraint, TargetCashFlow, TargetCashFlowID, Signage ")
						sql.AppendLine("FROM CashFlowMapping ")
						sql.AppendLine("WITH (NOLOCK) ")
						sql.AppendLine("WHERE ActiveAccount = 'TRUE' ")
						sql.AppendLine("AND ")
						sql.AppendLine("ActiveFlow = 'TRUE' ")
						sql.AppendLine("AND ")
						sql.AppendLine("FlowInAccountConstraint = 'TRUE' ")
						sql.AppendLine("AND ")
						sql.AppendLine("LEN(TargetCashFlow ) > 0 ") 'IS NOT NULL returned blank cells with data in the Signage column

						'define member script variables
						Dim sSourceAccount As String = String.Empty
						Dim sSourceFlow As String = String.Empty
						Dim sCashFlowTarget As String = String.Empty
						Dim sSignage As String = String.Empty
'						Dim iSourceAccount As Integer
'						Dim iSourceFlow As Integer
						Dim iCashFlowTargetID As Integer
						
						' ### Loop through SQL rows and add each mapping entry to dictionary

						Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						'execute SQL to create a data attachment data table for the POV data unit
							Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sql.ToString, True)
							If Not dt Is Nothing Then
									'loop through the records of the data table 
								For Each row As DataRow In dt.Rows
									iCashFlowTargetID = row("TargetCashFlowID")
									iTargetMember = CInt(row("TargetCashFlowID"))
									sSignage = row("Signage") & "1" 'assume +1 for non-entry
									iSourceSign = math.Sign(CInt(sSignage))
									targetCombination = row("SourceAccountID").ToString & "_" & row("SourceFlowID").ToString
									
									'Add the result and target combination to the mapping dictionary
									If oCFMap.TryGetValue(targetCombination, dCFTargetID) Then
										oCFMap.item(targetCombination) = iSourceSign * iTargetMember
									Else 
										oCFMap.Add(targetCombination,  iSourceSign * iTargetMember)	
									End If
								Next 
							End If
						End Using								
					End If

					globals.SetObject(sGlobalObjectName_Mapping, oCFMap)
				End SyncLock
				
				Return oCFMap
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "Get Parent"
		Public Function GetParent(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal args As DashboardStringFunctionArgs, ByVal rep As Object, ByVal entity As String) As String
					
			Try
				
				'declare main top entity
				Dim sMainEntity As String = rep.mainGroupEntity 
				
				'get user selected entity
				Dim userSelectedEntity As String = entity
				
				'only execute if entity is not = top entity 
				If userSelectedEntity = sMainEntity Then Return Nothing
					
				'convert entity name to entity member ID
				Dim entityID As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeID.Entity,userSelectedEntity)
				
				'define hierarchy in entity dimension to use by specifying the top parent of the hierarchy
				Dim hierarchyToUse As String = sMainEntity
				
				'get the ancestor ID
				Dim ancestorID As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity,hierarchyToUse)
				
				'update with entity dimension name to acquire the dimension primary key
				Dim dimensionPK As DimPk = BRApi.Finance.Dim.GetDimPk(si,rep.mainEntityDimName)
				
				'get list of possible parents
				Dim parents As List(Of Member) = BRApi.Finance.Members.GetParents(si,dimensionPK,entityID,False,Nothing)
				
				'for each possible parent, check if it's a descendant of the total parent. If so, return the entity.
				For Each parentMem As Member In parents						
					If BRApi.Finance.Members.IsDescendant(si,dimensionPK,ancestorID,parentMem.MemberId,Nothing) Then
						Return parentMem.Name
					End If					
				Next
				
				Return Nothing

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region
		
	End Class
End Namespace