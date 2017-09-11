#load nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&prerelease

Environment.SetVariableNames();

BuildParameters.SetParameters(context: Context, 
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            title: "Cake.MarkdownToPdf",
                            repositoryOwner: "twenzel",
                            repositoryName: "Cake.MarkdownToPdf",
                            appVeyorAccountName: "twenzel",
							shouldRunInspectCode: false);

				
BuildParameters.PrintParameters(Context);


	
ToolSettings.SetToolSettings(context: Context);
	
Build.Run();
//Build.RunNuGet();