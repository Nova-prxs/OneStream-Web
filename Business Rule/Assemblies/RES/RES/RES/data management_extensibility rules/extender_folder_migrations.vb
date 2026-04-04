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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_folder_migrations
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'This will generate a folder structure in the application public file system
						'Define nested tuples of folders
						Dim folderStructure As New List(Of Object) From {
							New Tuple(Of String, List(Of Object))(
								"Services", New List(Of Object) From {
									New Tuple(Of String, List(Of Object))(
										"Imported Data", New List(Of Object) From {
											"Work Orders",
											"Project Planning",
											"Structure Planning"
										}
									),
									New Tuple(Of String, List(Of Object))(
										"Templates", New List(Of Object) From {
											"Temporal", New List(Of Object) From {
												"Temporal_All"
											}
										}
									),
									"Spreadsheet Reports"								
								}
							)
						}
						
						'Generate folder structure
						Me.GenerateFolderStructure(si, "Documents/Public", folderStructure)
						
                        'Generate subfolders named Temporal_<Entity>
						Dim childEntities As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(
							si,
							"Entities",
							"E#RES_SER.Base",
							True,
							Nothing,
							Nothing
						)

                        Dim temporalSubfolders As New List(Of Object)
                        For Each entity As MemberInfo In childEntities
                            temporalSubfolders.Add("Temporal_" & entity.Member.Name)
                        Next

                        'Generate folder structure under Templates/Temporal
                        Dim templatesPath As String = "Documents/Public/Services/Templates/Temporal"
                        Me.GenerateFolderStructure(si, templatesPath, temporalSubfolders)
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Helper Functions"
		
		#Region "Generate Folder Structure"
		
		Public Sub GenerateFolderStructure(si As SessionInfo, baseDirectory As String, folderStructure As List(Of Object))
	        For Each item In folderStructure
	            If TypeOf item Is String Then
	                'Create a folder for string items
	                BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, baseDirectory, item.ToString())
	            ElseIf TypeOf item Is Tuple(Of String, List(Of Object)) Then
	                'Create a folder for the tuple's first item (string) and recurse for its second item (list)
	                Dim tuple As Tuple(Of String, List(Of Object)) = DirectCast(item, Tuple(Of String, List(Of Object)))
	                Dim newDirectory As String = Path.Combine(baseDirectory, tuple.Item1)
	                BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, baseDirectory, tuple.Item1)
	                GenerateFolderStructure(si, newDirectory, tuple.Item2)
	            End If
	        Next
	    End Sub
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
