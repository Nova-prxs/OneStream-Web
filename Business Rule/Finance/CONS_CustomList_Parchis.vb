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

Namespace OneStream.BusinessRule.Finance.CONS_CustomList_Parchis

#Region "MainClass"

	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
						
					Case Is = FinanceFunctionType.MemberList
						
'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx						
					'REAL - ESTA REGLA NOS DEVUELVE TODAS LAS ENTIDADES QUE TRABAJAN CON LA DN SELECCIONADA
						
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("EntitiesDN") Then
							
																		
							'Dim sTime As String = api.Pov.Time.Name
							Dim sTime As String = args.MemberListArgs.NameValuePairs("PERIOD")
							
							Dim sVariableGroup = args.MemberListArgs.NameValuePairs("GROUP")
							
							Dim sColumnaMes As String = ""
							
							brapi.ErrorLog.LogMessage(si, "VAR00 " & sTime.ToString)
							brapi.ErrorLog.LogMessage(si, "VAR10 " & sVariableGroup.ToString)
							Dim sListEntities As String = Me.getListGroup(si, sVariableGroup)
							
							Brapi.ErrorLog.LogMessage(si, "TIME: "& sTime)
							
							If sTime.Length = 6 Then
								sColumnaMes  = sTime.Substring(4,2) & "Value"
							Else
								sColumnaMes  = sTime.Substring(4,3) & "Value"
							End If
							
							Dim sYear As String = sTime.Substring(0,4)
							
							Dim sVariableDN = args.MemberListArgs.NameValuePairs("DN")						
							
							Dim UN_Members As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(api.Pov.UD1Dim.DimPk, "U1#[" & sVariableDN & "].Base.Where(Name DoesNotContain Elim)", Nothing)
													
							Dim sUNList As String = String.Empty
							For Each UN_Member In UN_Members
								sUNList = sUNList & "'" & UN_Member.Description & "',"					
							Next
							
							sUNList = sUNList.Substring(0,sUNList.Length-1)
								
							Dim sbSQL As New Text.StringBuilder()
						
							sbSQL.Append(" SELECT DISTINCT M.Name ")
							sbSQL.Append(" FROM vDataRecordAll S")
							
							sbSQL.Append(" INNER JOIN Member M")
							sbSQL.Append(" ON S.EntityId = M.MemberId")
							sbSQL.Append(" AND M.DimId = 32")
							
							sbSQL.Append(" INNER JOIN Member U")
							sbSQL.Append(" ON S.UD1Id = U.MemberId")
							sbSQL.Append(" AND U.DimId = 24")														
							
							'34605017 es el id de la cuenta parchis
							sbSQL.Append(" WHERE AccountId = '34605017'")
							'1048602 es el id del S#R
							sbSQL.Append(" AND ScenarioId = '1048602' ")
							sbSQL.Append(" AND " & sColumnaMes & " > 0 ")
							sbSQL.Append(" AND YearId = " & sYear & " ")
							sbSQL.Append(" AND U.Description IN (" & sUNList & ") ")
							sbSQL.Append(" AND M.Name LIKE 'E%' AND M.Name <> 'E999' ") '20231226 para que no salga la 999 en libros
							sbSQL.Append(" AND M.Name IN (" & sListEntities & ")")
							
							Dim sUP As String = String.Empty
							
							Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
									Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)
									
										For Each row As DataRow In dt.Rows
											
											sUP = sUP & "E#" & row(0) & ","
										
										Next
									
									End Using
							End Using
																									
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)														
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, sUP, Nothing)							
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)		
					
							If sUP <> ""
								Return objMemberList
							Else 
								Return sUP
							End If	
							
'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx						
					'REAL - ESTA REGLA NOS DEVUELVE TODAS LAS ENTIDADES QUE TRABAJAN CON EL SECTOR SELECCIONADO
						
					Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("EntitiesSECTOR") Then
						brapi.ErrorLog.LogMessage(si, "J1")
							Dim sVariableGroup = args.MemberListArgs.NameValuePairs("GROUP")											
							Dim sTime As String = api.Pov.Time.Name
							'Dim sTime As String = args.MemberListArgs.NameValuePairs("PERIOD")
							Dim sColumnaMes As String = ""
							Dim sListEntities As String = Me.getListGroup(si, sVariableGroup)
							
							If sTime.Length = 6 Then
								sColumnaMes  = sTime.Substring(4,2) & "Value"
							Else
								sColumnaMes  = sTime.Substring(4,3) & "Value"
							End If
							
							Dim sYear As String = sTime.Substring(0,4)
							
							Dim sVariableSECTOR = args.MemberListArgs.NameValuePairs("SECTOR")						
							
							Dim UN_Members As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(api.Pov.UD1Dim.DimPk, "U1#[" & sVariableSECTOR & "].Base.Where(Name DoesNotContain Elim)", Nothing)
											
							Dim sUNList As String = String.Empty
							For Each UN_Member In UN_Members
								sUNList = sUNList & "'" & UN_Member.Description & "',"					
							Next
							
							
							sUNList = sUNList.Substring(0,sUNList.Length-1)
							
							BRApi.ErrorLog.LogMessage(si, sYear)
							BRApi.ErrorLog.LogMessage(si, sColumnaMes)						
							BRApi.ErrorLog.LogMessage(si, sVariableSECTOR.ToString)						
							BRApi.ErrorLog.LogMessage(si, sUNList)						
																					
							Dim sbSQL As New Text.StringBuilder()
						
							sbSQL.Append(" SELECT DISTINCT M.Name ")
							sbSQL.Append(" FROM vDataRecordAll S")
							
							sbSQL.Append(" INNER JOIN Member M")
							sbSQL.Append(" ON S.EntityId = M.MemberId")
							sbSQL.Append(" AND M.DimId = 32")
							
							sbSQL.Append(" INNER JOIN Member U")
							sbSQL.Append(" ON S.UD1Id = U.MemberId")
							sbSQL.Append(" AND U.DimId = 24")														
							
							'34605017 es el id de la cuenta parchis
							sbSQL.Append(" WHERE AccountId = '34605017'")
							'1048602 es el id del S#R
							sbSQL.Append(" AND ScenarioId = '1048602'")
							sbSQL.Append(" AND " & sColumnaMes & " > 0")
							sbSQL.Append(" AND YearId = " & sYear & " ")
							sbSQL.Append(" AND U.Description IN (" & sUNList & ")")
							sbSQL.Append(" AND M.Name LIKE 'E%'")
							sbSQL.Append(" AND M.Name IN (" & sListEntities & ")")
																																																														
							Dim sUP As String = String.Empty
							
							Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
									Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)
									
										For Each row As DataRow In dt.Rows
											
											sUP = sUP & "E#" & row(0) & ","
										
										Next
									
									End Using
							End Using
																									
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)														
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, sUP, Nothing)							
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)		
																							
							If sUP <> ""
								Return objMemberList
							Else 
								Return sUP
							End If	
							
'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx						
				'REAL - ESTA REGLA NOS DEVUELVE TODAS LAS ENTIDADES QUE TRABAJAN CON LA UN SELECCIONADA
							
						Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("EntitiesUN") Then
							brapi.ErrorLog.LogMessage(si, "J2")
							Dim sVariableUN = args.MemberListArgs.NameValuePairs("UN")
							Dim sVariableGroup = args.MemberListArgs.NameValuePairs("GROUP")
							Dim sTime As String = api.Pov.Time.Name
							Dim sYear As String = sTime.Substring(0,4)
							Dim sColumnaMes As String = ""
						'Paso 1
							Dim sListEntities As String = Me.getListGroup(si, sVariableGroup)
							
							If sTime.Length = 6 Then
								sColumnaMes  = sTime.Substring(4,2) & "Value"
							Else
								sColumnaMes  = sTime.Substring(4,3) & "Value"
							End If
																											
							Dim sbSQL As New Text.StringBuilder()
							
							sbSQL.Append(" SELECT DISTINCT M.Name ")
							sbSQL.Append(" FROM vDataRecordAll S")
							
							sbSQL.Append(" INNER JOIN Member M")
							sbSQL.Append(" ON S.EntityId = M.MemberId")
							sbSQL.Append(" AND M.DimId = 32")
							
							sbSQL.Append(" INNER JOIN Member U")
							sbSQL.Append(" ON S.UD1Id = U.MemberId")
							sbSQL.Append(" AND U.DimId = 24")														
							
							sbSQL.Append(" WHERE AccountId = '34605017'")
							'1048602 es el id del S#R
							sbSQL.Append(" AND ScenarioId = '1048602' ")
							sbSQL.Append(" AND " & sColumnaMes & " > 0 ")
							sbSQL.Append(" AND YearId = " & sYear & " ")
							sbSQL.Append(" AND U.Name =  '" & sVariableUN & "' ")
							sbSQL.Append(" AND M.Name LIKE 'E%' ")
						'Paso 2
							sbSQL.Append(" AND M.Name IN (" & sListEntities & ")")
							
'							If sVariableGroup.Contains("FOCUS") Then
'								sbSQL.Append(" AND ParentId = '4194385' ")
'							Else If sVariableGroup.Contains("LEGAL") Then
'								sbSQL.Append(" AND ParentId = '4194304' ")
'							End If
							
							'BRApi.ErrorLog.LogMessage(si, "Lista Entidades " & sVariableGroup & ": " & sListEntities)
																									
							Dim sUP As String = String.Empty
							
							Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
									Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)
									
										For Each row As DataRow In dt.Rows
											
											sUP = sUP & "E#" & row(0) & ","
										
										Next
									
									End Using
							End Using
							'BRApi.ErrorLog.LogMessage(si, sVariableUN & " - " & sVariableGroup & " = "  & sUP)
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)														
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, sUP, Nothing)							
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)		
								
							If sUP <> ""
								Return objMemberList
							Else 
								Return sUP
							End If	
								
'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx								
					'REAL - ESTA REGLA NOS DEVUELVE TODAS LAS UNs QUE TRABAJAN CON LA ENTITY SELECCIONADA
							
						Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("Entities") Then	
							brapi.ErrorLog.LogMessage(si, "J3")
							Dim sVariableEntity = args.MemberListArgs.NameValuePairs("Entity")
							Dim sTime As String = api.Pov.Time.Name
							Dim sYear As String = sTime.Substring(0,4)
							Dim sColumnaMes As String = ""
							
							If sTime.Length = 6 Then
								sColumnaMes  = sTime.Substring(4,2) & "Value"
							Else
								sColumnaMes  = sTime.Substring(4,3) & "Value"
							End If
							
							Dim sbSQL As New Text.StringBuilder()
							
							sbSQL.Append(" SELECT DISTINCT U.Name ")
							sbSQL.Append(" FROM vDataRecordAll S")
							
							sbSQL.Append(" INNER JOIN Member M")
							sbSQL.Append(" ON S.EntityId = M.MemberId")
							sbSQL.Append(" AND M.DimId = 32")
							
							sbSQL.Append(" INNER JOIN Member U")
							sbSQL.Append(" ON S.UD1Id = U.MemberId")
							sbSQL.Append(" AND U.DimId = 24")														
							
							sbSQL.Append(" WHERE AccountId = '34605017'")
							'1048602 es el id del S#R
							sbSQL.Append(" AND ScenarioId = '1048602' ")
							sbSQL.Append(" AND " & sColumnaMes & " > 0 ")
							sbSQL.Append(" AND YearId = " & sYear & " ")
							sbSQL.Append(" AND M.Name =  '" & sVariableEntity & "' ")
							sbSQL.Append(" AND M.Name LIKE 'E%' ")
			
							Dim sUN As String = String.Empty
							
							Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
									Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)
									
										For Each row As DataRow In dt.Rows
											
											sUN = sUN & "U1#" & row(0) & ","
										
										Next
									
									End Using
							End Using
							
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)														
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, sUN, Nothing)							
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)		
																							
							Return objMemberList
							
						
'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx	
	' OBJETIVO	
'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx						
					'OBJETIVO - ESTA REGLA NOS DEVUELVE TODAS LAS ENTIDADES QUE TRABAJAN CON EL SECTOR SELECCIONADO
						
					Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("EntitiesSECTOR_O") Then
						brapi.ErrorLog.LogMessage(si, "J4")
							Dim sVariableGroup = args.MemberListArgs.NameValuePairs("GROUP")											
							Dim sTime As String = api.Pov.Time.Name
							Dim sColumnaMes As String = ""
							Dim sListEntities As String = Me.getListGroup(si, sVariableGroup)
							
							If sTime.Length = 6 Then
								sColumnaMes  = sTime.Substring(4,2) & "Value"
							Else
								sColumnaMes  = sTime.Substring(4,3) & "Value"
							End If
							
							Dim sYear As String = sTime.Substring(0,4)
							
							Dim sVariableSECTOR = args.MemberListArgs.NameValuePairs("SECTOR")						
							
							Dim UN_Members As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(api.Pov.UD1Dim.DimPk, "U1#[" & sVariableSECTOR & "].Base.Where(Name DoesNotContain Elim)", Nothing)
													
							Dim sUNList As String = String.Empty
							For Each UN_Member In UN_Members
								sUNList = sUNList & "'" & UN_Member.Description & "',"					
							Next
							
							sUNList = sUNList.Substring(0,sUNList.Length-1)
																					
							Dim sbSQL As New Text.StringBuilder()
						
							sbSQL.Append(" SELECT DISTINCT M.Name ")
							sbSQL.Append(" FROM vDataRecordAll S")
							
							sbSQL.Append(" INNER JOIN Member M")
							sbSQL.Append(" ON S.EntityId = M.MemberId")
							sbSQL.Append(" AND M.DimId = 32")
							
							sbSQL.Append(" INNER JOIN Member U")
							sbSQL.Append(" ON S.UD1Id = U.MemberId")
							sbSQL.Append(" AND U.DimId = 24")														
							
							'34605017 es el id de la cuenta parchis
							sbSQL.Append(" WHERE AccountId = '34605017'")
							'1048588 es el id del S#O1
							sbSQL.Append(" AND ScenarioId = '1048588' ")
							sbSQL.Append(" AND " & sColumnaMes & " > 0 ")
							sbSQL.Append(" AND YearId = " & sYear & " ")
							sbSQL.Append(" AND U.Description IN (" & sUNList & ") ")
							sbSQL.Append(" AND M.Name LIKE 'E%' ")
							sbSQL.Append(" AND M.Name IN (" & sListEntities & ")")
																																																														
							Dim sUP As String = String.Empty
							
							Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
									Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)
									
										For Each row As DataRow In dt.Rows
											
											sUP = sUP & "E#" & row(0) & ","
										
										Next
									
									End Using
							End Using
																									
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)														
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, sUP, Nothing)							
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)		
																							
							Return objMemberList
							
'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					'OBJETIVO - ESTA REGLA NOS DEVUELVE TODAS LAS ENTIDADES QUE TRABAJAN CON LA DN SELECCIONADA
						
						Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("EntitiesDN_O") Then
							brapi.ErrorLog.LogMessage(si, "J5")
							Dim sVariableGroup = args.MemberListArgs.NameValuePairs("GROUP")											
							Dim sTime As String = api.Pov.Time.Name
							Dim sColumnaMes As String = ""
							Dim sListEntities As String = Me.getListGroup(si, sVariableGroup)
							
							If sTime.Length = 6 Then
								sColumnaMes  = sTime.Substring(4,2) & "Value"
							Else
								sColumnaMes  = sTime.Substring(4,3) & "Value"
							End If
							
							Dim sYear As String = sTime.Substring(0,4)
							
							Dim sVariableDN = args.MemberListArgs.NameValuePairs("DN")						
							
							Dim UN_Members As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(api.Pov.UD1Dim.DimPk, "U1#[" & sVariableDN & "].Base.Where(Name DoesNotContain Elim)", Nothing)
													
							Dim sUNList As String = String.Empty
							For Each UN_Member In UN_Members
								sUNList = sUNList & "'" & UN_Member.Description & "',"					
							Next
							
							sUNList = sUNList.Substring(0,sUNList.Length-1)
																					
							Dim sbSQL As New Text.StringBuilder()
						
							sbSQL.Append(" SELECT DISTINCT M.Name ")
							sbSQL.Append(" FROM vDataRecordAll S")
							
							sbSQL.Append(" INNER JOIN Member M")
							sbSQL.Append(" ON S.EntityId = M.MemberId")
							sbSQL.Append(" AND M.DimId = 32")
							
							sbSQL.Append(" INNER JOIN Member U")
							sbSQL.Append(" ON S.UD1Id = U.MemberId")
							sbSQL.Append(" AND U.DimId = 24")														
							
							'34605017 es el id de la cuenta parchis
							sbSQL.Append(" WHERE AccountId = '34605017'")
							'1048588 es el id del S#O1
							sbSQL.Append(" AND ScenarioId = '1048588' ")
							sbSQL.Append(" AND " & sColumnaMes & " > 0 ")
							sbSQL.Append(" AND YearId = " & sYear & " ")
							sbSQL.Append(" AND U.Description IN (" & sUNList & ") ")
							sbSQL.Append(" AND M.Name LIKE 'E%' ")
							sbSQL.Append(" AND M.Name IN (" & sListEntities & ")")
																																																														
							Dim sUP As String = String.Empty
							
							Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
									Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)
									
										For Each row As DataRow In dt.Rows
											
											sUP = sUP & "E#" & row(0) & ","
										
										Next
									
									End Using
							End Using
																									
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)														
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, sUP, Nothing)							
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)		
																							
							Return objMemberList

							
'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx						
				'OBJETIVO - ESTA REGLA NOS DEVUELVE TODAS LAS ENTIDADES QUE TRABAJAN CON LA UN SELECCIONADA
							
						Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("EntitiesUN_O") Then
							brapi.ErrorLog.LogMessage(si, "J6")
							Dim sVariableUN = args.MemberListArgs.NameValuePairs("UN")
							Dim sVariableGroup = args.MemberListArgs.NameValuePairs("GROUP")
							Dim sTime As String = api.Pov.Time.Name
							Dim sYear As String = sTime.Substring(0,4)
							Dim sColumnaMes As String = ""
							Dim sListEntities As String = Me.getListGroup(si, sVariableGroup)
							
							If sTime.Length = 6 Then
								sColumnaMes  = sTime.Substring(4,2) & "Value"
							Else
								sColumnaMes  = sTime.Substring(4,3) & "Value"
							End If
																											
							Dim sbSQL As New Text.StringBuilder()
							
							sbSQL.Append(" SELECT DISTINCT M.Name ")
							sbSQL.Append(" FROM vDataRecordAll S")
							
							sbSQL.Append(" INNER JOIN Member M")
							sbSQL.Append(" ON S.EntityId = M.MemberId")
							sbSQL.Append(" AND M.DimId = 32")
							
							sbSQL.Append(" INNER JOIN Member U")
							sbSQL.Append(" ON S.UD1Id = U.MemberId")
							sbSQL.Append(" AND U.DimId = 24")														
							
							sbSQL.Append(" WHERE AccountId = '34605017'")
							'1048588 es el id del S#O1
							sbSQL.Append(" AND ScenarioId = '1048588' ")
							sbSQL.Append(" AND " & sColumnaMes & " > 0 ")
							sbSQL.Append(" AND YearId = " & sYear & " ")
							sbSQL.Append(" AND U.Name =  '" & sVariableUN & "' ")
							sbSQL.Append(" AND M.Name LIKE 'E%' ")
							sbSQL.Append(" AND M.Name IN (" & sListEntities & ")")
																									
							Dim sUP As String = String.Empty
							
							Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
									Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)
									
										For Each row As DataRow In dt.Rows
											
											sUP = sUP & "E#" & row(0) & ","
										
										Next
									
									End Using
							End Using
							
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)														
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, sUP, Nothing)							
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)		
																							
							Return objMemberList
							
'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx								
					'OBJETIVO - ESTA REGLA NOS DEVUELVE TODAS LAS UNs QUE TRABAJAN CON LA ENTITY SELECCIONADA
							
						Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("Entities_O") Then	
							brapi.ErrorLog.LogMessage(si, "J7")
							Dim sVariableEntity = args.MemberListArgs.NameValuePairs("Entity")
							Dim sVariableGroup = args.MemberListArgs.NameValuePairs("GROUP")
							Dim sTime As String = api.Pov.Time.Name
							Dim sYear As String = sTime.Substring(0,4)
							Dim sColumnaMes As String = ""
							Dim sListEntities As String = Me.getListGroup(si, sVariableGroup)
							
							If sTime.Length = 6 Then
								sColumnaMes  = sTime.Substring(4,2) & "Value"
							Else
								sColumnaMes  = sTime.Substring(4,3) & "Value"
							End If
							
							Dim sbSQL As New Text.StringBuilder()
							
							sbSQL.Append(" SELECT DISTINCT U.Name ")
							sbSQL.Append(" FROM vDataRecordAll S")
							
							sbSQL.Append(" INNER JOIN Member M")
							sbSQL.Append(" ON S.EntityId = M.MemberId")
							sbSQL.Append(" AND M.DimId = 32")
							
							sbSQL.Append(" INNER JOIN Member U")
							sbSQL.Append(" ON S.UD1Id = U.MemberId")
							sbSQL.Append(" AND U.DimId = 24")														
							
							sbSQL.Append(" WHERE AccountId = '34605017'")
							'1048588 es el id del S#O1
							sbSQL.Append(" AND ScenarioId = '1048588' ")
							sbSQL.Append(" AND " & sColumnaMes & " > 0 ")
							sbSQL.Append(" AND YearId = " & sYear & " ")
							sbSQL.Append(" AND M.Name =  '" & sVariableEntity & "' ")
							sbSQL.Append(" AND M.Name LIKE 'E%' ")
							sbSQL.Append(" AND M.Name IN (" & sListEntities & ")")
							
							Dim sUN As String = String.Empty
							
							Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
									Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)
									
										For Each row As DataRow In dt.Rows
											
											sUN = sUN & "U1#" & row(0) & ","
										
										Next
									
									End Using
							End Using
							
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)														
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, sUN, Nothing)							
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)		
																							
							Return objMemberList
							
						End If

												
																		
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	
#End Region

#Region "Helpers"

 Public Function getListGroup(ByVal si As SessionInfo, EntityGroup As String)
	Dim entities As String = String.Empty
	Dim Group_Members As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "Entities"), "E#[" & EntityGroup & "].Base", True)				
	Dim sEntityList As String = String.Empty
		
	For Each Entity_Member In Group_Members
		entities = BRApi.Finance.Members.GetMemberName(si, 0, Entity_Member.Member.MemberId)
		sEntityList = sEntityList & "'" & entities & "',"					
	Next
		
	sEntityList = sEntityList.Substring(0,sEntityList.Length-1)

	Return sEntityList
 End Function	 


#End Region

	End Class
End Namespace