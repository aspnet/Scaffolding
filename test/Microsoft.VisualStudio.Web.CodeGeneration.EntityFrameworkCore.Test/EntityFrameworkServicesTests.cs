﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.ProjectModel;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating.Compilation;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils;
using Moq;
using NuGet.Frameworks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore.Test
{
    public class EntityFrameworkServicesTests
    {
        private const string SkipReason = "CI doesn't have CLI version required for the MSBuild stuff to work";

        private IApplicationInfo _appInfo;
        private ICodeGenAssemblyLoadContext _loader;
        private IModelTypesLocator _modelTypesLocator;
        private IDbContextEditorServices _dbContextEditorServices;
        private Mock<IPackageInstaller> _packageInstaller;
        private Mock<IServiceProvider> _serviceProvider;
        private CodeAnalysis.Workspace _workspace;
        private ILogger _logger;
        private IProjectContext _projectContext;
        private ITestOutputHelper _output;

        public EntityFrameworkServicesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private EntityFrameworkServices GetEfServices(string path, string applicationName)
        {
            _appInfo = new ApplicationInfo(applicationName, path, "Debug");
            _logger = new ConsoleLogger();
            _packageInstaller = new Mock<IPackageInstaller>();
            _serviceProvider = new Mock<IServiceProvider>();

            _projectContext = GetProjectInformation(path);
            _workspace = new RoslynWorkspace(_projectContext);
            _loader = new TestAssemblyLoadContext(_projectContext);
            _modelTypesLocator = new ModelTypesLocator(_workspace);
            var dbContextMock = new Mock<IDbContextEditorServices>();
            var editSyntaxTreeResult = new EditSyntaxTreeResult()
            {
                Edited = true
            };

            dbContextMock.Setup(db => db.EditStartupForNewContext(It.IsAny<ModelType>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(editSyntaxTreeResult);

            var filesLocator = new FilesLocator();
            var compilationService = new RoslynCompilationService(_appInfo, _loader, _projectContext);
            var templatingService = new Templating.RazorTemplating(compilationService);
            _dbContextEditorServices = new DbContextEditorServices(_projectContext, _appInfo, filesLocator, templatingService);

            return new EntityFrameworkServices(
                _projectContext,
                _appInfo,
                _loader,
                _modelTypesLocator,
                _dbContextEditorServices,
                _packageInstaller.Object,
                _serviceProvider.Object,
                _workspace,
                _logger);

        }

        private IProjectContext GetProjectInformation(string path)
        {
            var rootContext = new MsBuildProjectContextBuilder(path)
                .Build();
            return rootContext;
        }

        [Fact (Skip = SkipReason)]
        public async void TestGetModelMetadata_WithoutDbContext()
        {
            using (var fileProvider = new TemporaryFileProvider())
            {
                SetupProjects(fileProvider);

                var appName = MsBuildProjectStrings.RootProjectName;
                var path = Path.Combine(fileProvider.Root, "Root", appName);
                var efServices = GetEfServices(path, appName);
                var modelType = _modelTypesLocator.GetType("ModelWithMatchingShortName").First();
                var metadata = await efServices.GetModelMetadata(modelType);
                Assert.Equal(ContextProcessingStatus.ContextAvailable, metadata.ContextProcessingStatus);
                Assert.Null(metadata.ModelMetadata.Navigations);
                Assert.False(metadata.ModelMetadata.Properties.Any());

                modelType = _modelTypesLocator.GetType("Library1.Models.Car").First();
                metadata = await efServices.GetModelMetadata(modelType);
                Assert.Equal(ContextProcessingStatus.ContextAvailable, metadata.ContextProcessingStatus);
                Assert.Null(metadata.ModelMetadata.Navigations);
                Assert.Null(metadata.ModelMetadata.PrimaryKeys);
                Assert.Equal(3, metadata.ModelMetadata.Properties.Length);
            }
        }

        private void SetupProjects(TemporaryFileProvider fileProvider)
        {
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Root"));
            Directory.CreateDirectory(Path.Combine(fileProvider.Root, "Library1"));
            fileProvider.Add("Nuget.config", MsBuildProjectStrings.NugetConfigTxt);

            fileProvider.Add($"Root/{MsBuildProjectStrings.RootProjectName}", MsBuildProjectStrings.RootProjectTxt);
            fileProvider.Add($"Root/Startup.cs", MsBuildProjectStrings.StartupTxt);

            fileProvider.Add($"Library1/{MsBuildProjectStrings.LibraryProjectName}", MsBuildProjectStrings.LibraryProjectTxt);
            fileProvider.Add($"Library1/ModelWithMatchingShortName.cs", "namespace Library1.Models { public class ModelWithMatchingShortName { } }");
            fileProvider.Add($"Library1/Car.cs", @"
using System.Collections.Generic;

namespace Library1.Models
{
    public class Car
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int ManufacturerID { get; set; }
        public Manufacturer Manufacturer { get; set; }
    }

    public class Manufacturer
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Car> Cars { get; set; }
    }
}");
            var result = Command.CreateDotNet("restore3",
                new[] { Path.Combine(fileProvider.Root, "Root", "test.csproj") })
                .OnErrorLine(l => _output.WriteLine(l))
                .OnOutputLine(l => _output.WriteLine(l))
                .Execute();
        }

        [Fact (Skip =SkipReason + "Need to workaround the fact that the test doesn't run in the project's dependency context.")]
        public async void TestGetModelMetadata_WithDbContext()
        {
            using (var fileProvider = new TemporaryFileProvider())
            {
                SetupProjects(fileProvider);

                var appName = MsBuildProjectStrings.RootProjectName;
                var path = Path.Combine(fileProvider.Root, "Root", appName);
                var efServices = GetEfServices(path, appName);

                var modelType = _modelTypesLocator.GetType("Library1.Models.Car").First();
                var metadata = await efServices.GetModelMetadata("TestProject.Models.CarContext", modelType, string.Empty);

                Assert.Equal(ContextProcessingStatus.ContextAvailable, metadata.ContextProcessingStatus);
                Assert.Equal(3, metadata.ModelMetadata.Properties.Length);
            }
        }
    }
}
