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

Namespace OneStream.BusinessRule.Extender.Export_CubeView_ORDER_simplificado
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ExtenderArgs) As Object 'ByVal api As Transformer
			Try
				'Dim nvbParams As NameValueFormatBuilder (api.WorkflowProfile.GetAttributeValue(api.ScenarioTypeID, sharedconstants.WorkflowProfileAttributeIndexes))
				'Dim nvbParams As name
				
				'Dim nvb As New NameValueFormatBuilder(api.WorkflowProfile.GetAttributeValue(api.ScenarioTypeID, sharedConstants.WorkflowProfileAttributeIndexes.Text4))
				
				
				
				'VARIABLES
				Dim dt As New DataTable
				Dim dtTop As New DataTable
				
				'Dim scenarioParametro As String = "Analytics_Scenario"
				Dim scenarioMiembro As String = "Real"
				'BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, scenarioParametro)
				
				Dim cubo As String = "ORDER"
				Dim cubeViewNameEuros As String = "ORDER_Extract_Euros"
				Dim cubeViewNameLocal As String = "ORDER_Extract_Local"
				Dim archivoEuros As String = cubeViewNameEuros & "_" & scenarioMiembro & "_simplificado_"
				Dim archivoLocal As String = cubeViewNameLocal & "_" & scenarioMiembro & "_simplificado_"
				Dim cubeViewNameTop As String = "ORDER_Top"
				
				'Dim cubeViewNameEurosLegal As String = "CONS_Extract_Euros_Legal"
				'Dim cubeViewNameLocalLegal As String = "CONS_Extract_Local_Legal"
				
				
				
				' CREACION ARCHIVO
				Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)
		        Dim folderPath As String = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName) & "\Analytics"
		        If Not Directory.Exists(folderPath) Then Directory.CreateDirectory(folderPath)
		   
		        Dim filePathEuros As String = folderPath & "\" & archivoEuros & DateTime.UtcNow.ToString("yyyyMMdd") & ".csv"
				If File.Exists(filePathEuros) Then File.Delete(filePathEuros)	
				Dim filePathLocal As String = folderPath & "\" & archivoLocal & DateTime.UtcNow.ToString("yyyyMMdd") & ".csv"
		        If File.Exists(filePathLocal) Then File.Delete(filePathLocal)	
								
				Dim csvStringEuros As New Text.StringBuilder()
				Dim csvStringLocal As New Text.StringBuilder()
				
				
				Dim scenarioDimName As String = "ORDE_Scenarios"
				
				Dim entityMemFilter As String = "E#E101"
				Dim scenarioMemFilter As String = "Real"
				Dim timeMemFilter As String = "2020M12"
				
				
				Dim parallelQueryCount As Integer = 8
				Dim logStatistics As Boolean = False
				
				'SOCIEDAD
				Dim entityDimName As String = "Entities"
				Dim entityTopMember As String = "LEGAL"
				Dim entityDimTypeId As Integer = 0
				Dim entityParametro As String = "Analytics_Entity"
				
				Dim dimPkSociedad As DimPk = BRApi.Finance.dim.GetDimPk(si, entityDimName)
				Dim padreIdSociedad As Object = BRApi.Finance.Members.GetMemberId(si, entityDimTypeId, entityTopMember)
				Dim listaSociedades = brapi.Finance.members.GetBaseMembers(si, dimPkSociedad, padreIdSociedad)
				'listaSociedades.Add(BRApi.Finance.Members.GetMember(si, entityDimTypeId, entityTopMember))
				'= listaSociedades + BRApi.Finance.Members.GetMember(si, entityDimTypeId, entityTopMember)
				
				'MONEDA LOCAL
				Dim monedaParametro As String = "Analytics_MonedaLocal"
				
				'GEOGR
				Dim geoDimName As String = "ORDE_Geographies"
				Dim geoTopMember As String = "Total Geografia"
				Dim geoDimTypeId As Integer = 12
				Dim geoParametro As String = "Analytics_Geography"
				
				Dim dimPkGeo As DimPk = BRApi.Finance.dim.GetDimPk(si, geoDimName)
				Dim padreIdGeo As Object = BRApi.Finance.Members.GetMemberId(si, geoDimTypeId, geoTopMember)
				Dim listaGeos = brapi.Finance.members.GetBaseMembers(si, dimPkGeo, padreIdGeo)
				
				'IC
				Dim icDimName As String = "Entities"
				Dim icNoneMember As String = "None" '"LEGAL"
				Dim icDimTypeId As Integer = 0
				Dim icParametro As String = "Analytics_IC"
				
				'Dim dimPkIC As DimPk = BRApi.Finance.dim.GetDimPk(si, icDimName)
				'Dim padreIdIC As Object = BRApi.Finance.Members.GetMemberId(si, icDimTypeId, icTopMember)
				'Dim listaICs = brapi.Finance.members.GetBaseMembers(si, dimPkIC, padreIdIC)
				
				'UN
				Dim unDimName As String = "ORDE_BusinessUnits"
				Dim unTopMember As String = "Top"
				Dim unDimTypeId As Integer = 9
				Dim unParametro As String = "Analytics_UN"
				
				Dim dimPkUN As DimPk = BRApi.Finance.dim.GetDimPk(si, unDimName)
				Dim padreIdUN As Object = BRApi.Finance.Members.GetMemberId(si, unDimTypeId, unTopMember)
				Dim listaUNs = brapi.Finance.members.GetBaseMembers(si, dimPkUN, padreIdUN)
				'listaUNs.Add(BRApi.Finance.Members.GetMember(si, unDimTypeId, unTopMember))
				'Dim listaUNs = brapi.Finance.members.GetBaseMembers(si, dimPkUN, padreIdUN)
				
				
				
					
				'BUCLE POR DIMENSIONES
				Dim  contador As Integer = 1
				
				'IC	fijo None
				brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, icParametro, icNoneMember)
				
				'SOCIEDAD	
				For Each miembroSociedad In listaSociedades
				
					Dim miembroMoneda As String = BRApi.Finance.Entity.GetLocalCurrency(si, miembroSociedad.MemberId).Name
					brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, monedaParametro, miembroMoneda)
					
					brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, entityParametro, miembroSociedad.Name)
					
					
							
					'UN	
					For Each miembroUN In listaUNs
				
						Dim nombreUN As String = miembroUN.Name
						brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, unParametro, nombreUN)
						
						'GEO = Total Paises
						brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, geoParametro, geoTopMember)
					
						dtTop = BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewNameTop, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, 
							timeMemFilter, Nothing, True, False, Nothing, parallelQueryCount, logStatistics) 'nvbParams, includeCellTextCols, useStandardFactTableFields, filter
				
				
						If dtTop IsNot Nothing Then 'si hay dato en la combinacion actual de Entity-UN, lanzamos ejecucion para paises


						'GEO
						For Each miembroGeo In listaGeos
						
							Dim nombreGeo As String = miembroGeo.Name
							brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, geoParametro, nombreGeo)
							
							dtTop = BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewNameTop, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, 
								timeMemFilter, Nothing, True, False, Nothing, parallelQueryCount, logStatistics) 'nvbParams, includeCellTextCols, useStandardFactTableFields, filter

							If dtTop IsNot Nothing Then 'si hay dato en el pais actual, lanzamos extraccion para clientes

								'LLAMADA AL CUBE VIEW EUROS GESTION
						
										'csvstringEuros.Append("cruce top valido - Entity: " & miembroSociedad.Name & " - IC: " & miembroIC.Name & " - Ceco: " & miembroCeco.Name & " - UN: " & miembroUN.Name  & vbCrLf)
								
										dt = BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewNameEuros, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, 
											timeMemFilter, Nothing, True, False, Nothing, parallelQueryCount, logStatistics) 'nvbParams, includeCellTextCols, useStandardFactTableFields, filter
								
													
										' COPIA AL ARCHIVO
										If dt IsNot Nothing Then	
										
											Dim csvRows As DataRow() = dt.select()   
											
											'csvstring.Append (contador & " - " & nombreSociedad & " - " & nombreCeco & vbCrLf)
											
											For Each fila As DataRow In csvRows
												For Each elemento As Object In fila.ItemArray 
													csvstringEuros.Append (elemento.tostring & ",")
												Next
												csvstringEuros.Append (vbCrLf)
											Next
								
											system.IO.file.WriteAllText(filePathEuros,csvstringEuros.tostring)
						
										End If
									
									'LLAMADA AL CUBE VIEW EUROS LEGAL
						
										
										
										
									'LLAMADA AL CUBE VIEW LOCAL-GESTION
						
										'csvstringLocal.Append("cruce top valido - Entity: " & miembroSociedad.Name & " - IC: " & miembroIC.Name & " - Ceco: " & miembroCeco.Name & " - UN: " & miembroUN.Name  & vbCrLf)
								
										dt = BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewNameLocal, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, 
											timeMemFilter, Nothing, True, False, Nothing, parallelQueryCount, logStatistics) 'nvbParams, includeCellTextCols, useStandardFactTableFields, filter
								
													
										' COPIA AL ARCHIVO
										If dt IsNot Nothing Then	
										
											Dim csvRows As DataRow() = dt.select()   
											
											'csvstring.Append (contador & " - " & nombreSociedad & " - " & nombreCeco & vbCrLf)
											
											For Each fila As DataRow In csvRows
												For Each elemento As Object In fila.ItemArray 
													csvstringLocal.Append (elemento.tostring & ",")
												Next
												csvstringLocal.Append (vbCrLf)
											Next
								
											system.IO.file.WriteAllText(filePathLocal,csvstringLocal.tostring)
						
										End If
										
										'LLAMADA AL CUBE VIEW LOCAL
						
							End If

						Next
						
					End If
				Next
					
				contador = contador + 1
			Next 

				
					
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace