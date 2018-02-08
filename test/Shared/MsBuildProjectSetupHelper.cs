// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
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

        private static string _globalJsonText;
        private static string GlobalJsonText
        {
            get
            {
                if (string.IsNullOrEmpty(_globalJsonText))
                {
                    lock (_syncObj)
                    {
                        if (string.IsNullOrEmpty(_globalJsonText))
                        {
                            string globalJsonFilePath = null;
                            var current = new DirectoryInfo(AppContext.BaseDirectory);
                            while (current != null)
                            {
                                globalJsonFilePath = Path.Combine(current.FullName, "global.json");
                                if (File.Exists(globalJsonFilePath))
                                {
                                    break;
                                }
                                current = current.Parent;
                            }

                            _globalJsonText = File.ReadAllText(globalJsonFilePath);
                        }
                    }
                }

                return _globalJsonText;
            }
        }

        public static void InstallGlobalTool()
        {
            File.WriteAllText("Nuget.config", MsBuildProjectStrings.GetNugetConfigText());
            var result = Command.CreateDotNet("install",
                new string[] { "tool", "dotnet-aspnet-codegenerator", "-g", "--configfile", "Nuget.config", "--version", MsBuildProjectStrings.GlobalToolVersion })
                .WithEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "true")
                .InWorkingDirectory(Directory.GetCurrentDirectory())
                .OnErrorLine(l => Console.WriteLine(l))
                .OnOutputLine(l => Console.WriteLine(l))
                .Execute();

            if (result.ExitCode != 0 )
            {
                Console.Error.WriteLine("Failed to install global tool.");
            }
        }

        public void SetupProjects(TemporaryFileProvider fileProvider, ITestOutputHelper output, bool fullFramework = false)
        {
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Root"));
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Library1"));
            fileProvider.Add("global.json", GlobalJsonText);

            var rootProjectTxt = fullFramework ? MsBuildProjectStrings.RootNet45ProjectTxt : MsBuildProjectStrings.RootProjectTxt;
            fileProvider.Add($"Root/{MsBuildProjectStrings.RootProjectName}", rootProjectTxt);
            fileProvider.Add($"Root/Startup.cs", MsBuildProjectStrings.StartupTxt);
            fileProvider.Add($"Root/{MsBuildProjectStrings.ProgramFileName}", MsBuildProjectStrings.ProgramFileText);

            fileProvider.Add($"Library1/{MsBuildProjectStrings.LibraryProjectName}", MsBuildProjectStrings.LibraryProjectTxt);
            fileProvider.Add($"Library1/ModelWithMatchingShortName.cs", "namespace Library1.Models { public class ModelWithMatchingShortName { } }");
            fileProvider.Add($"Library1/Car.cs", MsBuildProjectStrings.CarTxt);
            fileProvider.Add($"Library1/Product.cs", MsBuildProjectStrings.ProductTxt);
            RestoreAndBuild(Path.Combine(fileProvider.Root, "Root"), output);
        }

        internal void SetupProjectsForIdentityScaffolder(TemporaryFileProvider fileProvider, ITestOutputHelper output)
        {
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Root"));
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Library1"));
            fileProvider.Add("global.json", GlobalJsonText);
            fileProvider.Add($"Root/{MsBuildProjectStrings.RootProjectName}", MsBuildProjectStrings.RootProjectTxt);
            fileProvider.Add($"Root/Startup.cs", MsBuildProjectStrings.StartupTxt);
            fileProvider.Add($"Root/{MsBuildProjectStrings.ProgramFileName}", MsBuildProjectStrings.ProgramFileText);
            fileProvider.Add($"Root/{MsBuildProjectStrings.IdentityContextName}", MsBuildProjectStrings.IdentityContextText);
            fileProvider.Add($"Library1/{MsBuildProjectStrings.LibraryProjectName}", MsBuildProjectStrings.LibraryProjectTxt);
            RestoreAndBuild(Path.Combine(fileProvider.Root, "Root"), output);
        }

        private void RestoreAndBuild(string path, ITestOutputHelper output)
        {
            var result = Command.CreateDotNet("restore",
                new string[] { })
                .WithEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "true")
                .InWorkingDirectory(path)
                .OnErrorLine(l => output.WriteLine(l))
                .OnOutputLine(l => output.WriteLine(l))
                .Execute();

            if (result.ExitCode != 0)
            {
                throw new InvalidOperationException($"Restore failed with exit code: {result.ExitCode} :: Dotnet path: {DotNetMuxer.MuxerPathOrDefault()}");
            }

            result = Command.CreateDotNet("build", new string[] { "-c", Configuration })
                .WithEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "true")
                .InWorkingDirectory(path)
                .OnErrorLine(l => output.WriteLine(l))
                .OnOutputLine(l => output.WriteLine(l))
                .Execute();

            if (result.ExitCode != 0)
            {
                throw new InvalidOperationException($"Build failed with exit code: {result.ExitCode}");
            }
        }

        public void SetupProjectsWithoutEF(TemporaryFileProvider fileProvider, ITestOutputHelper output)
        {
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Root"));
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Library1"));
            fileProvider.Add("global.json", GlobalJsonText);

            var rootProjectTxt = MsBuildProjectStrings.RootProjectTxtWithoutEF;
            fileProvider.Add($"Root/{MsBuildProjectStrings.RootProjectName}", rootProjectTxt);
            fileProvider.Add($"Root/Startup.cs", MsBuildProjectStrings.StartupTxtWithoutEf);
            fileProvider.Add($"Root/{MsBuildProjectStrings.ProgramFileName}", MsBuildProjectStrings.ProgramFileText);

            fileProvider.Add($"Library1/{MsBuildProjectStrings.LibraryProjectName}", MsBuildProjectStrings.LibraryProjectTxt);
            fileProvider.Add($"Library1/ModelWithMatchingShortName.cs", "namespace Library1.Models { public class ModelWithMatchingShortName { } }");
            fileProvider.Add($"Library1/Car.cs", MsBuildProjectStrings.CarTxt);
            fileProvider.Add($"Library1/Product.cs", MsBuildProjectStrings.ProductTxt);

            RestoreAndBuild(Path.Combine(fileProvider.Root, "Root"), output);
        }

        public void SetupProjectsWithDbContextInDependency(TemporaryFileProvider fileProvider, ITestOutputHelper output)
        {
            fileProvider.Add("global.json", GlobalJsonText);
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Root"));
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "DAL"));
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Library1"));

            var rootProjectTxt = MsBuildProjectStrings.WebProjectTxt;
            fileProvider.Add($"Root/{MsBuildProjectStrings.RootProjectName}", rootProjectTxt);
            fileProvider.Add($"Root/Startup.cs", MsBuildProjectStrings.StartupWithDbContext);
            fileProvider.Add($"Root/{MsBuildProjectStrings.ProgramFileName}", MsBuildProjectStrings.ProgramFileText);
            fileProvider.Add($"Root/{MsBuildProjectStrings.AppSettingsFileName}", MsBuildProjectStrings.AppSettingsFileTxt);

            fileProvider.Add($"Library1/{MsBuildProjectStrings.LibraryProjectName}", MsBuildProjectStrings.LibraryProjectTxt);
            fileProvider.Add($"Library1/ModelWithMatchingShortName.cs", "namespace Library1.Models { public class ModelWithMatchingShortName { } }");
            fileProvider.Add($"Library1/Car.cs", MsBuildProjectStrings.CarTxt);
            fileProvider.Add($"Library1/Product.cs", MsBuildProjectStrings.ProductTxt);

            fileProvider.Add($"DAL/{MsBuildProjectStrings.DALProjectName}", MsBuildProjectStrings.DAL);
            fileProvider.Add($"DAL/{MsBuildProjectStrings.CarContextFileName}", MsBuildProjectStrings.CarContextTxt);
            

            RestoreAndBuild(Path.Combine(fileProvider.Root, "Root"), output);
        }
    }
}
