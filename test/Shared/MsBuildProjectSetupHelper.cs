// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.DependencyModel;
using Xunit.Abstractions;

namespace Microsoft.VisualStudio.Web.CodeGeneration
{
    internal class MsBuildProjectSetupHelper
    {
        #if RELEASE
        public const string Configuration = "Release";
        #else
        public const string Configuration = "Debug";
        #endif

        private static object _syncObj = new object();
        private static string _nugetConfigText;
        private static string NuGetConfigText
        {
            get
            {
                if (string.IsNullOrEmpty(_nugetConfigText))
                {
                    lock(_syncObj)
                    {
                        if (string.IsNullOrEmpty(_nugetConfigText))
                        {
                            string artifactsDir = null;
                            string nugetConfigPath = null;
                            var current = new DirectoryInfo(AppContext.BaseDirectory);
                            while (current != null)
                            {
                                if (File.Exists(Path.Combine(current.FullName, "Scaffolding.sln")))
                                {
                                    artifactsDir = Path.Combine(current.FullName, "artifacts", "build");
                                    nugetConfigPath = Path.Combine(current.FullName, "NuGet.config");
                                    break;
                                }
                                current = current.Parent;
                            }

                            _nugetConfigText = MsBuildProjectStrings.GetNugetConfigTxt(artifactsDir, nugetConfigPath);
                        }
                    }
                }

                return _nugetConfigText;
            }
        }

        private string AspNetCoreVersion
        {
            get 
            {
                var aspnetCoreLib = DependencyContext
                    .Default
                    .CompileLibraries
                    .FirstOrDefault(l => l.Name.StartsWith("Microsoft.Extensions.FileProviders.Abstractions", StringComparison.OrdinalIgnoreCase));

                return aspnetCoreLib?.Version;
            }
        }

        private string RuntimeFrameworkVersion =>
            DependencyContext
                .Default
                .RuntimeLibraries
                .FirstOrDefault(l => l.Name.StartsWith("Microsoft.NETCore.App", StringComparison.OrdinalIgnoreCase))
                ?.Version;

        private string CodeGenerationVersion
        {
            get 
            {
                var informationalVersionAttr = this.GetType().Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).First();
                return ((AssemblyInformationalVersionAttribute)informationalVersionAttr).InformationalVersion;
            }
        }

        public void SetupProjects(TemporaryFileProvider fileProvider, ITestOutputHelper output, bool fullFramework = false)
        {
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Root"));
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Library1"));
            fileProvider.Add("NuGet.config", NuGetConfigText);

            var rootProjectTxt = fullFramework ? MsBuildProjectStrings.RootNet45ProjectTxt : MsBuildProjectStrings.RootProjectTxt;
            fileProvider.Add($"Root/{MsBuildProjectStrings.RootProjectName}", string.Format(rootProjectTxt, AspNetCoreVersion, CodeGenerationVersion, RuntimeFrameworkVersion));
            fileProvider.Add($"Root/Startup.cs", MsBuildProjectStrings.StartupTxt);
            fileProvider.Add($"Root/{MsBuildProjectStrings.ProgramFileName}", MsBuildProjectStrings.ProgramFileText);

            fileProvider.Add($"Library1/{MsBuildProjectStrings.LibraryProjectName}", MsBuildProjectStrings.LibraryProjectTxt);
            fileProvider.Add($"Library1/ModelWithMatchingShortName.cs", "namespace Library1.Models { public class ModelWithMatchingShortName { } }");
            fileProvider.Add($"Library1/Car.cs", MsBuildProjectStrings.CarTxt);
            fileProvider.Add($"Library1/Product.cs", MsBuildProjectStrings.ProductTxt);
            RestoreAndBuild(Path.Combine(fileProvider.Root, "Root"), output);
        }

        private void RestoreAndBuild(string path, ITestOutputHelper output)
        {
            var result = Command.CreateDotNet("restore",
                new string[] {})
                .WithEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "true")
                .InWorkingDirectory(path)
                .OnErrorLine(l => output.WriteLine(l))
                .OnOutputLine(l => output.WriteLine(l))
                .Execute();

            if (result.ExitCode !=0)
            {
                throw new InvalidOperationException($"Restore failed with exit code: {result.ExitCode} :: Dotnet path: {DotNetMuxer.MuxerPathOrDefault()}");
            }

            result = Command.CreateDotNet("build", new string[] {"-c", Configuration})
                .WithEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "true")
                .InWorkingDirectory(path)
                .OnErrorLine(l => output.WriteLine(l))
                .OnOutputLine(l => output.WriteLine(l))
                .Execute();

           if (result.ExitCode !=0)
            {
                throw new InvalidOperationException($"Build failed with exit code: {result.ExitCode}");
            }
        }

        public void SetupProjectsWithoutEF(TemporaryFileProvider fileProvider, ITestOutputHelper output)
        {
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Root"));
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Library1"));
            fileProvider.Add("NuGet.config", NuGetConfigText);

            var rootProjectTxt = MsBuildProjectStrings.RootProjectTxtWithoutEF;
            fileProvider.Add($"Root/{MsBuildProjectStrings.RootProjectName}", string.Format(rootProjectTxt, AspNetCoreVersion, CodeGenerationVersion, RuntimeFrameworkVersion));
            fileProvider.Add($"Root/Startup.cs", MsBuildProjectStrings.StartupTxtWithoutEf);
            fileProvider.Add($"Root/{MsBuildProjectStrings.ProgramFileName}", MsBuildProjectStrings.ProgramFileText);

            fileProvider.Add($"Library1/{MsBuildProjectStrings.LibraryProjectName}", MsBuildProjectStrings.LibraryProjectTxt);
            fileProvider.Add($"Library1/ModelWithMatchingShortName.cs", "namespace Library1.Models { public class ModelWithMatchingShortName { } }");
            fileProvider.Add($"Library1/Car.cs", MsBuildProjectStrings.CarTxt);
            fileProvider.Add($"Library1/Product.cs", MsBuildProjectStrings.ProductTxt);

            RestoreAndBuild(Path.Combine(fileProvider.Root, "Root"), output);
        }
    }
}