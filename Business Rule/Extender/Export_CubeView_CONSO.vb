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


Namespace OneStream.BusinessRule.Extender.Export_CubeView_CONSO
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ExtenderArgs) As Object 'ByVal api As Transformer
			Try
				'Dim nvbParams As NameValueFormatBuilder (api.WorkflowProfile.GetAttributeValue(api.ScenarioTypeID, sharedconstants.WorkflowProfileAttributeIndexes))
				'Dim nvbParams As name
				
				'Dim nvb As New NameValueFormatBuilder(api.WorkflowProfile.GetAttributeValue(api.ScenarioTypeID, sharedConstants.WorkflowProfileAttributeIndexes.Text4))
				
				
				
				'VARIABLES
				Dim dt As New DataTable
				Dim dtTop As New DataTable
				
				Dim cubo As String = "CONSO"
				Dim cubeViewNameEuros As String = "CONS_Extract_Euros"
				Dim cubeViewNameLocal As String = "CONS_Extract_Local"
				Dim cubeViewNameTop As String = "CONS_Top"
				
				' CREACION ARCHIVO
				Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)
		        Dim folderPath As String = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName) & "\Analytics"
		        If Not Directory.Exists(folderPath) Then Directory.CreateDirectory(folderPath)
		   
		        Dim filePathEuros As String = folderPath & "\" & cubeViewNameEuros & "_" & DateTime.UtcNow.ToString("yyyyMMdd") & ".csv"
				If File.Exists(filePathEuros) Then File.Delete(filePathEuros)	
				Dim filePathLocal As String = folderPath & "\" & cubeViewNameLocal & "_" & DateTime.UtcNow.ToString("yyyyMMdd") & ".csv"
		        If File.Exists(filePathLocal) Then File.Delete(filePathLocal)	
								
				Dim csvStringEuros As New Text.StringBuilder()
				Dim csvStringLocal As New Text.StringBuilder()
				
				
				Dim scenarioDimName As String = "CONS_Scenarios"
				
				Dim entityMemFilter As String = "E#E101"
				Dim scenarioMemFilter As String = "R"
				Dim timeMemFilter As String = "2020M10"
				
				
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
				
				'CECO
				Dim cecoDimName As String = "CONS_CostCenters"
				Dim cecoTopMember As String = "Top"
				Dim cecoDimTypeId As Integer = 11
				Dim cecoParametro As String = "Analytics_Ceco"
				
				Dim dimPkCeco As DimPk = BRApi.Finance.dim.GetDimPk(si, cecoDimName)
				Dim padreIdCeco As Object = BRApi.Finance.Members.GetMemberId(si, cecoDimTypeId, cecoTopMember)
				Dim listaCecos = brapi.Finance.members.GetBaseMembers(si, dimPkCeco, padreIdCeco)
				
				'IC
				Dim icDimName As String = "Entities"
				Dim icTopMember As String = "LEGAL"
				Dim icDimTypeId As Integer = 0
				Dim icParametro As String = "Analytics_IC"
				
				Dim dimPkIC As DimPk = BRApi.Finance.dim.GetDimPk(si, icDimName)
				Dim padreIdIC As Object = BRApi.Finance.Members.GetMemberId(si, icDimTypeId, icTopMember)
				Dim listaICs = brapi.Finance.members.GetBaseMembers(si, dimPkIC, padreIdIC)
				
				'UN
				Dim unDimName As String = "CONS_BusinessUnits"
				Dim unTopMember As String = "Top"
				Dim unDimTypeId As Integer = 9
				Dim unParametro As String = "Analytics_UN"
				
				Dim dimPkUN As DimPk = BRApi.Finance.dim.GetDimPk(si, unDimName)
				Dim padreIdUN As Object = BRApi.Finance.Members.GetMemberId(si, unDimTypeId, unTopMember)
				Dim listaUNs = brapi.Finance.members.GetBaseMembers(si, dimPkUN, padreIdUN)
				
				'INDICES+RH Dinamicos/Almacenados
				Dim cuentaDimName As String = "CONS_Accounts"
				Dim indiceTopMember As String = "INDICES"
				Dim rhTopMember As String = "RH"
				Dim cuentaDimTypeId As Integer = 5
				Dim ctasDinParametro As String = "Analytics_CtasDinamicas"
				Dim ctasAlmParametro As String = "Analytics_CtasAlmacenadas"
				Dim ctasDinString As New Text.StringBuilder()
				Dim ctasAlmString As New Text.StringBuilder()
				
				Dim dimPkCuenta As DimPk = BRApi.Finance.dim.GetDimPk(si, cuentaDimName)
				
				Dim padreIdIndice As Object = BRApi.Finance.Members.GetMemberId(si, cuentaDimTypeId, indiceTopMember)
				Dim listaCuentas As List(Of Member) = brapi.Finance.members.GetBaseMembers(si, dimPkcuenta, padreIdIndice)
				
				Dim padreIdRh As Object = BRApi.Finance.Members.GetMemberId(si, cuentaDimTypeId, rhTopMember)
				listaCuentas.AddRange(brapi.Finance.Members.GetBaseMembers(si, dimPkcuenta, padreIdRh))
				'listaCuentas.AddRange(brapi.Finance.Account.)
				
				Dim listaDynamicCalc As New List(Of Member)
				Dim listaAlmacenadas As New List(Of Member)
				
				For Each miembro In listaCuentas
					If brapi.Finance.Account.GetAccountType(si, brapi.Finance.Members.GetMemberId(si, cuentaDimTypeId, miembro.Name)).Name = "DynamicCalc"
						listaDynamicCalc.Add(miembro)
						ctasDinString.Append (",A#" &miembro.Name)
						'system.IO.file.WriteAllText(filePath,",A#" &miembro.Name)
					Else
						listaAlmacenadas.Add(miembro)
						ctasAlmString.Append (",A#" &miembro.Name)
					End If
				Next
				
				'system.IO.file.WriteAllText(filePath,ctasAlmString.ToString)
				'Return Nothing
				
				'vk
				'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, ctasDinParametro, ctasDinString.ToString)
				brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, ctasAlmParametro, ctasAlmString.ToString)
				
					
				'BUCLE POR DIMENSIONES
				Dim  contador As Integer = 1
				
				'SOCIEDAD	
				For Each miembroSociedad In listaSociedades
				
					Dim nombreSociedad As String = miembroSociedad.Name
					brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, entityParametro, miembroSociedad.Name)
					
					
					
					'IC	
					For Each miembroIC In listaICs
				
						Dim nombreIC As String = miembroIC.Name
						brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, icParametro, nombreIC)
						
						'comprobamos si tiene en Ceco Top, UN Top antes de bajar, llamando al CubeViewTop
						brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, cecoParametro, cecoTopMember)
						brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, unParametro, unTopMember)
						
						dtTop = BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewNameTop, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, 
							timeMemFilter, Nothing, True, False, Nothing, parallelQueryCount, logStatistics) 'nvbParams, includeCellTextCols, useStandardFactTableFields, filter
					
						If dtTop IsNot Nothing Then
						
						csvstringEuros.Append (contador & " - entity: " & miembroSociedad.Name & " - IC: " & miembroIC.Name & " - Ceco: " & cecoTopMember & vbCrLf)
						csvstringLocal.Append (contador & " - entity: " & miembroSociedad.Name & " - IC: " & miembroIC.Name & " - Ceco: " & cecoTopMember & vbCrLf)
					
						'CECO	
						For Each miembroCeco In listaCecos
					
							Dim nombreCeco As String = miembroCeco.Name
							brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, cecoParametro, nombreCeco)
						
							dtTop = BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewNameTop, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, 
								timeMemFilter, Nothing, True, False, Nothing, parallelQueryCount, logStatistics) 'nvbParams, includeCellTextCols, useStandardFactTableFields, filter
					
					
							If dtTop IsNot Nothing Then 'si hay dato en la combinacion actual de IC-Ceco, lanzamos ejecucion
							
								'UN	
								For Each miembroUN In listaUNs
							
									Dim nombreUN As String = miembroUN.Name
									brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, unParametro, nombreUN)
								
									dtTop = BRApi.Import.Data.FdxExecuteCubeView(si, cubeViewNameTop, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, 
										timeMemFilter, Nothing, True, False, Nothing, parallelQueryCount, logStatistics) 'nvbParams, includeCellTextCols, useStandardFactTableFields, filter
							
							
									If dtTop IsNot Nothing Then 'si hay dato en la combinacion actual de IC-Ceco, lanzamos ejecucion	



									'LLAMADA AL CUBE VIEW EUROS
							
											'csvstring.Append("cruce top valido: " & nombreSociedad & " - " & nombreIC  & vbCrLf)
											csvstringEuros.Append("cruce top valido - Entity: " & miembroSociedad.Name & " - IC: " & miembroIC.Name & " - Ceco: " & miembroCeco.Name & " - UN: " & miembroUN.Name  & vbCrLf)
									
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
											
											
										'LLAMADA AL CUBE VIEW LOCAL
							
											'csvstring.Append("cruce top valido: " & nombreSociedad & " - " & nombreIC  & vbCrLf)
											csvstringLocal.Append("cruce top valido - Entity: " & miembroSociedad.Name & " - IC: " & miembroIC.Name & " - Ceco: " & miembroCeco.Name & " - UN: " & miembroUN.Name  & vbCrLf)
									
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
									End If
								Next
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