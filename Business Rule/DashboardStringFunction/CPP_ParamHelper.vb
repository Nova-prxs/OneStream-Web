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

Namespace OneStream.BusinessRule.DashboardStringFunction.CPP_ParamHelper
	Public Class MainClass
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 		CPP_ParamHelper
		'
		'Description:			Conditional Parameter helper functions
		'
		'Usage:					Used to provide conditional parameter processing functions that allow a parameter
		'						value to be interpreted and subtituted with a different string.
		'GetContentDescription
		'	Parameter Example:	BRString(CPP_ParamHelper, GetContentDescription, SelectedDashboard=|!SelectedContent_CPP!|)
		'
		'GetActivityClassName
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetActivityClassName, ClassID=|!SelectedClassID_CPP!|)
		'
		'GetActivityType
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetActivityType, ClassID=|!SelectedClassID_CPP!|)
		'
		'GetRegisterItemNames
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetRegisterItemNames, RegisterID=|!SelectedRegisterID_CPP!|,SecurityGroup=|!StoredViewEmpNameSecurity_CPPT!|, (OPTIONAL) TemplateType = False)
		'
		'GetCalcPlanFullName
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetCalcPlanFullName, CalcPlanID=|!SelectedCalcPlanID_CPP!|)
		'
		'GetColFmt
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetColFmt, Grp=Reg Col=1, RegID=All)
		'
		'GetStatusList
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetStatusList, Type=Base)
		'	Valid Types:		Base=Base List, Filter=List (With All Value), All= Complete List (With Exception)
		'
		'GetWFProfileColVisible
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetWFProfileColVisible, CurrentProfile=|WFProfile|,RegisterProfile=|!StoredPlanRegisterProfile_CPPT!|)
		'
		'GetWFProfileCriteria
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetWFProfileCriteria, CurrentProfile=|WFProfile|,RegisterProfile=|!StoredPlanRegisterProfile_CPPT!|)
		'
		'GetRegCriteria
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetRegCriteria, Status=|!StatusFilterList_CPP!|, RegID=|!SelectedRegisterIDDistinct_CPP!|)
		'
		'GetDeleteMessage Function
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetDeleteMessage, tRegisterID=|!SelectedRegisterID_CPP!|, deleteType=|!SelectedDeleteType_CPP!|)
		'
		'GetIsUserInGroup
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetIsUserInGroup, SecurityGroup=|!StoredViewEmpNameSecurity_CPPT!|)
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetIsUserInGroup, SecurityGroup=|!StoredManageStdPlansSecurity_CPPT!|)
		'
		'GetUserIsAdmin
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetUserIsAdmin)
		'
		'GetUserIsAdminOrMgr
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetUserIsAdminOrMgr)
		'
		'
		'***************************************************************************************************************************************************
		'CUSTOM FUNCTIONS
		'***************************************************************************************************************************************************
		'GetTimeSpan
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetTimeSpan,D1=|DCode1|,D2=|DCode2|,Type=D)
		'	Span Calculated as (D2-D1)
		'	Valid Types: (D=Days,M=Months,Y=Years)
		'
		'GetRegValue
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetRegValue,RegID=[|RegisterID|],RegInstance=[|RegisterIDInstance|],Field=[Code1],FieldType=T)
		'	Valid FieldTypes: (T=Text,N=Numeric,D=Date)
		'
		'GetLimitResidual
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetLimitResidual,RegID=[|RegisterID|],Account=[YourAccount],Period=[|CalcPer|],Field=[Amount], LimitValue=[100000], Filter=[RegisterID Like '%_|RegisterIDInstance|'])
		'
		'GetSumPer
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetSumP,RegID=[|RegisterID|],Account=[YourAccount],Period=[|CalcPer|],Field=[Amount])
		'
		'GetSumCum
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetSumC,RegID=[|RegisterID|],Account=[YourAccount],Period=[|CalcPer|],Field=[Amount])
		'
		'GetSumCustom
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetSumCustom,RegID=[|RegisterID|],Criteria=[Account='YourAccount' And Period=|CalcPer|],Field=[Amount])
		'
		'GetMin
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetMin,RegID=[|RegisterID|],Account=[YourAccount],Period=[|CalcPer|],Field=[Amount],SkipZeros=False)
		'
		'GetMinCustom
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetMinCustom,RegID=[|RegisterID|],Criteria=[Account='YourAccount' And Period=|CalcPer|],Field=[Amount],SkipZeros=False)
		'
		'GetMax
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetMax,RegID=[|RegisterID|],Account=[YourAccount],Period=[|CalcPer|],Field=[Amount],SkipZeros=False)
		'
		'GetMaxCustom
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetMaxCustom,RegID=[|RegisterID|],Criteria=[Account='YourAccount' And Period=|CalcPer|],Field=[Amount],SkipZeros=False)
		'
		'GetAvg
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetAvg,RegID=[|RegisterID|],Account=[YourAccount],Period=[|CalcPer|],Field=[Amount],SkipZeros=False)
		'
		'GetAvgCustom
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetAvgCustom,RegID=[|RegisterID|],Criteria=[Account='YourAccount' And Period=|CalcPer|],Field=[Amount],SkipZeros=False)
		'
		'GetCleanUsername
		'	Parameter Example:	XFBR(CPP_ParamHelper, GetCleanUsername)
		'
		'Created By:			OneStream Software
		'Date Created:			01-06-2015
		'------------------------------------------------------------------------------------------------------------
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				Dim cppHelper As New OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.MainClass
				If Not cppHelper.ValidateWorkflowPOVInitialization(si) Then Return Nothing

				If args.FunctionName.XFEqualsIgnoreCase("GetContentDescription") Then
					'Get an alternate description for the supplied dashboard name
					Return Me.GetContentDescription(si, globals, api, args)

				Else If args.FunctionName.XFEqualsIgnoreCase("GetActivityClassName") Then
					Return Me.GetActivityClassName(si, globals, api, args)

				Else If args.FunctionName.XFEqualsIgnoreCase("GetRegisterItemName") Then
					Return Me.GetRegisterItemName(si, globals, api, args)

				Else If args.FunctionName.XFEqualsIgnoreCase("GetCalcPlanFullName") Then
					Return Me.GetCalcPlanFullName(si, globals, api, args)

				Else If args.FunctionName.XFEqualsIgnoreCase("GetColFmt") Then
					Return Me.GetColFmt(si, globals, api, args)

				Else If args.FunctionName.XFEqualsIgnoreCase("GetStatusList") Then
					Return Me.GetStatusList(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetWFProfileColVisible")) Then
					Return Me.GetWFProfileColVisible(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetWFProfileCriteria")) Then
					Return Me.GetWFProfileCriteria(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetRegCriteria")) Then
					Return Me.GetRegCriteria(si, globals, api, args)

				Else If args.FunctionName.XFEqualsIgnoreCase("GetDeleteMessage") Then
					Return Me.GetDeleteMessage(si, globals, api, args)

				Else If args.FunctionName.XFEqualsIgnoreCase("GetIsUserInGroup") Then
					Return Me.GetUserInGroup(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetUserIsAdmin")) Then
					Return BRApi.Security.Authorization.IsUserInAdminGroup(si)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetUserIsAdminOrMgr")) Then
					Return Me.GetUserAdminOrManager(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetCleanUsername")) Then
					'Get the User Document Folder with the Clean Name (Consistent with Platform Folder Naming)
					Return StringHelper.RemoveSystemCharacters(si.AuthToken.UserName, True, False)

				Else If args.FunctionName.XFEqualsIgnoreCase("GetFieldValueUsingKey") Then
					'Lookup a row from a key field value and return a different field value from the row
					Dim tableName As String = args.NameValuePairs.XFGetValue("TableName")
					Dim keyField As String = args.NameValuePairs.XFGetValue("KeyField")
					Dim keyValue As String = args.NameValuePairs.XFGetValue("KeyValue")
					Dim fieldToReturn As String = args.NameValuePairs.XFGetValue("FieldToReturn")
					Return cppHelper .GetFieldValueUsingKey(si, tableName, keyField, keyValue, fieldToReturn)

				'***************************************************************************************************************************************************
				'CUSTOM FUNCTIONS
				'***************************************************************************************************************************************************
				Else If (args.FunctionName.XFEqualsIgnoreCase("GetTimeSpan")) Then
					Return Me.GetTimeSpan(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetRegValue")) Then
					Return Me.GetRegValue(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetLimitResidual")) Then
					Return Me.GetLimitResidual(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetSumPer")) Then
					Return Me.GetSumPer(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetSumCum")) Then
					Return Me.GetSumCum(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetSumCustom")) Then
					Return Me.GetSumCustom(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetMin")) Then
					Return Me.GetMin(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetMinCustom")) Then
					Return Me.GetMinCustom(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetMax")) Then
					Return Me.GetMax(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetMaxCustom")) Then
					Return Me.GetMaxCustom(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetAvg")) Then
					Return Me.GetAvg(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetAvgCustom")) Then
					Return Me.GetAvgCustom(si, globals, api, args)

							#Region "AIQOS Plan data mappings"					
		
		'Conditional Execution Disposal
				Else If (args.FunctionName.XFEqualsIgnoreCase("ConditionalExecution_Disposal")) Then
					'XFBR(CPP_ParamHelper, ConditionalExecution, WFTime = |WFTimeName|)M|CalcPer|' = |WFTimeName| --> Original Conditional Execution
					Dim WFTime As String = args.NameValuePairs.XFGetValue("WFTime")
					Dim WFYear As String = WFTime.Substring(0,5)  '---> comes with a single quote
					Dim To_Return As String = String.Empty
					To_Return = "" & WFYear & ""
					Return To_Return
					
		'Conditional Execution NEW
				Else If (args.FunctionName.XFEqualsIgnoreCase("ConditionalExecution_Amort")) Then
					'XFBR(CPP_ParamHelper, ConditionalExecution_Amort, WFTime = |WFTimeName|, AcqDate = |Code11|, Amort = |Ncode1|, CalcPer = |CalcPer|, Disposal = |Annot1|)  --> New Conditional Execution
					Dim WFTime_Aux As String = args.NameValuePairs.XFGetValue("WFTime")
					Dim AcqDate_Aux As String = args.NameValuePairs.XFGetValue("AcqDate") 				'---> Code11	
					Dim Amort As Decimal = args.NameValuePairs.XFGetValue("Amort") 						'---> NCode1
					Dim CalcPer As String = args.NameValuePairs.XFGetValue("CalcPer") 					'---> CalcPer
					Dim Disposal_Aux As String = args.NameValuePairs.XFGetValue("Disposal") 			'---> Annot1
					Dim Disposal As String = String.Empty
					
				'Time Variables ---> comes with a single quote
					Dim WFTime As String = WFTime_Aux.Replace("'", "") 									'---> WF Time
					Dim AcqDate As String = AcqDate_Aux.Replace("'", "") 								'---> Acquisition Date
					If Disposal_Aux.Length > 3 Then
						Disposal = Disposal_Aux.Replace("'", "") 										'---> Disposal Date
					Else 
						Disposal = "2099M12" 															'---> Default Disposal Date if blank
					End If
					
				'Year Variables
					Dim WFYear As Integer = WFTime.Substring(0, 4)										'---> WF Year
					Dim AcqYear As Integer = AcqDate.Substring(0, 4)									'---> Acquisition Time
					Dim DisposalYear As Integer = Disposal.Substring(0, 4)								'---> Disposal Time
					
				'Period Variables
					Dim WFPeriod As String = WFTime.Substring(5)										'---> WF Period		
					Dim AcqPeriod As String = AcqDate.Substring(5)										'---> Acquisition Period
					Dim DisposalPeriod As String = Disposal.Substring(5)								'---> Disposal Period
					
					Dim Years As Integer = WFYear - AcqYear -1
					Dim Periods As Integer = (Years * 12) + (12 - AcqPeriod) + WFPeriod  				'Periods between Acq time and WF time
					Dim Check As Decimal = Periods / 12 * Amort * 100                    				'Amortized so far. If above 100 it is not triggered
					
					Dim To_Return As String = "('" & WFYear.ToString & "M" & CalcPer & "' = '" & WFTime & "') AND (" & Check.ToString & " < 100) AND (" & AcqYear & " < " & WFYear & ") AND (" & DisposalYear & " > " & WFYear & " OR (" & DisposalYear & " = " & WFYear & " AND " & DisposalPeriod.ToString & " > " & WFPeriod.ToString & "))"
					
					Return To_Return
				

		'Flow mapping
				Else If (args.FunctionName.XFEqualsIgnoreCase("Flow")) Then
					'XFBR(CPP_ParamHelper, Flow, UD1 = |Code6|)
					Dim UD1_Aux As String = args.NameValuePairs.XFGetValue("UD1")
					Dim UD1 As String = UD1_Aux.Substring(1,4)  '---> comes with a single quote
					Dim Text1 As String = BRApi.Finance.UD.Text(si, DimType.UD1.Id, BRApi.Finance.Members.GetMemberId(si, DimType.UD1.Id, UD1), 1, -1, -1)
					Return Text1
							
			'UD1 mapping			
				Else If (args.FunctionName.XFEqualsIgnoreCase("UD1")) Then
					Dim UD1_Aux As String = args.NameValuePairs.XFGetValue("UD1")
					Dim UD1 As String = UD1_Aux.Substring(1,4)  '---> comes with a single quote
					Dim To_Return As String = String.Empty
					To_Return = UD1
					Return To_Return
					
			'UD6 mapping			
				Else If (args.FunctionName.XFEqualsIgnoreCase("UD6")) Then
					Dim UD6_Aux As String = args.NameValuePairs.XFGetValue("UD6")
					Dim UD6 As String = UD6_Aux.Replace("'", "")  '---> comes with a single quote
					Dim To_Return As String = String.Empty
					If UD6 = "" Then
						To_Return = "None"
					Else 
						To_Return = UD6
					End If
					Return To_Return
					
		#End Region					
			
					
			End If		
					
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#Region "Constants and Enumerations"

		'String Messages
		Private m_MsgCacheItemNotFound As String = "Cache item not found for RegisterID [{0}]."

#End Region

#Region "Standard Helper Functions"

		Public Function GetContentDescription(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Get the clean description for the content title
				Dim dashboardName As String = args.NameValuePairs.XFGetValue("SelectedDashboard", "[No Selection]")
				Dim dashboardDesc As String = BRApi.Database.LookupRowFieldValue(si, "App", "Dashboard", "Name = '" & SqlStringHelper.EscapeSqlString(dashboardName) & "'", "Description", String.Empty)

				'Make sure there is a description for the dashboard
				If String.IsNullOrEmpty(dashboardDesc) Then
					Return "** Description Not Set for Dashboard: ** " & dashboardName
				Else
					Return dashboardDesc
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetActivityClassName(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				Dim classID As String = args.NameValuePairs.XFGetValue("ClassID")
				Return BRApi.Database.LookupRowFieldValue(si, "App", "XFW_CPP_ActivityClass", "ClassID = '" & SqlStringHelper.EscapeSqlString(classID) & "'", "Name", "No Selection")

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetRegisterItemName(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				Dim registerID As String = args.NameValuePairs.XFGetValue("RegisterID")
				Return BRApi.Database.LookupRowFieldValue(si, "App", "XFW_CPP_Register", "RegisterID = '" & SqlStringHelper.EscapeSqlString(registerID) & "'", "RegisterID + ', ' + CapitalName As FullName", "No Selection")

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetCalcPlanFullName(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				Dim calcPlanID As String = args.NameValuePairs.XFGetValue("CalcPlanID")
				Return BRApi.Database.LookupRowFieldValue(si, "App", "XFW_CPP_CalcPlan", "CalcPlanID = '" & SqlStringHelper.EscapeSqlString(calcPlanID) & "'", "CalcPlanID + ', ' + Description", "No Selection")

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetColFmt(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Get the configurable column format string values (Control by user in settings screen)
				Dim grp As String = args.NameValuePairs.XFGetValue("Grp")
				Dim colIndex As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("Col", SharedConstants.Unknown.ToString))
				Dim regID As String = args.NameValuePairs.XFGetValue("RegID")
				Dim cppHelper As New OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.MainClass
				Return cppHelper.GetColFormat(si, globals, grp, colIndex, regID)

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetStatusList(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Get list of defined status value (Control by user in settings screen)
				Dim listType As String = args.NameValuePairs.XFGetValue("Type", "Base")
				Dim cppHelper As New OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.MainClass
				Return cppHelper.GetStatusList(si, listType)

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetWFProfileColVisible(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				'Check the current profile against the "Central Register Profile"
				'If the current profile is the central register, then return True so that we can show the WFProfile Column in the Grids
				Dim currentProfile As String = args.NameValuePairs.XFGetValue("CurrentProfile")
				Dim registerProfile As String = args.NameValuePairs.XFGetValue("RegisterProfile")

				If currentProfile.XFEqualsIgnoreCase(registerProfile) Then
					'Central Register load, show WFProfileName column
					Return "True"
				Else
					'Specific WFProfile no need to show WFProfileName column
					Return "False"
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetWFProfileCriteria(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				'Check the current profile against the "Central Register Profile"
				'If the current profile is the central register, then include all profiles
				Dim currentProfile As String = args.NameValuePairs.XFGetValue("CurrentProfile")
				Dim registerProfile As String = args.NameValuePairs.XFGetValue("RegisterProfile")

				'Get the current profile
				Dim currentProfileInfo As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, currentProfile)
				If Not currentProfileInfo Is Nothing Then
					If (currentProfileInfo.Type = WorkflowProfileTypes.Review) Or (currentProfileInfo.Type = WorkflowProfileTypes.BaseInput) Then
						'Create an IN Clause for the Descendant Workflow profiles of type InputImportChild
						Dim wfClusterPk As New WorkflowUnitClusterPk(currentProfileInfo.ProfileKey, si.WorkflowClusterPk.ScenarioKey, si.WorkflowClusterPk.TimeKey)
						Dim profileList As List(Of WorkflowProfileInfo) = BRApi.Workflow.Metadata.GetRelatives(si, wfClusterPk, WorkflowProfileRelativeTypes.Descendants, WorkflowProfileTypes.InputImportChild)
						If Not profileList Is Nothing Then
							Dim childNames As New List(Of String)
							For Each childProfile As WorkflowProfileInfo In profileList
								childNames.Add(childProfile.Name)
							Next
							Dim childInList As String = SQLStringHelper.CreateInList(childNames, True)
							If Not String.IsNullOrWhiteSpace(childInList) Then
								Return " In(" & childInList & ") "
							Else
								Return " = '" & currentProfile & "' "
							End If
						Else
							'Limit results to current profile
							Return " = '" & currentProfile & "' "
						End If

					Else If currentProfileInfo.Type = WorkflowProfileTypes.InputImportChild Then
						If currentProfile.XFEqualsIgnoreCase(registerProfile) Then
							'Current profile is the central register loading profile (So return all profiles)
							Return " Not In('') "
						Else
							'Limit results to current profile
							Return " = '" & currentProfile & "' "
						End If
					Else
						'Limit results to current profile
						Return " = '" & currentProfile & "' "
					End If
				Else
					'No current profile, so return nothing
					Return " = '' "
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetRegCriteria(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				'Get the parameters
				Dim status As String = args.NameValuePairs.XFGetValue("Status")
				Dim regID As String = args.NameValuePairs.XFGetValue("RegID")

				Dim crit As New Text.StringBuilder

				'Build where clause critier based on the supplied parameters
				'STATUS
				If Not String.IsNullOrWhiteSpace(status) And Not status.XFEqualsIgnoreCase("All") Then
					crit.Append(" And Status = '")
					crit.Append(status)
					crit.Append("'")
				End If

				'REGISTERID
				If Not String.IsNullOrWhiteSpace(regID) And Not regID.XFEqualsIgnoreCase("All") Then
					crit.Append(" And RegisterID = '")
					crit.Append(regID)
					crit.Append("'")
				End If

				Return crit.ToString

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetUserInGroup(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				Dim securityGroupName As String = args.NameValuePairs.XFGetValue("SecurityGroup")
				If BRApi.Security.Authorization.IsUserInGroup(si, securityGroupName) Then
					Return "True"
				Else
					Return "False"
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetUserAdminOrManager(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				Dim cppHelper As New OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.MainClass
				If cppHelper.IsUserAdminOrManager(si) Then
					Return "True"
				Else
					Return "False"
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetDeleteMessage(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim msg As String = String.Empty

				'Get WFCluster Criteria Values
				Dim tRegisterID As String = args.NameValuePairs.XFGetValue("tRegisterID")
				Dim tScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim tTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim deleteType As String = args.NameValuePairs.XFGetValue("deleteType")

				If deleteType = "0" Then
					'Selected Plan
					msg = "Delete Exception Allocation Plan:  '" & tRegisterID & ", " & tScenario & ", " & tTime & "'"
				Else If deleteType = "1" Then
					'Entire Register
					msg = "Delete Entire Register:  '" & tScenario & ", " & tTime & "'"
				Else
					msg = "Invalid Delete Type"
				End If

				Return msg

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "Custom Functions"

		Public Function GetTimeSpan(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim invariantIntFormat As String = "G"
				Dim invariantDateFormat As String = "yyyyMMdd"
				Dim span As Integer = 0

				'Process Date 1
				Dim d1Text As String = args.NameValuePairs.XFGetValue("D1", SharedConstants.DateTimeMinValue.ToString(invariantDateFormat))
				Dim d1 As DateTime = SharedConstants.DateTimeMinValue
				If Not DateTime.TryParseExact(d1Text, invariantDateFormat, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, d1) Then d1 = SharedConstants.DateTimeMinValue

				'Process Date 2
				Dim d2Text As String = args.NameValuePairs.XFGetValue("D2", SharedConstants.DateTimeMinValue.ToString(invariantDateFormat))
				Dim d2 As DateTime = SharedConstants.DateTimeMinValue
				If Not DateTime.TryParseExact(d2Text, invariantDateFormat, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, d2) Then d2 = SharedConstants.DateTimeMinValue

				'Process the Span Type
				Dim spanType As String = args.NameValuePairs.XFGetValue("Type")

				'Calculate the requested span between the two dates
				If spanType.XFEqualsIgnoreCase("D") Then
					'Days
					span = (d2-d1).TotalDays

				Else If spanType.XFEqualsIgnoreCase("M") Then
					'Months
					span = Me.GetMonthSpan(si, d1, d2)

				Else If spanType.XFEqualsIgnoreCase("Y") Then
					'Years
					span = d2.Year - d1.Year

				End If

				Return span.XFToStringForFormula(invariantIntFormat)

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetRegValue(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim regValue As String = String.Empty

				'Get the Register Key Information
				Dim regID As String = args.NameValuePairs.XFGetValue("RegID", SharedStringConstants.Unknown)
				Dim cleanRegID As String = regID.Replace("'", String.Empty)
				Dim regInstance As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("RegInstance", SharedConstants.Unknown))
				Dim field As String = args.NameValuePairs.XFGetValue("Field", SharedStringConstants.Unknown)
				Dim fieldType As String = args.NameValuePairs.XFGetValue("FieldType", "T")

				'Get the Cache Item
				Dim ci As OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
				If Not ci Is Nothing Then
					'Get a Register Value for the supplied Register Instance
					regValue = ci.GetRegisterValue(si, regInstance, field, fieldType)
				Else
					BRApi.ErrorLog.LogMessage(si, StringHelper.FormatMessage(m_MsgCacheItemNotFound, regID))
 				End If

				Return regValue

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetLimitResidual(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim residualValue As Decimal = 0

				'Prepare the Parameters
				Dim regID As String = args.NameValuePairs.XFGetValue("RegID", SharedStringConstants.Unknown)
				Dim cleanRegID As String = regID.Replace("'", String.Empty)
				Dim account As String = args.NameValuePairs.XFGetValue("Account", SharedStringConstants.Unknown)
				Dim period As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("Period", SharedConstants.Unknown))
				Dim field As String = args.NameValuePairs.XFGetValue("Field", SharedStringConstants.Unknown)
				Dim slimitValue As String = args.NameValuePairs.XFGetValue("LimitValue", "0")
				Dim limitValue As Decimal = 0
				If Not Decimal.TryParse(slimitValue, limitValue) Then
					limitValue = 0
				End If
				Dim filter As String = args.NameValuePairs.XFGetValue("Filter", SharedStringConstants.Unknown)

				'Get the Cache Item
				Dim ci As OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
				If Not ci Is Nothing Then
					'Get the Periodic Value (Or Residual) if it is less than the supplied cumulative limit
					residualValue = ci.GetLimitResidual(si, account, period, field, limitValue, filter)
				Else
					BRApi.ErrorLog.LogMessage(si, StringHelper.FormatMessage(m_MsgCacheItemNotFound, regID))
 				End If

				Return residualValue.XFToStringForFormula

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetSumPer(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim sumValue As Decimal = 0

				'Prepare the Parameters
				Dim regID As String = args.NameValuePairs.XFGetValue("RegID", SharedStringConstants.Unknown)
				Dim cleanRegID As String = regID.Replace("'", String.Empty)
				Dim account As String = args.NameValuePairs.XFGetValue("Account", SharedStringConstants.Unknown)
				Dim period As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("Period", SharedConstants.Unknown))
				Dim field As String = args.NameValuePairs.XFGetValue("Field", SharedStringConstants.Unknown)

				'Get the Cache Item
				Dim ci As OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
				If Not ci Is Nothing Then
					'Execute the sum (Single Period)
					sumValue = ci.SumPlanPeriod(si, account, period, field)
				Else
					BRApi.ErrorLog.LogMessage(si, StringHelper.FormatMessage(m_MsgCacheItemNotFound, regID))
 				End If

				Return sumValue.XFToStringForFormula

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetSumCum(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim sumValue As Decimal = 0

				'Prepare the Parameters
				Dim regID As String = args.NameValuePairs.XFGetValue("RegID", SharedStringConstants.Unknown)
				Dim cleanRegID As String = regID.Replace("'", String.Empty)
				Dim account As String = args.NameValuePairs.XFGetValue("Account", SharedStringConstants.Unknown)
				Dim period As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("Period", SharedConstants.Unknown))
				Dim field As String = args.NameValuePairs.XFGetValue("Field", SharedStringConstants.Unknown)

				'Get the Cache Item
				Dim ci As OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
				If Not ci Is Nothing Then
					'Execute the sum (All Periods Less Than or Equal to Supplied Period)
					sumValue = ci.SumPlanCumulative(si, account, period, field)
				Else
					BRApi.ErrorLog.LogMessage(si, StringHelper.FormatMessage(m_MsgCacheItemNotFound, regID))
 				End If

				Return sumValue.XFToStringForFormula

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetSumCustom(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim sumValue As Decimal = 0

				'Prepare the Parameters
				Dim regID As String = args.NameValuePairs.XFGetValue("RegID", SharedStringConstants.Unknown)
				Dim cleanRegID As String = regID.Replace("'", String.Empty)
				Dim criteria As String = args.NameValuePairs.XFGetValue("Criteria", SharedStringConstants.Unknown)
				Dim field As String = args.NameValuePairs.XFGetValue("Field", SharedStringConstants.Unknown)

				'Get the Cache Item
				Dim ci As OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
				If Not ci Is Nothing Then
					'Execute the sum (Custom Criteria)
					sumValue = ci.SumPlanCustom(si, criteria, field)
				Else
					BRApi.ErrorLog.LogMessage(si, StringHelper.FormatMessage(m_MsgCacheItemNotFound, regID))
 				End If

				Return sumValue.XFToStringForFormula

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetMin(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim minValue As Decimal = 0

				'Prepare the Parameters
				Dim regID As String = args.NameValuePairs.XFGetValue("RegID", SharedStringConstants.Unknown)
				Dim cleanRegID As String = regID.Replace("'", String.Empty)
				Dim account As String = args.NameValuePairs.XFGetValue("Account", SharedStringConstants.Unknown)
				Dim period As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("Period", SharedConstants.Unknown.ToString))
				Dim field As String = args.NameValuePairs.XFGetValue("Field", SharedStringConstants.Unknown)
				Dim skipZeros As Boolean = ConvertHelper.ToBoolean(args.NameValuePairs.XFGetValue("SkipZeros", "False"))

				'Get the Cache Item
				Dim ci As OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
				If Not ci Is Nothing Then
					'Get the min value (For Account and Period)
					minValue = ci.MinPlan(si, account, period, field, skipZeros)
				Else
					BRApi.ErrorLog.LogMessage(si, StringHelper.FormatMessage(m_MsgCacheItemNotFound, regID))
 				End If

				Return minValue.XFToStringForFormula

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetMinCustom(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim minValue As Decimal = 0

				'Prepare the Parameters
				Dim regID As String = args.NameValuePairs.XFGetValue("RegID", SharedStringConstants.Unknown)
				Dim cleanRegID As String = regID.Replace("'", String.Empty)
				Dim criteria As String = args.NameValuePairs.XFGetValue("Criteria", SharedStringConstants.Unknown)
				Dim field As String = args.NameValuePairs.XFGetValue("Field", SharedStringConstants.Unknown)
				Dim skipZeros As Boolean = ConvertHelper.ToBoolean(args.NameValuePairs.XFGetValue("SkipZeros", "False"))

				'Get the Cache Item
				Dim ci As OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
				If Not ci Is Nothing Then
					'Get the min value (Custom Criteria)
					minValue = ci.MinPlanCustom(si, criteria, field, skipZeros)
				Else
					BRApi.ErrorLog.LogMessage(si, StringHelper.FormatMessage(m_MsgCacheItemNotFound, regID))
 				End If

				Return minValue.XFToStringForFormula

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetMax(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim maxValue As Decimal = 0

				'Prepare the Parameters
				Dim regID As String = args.NameValuePairs.XFGetValue("RegID", SharedStringConstants.Unknown)
				Dim cleanRegID As String = regID.Replace("'", String.Empty)
				Dim account As String = args.NameValuePairs.XFGetValue("Account", SharedStringConstants.Unknown)
				Dim period As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("Period", SharedConstants.Unknown.ToString))
				Dim field As String = args.NameValuePairs.XFGetValue("Field", SharedStringConstants.Unknown)
				Dim skipZeros As Boolean = ConvertHelper.ToBoolean(args.NameValuePairs.XFGetValue("SkipZeros", "False"))

				'Get the Cache Item
				Dim ci As OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
				If Not ci Is Nothing Then
					'Get the max value (For Account and Period)
					maxValue = ci.MaxPlan(si, account, period, field, skipZeros)
				Else
					BRApi.ErrorLog.LogMessage(si, StringHelper.FormatMessage(m_MsgCacheItemNotFound, regID))
 				End If

				Return maxValue.XFToStringForFormula

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetMaxCustom(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim maxValue As Decimal = 0

				'Prepare the Parameters
				Dim regID As String = args.NameValuePairs.XFGetValue("RegID", SharedStringConstants.Unknown)
				Dim cleanRegID As String = regID.Replace("'", String.Empty)
				Dim criteria As String = args.NameValuePairs.XFGetValue("Criteria", SharedStringConstants.Unknown)
				Dim field As String = args.NameValuePairs.XFGetValue("Field", SharedStringConstants.Unknown)
				Dim skipZeros As Boolean = ConvertHelper.ToBoolean(args.NameValuePairs.XFGetValue("SkipZeros", "False"))

				'Get the Cache Item
				Dim ci As OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
				If Not ci Is Nothing Then
					'Get the max value (Custom Criteria)
					maxValue = ci.MaxPlanCustom(si, criteria, field, skipZeros)
				Else
					BRApi.ErrorLog.LogMessage(si, StringHelper.FormatMessage(m_MsgCacheItemNotFound, regID))
 				End If

				Return maxValue.XFToStringForFormula

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetAvg(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim avgValue As Decimal = 0

				'Prepare the Parameters
				Dim regID As String = args.NameValuePairs.XFGetValue("RegID", SharedStringConstants.Unknown)
				Dim cleanRegID As String = regID.Replace("'", String.Empty)
				Dim account As String = args.NameValuePairs.XFGetValue("Account", SharedStringConstants.Unknown)
				Dim period As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("Period", SharedConstants.Unknown.ToString))
				Dim field As String = args.NameValuePairs.XFGetValue("Field", SharedStringConstants.Unknown)
				Dim skipZeros As Boolean = ConvertHelper.ToBoolean(args.NameValuePairs.XFGetValue("SkipZeros", "False"))

				'Get the Cache Item
				Dim ci As OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
				If Not ci Is Nothing Then
					'Get the avg value (For Account and Period)
					avgValue = ci.AvgPlan(si, account, period, field, skipZeros)
				Else
					BRApi.ErrorLog.LogMessage(si, StringHelper.FormatMessage(m_MsgCacheItemNotFound, regID))
 				End If

				Return avgValue.XFToStringForFormula

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetAvgCustom(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim avgValue As Decimal = 0

				'Prepare the Parameters
				Dim regID As String = args.NameValuePairs.XFGetValue("RegID", SharedStringConstants.Unknown)
				Dim cleanRegID As String = regID.Replace("'", String.Empty)
				Dim criteria As String = args.NameValuePairs.XFGetValue("Criteria", SharedStringConstants.Unknown)
				Dim field As String = args.NameValuePairs.XFGetValue("Field", SharedStringConstants.Unknown)
				Dim skipZeros As Boolean = ConvertHelper.ToBoolean(args.NameValuePairs.XFGetValue("SkipZeros", "False"))

				'Get the Cache Item
				Dim ci As OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
				If Not ci Is Nothing Then
					'Get the avg value (Custom Criteria)
					avgValue = ci.AvgPlanCustom(si, criteria, field, skipZeros)
				Else
					BRApi.ErrorLog.LogMessage(si, StringHelper.FormatMessage(m_MsgCacheItemNotFound, regID))
 				End If

				Return avgValue.XFToStringForFormula

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "Private Helper Functions"

	Private Function GetMonthSpan(ByVal si As SessionInfo, ByVal startDate As DateTime, ByVal endDate As DateTime) As Integer
		Try
			'Calculate Month Difference
			Dim month As Integer = 0
			Dim year As Integer = 0
			If startDate.Month > endDate.Month Then
				month = (endDate.Month + 12) - (startDate.Month)
			Else
				month = (endDate.Month) - (startDate.Month)
			End If

			'Combine the Month and Year span for total Month Span
 			year = endDate.Year - (startDate.Year)
			month = Month + (year * 12)

			Return month

		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function

#End Region

	End Class
End Namespace