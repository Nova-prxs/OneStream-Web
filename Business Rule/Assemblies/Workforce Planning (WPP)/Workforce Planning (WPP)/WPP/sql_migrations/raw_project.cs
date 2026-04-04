using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
	public class raw_project : IMigration
	{
		//Declare table name
		string tableName = "XFC_INV_RAW_project";
		
		#region "Get Migration Query"

		public string GetMigrationQuery(SessionInfo si, string type)
		{
			try
			{
				//Handle type input
				if (type.ToLower() != "up" && type.ToLower() != "down")
					throw new XFException(si, new Exception("Migration type must be 'up' or 'down'"));
				//Up
				string upQuery = $@"
				IF OBJECT_ID(N'{tableName}', N'U') IS NULL
				BEGIN
					CREATE TABLE {tableName} (
					    func VARCHAR(255),
						type VARCHAR(255),
						label_position_1 VARCHAR(255),
					    label_position_2 VARCHAR(255),
					    label_position_3 VARCHAR(255),
					    label_position_4 VARCHAR(255),
					    label_position_5 VARCHAR(255),
					    region VARCHAR(255),
						company_name VARCHAR(255),
					    company_id VARCHAR(255),
					    country VARCHAR(255),
					    branch VARCHAR(255),
					    site VARCHAR(255),
					    professional_area VARCHAR(255),
					    poi_poe VARCHAR(255),
					    cc VARCHAR(255),
					    cc_name VARCHAR(255),
					    cpi VARCHAR(255),
					    cpi_name VARCHAR(255),
					    project VARCHAR(255) NOT NULL,
					    project_name VARCHAR(255),
					    decision_criteria VARCHAR(255),
					    aggregate VARCHAR(255),
					    libre VARCHAR(255),
					    delivered_before DECIMAL(18, 2),
					    delivered_year_py_2 DECIMAL(18, 2),
					    delivered_year_py_1 DECIMAL(18, 2),
					    visibility_year_cy DECIMAL(18, 2),
					    draft_visibility_year_cy DECIMAL(18, 2),
					    requirement_year_cy DECIMAL(18, 2),
					    decided_year_cy DECIMAL(18, 2),
					    ordered_year_cy DECIMAL(18, 2),
					    delivered_ytd_year_cy DECIMAL(18, 2),
					    visibility_year_ny_1 DECIMAL(18, 2),
					    draft_visibility_year_ny_1 DECIMAL(18, 2),
					    requirement_year_ny_1 DECIMAL(18, 2),
					    decided_year_ny_1 DECIMAL(18, 2),
					    ordered_year_ny_1 DECIMAL(18, 2),
					    visibility_year_ny_2 DECIMAL(18, 2),
					    draft_visibility_year_ny_2 DECIMAL(18, 2),
					    requirement_year_ny_2 DECIMAL(18, 2),
					    decided_year_ny_2 DECIMAL(18, 2),
					    ordered_year_ny_2 DECIMAL(18, 2),
					    visibility_year_ny_3 DECIMAL(18, 2),
					    draft_visibility_year_ny_3 DECIMAL(18, 2),
					    requirement_year_ny_3 DECIMAL(18, 2),
					    decided_year_ny_3 DECIMAL(18, 2),
					    visibility_year_ny_4 DECIMAL(18, 2),
					    draft_visibility_year_ny_4 DECIMAL(18, 2),
					    requirement_year_ny_4 DECIMAL(18, 2),
					    decided_year_ny_4 DECIMAL(18, 2),
					    total_visibility DECIMAL(18, 2),
					    total_draft_visibility DECIMAL(18, 2),
					    total_requirement DECIMAL(18, 2),
					    contract_commitment DECIMAL(18, 2),
					    total_decided DECIMAL(18, 2),
					    total_ordered DECIMAL(18, 2),
					    total_delivered DECIMAL(18, 2),
					    strategic_axis_name VARCHAR(255),
					    project_status VARCHAR(255),
					    cpil_name VARCHAR(255),
					    program_position VARCHAR(255),
					    dpci_analyst VARCHAR(255),
					    reason VARCHAR(255),
					    start_date DATE,
					    end_date DATE,
					    cash_before DECIMAL(18, 2),
					    cash_year_py_2 DECIMAL(18, 2),
					    cash_year_py_1 DECIMAL(18, 2),
					    cash_ytd_year_cy DECIMAL(18, 2),
					    total_cash DECIMAL(18, 2),
						budget_owner VARCHAR(255),
						date_of_data_extraction DATE NOT NULL,
						CONSTRAINT PK_{tableName} PRIMARY KEY (project, date_of_data_extraction)
					)
				END;
				";
				
				//Down
				string downQuery = $@"
					DROP TABLE {tableName};
				";
				
				return type.ToLower() == "up" ? upQuery : downQuery;
			}
			catch (Exception ex)
			{
				throw new XFException(si, ex);
			}
		}
		
		#endregion
		
		#region "Get Population Query"

		public string GetPopulationQuery(SessionInfo si, string type)
		{
			try
			{
				//Handle type input
				if (type.ToLower() != "up" && type.ToLower() != "down")
					throw new XFException(si, new Exception("Population type must be 'up' or 'down'"));
				//Up
				string upQuery = $@"
					
				";
				
				//Down
				string downQuery = $@"
					
				";
				return type.ToLower() == "up" ? upQuery : downQuery;
			}
			catch (Exception ex)
			{
				throw new XFException(si, ex);
			}
		}
		
		#endregion

	}
}
