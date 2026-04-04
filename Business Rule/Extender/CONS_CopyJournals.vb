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

Namespace OneStream.BusinessRule.Extender.CONS_CopyJournals
	
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			
			Try
				
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep					
						
						brapi.ErrorLog.LogMessage(si, "0. COMIENZO REGLA <Copy Journals>")
						
						Dim wfClusterPK As WorkflowUnitClusterPk = si.WorkflowClusterPk
						Dim profileName As String = BRApi.Workflow.Metadata.GetProfile(si, wfClusterPk.ProfileKey).Name
						Dim sTargetTime As String = TimeDimHelper.GetNameFromId(wfClusterPk.TimeKey)
						
						'Por seguridad sólo permitimos su ejecución para los meses de JULIO y NOVIEMBRE
						If sTargetTime.EndsWith("7") Or sTargetTime.EndsWith("11")

							'Subimos al workflow padre CONS_CORP
							Dim wfProfileParent As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetParent(si, profileName)						
							wfClusterPK.ProfileKey = wfProfileParent.ProfileKey						
							
							'1. Workflow CONS_CORP
							brapi.ErrorLog.LogMessage(si, "1. Profile: " & wfProfileParent.Name)
							CreateJournals(si, globals, api, args, wfClusterPK)
							
							'2. Recorremos el RESTO DE WORKFLOWS (CONS_E101, ...)
							wfClusterPK.ProfileKey = wfProfileParent.ProfileKey	
							Dim wfProfilesList As List(Of WorkflowProfileInfo)  = BRApi.Workflow.Metadata.GetRelatives(si, wfClusterPk, WorkflowProfileRelativeTypes.Siblings, WorkflowProfileTypes.AllProfiles)						
							For Each WfProfile As WorkflowProfileInfo In wfProfilesList
							brapi.ErrorLog.LogMessage(si, "2.Profile: " & WfProfile.Name)
								wfClusterPK.ProfileKey = WfProfile.ProfileKey
								CreateJournals(si, globals, api, args, wfClusterPK)
							Next
							
						End If
						
						Return Nothing	
						
				End Select

				Return Nothing
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

		Public Sub CreateJournals(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs, ByVal wfClusterPK As WorkflowUnitClusterPk)
			Try
				Dim profileName As String = BRApi.Workflow.Metadata.GetProfile(si, wfClusterPk.ProfileKey).Name & ".Journals"
				brapi.ErrorLog.LogMessage(si, "a.Profile: " & profileName)
				
				Dim sTargetTime As String = TimeDimHelper.GetNameFromId(wfClusterPk.TimeKey)
				Dim sSourceTime As String = TimeDimHelper.GetNameFromId(brapi.Finance.Time.GetPriorPeriod(si,si.WorkflowClusterPk.TimeKey))				
				Dim scenarioName As String = ScenarioDimHelper.GetNameFromID(si, wfClusterPk.ScenarioKey)
					
				Dim bPreOrPost As Boolean = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False,"StoredSettingIdentifierPosition_JRFT").XFEqualsIgnoreCase("Pre")
				Dim sRollForewardID As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False,"StoredSettingIdentifier_JRFT")
				Dim bRevPreOrPost As Boolean = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False,"StoredSettingRevIdentifierPosition_JRFT").XFEqualsIgnoreCase("Pre")
				Dim sRevID As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False,"StoredSettingRevIdentifier_JRFT")
				Dim iAutoPost As Integer = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False,"StoredSettingAutoPost_JRFT").XFConvertToInt				
				Dim cubeName As String = BRApi.Workflow.Metadata.GetProfile(si, wfClusterPk.ProfileKey).CubeName
				
				brapi.ErrorLog.LogMessage(si, "Profile:  " & profileName & "/ Scenario: " & scenarioName & "/ sourceTime: " & sSourceTime & "/ targetTime: " &  sTargetTime)
								
				'Set Calc Time
				Dim calcTime As DateTime = DateTime.Now
				Dim lNewJournalsList As New List(Of String)
				
				'Create DBConn Infos (Only Used for EngineIfStatement Expression Processing)
				Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
					
					Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)						
						
						Dim wfListaPK As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, profileName, scenarioName, sTargetTime)
						wfclusterpk.ProfileKey = wfListaPK.ProfileKey							
					
						'Get the Plan DataTable
						Using dtJournals As DataTable = GetJournalsList(si, wfClusterPK, profileName, scenarioName, sTargetTime, False, False, False, True)										
						'Using dtJournals As DataTable = Me.GetJournalRollforwardTable(si, wfClusterPk)
						
							Dim wfTargetPK As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, profileName, scenarioName, sTargetTime)
							wfclusterpk.TimeKey = wfTargetPK.TimeKey			
						
							'*** CALC PLAN  (Multi-Threaded)***
'							Dim sOldRegID As String = "xxx"
							Dim sOldTargetName As String = "xxx"
							Dim strPostfix As String = "_" & brapi.Workflow.Metadata.GetProfile(si, wfClusterPK.ProfileKey).Name
							strPostfix = strPostfix &  "_" & brapi.Finance.Metadata.GetMember(si, dimtype.Scenario.Id, wfClusterPK.ScenarioKey).Member.Name
							strPostfix = strPostfix &  "_" & brapi.Finance.Metadata.GetMember(si, dimtype.Time.Id, wfClusterPK.TimeKey).Member.Name
							
							For Each drJournal As DataRow In dtJournals.Rows
								
								Dim sName As String = drjournal("Name")
								Dim oOldJournal As JournalEx = BRApi.Journals.Metadata.GetJournalOrTemplate(si, sName)
								
								Dim oNewJournalHeader As JournalHeaderEx
								Dim oNewJournalLineItems As List(Of JournalLineItem)
								Dim bHasRows As Boolean = False
								
								brapi.ErrorLog.LogMessage(si, "Test " & profileName)

								oNewJournalLineItems = New List(Of JournalLineItem)
								oNewJournalHeader = New JournalHeaderEx(oOldJournal.Header)
						
								oNewJournalHeader.CubeName = oOldJournal.Header.CubeName
								
								bHasRows = True
								
								With oNewJournalHeader.Header
								
									.CanChangeJournalAmounts = oOldJournal.Header.Header.CanChangeJournalAmounts
									.CanChangeJournalLineItems = oOldJournal.Header.Header.CanChangeJournalLineItems
									.CanChangeJournalPov = oOldJournal.Header.Header.CanChangeJournalPov
									.CanChangeJournalSettings = oOldJournal.Header.Header.CanChangeJournalSettings
									.OriginatingTemplateID	= Nothing									
									.JournalType = oOldJournal.Header.Header.JournalType
									.MemberIds.Scenario = wfClusterPK.ScenarioKey
									.MemberIds.Time = wfClusterPK.TimeKey									
									.UniqueID = Guid.NewGuid() 									
									.WorkflowProfileID = wfClusterPK.ProfileKey
									.Name = sTargetTime & "_" & sName.Replace(sSourceTime & "_","")						
									.Description = oOldJournal.Header.Header.Description
									.IsSingleEntity = False 
									.JournalBalanceType = oOldJournal.Header.Header.JournalBalanceType
									.IsTemplate = oOldJournal.Header.Header.IsTemplate
									.JournalStatus = oOldJournal.Header.Header.JournalStatus					
									
									oNewJournalHeader.header.MemberIds.Consolidation = oOldJournal.Header.Header.MemberIds.Consolidation

								End With

								Dim oNewJournal As journal

								For Each oOldLineItem As JournalLineItemEx In oOldJournal.LineItems
									Dim oNewlineItem As New JournalLineItemEx(oOldLineItem)
									
									If Not oOldLineItem.LineItem.Description.Contains(sRollForewardID) Then 
										'oNewLineItem.LineItem.Description = sRollForewardID & " (" & sName & "): " & oOldLineItem.LineItem.Description & " (" & brapi.Finance.Members.GetMemberName(si, dimtypeid.Account,oOldLineItem.LineItem.AccountId)  & " / " & brapi.Finance.Members.GetMemberName(si, dimtypeid.Flow,oOldLineItem.LineItem.FlowId) & ")"									
										oNewLineItem.LineItem.Description = oOldLineItem.LineItem.Description	
									End If

									If (oNewlineItem.LineItem.CubeId = ConsMemberId.Unknown) Then
										oNewlineItem.LineItem.CubeId = oOldJournal.Header.Header.CubeId									
									End If
									
									If (oNewlineItem.LineItem.EntityId = ConsMemberId.Unknown) Then
										oNewlineItem.LineItem.EntityId = oOldJournal.Header.Header.MemberIds.Entity
									End If

									If (oNewlineItem.LineItem.ParentId = ConsMemberId.Unknown) Then
										oNewlineItem.LineItem.ParentId = oOldJournal.Header.Header.MemberIds.Parent
									End If
									
									If (oNewlineItem.LineItem.accountId = ConsMemberId.Unknown) Then
										oNewlineItem.LineItem.accountId = oOldJournal.Header.Header.MemberIds.Account
									End If
									
									If (oNewlineItem.LineItem.flowId = ConsMemberId.Unknown) Then
										oNewlineItem.LineItem.flowId = oOldJournal.Header.Header.MemberIds.Flow
									End If
																		
									'If (oNewlineItem.LineItem.ICId = ConsMemberId.Unknown) Then
										'oNewlineItem.LineItem.ICId = oOldJournal.Header.Header.MemberIds.IC
									'End If									
									
									If (oNewlineItem.LineItem.UD1Id = ConsMemberId.Unknown) Then
										oNewlineItem.LineItem.UD1Id = oOldJournal.Header.Header.MemberIds.UD1
									End If

									If (oNewlineItem.LineItem.UD2Id = ConsMemberId.Unknown) Then
										oNewlineItem.LineItem.UD2Id = oOldJournal.Header.Header.MemberIds.UD2
									End If

									If (oNewlineItem.LineItem.UD3Id = Nothing) Then
										oNewlineItem.LineItem.UD3Id = oOldJournal.Header.Header.MemberIds.UD3
									End If

									If (oNewlineItem.LineItem.UD4Id = Nothing) Then
										oNewlineItem.LineItem.UD4Id = oOldJournal.Header.Header.MemberIds.UD4
									End If

									If (oNewlineItem.LineItem.UD5Id = Nothing) Then
										oNewlineItem.LineItem.UD5Id = oOldJournal.Header.Header.MemberIds.UD5
									End If

									If (oNewlineItem.LineItem.UD6Id = Nothing) Then
										oNewlineItem.LineItem.UD6Id = oOldJournal.Header.Header.MemberIds.UD6
									End If

									If (oNewlineItem.LineItem.UD7Id = Nothing) Then
										oNewlineItem.LineItem.UD7Id = oOldJournal.Header.Header.MemberIds.UD7
									End If

									If (oNewlineItem.LineItem.UD8Id = Nothing) Then
										oNewlineItem.LineItem.UD8Id = oOldJournal.Header.Header.MemberIds.UD8
									End If
									
									'oNewlineItem.LineItem.Description = "JRF: " & oOldLineItem.LineItem.Description
									oNewjournalLineItems.Add(oNewlineItem.LineItem.GetCopyOfTemplateLineItemForNewJournal())
																			
								Next
								
								oNewJournal = New journal(oNewjournalheader.Header,oNewJournalLineItems) '= BRApi.Journals.Metadata.GetJournalOrTemplate(si, oNewJournalInfo.Name)							
								'brapi.ErrorLog.LogMessage(si, "End " & oNewjournalheader.Header.Name & ", " & _ 
								'										oNewjournalheader.Header.JournalStatus.ToString & ", " & _
								'										TimeDimHelper.GetNameFromId(wfClusterPk.TimeKey) & ", " & _
								'										TimeDimHelper.GetNameFromId(oNewjournalheader.Header.MemberIds.Time))

								BRApi.Journals.Metadata.SaveJournalOrTemplateUsingIds(si, oNewjournal, False, True)
								
								If Not lNewJournalsList.Exists(Function(x) x.Equals(oNewjournalheader.Header.Name)) Then
									lNewJournalsList.Add(oNewjournalheader.Header.Name)
								End If
							'Exit For	
							Next

							For Each sAutoJournal As String In lNewJournalsList
								Dim oAutoJournal As JournalEx = BRApi.Journals.Metadata.GetJournalOrTemplate(si, sAutoJournal)								
								brapi.ErrorLog.LogMessage(si, "Journal Name: " & oAutoJournal.Header.Header.Description)
								If oAutoJournal.Header.Header.Name.StartsWith(sTargetTime & "_" ) Then									
									If iAutopost >= 1 Then Brapi.Journals.Process.ExecuteSubmit(si, oAutoJournal.Header.Header.UniqueID)
									If iAutopost >= 2 Then Brapi.Journals.Process.ExecuteApprove(si, oAutoJournal.Header.Header.UniqueID)
									If iAutopost = 3 Then Brapi.Journals.Process.ExecutePost(si, oAutoJournal.Header.Header.UniqueID)
								End If 
							Next
							
						End Using
					End Using	
				End Using
				
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try		
		End Sub	
		
		Private Function GetJournalsList(ByVal si As SessionInfo, ByVal wfClusterPk As WorkflowUnitClusterPk, ByVal strWFProfile As String, ByVal strWFScenario As String, ByVal strWFTime As String, Optional ByVal bWorking As Boolean = True, Optional ByVal bSubmit As Boolean = True, Optional ByVal bApprove As Boolean = True, Optional ByVal bPost As Boolean = True) As DataTable
			Try

				'Create and fill it from the list of valid Substitution Variables
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "JournalList")
				Dim wfTargetPK As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, strWFProfile, strWFScenario, strWFTime)
				
				wfclusterpk.TimeKey = brapi.Finance.Time.GetPriorPeriod(si,si.WorkflowClusterPk.TimeKey)			
				wfclusterpk.ProfileKey = wfTargetPK.ProfileKey
				wfclusterpk.ScenarioKey = wfTargetPK.ScenarioKey
				
				Dim sTargetTime As String = TimeDimHelper.GetNameFromId(wfClusterPk.TimeKey)
				brapi.ErrorLog.LogMessage(si, sTargetTime)
				
				Dim journalsCollection As JournalsAndTemplatesForWorkflow = BRApi.Journals.Metadata.GetJournalsAndTemplates(si,wfClusterPK)
				
				Dim strPostfix As String
						
				brapi.ErrorLog.LogMessage(si, "1. GetJournalList")
				
				For Each oJournal As Journalsummaryinfo In journalsCollection.Journals
					
					brapi.ErrorLog.LogMessage(si, "2. GetJournalList")
					
					If bWorking And oJournal.JournalStatus.Equals(journalstatus.Working) Or _
					bWorking And oJournal.JournalStatus.Equals(journalstatus.Rejected) Or _
					bSubmit And oJournal.JournalStatus.Equals(journalstatus.Submitted) Or _
					bApprove And oJournal.JournalStatus.Equals(journalstatus.Approved) Or _
					bPost And oJournal.JournalStatus.Equals(journalstatus.Posted) Then
						
						strPostfix = "_" & brapi.Workflow.Metadata.GetProfile(si,wfClusterPK.ProfileKey).Name
						strPostfix = strPostfix &  "_" & brapi.Finance.Metadata.GetMember(si,dimtype.Scenario.Id, wfClusterPK.ScenarioKey).Member.Name
						strPostfix = strPostfix &  "_" & brapi.Finance.Metadata.GetMember(si, dimtype.Time.Id, wfClusterPK.TimeKey).Member.Name
						
						Me.WriteNameValuePairRow(si, dt, oJournal.Name.Replace(strPostfix, ""), oJournal.name)
						
						brapi.ErrorLog.LogMessage(si, strPostfix)
						
					End If
				Next
				
				Return dt 
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function				
		
#Region "DataTable Helpers"

		Private Function CreateNameValuePairTable(ByVal si As SessionInfo, ByVal dataTableName As String) As DataTable
			Try
				'Create the data table to return
				Dim dt As New DataTable(dataTableName)
				
				Dim objCol = New DataColumn
	            objCol.ColumnName = "Name"
	            objCol.DataType = GetType(String)
	            objCol.DefaultValue = ""
	            objCol.AllowDBNull = False
	            dt.Columns.Add(objCol)
				
				objCol = New DataColumn
				objCol.ColumnName = "Value"
				objCol.DataType = GetType(String)
	            objCol.DefaultValue = ""
	            objCol.AllowDBNull = False
	            dt.Columns.Add(objCol)
							
				Return dt
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function CreateNameUIDPairTable(ByVal si As SessionInfo, ByVal dataTableName As String) As DataTable
			Try
				'Create the data table to return
				Dim dt As New DataTable(dataTableName)
				
				Dim objCol = New DataColumn
	            objCol.ColumnName = "Name"
	            objCol.DataType = GetType(String)
	            objCol.DefaultValue = ""
	            objCol.AllowDBNull = False
	            dt.Columns.Add(objCol)
				
				objCol = New DataColumn
				objCol.ColumnName = "Value"
				objCol.DataType = GetType(Guid)
	            objCol.AllowDBNull = True
	            dt.Columns.Add(objCol)
							
				Return dt
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Sub WriteNameValuePairRow(ByVal si As SessionInfo, ByVal dt As DataTable, ByVal name As String, ByVal value As String)
			Try
	            'Create a new row and append it to the table
				Dim row As DataRow = dt.NewRow()

				row("Name") = name
				row("Value") = value
				
                dt.Rows.Add(row)
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub		
		
		Private Sub WriteNameUIDPairRow(ByVal si As SessionInfo, ByVal dt As DataTable, ByVal name As String, ByVal value As Guid)
			Try
	            'Create a new row and append it to the table
				Dim row As DataRow = dt.NewRow()

				row("Name") = name
				row("Value") = value
				
                dt.Rows.Add(row)
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub		
		
#End Region	

	End Class
	
End Namespace