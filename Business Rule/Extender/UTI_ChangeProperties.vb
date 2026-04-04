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

Namespace OneStream.BusinessRule.Extender.UTI_ChangeProperties
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						Dim paramFunctionType As String = args.NameValuePairs("AccountAdjustment")
						
						If paramFunctionType = "Account" Then
							
							Dim AccountList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account_Details", "A#BS.base, A#PL.base, A#Manual_AC,A#CF.base",True)
							
							For Each AccountInfo As MemberInfo In AccountList
								
								Dim AccountName As String = AccountInfo.Member.Name.ToString
								Me.UpdateAccountAdjustment(si, AccountName)

							Next
							
							
						End If
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#Region "Functions"
		Public Sub UpdateAccountAdjustment(ByVal si As SessionInfo, ByVal accountName As String)			
			'Control Switches
			Dim saveMember As Boolean = True 
			Dim saveProps As Boolean = True
			Dim saveDescs As Boolean = False
			Dim saveRelationship As Boolean = False
			Dim isNewMember As Boolean = False
			
			'Get account properties
			Dim accountMemberInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimType.Account.Id, accountName, True)
			Dim accountProp As AccountVMProperties = accountMemberInfo.GetAccountProperties()	
			
			' brapi.ErrorLog.LogMessage(si, accountProp.AdjustmentType.GetStoredValue(scenarioType.ScenarioType2.Id, -1).ToString)

			accountProp.AdjustmentType.SetStoredValue(scenarioType.ScenarioType2.Id, -1, 2)
			
			' Para eliminar la propiedad creada, descomentar la siguiente linea
'			accountProp.AdjustmentType.RemoveStoredPropertyItem(scenarioType.ScenarioType2.Id, -1)
			
			'Save entity properties	
			BRApi.Finance.MemberAdmin.SaveMemberInfo(si, accountMemberInfo, saveMember, saveProps, saveDescs, isNewMember)

		End Sub
#End Region

	End Class
End Namespace