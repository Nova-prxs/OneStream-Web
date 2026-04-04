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

Namespace OneStream.BusinessRule.Extender.ExportCubeView2
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ExtenderArgs) As Object
			Try
				'Dim nvbParams As NameValueFormatBuilder (api.WorkflowProfile.GetAttributeValue(api.ScenarioTypeID, sharedconstants.WorkflowProfileAttributeIndexes))
				'Dim nvbParams As name
				
				'Dim nvb As New NameValueFormatBuilder(api.WorkflowProfile.GetAttributeValue(api.ScenarioTypeID, sharedConstants.WorkflowProfileAttributeIndexes.Text4))
				
				
				' GENERACION ARCHIVO
				Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)
		        Dim folderPath As String = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName)
		        If Not Directory.Exists(folderPath) Then Directory.CreateDirectory(folderPath)
		   
		        Dim filePath As String = folderPath & "\ORDE_Extract1_" & DateTime.UtcNow.ToString("yyyyMMdd") & ".csv"        
		        If File.Exists(filePath) Then File.Delete(filePath)	
				
				Dim dt As New DataTable
				Dim csvString As New Text.StringBuilder()
				
					
				'BUCLE POR DIMENSIONES
					
					Dim dimPkSociedad As DimPk = BRApi.Finance.dim.GetDimPk(si, "Entities")
					Dim padreIdSociedad As Object = BRApi.Finance.Members.GetMemberId(si, 0, "LEGAL")
					Dim listaSociedades = brapi.Finance.members.GetBaseMembers(si, dimPkSociedad, padreIdSociedad)
					
				Dim  contador As Integer = 1
					'For contador As Integer = 1 To 3 
				For Each miembroSociedad In listaSociedades
				
					Dim nombreSociedad As String = miembroSociedad.Name
					brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "Analytics_Entity", nombreSociedad)
					
					
					'api.Pov.Entity = "E105"
					
					Dim cubeViewName As String = "ORDE_Extract1"
					Dim entityDimName As String = "Entities"
					Dim entityMemFilter As String = "E#E101"
					Dim scenarioDimName As String = "ORDE_Scenarios"
					Dim scenarioMemFilter As String = "Real"
					Dim timeMemFilter As String = "2020M10"
					
					
					', includeCellTextCols, useStandardFactTableFields, filter, 
					'Dim filter As String = nvbParams.Namevaluepairs.XFGetValue("Filter",String.Empty)
					Dim parallelQueryCount As Integer = 8
					Dim logStatistics As Boolean = False
					
					
					
					'LLAMADA AL CUBE VIEW
					
					dt = BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewName, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, 
						timeMemFilter, Nothing, True, False, Nothing, parallelQueryCount, logStatistics) 'nvbParams, includeCellTextCols, useStandardFactTableFields, filter
					
								
						
					'entityMemFilter	= "E105"
					
					'brapi.Import.Data.fdxexe
					'dt.Merge(BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewName, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, 
						'timeMemFilter, Nothing, True, True, Nothing, parallelQueryCount, logStatistics),True )'nvbParams, includeCellTextCols, useStandardFactTableFields, filter
						
				If dt IsNot Nothing Then
							
				
							  					 
					

					' COPIA AL ARCHIVO
				
						
						Dim csvRows As DataRow() = dt.select()    'dt.select()
						'Dim csvString As New Text.StringBuilder()
						
						For Each fila As DataRow In csvRows
							'csvString.Append (fila.tostring & " ")
							For Each elemento As Object In fila.ItemArray 
								csvstring.Append (elemento.tostring & ",")
							Next
							csvstring.Append (vbCrLf)
						Next
						
						'Dim csvString2 As String = dt.GetObjectData()
				
					End If
						csvstring.Append (contador & " " & nombreSociedad & vbCrLf)
						system.IO.file.WriteAllText(filePath,csvString.tostring)
			
					contador = contador + 1
				Next 

				
					
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace