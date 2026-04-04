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

Namespace OneStream.BusinessRule.Extender.UTI_AddNewMembers
	Public Class MainClass
		
		'Reference business rule to get functions and variables
		Dim SharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				'Declare not real member names
				Dim notMemberNames As New List(Of String) From {
					"None",
					"(Bypass)",
					"~"
				}
				
				'TEMPORAL: Entities that must not update comparability
				Dim notUpdatingEntities = SharedFunctionsBR.GetNotUpdatingEntities(si)
				
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						'We get the necessary parameters and initialize the dimension type id
						Dim ParamDimensionType As String = args.NameValuePairs("DimensionType")
						Dim dimensionTypeId As Integer
						
						#Region "Entity"
						
						#Region "Update Entity Dimension"
						
						'Control dimension type
						If ParamDimensionType = "Entity" Then
							
							'Get Entity id
							dimensionTypeId = DimType.Entity.Id
							
							'Build a loop for each Entity Hierarchy type
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
								Dim selectQuery As String = "SELECT DISTINCT hierarchy
															 FROM dbo.XFC_CEBESHierarchy"
								
								Dim hierarchyDt As DataTable = BRApi.Database.ExecuteSql(dbcon,selectQuery,False)
							
								If hierarchyDt IsNot Nothing AndAlso hierarchyDt.Rows.Count > 0 Then
										
									Dim hierarchyName As String
									Dim cebeDict As New Dictionary(Of String, MemberInfo)
									
									'Loop through all the hierarchy types to insert new members
									For Each row As DataRow In hierarchyDt.Rows
									
										If row("hierarchy").ToString <> "" Then
											
											hierarchyName = row("hierarchy").ToString
											
											'Update the members dictionary to prevent errors
											For Each kvp As KeyValuePair(Of String, MemberInfo) In Me.CreateMemberLookupUsingFilter(si, "Entities", "E#Root.DescendantsInclusive")
											
												cebeDict(kvp.Key) =kvp.Value
											
											Next
											
											'Build a query to get each cebe info with it's parent
											selectQuery = $"SELECT
																ch1.id,
																ch1.cebe AS cebe,
																CASE
																	WHEN c1.description IS NOT NULL THEN c1.description
																	ELSE ch1.description
																END AS description,
																ch1.father_id,
																ch2.cebe AS father_cebe,
																CASE
												                	WHEN c2.description IS NOT NULL THEN c2.description
												                	ELSE ch2.description
												                END AS father_description
															FROM
																dbo.XFC_CEBESHierarchy AS ch1
															LEFT JOIN
																dbo.XFC_CEBES AS c1 ON ch1.cebe = c1.cebe
															LEFT JOIN
																dbo.XFC_CEBESHierarchy AS ch2 ON ch1.father_id = ch2.id
																AND ch2.hierarchy = '{hierarchyName}'
															LEFT JOIN
																dbo.XFC_CEBES AS c2 ON ch2.cebe = c2.cebe
															WHERE
																ch1.hierarchy = '{hierarchyName}'
															ORDER BY
																ch1.id ASC;"
											
											Dim CEBEsDt As DataTable = BRApi.Database.ExecuteSql(dbcon,selectQuery,False)
											
											If CEBEsDt IsNot Nothing AndAlso CEBEsDt.Rows.Count > 0 Then
												
												Dim cebeName As String
												Dim cebeDescription As String
												Dim cebeParentName As String
												
												'Loop through all the hierarchy types to insert new members
												For Each cebeRow As DataRow In CEBEsDt.Rows
													
													'Get info to create members
													cebeName = cebeRow("cebe").ToString.Replace("-","_").Replace("/","_")
													cebeDescription = cebeRow("description").ToString
													cebeParentName = IIf(cebeRow("father_id") = "0", "Root", cebeRow("father_cebe").ToString).Replace("-","_").Replace("/","_")
													
													'If member is not already created, create it
													If Not cebeDict.ContainsKey(cebeName) Then
														'Final check in case it's an orfan
														Dim mbrInfo As Member = BRApi.Finance.Members.GetMember(si, dimTypeId.Entity, cebeName)
														If mbrInfo IsNot Nothing AndAlso mbrInfo.MemberId <> -1 Then Continue For
														
														Me.AddEntityMember(si, dimensionTypeId, "Entities", cebeName, cebeDescription, cebeParentName)
														
													End If
													
												Next
												
												'Loop through all the members to remove relationships
												For Each cebeRow As DataRow In CEBEsDt.Rows
													
													'Get member info
													cebeName = cebeRow("cebe").ToString.Replace("-","_").Replace("/","_")
													
													'Update relationship
													Me.RemoveMemberRelationships(si, dimensionTypeId, "Entities", cebeName)
													
												Next
												
												'Loop through all the hierarchy types to update member relationships
												For Each cebeRow As DataRow In CEBEsDt.Rows
													
													'Get info to update members
													cebeName = cebeRow("cebe").ToString.Replace("-","_").Replace("/","_")
													cebeDescription = cebeRow("description").ToString
													cebeParentName = IIf(cebeRow("father_id") = "0", "Root", cebeRow("father_cebe").ToString).Replace("-","_").Replace("/","_")
													
													'Update relationship
													Me.AddMemberRelationship(si, dimensionTypeId, "Entities", cebeName, cebeDescription, cebeParentName)
													
												Next
												
											End If
											
										End If
										
									Next
									
								End If
							
							End Using
							
						#End Region
						
						#Region "Update Entity Text"
						
						'Control dimension type
						Else If ParamDimensionType = "EntityText" Then
							'Get base entities
							Dim cebeDict As Dictionary(Of String, MemberInfo) = Me.CreateMemberLookupUsingFilter(si, "Entities", "E#Root.Base")
			
							'Get closing dates from aux table and pass them to a dictionary
							Dim closingDatesDict As New Dictionary(Of String, String)
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim selectQuery As String = $"
									SELECT
										cebe,
										YEAR(close_date) as close_year
									FROM XFC_CEBESClosings
									WHERE YEAR(close_date) <> 2100
								"
								Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
								If dt.Rows.Count > 0 Then
									For Each row As DataRow In dt.Rows
										closingDatesDict(row("cebe")) = row("close_year").ToString
									Next
								End If
							End Using
							'Loop through all the cebes to update the text properties
							For Each cebe As KeyValuePair(Of String, MemberInfo) In cebeDict
								If notMemberNames.Contains(cebe.Key) Then Continue For
								'Declare closing year for auxiliary table
								Dim auxClosingYear As String = If(closingDatesDict.ContainsKey(cebe.Key), closingDatesDict(cebe.Key), "None")
								Me.UpdateEntityText(si, cebe.Key, auxClosingYear, notUpdatingEntities)
							Next
							
						#End Region
							
						#End Region
						
						#Region "Account"
						
						Else If ParamDimensionType = "Account" Then
							
							Dim accountsNotInBase As String = 	"'62900015', '62300055', '62300080', '62300090', '62300020', " &
																"'62300023', '62300024', '62300019', '62300022', '62500001', " &
																"'62500400', '62100001', '62110000', '67800202', '67800203', " &
																"'67800205', '67800208', '67800207', '67800206', '67800200', " &
																"'67800201', '67800204', '67800299', '90000171'"
							
							'List of accounts to match the hierarchy level
'							accountsNotInBase += "'62100010', '62900101', " &
'												"'72150000', '75200000', '75900101', '62100800', '62150000', " &
'												"'62100008', '62100009', '62302000', '62300021', '62900016', " &
'												"'62900026', '62300001', '62300002', '62300003', '62300005', " &
'												"'62300006', '62300007', '62300010', '62300012', '62359000', " &
'												"'62300011', '62800004', '62900011', '62900020', '62100006', " &
'												"'62300008', '62300800', '62300801', '62300802', '62300803', " &
'												"'62900024', '62902400', '62902401', '62400000', '62400001', " &
'												"'62900025', '62110005', '62110004', '62900000', '62900006', " &
'												"'62900012', '62900030', '62900031', '62900032', '62900033', " &
'												"'62900034', '62110001', '62900014', '62900100', '70050003', " &
'												"'75900014', '75900100', '75910000', '75940000', '75100000', " &
'												"'62800000', '62800002', '62820000', '62800001', '62810000', " &
'												"'62900405', '62700021', '62730000', '62780000', '70030000', " &
'												"'70030100', '70030200', '70030300', '75930100', '61100004', " &
'												"'62700000', '62700001', '62700002', '62700004', '62700005', " &
'												"'62700006', '62700007', '62700008', '62700009', '62700010', " &
'												"'62700012', '62700013', '62700014', '62700015', '62700016', " &
'												"'62700017', '62700018', '62700019', '62700020', '62700022', " &
'												"'75300099', '80000000', '90000000', '62200001', '62200002', " &
'												"'62200003', '62200004', '62200005', '62200006', '62200011', " &
'												"'62200012', '62200013', '62200014', '62200016', '62201100', " &
'												"'62201400', '62201401', '62201402', '62201403', '62201404', " &
'												"'62201405', '62201406', '62201407', '62201408', '62201409', " &
'												"'62201410', '62201411', '62900001', '70050001', '70050002', " &
'												"'70050004', '70050005', '70050006', '70050007', '70050008', " &
'												"'70050020', '70050030', '62900013', '62900017', '62300070', " &
'												"'74600000', '75200010', '61000006', '60200400', '60200000', " &
'												"'60000017', '60000006', '62790000', '62900010', '62900021', " &
'												"'62900019', '62900004', '62900023', '62900040', '65000000', " &
'												"'65001000', '69400000', '65900001', '66950000', '70000006', " &
'												"'70520000', '73000000', '73100000', '73100001', '74000002', " &
'												"'79401000', '79400000', '79000000', '75930000', '75900013', " &
'												"'75900001', '75900000', '75300020', '75300004', '75200011', " &
'												"'75300002', '75200009', '75119000', '75000099', '75000040', " &
'												"'75000013', '75000012', '75000011', '75000006', '62800066', " &
'												"'75000001', '75000000', '74000001', '74000000', '73200002', " &
'												"'73200001', '74000003', '60000019', '61029800', '60900001', " &
'												"'60901300', '60901400', '60908900', '60929900', '60909900', " &
'												"'60900010', '70919000', '60089500', '60100131', '60200002', " &
'												"'60200003', '61019500', '61039500', '75300007', '75916000', " &
'												"'75300008', '75300100', '75120000'"
																
							Dim accountsInBase As String = "''"'"'AL-EX-BVI'"
							
							'Get Account id
							dimensionTypeId = DimType.Account.Id
							
							'Build a loop for each Account Hierarchy type
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
								Dim selectQuery As String = "SELECT DISTINCT hierarchy
															 FROM dbo.XFC_AccountHierarchy"
								
								Dim hierarchyDt As DataTable = BRApi.Database.ExecuteSql(dbcon,selectQuery,False)
							
								If hierarchyDt IsNot Nothing AndAlso hierarchyDt.Rows.Count > 0 Then
									
									'Create node level members
									Dim hierarchyName As String
									Dim accountDict As Dictionary(Of String, MemberInfo) = Me.CreateMemberLookupUsingFilter(si, "Accounts", "A#Root.DescendantsInclusive")
'									accountDict = accountDict.Concat(Me.CreateMemberLookupUsingFilter(si, "Accounts", "A#60000006, A#60000017, A#60000019, A#60000900, A#60004000, A#60089500, A#60100001, A#60100002, A#60100003, A#60100004, A#60100005, A#60100006, A#60100007, A#60100008, A#60100009, A#60100010, A#60100013, A#60100014, A#60100015, A#60100016, A#60100017, A#60100018, A#60100019, A#60100020, A#60100021, A#60100022, A#60100023, A#60100024, A#60100025, A#60100080, A#60100099, A#60100100, A#60100110, A#60100120, A#60100130, A#60100131, A#60100132, A#60100134, A#60100135, A#60100140, A#60100180, A#60100197, A#60100200, A#60100210, A#60100220, A#60100300, A#60100339, A#60100400, A#60101000, A#60101001, A#60101002, A#60200000, A#60200001, A#60200002, A#60200003, A#60200004, A#60200400, A#60201001, A#60600000, A#60700000, A#60900001, A#60900010, A#60901300, A#60901400, A#60908900, A#60909900, A#60929900, A#61000000, A#61000001, A#61000006, A#61014000, A#61019500, A#61021300, A#61021400, A#61023900, A#61024000, A#61028000, A#61028900, A#61029800, A#61039500, A#61100001, A#61100002, A#61100003, A#61100004, A#61100005, A#61200000, A#62100001, A#62100005, A#62100006, A#62100008, A#62100009, A#62100010, A#62100800, A#62110000, A#62110001, A#62110002, A#62110003, A#62110004, A#62110005, A#62110006, A#62110013, A#62150000, A#62200001, A#62200002, A#62200003, A#62200004, A#62200005, A#62200006, A#62200011, A#62200012, A#62200013, A#62200014, A#62200016, A#62201100, A#62201400, A#62201401, A#62201402, A#62201403, A#62201404, A#62201405, A#62201406, A#62201407, A#62201408, A#62201409, A#62201410, A#62201411, A#62300001, A#62300002, A#62300003, A#62300005, A#62300006, A#62300007, A#62300008, A#62300009, A#62300010, A#62300011, A#62300012, A#62300019, A#62300020, A#62300021, A#62300022, A#62300023, A#62300024, A#62300055, A#62300070, A#62300080, A#62300090, A#62300800, A#62300801, A#62300802, A#62300803, A#62300900, A#62302000, A#62330000, A#62359000, A#62400000, A#62400001, A#62500001, A#62500400, A#62600001, A#62700000, A#62700001, A#62700002, A#62700003, A#62700004, A#62700005, A#62700006, A#62700007, A#62700008, A#62700009, A#62700010, A#62700012, A#62700013, A#62700014, A#62700015, A#62700016, A#62700017, A#62700018, A#62700019, A#62700020, A#62700021, A#62700022, A#62700032, A#62700040, A#62730000, A#62780000, A#62790000, A#62800000, A#62800001, A#62800002, A#62800003, A#62800004, A#62800005, A#62800006, A#62800007, A#62800008, A#62800009, A#62800010, A#62800011, A#62800012, A#62800013, A#62800066, A#62810000, A#62820000, A#62900000, A#62900001, A#62900002, A#62900003, A#62900004, A#62900005, A#62900006, A#62900007, A#62900008, A#62900009, A#62900010, A#62900011, A#62900012, A#62900013, A#62900014, A#62900015, A#62900016, A#62900017, A#62900018, A#62900019, A#62900020, A#62900021, A#62900022, A#62900023, A#62900024, A#62900025, A#62900026, A#62900030, A#62900031, A#62900032, A#62900033, A#62900034, A#62900040, A#62900100, A#62900101, A#62900400, A#62900402, A#62900403, A#62900405, A#62900500, A#62902400, A#62902401, A#62950500, A#62980000, A#62988000, A#63000000, A#63000010, A#63000020, A#63000030, A#63000040, A#63040000, A#63060000, A#63100001, A#63100002, A#63100003, A#63100004, A#63100010, A#63100020, A#63100030, A#63100040, A#63100050, A#63100060, A#63100070, A#63100071, A#63100072, A#63100073, A#63100074, A#63100080, A#63160000, A#63300000, A#63800000, A#64000000, A#64000001, A#64000002, A#64000003, A#64000004, A#64000005, A#64000006, A#64000007, A#64000008, A#64000009, A#64000010, A#64000011, A#64000012, A#64000013, A#64000014, A#64000015, A#64000016, A#64000017, A#64000018, A#64000019, A#64000020, A#64000021, A#64000022, A#64000023, A#64000025, A#64000027, A#64000028, A#64000030, A#64000031, A#64000032, A#64000033, A#64000034, A#64000035, A#64000036, A#64000037, A#64000038, A#64000039, A#64000040, A#64000041, A#64000042, A#64000043, A#64000044, A#64000045, A#64000046, A#64000047, A#64000048, A#64000049, A#64000050, A#64000051, A#64000052, A#64000053, A#64000054, A#64000055, A#64000056, A#64000057, A#64000058, A#64000059, A#64000060, A#64000092, A#64000093, A#64000094, A#64000100, A#64000101, A#64000102, A#64000103, A#64000104, A#64000105, A#64000106, A#64000107, A#64000108, A#64000132, A#64000232, A#64000332, A#64000432, A#64000532, A#64001100, A#64001101, A#64001110, A#64001120, A#64001210, A#64001220, A#64001230, A#64001240, A#64001250, A#64001260, A#64001270, A#64001280, A#64001300, A#64001400, A#64001500, A#64001600, A#64001700, A#64002100, A#64002101, A#64002110, A#64002111, A#64002120, A#64002210, A#64002220, A#64002230, A#64002240, A#64002250, A#64002260, A#64002280, A#64002300, A#64002400, A#64002500, A#64002600, A#64002700, A#64003000, A#64003100, A#64003101, A#64003110, A#64003120, A#64003210, A#64003220, A#64003230, A#64003240, A#64003250, A#64003260, A#64003280, A#64003300, A#64003400, A#64003500, A#64003600, A#64003700, A#64004100, A#64004101, A#64004110, A#64004120, A#64004210, A#64004220, A#64004230, A#64004240, A#64004250, A#64004260, A#64004280, A#64004300, A#64004400, A#64004500, A#64004600, A#64004700, A#64005000, A#64005100, A#64005101, A#64005110, A#64005120, A#64005210, A#64005220, A#64005230, A#64005240, A#64005250, A#64005260, A#64005280, A#64005300, A#64005400, A#64005500, A#64005600, A#64005700, A#64006100, A#64006101, A#64006110, A#64006120, A#64006210, A#64006220, A#64006230, A#64006240, A#64006250, A#64006260, A#64006280, A#64006300, A#64006400, A#64006500, A#64006600, A#64006700, A#64007100, A#64007110, A#64007120, A#64007210, A#64007220, A#64007230, A#64007240, A#64007250, A#64007260, A#64007280, A#64007300, A#64007400, A#64007500, A#64007600, A#64008100, A#64008110, A#64008120, A#64008210, A#64008220, A#64008230, A#64008240, A#64008250, A#64008260, A#64008280, A#64008300, A#64008400, A#64008500, A#64008600, A#64020000, A#64021000, A#64022000, A#64025000, A#64030000, A#64031000, A#64032000, A#64033000, A#64034000, A#64040000, A#64041000, A#64042000, A#64043000, A#64044000, A#64045000, A#64046000, A#64047000, A#64050000, A#64050500, A#64051000, A#64052000, A#64053000, A#64054000, A#64060000, A#64070000, A#64076900, A#64077000, A#64080000, A#64081000, A#64089900, A#64090000, A#64100001, A#64100900, A#64200000, A#64200001, A#64200002, A#64200003, A#64200004, A#64200005, A#64200006, A#64200007, A#64200010, A#64200011, A#64200012, A#64200013, A#64200014, A#64200015, A#64200016, A#64200017, A#64200018, A#64200019, A#64200020, A#64200021, A#64200022, A#64200023, A#64200024, A#64200025, A#64200026, A#64200027, A#64200028, A#64200030, A#64200031, A#64200091, A#64200092, A#64201000, A#64201200, A#64201300, A#64201400, A#64201600, A#64202000, A#64203000, A#64203500, A#64204000, A#64289900, A#64900000, A#64900001, A#64900002, A#64900003, A#64900004, A#64900005, A#64900006, A#64900010, A#64900011, A#64900012, A#64900013, A#64900014, A#64900015, A#64900017, A#64900018, A#64900050, A#64901100, A#64902100, A#64903100, A#64904100, A#64920000, A#64970000, A#64970050, A#64971100, A#64972100, A#64973100, A#64974100, A#64980000, A#65000000, A#65001000, A#65300000, A#65310000, A#65900001, A#66100000, A#66200000, A#66220000, A#66230001, A#66230002, A#66320001, A#66330000, A#66330001, A#66500000, A#66630000, A#66800000, A#66800001, A#66800009, A#66809999, A#66900000, A#66900001, A#66900002, A#66900003, A#66900004, A#66900005, A#66900006, A#66900007, A#66900008, A#66900009, A#66900010, A#66900011, A#66900012, A#66900013, A#66900014, A#66900015, A#66900016, A#66900020, A#66910000, A#66920000, A#66950000, A#67000000, A#67100000, A#67100001, A#67200000, A#67200001, A#67330000, A#67800000, A#67800001, A#67800002, A#67800003, A#67800004, A#67800005, A#67800006, A#67800007, A#67800008, A#67800010, A#67800011, A#67800012, A#67800100, A#67800200, A#67800201, A#67800202, A#67800203, A#67800204, A#67800205, A#67800206, A#67800207, A#67800208, A#67800299, A#67800300, A#67800400, A#67820000, A#67900001, A#68000000, A#68000001, A#68100000, A#68100001, A#68100002, A#68100003, A#68161000, A#68200001, A#69000015, A#69000016, A#69100000, A#69200000, A#69300000, A#69310000, A#69321300, A#69321400, A#69323900, A#69328000, A#69328900, A#69400000, A#69500000, A#69600000, A#69610000, A#69700000, A#69710000, A#69900000, A#69910000, A#70000001, A#70000002, A#70000004, A#70000005, A#70000006, A#70000010, A#70000011, A#70000012, A#70000013, A#70000014, A#70000015, A#70000016, A#70000017, A#70000080, A#70000082, A#70000083, A#70010000, A#70011300, A#70011400, A#70013900, A#70014000, A#70018900, A#70019800, A#70019900, A#70021300, A#70021400, A#70023900, A#70028900, A#70029800, A#70029900, A#70030000, A#70030100, A#70030200, A#70030300, A#70050001, A#70050002, A#70050003, A#70050004, A#70050005, A#70050006, A#70050007, A#70050008, A#70050020, A#70050030, A#70063900, A#70064000, A#70071300, A#70071400, A#70078900, A#70079500, A#70111700, A#70120000, A#70124000, A#70520000, A#70600000, A#70900000, A#70900100, A#70900110, A#70900111, A#70910000, A#70919000, A#71100000, A#71124000, A#72150000, A#73000000, A#73100000, A#73100001, A#73200001, A#73200002, A#74000000, A#74000001, A#74000002, A#74000003, A#74600000, A#75000000, A#75000001, A#75000002, A#75000003, A#75000004, A#75000005, A#75000006, A#75000007, A#75000008, A#75000010, A#75000011, A#75000012, A#75000013, A#75000040, A#75000099, A#75100000, A#75119000, A#75120000, A#75200000, A#75200009, A#75200010, A#75200011, A#75300001, A#75300002, A#75300004, A#75300006, A#75300007, A#75300008, A#75300010, A#75300011, A#75300020, A#75300099, A#75300100, A#75310000, A#75320000, A#75900000, A#75900001, A#75900002, A#75900003, A#75900004, A#75900005, A#75900006, A#75900007, A#75900008, A#75900009, A#75900010, A#75900011, A#75900012, A#75900013, A#75900014, A#75900015, A#75900016, A#75900020, A#75900021, A#75900100, A#75900101, A#75901500, A#75910000, A#75916000, A#75930000, A#75930100, A#75931000, A#75940000, A#75960000, A#76000001, A#76100000, A#76230000, A#76330001, A#76500000, A#76800000, A#76800001, A#76809999, A#76900001, A#76900002, A#77000000, A#77100000, A#77300000, A#77400000, A#77800000, A#77800001, A#77800002, A#77800003, A#77800004, A#77800006, A#77900001, A#79000000, A#79100000, A#79400000, A#79401000, A#79500000, A#79600000, A#80000000, A#90000000, A#90000599, A#90000699, A#90022800").Where(Function(kvp) Not accountDict.ContainsKey(kvp.Key))).ToDictionary(Function(kvp) kvp.Key, Function(kvp) kvp.Value)
									
									'Loop through all the hierarchy types to insert new members
									For Each row As DataRow In hierarchyDt.Rows
									
										If row("hierarchy").ToString <> "" Then
											
											hierarchyName = row("hierarchy").ToString
											
											accountDict = Me.CreateMemberLookupUsingFilter(si, "Accounts", "A#Root.DescendantsInclusive")
'											accountDict = accountDict.Concat(Me.CreateMemberLookupUsingFilter(si, "Accounts", "A#60000006, A#60000017, A#60000019, A#60000900, A#60004000, A#60089500, A#60100001, A#60100002, A#60100003, A#60100004, A#60100005, A#60100006, A#60100007, A#60100008, A#60100009, A#60100010, A#60100013, A#60100014, A#60100015, A#60100016, A#60100017, A#60100018, A#60100019, A#60100020, A#60100021, A#60100022, A#60100023, A#60100024, A#60100025, A#60100080, A#60100099, A#60100100, A#60100110, A#60100120, A#60100130, A#60100131, A#60100132, A#60100134, A#60100135, A#60100140, A#60100180, A#60100197, A#60100200, A#60100210, A#60100220, A#60100300, A#60100339, A#60100400, A#60101000, A#60101001, A#60101002, A#60200000, A#60200001, A#60200002, A#60200003, A#60200004, A#60200400, A#60201001, A#60600000, A#60700000, A#60900001, A#60900010, A#60901300, A#60901400, A#60908900, A#60909900, A#60929900, A#61000000, A#61000001, A#61000006, A#61014000, A#61019500, A#61021300, A#61021400, A#61023900, A#61024000, A#61028000, A#61028900, A#61029800, A#61039500, A#61100001, A#61100002, A#61100003, A#61100004, A#61100005, A#61200000, A#62100001, A#62100005, A#62100006, A#62100008, A#62100009, A#62100010, A#62100800, A#62110000, A#62110001, A#62110002, A#62110003, A#62110004, A#62110005, A#62110006, A#62110013, A#62150000, A#62200001, A#62200002, A#62200003, A#62200004, A#62200005, A#62200006, A#62200011, A#62200012, A#62200013, A#62200014, A#62200016, A#62201100, A#62201400, A#62201401, A#62201402, A#62201403, A#62201404, A#62201405, A#62201406, A#62201407, A#62201408, A#62201409, A#62201410, A#62201411, A#62300001, A#62300002, A#62300003, A#62300005, A#62300006, A#62300007, A#62300008, A#62300009, A#62300010, A#62300011, A#62300012, A#62300019, A#62300020, A#62300021, A#62300022, A#62300023, A#62300024, A#62300055, A#62300070, A#62300080, A#62300090, A#62300800, A#62300801, A#62300802, A#62300803, A#62300900, A#62302000, A#62330000, A#62359000, A#62400000, A#62400001, A#62500001, A#62500400, A#62600001, A#62700000, A#62700001, A#62700002, A#62700003, A#62700004, A#62700005, A#62700006, A#62700007, A#62700008, A#62700009, A#62700010, A#62700012, A#62700013, A#62700014, A#62700015, A#62700016, A#62700017, A#62700018, A#62700019, A#62700020, A#62700021, A#62700022, A#62700032, A#62700040, A#62730000, A#62780000, A#62790000, A#62800000, A#62800001, A#62800002, A#62800003, A#62800004, A#62800005, A#62800006, A#62800007, A#62800008, A#62800009, A#62800010, A#62800011, A#62800012, A#62800013, A#62800066, A#62810000, A#62820000, A#62900000, A#62900001, A#62900002, A#62900003, A#62900004, A#62900005, A#62900006, A#62900007, A#62900008, A#62900009, A#62900010, A#62900011, A#62900012, A#62900013, A#62900014, A#62900015, A#62900016, A#62900017, A#62900018, A#62900019, A#62900020, A#62900021, A#62900022, A#62900023, A#62900024, A#62900025, A#62900026, A#62900030, A#62900031, A#62900032, A#62900033, A#62900034, A#62900040, A#62900100, A#62900101, A#62900400, A#62900402, A#62900403, A#62900405, A#62900500, A#62902400, A#62902401, A#62950500, A#62980000, A#62988000, A#63000000, A#63000010, A#63000020, A#63000030, A#63000040, A#63040000, A#63060000, A#63100001, A#63100002, A#63100003, A#63100004, A#63100010, A#63100020, A#63100030, A#63100040, A#63100050, A#63100060, A#63100070, A#63100071, A#63100072, A#63100073, A#63100074, A#63100080, A#63160000, A#63300000, A#63800000, A#64000000, A#64000001, A#64000002, A#64000003, A#64000004, A#64000005, A#64000006, A#64000007, A#64000008, A#64000009, A#64000010, A#64000011, A#64000012, A#64000013, A#64000014, A#64000015, A#64000016, A#64000017, A#64000018, A#64000019, A#64000020, A#64000021, A#64000022, A#64000023, A#64000025, A#64000027, A#64000028, A#64000030, A#64000031, A#64000032, A#64000033, A#64000034, A#64000035, A#64000036, A#64000037, A#64000038, A#64000039, A#64000040, A#64000041, A#64000042, A#64000043, A#64000044, A#64000045, A#64000046, A#64000047, A#64000048, A#64000049, A#64000050, A#64000051, A#64000052, A#64000053, A#64000054, A#64000055, A#64000056, A#64000057, A#64000058, A#64000059, A#64000060, A#64000092, A#64000093, A#64000094, A#64000100, A#64000101, A#64000102, A#64000103, A#64000104, A#64000105, A#64000106, A#64000107, A#64000108, A#64000132, A#64000232, A#64000332, A#64000432, A#64000532, A#64001100, A#64001101, A#64001110, A#64001120, A#64001210, A#64001220, A#64001230, A#64001240, A#64001250, A#64001260, A#64001270, A#64001280, A#64001300, A#64001400, A#64001500, A#64001600, A#64001700, A#64002100, A#64002101, A#64002110, A#64002111, A#64002120, A#64002210, A#64002220, A#64002230, A#64002240, A#64002250, A#64002260, A#64002280, A#64002300, A#64002400, A#64002500, A#64002600, A#64002700, A#64003000, A#64003100, A#64003101, A#64003110, A#64003120, A#64003210, A#64003220, A#64003230, A#64003240, A#64003250, A#64003260, A#64003280, A#64003300, A#64003400, A#64003500, A#64003600, A#64003700, A#64004100, A#64004101, A#64004110, A#64004120, A#64004210, A#64004220, A#64004230, A#64004240, A#64004250, A#64004260, A#64004280, A#64004300, A#64004400, A#64004500, A#64004600, A#64004700, A#64005000, A#64005100, A#64005101, A#64005110, A#64005120, A#64005210, A#64005220, A#64005230, A#64005240, A#64005250, A#64005260, A#64005280, A#64005300, A#64005400, A#64005500, A#64005600, A#64005700, A#64006100, A#64006101, A#64006110, A#64006120, A#64006210, A#64006220, A#64006230, A#64006240, A#64006250, A#64006260, A#64006280, A#64006300, A#64006400, A#64006500, A#64006600, A#64006700, A#64007100, A#64007110, A#64007120, A#64007210, A#64007220, A#64007230, A#64007240, A#64007250, A#64007260, A#64007280, A#64007300, A#64007400, A#64007500, A#64007600, A#64008100, A#64008110, A#64008120, A#64008210, A#64008220, A#64008230, A#64008240, A#64008250, A#64008260, A#64008280, A#64008300, A#64008400, A#64008500, A#64008600, A#64020000, A#64021000, A#64022000, A#64025000, A#64030000, A#64031000, A#64032000, A#64033000, A#64034000, A#64040000, A#64041000, A#64042000, A#64043000, A#64044000, A#64045000, A#64046000, A#64047000, A#64050000, A#64050500, A#64051000, A#64052000, A#64053000, A#64054000, A#64060000, A#64070000, A#64076900, A#64077000, A#64080000, A#64081000, A#64089900, A#64090000, A#64100001, A#64100900, A#64200000, A#64200001, A#64200002, A#64200003, A#64200004, A#64200005, A#64200006, A#64200007, A#64200010, A#64200011, A#64200012, A#64200013, A#64200014, A#64200015, A#64200016, A#64200017, A#64200018, A#64200019, A#64200020, A#64200021, A#64200022, A#64200023, A#64200024, A#64200025, A#64200026, A#64200027, A#64200028, A#64200030, A#64200031, A#64200091, A#64200092, A#64201000, A#64201200, A#64201300, A#64201400, A#64201600, A#64202000, A#64203000, A#64203500, A#64204000, A#64289900, A#64900000, A#64900001, A#64900002, A#64900003, A#64900004, A#64900005, A#64900006, A#64900010, A#64900011, A#64900012, A#64900013, A#64900014, A#64900015, A#64900017, A#64900018, A#64900050, A#64901100, A#64902100, A#64903100, A#64904100, A#64920000, A#64970000, A#64970050, A#64971100, A#64972100, A#64973100, A#64974100, A#64980000, A#65000000, A#65001000, A#65300000, A#65310000, A#65900001, A#66100000, A#66200000, A#66220000, A#66230001, A#66230002, A#66320001, A#66330000, A#66330001, A#66500000, A#66630000, A#66800000, A#66800001, A#66800009, A#66809999, A#66900000, A#66900001, A#66900002, A#66900003, A#66900004, A#66900005, A#66900006, A#66900007, A#66900008, A#66900009, A#66900010, A#66900011, A#66900012, A#66900013, A#66900014, A#66900015, A#66900016, A#66900020, A#66910000, A#66920000, A#66950000, A#67000000, A#67100000, A#67100001, A#67200000, A#67200001, A#67330000, A#67800000, A#67800001, A#67800002, A#67800003, A#67800004, A#67800005, A#67800006, A#67800007, A#67800008, A#67800010, A#67800011, A#67800012, A#67800100, A#67800200, A#67800201, A#67800202, A#67800203, A#67800204, A#67800205, A#67800206, A#67800207, A#67800208, A#67800299, A#67800300, A#67800400, A#67820000, A#67900001, A#68000000, A#68000001, A#68100000, A#68100001, A#68100002, A#68100003, A#68161000, A#68200001, A#69000015, A#69000016, A#69100000, A#69200000, A#69300000, A#69310000, A#69321300, A#69321400, A#69323900, A#69328000, A#69328900, A#69400000, A#69500000, A#69600000, A#69610000, A#69700000, A#69710000, A#69900000, A#69910000, A#70000001, A#70000002, A#70000004, A#70000005, A#70000006, A#70000010, A#70000011, A#70000012, A#70000013, A#70000014, A#70000015, A#70000016, A#70000017, A#70000080, A#70000082, A#70000083, A#70010000, A#70011300, A#70011400, A#70013900, A#70014000, A#70018900, A#70019800, A#70019900, A#70021300, A#70021400, A#70023900, A#70028900, A#70029800, A#70029900, A#70030000, A#70030100, A#70030200, A#70030300, A#70050001, A#70050002, A#70050003, A#70050004, A#70050005, A#70050006, A#70050007, A#70050008, A#70050020, A#70050030, A#70063900, A#70064000, A#70071300, A#70071400, A#70078900, A#70079500, A#70111700, A#70120000, A#70124000, A#70520000, A#70600000, A#70900000, A#70900100, A#70900110, A#70900111, A#70910000, A#70919000, A#71100000, A#71124000, A#72150000, A#73000000, A#73100000, A#73100001, A#73200001, A#73200002, A#74000000, A#74000001, A#74000002, A#74000003, A#74600000, A#75000000, A#75000001, A#75000002, A#75000003, A#75000004, A#75000005, A#75000006, A#75000007, A#75000008, A#75000010, A#75000011, A#75000012, A#75000013, A#75000040, A#75000099, A#75100000, A#75119000, A#75120000, A#75200000, A#75200009, A#75200010, A#75200011, A#75300001, A#75300002, A#75300004, A#75300006, A#75300007, A#75300008, A#75300010, A#75300011, A#75300020, A#75300099, A#75300100, A#75310000, A#75320000, A#75900000, A#75900001, A#75900002, A#75900003, A#75900004, A#75900005, A#75900006, A#75900007, A#75900008, A#75900009, A#75900010, A#75900011, A#75900012, A#75900013, A#75900014, A#75900015, A#75900016, A#75900020, A#75900021, A#75900100, A#75900101, A#75901500, A#75910000, A#75916000, A#75930000, A#75930100, A#75931000, A#75940000, A#75960000, A#76000001, A#76100000, A#76230000, A#76330001, A#76500000, A#76800000, A#76800001, A#76809999, A#76900001, A#76900002, A#77000000, A#77100000, A#77300000, A#77400000, A#77800000, A#77800001, A#77800002, A#77800003, A#77800004, A#77800006, A#77900001, A#79000000, A#79100000, A#79400000, A#79401000, A#79500000, A#79600000, A#80000000, A#90000000, A#90000599, A#90000699, A#90022800").Where(Function(kvp) Not accountDict.ContainsKey(kvp.Key))).ToDictionary(Function(kvp) kvp.Key, Function(kvp) kvp.Value)
											
											'Build a query to get each cebe info with it's parent
											selectQuery = $"SELECT
																ah1.id,
																ah1.account_number,
																CASE
																	WHEN a1.descriptive IS NOT NULL THEN a1.descriptive
																	ELSE ah1.description
																END AS description,
																ah1.father_id,
																ah2.account_number AS father_account_number,
																CASE
												                	WHEN a2.descriptive IS NOT NULL THEN a2.descriptive
												                	ELSE ah2.description
												                END AS father_description
															FROM
																dbo.XFC_AccountHierarchy AS ah1
															LEFT JOIN
																dbo.XFC_Accounts AS a1 ON ah1.account_number = CAST(a1.account_number AS VARCHAR)
															LEFT JOIN
																dbo.XFC_AccountHierarchy AS ah2 ON ah1.father_id = ah2.id
																AND ah2.hierarchy = '{hierarchyName}'
															LEFT JOIN
																dbo.XFC_Accounts AS a2 ON ah2.account_number = CAST(a2.account_number AS VARCHAR)
															WHERE
																ah1.hierarchy = '{hierarchyName}'
																AND
																	(
																		ah1.account_number NOT IN ({accountsInBase})
																		AND
																		(
																			ah1.account_number NOT LIKE '[0-9]%'
																			OR
																			ah1.account_number IN ({accountsNotInBase})
																		)
																	)
																
															ORDER BY
																ah1.id ASC;"
															
											
											hierarchyName = hierarchyName.Replace("-","_")
											
											Dim AccountsDt As DataTable = BRApi.Database.ExecuteSql(dbcon,selectQuery,False)
											
											If AccountsDt IsNot Nothing AndAlso AccountsDt.Rows.Count > 0 Then
												
												Dim accountName As String
												Dim accountDescription As String
												Dim accountParentName As String
												
												'Loop through all the members to insert them
												For Each accountRow As DataRow In accountsDt.Rows
													
													'Get info to create member
													accountName = accountRow("account_number").ToString.Replace("-","_").Replace("/","_")
													accountDescription = accountRow("description").ToString
													accountParentName = IIf(accountRow("father_id") = "0", "Root", accountRow("father_account_number").ToString).Replace("-","_").Replace("/","_")
													
													'If member is not already created, create it
													If Not accountDict.ContainsKey(accountName) Then
														'Final check in case it's an orfan
														Dim mbrInfo As Member = BRApi.Finance.Members.GetMember(si, dimTypeId.Account, accountName)
														If mbrInfo IsNot Nothing AndAlso mbrInfo.MemberId <> -1 Then Continue For
														
														Me.AddAccountMember(si, dimensionTypeId, "Accounts", accountName, accountDescription, accountParentName)
														
													End If
													
												Next
												
												'Loop through all the members to remove relationships
												For Each accountRow As DataRow In accountsDt.Rows
													
													'Get member info
													accountName = accountRow("account_number").ToString.Replace("-","_").Replace("/","_")
													
													'Remove relationship
													Me.RemoveMemberRelationships(si, dimensionTypeId, "Accounts", accountName)
													Me.RemoveMemberRelationships(si, dimensionTypeId, "Accounts_Base", accountName)
													
												Next
												
												'Loop through all the members to update the relationships
												For Each accountRow As DataRow In accountsDt.Rows
													
													'Get member info
													accountName = accountRow("account_number").ToString.Replace("-","_").Replace("/","_")
													accountDescription = accountRow("description").ToString
													accountParentName = IIf(accountRow("father_id") = "0", "Root", accountRow("father_account_number").ToString).Replace("-","_").Replace("/","_")
													
													'Update relationship
													Me.AddMemberRelationship(si, dimensionTypeId, "Accounts", accountName, accountDescription, accountParentName)
													
												Next
												
											End If
											
										End If
										
									Next
									
									'Create base level members
									accountDict = Me.CreateMemberLookupUsingFilter(si, "Accounts_Base", "A#Root.DescendantsInclusive")
'									accountDict = accountDict.Concat(Me.CreateMemberLookupUsingFilter(si, "Accounts", "A#60000006, A#60000017, A#60000019, A#60000900, A#60004000, A#60089500, A#60100001, A#60100002, A#60100003, A#60100004, A#60100005, A#60100006, A#60100007, A#60100008, A#60100009, A#60100010, A#60100013, A#60100014, A#60100015, A#60100016, A#60100017, A#60100018, A#60100019, A#60100020, A#60100021, A#60100022, A#60100023, A#60100024, A#60100025, A#60100080, A#60100099, A#60100100, A#60100110, A#60100120, A#60100130, A#60100131, A#60100132, A#60100134, A#60100135, A#60100140, A#60100180, A#60100197, A#60100200, A#60100210, A#60100220, A#60100300, A#60100339, A#60100400, A#60101000, A#60101001, A#60101002, A#60200000, A#60200001, A#60200002, A#60200003, A#60200004, A#60200400, A#60201001, A#60600000, A#60700000, A#60900001, A#60900010, A#60901300, A#60901400, A#60908900, A#60909900, A#60929900, A#61000000, A#61000001, A#61000006, A#61014000, A#61019500, A#61021300, A#61021400, A#61023900, A#61024000, A#61028000, A#61028900, A#61029800, A#61039500, A#61100001, A#61100002, A#61100003, A#61100004, A#61100005, A#61200000, A#62100001, A#62100005, A#62100006, A#62100008, A#62100009, A#62100010, A#62100800, A#62110000, A#62110001, A#62110002, A#62110003, A#62110004, A#62110005, A#62110006, A#62110013, A#62150000, A#62200001, A#62200002, A#62200003, A#62200004, A#62200005, A#62200006, A#62200011, A#62200012, A#62200013, A#62200014, A#62200016, A#62201100, A#62201400, A#62201401, A#62201402, A#62201403, A#62201404, A#62201405, A#62201406, A#62201407, A#62201408, A#62201409, A#62201410, A#62201411, A#62300001, A#62300002, A#62300003, A#62300005, A#62300006, A#62300007, A#62300008, A#62300009, A#62300010, A#62300011, A#62300012, A#62300019, A#62300020, A#62300021, A#62300022, A#62300023, A#62300024, A#62300055, A#62300070, A#62300080, A#62300090, A#62300800, A#62300801, A#62300802, A#62300803, A#62300900, A#62302000, A#62330000, A#62359000, A#62400000, A#62400001, A#62500001, A#62500400, A#62600001, A#62700000, A#62700001, A#62700002, A#62700003, A#62700004, A#62700005, A#62700006, A#62700007, A#62700008, A#62700009, A#62700010, A#62700012, A#62700013, A#62700014, A#62700015, A#62700016, A#62700017, A#62700018, A#62700019, A#62700020, A#62700021, A#62700022, A#62700032, A#62700040, A#62730000, A#62780000, A#62790000, A#62800000, A#62800001, A#62800002, A#62800003, A#62800004, A#62800005, A#62800006, A#62800007, A#62800008, A#62800009, A#62800010, A#62800011, A#62800012, A#62800013, A#62800066, A#62810000, A#62820000, A#62900000, A#62900001, A#62900002, A#62900003, A#62900004, A#62900005, A#62900006, A#62900007, A#62900008, A#62900009, A#62900010, A#62900011, A#62900012, A#62900013, A#62900014, A#62900015, A#62900016, A#62900017, A#62900018, A#62900019, A#62900020, A#62900021, A#62900022, A#62900023, A#62900024, A#62900025, A#62900026, A#62900030, A#62900031, A#62900032, A#62900033, A#62900034, A#62900040, A#62900100, A#62900101, A#62900400, A#62900402, A#62900403, A#62900405, A#62900500, A#62902400, A#62902401, A#62950500, A#62980000, A#62988000, A#63000000, A#63000010, A#63000020, A#63000030, A#63000040, A#63040000, A#63060000, A#63100001, A#63100002, A#63100003, A#63100004, A#63100010, A#63100020, A#63100030, A#63100040, A#63100050, A#63100060, A#63100070, A#63100071, A#63100072, A#63100073, A#63100074, A#63100080, A#63160000, A#63300000, A#63800000, A#64000000, A#64000001, A#64000002, A#64000003, A#64000004, A#64000005, A#64000006, A#64000007, A#64000008, A#64000009, A#64000010, A#64000011, A#64000012, A#64000013, A#64000014, A#64000015, A#64000016, A#64000017, A#64000018, A#64000019, A#64000020, A#64000021, A#64000022, A#64000023, A#64000025, A#64000027, A#64000028, A#64000030, A#64000031, A#64000032, A#64000033, A#64000034, A#64000035, A#64000036, A#64000037, A#64000038, A#64000039, A#64000040, A#64000041, A#64000042, A#64000043, A#64000044, A#64000045, A#64000046, A#64000047, A#64000048, A#64000049, A#64000050, A#64000051, A#64000052, A#64000053, A#64000054, A#64000055, A#64000056, A#64000057, A#64000058, A#64000059, A#64000060, A#64000092, A#64000093, A#64000094, A#64000100, A#64000101, A#64000102, A#64000103, A#64000104, A#64000105, A#64000106, A#64000107, A#64000108, A#64000132, A#64000232, A#64000332, A#64000432, A#64000532, A#64001100, A#64001101, A#64001110, A#64001120, A#64001210, A#64001220, A#64001230, A#64001240, A#64001250, A#64001260, A#64001270, A#64001280, A#64001300, A#64001400, A#64001500, A#64001600, A#64001700, A#64002100, A#64002101, A#64002110, A#64002111, A#64002120, A#64002210, A#64002220, A#64002230, A#64002240, A#64002250, A#64002260, A#64002280, A#64002300, A#64002400, A#64002500, A#64002600, A#64002700, A#64003000, A#64003100, A#64003101, A#64003110, A#64003120, A#64003210, A#64003220, A#64003230, A#64003240, A#64003250, A#64003260, A#64003280, A#64003300, A#64003400, A#64003500, A#64003600, A#64003700, A#64004100, A#64004101, A#64004110, A#64004120, A#64004210, A#64004220, A#64004230, A#64004240, A#64004250, A#64004260, A#64004280, A#64004300, A#64004400, A#64004500, A#64004600, A#64004700, A#64005000, A#64005100, A#64005101, A#64005110, A#64005120, A#64005210, A#64005220, A#64005230, A#64005240, A#64005250, A#64005260, A#64005280, A#64005300, A#64005400, A#64005500, A#64005600, A#64005700, A#64006100, A#64006101, A#64006110, A#64006120, A#64006210, A#64006220, A#64006230, A#64006240, A#64006250, A#64006260, A#64006280, A#64006300, A#64006400, A#64006500, A#64006600, A#64006700, A#64007100, A#64007110, A#64007120, A#64007210, A#64007220, A#64007230, A#64007240, A#64007250, A#64007260, A#64007280, A#64007300, A#64007400, A#64007500, A#64007600, A#64008100, A#64008110, A#64008120, A#64008210, A#64008220, A#64008230, A#64008240, A#64008250, A#64008260, A#64008280, A#64008300, A#64008400, A#64008500, A#64008600, A#64020000, A#64021000, A#64022000, A#64025000, A#64030000, A#64031000, A#64032000, A#64033000, A#64034000, A#64040000, A#64041000, A#64042000, A#64043000, A#64044000, A#64045000, A#64046000, A#64047000, A#64050000, A#64050500, A#64051000, A#64052000, A#64053000, A#64054000, A#64060000, A#64070000, A#64076900, A#64077000, A#64080000, A#64081000, A#64089900, A#64090000, A#64100001, A#64100900, A#64200000, A#64200001, A#64200002, A#64200003, A#64200004, A#64200005, A#64200006, A#64200007, A#64200010, A#64200011, A#64200012, A#64200013, A#64200014, A#64200015, A#64200016, A#64200017, A#64200018, A#64200019, A#64200020, A#64200021, A#64200022, A#64200023, A#64200024, A#64200025, A#64200026, A#64200027, A#64200028, A#64200030, A#64200031, A#64200091, A#64200092, A#64201000, A#64201200, A#64201300, A#64201400, A#64201600, A#64202000, A#64203000, A#64203500, A#64204000, A#64289900, A#64900000, A#64900001, A#64900002, A#64900003, A#64900004, A#64900005, A#64900006, A#64900010, A#64900011, A#64900012, A#64900013, A#64900014, A#64900015, A#64900017, A#64900018, A#64900050, A#64901100, A#64902100, A#64903100, A#64904100, A#64920000, A#64970000, A#64970050, A#64971100, A#64972100, A#64973100, A#64974100, A#64980000, A#65000000, A#65001000, A#65300000, A#65310000, A#65900001, A#66100000, A#66200000, A#66220000, A#66230001, A#66230002, A#66320001, A#66330000, A#66330001, A#66500000, A#66630000, A#66800000, A#66800001, A#66800009, A#66809999, A#66900000, A#66900001, A#66900002, A#66900003, A#66900004, A#66900005, A#66900006, A#66900007, A#66900008, A#66900009, A#66900010, A#66900011, A#66900012, A#66900013, A#66900014, A#66900015, A#66900016, A#66900020, A#66910000, A#66920000, A#66950000, A#67000000, A#67100000, A#67100001, A#67200000, A#67200001, A#67330000, A#67800000, A#67800001, A#67800002, A#67800003, A#67800004, A#67800005, A#67800006, A#67800007, A#67800008, A#67800010, A#67800011, A#67800012, A#67800100, A#67800200, A#67800201, A#67800202, A#67800203, A#67800204, A#67800205, A#67800206, A#67800207, A#67800208, A#67800299, A#67800300, A#67800400, A#67820000, A#67900001, A#68000000, A#68000001, A#68100000, A#68100001, A#68100002, A#68100003, A#68161000, A#68200001, A#69000015, A#69000016, A#69100000, A#69200000, A#69300000, A#69310000, A#69321300, A#69321400, A#69323900, A#69328000, A#69328900, A#69400000, A#69500000, A#69600000, A#69610000, A#69700000, A#69710000, A#69900000, A#69910000, A#70000001, A#70000002, A#70000004, A#70000005, A#70000006, A#70000010, A#70000011, A#70000012, A#70000013, A#70000014, A#70000015, A#70000016, A#70000017, A#70000080, A#70000082, A#70000083, A#70010000, A#70011300, A#70011400, A#70013900, A#70014000, A#70018900, A#70019800, A#70019900, A#70021300, A#70021400, A#70023900, A#70028900, A#70029800, A#70029900, A#70030000, A#70030100, A#70030200, A#70030300, A#70050001, A#70050002, A#70050003, A#70050004, A#70050005, A#70050006, A#70050007, A#70050008, A#70050020, A#70050030, A#70063900, A#70064000, A#70071300, A#70071400, A#70078900, A#70079500, A#70111700, A#70120000, A#70124000, A#70520000, A#70600000, A#70900000, A#70900100, A#70900110, A#70900111, A#70910000, A#70919000, A#71100000, A#71124000, A#72150000, A#73000000, A#73100000, A#73100001, A#73200001, A#73200002, A#74000000, A#74000001, A#74000002, A#74000003, A#74600000, A#75000000, A#75000001, A#75000002, A#75000003, A#75000004, A#75000005, A#75000006, A#75000007, A#75000008, A#75000010, A#75000011, A#75000012, A#75000013, A#75000040, A#75000099, A#75100000, A#75119000, A#75120000, A#75200000, A#75200009, A#75200010, A#75200011, A#75300001, A#75300002, A#75300004, A#75300006, A#75300007, A#75300008, A#75300010, A#75300011, A#75300020, A#75300099, A#75300100, A#75310000, A#75320000, A#75900000, A#75900001, A#75900002, A#75900003, A#75900004, A#75900005, A#75900006, A#75900007, A#75900008, A#75900009, A#75900010, A#75900011, A#75900012, A#75900013, A#75900014, A#75900015, A#75900016, A#75900020, A#75900021, A#75900100, A#75900101, A#75901500, A#75910000, A#75916000, A#75930000, A#75930100, A#75931000, A#75940000, A#75960000, A#76000001, A#76100000, A#76230000, A#76330001, A#76500000, A#76800000, A#76800001, A#76809999, A#76900001, A#76900002, A#77000000, A#77100000, A#77300000, A#77400000, A#77800000, A#77800001, A#77800002, A#77800003, A#77800004, A#77800006, A#77900001, A#79000000, A#79100000, A#79400000, A#79401000, A#79500000, A#79600000, A#80000000, A#90000000, A#90000599, A#90000699, A#90022800").Where(Function(kvp) Not accountDict.ContainsKey(kvp.Key))).ToDictionary(Function(kvp) kvp.Key, Function(kvp) kvp.Value)
									
									'Loop through all the hierarchy types to insert new members
									For Each row As DataRow In hierarchyDt.Rows
									
										If row("hierarchy").ToString <> "" Then
											
											hierarchyName = row("hierarchy").ToString
											
											accountDict = Me.CreateMemberLookupUsingFilter(si, "Accounts_Base", "A#Root.DescendantsInclusive")
'											accountDict = accountDict.Concat(Me.CreateMemberLookupUsingFilter(si, "Accounts", "A#60000006, A#60000017, A#60000019, A#60000900, A#60004000, A#60089500, A#60100001, A#60100002, A#60100003, A#60100004, A#60100005, A#60100006, A#60100007, A#60100008, A#60100009, A#60100010, A#60100013, A#60100014, A#60100015, A#60100016, A#60100017, A#60100018, A#60100019, A#60100020, A#60100021, A#60100022, A#60100023, A#60100024, A#60100025, A#60100080, A#60100099, A#60100100, A#60100110, A#60100120, A#60100130, A#60100131, A#60100132, A#60100134, A#60100135, A#60100140, A#60100180, A#60100197, A#60100200, A#60100210, A#60100220, A#60100300, A#60100339, A#60100400, A#60101000, A#60101001, A#60101002, A#60200000, A#60200001, A#60200002, A#60200003, A#60200004, A#60200400, A#60201001, A#60600000, A#60700000, A#60900001, A#60900010, A#60901300, A#60901400, A#60908900, A#60909900, A#60929900, A#61000000, A#61000001, A#61000006, A#61014000, A#61019500, A#61021300, A#61021400, A#61023900, A#61024000, A#61028000, A#61028900, A#61029800, A#61039500, A#61100001, A#61100002, A#61100003, A#61100004, A#61100005, A#61200000, A#62100001, A#62100005, A#62100006, A#62100008, A#62100009, A#62100010, A#62100800, A#62110000, A#62110001, A#62110002, A#62110003, A#62110004, A#62110005, A#62110006, A#62110013, A#62150000, A#62200001, A#62200002, A#62200003, A#62200004, A#62200005, A#62200006, A#62200011, A#62200012, A#62200013, A#62200014, A#62200016, A#62201100, A#62201400, A#62201401, A#62201402, A#62201403, A#62201404, A#62201405, A#62201406, A#62201407, A#62201408, A#62201409, A#62201410, A#62201411, A#62300001, A#62300002, A#62300003, A#62300005, A#62300006, A#62300007, A#62300008, A#62300009, A#62300010, A#62300011, A#62300012, A#62300019, A#62300020, A#62300021, A#62300022, A#62300023, A#62300024, A#62300055, A#62300070, A#62300080, A#62300090, A#62300800, A#62300801, A#62300802, A#62300803, A#62300900, A#62302000, A#62330000, A#62359000, A#62400000, A#62400001, A#62500001, A#62500400, A#62600001, A#62700000, A#62700001, A#62700002, A#62700003, A#62700004, A#62700005, A#62700006, A#62700007, A#62700008, A#62700009, A#62700010, A#62700012, A#62700013, A#62700014, A#62700015, A#62700016, A#62700017, A#62700018, A#62700019, A#62700020, A#62700021, A#62700022, A#62700032, A#62700040, A#62730000, A#62780000, A#62790000, A#62800000, A#62800001, A#62800002, A#62800003, A#62800004, A#62800005, A#62800006, A#62800007, A#62800008, A#62800009, A#62800010, A#62800011, A#62800012, A#62800013, A#62800066, A#62810000, A#62820000, A#62900000, A#62900001, A#62900002, A#62900003, A#62900004, A#62900005, A#62900006, A#62900007, A#62900008, A#62900009, A#62900010, A#62900011, A#62900012, A#62900013, A#62900014, A#62900015, A#62900016, A#62900017, A#62900018, A#62900019, A#62900020, A#62900021, A#62900022, A#62900023, A#62900024, A#62900025, A#62900026, A#62900030, A#62900031, A#62900032, A#62900033, A#62900034, A#62900040, A#62900100, A#62900101, A#62900400, A#62900402, A#62900403, A#62900405, A#62900500, A#62902400, A#62902401, A#62950500, A#62980000, A#62988000, A#63000000, A#63000010, A#63000020, A#63000030, A#63000040, A#63040000, A#63060000, A#63100001, A#63100002, A#63100003, A#63100004, A#63100010, A#63100020, A#63100030, A#63100040, A#63100050, A#63100060, A#63100070, A#63100071, A#63100072, A#63100073, A#63100074, A#63100080, A#63160000, A#63300000, A#63800000, A#64000000, A#64000001, A#64000002, A#64000003, A#64000004, A#64000005, A#64000006, A#64000007, A#64000008, A#64000009, A#64000010, A#64000011, A#64000012, A#64000013, A#64000014, A#64000015, A#64000016, A#64000017, A#64000018, A#64000019, A#64000020, A#64000021, A#64000022, A#64000023, A#64000025, A#64000027, A#64000028, A#64000030, A#64000031, A#64000032, A#64000033, A#64000034, A#64000035, A#64000036, A#64000037, A#64000038, A#64000039, A#64000040, A#64000041, A#64000042, A#64000043, A#64000044, A#64000045, A#64000046, A#64000047, A#64000048, A#64000049, A#64000050, A#64000051, A#64000052, A#64000053, A#64000054, A#64000055, A#64000056, A#64000057, A#64000058, A#64000059, A#64000060, A#64000092, A#64000093, A#64000094, A#64000100, A#64000101, A#64000102, A#64000103, A#64000104, A#64000105, A#64000106, A#64000107, A#64000108, A#64000132, A#64000232, A#64000332, A#64000432, A#64000532, A#64001100, A#64001101, A#64001110, A#64001120, A#64001210, A#64001220, A#64001230, A#64001240, A#64001250, A#64001260, A#64001270, A#64001280, A#64001300, A#64001400, A#64001500, A#64001600, A#64001700, A#64002100, A#64002101, A#64002110, A#64002111, A#64002120, A#64002210, A#64002220, A#64002230, A#64002240, A#64002250, A#64002260, A#64002280, A#64002300, A#64002400, A#64002500, A#64002600, A#64002700, A#64003000, A#64003100, A#64003101, A#64003110, A#64003120, A#64003210, A#64003220, A#64003230, A#64003240, A#64003250, A#64003260, A#64003280, A#64003300, A#64003400, A#64003500, A#64003600, A#64003700, A#64004100, A#64004101, A#64004110, A#64004120, A#64004210, A#64004220, A#64004230, A#64004240, A#64004250, A#64004260, A#64004280, A#64004300, A#64004400, A#64004500, A#64004600, A#64004700, A#64005000, A#64005100, A#64005101, A#64005110, A#64005120, A#64005210, A#64005220, A#64005230, A#64005240, A#64005250, A#64005260, A#64005280, A#64005300, A#64005400, A#64005500, A#64005600, A#64005700, A#64006100, A#64006101, A#64006110, A#64006120, A#64006210, A#64006220, A#64006230, A#64006240, A#64006250, A#64006260, A#64006280, A#64006300, A#64006400, A#64006500, A#64006600, A#64006700, A#64007100, A#64007110, A#64007120, A#64007210, A#64007220, A#64007230, A#64007240, A#64007250, A#64007260, A#64007280, A#64007300, A#64007400, A#64007500, A#64007600, A#64008100, A#64008110, A#64008120, A#64008210, A#64008220, A#64008230, A#64008240, A#64008250, A#64008260, A#64008280, A#64008300, A#64008400, A#64008500, A#64008600, A#64020000, A#64021000, A#64022000, A#64025000, A#64030000, A#64031000, A#64032000, A#64033000, A#64034000, A#64040000, A#64041000, A#64042000, A#64043000, A#64044000, A#64045000, A#64046000, A#64047000, A#64050000, A#64050500, A#64051000, A#64052000, A#64053000, A#64054000, A#64060000, A#64070000, A#64076900, A#64077000, A#64080000, A#64081000, A#64089900, A#64090000, A#64100001, A#64100900, A#64200000, A#64200001, A#64200002, A#64200003, A#64200004, A#64200005, A#64200006, A#64200007, A#64200010, A#64200011, A#64200012, A#64200013, A#64200014, A#64200015, A#64200016, A#64200017, A#64200018, A#64200019, A#64200020, A#64200021, A#64200022, A#64200023, A#64200024, A#64200025, A#64200026, A#64200027, A#64200028, A#64200030, A#64200031, A#64200091, A#64200092, A#64201000, A#64201200, A#64201300, A#64201400, A#64201600, A#64202000, A#64203000, A#64203500, A#64204000, A#64289900, A#64900000, A#64900001, A#64900002, A#64900003, A#64900004, A#64900005, A#64900006, A#64900010, A#64900011, A#64900012, A#64900013, A#64900014, A#64900015, A#64900017, A#64900018, A#64900050, A#64901100, A#64902100, A#64903100, A#64904100, A#64920000, A#64970000, A#64970050, A#64971100, A#64972100, A#64973100, A#64974100, A#64980000, A#65000000, A#65001000, A#65300000, A#65310000, A#65900001, A#66100000, A#66200000, A#66220000, A#66230001, A#66230002, A#66320001, A#66330000, A#66330001, A#66500000, A#66630000, A#66800000, A#66800001, A#66800009, A#66809999, A#66900000, A#66900001, A#66900002, A#66900003, A#66900004, A#66900005, A#66900006, A#66900007, A#66900008, A#66900009, A#66900010, A#66900011, A#66900012, A#66900013, A#66900014, A#66900015, A#66900016, A#66900020, A#66910000, A#66920000, A#66950000, A#67000000, A#67100000, A#67100001, A#67200000, A#67200001, A#67330000, A#67800000, A#67800001, A#67800002, A#67800003, A#67800004, A#67800005, A#67800006, A#67800007, A#67800008, A#67800010, A#67800011, A#67800012, A#67800100, A#67800200, A#67800201, A#67800202, A#67800203, A#67800204, A#67800205, A#67800206, A#67800207, A#67800208, A#67800299, A#67800300, A#67800400, A#67820000, A#67900001, A#68000000, A#68000001, A#68100000, A#68100001, A#68100002, A#68100003, A#68161000, A#68200001, A#69000015, A#69000016, A#69100000, A#69200000, A#69300000, A#69310000, A#69321300, A#69321400, A#69323900, A#69328000, A#69328900, A#69400000, A#69500000, A#69600000, A#69610000, A#69700000, A#69710000, A#69900000, A#69910000, A#70000001, A#70000002, A#70000004, A#70000005, A#70000006, A#70000010, A#70000011, A#70000012, A#70000013, A#70000014, A#70000015, A#70000016, A#70000017, A#70000080, A#70000082, A#70000083, A#70010000, A#70011300, A#70011400, A#70013900, A#70014000, A#70018900, A#70019800, A#70019900, A#70021300, A#70021400, A#70023900, A#70028900, A#70029800, A#70029900, A#70030000, A#70030100, A#70030200, A#70030300, A#70050001, A#70050002, A#70050003, A#70050004, A#70050005, A#70050006, A#70050007, A#70050008, A#70050020, A#70050030, A#70063900, A#70064000, A#70071300, A#70071400, A#70078900, A#70079500, A#70111700, A#70120000, A#70124000, A#70520000, A#70600000, A#70900000, A#70900100, A#70900110, A#70900111, A#70910000, A#70919000, A#71100000, A#71124000, A#72150000, A#73000000, A#73100000, A#73100001, A#73200001, A#73200002, A#74000000, A#74000001, A#74000002, A#74000003, A#74600000, A#75000000, A#75000001, A#75000002, A#75000003, A#75000004, A#75000005, A#75000006, A#75000007, A#75000008, A#75000010, A#75000011, A#75000012, A#75000013, A#75000040, A#75000099, A#75100000, A#75119000, A#75120000, A#75200000, A#75200009, A#75200010, A#75200011, A#75300001, A#75300002, A#75300004, A#75300006, A#75300007, A#75300008, A#75300010, A#75300011, A#75300020, A#75300099, A#75300100, A#75310000, A#75320000, A#75900000, A#75900001, A#75900002, A#75900003, A#75900004, A#75900005, A#75900006, A#75900007, A#75900008, A#75900009, A#75900010, A#75900011, A#75900012, A#75900013, A#75900014, A#75900015, A#75900016, A#75900020, A#75900021, A#75900100, A#75900101, A#75901500, A#75910000, A#75916000, A#75930000, A#75930100, A#75931000, A#75940000, A#75960000, A#76000001, A#76100000, A#76230000, A#76330001, A#76500000, A#76800000, A#76800001, A#76809999, A#76900001, A#76900002, A#77000000, A#77100000, A#77300000, A#77400000, A#77800000, A#77800001, A#77800002, A#77800003, A#77800004, A#77800006, A#77900001, A#79000000, A#79100000, A#79400000, A#79401000, A#79500000, A#79600000, A#80000000, A#90000000, A#90000599, A#90000699, A#90022800").Where(Function(kvp) Not accountDict.ContainsKey(kvp.Key))).ToDictionary(Function(kvp) kvp.Key, Function(kvp) kvp.Value)
											
											'Build a query to get each cebe info with it's parent
											selectQuery = $"SELECT
																ah1.id,
																ah1.account_number,
																CASE
																	WHEN a1.descriptive IS NOT NULL THEN a1.descriptive
																	ELSE ah1.description
																END AS description,
																ah1.father_id,
																ah2.account_number AS father_account_number,
																CASE
												                	WHEN a2.descriptive IS NOT NULL THEN a2.descriptive
												                	ELSE ah2.description
												                END AS father_description
															FROM
																dbo.XFC_AccountHierarchy AS ah1
															LEFT JOIN
																dbo.XFC_Accounts AS a1 ON ah1.account_number = CAST(a1.account_number AS VARCHAR)
															LEFT JOIN
																dbo.XFC_AccountHierarchy AS ah2 ON ah1.father_id = ah2.id
																AND ah2.hierarchy = '{hierarchyName}'
															LEFT JOIN
																dbo.XFC_Accounts AS a2 ON ah2.account_number = CAST(a2.account_number AS VARCHAR)
															WHERE
																ah1.hierarchy = '{hierarchyName}'
																AND
																	(
																		ah1.account_number IN ({accountsInBase})
																		OR
																		(
																			ah1.account_number LIKE '[0-9]%'
																			AND
																			ah1.account_number NOT IN ({accountsNotInBase})
																		)
																	)
															ORDER BY
																ah1.id ASC;"
															
											
											hierarchyName = hierarchyName.Replace("-","_")
											
											Dim AccountsDt As DataTable = BRApi.Database.ExecuteSql(dbcon,selectQuery,False)
											
											If AccountsDt IsNot Nothing AndAlso AccountsDt.Rows.Count > 0 Then
												
												Dim accountName As String
												Dim accountDescription As String
												Dim accountParentName As String
												
												'Loop through all the members to insert them
												For Each accountRow As DataRow In accountsDt.Rows
													
													'Get info to create members
													accountName = accountRow("account_number").ToString.Replace("-","_").Replace("/","_")
													accountDescription = accountRow("description").ToString
													accountParentName = IIf(accountRow("father_id") = "0", "Root", accountRow("father_account_number").ToString).Replace("-","_").Replace("/","_")
													
													'Get real parent taking extensibility into account
													accountParentName = Me.GetRealBaseParent(si, dimensionTypeId, accountParentName)
													
													'If member is not already created, create it
													If Not accountDict.ContainsKey(accountName) Then
														'Final check in case it's an orfan
														Dim mbrInfo As Member = BRApi.Finance.Members.GetMember(si, dimTypeId.Account, accountName)
														If mbrInfo IsNot Nothing AndAlso mbrInfo.MemberId <> -1 Then Continue For
														
														Me.AddAccountMember(si, dimensionTypeId, "Accounts_Base", accountName, accountDescription, accountParentName)
														
													End If
													
												Next
												
												'Loop through all the members to remove relationships
												For Each accountRow As DataRow In accountsDt.Rows
													
													'Get member info
													accountName = accountRow("account_number").ToString.Replace("-","_").Replace("/","_")
													
													'Remove relationship
													Me.RemoveMemberRelationships(si, dimensionTypeId, "Accounts_Base", accountName)
													
												Next
												
'												'Loop through all the members to update the relationships
												For Each accountRow As DataRow In accountsDt.Rows
													
													'Get member info
													accountName = accountRow("account_number").ToString.Replace("-","_").Replace("/","_")
													accountDescription = accountRow("description").ToString
													accountParentName = IIf(accountRow("father_id") = "0", "Root", accountRow("father_account_number").ToString).Replace("-","_").Replace("/","_")
													
													'Get real parent taking extensibility into account
													accountParentName = Me.GetRealBaseParent(si, dimensionTypeId, accountParentName)
													
													'Update relationship
													Me.AddMemberRelationship(si, dimensionTypeId, "Accounts_Base", accountName, accountDescription, accountParentName)
													
												Next
												
											End If
											
										End If
										
									Next
									
								End If
							
							End Using
							
						#End Region
						
						#Region "Test"
							
						Else If ParamDimensionType = "Test" Then
						
							Me.RemoveMemberRelationships(si, dimtypeid.Account, "ALSEA_ACCOUNTS", "AL_MEXICO")
							
						End If
						
						#End Region
						
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
		
		#Region "General Helpers"
		
		#Region "Member Lookup"

		Public Function CreateMemberLookupUsingFilter(ByVal si As SessionInfo, ByVal dimensionName As String, ByVal memberFilter As String) As Dictionary(Of String, MemberInfo)			
			Try
				'Define the dictionary that will act as the lookup (Note, the last part of the declaration makes the look case insensitive)
				Dim memLookup As New Dictionary(Of String, MemberInfo)(StringComparer.InvariantCultureIgnoreCase)
				
				'Execute the filter and check the result
				Dim memList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, dimensionName, memberFilter, True)
				If Not memList Is Nothing Then
					'Loop over the members and add them to the lookup dictionary (Keyed by name)
					For Each memInfo As MemberInfo In memList
						memLookup.Add(memInfo.Member.Name, memInfo)		
					Next											
				End If									
					
				'Add the bypass/unmapped members (Not a real member, but we do not to add / suspense items mapped to bypass)
				memLookup.Add(StageConstants.TransformationGeneral.BypassRow, Nothing)
				memLookup.Add(StageConstants.TransformationGeneral.DimUnmapped, Nothing)
				
				Return memLookup
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#End Region
		
		#Region "Add Entity Member"
		
		Public Function AddEntityMember(ByVal si As SessionInfo, ByVal dimTypeId As Integer, ByVal dimensionName As String, ByVal memberName As String, ByVal memberDescription As String, ByVal parentName As String) As MemberInfo
			Try
				
				Dim newMemInfo As MemberInfo = Nothing
				
				'Get the dimension parent member which will give us the dimension / relationship info need to create the member
				Dim parentMem As Member = BRApi.Finance.Members.ReadMemberNoCache(si, dimTypeId, parentName)
				
				If Not parentMem Is Nothing Then										
					'Control Switches
					Dim saveMember As Boolean = True 
					Dim saveProps As Boolean = True
					Dim saveDescs As Boolean = False
					Dim saveRelationship As Boolean = True
					Dim isNewMember As Boolean = True
					
					' dimension
					Dim memDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.ReadDimNoCache(si, dimensionName)
					
					'Member
					Dim dimensionPk As New DimPk(dimTypeId, memDim.DimPk.DimId)
					Dim newMemPk As New MemberPk(dimTypeId, DimConstants.Unknown)
					Dim newMem As New Member(newMemPk,memberName,memberDescription,memDim.DimPk.DimId)
					
					'Other parameters that we are not using but must be supplied
					Dim varProps As New VaryingMemberProperties(newMemPk.DimTypeId, newMemPk.MemberId, DimConstants.Unknown)
					Dim altDescList As List(Of MemberDescription) = Nothing
					
					newMemInfo = New MemberInfo(newMem, varProps, Nothing, memDim, DimConstants.Unknown)
					
					'Modify some properties
					Dim entityProps As EntityVMProperties = newMemInfo.GetEntityProperties()
					entityProps.Currency.SetStoredValue(Currency.EUR.Id)
					
					'Get Entity's annual comparability for each year
					Dim annualComparabilityDt As DataTable = Me.GetMemberAnnualComparability(si, memberName)
					
					'Check if they exist, if not, we assume it's comparable
					If annualComparabilityDt.Rows.Count() > 0 Then
						
						For Each annualComparabilityRow As DataRow In annualComparabilityDt.Rows()
							
							Dim comparativeString As String = annualComparabilityRow("annualcomparability")
							
							Dim textTimeId = BRApi.Finance.Members.GetMemberId(si, DimType.Time.Id, annualComparabilityRow("year"))
						
							entityProps.Text1.SetStoredValue(-1, textTimeId, comparativeString)
							entityProps.Text2.SetStoredValue(-1, textTimeId, comparativeString)
							
						Next
						
					Else
						
						entityProps.Text1.SetStoredValue(-1, -1, "Comparables")
						entityProps.Text2.SetStoredValue(-1, -1, "Comparables")
						
					End If
					
					BRApi.Finance.MemberAdmin.SaveMemberInfo(si, newMemInfo, saveMember, saveProps, saveDescs, isNewMember)
									
					'Relationship
					Dim relPk As New RelationshipPk(dimTypeId, parentMem.MemberId, BRApi.Finance.Members.ReadMemberNoCache(si, dimTypeId, membername).MemberId)					
					Dim rel As New Relationship(relPk, memDim.DimPk.DimId, RelationshipMovementType.InsertAsLastSibling, 1)
					Dim relInfo As New RelationshipInfo(rel, Nothing)
					Dim relPostionOpt As New RelationshipPositionOptions()										
					
					'Create the relationship
					BRApi.Finance.MemberAdmin.SaveRelationshipInfo(si, relInfo, relPostionOpt)	
					
				End If
				
				Return newMemInfo
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#End Region
		
		#Region "Add Account Member"
		
		Public Function AddAccountMember(ByVal si As SessionInfo, ByVal dimTypeId As Integer, ByVal dimensionName As String, ByVal memberName As String, ByVal memberDescription As String, ByVal parentName As String) As MemberInfo
			Try
				
				Dim newMemInfo As MemberInfo = Nothing
				
				'BRApi.ErrorLog.LogMessage(si, $"Entro aqui con cuenta {memberName}, padre {parentName}")
				
				'Get the dimension parent member which will give us the dimension / relationship info need to create the member
				Dim parentMem As Member = BRApi.Finance.Members.ReadMemberNoCache(si, dimTypeId, parentName)
				
				If Not parentMem Is Nothing Then										
					'Control Switches
					Dim saveMember As Boolean = True 
					Dim saveProps As Boolean = True
					Dim saveDescs As Boolean = False
					Dim saveRelationship As Boolean = True
					Dim isNewMember As Boolean = True
					
					' dimension
					Dim memDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.ReadDimNoCache(si, dimensionName)
					
					'Member
					Dim dimensionPk As New DimPk(dimTypeId, memDim.DimPk.DimId)
					Dim newMemPk As New MemberPk(dimTypeId, DimConstants.Unknown)
					Dim newMem As New Member(newMemPk, memberName, memberDescription, memDim.DimPk.DimId)
					
					'Other parameters that we are not using but must be supplied
					Dim varProps As New VaryingMemberProperties(newMemPk.DimTypeId, newMemPk.MemberId, DimConstants.Unknown)
					Dim altDescList As List(Of MemberDescription) = Nothing
					
					newMemInfo = New MemberInfo(newMem, varProps, Nothing, memDim, DimConstants.Unknown)
					
					'Modify some properties
					Dim accountProps As AccountVMProperties = newMemInfo.GetAccountProperties()
					If memberName.StartsWith("6") Then
						
						accountProps.AccountType.SetStoredValue(AccountType.Revenue.Id)
						
					Else If memberName.StartsWith("7") Then
						
						accountProps.AccountType.SetStoredValue(AccountType.Revenue.Id)
						
					Else
						
						accountProps.AccountType.SetStoredValue(AccountType.Revenue.Id)
						
					End If
					
					BRApi.Finance.MemberAdmin.SaveMemberInfo(si, newMemInfo, saveMember, saveProps, saveDescs, isNewMember)
									
					'Relationship
					Dim relPk As New RelationshipPk(dimTypeId, parentMem.MemberId, BRApi.Finance.Members.ReadMemberNoCache(si, dimTypeId, membername).MemberId)					
					Dim rel As New Relationship(relPk, memDim.DimPk.DimId, RelationshipMovementType.InsertAsLastSibling, 1)
					Dim relInfo As New RelationshipInfo(rel, Nothing)
					Dim relPostionOpt As New RelationshipPositionOptions()										
					
					'Create the relationship
					BRApi.Finance.MemberAdmin.SaveRelationshipInfo(si, relInfo, relPostionOpt)	
					
				End If
				
				Return newMemInfo
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#End Region
		
		#Region "Add Member Relationship"
		
		Public Function AddMemberRelationship(ByVal si As SessionInfo, ByVal dimTypeId As Integer, ByVal dimensionName As String, ByVal memberName As String, ByVal memberDescription As String, ByVal parentName As String) As MemberInfo
			Try
				
				'Update description first for entities
				If dimensionName = "Entities" Then
				
					'Get entity member and member info and convert to writable member to update description
					Dim member As Member = BRApi.Finance.Members.GetMember(si, dimTypeId, memberName)
					Dim newMember As New WritableMember(member)
					'Update description and save
					newMember.Description = memberDescription
					BRApi.Finance.MemberAdmin.SaveMemberInfo(si, True, newMember, False, Nothing, False, Nothing, False)
					
				End If
				
				'Get the dimension parent member which will give us the dimension / relationship info
				Dim parentMem As Member = BRApi.Finance.Members.ReadMemberNoCache(si, dimTypeId, parentName)
				
				If Not parentMem Is Nothing Then
					
					' dimension
					Dim memDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.ReadDimNoCache(si, dimensionName)
									
					'Relationship
					Dim relPk As New RelationshipPk(dimTypeId, parentMem.MemberId, BRApi.Finance.Members.ReadMemberNoCache(si, dimTypeId, memberName).MemberId)					
					Dim rel As New Relationship(relPk, memDim.DimPk.DimId, RelationshipMovementType.InsertAsLastSibling, 1)
					Dim relInfo As New RelationshipInfo(rel, Nothing)
					Dim relPostionOpt As New RelationshipPositionOptions()
					
					'Create the relationship
					BRApi.Finance.MemberAdmin.SaveRelationshipInfo(si, relInfo, relPostionOpt)
					
				End If
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#End Region
		
		#Region "Update Entity Text"
		
		Public Sub UpdateEntityText(ByVal si As SessionInfo, ByVal entityName As String, ByVal auxClosingYear As String, ByVal notUpdatingEntities As HashSet(Of String))			
			'Control Switches
			Dim saveMember As Boolean = True 
			Dim saveProps As Boolean = True
			Dim saveDescs As Boolean = False
			Dim saveRelationship As Boolean = False
			Dim isNewMember As Boolean = False
			
			'Get entity properties
			Dim entityMemberInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimType.Entity.Id, entityName, True)
			Dim entityProps As EntityVMProperties = entityMemberInfo.GetEntityProperties()

			'Get Entity's annual comparability for each year
			Dim annualComparabilityDt As DataTable = Me.GetMemberAnnualComparability(si, entityName)
			'Check if they exist, if not, we assume it's comparable
			If annualComparabilityDt.Rows.Count() > 0 Then
				
				For Each annualComparabilityRow As DataRow In annualComparabilityDt.Rows()
					
					Dim textTimeId = BRApi.Finance.Members.GetMemberId(si, DimType.Time.Id, annualComparabilityRow("year"))
					'Get comparative string for each text type
					Dim text1comparativeString As String = annualComparabilityRow("annualcomparability")
					'Text 2 comparability depends on aux table
					Dim text2comparativeString As String = If(
						auxClosingYear <> "None" AndAlso annualComparabilityRow("year") >= CInt(auxClosingYear),
						"Cerradas",
						text1comparativeString
					)
				
					entityProps.Text1.SetStoredValue(-1, textTimeId, text1comparativeString)
					If notUpdatingEntities.Contains(entityName) Then Continue For
					entityProps.Text2.SetStoredValue(-1, textTimeId, text2comparativeString)
					
				Next
				
			Else
				
				entityProps.Text1.SetStoredValue(-1, -1, "Comparables")
				entityProps.Text2.SetStoredValue(-1, -1, "Comparables")
				
			End If
			
			'Save entity properties	
			BRApi.Finance.MemberAdmin.SaveMemberInfo(si, entityMemberInfo, saveMember, saveProps, saveDescs, isNewMember)
		End Sub
		
		#End Region
		
		#Region "Remove Member Relationships"
		
		Public Sub RemoveMemberRelationships(ByVal si As SessionInfo, ByVal dimTypeId As Integer, ByVal dimensionName As String,ByVal parentName As String)
 
			Dim osMember As Member = BRApi.Finance.Members.GetMember(si, dimTypeId, parentName)'Member
			Dim dimensionPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, dimensionName)
			Dim osBaseList As list(Of Member) = BRApi.Finance.Members.GetChildren(si, dimensionPk, osMember.MemberId)
			
'			If dimensionName = "Accounts_Base" AndAlso parentName = "AL_W_OFF" Then
'				brapi.ErrorLog.LogMessage(si, parentName)
'				brapi.ErrorLog.LogMessage(si, dimensionName)
'				brapi.ErrorLog.LogMessage(si, osBaseList.Count)
'				For Each osBase As Member In osBaseList
'					brapi.ErrorLog.LogMessage(si, osBase.Name)
'				Next
'			End If
			
			For Each baseMember As Member In osBaseList
				
				Dim PId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId, osMember.Name)
				Dim myId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId, baseMember.Name)
				Dim relationshipPks As New List(Of RelationshipPk)
				Dim RelPk As New RelationshipPk(dimensionPk.DimTypeId, PId, myId)
				relationshipPks.Add(RelPk)
				 
				BRApi.Finance.MemberAdmin.RemoveRelationships(si, dimensionPk, relationshipPks, True)
			 
			Next
		
		End Sub
		
		#End Region

		#Region "Get Member Annual Comparability"
		
		Public Function GetMemberAnnualComparability(ByVal si As SessionInfo, ByVal memberName As String) As DataTable
		
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
				Dim selectQuery As String = $"	WITH RankedData AS (
												    SELECT 
												        YEAR(date) AS year,
												        desc_annualcomparability AS annualcomparability,
												        ROW_NUMBER() OVER (PARTITION BY YEAR(date) ORDER BY date DESC) AS rn
												    FROM 
												        XFC_ComparativeCEBES
												    WHERE 
												        cebe = '{memberName}'
												)
												SELECT 
												    year,
												    annualcomparability
												FROM 
												    RankedData
												WHERE 
												    rn = 1"
				
				Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
			
				Return dt
			
			End Using
			
		End Function
		
		#End Region
		
		#Region "Get real base parent"
		
		Public Function GetRealBaseParent(ByVal si As SessionInfo, ByVal dimensionTypeId As Integer, ByVal accountParentName As String)
			'Get if parent has child on non extended dimension
			Dim parentChildsAccountDict As Dictionary(Of String, MemberInfo) = Me.CreateMemberLookupUsingFilter(si, "Accounts", $"A#{accountParentName}.Children")
			'Clean dictionary
			parentChildsAccountDict.Remove("~")
			parentChildsAccountDict.Remove("(Bypass)")
			If parentChildsAccountDict IsNot Nothing AndAlso parentChildsAccountDict.Count > 0 Then
				'Name dummy account
				Dim parentDummyAccountName As String = $"{accountParentName}_Extended"
				
				'Create dummy account if not created yet, else check if it is not already a children of the parent to create the relationship
				Dim parentDummyAccountMemberdict As Dictionary(Of String, MemberInfo) = Me.CreateMemberLookupUsingFilter(si, "Accounts", $"A#{parentDummyAccountName}")
				'Clean dictionary
				parentDummyAccountMemberdict.Remove("~")
				parentDummyAccountMemberdict.Remove("(Bypass)")
				If parentDummyAccountMemberdict.Count < 1 Then
					Me.AddAccountMember(si, dimensionTypeId, "Accounts", parentDummyAccountName, parentDummyAccountName, accountParentName)
				Else If Not parentChildsAccountDict.ContainsKey(parentDummyAccountName)
					Me.AddMemberRelationship(si, dimensionTypeId, "Accounts", parentDummyAccountName, parentDummyAccountName, accountParentName)
				End If
				
				'Update accountParentName to the dummy account to preserve extensibility functionality
				accountParentName = parentDummyAccountName
			End If
			
			Return accountParentName
		End Function
		
		#End Region
		
		#End Region
		
	End Class
End Namespace