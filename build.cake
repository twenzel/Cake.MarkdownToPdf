#tool "nuget:?package=GitVersion.CommandLine&version=5.7.0"
#tool "nuget:?package=NuGet.CommandLine&version=6.4.0"

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
//    Build Variables
/////////////////////////////////////////////////////////////////////
var solution = "./Cake.MarkdownToPdf.sln";
var project = "./src/Cake.MarkdownToPdf/Cake.MarkdownToPdf.csproj";
var outputDirRoot = new DirectoryPath("./BuildArtifacts/").MakeAbsolute(Context.Environment);
var packageOutputDir = outputDirRoot.Combine("NuGet");
var outputDirTemp = outputDirRoot.Combine("Temp");
var outputDirTests = outputDirTemp.Combine("Tests/");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Removes the output directory")
	.Does(() => {
	  
	EnsureDirectoryDoesNotExist(outputDirRoot, new DeleteDirectorySettings {
			Recursive = true,
			Force = true
		});
	CreateDirectory(outputDirRoot);
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

		var msBuildSettings = new DotNetMSBuildSettings()
		{
			Version =  versionInfo.AssemblySemVer,
			InformationalVersion = versionInfo.InformationalVersion,
			PackageVersion = versionInfo.NuGetVersionV2
		}.WithProperty("PackageOutputPath", packageOutputDir.FullPath);	

		var settings = new DotNetBuildSettings {
			Configuration = "Release",
			MSBuildSettings = msBuildSettings
		};
	 
		DotNetBuild(solution, settings);

    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
		var settings = new DotNetTestSettings {
			Configuration = "Release",
			Loggers = new[]{"trx;"},
			ResultsDirectory = outputDirTests,
			Collectors = new[] {"XPlat Code Coverage"},				
			NoBuild = true
		};

        DotNetTest(solution, settings);	
    });

Task("Default")
    .IsDependentOn("Test");

RunTarget(target);
