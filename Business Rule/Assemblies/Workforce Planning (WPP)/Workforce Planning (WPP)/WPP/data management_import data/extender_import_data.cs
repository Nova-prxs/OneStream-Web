using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_import_data
{
	public class MainClass
	{
		//Reference "UTI_SharedFunctionsCS" Business Rule
		OneStream.BusinessRule.Finance.UTI_SharedFunctionsCS.MainClass UTISharedFunctionsBR = new OneStream.BusinessRule.Finance.UTI_SharedFunctionsCS.MainClass();
		
		public object Main(SessionInfo si, BRGlobals globals, object api, ExtenderArgs args)
		{
			try
			{
				switch (args.FunctionType)
				{
					case ExtenderFunctionType.Unknown:
						break;
						
					case ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep:
						//Get parameters and create full path and declare column mapping dictionary
						string tableName = args.NameValuePairs["p_table"];
						string sectionName = args.NameValuePairs["p_section"];
						string filesType = args.NameValuePairs["p_files_type"];
						string delimiter = string.Empty;
						if (filesType == "Delimited") delimiter = args.NameValuePairs["p_delimiter"];
						string filesFolderName = args.NameValuePairs["p_folder"];
						string method = args.NameValuePairs["p_method"];
						string fullPath = $"Documents/Public/{sectionName}/{filesType} Files/{filesFolderName}";
						Dictionary<string, string> columnMappingDict = new Dictionary<string, string>();
						
						//Get column mapping dictionary
						columnMappingDict = this.GetColumnMappingDict(si, tableName);
						
						//Create data table from files folder
						DataTable dt = new DataTable();
						if (filesType == "Excel")
						{
							dt = UTISharedFunctionsBR.CreateDataTableFromExcelFilesFolder(si, fullPath, FileSystemLocation.ApplicationDatabase);
						}
						else if (filesType == "Delimited")
						{
							dt = UTISharedFunctionsBR.CreateDataTableFromDelimitedFilesFolder(si, fullPath, FileSystemLocation.ApplicationDatabase, delimiter[0]);
						}
						else
						{
							throw ErrorHandler.LogWrite(si, new XFException("Files type must me 'Excel' or 'Delimited'"));
						}
						
						//Map and filter columns in DataTable
						dt = (DataTable)UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict);
						
						//Remove all files in folder
						UTISharedFunctionsBR.DeleteAllFilesInFolder(si, fullPath, FileSystemLocation.ApplicationDatabase);
						
						//Load data table to custom table
						UTISharedFunctionsBR.LoadDataTableToCustomTable(si, tableName, dt, method);
						
						//Run up population queries for dependent tables
						this.RunPopulationQueries(si, tableName);
						break;
						
					case ExtenderFunctionType.ExecuteExternalDimensionSource:
						break;
				}

				return null;
			}
			catch (Exception ex)
			{
				throw ErrorHandler.LogWrite(si, new XFException(si, ex));
			}
		}
		
		#region "Helper Functions"
		
		#region "Get Column Mapping Dictionary"
		
		private Dictionary<string, string> GetColumnMappingDict(SessionInfo si, string tableName)
		{
			//Declare new dictionary
			Dictionary<string, string> columnMappingDict;
		
			//Control table column mapping dictionaries
			if (tableName == "XFC_INV_RAW_project")
			{
				columnMappingDict = new Dictionary<string, string> {
					{"Function", "func"},
					{"Label position 1", "label_position_1"},
					{"Label position 2", "label_position_2"},
					{"Label position 3", "label_position_3"},
					{"Label position 4", "label_position_4"},
					{"Label position 5", "label_position_5"},
					{"Region", "region"},
					{"Company name", "company_name"},
					{"Company ID", "company_id"},
					{"Country", "country"},
					{"Branch", "branch"},
					{"Site", "site"},
					{"Professional area", "professional_area"},
					{"POI / POE", "poi_poe"},
					{"CC", "cc"},
					{"CC name", "cc_name"},
					{"CPI", "cpi"},
					{"CPI name", "cpi_name"},
					{"Project", "project"},
					{"Project name", "project_name"},
					{"Decision criteria", "decision_criteria"},
					{"Aggregate", "aggregate"},
					{"Libre", "libre"},
					{"Delivered N-3 and before", "delivered_before"},
					{"Delivered Year N-2", "delivered_year_py_2"},
					{"Delivered Year N-1", "delivered_year_py_1"},
					{"Visi Year N", "visibility_year_cy"},
					{"Draft Visi Year N", "draft_visibility_year_cy"},
					{"Requirement Year N", "requirement_year_cy"},
					{"Decided Year N", "decided_year_cy"},
					{"Ordered Year N", "ordered_year_cy"},
					{"Delivered YTD Year N", "delivered_ytd_year_cy"},
					{"Visi Year N+1", "visibility_year_ny_1"},
					{"Draft Visi Year N+1", "draft_visibility_year_ny_1"},
					{"Requirement Year N+1", "requirement_year_ny_1"},
					{"Decided Year N+1", "decided_year_ny_1"},
					{"Ordered Year N+1", "ordered_year_ny_1"},
					{"Visi Year N+2", "visibility_year_ny_2"},
					{"Draft Visi Year N+2", "draft_visibility_year_ny_2"},
					{"Requirement Year N+2", "requirement_year_ny_2"},
					{"Decided Year N+2", "decided_year_ny_2"},
					{"Ordered Year N+2", "ordered_year_ny_2"},
					{"Visi Year N+3", "visibility_year_ny_3"},
					{"Draft Visi Year N+3", "draft_visibility_year_ny_3"},
					{"Requirement Year N+3", "requirement_year_ny_3"},
					{"Decided Year N+3", "decided_year_ny_3"},
					{"Visi Year N+4", "visibility_year_ny_4"},
					{"Draft Visi Year N+4", "draft_visibility_year_ny_4"},
					{"Requirement Year N+4", "requirement_year_ny_4"},
					{"Decided Year N+4", "decided_year_ny_4"},
					{"Total Visi", "total_visibility"},
					{"Total Draft Visi", "total_draft_visibility"},
					{"Total Requirement", "total_requirement"},
					{"Contract Commitment", "contract_commitment"},
					{"Total Decided", "total_decided"},
					{"Total Ordered", "total_ordered"},
					{"Total Delivered", "total_delivered"},
					{"Strategic axis name", "strategic_axis_name"},
					{"Project status", "project_status"},
					{"CPIL name", "cpil_name"},
					{"Program Position", "program_position"},
					{"DPCI Analyst", "dpci_analyst"},
					{"Reason", "reason"},
					{"Start date of project", "start_date"},
					{"End date of project", "end_date"},
					{"Cash before", "cash_before"},
					{"Cash Year N-2", "cash_year_py_2"},
					{"Cash Year N-1", "cash_year_py_1"},
					{"Cash YTD Year N", "cash_ytd_year_cy"},
					{"Total Cash", "total_cash"},
					{"Reporting Budget Owner", "budget_owner"},
					{"Date of data extraction", "date_of_data_extraction"},
					{"Type", "type"},
					{"MA-DATE", "ma_date"}
				};
			}
			else
			{
				throw ErrorHandler.LogWrite(si, new XFException($"There is no column mapping dictionary for table {tableName}"));
			}
			
			return columnMappingDict;
		}
		
		#endregion
		
		#region "Run Population Queries"
		
		private void RunPopulationQueries(SessionInfo si, string tableName)
		{
			//Declare list of objects to populate with tables
			List<IMigration> tableList = new List<IMigration>();
			//Control table name
			if (tableName == "XFC_INV_RAW_project")
			{
				tableList.Add(new Workspace.__WsNamespacePrefix.__WsAssemblyName.project());
			}
			
			//Return if no tables
			if (tableList.Count < 1) return;
			
			//Get migration queries
			List<string> populationQueries = this.GetPopulationQueries(si, tableList);
			
			using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
			{
				//Populate tables
				foreach (var query in populationQueries)
				{
					BRApi.Database.ExecuteSql(dbConn, query, false);
				}
			}
		}
		
		private List<string> GetPopulationQueries(SessionInfo si, List<IMigration> tables)
		{
			//Declare list of queries
			List<string> queries = new List<string>();
			
			//Loop through all the tables to get the queries
			foreach (var table in tables)
			{
				queries.Add(table.GetPopulationQuery(si, "up"));
			}
			
			return queries;
		}
		
		#endregion
		
		#endregion
		
	}
}
