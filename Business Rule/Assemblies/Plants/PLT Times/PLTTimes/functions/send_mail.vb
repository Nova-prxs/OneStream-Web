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

'Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.send_mail
Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.send_mail
	

      Public Class MainClass

      Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
            ' Flag for errors
            Dim exFlag As Boolean = False
            Dim exMsg As String = String.Empty

            ' Method name
            Dim methodName As String = System.Reflection.MethodInfo.GetCurrentMethod().Name
			
            Try
                  	' Log start
                 	 BRApi.ErrorLog.LogMessage(si, "Starting Send Mail Business Rule...")
					 
				  	' == PARTAMETERS ==
					
					' Mejorar las funciones para que el admniistrador pueda gestionar los correos de destino desde una tabla
					' en el sistema.
					
					Dim mailList As String = args.NameValuePairs.GetValueOrDefault("mailList", "NotRecived")
					Dim currentTime = Now
					' Recived 
					Dim sFactory As String = 		args.NameValuePairs.GetValueOrEmpty("factory")
					Dim sProduct As String = 		args.NameValuePairs.GetValueOrEmpty("product")
					Dim sGM As String = 			args.NameValuePairs.GetValueOrEmpty("gm")
					Dim sCC As String = 			args.NameValuePairs.GetValueOrEmpty("cc")
					Dim sTime As String = 			args.NameValuePairs.GetValueOrEmpty("time")
					Dim sTimeUnit As String = 		args.NameValuePairs.GetValueOrEmpty("time_unit")
					Dim sStartDate As String = 		args.NameValuePairs.GetValueOrEmpty("start_date")
					Dim sEndDateOption As String = 	args.NameValuePairs.GetValueOrEmpty("end_date_option")
					Dim sNewValue As String = 		args.NameValuePairs.GetValueOrEmpty("new_value").Replace(",",".")
					Dim sComment As String = 		args.NameValuePairs.GetValueOrEmpty("comment") & $" - Modified by {si.UserName} on {currentTime}"
					Dim sEndDate As String = 		args.NameValuePairs.GetValueOrEmpty("end_date")
					Dim sNewValueUO1 As String = 	args.NameValuePairs.GetValueOrEmpty("new_value_UO1")
					Dim sNewValueUO2 As String = 	args.NameValuePairs.GetValueOrEmpty("new_value_UO2")
					Dim message As String = 		args.NameValuePairs.GetValueOrDefault("message", "NotRecived")
					Brapi.ErrorLog.LogMessage(si, "Entrando al DT")
					Dim dt As DataTable = Nothing 
					If sFactory<> String.Empty
						dt = MailInfoHelpers.GetDataTable(si, globals, api, args)	
					End If
					' Brapi.ErrorLog.LogMessage(si, "DT Obtenido: "& dt.Rows.Count)
					' Crear el objeto MailInfo con los datos
					Dim mailInfo As New MailInfoHelpers() With {
					    .sProduct = sProduct,
					    .sCC = sCC,
					    .sGM = sGM,
					    .sTime = sTime,
					    .sTimeUnit = sTimeUnit,
					    .sStartDate = sStartDate,
					    .sNewValue = sNewValue,
					    .sComment = sComment,
					    .sNewValueUO1 = sNewValueUO1,  
					    .sNewValueUO2 = sNewValueUO2,
					    .sEndDate = sEndDate,
						.dtData = dt
					}
					
					' == Get Email Parameters ==
				  	
                  	' Get connection name from dashboard parameter
                  	Dim emailConnectionName As String = EmailHelpers.GetEmailConnectionName(si)
                  	BRApi.ErrorLog.LogMessage(si, $"Email Connection: {emailConnectionName}")
				  	
                  	If String.IsNullOrEmpty(emailConnectionName) Then
                  	      Throw New XFUserMsgException(si, Nothing, Nothing, "Email Connection Name is empty")
                  	End If
				  	
                  	' Get distribution list from dashboard parameter
                  	Dim toList As List(Of String) = EmailHelpers.GetTolist(si, mailList)
                  	BRApi.ErrorLog.LogMessage(si, $"To List: {String.Join(",", toList)}")
				  	
                  	If toList Is Nothing OrElse toList.Count = 0 Then
                  	      Throw New XFUserMsgException(si, Nothing, Nothing, "To List is empty")
                  	End If
				  	
                  	' Get email subject
                  	Dim emailSubject As String = EmailHelpers.GetSubject(si, False)
                  	BRApi.ErrorLog.LogMessage(si, $"Email Subject: {emailSubject}")
				  	
                  	' Get email body header and footer
                  	Dim emailHeader As String = EmailHelpers.GetBodyHeader(si)
                  	Dim emailFooter As String = EmailHelpers.GetBodyFooter(si)
				  	
                  	' Build email body content
                  	Dim emailBodyContent As String = If(message<>"NotRecived", message, "Product1 - 1 hours - 1.4 hours, Product2 - 2 hours - 0.3 hours, Product3 - 30 min - 20 min")
				  	
                  	' Get email body HTML formatted
                  	Dim emailBodyAsHtml As String = EmailHelpers.GetHtmlBody(si, emailHeader, emailBodyContent, emailFooter, MailInfo)
                  	BRApi.ErrorLog.LogMessage(si, "Email body prepared successfully")
				  	
                  	' Initialize empty attachments list
                  	Dim attachmentsList As New List(Of String)
				  	
                  	' == Send Email ==
				  	
                  	EmailHelpers.SendEmail(si, emailConnectionName, toList, emailSubject, emailBodyAsHtml, True, attachmentsList)
				  	
				  	' Log success
                  	BRApi.ErrorLog.LogMessage(si, "Email sent successfully!")
				  	
                  	Return Nothing

            Catch ex As Exception
				
				#Region "Manejo del Error"
				
                  ' Flag error
                  exFlag = True

                  ' Get exception message
                  exMsg = XFException.GetOriginalException(ex).Message

                  ' Log exception to OneStream error log
                  BRApi.ErrorLog.LogMessage(si, $"Error in {methodName}: {exMsg}")

                  ' Attempt to send error notification email
                  Try
'                        Dim connectionName As String = EmailHelpers.GetEmailConnectionName(si, SharedCS.Const_DashboardParamSuffix)
'                        Dim toList As List(Of String) = EmailHelpers.GetTolist(si, SharedCS.Const_DashboardParamPrefix, SharedCS.Const_DashboardParamSuffix)
'                        Dim subject As String = "Error - OneStream Send Mail Business Rule"
'                        Dim bodyContent As String = $"An error occurred in the Send Mail Business Rule:{Environment.NewLine}{Environment.NewLine}{exMsg}"
'                        Dim header As String = EmailHelpers.GetBodyHeader(si, SharedCS.Const_DashboardParamPrefix, SharedCS.Const_DashboardParamSuffix)
'                        Dim footer As String = EmailHelpers.GetBodyFooter(si, SharedCS.Const_DashboardParamPrefix, SharedCS.Const_DashboardParamSuffix)
'                        Dim emailBody As String = EmailHelpers.GetHtmlBody(si, header, bodyContent, footer, SharedCS.Const_DashboardParamPrefix, SharedCS.Const_DashboardParamSuffix)
'                        EmailHelpers.SendEmail(si, connectionName, toList, subject, emailBody, True, New List(Of String))
                        BRApi.ErrorLog.LogMessage(si, "Error notification email sent!")
                  Catch
                        ' Silent catch - don't throw exception for error notification emails
                  End Try

                  ' Throw exception
                  Throw New XFException(si, ex)
				  
				#End Region
				
            End Try
      End Function
	
      End Class

	#Region "Helper Class for Email Utility Functions"
			
				#Region "Get Connection, Get To List, Get Subject, Get Header, Get Footer"
	      ''' <summary>
	      ''' Helper class containing email utility functions
	      ''' </summary>
	      Public Class EmailHelpers
	
	            Public Shared Function GetEmailConnectionName(si As SessionInfo) As String
	                  Try 
						  Return BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Guid.Empty, $"!{SharedCS.Const_Workspace}.Email_Batch_ConnectionName{SharedCS.Const_DashboardParamSuffix}!")
	                  Catch ex As Exception
	                        Return String.Empty
	                  End Try
	            End Function
	
	            Public Shared Function GetTolist(si As SessionInfo, mailList As String) As List(Of String)
	                  Try
						  	If mailList <> "NotRecived"
	                        	Return mailList.Split(";"c).Where(Function(x) Not String.IsNullOrEmpty(x.Trim())).Select(Function(x) x.Trim()).ToList()
							Else	
	                        	Dim toList As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Guid.Empty, $"!{SharedCS.Const_Workspace}.{SharedCS.Const_DashboardParamPrefix}ToList{SharedCS.Const_DashboardParamSuffix}!")
	                        	Return toList.Split(";"c).Where(Function(x) Not String.IsNullOrEmpty(x.Trim())).Select(Function(x) x.Trim()).ToList()
							End If
	                  Catch ex As Exception
	                        Return New List(Of String)
	                  End Try
	            End Function
	
	            Public Shared Function GetSubject(si As SessionInfo, hasError As Boolean) As String
	                  Try
	                        If hasError Then							
	                              Return BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Guid.Empty, $"!{SharedCS.Const_Workspace}.{SharedCS.Const_DashboardParamPrefix}SubjectError{SharedCS.Const_DashboardParamSuffix}!")
	                        Else
	                              Return BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Guid.Empty, $"!{SharedCS.Const_Workspace}.{SharedCS.Const_DashboardParamPrefix}SubjectSuccess{SharedCS.Const_DashboardParamSuffix}!")
	                        End If
	                  Catch ex As Exception
	                        Return "OneStream Notification"
	                  End Try
	            End Function
	
	            Public Shared Function GetBodyHeader(si As SessionInfo) As String
	                  Try 
	                        Return BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Guid.Empty, $"!{SharedCS.Const_Workspace}.{SharedCS.Const_DashboardParamPrefix}BodyHeader{SharedCS.Const_DashboardParamSuffix}!")
	                  Catch ex As Exception
	                        Return "OneStream Notification"
	                  End Try
	            End Function
	
	            Public Shared Function GetBodyFooter(si As SessionInfo) As String
	                  Try
	                        Return BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Guid.Empty, $"!{SharedCS.Const_Workspace}.{SharedCS.Const_DashboardParamPrefix}BodyFooter{SharedCS.Const_DashboardParamSuffix}!")
	                  Catch ex As Exception
	                        Return "End of message"
	                  End Try
	            End Function
				
				#End Region
				
				#Region "Get HTML"
				
					#Region "Body"
							
				Public Shared Function GetHtmlBody(si As SessionInfo, header As String, content As String, footer As String, mailInfo As MailInfoHelpers) As String
				    Try
				        ' Determinar la acción
				        Dim action As String = ""
				        If content.Contains("Time Modified") Then
				            action = "Modified"
				        ElseIf content.Contains("Time Deleted") Then
				            action = "Deleted"
				        ElseIf content.Contains("Time Created") Then
				            action = "Created"
				        End If
				
				        ' Colores por acción
				        Dim titleColor As String = "#34495e"
				        Dim borderColor As String = "#bdc3c7"
				        Dim titleText As String = "Update"
				
				        Select Case action
				            Case "Created"
				                titleColor = "#27ae60"
				                borderColor = "#2ecc71"
				                titleText = "Time Created"
				            Case "Modified"
				                titleColor = "#2980b9"
				                borderColor = "#3498db"
				                titleText = "Time Modified"
				            Case "Deleted"
				                titleColor = "#c0392b"
				                borderColor = "#e74c3c"
				                titleText = "Stop Production"
				        End Select
				
				        ' Construcción del bloque de contenido
				        Dim details As String = ""
				
				        Select Case action
				            Case "Created"
				                details =
				$"
				        <p><strong>Time UO1:</strong> {mailInfo.sNewValueUO1} {mailInfo.sTimeUnit}</p>
				        <p><strong>Time UO2:</strong> {mailInfo.sNewValueUO2} {mailInfo.sTimeUnit}</p>
				        <p><strong>Start Date:</strong> {mailInfo.sStartDate}</p>
				"
				            Case "Modified"
				                details =
				$"
				        <p><strong>Cost Center:</strong> {mailInfo.sCC}</p>
				        <p><strong>Time:</strong> {mailInfo.sTime} ({mailInfo.sTimeUnit})</p>
				        <p><strong>Start Date:</strong> {mailInfo.sStartDate}</p>
				        <p><strong>New Value:</strong> {mailInfo.sNewValue}</p>
				"
				            Case "Deleted"
				                details =
				$"
				        <p><strong>End Date:</strong> {mailInfo.sEndDate}</p>
				"
				        End Select
				
						' === Mini gráfico (timeline) a la derecha ===
						' Dim graphHtml As String = If(mailInfo.dtData.Rows.Count > 0, BuildBeforeAfterUOTimelineHtml(mailInfo.dtData),"")
						
						#Region "Old Graph"
						
'						Dim dotInactive As String = "#d0d0d0"
'						Dim dotCreated As String = If(action = "Created", borderColor, dotInactive)
'						Dim dotModified As String = If(action = "Modified", borderColor, dotInactive)
'						Dim dotDeleted As String = If(action = "Deleted", borderColor, dotInactive)
						
'						Dim graphHtml As String =
'						$"
'						<div style='font-size: 12px; color:#777; text-align:center;'>
'						    <div style='font-weight:bold; color:#666; margin-bottom:8px;'>Timeline</div>
						
'						    <div style='white-space:nowrap;'>
'						        <span style='display:inline-block;width:10px;height:10px;border-radius:50%;background:{dotCreated};'></span>
'						        <span style='display:inline-block;width:28px;height:2px;background:#e0e0e0;vertical-align:middle;'></span>
						
'						        <span style='display:inline-block;width:10px;height:10px;border-radius:50%;background:{dotModified};'></span>
'						        <span style='display:inline-block;width:28px;height:2px;background:#e0e0e0;vertical-align:middle;'></span>
						
'						        <span style='display:inline-block;width:10px;height:10px;border-radius:50%;background:{dotDeleted};'></span>
'						    </div>
						
'						    <div style='margin-top:6px;'>
'						        <span style='display:inline-block;width:55px;'>Created</span>
'						        <span style='display:inline-block;width:55px;'>Modified</span>
'						        <span style='display:inline-block;width:55px;'>Deleted</span>
'						    </div>
'						</div>
'						"

						#End Region
						
				        ' TARJETA FINAL
						
						#Region "Versión con Gráfico"
						
'						Dim cardHtml As String =
'						$"
'						<div style='border-left: 6px solid {borderColor}; padding: 20px; margin-bottom: 25px;
'						            background: #ffffff; border-radius: 10px; box-shadow: 0 2px 8px rgba(0,0,0,0.07);'>
						
'						    <div style='font-size: 22px; font-weight: bold; color: {titleColor}; margin-bottom: 15px;'>
'						        {titleText}
'						    </div>
						
'						    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='border-collapse:collapse;'>
'						        <tr>
'						            <!-- Columna izquierda: tu contenido -->
'						            <td valign='top' style='font-size: 15px; color: #555; padding-right: 15px;'>
'						                <p><strong>Product:</strong> {mailInfo.sProduct}</p>
'						                <p><strong>GM:</strong> {mailInfo.sGM}</p>
						
'						                {details}
						
'						                <p><strong>Comment:</strong> {mailInfo.sComment}</p>
'						            </td>
						
'						            <!-- Columna derecha: mini gráfico -->
'						            <td valign='top' width='200' style='padding-left: 15px; border-left: 1px solid #eee;'>
'						                {graphHtml}
'						            </td>
'						        </tr>
'						    </table>
'						</div>
'						"
						#End Region
						
				        Dim cardHtml As String =
				$"
				<div style='border-left: 6px solid {borderColor}; padding: 20px; margin-bottom: 25px;
				            background: #ffffff; border-radius: 10px; box-shadow: 0 2px 8px rgba(0,0,0,0.07);'>
				    <div style='font-size: 22px; font-weight: bold; color: {titleColor}; margin-bottom: 15px;'>
				        {titleText}
				    </div>
				
				    <div style='font-size: 15px; color: #555;'>
				        <p><strong>Product:</strong> {mailInfo.sProduct}</p>
				        <p><strong>GM:</strong> {mailInfo.sGM}</p>
				
				        {details}
				
				        <p><strong>Comment:</strong> {mailInfo.sComment}</p>
				    </div>
				</div>
				"
				
				        ' HTML COMPLETO
				        Dim finalHtml As String =
				$"
				<!DOCTYPE html>
				<html>
				<head>
				<meta charset='UTF-8'>
				</head>
				<body style='font-family: Arial, sans-serif; background-color: #f5f7fa; padding: 20px;'>
				
				<div style='max-width: 650px; margin: auto; background: white; border-radius: 10px;
				            box-shadow: 0 2px 10px rgba(0,0,0,0.1); padding: 30px;'>
				
				    <div style='text-align: center; font-size: 26px; font-weight: bold; color: #2c3e50; margin-bottom: 30px;'>
				        {header}
				    </div>
				
				    {cardHtml}
				
				    <div style='margin-top: 25px; font-size: 13px; color: #888; text-align: center; border-top: 1px solid #eee; padding-top: 15px;'>
				        {footer}
				    </div>
				
				</div>
				</body>
				</html>
				"
				
				        Return finalHtml
				
				    Catch ex As Exception
				        Return "<html><body>Error generating email</body></html>"
				    End Try
				End Function
				
				#End Region
				
					#Region "Attachemnt"
				
	            Public Shared Function GetHtmlAttachment(si As SessionInfo, results As DataTable) As String
	                  Try
	                        Dim htmlBuilder As New System.Text.StringBuilder
	
	                        htmlBuilder.AppendLine("<!DOCTYPE html>")
	                        htmlBuilder.AppendLine("<html>")
	                        htmlBuilder.AppendLine("<head>")
	                        htmlBuilder.AppendLine("<meta charset='UTF-8'>")
	                        htmlBuilder.AppendLine("<style>")
	                        htmlBuilder.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }")
	                        htmlBuilder.AppendLine("table { border-collapse: collapse; width: 100%; }")
	                        htmlBuilder.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }")
	                        htmlBuilder.AppendLine("th { background-color: #4CAF50; color: white; }")
	                        htmlBuilder.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }")
	                        htmlBuilder.AppendLine("</style>")
	                        htmlBuilder.AppendLine("</head>")
	                        htmlBuilder.AppendLine("<body>")
	                        htmlBuilder.AppendLine("<h1>Batch Results</h1>")
	                        htmlBuilder.AppendLine("<table>")
	
	                        ' Add header row
	                        htmlBuilder.Append("<tr>")
	                        For Each column As DataColumn In results.Columns
	                              htmlBuilder.Append($"<th>{column.ColumnName}</th>")
	                        Next
	                        htmlBuilder.AppendLine("</tr>")
	
	                        ' Add data rows
	                        For Each row As DataRow In results.Rows
	                              htmlBuilder.Append("<tr>")
	                              For Each column As DataColumn In results.Columns
	                                    htmlBuilder.Append($"<td>{row(column).ToString()}</td>")
	                              Next
	                              htmlBuilder.AppendLine("</tr>")
	                        Next
	
	                        htmlBuilder.AppendLine("</table>")
	                        htmlBuilder.AppendLine("</body>")
	                        htmlBuilder.AppendLine("</html>")
	
	                        Return htmlBuilder.ToString()
	                  Catch ex As Exception
	                        Return $"<html><body><p>Error generating HTML attachment: {ex.Message}</p></body></html>"
	                  End Try
	            End Function
				
				#End Region
				
					#Region "Build Line Chart"
				
				Private Shared Function BuildBeforeAfterUOTimelineHtml(dt As DataTable) As String
				
				    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
				        Return "<div style='font-size:12px;color:#999;'>No data</div>"
				    End If
				
				    If Not dt.Columns.Contains("all_dates") OrElse Not dt.Columns.Contains("value") OrElse Not dt.Columns.Contains("uotype") Then
				        Return "<div style='font-size:12px;color:#999;'>Missing columns (all_dates, value, uotype)</div>"
				    End If
				
				    ' Defaults visuales (email-safe)
				    Dim colorUO1 As String = "#FF8C8C"
				    Dim colorUO2 As String = "#6BC8FF"
				    Dim dotInactive As String = "#d0d0d0"
				    Dim lineColor As String = "#e0e0e0"
				
				    ' 1) Sacar últimos 2 CAMBIOS por UO (no últimas 2 filas)
				    Dim uo1Points As List(Of Tuple(Of String, String)) = GetLastTwoValueChanges(dt, "UO1")
				    Dim uo2Points As List(Of Tuple(Of String, String)) = GetLastTwoValueChanges(dt, "UO2")
				
				    ' 2) Mapear a antes/ahora
				    Dim uo1BeforeDate As String = "" : Dim uo1BeforeVal As String = ""
				    Dim uo1NowDate As String = ""    : Dim uo1NowVal As String = ""
				    If uo1Points.Count = 1 Then
				        uo1NowDate = uo1Points(0).Item1 : uo1NowVal = uo1Points(0).Item2
				    ElseIf uo1Points.Count >= 2 Then
				        uo1BeforeDate = uo1Points(0).Item1 : uo1BeforeVal = uo1Points(0).Item2
				        uo1NowDate = uo1Points(1).Item1    : uo1NowVal = uo1Points(1).Item2
				    End If
				
				    Dim uo2BeforeDate As String = "" : Dim uo2BeforeVal As String = ""
				    Dim uo2NowDate As String = ""    : Dim uo2NowVal As String = ""
				    If uo2Points.Count = 1 Then
				        uo2NowDate = uo2Points(0).Item1 : uo2NowVal = uo2Points(0).Item2
				    ElseIf uo2Points.Count >= 2 Then
				        uo2BeforeDate = uo2Points(0).Item1 : uo2BeforeVal = uo2Points(0).Item2
				        uo2NowDate = uo2Points(1).Item1    : uo2NowVal = uo2Points(1).Item2
				    End If
				
				    Dim sb As New StringBuilder()
				
				    sb.Append("<div style='font-family:Arial,sans-serif;'>")
				    sb.Append("<div style='font-size:16px;font-weight:bold;color:#2c3e50;margin-bottom:10px;'>Current vs Next times</div>")
				    sb.Append("<div style='font-size:12px;color:#777;margin-bottom:12px;'>Current and future times.</div>")
				
				    sb.Append("<table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='border-collapse:collapse;'>")
				    sb.Append(RenderRowHtml("UO1", colorUO1, uo1BeforeDate, uo1BeforeVal, uo1NowDate, uo1NowVal, dotInactive, lineColor))
				    sb.Append(RenderRowHtml("UO2", colorUO2, uo2BeforeDate, uo2BeforeVal, uo2NowDate, uo2NowVal, dotInactive, lineColor))
				    sb.Append("</table>")
				
				    sb.Append("</div>")
				
				    Return sb.ToString()
				End Function
				
				
				' Devuelve los 2 últimos CAMBIOS de value para una UO:
				' (all_dates_string, value_string), todo como strings.
				Private Shared Function GetLastTwoValueChanges(dt As DataTable, uoType As String) As List(Of Tuple(Of String, String))
				
				    Dim rows As New List(Of DataRow)()
				    For Each r As DataRow In dt.Rows
				        Dim uo As String = r("uotype").ToString().Trim().ToUpperInvariant()
				        If uo = uoType Then rows.Add(r)
				    Next
				
				    ' Si tu SQL ya viene ordenada por fecha, puedes quitar esto.
				    ' Si all_dates es DateTime, ordena bien; si no, ordena por string.
				    rows.Sort(Function(a As DataRow, b As DataRow)
				                  Dim oa As Object = a("all_dates")
				                  Dim ob As Object = b("all_dates")
				
				                  If TypeOf oa Is DateTime AndAlso TypeOf ob Is DateTime Then
				                      Return DateTime.Compare(CType(oa, DateTime), CType(ob, DateTime))
				                  End If
				
				                  Return String.Compare(oa.ToString(), ob.ToString(), StringComparison.Ordinal)
				              End Function)
				
				    Dim changes As New List(Of Tuple(Of String, String))()
				    Dim lastVal As String = Nothing
				
				    For Each r As DataRow In rows
				        Dim d As String = r("all_dates").ToString().Trim()
				        Dim v As String = r("value").ToString().Trim()
				
				        If lastVal Is Nothing Then
				            changes.Add(Tuple.Create(d, v))
				            lastVal = v
				        Else
				            If v <> lastVal Then
				                changes.Add(Tuple.Create(d, v))
				                lastVal = v
				            End If
				        End If
				    Next
				
				    ' Quedarse con los 2 últimos cambios
				    Dim result As New List(Of Tuple(Of String, String))()
				    If changes.Count = 1 Then
				        result.Add(changes(0))
				    ElseIf changes.Count >= 2 Then
				        result.Add(changes(changes.Count - 2))
				        result.Add(changes(changes.Count - 1))
				    End If
				
				    Return result
				End Function
				
				
				Private Shared Function RenderRowHtml(label As String,
				                               color As String,
				                               beforeDate As String, beforeVal As String,
				                               nowDate As String, nowVal As String,
				                               dotInactive As String,
				                               lineColor As String) As String
				
				    Dim hasBefore As Boolean = (Not String.IsNullOrWhiteSpace(beforeDate)) OrElse (Not String.IsNullOrWhiteSpace(beforeVal))
				    Dim hasNow As Boolean = (Not String.IsNullOrWhiteSpace(nowDate)) OrElse (Not String.IsNullOrWhiteSpace(nowVal))
				
				    Dim beforeDot As String = If(hasBefore, color, dotInactive)
				    Dim nowDot As String = If(hasNow, color, dotInactive)
				
				    If String.IsNullOrWhiteSpace(beforeDate) Then beforeDate = "-"
				    If String.IsNullOrWhiteSpace(beforeVal) Then beforeVal = "-"
				    If String.IsNullOrWhiteSpace(nowDate) Then nowDate = "-"
				    If String.IsNullOrWhiteSpace(nowVal) Then nowVal = "-"
				
				    Dim sb As New StringBuilder()
				
				    sb.Append("<tr>")
				
				    sb.Append("<td valign='top' style='padding:12px 10px 12px 0;width:70px;font-size:14px;color:#2c3e50;font-weight:bold;'>")
				    sb.Append(label)
				    sb.Append("</td>")
				
				    sb.Append("<td valign='top' style='padding:12px 0;'>")
				
				    sb.Append("<div style='white-space:nowrap;'>")
				    sb.Append("<span style='display:inline-block;width:14px;height:14px;border-radius:50%;background:")
				    sb.Append(beforeDot)
				    sb.Append("'></span>")
				
				    sb.Append("<span style='display:inline-block;width:220px;height:4px;background:")
				    sb.Append(lineColor)
				    sb.Append(";vertical-align:middle;margin:0 12px;border-radius:3px;'></span>")
				
				    sb.Append("<span style='display:inline-block;width:14px;height:14px;border-radius:50%;background:")
				    sb.Append(nowDot)
				    sb.Append("'></span>")
				    sb.Append("</div>")
				
				    sb.Append("<table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='border-collapse:collapse;margin-top:8px;'>")
				    sb.Append("<tr>")
				
				    sb.Append("<td valign='top' style='font-size:12px;color:#555;'>")
				    sb.Append("<strong>Current</strong><br/>")
				    sb.Append(beforeDate)
				    sb.Append("<br/><strong>")
				    sb.Append(beforeVal)
				    sb.Append("</strong></td>")
				
				    sb.Append("<td valign='top' style='font-size:12px;color:#555;text-align:right;'>")
				    sb.Append("<strong>Next</strong><br/>")
				    sb.Append(nowDate)
				    sb.Append("<br/><strong>")
				    sb.Append(nowVal)
				    sb.Append("</strong></td>")
				
				    sb.Append("</tr>")
				    sb.Append("</table>")
				
				    sb.Append("</td>")
				    sb.Append("</tr>")
				
				    Return sb.ToString()
				End Function
				#End Region
			
				#End Region
									
				#Region "OLD VERSION"
						  
	'                        Dim htmlBody As New System.Text.StringBuilder
							
	'						htmlBody.AppendLine("<!DOCTYPE html>")
	'						htmlBody.AppendLine("<html>")
	'						htmlBody.AppendLine("<head>")
	'						htmlBody.AppendLine("<meta charset='UTF-8'>")
	'						htmlBody.AppendLine("<style>")
	'						htmlBody.AppendLine("body { font-family: Arial, sans-serif; background-color: #f5f7fa; padding: 20px; }")
	'						htmlBody.AppendLine(".container { max-width: 650px; margin: auto; background: white; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); padding: 30px; }")
	'						htmlBody.AppendLine(".header { text-align: center; font-size: 24px; font-weight: bold; color: #2c3e50; margin-bottom: 25px; }")
	'						htmlBody.AppendLine(".product-card { border: 1px solid #e5e5e5; border-radius: 8px; padding: 15px 20px; margin-bottom: 15px; background-color: #fafafa; }")
	'						htmlBody.AppendLine(".product-title { font-size: 16px; font-weight: bold; color: #34495e; }")
	'						htmlBody.AppendLine(".product-time { font-size: 14px; color: #666; margin-top: 5px; }")
	'						htmlBody.AppendLine(".footer { margin-top: 25px; font-size: 13px; color: #888; text-align: center; border-top: 1px solid #eee; padding-top: 15px; }")
	'						htmlBody.AppendLine("</style>")
	'						htmlBody.AppendLine("</head>")
	'						htmlBody.AppendLine("<body>")
	'						htmlBody.AppendLine("<div class='container'>")
	'						htmlBody.AppendLine($"<div class='header'>{header}</div>")
							
	'						' Generar tarjetas dinámicamente (produc-time, product-time)
	'						Dim lista As New List(Of (Producto As String, TimeUO1 As String, TimeUO2 As String))()
							
	'						Dim bloques = content.Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)
							
	'						For Each bloque As String In bloques
	'						    Dim linea As String = bloque.Trim()
							
	'						    If linea.Contains("-") Then
	'						        Dim partes = linea.Split("-"c)
							
	'						        If partes.Length >= 3 Then
	'						            Dim prod As String = partes(0).Trim()
	'						            Dim timeUO1 As String = partes(1).Trim()
	'						            Dim timeUO2 As String = partes(2).Trim()
							
	'						            lista.Add((prod, timeUO1, timeUO2))
	'						        End If
	'						    End If
	'						Next
							
	'						For Each item In lista
	'						    htmlBody.AppendLine(
	'						        $"<div class='product-card'>" &
	'						        $"<div class='product-title'>{item.Producto}</div>" &
	'						        $"<div class='product-time'>New UO1: {item.TimeUO1}</div>" &
	'						        $"<div class='product-time'>New UO2: {item.TimeUO2}</div>" &
	'						        $"</div>"
	'						    )
	'						Next
													
	'						' htmlBody.AppendLine($"{content}")
							
	'						htmlBody.AppendLine($"<div class='footer'>{footer}</div>")
	'						htmlBody.AppendLine("</div>")
	'						htmlBody.AppendLine("</body>")
	'						htmlBody.AppendLine("</html>")
	'                        Return htmlBody.ToString()
							
							#End Region
							
				#Region "Send Email"
				
	            Public Shared Sub SendEmail(si As SessionInfo, connectionName As String, toList As List(Of String), subject As String, bodyAsHtml As String, isBodyHtml As Boolean, attachments As List(Of String))
	                  Try
	                        ' Create mail message
	                        Dim mailMessage As New System.Net.Mail.MailMessage()
	                        
	                        ' Add recipients
	                        For Each recipient As String In toList
	                              If Not String.IsNullOrEmpty(recipient.Trim()) Then
	                                    mailMessage.To.Add(New System.Net.Mail.MailAddress(recipient.Trim()))
	                              End If
	                        Next
	                        
	                        ' Set subject and body
	                        mailMessage.Subject = subject
	                        mailMessage.Body = bodyAsHtml
	                        mailMessage.IsBodyHtml = isBodyHtml
	                        
	                        ' Add attachments if any
	                        If attachments IsNot Nothing AndAlso attachments.Count > 0 Then
	                              For Each attachment As String In attachments
	                                    If System.IO.File.Exists(attachment) Then
	                                          mailMessage.Attachments.Add(New System.Net.Mail.Attachment(attachment))
	                                    End If
	                              Next
	                        End If
							
							Brapi.Utilities.SendMail(si, connectionName, toList, subject, bodyAsHtml, True, attachments)
							
	                        ' Dispose message
	                        mailMessage.Dispose()
	                        
	                  Catch ex As Exception
	                        Throw New XFException($"Error sending email: {ex.Message}")
	                  End Try
	            End Sub
				
				#End Region

	      End Class
	
	#End Region
	
	#Region "Shared Constants and Strings"
	
	      ''' <summary>
	      ''' Shared constants and strings used across the email functionality
	      ''' </summary>
	      Public Class SharedCS
	
	            ' Integration workspace name
	            Public Const Const_Workspace As String = "Plants"
	
	            ' Email message types
	            Public Const Const_EmailMsgTypeExceptions As String = "Exception"
	            Public Const Const_EmailMsgTypeSuccess As String = "Success"
	
	            ' Dashboard parameter prefixes and suffixes
	            Public Const Const_DashboardParamPrefix As String = "Email_Batch_"
	            Public Const Const_DashboardParamSuffix As String = "_PLTimes_Email"
	
	      End Class
	
	#End Region

Public Class ProductoTiempo
    Public Property Producto As String
    Public Property Tiempo As String
End Class

Public Class MailInfoHelpers
    Public Property sProduct As String
    Public Property sCC As String
    Public Property sGM As String
    Public Property sTime As String
    Public Property sTimeUnit As String
    Public Property sStartDate As String
    Public Property sNewValue As String
    Public Property sComment As String
    Public Property sNewValueUO1 As String
    Public Property sNewValueUO2 As String
    Public Property sEndDate As String
	Public Property dtData As DataTable
			
			
	Public Shared Function GetDataTable(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As DataTable
		Brapi.ErrorLog.LogMessage(si, "Entra por aqui 1")
		Dim dt As DataTable = Nothing
		Dim argsDS As New DashboardDataSetArgs With {
			.FunctionType = DashboardDataSetFunctionType.GetDataSet,
			.DataSetName = "Reporting_UO2_GM"
		}

		argsDS.NameValuePairs = args.NameValuePairs
		argsDS.NameValuePairs.Add("returnTable", "Si")
		argsDS.NameValuePairs("time") = "All"
		Dim queries As New Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.queries.MainClass
		' Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) 
		dt = queries.Main(si, globals, api, argsDS)
		
		Return dt
	End Function
End Class

End Namespace
