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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.main_solution_helper
	Public Class MainSolutionHelper
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
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


        Public Shared Function GetTransformationRulesMapping(ByVal si As SessionInfo) As Dictionary(Of String, String)
            Dim rulesMapping As New Dictionary(Of String, String)
            
            Try
                ' Obtener el rule group TR_CostCenter
                Dim ruleGroup = BRApi.Import.Metadata.GetRuleGroup(si, "TR_CostCenter", True)
                
                If ruleGroup IsNot Nothing AndAlso ruleGroup.Rules IsNot Nothing Then
                    
                    For Each rule As Object In ruleGroup.Rules
                        Try
                            ' Leer las propiedades usando late binding
                            Dim sourceValue As String = rule.RuleName  ' Este es el cost center
                            Dim targetValue As String = ""
                            
                            Try
                                ' Intentar diferentes nombres posibles para la propiedad del target
                                If rule.GetType().GetProperty("TargetMember") IsNot Nothing Then
                                    targetValue = rule.TargetMember?.ToString()
                                ElseIf rule.GetType().GetProperty("Target") IsNot Nothing Then
                                    targetValue = rule.Target?.ToString()
                                ElseIf rule.GetType().GetProperty("MemberName") IsNot Nothing Then
                                    targetValue = rule.MemberName?.ToString()
                                ElseIf rule.GetType().GetProperty("OneStreamMemberName") IsNot Nothing Then
                                    targetValue = rule.OneStreamMemberName?.ToString()
                                Else
                                    ' Usar reflection para encontrar la propiedad correcta
                                    Dim ruleType As Type = rule.GetType()
                                    Dim properties() As System.Reflection.PropertyInfo = ruleType.GetProperties()
                                    
                                    For Each prop As System.Reflection.PropertyInfo In properties
                                        ' Buscar propiedades que puedan contener el target value
                                        If prop.PropertyType Is GetType(String) AndAlso 
                                           (prop.Name.Contains("Target") OrElse 
                                            prop.Name.Contains("Member") OrElse 
                                            prop.Name.Contains("Value")) AndAlso
                                           prop.Name <> "RuleName" Then
                                            
                                            Try
                                                Dim value As Object = prop.GetValue(rule, Nothing)
                                                If value IsNot Nothing AndAlso Not String.IsNullOrEmpty(value.ToString()) Then
                                                    targetValue = value.ToString()
                                                    Exit For
                                                End If
                                            Catch
                                                Continue For
                                            End Try
                                        End If
                                    Next
                                End If
                                
                            Catch propEx As Exception
                                ' Si falla todo, continuar con la siguiente regla
                                Continue For
                            End Try
                            
                            If Not String.IsNullOrEmpty(sourceValue) AndAlso Not String.IsNullOrEmpty(targetValue) Then
                                ' Agregar al diccionario (sourceValue es el cost center, targetValue es el group)
                                If Not rulesMapping.ContainsKey(sourceValue) Then
                                    rulesMapping.Add(sourceValue, targetValue)
                                End If
                            End If
                            
                        Catch ruleEx As Exception
                            ' Continuar con la siguiente regla
                            Continue For
                        End Try
                    Next
                    
                End If
                
            Catch ex As Exception
                ' Retornar diccionario vacío si hay error
            End Try
            
            Return rulesMapping
        End Function
        
    End Class
End Namespace