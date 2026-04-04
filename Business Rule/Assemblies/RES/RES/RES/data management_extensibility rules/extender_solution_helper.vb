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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_solution_helper
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						'Get the function name
						Dim functionName As String = args.NameValuePairs("p_function")
						
						Select Case functionName
							
							#Region "Populate Aux Tables"
							
							#Region "Populate Entity Currency Table"
							
							Case "PopulateEntityCurrencyTable"
								'Get all the base entitites
								Dim baseEntities As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(
									si,
									"Entities",
									"E#Root.Base",
									True,
									Nothing,
									Nothing
								)
								
								'Populate data table for each entity and its currency
								Dim dt As New DataTable()
								dt.Columns.Add("entity")
								dt.Columns.Add("currency")
								For Each baseEntity In BaseEntities
									Dim row As DataRow = dt.NewRow()
									row("entity") = baseEntity.Member.Name
									row("currency") = BRApi.Finance.Entity.GetLocalCurrency(si, baseEntity.Member.MemberId).Name
									dt.Rows.Add(row)
								Next
								
								'Populate custom table
								UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_RES_AUX_entity_currency", dt, "replace")
								
							#End Region
							
							#Region "Populate Scenario Fx Rate Type Table"
								
							Case "PopulateScenarioFxRateTypeTable"
								'Get all the base scenarios
								Dim scenarioDimensions As String() = BRApi.Finance.Dim.GetDims(si, dimTypeId.Scenario).Select(Function(s) s.Name).ToArray()
								Dim baseScenarios As New List(Of MemberInfo)
								For Each scenarioDimension In scenarioDimensions
									Dim scenarioDimensionScenarios As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(
										si,
										scenarioDimension,
										"S#Root.Base",
										True,
										Nothing,
										Nothing
									)
									For Each scenarioDimensionScenario In scenarioDimensionScenarios
										If Not baseScenarios.Contains(scenarioDimensionScenario) Then baseScenarios.Add(scenarioDimensionScenario)
									Next
								Next
								
								'Populate data table for each scenario
								Dim dt As New DataTable()
								dt.Columns.Add("scenario")
								dt.Columns.Add("fxratetype_rev")
								dt.Columns.Add("fxratetype_bal")
								For Each baseScenario In BaseScenarios
									Dim row As DataRow = dt.NewRow()
									row("scenario") = baseScenario.Member.Name
									row("fxratetype_rev") = BRApi.Finance.Scenario.GetFxRateTypeForRevenueExpense(si, baseScenario.Member.MemberId)
									row("fxratetype_bal") = BRApi.Finance.Scenario.GetFxRateTypeForAssetLiability(si, baseScenario.Member.MemberId)
									dt.Rows.Add(row)
								Next
								
								'Populate custom table
								UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_RES_AUX_scenario_fxratetype", dt, "replace")
								
							#End Region
							
							#Region "Populate Department Table"
							
							Case "PopulateDepartmentTable"
							    ' Create DataTable and define columns
							    Dim dt As New DataTable()
							    dt.Columns.Add("cc")
							    dt.Columns.Add("cc_description")
							    dt.Columns.Add("subdepartment")
							    dt.Columns.Add("subdepartment_description")
							    dt.Columns.Add("department_description")
							
							    ' Get all level 0 members under TOT
							    Dim baseNodes As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(
							        si, "SERV_Departments", "U3#TOT.Base", True
							    )
							
							    For Each node In baseNodes
							        Dim nodeName As String = node.Member.Name
							        Dim nodeDesc As String = If(String.IsNullOrWhiteSpace(node.Member.Description), nodeName, node.Member.Description)
									
									Dim safeNodeName = $"[{nodeName}]"
									
							        ' Check if the immediate parent is TOT (e.g., "None")
							        Dim immediateParents = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "SERV_Departments", $"U3#{safeNodeName}.Parents", True)
							        If immediateParents.Count = 1 AndAlso immediateParents(0).Member.Name = "TOT" Then
							            Dim rowNone As DataRow = dt.NewRow()
							            rowNone("cc") = nodeName
							            rowNone("cc_description") = nodeName
							            rowNone("subdepartment") = nodeName
							            rowNone("subdepartment_description") = nodeName
							            rowNone("department_description") = nodeName
							            dt.Rows.Add(rowNone)
							            Continue For
							        End If
							
							        ' Build the upward path from base to DEPT
							        Dim currentMember As MemberInfo = node
							        Dim path As New List(Of MemberInfo)
							        path.Add(currentMember)
							
							        ' Traverse hierarchy until reaching DEPT
							        Dim foundDept As Boolean = False
							        Do
							            Dim currentSafeName = $"[{currentMember.Member.Name}]"
										Dim parentList = BRApi.Finance.Metadata.GetMembersUsingFilter(
							                si, "SERV_Departments", $"U3#{currentSafeName}.Parents", True
							            )
							
							            If parentList.Count = 0 Then Exit Do
							
							            currentMember = parentList(0)
							            path.Add(currentMember)
							
							            If currentMember.Member.Name = "DEPT" Then
							                foundDept = True
							                Exit Do
							            End If
							        Loop
							
							        If Not foundDept OrElse path.Count < 3 Then Continue For
							
							        ' Prepare output fields
							        Dim ccName As String = ""
							        Dim ccDesc As String = ""
							        Dim subDeptName As String = ""
							        Dim subDeptDesc As String = ""
							        Dim deptDesc As String = ""
							
							        If path.Count >= 4 Then
							            ' Full hierarchy: CC > SubDept > DeptNode > DEPT
							            ccName = path(0).Member.Name
							            ccDesc = If(String.IsNullOrWhiteSpace(path(0).Member.Description), ccName, path(0).Member.Description)
							
							            subDeptName = path(1).Member.Name
							            subDeptDesc = If(String.IsNullOrWhiteSpace(path(1).Member.Description), subDeptName, path(1).Member.Description)
							
							            Dim deptNodeName = path(path.Count - 2).Member.Name
							            Dim deptParentDesc = If(String.IsNullOrWhiteSpace(path(2).Member.Description), path(2).Member.Name, path(2).Member.Description)
							            deptDesc = deptNodeName & " - " & deptParentDesc
							
							        ElseIf path.Count = 3 Then
							            ' Flat hierarchy like COGS: COGS > DCOGS > DEPT
							            ccName = path(0).Member.Name
							            ccDesc = If(String.IsNullOrWhiteSpace(path(0).Member.Description), ccName, path(0).Member.Description)
							
							            subDeptName = path(0).Member.Name
							            subDeptDesc = ccDesc
							
							            Dim deptNodeName = path(1).Member.Name
							            deptDesc = deptNodeName & " - " & subDeptDesc
							
							        Else
							            Continue For
							        End If
							
							        ' Add row to the DataTable
							        Dim row As DataRow = dt.NewRow()
							        row("cc") = ccName
							        row("cc_description") = ccDesc
							        row("subdepartment") = subDeptName
							        row("subdepartment_description") = subDeptDesc
							        row("department_description") = deptDesc
							        dt.Rows.Add(row)
							
							    Next
							
							    ' Populate the custom table
							    UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_RES_AUX_department", dt, "replace")

							#End Region

							#Region "Populate Technology Table"

							Case "PopulateTechnologyTable"
								' Create DataTable
								Dim dt As New DataTable()
								dt.Columns.Add("id")
								dt.Columns.Add("description")

								' Get all base members under SRVC in SERV_Technologies (UD1)
								Dim baseTechnologies As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(
									si, "SERV_Technologies", "U1#SRVC.Base", True
								)

								For Each tech In baseTechnologies
									Dim row As DataRow = dt.NewRow()
									row("id") = tech.Member.Name
									row("description") = If(String.IsNullOrWhiteSpace(tech.Member.Description), tech.Member.Name, tech.Member.Description)
									dt.Rows.Add(row)
								Next

								' Populate custom table
								UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_RES_AUX_technology", dt, "replace")

							#End Region

							#Region "Populate Other Tables"
								
							Case "PopulateOtherTables"
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteSql(
										dbConn,
										"								
										-- DIM Account table
										TRUNCATE TABLE XFC_RES_DIM_Account;
										
										WITH HierarchyLevels AS (
										    SELECT m.MemberId, 0 AS Level
										    FROM Member m
										    WHERE m.DimId = 38
										      AND m.MemberId NOT IN (SELECT ParentId FROM Relationship WHERE DimId = 38)
										
										    UNION ALL
										
										    SELECT r.ParentId, h.Level + 1
										    FROM Relationship r
										    INNER JOIN HierarchyLevels h ON r.ChildId = h.MemberId
										    WHERE r.DimId = 38
										),
										PLDescendants AS (
										    SELECT m.MemberId
										    FROM Member m
										    WHERE m.DimId = 38 AND m.Name = 'PL'
										    
										    UNION ALL
										
										    SELECT r.ChildId
										    FROM Relationship r
										    INNER JOIN PLDescendants d ON r.ParentId = d.MemberId
										    WHERE r.DimId = 38
										)
										INSERT INTO XFC_RES_DIM_Account (
										    MemberName, MemberDescription, MemberParent, ParentDescription, Level, LastUpdated
										)
										SELECT 
										    m.Name AS MemberName,
										    CASE 
										        WHEN m.Description IS NULL OR LTRIM(RTRIM(m.Description)) = '' THEN m.Name
										        ELSE m.Description
										    END AS MemberDescription,
										    ISNULL(mp.Name, 'Root') AS MemberParent,
										    CASE 
										        WHEN mp.Description IS NULL OR LTRIM(RTRIM(mp.Description)) = '' THEN ISNULL(mp.Name, 'Root Node')
										        ELSE mp.Description
										    END AS ParentDescription,
										    MAX(h.Level) AS Level,
										    GETDATE() AS LastUpdated
										FROM Member m
										LEFT JOIN Relationship r ON r.ChildId = m.MemberId AND r.DimId = 38
										LEFT JOIN Member mp ON r.ParentId = mp.MemberId
										LEFT JOIN HierarchyLevels h ON m.MemberId = h.MemberId
										WHERE m.DimId = 38
										  AND m.MemberId IN (SELECT MemberId FROM PLDescendants)
										  AND (
										      m.Description NOT LIKE '%\%%' ESCAPE '\'
										      OR m.Description IS NULL
										  )
										GROUP BY m.Name, m.Description, mp.Name, mp.Description;
										",
										False
									)
									
									' Build DIM Entity table from children of RES_SER
									Dim baseEntities As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(
										si,
										"Entities",
										"E#RES_SER.Base",
										True,
										Nothing,
										Nothing
									)
									
									' Validate that entities were found
									If baseEntities Is Nothing OrElse baseEntities.Count = 0 Then
										Throw New XFException(si, New Exception("No se encontraron entidades bajo RES_SER."))
									End If
							
									' Build a map: entity name -> currency name
									Dim currencyMap As New Dictionary(Of String, String)
									For Each baseEntity In baseEntities
										Dim name = baseEntity.Member.Name
										Dim currency = BRApi.Finance.Entity.GetLocalCurrency(si, baseEntity.Member.MemberId).Name
										currencyMap(name) = currency
									Next
							
									' Construct a CASE WHEN statement for currency based on entity name
									Dim currencyCase As String = "CASE m.Name" & vbCrLf
									For Each kvp In currencyMap
										currencyCase &= $"    WHEN '{kvp.Key.Replace("'", "''")}' THEN '{kvp.Value}'" & vbCrLf
									Next
									currencyCase &= "    ELSE NULL END"
							
									' Create an IN clause with all entity names
									Dim entityNames As String = String.Join(",", currencyMap.Keys.Select(Function(e) $"'{e.Replace("'", "''")}'"))
							
									' Final SQL to truncate and populate the DIM Entity table
									Dim dimEntitySql As String = $"
										TRUNCATE TABLE XFC_RES_DIM_Entity;
							
										INSERT INTO XFC_RES_DIM_Entity (
											entity, entityDescription, currency, country, countryDescription, region, regionDescription, LastUpdated
										)
										SELECT 
											m.Name,
											m.Description,
											{currencyCase} AS currency,
											mp.TextValue AS country,
											country.Description AS countryDescription,
											region.Name AS region,
											region.Description AS regionDescription,
											GETDATE() AS LastUpdated
										FROM 
											Member m
										LEFT JOIN 
											MemberProperty mp ON m.MemberID = mp.MemberID AND mp.PropertyID = 900300
										LEFT JOIN 
											Member country ON country.Name = mp.TextValue AND country.DimID = 43
										LEFT JOIN 
											Relationship r ON r.ChildId = country.MemberID
										LEFT JOIN 
											Member region ON region.MemberID = r.ParentId
										WHERE 
											m.DimID = 32
											AND m.Name IN ({entityNames});
									"
							
									BRApi.Database.ExecuteSql(dbConn, dimEntitySql, False)								
									
								End Using
								
							#End Region
							
							#End Region
								
						End Select
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
