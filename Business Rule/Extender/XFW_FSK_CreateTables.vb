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

Namespace OneStream.BusinessRule.Extender.XFW_FSK_CreateTables
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
							Dim sql As New Text.StringBuilder
							sql.Append("IF EXISTS (SELECT 1 FROM information_schema.tables WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = 'XFW_FSK_EliminationLineItem')")
							sql.Append("BEGIN ")
							sql.Append("DROP TABLE [dbo].[XFW_FSK_EliminationLineItem]")
							sql.Append("END ")
							sql.AppendLine()
							sql.AppendLine()
							sql.Append("CREATE TABLE [dbo].[XFW_FSK_EliminationLineItem](")
							sql.Append("[UniqueID] [uniqueidentifier] Not NULL,")
							sql.Append("[EliminationID] [uniqueidentifier] Not NULL,")
							sql.Append("[SortOrder] [int] Not NULL,")
							sql.Append("[CubeId] [int] Not NULL,")
							sql.Append("[IsBS] [bit] Not NULL,")
							sql.Append("[IsIS] [bit] Not NULL,")
							sql.Append("[EntityId] [int] Not NULL,")
							sql.Append("[ParentId] [int] Not NULL,")
							sql.Append("[ConsId] [int] Not NULL,")
							sql.Append("[ScenarioId] [int] Not NULL,")
							sql.Append("[TimeId] [int] Not NULL,")
							sql.Append("[AccountId] [int] Not NULL,")
							sql.Append("[FlowId] [int] Not NULL,")
							sql.Append("[ICId] [int] Not NULL,")
							sql.Append("[UD1Id] [int] Not NULL,")
							sql.Append("[UD2Id] [int] Not NULL,")
							sql.Append("[UD3Id] [int] Not NULL,")
							sql.Append("[UD4Id] [int] Not NULL,")
							sql.Append("[UD5Id] [int] Not NULL,")
							sql.Append("[UD6Id] [int] Not NULL,")
							sql.Append("[UD7Id] [int] Not NULL,")
							sql.Append("[UD8Id] [int] Not NULL,")
							sql.Append("[Description] [nvarchar](max) Not NULL,")
							sql.Append("[LineItemType] [int] Not NULL,")
							sql.Append("[Amount] [decimal](28, 9) Not NULL,")
							sql.Append("[BRLine] [int] Not NULL,")
							sql.Append("[ConsRule] [nvarchar](80) Not NULL,")
							sql.Append("CONSTRAINT [PK_XFW_FSK_EliminationLineItem] PRIMARY KEY NONCLUSTERED ")
							sql.Append("(")
							sql.Append("[UniqueID] ASC")
							sql.Append(")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON))")
							sql.Append("IF EXISTS (SELECT 1 FROM information_schema.tables WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = 'XFW_FSK_PelimPartners')")
							sql.Append("BEGIN ")
							sql.Append("DROP TABLE [dbo].[XFW_FSK_PelimPartners]")
							sql.Append("END ")
							sql.Append("CREATE TABLE [dbo].[XFW_FSK_PelimPartners](")
							sql.Append("[UniqueId] [uniqueidentifier] Not NULL,")
							sql.Append("[ScenarioId] [int] Not NULL,")
							sql.Append("[TimeId] [int] Not NULL,")
							sql.Append("[CubeId] [int] Not NULL,")
							sql.Append("[EntityIDforElim] [int] Not NULL,")
							sql.Append("[ICIDforElim] [int] Not NULL,")
							sql.Append("[EntityNameforElim] CHAR(100) Not NULL,") 
							sql.Append("[ICNameforElim] CHAR(100) Not NULL)") 

							
							brapi.Database.ExecuteSql(dbconnapp, sql.ToString, False)

						End Using
				Return Nothing						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						'Add External Members
						Dim externalMembers As New List(Of NameValuePair)
						externalMembers.Add(New NameValuePair("YourMember1Name","YourMember1Value"))
						externalMembers.Add(New NameValuePair("YourMember2Name","YourMember2Value"))
						Return externalMembers
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace