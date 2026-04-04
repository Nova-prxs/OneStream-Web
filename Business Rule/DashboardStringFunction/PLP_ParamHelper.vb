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

Namespace OneStream.BusinessRule.DashboardStringFunction.PLP_ParamHelper
    Public Class MainClass
        '------------------------------------------------------------------------------------------------------------
        'Reference Code: 		PLP_ParamHelper
        '
        'Description:			Conditional Parameter helper functions
        '
        'Usage:					Used to provide conditional parameter processing functions that allow a parameter
        '						value to be interpreted and subtituted with a different string.
        'GetContentDescription
        '	Parameter Example:	BRString(PLP_ParamHelper, GetContentDescription, SelectedDashboard=|!SelectedContent_PLP!|)
        '
        'GetActivityClassName
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetActivityClassName, ClassID=|!SelectedClassID_PLP!|)
        '
        'GetActivityType
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetActivityType, ClassID=|!SelectedClassID_PLP!|)
        '
        'GetRegisterItemNames
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetRegisterItemNames, RegisterID=|!SelectedRegisterID_PLP!|,SecurityGroup=|!StoredViewEmpNameSecurity_PLPT!|, (OPTIONAL) TemplateType = False)
        '
        'GetCalcPlanFullName
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetCalcPlanFullName, CalcPlanID=|!SelectedCalcPlanID_PLP!|)
        '
        'GetColFmt
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetColFmt, Grp=Reg Col=1, RegID=All)
        '
        'GetStatusList
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetStatusList, Type=Base)
        '	Valid Types:		Base=Base List, Filter=List (With All Value), All= Complete List (With Exception)
        '
        'GetWFProfileColVisible
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetWFProfileColVisible, CurrentProfile=|WFProfile|,RegisterProfile=|!StoredPlanRegisterProfile_PLPT!|)
        '
        'GetWFProfileCriteria
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetWFProfileCriteria, CurrentProfile=|WFProfile|,RegisterProfile=|!StoredPlanRegisterProfile_PLPT!|)
        '
        'GetRegCriteria
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetRegCriteria, Status=|!StatusFilterList_PLP!|, RegID=|!SelectedRegisterIDDistinct_PLP!|)
        '
        'GetDeleteMessage Function
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetDeleteMessage, tRegisterID=|!SelectedRegisterID_PLP!|, deleteType=|!SelectedDeleteType_PLP!|)
        '
        'GetIsUserInGroup
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetIsUserInGroup, SecurityGroup=|!StoredViewEmpNameSecurity_PLPT!|)
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetIsUserInGroup, SecurityGroup=|!StoredManageStdPlansSecurity_PLPT!|)
        '
        'GetUserIsAdmin
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetUserIsAdmin)
        '
        'GetUserIsAdminOrMgr
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetUserIsAdminOrMgr)
        '
        '
        '***************************************************************************************************************************************************
        'CUSTOM FUNCTIONS
        '***************************************************************************************************************************************************
        'GetTimeSpan
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetTimeSpan,D1=|DCode1|,D2=|DCode2|,Type=D)
        '	Span Calculated as (D2-D1)
        '	Valid Types: (D=Days,M=Months,Y=Years)
        '
        'GetRegValue
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetRegValue,RegID=[|RegisterID|],RegInstance=[|RegisterIDInstance|],Field=[Code1],FieldType=T)
        '	Valid FieldTypes: (T=Text,N=Numeric,D=Date)
        '
        'GetLimitResidual
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetLimitResidual,RegID=[|RegisterID|],Account=[YourAccount],Period=[|CalcPer|],Field=[Amount], LimitValue=[100000], Filter=[RegisterID Like '%_|RegisterIDInstance|'])
        '
        'GetSumPer
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetSumP,RegID=[|RegisterID|],Account=[YourAccount],Period=[|CalcPer|],Field=[Amount])
        '
        'GetSumCum
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetSumC,RegID=[|RegisterID|],Account=[YourAccount],Period=[|CalcPer|],Field=[Amount])
        '
        'GetSumCustom
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetSumCustom,RegID=[|RegisterID|],Criteria=[Account='YourAccount' And Period=|CalcPer|],Field=[Amount])
        '
        'GetMin
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetMin,RegID=[|RegisterID|],Account=[YourAccount],Period=[|CalcPer|],Field=[Amount],SkipZeros=False)
        '
        'GetMinCustom
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetMinCustom,RegID=[|RegisterID|],Criteria=[Account='YourAccount' And Period=|CalcPer|],Field=[Amount],SkipZeros=False)
        '
        'GetMax
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetMax,RegID=[|RegisterID|],Account=[YourAccount],Period=[|CalcPer|],Field=[Amount],SkipZeros=False)
        '
        'GetMaxCustom
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetMaxCustom,RegID=[|RegisterID|],Criteria=[Account='YourAccount' And Period=|CalcPer|],Field=[Amount],SkipZeros=False)
        '
        'GetAvg
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetAvg,RegID=[|RegisterID|],Account=[YourAccount],Period=[|CalcPer|],Field=[Amount],SkipZeros=False)
        '
        'GetAvgCustom
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetAvgCustom,RegID=[|RegisterID|],Criteria=[Account='YourAccount' And Period=|CalcPer|],Field=[Amount],SkipZeros=False)
        '
        'GetCleanUsername
        '	Parameter Example:	XFBR(PLP_ParamHelper, GetCleanUsername)
        '
        'Created By:			OneStream Software
        'Date Created:			01-06-2015
        '------------------------------------------------------------------------------------------------------------
        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
            Try
                Dim plpHelper As New DashboardExtender.PLP_SolutionHelper.MainClass
                If Not plpHelper.ValidateWorkflowPOVInitialization(si) Then Return Nothing

                If args.FunctionName.XFEqualsIgnoreCase("GetContentDescription") Then
                    'Get an alternate description for the supplied dashboard name
                    Return Me.GetContentDescription(si, globals, api, args)

                ElseIf args.FunctionName.XFEqualsIgnoreCase("GetActivityClassName") Then
                    Return Me.GetActivityClassName(si, globals, api, args)

                ElseIf args.FunctionName.XFEqualsIgnoreCase("GetRegisterItemNames") Then
                    Return Me.GetRegisterItemNames(si, globals, api, args)

                ElseIf args.FunctionName.XFEqualsIgnoreCase("GetCalcPlanFullName") Then
                    Return Me.GetCalcPlanFullName(si, globals, api, args)

                ElseIf args.FunctionName.XFEqualsIgnoreCase("GetColFmt") Then
                    Return Me.GetColFmt(si, globals, api, args)

                ElseIf args.FunctionName.XFEqualsIgnoreCase("GetStatusList") Then
                    Return Me.GetStatusList(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetWFProfileColVisible")) Then
                    Return Me.GetWFProfileColVisible(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetWFProfileCriteria")) Then
                    Return Me.GetWFProfileCriteria(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetRegCriteria")) Then
                    Return Me.GetRegCriteria(si, globals, api, args)

                ElseIf args.FunctionName.XFEqualsIgnoreCase("GetDeleteMessage") Then
                    Return Me.GetDeleteMessage(si, globals, api, args)

                ElseIf args.FunctionName.XFEqualsIgnoreCase("GetIsUserInGroup") Then
                    Return Me.GetUserInGroup(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetUserIsAdmin")) Then
                    Return BRApi.Security.Authorization.IsUserInAdminGroup(si)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetUserIsAdminOrMgr")) Then
                    Return Me.GetUserAdminOrManager(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetCleanUsername")) Then
                    'Get the User Document Folder with the Clean Name (Consistent with Platform Folder Naming)
                    Return StringHelper.RemoveSystemCharacters(si.AuthToken.UserName, True, False)

                ElseIf args.FunctionName.XFEqualsIgnoreCase("GetFieldValueUsingKey") Then
                    'Lookup a row from a key field value and return a different field value from the row
                    Dim tableName As String = args.NameValuePairs.XFGetValue("TableName")
                    Dim keyField As String = args.NameValuePairs.XFGetValue("KeyField")
                    Dim keyValue As String = args.NameValuePairs.XFGetValue("KeyValue")
                    Dim fieldToReturn As String = args.NameValuePairs.XFGetValue("FieldToReturn")
                    Return plpHelper.GetFieldValueUsingKey(si, tableName, keyField, keyValue, fieldToReturn)

                    '***************************************************************************************************************************************************
                    'CUSTOM FUNCTIONS
                    '***************************************************************************************************************************************************
                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetTimeSpan")) Then
                    Return Me.GetTimeSpan(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetRegValue")) Then
                    Return Me.GetRegValue(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetLimitResidual")) Then
                    Return Me.GetLimitResidual(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetSumPer")) Then
                    Return Me.GetSumPer(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetSumCum")) Then
                    Return Me.GetSumCum(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetSumCustom")) Then
                    Return Me.GetSumCustom(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetMin")) Then
                    Return Me.GetMin(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetMinCustom")) Then
                    Return Me.GetMinCustom(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetMax")) Then
                    Return Me.GetMax(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetMaxCustom")) Then
                    Return Me.GetMaxCustom(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetAvg")) Then
                    Return Me.GetAvg(si, globals, api, args)

                ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetAvgCustom")) Then
                    Return Me.GetAvgCustom(si, globals, api, args)

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
                Return BRApi.Database.LookupRowFieldValue(si, "App", "XFW_PLP_ActivityClass", "ClassID = '" & SqlStringHelper.EscapeSqlString(classID) & "'", "Name", "No Selection")

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        Public Function GetRegisterItemNames(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
            Try

                Dim registerID As String = args.NameValuePairs.XFGetValue("RegisterID")
                Dim securityGroupName As String = args.NameValuePairs.XFGetValue("SecurityGroup")
                Dim templateType As Boolean = args.NameValuePairs.XFGetValue("TemplateType", False)

                If templateType = True Then
                    Return BRApi.Database.LookupRowFieldValue(si, "App", "XFW_PLP_Register", "RegisterID = '" & SqlStringHelper.EscapeSqlString(registerID) & "'", "RegisterID + ', ' + JobTitle", "No Selection")
                Else
                    If BRApi.Security.Authorization.IsUserInGroup(si, securityGroupName) Then
                        Return BRApi.Database.LookupRowFieldValue(si, "App", "XFW_PLP_Register", "RegisterID = '" & SqlStringHelper.EscapeSqlString(registerID) & "'", "RegisterID + ', ' + JobTitle + ' - ' + LastName + ', ' + FirstName As FullName", "No Selection")
                    Else
                        Return BRApi.Database.LookupRowFieldValue(si, "App", "XFW_PLP_Register", "RegisterID = '" & SqlStringHelper.EscapeSqlString(registerID) & "'", "RegisterID + ', ' + JobTitle As FullName", "No Selection")
                    End If
                End If

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        Public Function GetCalcPlanFullName(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
            Try

                Dim calcPlanID As String = args.NameValuePairs.XFGetValue("CalcPlanID")
                Return BRApi.Database.LookupRowFieldValue(si, "App", "XFW_PLP_CalcPlan", "CalcPlanID = '" & SqlStringHelper.EscapeSqlString(calcPlanID) & "'", "CalcPlanID + ', ' + Description", "No Selection")

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
                Dim plpHelper As New DashboardExtender.PLP_SolutionHelper.MainClass
                Return plpHelper.GetColFormat(si, globals, grp, colIndex, regID)

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        Public Function GetStatusList(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
            Try
                'Get list of defined status value (Control by user in settings screen)
                Dim listType As String = args.NameValuePairs.XFGetValue("Type", "Base")
                Dim plpHelper As New DashboardExtender.PLP_SolutionHelper.MainClass
                Return plpHelper.GetStatusList(si, listType)

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
                            Dim childInList As String = SqlStringHelper.CreateInList(childNames, True)
                            If Not String.IsNullOrWhiteSpace(childInList) Then
                                Return " In(" & childInList & ") "
                            Else
                                Return " = '" & currentProfile & "' "
                            End If
                        Else
                            'Limit results to current profile
                            Return " = '" & currentProfile & "' "
                        End If

                    ElseIf currentProfileInfo.Type = WorkflowProfileTypes.InputImportChild Then
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
                Dim plpHelper As New DashboardExtender.PLP_SolutionHelper.MainClass
                If plpHelper.IsUserAdminOrManager(si) Then
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
                Dim tTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
                Dim deleteType As String = args.NameValuePairs.XFGetValue("deleteType")

                If deleteType = "0" Then
                    'Selected Plan
                    msg = "Delete Exception Allocation Plan:  '" & tRegisterID & ", " & tScenario & ", " & tTime & "'"
                ElseIf deleteType = "1" Then
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
                    span = (d2 - d1).TotalDays

                ElseIf spanType.XFEqualsIgnoreCase("M") Then
                    'Months
                    span = Me.GetMonthSpan(si, d1, d2)

                ElseIf spanType.XFEqualsIgnoreCase("Y") Then
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
                Dim ci As DashboardExtender.PLP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
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
                Dim ci As DashboardExtender.PLP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
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
                Dim ci As DashboardExtender.PLP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
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
                Dim ci As DashboardExtender.PLP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
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
                Dim ci As DashboardExtender.PLP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
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
                Dim ci As DashboardExtender.PLP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
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
                Dim ci As DashboardExtender.PLP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
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
                Dim ci As DashboardExtender.PLP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
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
                Dim ci As DashboardExtender.PLP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
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
                Dim ci As DashboardExtender.PLP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
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
                Dim ci As DashboardExtender.PLP_SolutionHelper.RegisterCacheItem = globals.GetObject(cleanRegID)
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
                month = month + (year * 12)

                Return month

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

#End Region

    End Class
End Namespace