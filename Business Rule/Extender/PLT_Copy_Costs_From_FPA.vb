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
Imports System.IO.Compression

Namespace OneStream.BusinessRule.Extender.PLT_Copy_Costs_From_FPA
	
	Public Class MainClass		
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				
				Select Case args.FunctionType

					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						Dim sScenarioName As String = "Actual"
						Dim sScenarioKey As String = si.WorkflowClusterPk.ScenarioKey.ToString
						
						Dim sTimeName As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk).Split("#")(3)
						Dim sTimeKey As String = si.WorkflowClusterPk.TimeKey.ToString
						
						Dim sYear As String = sTimeName.Split("M")(0)
						Dim sMonth As String = sTimeName.Split("M")(1)
						
						Dim sProfileNameParent As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk).Split("#")(1).Replace(":S","")
						Dim sProfileName As String = sProfileNameParent & ".Import PL"
								
						Dim sProfileKey As String = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si,sProfileName,sScenarioName,sTimeName).ProfileKey.ToString
						'BRApi.ErrorLog.LogMessage(si, "pk: " & sProfileKey)
						
						Dim sql As String = $"
							SELECT A.Bytes, B.id_factory
							FROM ( 	SELECT TOP 1 SourceFileBytes AS Bytes
									FROM StageArchivesInformation 
									WHERE Wfk = '{sProfileKey}'
									AND Wsk = '{sScenarioKey}'
									AND Wtk = '{sTimeKey}') A
							
								,( 	SELECT id AS id_factory
									FROM XFC_PLT_MASTER_Factory
									WHERE profile_name = '{sProfileNameParent}') B
						"
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						
'							BRApi.ErrorLog.LogMessage(si, sql)

							Dim dt As New DataTable("Costs")
							dt.Columns.Add("bytes", GetType(Byte()))  
							dt.Columns.Add("id_factory", GetType(String))  
							
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
																
							Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							
							For Each dr	As DataRow In dt.Rows
								Dim contentFileBytes As Byte() = DescomprimirGzip(dr(0))
								Dim sFactory As String = dr(1)
								
'								BRApi.ErrorLog.LogMessage(si, "sFactory: " & sFactory)
'								BRApi.ErrorLog.LogMessage(si, "sMonth: " & sMonth)
'								BRApi.ErrorLog.LogMessage(si, "sYear: " & sYear)
								
								shared_queries.InsertCosts(si, sFactory, sYear, sMonth, contentFileBytes)
							Next
							
						End Using	
						
				End Select

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
		Public Function DescomprimirGzip(ByVal compressedData As Byte()) As Byte()
		    ' Crear un flujo de memoria para los datos comprimidos
		    Using ms As New MemoryStream(compressedData)
		        ' Crear un flujo de memoria para los datos descomprimidos
		        Using decompressedStream As New MemoryStream()
		            ' Crear un GZipStream para descomprimir
		            Using gzip As New GZipStream(ms, CompressionMode.Decompress)
		                ' Copiar los datos descomprimidos al flujo de memoria
		                gzip.CopyTo(decompressedStream)
		            End Using
		            ' Devolver los datos descomprimidos como un arreglo de bytes
		            Return decompressedStream.ToArray()
		        End Using
		    End Using
		End Function		
		
	End Class
	
End Namespace