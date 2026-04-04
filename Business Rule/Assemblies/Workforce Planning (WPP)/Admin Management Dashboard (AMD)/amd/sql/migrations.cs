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
	public class Migrations
	{
		//Get all the table classes
		List<IMigration> tables = new List<IMigration> {
			new Workspace.__WsNamespacePrefix.__WsAssemblyName.component_config()
		};

		#region "Get Migration Queries"
		
		public List<string> GetMigrationQueries(SessionInfo si, string type)
		{
			try
			{
				//Declare list of queries
				List<string> queries = new List<string>();
				
				//Loop through all the tables to get the queries
				foreach (var table in tables)
				{
					queries.Add(table.GetMigrationQuery(si, type));
				}
				
				return queries;
			}
			catch (Exception ex)
			{
				throw new XFException(si, ex);
			}
		}
		
		#endregion

		#region "Get Population Queries"
		
		public List<string> GetPopulationQueries(SessionInfo si, string type)
		{
			try
			{
				//Declare list of queries
				List<string> queries = new List<string>();
				
				//Loop through all the tables to get the queries
				foreach (var table in tables)
				{
					queries.Add(table.GetPopulationQuery(si, type));
				}
				
				return queries;
			}
			catch (Exception ex)
			{
				throw new XFException(si, ex);
			}
		}
		
		#endregion

	}
}
