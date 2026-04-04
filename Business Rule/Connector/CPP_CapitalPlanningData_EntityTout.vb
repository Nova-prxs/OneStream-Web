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

Namespace OneStream.BusinessRule.Connector.CPP_CapitalPlanningData_EntityTout
	Public Class MainClass
		'-------------------------------------------------------------------------------------------------------------------
		'ENTITY TRANSFER-OUT CONNECTOR
		'
		'Exclude plan (Transfer-In) data that must be Loaded and Process by the workflows that own the entity being transferred
		'
		'Usage Requirements:
		'
		'1) Assumes worflows have assigned entities
		'2) Intended to be used for distributed Capital Plan Register that require cross Workflow entity transfers
		'3) This Connector should be assigned to the Standard Capital Plan import Workflow (The Workflow that initiates the transfer)
		'
		'--------------------------------------------------------------------------------------------------------------------
		'CAPITAL PLANNING CONNECTOR (DataSource Configuration Requirements)
		'
		'Required DataSource Field Mappings (Enables preconfigured Drill-Back to Function)
		'
		'1) WFProfileName:	Must assign to LABEL source dimension in DataSource
		'2) WFScenarioName:	Must assign to SOURCEID source dimension in DataSource
		'
		'--------------------------------------------------------------------------------------------------------------------
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As Object
			Try

				Select Case args.ActionType
					Case Is = ConnectorActionTypes.GetFieldList
						'Get the list of field names in the source table by selecting one row
						Dim appCon As String = Me.GetCurrentAppConnectionString(si, globals, api, args)
						Dim sql As String = Me.GetFieldListSQL(si, globals, api, args)
						Return api.Parser.GetFieldNameListForSQLQuery(si, DbProviderType.SqlServer, appCon, False, sql, False)

					Case Is = ConnectorActionTypes.GetData
						'Validate required DataSource field assignments
						Dim dataSource As ParserLayoutInfo = Me.GetDataSource(si, args)
						Me.ValidateDataSourceFieldAssignment(si, args, StageTableFields.StageSourceData.DimLabel,"WFProfileName", dataSource)
						Me.ValidateDataSourceFieldAssignment(si, args, StageTableFields.StageSourceData.DimSourceID,"WFScenarioName", dataSource)

						'Validate the we are not attempting import/process data from the CENTRAL REGISTER MGMT Workflow profile
						Me.ValidateNoImportFromCentralRegisterProfile(si, globals, api, args)

						'Load the Capital Plan Data
						Dim appCon As String = Me.GetCurrentAppConnectionString(si, globals, api, args)
						Dim sql As String = Me.GetDataSelectSQL(si, globals, api, args)
						api.Parser.ProcessSQLQuery(si, DbProviderType.SqlServer, appcon, False, sql, False, api.ProcessInfo)

					Case Is = ConnectorActionTypes.GetDrillBackTypes
						'Return the list of Drill Types (Options) to present to the end user
						Return  Me.GetDrillBackTypeList(si, globals, api, args)

					Case Is = ConnectorActionTypes.GetDrillBack
						'Process the specific Drill-Back type
						Dim appCon As String = Me.GetCurrentAppConnectionString(si, globals, api, args)
						Return Me.GetDrillBack(si, globals, api, args, args.DrillBackType.DisplayType, appcon)

				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#Region "Constants and Enumerations"

	Public Enum WFProfileCriteriaTypes
		CentralImport = 0
		SameProfile = 1
		ProfileText1Map = 2
	End Enum

#End Region

#Region "Connection and Field List Methods"

		Private Function GetCurrentAppConnectionString(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As String
			Try

				'Get the connection string for the current application
				Dim appCon As String = String.Empty
				Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					appcon = dbConnApp.ConnectionString
				End Using
				Return appCon

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function GetFieldListSQL(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As String
			Try

				'Get the SQL string for the valid fields in the Plan table
				Dim sql As New Text.StringBuilder
				sql.Append("Select Top 1 WFProfileName, WFScenarioName, WFTimeName, Period, Entity, ActivityType, ImpactType, Account, Flow, IC, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, Amount As Amt From XFW_CPP_Plan")
				Return sql.ToString

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "Source Data Methods"

		Private Function GetDataSelectSQL(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As String
			Try
				'Retrieve the Capital Plan scenario and time to load from stored Settings
				Dim cppScenario As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "StoredPlanConnectorScenario_CPPT")
				Dim cppTime As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "StoredPlanConnectorTime_CPPT")

				'Replace WF value with actual workflow scenario and time
				If cppScenario.XFEqualsIgnoreCase("WF") Then cppScenario = ScenarioDimHelper.GetNameFromId(si, api.WorkflowUnitPk.ScenarioKey)
				If cppTime.XFEqualsIgnoreCase("WF") Then cppTime = BRApi.Finance.Time.GetNameFromId(si,api.WorkflowUnitPk.TimeKey)

				'Execute a summary query against the Plan Table
				Dim sql As New Text.StringBuilder

				sql.Append("Select WFProfileName, WFScenarioName, WFTimeName, Period, Entity, ActivityType, ImpactType, Account, Flow, IC, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, Sum(Amount) As Amt ")
				sql.Append("From XFW_CPP_Plan ")
				sql.Append("Where ")
				sql.Append("(WFScenarioName = '" & SqlStringHelper.EscapeSqlString(cppScenario) &  "') ")
				sql.Append("And (WFTimeName = '" & SqlStringHelper.EscapeSqlString(cppTime) &  "') ")
				sql.Append(Me.GetWFProfileCriteria(si, api))
				sql.Append(Me.GetAssignedEntityCriteria(si, api))
				sql.Append("Group By WFProfileName, WFScenarioName, WFTimeName, Period, Entity, ActivityType, ImpactType, Account, Flow, IC, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8 ")

				Return sql.ToString

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Sub ValidateNoImportFromCentralRegisterProfile(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs)
			Try
				'Retrieve the name of the Workflow profile used for CENTRAL REGISTER MGMT
				Dim registerProfile As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "StoredPlanRegisterProfile_CPPT")

				'Get the stored WFProfile Criteria Value so that we can determine how the Workflow profile criteria is derived
				Dim loadingProfile As String = String.Empty
				Dim wfProfileCriteria As Integer = CType(BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "StoredPlanConnectorProfile_CPPT"), Integer)
				Dim wfProfileCriteriaType As WFProfileCriteriaTypes = CType(wfProfileCriteria, WFProfileCriteriaTypes)
				Select Case wfProfileCriteriaType
					Case Is = wfProfileCriteriaType.CentralImport
						loadingProfile = api.WorkflowProfile.Name

					Case Is = wfProfileCriteriaType.SameProfile
						loadingProfile = api.WorkflowProfile.Name

					Case Is  = wfProfileCriteriaType.ProfileText1Map
						loadingProfile = api.WorkflowProfile.GetAttributeValue(api.ScenarioTypeID, SharedConstants.WorkflowProfileAttributeIndexes.Text1)
				End Select

				'Make sure that the user is not trying to load data from the PlanRegisterProfile (Central Register Management Profile)
				If registerProfile.XFEqualsIgnoreCase(loadingProfile) Then
					'Throw, Not allowed to load cube from the central register management profile
					Throw New Exception("Capital Planning: Cannot load data into cube from the Central Register Mangement Workflow Profile '" & registerProfile & "'." )
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub

#End Region

#Region "Drill Back Methods"

		Private Function GetDrillBackTypeList(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As List(Of DrillBackTypeInfo)
			Try
				'Create the SQL Statement
				Dim drillTypes As New List(Of DrillBackTypeInfo)

				If args.DrillCode.XFEqualsIgnoreCase(StageConstants.TransformationGeneral.DrillCodeDefaultValue) Then
					drillTypes.Add(New DrillBackTypeInfo(ConnectorDrillBackDisplayTypes.DataGrid, New NameAndDesc("Plan Detail","Plan Detail")))
					drillTypes.Add(New DrillBackTypeInfo(ConnectorDrillBackDisplayTypes.DataGrid, New NameAndDesc("Plan Trend (Forward 12)","Plan Trend (Forward 12)")))

				Else If args.DrillCode.XFEqualsIgnoreCase("RegisterIDDetail") Then
					drillTypes.Add(New DrillBackTypeInfo(ConnectorDrillBackDisplayTypes.DataGrid, New NameAndDesc("All Plan Activity","All Plan Activity")))
					drillTypes.Add(New DrillBackTypeInfo(ConnectorDrillBackDisplayTypes.DataGrid, New NameAndDesc("Calculation Plan (Calculation Detail)","Calculation Plan (Calculation Detail)")))
				End If

				Return drillTypes

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function GetDrillBack(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs, ByVal drillBackType As ConnectorDrillBackDisplayTypes, ByVal appcon As String) As DrillBackResultInfo
			Try
				Select Case drillBackType

					Case Is = ConnectorDrillBackDisplayTypes.DataGrid

						Dim drillBackInfo As New DrillBackResultInfo

						If args.DrillBackType.NameAndDescription.Name.XFEqualsIgnoreCase("Plan Detail") Then
							'Level 1: Plan Detail
							Dim drillBackSQL As String = GetDrillBackSQL_PlanDetail_L1(si, globals, api, args)
							drillBackInfo.DisplayType = ConnectorDrillBackDisplayTypes.DataGrid
							drillBackInfo.DataTable = api.Parser.GetXFDataTableForSQLQuery(si, DbProviderType.sqlserver, appcon, False, drillBackSQL, False, args.PageSize, args.PageNumber)
							Return drillBackInfo

						Else If args.DrillBackType.NameAndDescription.Name.XFEqualsIgnoreCase("Plan Trend (Forward 12)") Then
							'Level 1: Plan Trend
							Dim drillBackSQL As String = GetDrillBackSQL_PlanTrend_L1(si, globals, api, args)
							drillBackInfo.DisplayType = ConnectorDrillBackDisplayTypes.DataGrid
							drillBackInfo.DataTable = api.Parser.GetXFDataTableForSQLQuery(si, DbProviderType.sqlserver, appcon, False, drillBackSQL, False, args.PageSize, args.PageNumber)
							Return drillBackInfo

						Else If args.DrillBackType.NameAndDescription.Name.XFEqualsIgnoreCase("All Plan Activity") Then
							'Level 2: All Plan Activity
							Dim drillBackSQL As String = GetDrillBackSQL_AllPlanActivity_L2(si, globals, api, args)
							drillBackInfo.DisplayType = ConnectorDrillBackDisplayTypes.DataGrid
							drillBackInfo.DataTable = api.Parser.GetXFDataTableForSQLQuery(si, DbProviderType.sqlserver, appcon, False, drillBackSQL, False, args.PageSize, args.PageNumber)
							Return drillBackInfo

						Else If args.DrillBackType.NameAndDescription.Name.XFEqualsIgnoreCase("Calculation Plan (Calculation Detail)") Then
							'Level 2: Calculation Plan Line
							Dim drillBackSQL As String = GetDrillBackSQL_CalculationPlanLine_L2(si, globals, api, args)
							drillBackInfo.DisplayType = ConnectorDrillBackDisplayTypes.DataGrid
							drillBackInfo.DataTable = api.Parser.GetXFDataTableForSQLQuery(si, DbProviderType.sqlserver, appcon, False, drillBackSQL, False, args.PageSize, args.PageNumber)
							Return drillBackInfo

						Else
							Return Nothing
						End If

					Case Else
						Return Nothing
				End Select

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function GetDrillBackSQL_PlanDetail_L1(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As String
			Try
				'Get the values for the source row that we are drilling back to
				Dim dataSource As ParserLayoutInfo = Me.GetDataSource(si, args)
				Dim sourceValues As Dictionary(Of String, Object) = api.Parser.GetFieldValuesForSourceDataRow(si, args.RowID)
				Dim activeDims As Dictionary(Of String, String) = Me.GetActiveDimensions(si)
				If (Not sourceValues Is Nothing) And (sourceValues.Count > 0) Then

					Dim statement As New Text.StringBuilder
					Dim selectClause As New Text.StringBuilder
					Dim fromClause As New Text.StringBuilder
					Dim whereClause As New Text.StringBuilder
					Dim orderByClause As New Text.StringBuilder

					'Create the SQL Statement
					selectClause.Append("SELECT ")
					selectClause.Append("Amount, Period, RegisterID, RegisterDescription, ClassID, ClassDescription, ClassItemDescription, ")
					selectClause.Append("Code1, Code2, Code3, Code4, ImpactType, ActivityType, Status, PlanType, ")
					selectClause.Append("CalcTime, CalcBy, FKDetailID, 'RegisterIDDetail' As " & StageConstants.TransformationGeneral.DrillCodeFieldName & " ")

					fromClause.Append(" FROM XFW_CPP_Plan ")

					'Build the Where clause for the detail records
					whereClause.Append("WHERE ")
					whereClause.Append("(")
					whereClause.Append(Me.GetDimensionCriteria(si, args, StageTableFields.StageSourceData.DimLabel, sourceValues, dataSource))
					whereClause.Append(") ")

					whereClause.Append("And (")
					whereClause.Append(Me.GetDimensionCriteria(si, args, StageTableFields.StageSourceData.DimSourceID, sourceValues, dataSource))
					whereClause.Append(") ")

					whereClause.Append("And (")
					whereClause.Append(Me.GetDimensionCriteria(si, args, StageTableFields.StageSourceData.DimTime, sourceValues, dataSource))
					whereClause.Append(") ")

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.Entity) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.Entity, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.Account) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.Account, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.Flow) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.Flow, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.IC) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.IC, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD1) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD1, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD2) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD2, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD3) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD3, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD4) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD4, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD5) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD5, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD6) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD6, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD7) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD7, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD8) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD8, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					orderByClause.Append("ORDER BY ")
					orderByClause.Append("ActivityType, Account")

					'Create the full SQL Statement
					statement.Append(selectClause.ToString)
					statement.Append(fromClause.ToString)
					If args.ClientFilterRequest.length > 0 Then
						statement.Append(whereClause.ToString)
						statement.Append(" And ")
						statement.Append(args.ClientFilterRequest)
					Else
						statement.Append(whereClause.ToString)
					End If
					If args.ClientSortRequest.Length > 0 Then
						statement.Append(args.ClientSortRequest)
					Else
						statement.Append(orderByClause.ToString)
					End If

					'ErrorHandler.LogMessage(si, statement.ToString)

					Return statement.ToString
				Else
					Return String.Empty
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function GetDrillBackSQL_PlanTrend_L1(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As String
			Try
				'Get the values for the source row that we are drilling back to
				Dim dataSource As ParserLayoutInfo = Me.GetDataSource(si, args)
				Dim sourceValues As Dictionary(Of String, Object) = api.Parser.GetFieldValuesForSourceDataRow(si, args.RowID)
				Dim activeDims As Dictionary(Of String, String) = Me.GetActiveDimensions(si)
				If (Not sourceValues Is Nothing) And (sourceValues.Count > 0) Then

					Dim statement As New Text.StringBuilder
					Dim selectClause As New Text.StringBuilder
					Dim fromClause As New Text.StringBuilder
					Dim whereClause As New Text.StringBuilder
					Dim groupByClause As New Text.StringBuilder
					Dim orderByClause As New Text.StringBuilder

					Dim startPeriod As Integer = CType(sourceValues.Item(StageTableFields.StageSourceData.DimTime), Integer)

					'Create the SQL Statement
					selectClause.Append("Select ")
					selectClause.Append("Status, ")
					selectClause.Append("RegisterID, ")
					selectClause.Append("RegisterDescription, ")
					selectClause.Append("ClassDescription, ")
					selectClause.Append("ClassItemDescription, ")
					selectClause.Append("ActivityType, ")
					selectClause.Append("Sum((Case When Period = " & startPeriod.ToString & " Then Amount Else 0 End)) As Per" & startPeriod.ToString & ", ")
					selectClause.Append("Sum((Case When Period = (" & startPeriod.ToString & "+1) Then Amount Else 0 End)) As Per" & (startPeriod+1).ToString & ", ")
					selectClause.Append("Sum((Case When Period = (" & startPeriod.ToString & "+2) Then Amount Else 0 End)) As Per" & (startPeriod+2).ToString & ", ")
					selectClause.Append("Sum((Case When Period = (" & startPeriod.ToString & "+3) Then Amount Else 0 End)) As Per" & (startPeriod+3).ToString & ", ")
					selectClause.Append("Sum((Case When Period = (" & startPeriod.ToString & "+4) Then Amount Else 0 End)) As Per" & (startPeriod+4).ToString & ", ")
					selectClause.Append("Sum((Case When Period = (" & startPeriod.ToString & "+5) Then Amount Else 0 End)) As Per" & (startPeriod+5).ToString & ", ")
					selectClause.Append("Sum((Case When Period = (" & startPeriod.ToString & "+6) Then Amount Else 0 End)) As Per" & (startPeriod+6).ToString & ", ")
					selectClause.Append("Sum((Case When Period = (" & startPeriod.ToString & "+7) Then Amount Else 0 End)) As Per" & (startPeriod+7).ToString & ", ")
					selectClause.Append("Sum((Case When Period = (" & startPeriod.ToString & "+8) Then Amount Else 0 End)) As Per" & (startPeriod+8).ToString & ", ")
					selectClause.Append("Sum((Case When Period = (" & startPeriod.ToString & "+9) Then Amount Else 0 End)) As Per" & (startPeriod+9).ToString & ", ")
					selectClause.Append("Sum((Case When Period = (" & startPeriod.ToString & "+10) Then Amount Else 0 End)) As Per" & (startPeriod+10).ToString & ", ")
					selectClause.Append("Sum((Case When Period = (" & startPeriod.ToString & "+11) Then Amount Else 0 End)) As Per" & (startPeriod+11).ToString & " ")

					fromClause.Append(" FROM XFW_CPP_Plan ")

					'Build the Where clause for the detail records
					whereClause.Append("WHERE ")
					whereClause.Append("(")
					whereClause.Append(Me.GetDimensionCriteria(si, args, StageTableFields.StageSourceData.DimLabel, sourceValues, dataSource))
					whereClause.Append(") ")

					whereClause.Append("And (")
					whereClause.Append(Me.GetDimensionCriteria(si, args, StageTableFields.StageSourceData.DimSourceID, sourceValues, dataSource))
					whereClause.Append(") ")

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.Entity) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.Entity, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.Account) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.Account, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.Flow) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.Flow, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.IC) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.IC, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD1) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD1, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD2) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD2, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD3) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD3, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD4) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD4, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD5) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD5, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD6) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD6, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD7) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD7, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.UD8) Then
						whereClause.Append("And (")
						whereClause.Append(Me.GetDimensionCriteria(si, args, StageConstants.MasterDimensionNames.UD8, sourceValues, dataSource))
						whereClause.Append(") ")
					End If

					groupByClause.Append("Group By ")
					groupByClause.Append("Status, ")
					groupByClause.Append("RegisterID, ")
					groupByClause.Append("RegisterDescription, ")
					groupByClause.Append("ClassDescription, ")
					groupByClause.Append("ClassItemDescription, ")
					groupByClause.Append("ActivityType ")

					orderByClause.Append("ORDER BY ")
					orderByClause.Append("RegisterID, ActivityType")

					'Create the full SQL Statement
					statement.Append(selectClause.ToString)
					statement.Append(fromClause.ToString)
					If args.ClientFilterRequest.length > 0 Then
						statement.Append(whereClause.ToString)
						statement.Append(" And ")
						statement.Append(args.ClientFilterRequest)
					Else
						statement.Append(whereClause.ToString)
					End If
					statement.Append(groupByClause.ToString)
					If args.ClientSortRequest.Length > 0 Then
						statement.Append(args.ClientSortRequest)
					Else
						statement.Append(orderByClause.ToString)
					End If

					'ErrorHandler.LogMessage(si, statement.ToString)

					Return statement.ToString
				Else
					Return String.Empty
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function GetDrillBackSQL_AllPlanActivity_L2(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As String
			Try
				'Get the values for the source row that we are drilling back to
				Dim sourceValues As Dictionary(Of String, Object) = api.Parser.GetFieldValuesForSourceDataRow(si, args.RowID)
				Dim activeDims As Dictionary(Of String, String) = Me.GetActiveDimensions(si)
				If (Not sourceValues Is Nothing) And (sourceValues.Count > 0) Then

					Dim statement As New Text.StringBuilder
					Dim selectClause As New Text.StringBuilder
					Dim fromClause As New Text.StringBuilder
					Dim whereClause As New Text.StringBuilder
					Dim orderByClause As New Text.StringBuilder

					'Create the SQL Statement
					selectClause.Append("SELECT ")
					selectClause.Append("Amount,Period,RegisterID,RegisterDescription,ClassID,ClassDescription,ClassItemDescription,ImpactType,ActivityType,Status,PlanType,CalcTime,CalcBy,FKDetailID, 'CalculationPlanLine' As " & StageConstants.TransformationGeneral.DrillCodeFieldName & " ")

					fromClause.Append(" FROM XFW_CPP_Plan ")

					'Build the Where clause for the detail records
					whereClause.Append("WHERE ")
					whereClause.Append("(")
					whereClause.Append("WFProfileName = '" & SqlStringHelper.EscapeSqlString(sourceValues.Item(StageTableFields.StageSourceData.DimLabel).ToString) & "' ")
					whereClause.Append(") ")

					whereClause.Append("And (")
					whereClause.Append("WFScenarioName = '" & SqlStringHelper.EscapeSqlString(sourceValues.Item(StageTableFields.StageSourceData.DimSourceID).ToString) & "' ")
					whereClause.Append(") ")

					whereClause.Append("And (")
					whereClause.Append("WFTimeName = " & SqlStringHelper.EscapeSqlString(sourceValues.Item(StageTableFields.StageSourceData.DimTime).ToString) & " ")
					whereClause.Append(") ")

					If activeDims.ContainsKey(StageConstants.MasterDimensionNames.Entity) Then
						whereClause.Append("And (")
						whereClause.Append("RegisterID = '" & SqlStringHelper.EscapeSqlString(args.GetSourceRowValue("RegisterID").ToString) & "' ")
						whereClause.Append(") ")
					End If

					orderByClause.Append("ORDER BY ")
					orderByClause.Append("ActivityType, Account")

					'Create the full SQL Statement
					statement.Append(selectClause.ToString)
					statement.Append(fromClause.ToString)
					If args.ClientFilterRequest.length > 0 Then
						statement.Append(whereClause.ToString)
						statement.Append(" And ")
						statement.Append(args.ClientFilterRequest)
					Else
						statement.Append(whereClause.ToString)
					End If
					If args.ClientSortRequest.Length > 0 Then
						statement.Append(args.ClientSortRequest)
					Else
						statement.Append(orderByClause.ToString)
					End If

					'ErrorHandler.LogMessage(si, statement.ToString)

					Return statement.ToString
				Else
					Return String.Empty
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function GetDrillBackSQL_CalculationPlanLine_L2(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As String
			Try
				'Get the values for the source row that we are drilling back to
				Dim sourceValues As Dictionary(Of String, Object) = api.Parser.GetFieldValuesForSourceDataRow(si, args.RowID)
				If (Not sourceValues Is Nothing) And (sourceValues.Count > 0) Then

					Dim statement As New Text.StringBuilder
					Dim selectClause As New Text.StringBuilder
					Dim fromClause As New Text.StringBuilder
					Dim whereClause As New Text.StringBuilder
					Dim orderByClause As New Text.StringBuilder

					'Create the SQL Statement
					selectClause.Append("SELECT FKClassID,Description,WeightOrCount,PeriodDivisor,PeriodFilter,Condition,EntityOverride,ICOverride,UD1Override,UD2Override,UD3Override,UD4Override,UD5Override,UD6Override,UD7Override,UD8Override ")

					'Evaluate the Calc Plan (Exception Calc Plan vs Standard Calc Plan)
					Dim status As String = args.GetSourceRowValue("Status").ToString
					If status.XFEqualsIgnoreCase("Exception") Then
						'Exception Calc Plans are stored in the RegisterDetail table
						fromClause.Append(" FROM XFW_CPP_RegisterDetail ")

						'Build the Where clause for the detail records
						whereClause.Append("WHERE ")
						whereClause.Append("(")
						whereClause.Append("RegisterDetailID = '" & SqlStringHelper.EscapeSqlString(args.GetSourceRowValue("FKDetailID").ToString) & "'")
						whereClause.Append(") ")
					Else
						'Standard Calc Plans are stored in the RegisterDetail table
						fromClause.Append(" FROM XFW_CPP_CalcPlanDetail ")

						'Build the Where clause for the detail records
						whereClause.Append("WHERE ")
						whereClause.Append("(")
						whereClause.Append("CalcPlanDetailID = '" & SqlStringHelper.EscapeSqlString(args.GetSourceRowValue("FKDetailID").ToString) & "'")
						whereClause.Append(") ")
					End If

					'Create the full SQL Statement
					statement.Append(selectClause.ToString)
					statement.Append(fromClause.ToString)
					If args.ClientFilterRequest.length > 0 Then
						statement.Append(whereClause.ToString)
						statement.Append(" And ")
						statement.Append(args.ClientFilterRequest)
					Else
						statement.Append(whereClause.ToString)
					End If
					If args.ClientSortRequest.Length > 0 Then
						statement.Append(args.ClientSortRequest)
					Else
						statement.Append(orderByClause.ToString)
					End If

					'ErrorHandler.LogMessage(si, statement.ToString)

					Return statement.ToString
				Else
					Return String.Empty
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "General Connector Helper Methods"

		Private Function GetActiveDimensions(ByVal si As SessionInfo) As Dictionary(Of String, String)
			Try
				'Get the list of active dimensions for the Cube / ScenarioType combination
				Dim activeDims As New Dictionary(Of String, String)(StringComparison.InvariantCultureIgnoreCase)

				Dim cubeName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim scenarioTypeID As Integer = BRApi.Workflow.General.GetScenarioTypeId(si, si.WorkflowClusterPk)
				Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim activeDimList As List(Of IntegrationMapInfo) = IntegrationMap.GetDimensions(dbConnApp, cubeName, scenarioTypeID, IntegrationMapItemStateTypes.Active)
					For Each activeDim As IntegrationMapInfo In activeDimList
						activeDims.Add(activeDim.DimensionStageName, activeDim.DimensionFinanceName)
					Next
				End Using

				Return activeDims

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function GetDataSource(ByVal si As SessionInfo, ByVal args As ConnectorArgs) As ParserLayoutInfo
			Try
				'Get the DataSource that is associated with the Workflow that is drilling back
				Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
					Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Return StageMetadataWcf.GetParserLayout(dbConnFW, dbConnApp, args.DataSourceName, True)
					End Using
				End Using

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function GetDimensionCriteria(ByVal si As SessionInfo, ByVal args As ConnectorArgs, ByVal masterDimName As String, ByVal sourceValues As Dictionary(Of String, Object), ByVal dataSource As ParserLayoutInfo) As String
			Try

				Dim criteria As String = String.Empty
				If Not dataSource Is Nothing Then
					'Build a list of parser dimensions that match the targetDimName
					Dim matchingDims As New List(Of ParserDimensionInfo)
					For Each pDim As ParserDimensionInfo In dataSource.Dimensions
						If pDim.Name.XFEqualsIgnoreCase(masterDimName) Then
							matchingDims.Add(pDim)
						End If
					Next

					'Evaluate the matching dimensions and build the criterial statement
					If matchingDims.Count = 1 Then
						Dim matchingDim As ParserDimensionInfo = matchingDims(0)
						Dim sourceField As String = matchingDim.GetAttributeValue(StageConstants.DimensionAttributeIndexes.ConnectorSourceFieldName)
						criteria = sourceField & " = '" & SqlStringHelper.EscapeSqlString(sourceValues(masterDimName)) & "' "
					Else
						'Throw and error, we cannot automatically determine drillback with Concatenated Source dimensions
						Throw New Exception("Capital Planning: Dimension '" & masterDimName & "' is concatenated from multiple source fields standard drill back not supported (Connector must be customized)")
					End If
				End If
				Return criteria

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function GetWFProfileCriteria(ByVal si As SessionInfo, ByVal api As Transformer) As String
			Try

				'Get the stored WFProfile Criteria Value
				Dim wfProfileCriteria As Integer = CType(BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "StoredPlanConnectorProfile_CPPT"), Integer)
				Dim wfProfileCriteriaType As WFProfileCriteriaTypes = CType(wfProfileCriteria, WFProfileCriteriaTypes)

				Select Case wfProfileCriteriaType
					Case Is = wfProfileCriteriaType.CentralImport
						'In this case, load all Workflow profiles
						Return String.Empty

					Case Is = wfProfileCriteriaType.SameProfile
						Return "And (WFProfileName = '" & SqlStringHelper.EscapeSqlString(api.WorkflowProfile.Name) & "') "

					Case Is  = wfProfileCriteriaType.ProfileText1Map
						Dim mappedProfileName As String = api.WorkflowProfile.GetAttributeValue(api.ScenarioTypeID, SharedConstants.WorkflowProfileAttributeIndexes.Text1)
						If Not String.IsNullOrWhiteSpace(mappedProfileName) Then
							Return "And (WFProfileName = '" & SqlStringHelper.EscapeSqlString(mappedProfileName) & "') "
						Else
							Throw New Exception("Capital Planning: Profile Name Map value not assigned to Text1 for WFProfile '" & api.WorkflowProfile.Name & "', Scenario Type: '" & ScenarioType.GetItem(api.ScenarioTypeID).Name & "'")
						End If
					Case Else
						Throw New Exception("Capital Planning: Unknown Profile Criteria Type.")
				End Select

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function GetAssignedEntityCriteria(ByVal si As SessionInfo, ByVal api As Transformer) As String
			Try
				Dim inClause As String = String.Empty

				'Get the list of entities assigned to this Workflow
				Dim assignedEntities As List(Of WorkflowProfileEntityInfo) = BRApi.Workflow.Metadata.GetProfileEntities(si, api.WorkflowProfile.ProfileKey)
				If Not assignedEntities Is Nothing Then
					If assignedEntities.Count > 0 Then
						'Create the list of entity names
						Dim entityNames As New List(Of String)
						For Each entityInfo As WorkflowProfileEntityInfo In assignedEntities
							entityNames.Add(SqlStringHelper.EscapeSqlString(entityInfo.EntityName))
						Next

						'Create a SQL In Clause containing the list of owned entities
						Dim inList As String = SQLStringHelper.CreateInList(entityNames, True)
						InClause = " And (Entity In(" & inList & ")) "
					Else
						'Throw and error, No Assigned entities (Required For Entity Transfer Processing)
						Throw New Exception("Capital Planning: Workflow Profile '" & api.WorkflowProfile.Name & "' must have assigned Entities for T-Out/T-In Processing.")
					End If
				Else
					'Throw and error, No Assigned entities (Required For Entity Transfer Processing)
					Throw New Exception("Capital Planning: Workflow Profile '" & api.WorkflowProfile.Name & "' must have assigned Entities for T-Out/T-In Processing.")
				End If

				Return inClause

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Sub ValidateDataSourceFieldAssignment(ByVal si As SessionInfo, ByVal args As ConnectorArgs, requiredMasterDim As String, ByVal requiredSourceField As String, ByVal dataSource As ParserLayoutInfo)
			Try

				If Not dataSource Is Nothing Then
					Dim foundDim As Boolean = False
					'Evaluate the source field name that is assigne to the target
					For Each pDim As ParserDimensionInfo In dataSource.Dimensions
						If pDim.Name.XFEqualsIgnoreCase(requiredMasterDim) Then
							foundDim = True
							Dim assignedSourceField As String = pDim.GetAttributeValue(StageConstants.DimensionAttributeIndexes.ConnectorSourceFieldName)
							If Not assignedSourceField.XFEqualsIgnoreCase(requiredSourceField) Then
								'Throw and error, the required source field assignment has not been met
								Throw New Exception("Capital Planning: DataSource '" & dataSource.Name & "' requires that source field '" & requiredSourceField & "' be assigned to dimension '" & requiredMasterDim & "'." )
							End If
							Exit For
						End If
					Next

					If Not foundDim Then
						'Throw the same error, the required dimension was not found in the data source
						Throw New Exception("Capital Planning: DataSource '" & dataSource.Name & "' requires that source field '" & requiredSourceField & "' be assigned to dimension '" & requiredMasterDim & "'." )
					End If
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub

#End Region

	End Class
End Namespace