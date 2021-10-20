#tool "nuget:?package=GitVersion.CommandLine&version=5.7.0"
#tool "nuget:?package=NuGet.CommandLine&version=5.8.1"

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
//    Build Variables
/////////////////////////////////////////////////////////////////////
var solution = "./Cake.MarkdownToPdf.sln";
var project = "./src/Cake.MarkdownToPdf/Cake.MarkdownToPdf.csproj";
var outputDir = "./buildArtifacts/";
var outputDirAddin = outputDir+"Addin/";
var outputDirNuget = outputDir+"NuGet/";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Removes the output directory")
	.Does(() => {
	  
	if (DirectoryExists(outputDir))
	{
		DeleteDirectory(outputDir, new DeleteDirectorySettings {
			Recursive = true,
			Force = true
		});
	}
	CreateDirectory(outputDir);
});

GitVersion versionInfo = null;
Task("Version")
  .Description("Retrieves the current version from the git repository")
  .Does(() => {
		
	versionInfo = GitVersion(new GitVersionSettings {
		UpdateAssemblyInfo = false
	});
		
	Information("Version: "+ versionInfo.FullSemVer);
  });

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Version")
    .Does(() => {
 		
		var msBuildSettings = new DotNetCoreMSBuildSettings()
			.WithProperty("Version", versionInfo.AssemblySemVer)
			.WithProperty("InformationalVersion", versionInfo.InformationalVersion);

		var settings = new DotNetCoreBuildSettings
		 {			
			 Configuration = "Release",
			 OutputDirectory = outputDirAddin,
			 MSBuildSettings = msBuildSettings
		 };
		 
		 DotNetCoreBuild(project, settings);
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
 		DotNetCoreTest("./tests/Cake.MarkdownToPdf.Tests/Cake.MarkdownToPdf.Tests.csproj");			
    });

Task("Pack")
    .IsDependentOn("Test")
	.IsDependentOn("Version")
    .Does(() => {
        
		// var settings = new DotNetCorePublishSettings
		//  {						
		// 	 OutputDirectory = outputDirNuget,
		// 	 NoBuild = true	
		//  };

		// DotNetCorePublish(project, settings);	
		var msBuildSettings = new DotNetCoreMSBuildSettings()
			.WithProperty("Version", versionInfo.AssemblySemVer)
			.WithProperty("InformationalVersion", versionInfo.InformationalVersion);

		var settings = new DotNetCorePackSettings
		{		
			OutputDirectory = outputDirNuget,
			MSBuildSettings = msBuildSettings,
			NoBuild = true	
		};

		DotNetCorePack(project, settings);			
    });

Task("TestFrosting")
    .IsDependentOn("Pack")
    .Does(() =>
    { 			
		DotNetCoreRun("./tests/Cake.Frosting.Tests/Cake.Frosting.Tests.csproj");
    });

Task("Default")
    .IsDependentOn("Test");

RunTarget(target);