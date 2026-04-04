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
                    If args.FunctionName.XFEqualsIgnoreCase("GetExcel") Then
                        
						' 1. Lista de parámetros comunes (clave del NameValuePair → nombre del SubstVar)				
						Dim sReport As String  = args.NameValuePairs("Report") ' args.CustomSubstVars("prm_ALC_Scenario")							

						Dim location As FileSystemLocation = FileSystemLocation.ApplicationDatabase	
						Dim sRootPath As String = "Documents/Public/Plants/Table Views Users"
						Dim sFilePath As String = $"{sRootPath}/Manufacturing_"  & si.UserName & ".xlsx"
						
'						BRApi.ErrorLog.LogMessage(si, "sFilePath: " & sFilePath)
						
						Dim sTemplatePath As String = $"Documents/Public/Plants/Table Views/Manufacturing_Users_" & sReport & ".xlsx"
						
'						BRApi.ErrorLog.LogMessage(si, "sTemplatePath: " & sTemplatePath)
						
'						If BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, sFilePath) = False

						#Region "Creación de un fichero a partir del Template"
						
						' --------------- GUARDADO FINAL DEL ARCHIVO ---------------
						Dim fileInfoTemp As XFFileEx = BRApi.FileSystem.GetFile(si, location, sTemplatePath, True, False, False, SharedConstants.Unknown, Nothing, True)
			            Dim fileTemp As XFFile = fileInfoTemp.XFFile
						
'						BRApi.ErrorLog.LogMessage(si, "fileTemp: " & fileTemp.Length.ToString)

						' Crear el Folder Path de destino si es necesario
						
						'brapi.FileSystem.CreateFullFolderPathIfNecessary(si, location, sRootPath, $"")
						
						' Cambiar las propiedades del archivo
						fileTemp.FileInfo.ContentFileExtension = "xlsx"
						fileTemp.FileInfo.Name = $"Manufacturing_"  & si.UserName & ".xlsx"
						fileTemp.FileInfo.Description = "Created by " & si.UserName
						fileTemp.FileInfo.FolderFullName = sRootPath
			            brapi.FileSystem.InsertOrUpdateFile(si, fileTemp)
						
						#End Region
						
'						End If	
						
						Return sFilePath

                	End If
					
	            End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

	End Class
End Namespace
