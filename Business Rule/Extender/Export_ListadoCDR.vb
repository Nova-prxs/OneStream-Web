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

Namespace OneStream.BusinessRule.Extender.Export_ListadoCDR
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ExtenderArgs) As Object 'ByVal api As Transformer
			Try
				
				'VARIABLES
			
				Dim cubo As String = "CONSO"
				Dim cubeViewName As String = "CONS_Extract1"
				Dim cubeViewNameTop As String = "CONS_Top"
							
				Dim scenarioDimName As String = "CONS_Scenarios"
				
				Dim scenarioMemFilter As String = "R"
				
				Dim parallelQueryCount As Integer = 6
				Dim logStatistics As Boolean = False
				
				'INDICES+RH Dinamicos/Almacenados
				Dim cuentaDimName As String = "CONS_Accounts"
				Dim cdrTopMember As String = "CDR"
				Dim cuentaDimTypeId As Integer = 5
				Dim ctasCdrParametro As String = "Analytics_CtasCDR"
				Dim ctasCdrString As New Text.StringBuilder()
				
				Dim dimPkCuenta As DimPk = BRApi.Finance.dim.GetDimPk(si, cuentaDimName)
				
				Dim padreIdCdr As Object = BRApi.Finance.Members.GetMemberId(si, cuentaDimTypeId, cdrTopMember)
				Dim listaCuentas As List(Of Member) = brapi.Finance.members.GetDescendants(si, dimPkcuenta, padreIdCdr)
				
				Dim listaCDR As New List(Of Member)
				
				For Each miembro In listaCuentas
					If Not BRApi.Finance.Members.IsBase(si, dimPkCuenta, padreIdCdr, brapi.Finance.Members.GetMemberId(si, cuentaDimTypeId, miembro.Name)) And 
					miembro.Name.StartsWith("CR")	
						listaCDR.Add(miembro)
						ctasCdrString.Append (",A#" &miembro.Name)
					End If
				Next
	
				brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, ctasCdrParametro, ctasCdrString.ToString)
	
			
			Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace