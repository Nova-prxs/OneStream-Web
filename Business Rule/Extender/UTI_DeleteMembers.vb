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

Namespace OneStream.BusinessRule.Extender.UTI_DeleteMembers
	
	Public Class MainClass
		
#Region "Main"
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
				
				Try
					
					Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)

					Dim dimensionPK As DimPk = BRApi.Finance.Dim.GetDimPk(si,"Accounts")
					Dim nValue As Integer = BRApi.Finance.Members.GetMemberId(si, dimensionPK.DimTypeId,"AL_RDI")
					
					Dim objList1 As List(Of Member) = BRApi.Finance.Members.GetAllMembers(si,dimensionPK,nValue)
					Dim objList2 As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,dimensionPK,nValue)
					
					Dim mbrct As Integer = objList1.Count
					
					While mbrct > 3
					
						For Each mbrbe As Member In objList2
						
							If mbrbe.Name <> "None" Then
							BRApi.Finance.MemberAdmin.RemoveMember(si, dimensionPK, mbrbe.MemberPk)
							
							End If
							
						Next
						
						objList1 = BRApi.Finance.Members.GetAllMembers(si,dimensionPK,nValue)
						objList2 = BRApi.Finance.Members.GetBaseMembers(si,dimensionPK,nValue)
						mbrct = objList1.Count
					
					End While
					
					Return Nothing
					
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				End Try
			End Function
			
#End Region
		
	End Class
End Namespace