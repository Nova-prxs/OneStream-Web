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

Namespace OneStream.BusinessRule.Extender.XPBI_ExportMetadataDynamics
	
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
				
			Try

				Dim dt As New DataTable
				
				'BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, scenarioParametro)
												
				brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "PBI_CV_Member_Scenario", args.NameValuePairs.XFGetValue("Scenario"))

				Dim sCube As String = "CONSO"
				Dim sCubeViewName As String = "PBI_CONS_Extract_DynamicAccount"				
				
				Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)
		        Dim sFileName As String = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName) & "\PowerBI\CONS_PowerBI_ExportData_Dynamics.csv"
								
				Dim csvStringLocal As New Text.StringBuilder()							
								
				Dim parallelQueryCount As Integer = 8
				Dim logStatistics As Boolean = False			

				Dim iEntity As Integer = 1
				Dim iTime As Integer = 1
				
				Dim sTimeFilter As String = "T#" & args.NameValuePairs.XFGetValue("Time") & ".Base"
				
				'Entity					
				For Each EntityMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#LEGAL, E#LEGAL.Base", True)
					'Time
					For Each TimeMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Time", sTimeFilter, True)
					
						'BRAPi.ErrorLog.LogMessage(si,"Entity: " & EntityMember.Member.Name)
						'BRAPi.ErrorLog.LogMessage(si,"Time: " & TimeMember.Member.Name)
					
						brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "PBI_CV_Member_Entity", EntityMember.Member.Name)
						brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "PBI_CV_Member_Time", TimeMember.Member.Name)					
						
						'BRAPi.ErrorLog.LogMessage(si,"Llega1")
						
						dt = BRApi.Import.Data.FdxExecuteCubeView(si, sCubeViewName, "Entities", Nothing, "CONS_Scenarios", Nothing, _
							Nothing, Nothing, True, False, Nothing, parallelQueryCount, logStatistics)
						
						'BRAPi.ErrorLog.LogMessage(si,"Llega2")
							
						'BRAPi.ErrorLog.LogMessage(si,"dt filas: " & dt.Rows.Count.ToString())
							
						' COPIA AL ARCHIVO
						If dt IsNot Nothing Then	
						
							Dim csvRows As DataRow() = dt.select()   
															
							For Each fila As DataRow In csvRows
								For Each elemento As Object In fila.ItemArray 
									csvstringLocal.Append (elemento.tostring & ",")
								Next
								csvstringLocal.Append (vbCrLf)
							Next
				
							system.IO.file.WriteAllText(sFileName,csvstringLocal.tostring)
		
						End If
						
						iTime = iTime + 1
										
					Next
					
					iEntity = iEntity + 1
					
				Next
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
	End Class
	
End Namespace