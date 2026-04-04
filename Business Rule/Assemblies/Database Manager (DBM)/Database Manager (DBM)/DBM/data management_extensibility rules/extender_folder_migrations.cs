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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_folder_migrations
{
	public class MainClass
	{
		public object Main(SessionInfo si, BRGlobals globals, object api, ExtenderArgs args)
		{
			try
			{
				switch (args.FunctionType)
				{
					case ExtenderFunctionType.Unknown:
						break;
						
					case ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep:
						//This will generate a folder structure in the application public file system
						//Define nested tuples of folders
						var folderStructure = new List<object> {
							new Tuple<string, List<object>>(
								"Investments",
								new List<object> {
									new Tuple<string, List<object>>(
										"Delimited Files",
										new List<object> {
											"Cash",
											"Project"
										}
									),
									new Tuple<string, List<object>>(
										"Excel Files",
										new List<object> {
											"Capital",
											"Cash",
											"Project",
											"RnD"
										}
									),
									new Tuple<string, List<object>>(
										"Imported Data",
										new List<object> {
											new Tuple<string, List<object>>(
												"Capital",
												new List<object> {
													new Tuple<string, List<object>>(
														"Depreciations",
														new List<object> {
															"Processed"
														}
													),
													new Tuple<string, List<object>>(
														"Assets",
														new List<object> {
															"Processed"
														}
													)
												}
											),
											new Tuple<string, List<object>>(
												"Cash",
												new List<object> {
													"Processed"
												}
											),
											new Tuple<string, List<object>>(
												"RnD",
												new List<object> {
													new Tuple<string, List<object>>(
														"Projects",
														new List<object> {
															"Processed"
														}
													)
												}
											)
										}
									),
									"Templates"
								}
							)
						};
						
						//Generate folder structure
						this.GenerateFolderStructure(si, "Documents/Public", folderStructure);
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
		
		#region "Generate Folder Structure"
		
		public void GenerateFolderStructure(SessionInfo si, string baseDirectory, List<object> folderStructure)
		{
			foreach (var item in folderStructure)
			{
				if (item is string)
				{
					//Create a folder for string items
					BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, baseDirectory, item.ToString());
				}
				else if (item is Tuple<string, List<object>>)
				{
					//Create a folder for the tuple's first item (string) and recurse for its second item (list)
					var tuple = (Tuple<string, List<object>>)item;
					string newDirectory = Path.Combine(baseDirectory, tuple.Item1);
					BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, baseDirectory, tuple.Item1);
					GenerateFolderStructure(si, newDirectory, tuple.Item2);
				}
			}
		}
		
		#endregion
		
		#endregion
		
	}
}
