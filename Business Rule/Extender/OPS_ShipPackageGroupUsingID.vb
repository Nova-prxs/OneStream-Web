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

Namespace OneStream.BusinessRule.Extender.OPS_ShipPackageGroupUsingID
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try				
				If args.FunctionType = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
					'Prepare parameters
					Dim ph As New OneStream.BusinessRule.DashboardExtender.OPS_SolutionHelper.MainClass						
					Dim packageGroupID As String = args.NameValuePairs.XFGetValue("PackageGroupID")
					Dim debugMode As Boolean = args.NameValuePairs.XFGetValue("DebugMode").XFConvertToBool()
					
					'Update the task activity description with the package group name
					Dim packageName As String = ph.GetFieldFromID(si, "XFW_OPS_PackageGroup", "PackageGroupID", packageGroupID, "PackageGroupName")
					Dim description As String = "Parcel Service Group Shipment (" & packageName & ")"
					Dim dml As String = "UPDATE TaskActivity Set Description = '" & description & "' Where (TaskActivityType = 1000) AND (Description = N'Parcel Service Group Shipment') AND (TaskActivityStatus = 1000) AND (AuthSessionID = '" & si.AuthToken.AuthSessionID.ToString & "')"
					Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
						BRApi.Database.ExecuteActionQuery(dbConnFW, dml, False, False)
					End Using

					'Ship the package group
					ph.ShipPackageGroup(si, packageGroupID, debugMode)
					
				End If
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace