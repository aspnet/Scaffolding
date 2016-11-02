// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.DotNet.Cli.Utils;
using Xunit.Abstractions;

namespace Microsoft.VisualStudio.Web.CodeGeneration
{
    internal class MsBuildProjectSetupHelper
    {
        public void SetupProjects(TemporaryFileProvider fileProvider, ITestOutputHelper output)
        {
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Root"));
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Library1"));
            fileProvider.Add("Nuget.config", MsBuildProjectStrings.NugetConfigTxt);

            fileProvider.Add($"Root/{MsBuildProjectStrings.RootProjectName}", MsBuildProjectStrings.RootProjectTxt);
            fileProvider.Add($"Root/Startup.cs", MsBuildProjectStrings.StartupTxt);
            fileProvider.Add($"Root/{MsBuildProjectStrings.ProgramFileName}", MsBuildProjectStrings.ProgramFileText);

            fileProvider.Add($"Library1/{MsBuildProjectStrings.LibraryProjectName}", MsBuildProjectStrings.LibraryProjectTxt);
            fileProvider.Add($"Library1/ModelWithMatchingShortName.cs", "namespace Library1.Models { public class ModelWithMatchingShortName { } }");
            fileProvider.Add($"Library1/Car.cs", MsBuildProjectStrings.CarTxt);
            fileProvider.Add($"Library1/Product.cs", MsBuildProjectStrings.ProductTxt);

            var result = Command.CreateDotNet("restore",
                new[] { Path.Combine(fileProvider.Root, "Root", "Test.csproj") })
                .OnErrorLine(l => output.WriteLine(l))
                .OnOutputLine(l => output.WriteLine(l))
                .Execute();
        }
    }
}