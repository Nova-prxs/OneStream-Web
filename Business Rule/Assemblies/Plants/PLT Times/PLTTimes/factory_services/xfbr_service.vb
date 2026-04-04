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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class xfbr_service
		Implements IWsasXFBRStringV800

        Public Function GetXFBRString(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal workspace As DashboardWorkspace, _
			ByVal args As DashboardStringFunctionArgs) As String Implements IWsasXFBRStringV800.GetXFBRString
            Try
                If (globals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
                    If args.FunctionName.XFEqualsIgnoreCase("GetLink") Then
						
						If brapi.FileSystem.DoesFileExist(si,FileSystemLocation.ApplicationDatabase, "Documents/Public/Times/Admin/XFC_TIMES_HistoricalData_Loaded.xlsx")
                        	Return "[Documents/Public/Times/Admin/XFC_TIMES_HistoricalData_Loaded.xlsx]"
						Else
                        	Return "[Documents/Public/Times/Admin/XFC_TIMES_HistoricalData.xlsx]" 
						End If
                	End If
	            End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

	End Class
End Namespace
