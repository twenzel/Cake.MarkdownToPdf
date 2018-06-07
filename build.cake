var target = Argument("target", "Default");
var solution = "./Cake.MarkdownToPdf.sln";

Task("Clean")
    .Does(() => {
        DotNetCoreClean(solution);
    });

Task("Build")
    .Does(() => {
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = "Release",
            OutputDirectory = "./buildArtifacts/"
        };

        DotNetCoreBuild(solution, settings);
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
    .Does(() => {
        DotNetCorePack("./src/Cake.MarkdownToPdf/Cake.MarkdownToPdf.csproj", new DotNetCorePackSettings {
            OutputDirectory = "./nuget/",
            NoBuild = true
        });
    });

Task("Default")
    .IsDependentOn("Test");

RunTarget(target);