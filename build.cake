#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
//    Build Variables
/////////////////////////////////////////////////////////////////////
var solution = "./Cake.MarkdownToPdf.sln";
var project = "./src/Cake.MarkdownToPdf/Cake.MarkdownToPdf.csproj";
var outputDir = "./buildArtifacts/";
var outputDirAddin = outputDir+"Addin/";
var outputDirNuget = outputDir+"NuGet/";
var nuspecDir = "./nuspec/";

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
 		
		var settings = new DotNetCorePublishSettings
		 {			
			 Configuration = "Release",
			 OutputDirectory = outputDirAddin,
			 ArgumentCustomization = args => args.Append("/p:SemVer=" + versionInfo.NuGetVersionV2)
		 };
		 
		 DotNetCorePublish(project, settings);
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var projectFiles = GetFiles("./tests/**/*.csproj");
        foreach(var file in projectFiles)
        {
            DotNetCoreTest(file.FullPath);
        }
    });

Task("Pack")
    .IsDependentOn("Test")
	.IsDependentOn("Version")
    .Does(() => {
        
		var nuGetPackSettings = new NuGetPackSettings {	
			Version = versionInfo.NuGetVersionV2,
			BasePath = outputDirAddin,
			OutputDirectory = outputDirNuget,
			NoPackageAnalysis = true
		};
		
		NuGetPack(nuspecDir + "Cake.MarkdownToPdf.nuspec", nuGetPackSettings);			
    });

Task("Default")
    .IsDependentOn("Test");

RunTarget(target);