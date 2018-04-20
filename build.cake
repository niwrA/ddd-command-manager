#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");
var nugetPushFeed = Argument<string>("nugetPushFeed", "Local");
 // Mandatory for the Publish-Nuget / Package targets
var packversion = Argument<string>("packversion", "0.0.0.1");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define build file variables
var nugetConfigFile = "NuGet.config";
var packageId = "CommandManagerNet";
var solutionFile = "DDD.Command.Manager.sln";
var projectFile = "CommandManagerNet/CommandManagerNet.csproj";
var testDllsPattern = "*Tests.dll";

// Define directories.
var nugetOutputDir = "nuget";

// NuGet restore config
var nugetRestoreSettings = new NuGetRestoreSettings {
    ConfigFile = new FilePath(nugetConfigFile)
};

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories("./**/bin/" + configuration);
    CleanDirectories("./**/obj/" + configuration);
    CleanDirectory(nugetOutputDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solutionFile, nugetRestoreSettings);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    // Use MSBuild
    MSBuild(solutionFile, new MSBuildSettings()
		.WithProperty("Configuration", configuration)
	);
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3("./**/bin/" + configuration + "/" + testDllsPattern, new NUnit3Settings {
        NoResults = true
    });
});

Task("Package")
	.IsDependentOn("Run-Unit-Tests")
	.Does(() => {
		// Verify if packversion argument is given
		if (!HasArgument("packversion"))
		{
			throw new Exception("'packversion' argument not specified");
		}

		// Pack the package.
        NuGetPack(projectFile, new NuGetPackSettings {
			Version			= packversion,
			OutputDirectory = nugetOutputDir,
			IncludeReferencedProjects = true,
			Properties		= new Dictionary<string, string>() { { "Configuration", configuration } }
		});
	 });

Task("Publish-Nuget")
	.IsDependentOn("Package")
	.Does(() => {

		var packages = GetFiles(nugetOutputDir + "/*.nupkg");
		// Push the packages.
        NuGetPush(packages, new NuGetPushSettings  { 
			Source		= nugetPushFeed,
			Verbosity	= NuGetVerbosity.Detailed,
        });
	 });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);