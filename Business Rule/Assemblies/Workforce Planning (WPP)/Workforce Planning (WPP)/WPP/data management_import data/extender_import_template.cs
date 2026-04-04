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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_import_templates
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
						//Get function name
						string paramFunction = args.NameValuePairs["p_function"];
						
						//Control function name
						
						#region "Import Projects"
						
						if (paramFunction == "ImportProjects")
						{
							//Get parameters
							string queryParams = args.NameValuePairs["p_query_params"];
							Dictionary<string, string> parameterDict = UTISharedFunctionsBR.SplitQueryParams(si, queryParams);
							List<DbParamInfo> dbParamInfos = UTISharedFunctionsBR.CreateQueryParams(si, queryParams);
							
							//Get file names in the imported data capital folder and build path to save the processed files
							string sourcePath = "Documents/Public/Investments/Imported Data/RnD/Projects";
							string targetFinalPath = "Processed";
							string targetPath = $"{sourcePath}/{targetFinalPath}";
							List<NameAndAccessLevel> fileNames = BRApi.FileSystem.GetAllFileNames(
								si,
								FileSystemLocation.ApplicationDatabase,
								sourcePath,
								XFFileType.All,
								false, false, false
							);
							
							//Loop through the folder files to populate the raw data table (it must be just one)
							foreach (NameAndAccessLevel fileName in fileNames)
							{
								//Check that the file has the correct name
								if (!fileName.Name.Contains("Projects"))
								{
									//Clear folder
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase);
									throw ErrorHandler.LogWrite(si, new XFException($"Uploaded file is not correct. Check that the name begins with 'Projects'."));
								}
								
								//Get file bytes
								byte[] fileBytes = BRApi.FileSystem.GetFile(
									si,
									FileSystemLocation.ApplicationDatabase,
									fileName.Name,
									true, true
								).XFFile.ContentFileBytes;
								
								//Load excel template
								BRApi.Utilities.LoadCustomTableUsingExcel(
									si,
									SourceDataOriginTypes.FromFileUpload,
									fileName.Name,
									fileBytes
								);
							
								//Create processed folder if necessary
								BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, sourcePath, targetFinalPath);
								
								//Copy file to processed folder
								UTISharedFunctionsBR.CopyXLSXFile(si, fileName.Name.Split("/").Last(), sourcePath, fileName.Name.Split("/").Last(), targetPath);
								
								//Clear folder and exit the loop
								UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase);
								break;
							}
							
							//Run population queries for rnd raw project table
							this.RunPopulationQueries(si, "XFC_INV_RAW_project");
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
		
		#region "Helper Functions"
		
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
