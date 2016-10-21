// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.ProjectModel.Resolution;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Frameworks;

namespace Microsoft.Extensions.ProjectModel
{
    internal class DotNetProjectContext : IProjectContext
    {
        private readonly ProjectContext _projectContext;
        private readonly OutputPaths _paths;
        private readonly Lazy<JObject> _rawProject;
        private readonly CommonCompilerOptions _compilerOptions;
        private readonly Lazy<DotNetDependencyProvider> _dependencyProvider;

        private IEnumerable<DependencyDescription> _packageDependencies;
        private IEnumerable<ResolvedReference> _compilationAssemblies;
        private IEnumerable<string> _projectReferences;

        public DotNetProjectContext(ProjectContext projectContext, string configuration, string outputPath, IEnumerable<ProjectReferenceInformation> projectReferenceInformation)
        {
            if (projectContext == null)
            {
                throw new ArgumentNullException(nameof(projectContext));
            }

            if (string.IsNullOrEmpty(configuration))
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _rawProject = new Lazy<JObject>(() =>
                {
                    using (var stream = new FileStream(projectContext.ProjectFile.ProjectFilePath, FileMode.Open, FileAccess.Read))
                    using (var streamReader = new StreamReader(stream))
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        return JObject.Load(jsonReader);
                    }
                });

            Configuration = configuration;
            _projectContext = projectContext;

            _paths = projectContext.GetOutputPaths(configuration, buidBasePath: null, outputPath: outputPath);
            _compilerOptions = _projectContext.ProjectFile.GetCompilerOptions(_projectContext.TargetFramework, Configuration);

            // Workaround https://github.com/dotnet/cli/issues/3164
            IsClassLibrary = !(_compilerOptions?.EmitEntryPoint
                    ?? projectContext.ProjectFile.GetCompilerOptions(null, configuration).EmitEntryPoint.GetValueOrDefault());

            _dependencyProvider = new Lazy<DotNetDependencyProvider>(() => new DotNetDependencyProvider(_projectContext));
            ProjectReferenceInformation = projectReferenceInformation;
        }

        public bool IsClassLibrary { get; }

        public string TargetFramework => _projectContext.TargetFramework.GetShortFolderName();
        public string Config => _paths.RuntimeFiles.Config;
        public string PackagesDirectory => _projectContext.PackagesDirectory;
        public string AssemblyName => string.IsNullOrEmpty(AssemblyFullPath)
            ? ProjectName
            : Path.GetFileNameWithoutExtension(AssemblyFullPath);
        public string AssemblyFullPath =>
            !IsClassLibrary && (_projectContext.IsPortable || _projectContext.TargetFramework.IsDesktop())
                ? _paths.RuntimeFiles.Executable
                : _paths.RuntimeFiles.Assembly;

        public string Configuration { get; }
        public string ProjectFullPath => _projectContext.ProjectFile.ProjectFilePath;
        public string ProjectName => _projectContext.ProjectFile.Name;
        // TODO read from xproj if available
        public string RootNamespace => _projectContext.ProjectFile.Name;
        public string TargetDirectory => _paths.RuntimeOutputPath;
        public string Platform => _compilerOptions?.Platform;

        public IEnumerable<string> CompilationItems
            => (_compilerOptions?.CompileInclude == null)
            ? _projectContext.ProjectFile.Files.SourceFiles
            : _compilerOptions.CompileInclude.ResolveFiles();

        public IEnumerable<string> EmbededItems
            => (_compilerOptions?.EmbedInclude == null)
            ? _projectContext.ProjectFile.Files.ResourceFiles.Keys
            : _compilerOptions.EmbedInclude.ResolveFiles();

        public IEnumerable<DependencyDescription> PackageDependencies
        {
            get
            {
                if (_packageDependencies == null)
                {
                    _packageDependencies = _dependencyProvider.Value.GetPackageDependencies();
                }

                return _packageDependencies;
            }
        }

        public IEnumerable<ResolvedReference> CompilationAssemblies
        {
            get
            {
                if (_compilationAssemblies == null)
                {
                    _compilationAssemblies = _dependencyProvider.Value.GetResolvedReferences();
                }

                return _compilationAssemblies;
            }
        }

        public IEnumerable<string> ProjectReferences
        {
            get
            {
                if (_projectReferences == null)
                {
                    _projectReferences = _dependencyProvider.Value.GetProjectReferences();
                }

                return _projectReferences;
            }
        }

        public IEnumerable<ProjectReferenceInformation> ProjectReferenceInformation { get; set; }
    }
}