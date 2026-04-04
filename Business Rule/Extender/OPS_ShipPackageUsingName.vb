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

Namespace OneStream.BusinessRule.Extender.OPS_ShipPackageUsingName
	Public Class MainClass
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 		OPS_ShipPackageUsingName 
		'
		'Description:			Parcel Service Data Management step helper used to execute the parcel service package]
		'						shipping routine as a background Data Management job (Package Referenced By Name).
		'
		'		
		'Created By:			Tom Shea
		'Date Created:			1-24-2015
		'------------------------------------------------------------------------------------------------------------		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType											
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Prepare parameters
						Dim ph As New OneStream.BusinessRule.DashboardExtender.OPS_SolutionHelper.MainClass
						Dim packageName As String = args.NameValuePairs.XFGetValue("PackageName")
						Dim packageID As String = ph.GetFieldFromID(si, "XFW_OPS_Package", "PackageName", packageName, "PackageID")
						Dim debugMode As Boolean = args.NameValuePairs.XFGetValue("DebugMode").XFConvertToBool()

						'Update the task activity description with the package name
						Dim description As String = "Parcel Service Shipment (" & packageName & ")"
						Dim dml As String = "UPDATE TaskActivity Set Description = '" & description & "' Where (TaskActivityType = 1000) AND (Description = N'Parcel Service Shipment') AND (TaskActivityStatus = 1000) AND (AuthSessionID = '" & si.AuthToken.AuthSessionID.ToString & "')"
						Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
							BRApi.Database.ExecuteActionQuery(dbConnFW, dml, False, False)
						End Using
						
						'Ship the package
						ph.ShipPackage(si, String.Empty, packageID, debugMode)
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace