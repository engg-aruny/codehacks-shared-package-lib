### What is .NET package management?
This tool, through which a developer can create, share and consume code from geographically distributed teams, also helps the teams follow standard guidelines and code practices. A shared package can be used in multiple projects with a one-time investment, and easy-to-update software centrally without impacting many consumers.

Simply put, a NuGet package is a ZIP file generated with a `.nupkg` extension containing DLLs and other manifest files that include information about the package. The developer generally shares published code with the public and private host, then a consumer retrieves the package from the host to the local machine.

**Let's understand the flow of the NuGet package from creator to  consumer**

![Flow of Nuget package](https://www.dropbox.com/s/hv0knijm19gl3lz/flow_package_mgmt.jpg?raw=1 "Flow of Nuget package")

### What we are building here?
Let's take an example that we have multiple projects in the firm and everyone wanted to consume or enable **"Application Insights telemetry collection"** to the application to achieve there could possibly be **two approaches**.

**First Approach**, everyone writes their own code in different projects to enable **"Application Insights telemetry collection"**

**Second Approach**, creating a shared library and a adding middleware extension over in the library and then publishing assemblies to public/private host (NuGet/Azure DevOps).

> The source code for this article can be found on [GitHub](https://github.com/engg-aruny/codehacks-shared-package-lib).

### Creating a package or library

Create a library package that adds or registers AddApplicationInsightsTelemetry() to an application

1. Create a new class library project in Visual Studio. You can name it whatever you like, for example, "ApplicationInsightsLibrary".

2. Add the following NuGet packages to the project:

```bash
Install-Package Microsoft.ApplicationInsights.AspNetCore
Install-Package Microsoft.Extensions.DependencyInjection

```

3. In the `codehacks-shared-package-lib` project, create a new class named `ApplicationInsightsExtensions`.

4. Inside the `ApplicationInsightsExtensions` class, create a public static method named `AddApplicationInsightsTelemetry` that takes an `IServiceCollection` parameter.

5. Inside the AddApplicationInsightsTelemetry method, add the following code to configure Application Insights telemetry:

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace codehacks_shared_package_lib
{
    public class ApplicationInsightsExtensions
    {
        public static void AddApplicationInsightsTelemetry(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
        }
    }
}
```

**A developer can generate a manifest with help of different tools** 
- `dotnet CLI`
- `nuget.exe CLI` 
- `MSBuild`

#### Pack Command
To build and generate a .nupkg file, run the dotnet pack command.

```bash
dotnet pack
```

We have an automatic way of generating packages as well whenever build the project. You need to add the following lines in the project file `propertyGroup` tag.

```bash
<GeneratePackageOnBuild>true</GeneratePackageOnBuild>
```

![dotnet pack command](https://www.dropbox.com/s/eovlmv3trc02s6i/dotnet_pack_output.jpg?raw=1 "dotnet pack command")

> Make sure to copy the path from the output window which looks like this: `C:\Code\Playground\codehacks-shared-package-lib\bin\Release\codehacks-shared-package-lib.1.0.0.nupkg`
### Consuming in NuGet package Locally

To consume a NuGet package locally, you can follow these steps.

1. Run the `dotnet pack` command per the instructions above or locate a .nupkg file from your package.
2. You need to define the package source i.e. `Tools > NuGet Package Manager > Package Manager`. see the snapshot below.
![define the package source](https://www.dropbox.com/s/jdr6rv9xizjsurc/package_manager_settings.jpg?raw=1 "define the package source")
3. Click on `Package Source -> + icon -> enter copied folder path ->  Hit OK` (like **'C:\Code\Playground\codehacks-shared-package-lib\bin\Release\'**)

Now you have completed configuring the package source, you can go to the consumer application now and right-click on the project or solution and then "Manage NuGet Package > Browse" as in the snapshot below.
![Manage NuGet Package](https://www.dropbox.com/s/m0oto7jbcbm45f4/manage_nuget_package.jpg?raw=1 "Manage NuGet Package")

> Make sure to change the package source from the right-hand corner icon.

![Manage NuGet Package Browse](https://www.dropbox.com/s/sgfgkejqbdqvkaq/manage_nuget_package_browsewindow.jpg?raw=1 "Manage NuGet Package Browse")

4. To use the library package in your application, add it as a reference to your project and call the `AddApplicationInsightsTelemetry` method in the `ConfigureServices` method of your Startup class

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add other services here
    
    // Add Application Insights telemetry
    ApplicationInsightsExtensions.AddApplicationInsightsTelemetry(services);
}

```
### Publishing package to Artifact feed in Azure DevOps

Here is an example of a YAML pipeline that builds a .NET Core application, runs unit tests, and publishes the resulting artifacts to Azure Artifacts

**Create a feed:** Create a feed in your Azure DevOps organization to store your packages. To create a feed, go to the Artifacts section of your project and click on the **"Create Feed"** button.

**Configure your pipeline:** In your pipeline, add a task to publish the package to the feed. The exact configuration of this task depends on the format of your package. Here are some examples:

```yaml
trigger:
- main

variables:
  buildConfiguration: 'Release'
  versionSuffix: '$(Build.BuildNumber)'

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '6.x'
    includePreviewVersions: true

- task: DotNetCoreCLI@2
  displayName: 'Restore NuGet packages'
  inputs:
    command: 'restore'
    projects: '**/*.csproj'
    feedsToUse: 'config'
    nugetConfigPath: './NuGet.config'

- task: DotNetCoreCLI@2
  displayName: 'Build project'
  inputs:
    command: 'build'
    projects: '**/*.csproj'
    arguments: '--configuration $(buildConfiguration) /p:VersionSuffix=$(versionSuffix)'

- task: DotNetCoreCLI@2
  displayName: 'Create NuGet package'
  inputs:
    command: 'pack'
    packagesToPack: '**/*.csproj'
    versionSuffix: '$(versionSuffix)'
    configuration: '$(buildConfiguration)'
    includeSymbols: true
    outputDir: '$(Build.ArtifactStagingDirectory)/NuGetPackages'

- task: DotNetCoreCLI@2
  displayName: 'Push NuGet package to feed'
  inputs:
    command: 'push'
    nuGetFeedType: 'internal'
    packagesToPush: '$(Build.ArtifactStagingDirectory)/NuGetPackages/*.nupkg'
    nuGetFeedPublish: 'my-nuget-feed'
    versioningScheme: 'byEnvVar'
    versionEnvVar: 'BUILD_BUILDNUMBER'

```

### Publish packages to NuGet.org

1. Sign in to your [NuGet.org](https://www.nuget.org/) account or create one if you haven't.

2. Select your user name icon then select API Keys.

3. Select Create then enter a name for your key. Give your key a Push new packages and package version scope, and enter * in the glob pattern field to select all packages. Select Create when you are done.

Here is a more [official article](https://learn.microsoft.com/en-us/azure/devops/artifacts/nuget/publish-to-nuget-org?view=azure-devops&tabs=dotnet) about it.

### Summary
.NET package management using NuGet is a popular method of managing dependencies in .NET projects. NuGet is a package manager for .NET that enables developers to easily find, install, and update third-party libraries and tools in their projects.

NuGet packages are versioned to enable developers to track changes and ensure consistency in their projects. Version numbers are composed of three parts: major version, minor version, and patch version. Developers can use semantic versioning to determine how to update package versions based on the changes made to the package.

In addition to managing dependencies, NuGet can also be used to create and publish packages to both public and private NuGet feeds. When creating a package, developers can specify the version number, dependencies, and other metadata. NuGet also supports the versioning of packages, enabling developers to track changes and manage updates.

In order to manage NuGet packages and versions in .NET projects, developers can use the NuGet Package Manager in Visual Studio, the dotnet CLI, or YAML pipelines in Azure DevOps. Developers can restore, install, update, and remove packages as needed, and specify the version of a package to ensure consistency across different environments.

Overall, NuGet and versioning provide powerful tools for .NET developers to manage dependencies and ensure consistency in their projects, simplifying the development process and improving code quality.
