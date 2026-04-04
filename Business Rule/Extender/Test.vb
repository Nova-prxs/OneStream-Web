Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace OneStream.BusinessRule.Extender.Test
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
'				Select Case args.FunctionType
					
'					Case Is = ExtenderFunctionType.Unknown
						
'					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
'					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
'						'Add External Members
'						Dim externalMembers As New List(Of NameValuePair)
'						externalMembers.Add(New NameValuePair("YourMember1Name","YourMember1Value"))
'						externalMembers.Add(New NameValuePair("YourMember2Name","YourMember2Value"))
'						Return externalMembers
'				End Select


'Dim DeleteSQL As String = $"ALTER TABLE XFC_VAT_Plan ALTER COLUMN [VAT] decimal(5,2);"
'Dim DeleteSQL As String = $"ALTER TABLE XFC_VAT_Plan ALTER COLUMN [Useful_life] decimal(5,2);"
'Dim DeleteSQL As String = $"ALTER TABLE XFC_Capex_Plan ALTER COLUMN [VAT] decimal(5,2);"
Dim DeleteSQL As String = $"ALTER TABLE XFC_Capex_Plan ALTER COLUMN [Useful_life] decimal(5,2);"
'Dim DeleteSQL As String = $"ALTER TABLE XFC_Capex_Plan ALTER COLUMN [Description] nvarchar(600);"
					
Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						BRApi.Database.ExecuteActionQuery(dbconn, DeleteSQL, False, True)
					End Using

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace