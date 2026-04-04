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
 
Namespace OneStream.BusinessRule.Extender.UTI_CopyJournals
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					Case Is = ExtenderFunctionType.Unknown
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						' Obtener los parámetros
						Dim paramSourceScenario As String = args.NameValuePairs("p_source_scenario")
						Dim paramTargetScenario As String = args.NameValuePairs("p_target_scenario")
						'Dim paramWFProfile As String = args.NameValuePairs("p_wf_profile")
						'Dim paramTime As String = args.NameValuePairs("p_target_time")
						' Obtener el año y mes dinámicamente desde el TimeKey del Workflow
						'Dim timeKey As String = si.WorkflowClusterPk.TimeKey.ToString()
						Dim timeKey As String = args.NameValuePairs("p_target_time")
						Dim year As String = timeKey.Substring(0, 4) ' Primeros 4 caracteres (año)
						'Dim month As String = timeKey.Substring(4, 2) ' Los 2 caracteres siguientes (mes)
						
						Dim monthI As String 
						If (timeKey.Length = 7) Then	
							monthI = timeKey.Substring(5, 2) ' Los 2 caracteres siguientes (mes)
						Else
							 monthI = timeKey.Substring(5, 1) '
						End If
						Dim monthInt As Integer = Integer.Parse(monthI)
						Dim previousMonthInt As Integer
						BRApi.ErrorLog.LogMessage(si, "mes Str: " + monthI )

					    If monthInt = 1 Then
					        previousMonthInt = monthInt
					    Else
					        previousMonthInt = monthInt - 1
					    End If
						
						' Definir los meses específicos: enero, noviembre, diciembre
						Dim monthsToCopy As New List(Of String) From {previousMonthInt.ToString, monthInt.ToString}
						
						
						'Modificacion de dataEntry a jounal
						Dim AccountList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account_Details", "A#BS.base, A#PL.base, A#CF.base",True)
						
						For Each AccountInfo As MemberInfo In AccountList
							
							Dim AccountName As String = AccountInfo.Member.Name.ToString
							Me.UpdateAccountAdjustment(si, AccountName, 1)

						Next
							
						' Definir los meses específicos: enero, noviembre, diciembre
						'Dim monthsToCopy As New List(Of String) From {, month}
 						
						For Each paramWFProfile As String In {"WP_Horse_Admin", "WP_Horse Group"}
							' Define un listado de perfiles de flujo de trabajo
							Dim parentWfProfileInfo As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, paramWFProfile)
							Dim baseWfProfileInfoList As New List(Of WorkflowProfileInfo)( _
							    BRApi.Workflow.Metadata.GetRelatives( _
									si, _
							        BRApi.Workflow.General.GetWorkflowUnitClusterPk( _
										si, _
							            parentWfProfileInfo.Name, _
										paramSourceScenario, _
										BRApi.Finance.Members.GetMemberName(si, dimTypeId.Time, si.WorkflowClusterPk.TimeKey)), _
								        WorkflowProfileRelativeTypes.Descendants, _
								        WorkflowProfileTypes.InputAdjChild _
									) _
								)
							
							' Bucle para cada perfil de flujo de trabajo
							For Each baseWfProfileInfo As WorkflowProfileInfo In baseWfProfileInfoList
								' Generar los TimeKeys para enero, noviembre y diciembre
								For Each monthToCopy As String In monthsToCopy
									Dim timeKeyForMonth As String = year & "M" & monthToCopy ' Concatenar año + mes
	 
									' Obtener el miembro de tiempo (TimeKey) para cada mes
									Dim timeKeyId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, timeKeyForMonth)
	 
									' Generar el WorkflowUnitPk para cada mes
									Dim targetWfUnitPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(
										si,
										BRApi.Workflow.Metadata.GetProfile(si, baseWfProfileInfo.ProfileKey).Name,
										paramTargetScenario,
										BRApi.Finance.Members.GetMemberName(si, dimTypeId.Time, timeKeyId)
									)
									Dim targetWfClusterPk As New WorkflowUnitClusterPk()
									targetWfClusterPk.ProfileKey = targetWfUnitPk.ProfileKey
									targetWfClusterPk.ScenarioKey = targetWfUnitPk.ScenarioKey
									targetWfClusterPk.TimeKey = targetWfUnitPk.TimeKey
									' Obtener los diarios
									Dim journalCollection As JournalsAndTemplatesForWorkflow = BRApi.Journals.Metadata.GetJournalsAndTemplates(si, targetWfClusterPk)
	 								'brapi.ErrorLog.LogMessage(si, "entra1")
									' Bucle para limpiar diarios antiguos
									For Each oldJournal As JournalSummaryInfo In journalCollection.Journals
										Dim oldJournalObject As JournalEx = BRApi.Journals.Metadata.GetJournalOrTemplate(si, oldJournal.Name)
										If oldJournalObject.Header.CanUnpost Then
											BRApi.Journals.Process.ExecuteUnpost(si, oldJournalObject.Header.Header.UniqueID)
											BRApi.Journals.Process.ExecuteReject(si, oldJournalObject.Header.Header.UniqueID)
										End If
										If oldJournalObject.Header.CanReject Then BRApi.Journals.Process.ExecuteReject(si, oldJournalObject.Header.Header.UniqueID)
										BRApi.Journals.Metadata.DeleteJournalOrTemplate(si, oldJournalObject.Header.Header.UniqueID)
									Next
									'brapi.ErrorLog.LogMessage(si, "entra2")
									' Generar el WorkflowUnitPk para el escenario fuente
									Dim sourceWfUnitPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(
										si,
										BRApi.Workflow.Metadata.GetProfile(si, baseWfProfileInfo.ProfileKey).Name,
										paramSourceScenario,
										BRApi.Finance.Members.GetMemberName(si, dimTypeId.Time, timeKeyId)
									)
									Dim sourceWfClusterPk As New WorkflowUnitClusterPk()
									sourceWfClusterPk.ProfileKey = sourceWfUnitPk.ProfileKey
									sourceWfClusterPk.ScenarioKey = sourceWfUnitPk.ScenarioKey
									sourceWfClusterPk.TimeKey = sourceWfUnitPk.TimeKey
									' Obtener los diarios
									journalCollection = BRApi.Journals.Metadata.GetJournalsAndTemplates(si, sourceWfClusterPk)
	 								
									' Copiar los diarios
									For Each oldJournal As JournalSummaryInfo In journalCollection.Journals
										Dim oldJournalObject As JournalEx = BRApi.Journals.Metadata.GetJournalOrTemplate(si, oldJournal.Name)
										If oldJournalObject.Header.Header.JournalStatus = JournalStatus.Posted Then
											' Crear una copia de la cabecera
											Dim newJournalHeader As New JournalHeader(oldJournalObject.Header.Header)
											newJournalHeader.Name = newJournalHeader.Name.Replace($"_{paramSourceScenario}_", $"_{paramTargetScenario}_")
											If Not newJournalHeader.Name.Contains($"_{paramTargetScenario}_") Then newJournalHeader.Name = $"{newJournalHeader.Name}_{paramTargetScenario}"
											If newJournalHeader.Name.Length > 100 Then newJournalHeader.Name = Right(newJournalHeader.Name, 100)
											newJournalHeader.UniqueID = Guid.NewGuid()
											newJournalHeader.MemberIds.Scenario = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, paramTargetScenario)
											' Crear la copia de las líneas del diario
											Dim newJournalLineItems As List(Of JournalLineItem) = oldJournalObject.LineItems.Select(Function(x) New JournalLineItem(x.LineItem)).ToList
											' Guardar el nuevo diario
											Dim newJournalObject As New Journal(newJournalHeader, newjournalLineItems)
											
											BRApi.Journals.Metadata.SaveJournalOrTemplateUsingIds(si, newJournalObject, False, True)
											' Publicar el diario
											BRApi.Journals.Process.ExecuteSubmit(si, newJournalHeader.UniqueID)
											BRApi.Journals.Process.ExecuteApprove(si, newJournalHeader.UniqueID)
											BRApi.Journals.Process.ExecutePost(si, newJournalHeader.UniqueID)
										End If
									Next
								Next
							Next
						Next
						
					
					'Modificacion de journal a dataentry	
					For Each AccountInfo As MemberInfo In AccountList
					
						Dim AccountName As String = AccountInfo.Member.Name.ToString
						Me.UpdateAccountAdjustment(si, AccountName, 2)

					Next
					
					
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
				End Select
 
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		
		
		
		Public Sub UpdateAccountAdjustment(ByVal si As SessionInfo, ByVal accountName As String, ByVal dataType As Integer)			
			'Control Switches
			Dim saveMember As Boolean = True 
			Dim saveProps As Boolean = True
			Dim saveDescs As Boolean = False
			Dim saveRelationship As Boolean = False
			Dim isNewMember As Boolean = False
			
			'Get account properties
			Dim accountMemberInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimType.Account.Id, accountName, True)
			Dim accountProp As AccountVMProperties = accountMemberInfo.GetAccountProperties()	
			
			' brapi.ErrorLog.LogMessage(si, accountProp.AdjustmentType.GetStoredValue(scenarioType.History.Id, -1).ToString)

			accountProp.AdjustmentType.SetStoredValue(scenarioType.ScenarioType2.Id, -1, dataType)
			
			' Para eliminar la propiedad creada, descomentar la siguiente linea
'			accountProp.AdjustmentType.RemoveStoredPropertyItem(scenarioType.History.Id, -1)
			
			'Save entity properties	
			BRApi.Finance.MemberAdmin.SaveMemberInfo(si, accountMemberInfo, saveMember, saveProps, saveDescs, isNewMember)

		End Sub
	End Class
End Namespace