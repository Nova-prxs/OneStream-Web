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
	Public Class sharedClass

		Public Function queriesCommon (ByVal si As SessionInfo, ByVal query As String) As String
            Try
				
				Dim sql As String = String.Empty
				
				If query = "DistribucionCostes"
					
					#Region "Consulta General"							
						sql = "HOLA"			
					#End Region	
									
				ElseIf query = ""
					
				End If	
				Return sql 	
			
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

	End Class
End Namespace
