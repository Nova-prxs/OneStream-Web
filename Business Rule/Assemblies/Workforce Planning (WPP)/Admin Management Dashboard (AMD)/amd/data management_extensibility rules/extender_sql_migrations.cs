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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_sql_migrations
{
	public class MainClass
	{
		public object Main(SessionInfo si, BRGlobals globals, object api, ExtenderArgs args)
		{
			//Get assembly migrations class
			var migrationsClass = new Workspace.__WsNamespacePrefix.__WsAssemblyName.Migrations();
			
			try
			{
				switch (args.FunctionType)
				{
					case ExtenderFunctionType.Unknown:
						break;
						
					case ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep:
						//Get function name
						string functionName = args.NameValuePairs["p_function"];
						
						//Control function name
						
						#region "Up Migration"
						
						if (functionName == "UpMigration")
						{
							using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
							{
								//Execute migration queries
								foreach (var query in migrationsClass.GetMigrationQueries(si, "up"))
								{
									BRApi.Database.ExecuteSql(dbConn, query, false);
								}
								//Populate tables
								foreach (var query in migrationsClass.GetPopulationQueries(si, "up"))
								{
									BRApi.Database.ExecuteSql(dbConn, query, false);
								}
							}
						}
						
						#endregion
						
						#region "Create Tables"
						
						else if (functionName == "CreateTables")
						{
							using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
							{
								//Execute migration queries
								foreach (var query in migrationsClass.GetMigrationQueries(si, "up"))
								{
									BRApi.Database.ExecuteSql(dbConn, query, false);
								}
							}
						}
						
						#endregion
						
						#region "Populate Tables"
						
						else if (functionName == "PopulateTables")
						{
							using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
							{
								//Populate tables
								foreach (var query in migrationsClass.GetPopulationQueries(si, "up"))
								{
									BRApi.Database.ExecuteSql(dbConn, query, false);
								}
							}
						}
						
						#endregion
						
						#region "Delete Tables"
						
						else if (functionName == "DeleteTables")
						{
							using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
							{
								//Populate tables
								List<string> queries = migrationsClass.GetMigrationQueries(si, "down");
								queries.Reverse();
								foreach (var query in queries)
								{
									BRApi.Database.ExecuteSql(dbConn, query, false);
								}
							}
						}
						
						#endregion
						
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
	}
}
