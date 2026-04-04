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

Namespace OneStream.BusinessRule.DashboardStringFunction.OPS_ParamHelper
	Public Class MainClass
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 		OPS_ParamHelper 
		'
		'Description:			Parcel Service Parameter helper functions
		'
		'Usage:					Used to provide conditional parameter processing functions that allow a parameter  
		'						value to be interpreted and subtituted with a different string.
		'GetContentDescription
		'	Parameter Example:		XFBR(OPS_ParamHelper, GetContentDescription, SelectedDashboard=|!SelectedContentDashboard_OPS!|)
		'
		'GetPackageName
		'	Parameter Example:		XFBR(OPS_ParamHelper, GetPackageName, SelectedPackageID=|!SelectedPackageID_OPS!|)
		'
		'GetPackageDeliveryType
		'	Parameter Example:		XFBR(OPS_ParamHelper, GetPackageDeliveryType, SelectedPackageID=|!SelectedPackageIDHome_OPS!|)
		'
		'GetPackageEmailTitle
		'	Parameter Example:		XFBR(OPS_ParamHelper, GetPackageEmailTitle, SelectedPackageID=|!SelectedPackageIDHome_OPS!|, DefaultTitle=|!StoredDefaultEmailTitle_OPS!|)
		'		
		'GetPackageEmailMessage
		'	Parameter Example:		XFBR(OPS_ParamHelper, GetPackageEmailMessage, SelectedPackageID=|!SelectedPackageIDHome_OPS!|, DefaultMessage=|!StoredDefaultEmailMessage_OPS!|)
		'		
		'GetPackageContentFilePath
		'	Parameter Example:		XFBR(OPS_ParamHelper, GetPackageContentFilePath, SelectedContentID=|!SelectedPackageContentID_OPS!|)
		'
		'GetPackageContentFileName
		'	Parameter Example:		XFBR(OPS_ParamHelper, GetPackageContentFileName, SelectedContentID=|!SelectedPackageContentID_OPS!|)
		'
		'GetDistributionName
		'	Parameter Example:		XFBR(OPS_ParamHelper, GetDistributionName, SelectedDistributionID=|!SelectedDistributionID_OPS!|)
		'
		'GetUserItemFilter
		'	Parameter Example:		XFBR(OPS_ParamHelper, GetUserItemFilter, FilterType=[Edit or List])
		'
		'GetUserIsAdmin
		'	Parameter Example:		XFBR(OPS_ParamHelper, GetUserIsAdmin)		
		'
		'Created By:			Tom Shea
		'Date Created:			01-06-2015
		'------------------------------------------------------------------------------------------------------------		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try

				If args.FunctionName.XFEqualsIgnoreCase("GetContentDescription") Then
					Return Me.GetContentDescription(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("OnGetParcelName") Then
					Return Me.OnGetParcelName(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetPackageName") Then
					Return Me.GetPackageName(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetPackageGroupName") Then
					Return Me.GetPackageGroupName(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetPackageContentsDescription") Then
					Return Me.GetPackageContentsDescription(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetPackageShipToDescription") Then
					Return Me.GetPackageShipToDescription(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetPackageDeliveryType") Then
					Return Me.GetPackageDeliveryType(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetPackageEmailTitle") Then
					Return Me.GetPackageEmailTitle(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetPackageEmailMessage") Then
					Return Me.GetPackageEmailMessage(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetPackageContentFilePath") Then
					Return Me.GetPackageContentFilePath(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetPackageContentFileName") Then
					Return Me.GetPackageContentFileName(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetDistributionName") Then
					Return Me.GetDistributionName(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetUserItemFilter") Then
					Return Me.GetUserItemFilter(si, globals, api, args)

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetUserIsAdmin")) Then
					Return Me.GetUserIsAdmin(si, globals, api, args)

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetUserInGroupOrAdminOrManage")) Then
					Return Me.GetUserInGroupOrAdminOrManage(si, globals, api, args)

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetCleanUserName")) Then
					'Get the User Document Folder with the Clean Name (Consistent with Platform Folder Naming)
					Dim allowPeriods As Boolean = True
					Dim allowSpaces As Boolean = False
					Return StringHelper.RemoveSystemCharacters(si.AuthToken.UserName, allowPeriods, allowSpaces)
				ElseIf (args.FunctionName.XFEqualsIgnoreCase("AreParametersSet")) Then
					Return Me.AreParametersSet(si, globals, api, args)
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#Region "Conditional Parameter/Lookup Functions"

		Public Function GetContentDescription(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Declare an instance of the dashboard extender helper class
				Dim opsHelper As New OneStream.BusinessRule.DashboardExtender.OPS_SolutionHelper.MainClass

				'Get the clean description for the content title
				Dim dashboardName As String = args.NameValuePairs.XFGetValue("SelectedDashboard", "[No Selection]")
				Dim dashboardDesc As String = opsHelper.GetFieldFromID(si, "Dashboard", "Name", dashboardName, "Description")

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

		Public Function OnGetParcelName(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Return GetParcelName(si, args, args.NameValuePairs.XFGetValue("ParcelType", "Package"))
		End Function

		Private Function GetParcelName(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs, typeName As String) As String
			Try


				'Declare an instance of the dashboard extender helper class
				Dim opsHelper As New OneStream.BusinessRule.DashboardExtender.OPS_SolutionHelper.MainClass
				Dim parcelID As String = opsHelper.ValidateGuid(si, args.NameValuePairs.XFGetValue("Selected" & typeName & "ID", "[No Selection]"))
				Dim parcelName As String = opsHelper.GetFieldFromID(si, "XFW_OPS_" & typeName, typeName & "ID", parcelID, typeName & "Name")

				If Not String.IsNullOrEmpty(parcelName) Then
					Dim separator As String = args.NameValuePairs.XFGetValue("Separator", String.Empty)
					Dim separatorLocation As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("SeparatorLocation", String.Empty))

					If separatorLocation = 0 Then '"PackageName -"
						Return StringHelper.FormatMessage("{0} {1}", separator, parcelName)
					ElseIf separatorLocation = 1 Then '"- PackageName"
						Return StringHelper.FormatMessage("{0} {1}", parcelName, separator)
					Else
						Return parcelName
					End If
				Else
					Return parcelName
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetPackageName(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Return GetParcelName(si, args, "Package")
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetPackageGroupName(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Return GetParcelName(si, args, "PackageGroup")
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetPackageDeliveryType(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Declare an instance of the dashboard extender helper class
				Dim opsHelper As New OneStream.BusinessRule.DashboardExtender.OPS_SolutionHelper.MainClass

				'Get the name from the ID
				Dim packageID As String = args.NameValuePairs.XFGetValue("SelectedPackageID", "[No Selection]")
				packageID = opsHelper.ValidateGuid(si, packageID)

				If Not packageID = Guid.Empty.ToString Then
					Dim deliveryType As String = opsHelper.GetFieldFromID(si, "XFW_OPS_Package", "PackageID", packageID, "DeliveryType")

					Select Case deliveryType
						Case Is = "1"
							Return "Send One Email to All Recipients"
						Case Is = "2"
							Return "Send Separate Emails to Each Recipient"
						Case Is = "3"
							Return "Copy Package Contents to Fileshare and Send Notification Email(s)"
						Case Is = "4"
							Return "Copy Package Contents to Fileshare Only (No Email)"
						Case Else
							Return "Unknown Delivery Type"
					End Select
				Else
					Return String.Empty
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetPackageEmailTitle(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim emailTitle As String = String.Empty
				Dim opsHelper As New OneStream.BusinessRule.DashboardExtender.OPS_SolutionHelper.MainClass

				'Get the name from the ID
				Dim packageID As String = args.NameValuePairs.XFGetValue("SelectedPackageID", "[No Selection]")
				packageID = opsHelper.ValidateGuid(si, packageID)

				If Not packageID = Guid.Empty.ToString Then
					If Not opsHelper.GetFieldFromID(si, "XFW_OPS_Package", "PackageID", packageID, "DeliveryType").XFEqualsIgnoreCase("4") Then
						emailTitle = opsHelper.GetFieldFromID(si, "XFW_OPS_Package", "PackageID", packageID, "EmailTitle")
						If String.IsNullOrEmpty(emailTitle) OrElse emailTitle.XFEqualsIgnoreCase("(Default)") Then
							'No specific Title, just return the default title
							emailTitle = args.NameValuePairs.XFGetValue("DefaultTitle")
						End If

						Dim packageName As String = opsHelper.GetFieldFromID(si, "XFW_OPS_Package", "PackageID", packageID, "PackageName")
						If Not emailTitle.XFContainsIgnoreCase(packageName) Then
							emailTitle = emailTitle & " (" & packageName & ")"
						End If

						emailTitle = ParseSubstitutionParameters(si, emailTitle, packageID, opsHelper)
					End If
				End If

				Return emailTitle

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetPackageEmailMessage(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim emailMessage As String = String.Empty
				Dim opsHelper As New OneStream.BusinessRule.DashboardExtender.OPS_SolutionHelper.MainClass

				'Get the name from the ID
				Dim packageID As String = args.NameValuePairs.XFGetValue("SelectedPackageID", "[No Selection]")
				packageID = opsHelper.ValidateGuid(si, packageID)

				If Not packageID = Guid.Empty.ToString Then
					If Not opsHelper.GetFieldFromID(si, "XFW_OPS_Package", "PackageID", packageID, "DeliveryType").XFEqualsIgnoreCase("4") Then
						emailMessage = opsHelper.GetFieldFromID(si, "XFW_OPS_Package", "PackageID", packageID, "EmailMessage")
						If String.IsNullOrEmpty(emailMessage) OrElse emailMessage.XFEqualsIgnoreCase("(Default)") Then
							'No specific Title, just return the default message
							emailMessage = args.NameValuePairs.XFGetValue("DefaultMessage")
						End If

						emailMessage = ParseSubstitutionParameters(si, emailMessage, packageID, opsHelper)
					End If
				End If

				Return emailMessage

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetPackageContentsDescription(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim contentDescription As String = String.Empty
				Dim opsHelper As New OneStream.BusinessRule.DashboardExtender.OPS_SolutionHelper.MainClass
				Dim packageID As String = args.NameValuePairs.XFGetValue("SelectedPackageID", "[No Selection]")
				packageID = opsHelper.ValidateGuid(si, packageID)

				If Not packageID = Guid.Empty.ToString Then
					Dim zipEmail As Boolean = opsHelper.GetFieldFromID(si, "XFW_OPS_Package", "PackageID", packageID, "ZipEmail").XFEqualsIgnoreCase("True")
					Dim deliveryType As Integer = Convert.ToInt32(opsHelper.GetFieldFromID(si, "XFW_OPS_Package", "PackageID", packageID, "DeliveryType"))

					If (zipEmail AndAlso (deliveryType = 1 OrElse deliveryType = 2)) Then
						contentDescription = "(" & opsHelper.GetFieldFromID(si, "XFW_OPS_Package", "PackageID", packageID, "PackageName") & ".zip)"
					End If
				End If

				Return contentDescription

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetPackageShipToDescription(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim contentDescription As String = String.Empty
				Dim opsHelper As New OneStream.BusinessRule.DashboardExtender.OPS_SolutionHelper.MainClass
				Dim packageID As String = opsHelper.ValidateGuid(si, args.NameValuePairs.XFGetValue("SelectedPackageID", "[No Selection]"))

				If Not packageID = Guid.Empty.ToString Then
					Dim deliveryType As String = opsHelper.GetFieldFromID(si, "XFW_OPS_Package", "PackageID", packageID, "DeliveryType")

					Select Case deliveryType
						Case Is = "4"
							contentDescription = "(No Email)"
						Case Else
							Dim distID As String = opsHelper.ValidateGuid(si, opsHelper.GetFieldFromID(si, "XFW_OPS_Package", "PackageID", packageID, "FKDistributionID"))
							If opsHelper.GetFieldFromID(si, "XFW_OPS_Distribution", "DistributionID", distID, "IsPublic").XFEqualsIgnoreCase("False") Then
								contentDescription = "(Private Distribution List)"
							End If
					End Select
				End If

				Return contentDescription

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function ParseSubstitutionParameters(ByVal si As SessionInfo, substParam As String, packageID As String, ByRef opsHelper As OneStream.BusinessRule.DashboardExtender.OPS_SolutionHelper.MainClass) As String
			Dim convertedParam As String = substParam
			If StringHelper.DoesStringContainSubstVarsOrSubstVarStringFunctions(convertedParam) Then
				Dim literalParams As Dictionary(Of String, String) = opsHelper.GetAllLiteralParams(si, False)
				Dim mergedParameters As Dictionary(Of String, String) = opsHelper.GetMergedParams(si, packageID, String.Empty, False, literalParams)
				Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
					Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Dim subVarInfo As SubstVarSourceInfo = SubstitutionVariablesHelper.CreateSubstVarSourceInfo(dbConnFW, dbConnApp)
						convertedParam = SubstitutionVariableParser.ConvertString(si, subVarInfo, mergedParameters, convertedParam)
					End Using
				End Using
			End If

			Return convertedParam
		End Function

		Public Function GetPackageContentFilePath(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Declare an instance of the dashboard extender helper class
				Dim opsHelper As New OneStream.BusinessRule.DashboardExtender.OPS_SolutionHelper.MainClass

				'Get the file path from the ContentID
				Dim contentID As String = args.NameValuePairs.XFGetValue("SelectedContentID", "[No Selection]")
				contentID = opsHelper.ValidateGuid(si, contentID)
				Return opsHelper.GetFieldFromID(si, "XFW_OPS_PackageContents", "ContentID", contentID, "SourceXFDocPath")

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetPackageContentFileName(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Declare an instance of the dashboard extender helper class
				Dim opsHelper As New OneStream.BusinessRule.DashboardExtender.OPS_SolutionHelper.MainClass
				Dim contentFileName As String = String.Empty

				'Get the file path from the ContentID
				Dim contentID As String = args.NameValuePairs.XFGetValue("SelectedContentID", "[No Selection]")
				contentID = opsHelper.ValidateGuid(si, contentID)
				Dim filePath As String = opsHelper.GetFieldFromID(si, "XFW_OPS_PackageContents", "ContentID", contentID, "SourceXFDocPath")
				Dim separator As String = args.NameValuePairs.XFGetValue("Separator", String.Empty)
				Dim separatorLocation As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("SeparatorLocation", String.Empty))
				If filePath.XFContainsIgnoreCase("/") Then
					Dim fileSegs As List(Of String) = StringHelper.SplitString(filePath, "/", StageConstants.ParserDefaults.DefaultQuoteCharacter)
					If fileSegs.Count > 0 Then
						contentFileName = fileSegs(fileSegs.Count - 1)
					Else
						contentFileName = filePath
					End If
				Else
					contentFileName = filePath
				End If

				If contentFileName <> String.Empty And separatorLocation = 0 Then '"- ContentFilePath"
					Return StringHelper.FormatMessage("{0} {1}", separator, contentFileName)
				ElseIf contentFileName <> String.Empty And separatorLocation = 1 Then '"ContentFilePath -"
					Return StringHelper.FormatMessage("{0} {1}", contentFileName, separator)
				Else
					Return contentFileName
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetDistributionName(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Declare an instance of the dashboard extender helper class
				Dim opsHelper As New OneStream.BusinessRule.DashboardExtender.OPS_SolutionHelper.MainClass

				'Get the name from the ID
				Dim distributionID As String = args.NameValuePairs.XFGetValue("SelectedDistributionID", "[No Selection]")
				distributionID = opsHelper.ValidateGuid(si, distributionID)
				Return opsHelper.GetFieldFromID(si, "XFW_OPS_Distribution", "DistributionID", distributionID, "DistributionName")

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetUserItemFilter(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim filterType As String = args.NameValuePairs.XFGetValue("FilterType", "Edit")
				Dim bIncludeWhere As Boolean = args.NameValuePairs.XFGetValue("IncludeWhere", "True").XFConvertToBool
				Dim whereString As String = String.Empty
				If (bIncludeWhere) Then
					whereString = "WHERE "
				End If
				'Return the proper where clause based on user type (Administrators can see all items)
				'Check to see if the user is an administrator
				If (BRApi.Security.Authorization.IsUserInAdminGroup(si) OrElse
					BRApi.Security.Authorization.IsUserInGroup(si, BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "StoredManageSecurity_OPS"))) Then
					'User is an admin or Manage, return an empty string (No Where Clause)
					Return String.Empty
				Else
					'User is not an admin, filter items to items created by the user and items flagged as public
					If filterType.XFEqualsIgnoreCase("Edit") Then
						'Each user can only edit items that they created
						Return whereString & "CreatedBy = '" & si.AuthToken.UserName & "'"

					ElseIf filterType.XFEqualsIgnoreCase("List") Then
						'Each user can only see items that they created or items flagged as public
						Return whereString & "(CreatedBy = '" & si.AuthToken.UserName & "') or (IsPublic = 1)"

					Else
						'Just default to most restrictive filter (UserName)
						Return whereString & "CreatedBy = '" & si.AuthToken.UserName & "'"
					End If
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetUserIsAdmin(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Check to see if the user is an administrator (Can be used to hide objects that are administrator only)
				Return BRApi.Security.Authorization.IsUserInAdminGroup(si)

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetUserInGroupOrAdminOrManage(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Get the security group that was specified
				Dim secGroup As String = args.NameValuePairs.XFGetValue("GroupName", "")
				Dim mgrGroup As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "StoredManageSecurity_OPS")

				'Check to see if the user is an administrator (Can be used to hide objects that are administrator only)
				Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
					If EngineSecurity.IsUserInGroup(si, mgrGroup) Then
						Return True
					ElseIf EngineSecurity.IsUserInGroup(dbConnFW, "Administrators") Then
						Return True
					ElseIf (Not String.IsNullOrEmpty(secGroup) AndAlso EngineSecurity.IsUserInGroup(dbConnFW, secGroup)) Then
						Return True
					Else
						Return False
					End If
				End Using

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

		End Function

		Public Function AreParametersSet(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim numParams As Integer = Convert.ToInt32(args.NameValuePairs.XFGetValue("ParameterCount", "1"))
				Dim bRequireAll As Boolean = Convert.ToBoolean(args.NameValuePairs.XFGetValue("RequireAll", "TRUE"))
				Dim bParamSet As Boolean = False

				For paramCount As Integer = 1 To numParams
					bParamSet = Not String.IsNullOrEmpty(args.NameValuePairs.XFGetValue(String.Format("Parameter{0}", paramCount), String.Empty))

					If (bRequireAll) Then
						If (Not bParamSet) Then
							Return False
						End If
					Else
						If (bParamSet) Then
							Return True
						End If
					End If
				Next

				Return bRequireAll
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

	End Class
End Namespace