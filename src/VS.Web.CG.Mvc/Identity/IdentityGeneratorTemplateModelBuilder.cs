// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.ProjectModel;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;

namespace Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Identity
{
    internal class IdentityGeneratorTemplateModelBuilder
    {
        private IdentityGeneratorCommandLineModel _commandlineModel;
        private IApplicationInfo _applicationInfo;
        private IProjectContext _projectContext;
        private Workspace _workspace;
        private ICodeGenAssemblyLoadContext _loader;
        private IFileSystem _fileSystem;
        private ILogger _logger;

        private ReflectedTypesProvider _reflectedTypesProvider;

        public IdentityGeneratorTemplateModelBuilder(
            IdentityGeneratorCommandLineModel commandlineModel,
            IApplicationInfo applicationInfo,
            IProjectContext projectContext,
            Workspace workspace,
            ICodeGenAssemblyLoadContext loader,
            IFileSystem fileSystem,
            ILogger logger)
        {
            if (commandlineModel == null)
            {
                throw new ArgumentNullException(nameof(commandlineModel));
            }

            if (applicationInfo == null)
            {
                throw new ArgumentNullException(nameof(applicationInfo));
            }

            if (projectContext == null)
            {
                throw new ArgumentNullException(nameof(projectContext));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (loader == null)
            {
                throw new ArgumentNullException(nameof(loader));
            }

            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _commandlineModel = commandlineModel;
            _applicationInfo = applicationInfo;
            _projectContext = projectContext;
            _workspace = workspace;
            _loader = loader;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        internal bool IsFilesSpecified => !string.IsNullOrEmpty(_commandlineModel.Files);
        internal bool IsDbContextSpecified => !string.IsNullOrEmpty(_commandlineModel.DbContext);
        internal bool IsUsingExistingDbContext { get; set; }

        private Type _userType;

        internal string UserClass { get; private set; }
        internal string UserClassNamespace { get; private set; }

        internal Type UserType 
        {
            get
            {
                return _userType;
            }
            set
            {
                _userType = value;
                UserClass = _userType?.Name;
                UserClassNamespace = _userType?.Namespace;
            }
        }

        internal string DbContextClass { get; private set; }
        internal string DbContextNamespace { get; private set; }
        internal string RootNamespace { get; private set; }
        internal bool IsGenerateCustomUser { get; private set; }
        internal IdentityGeneratorFile[] FilesToGenerate { get; private set; }
        internal IEnumerable<string> NamedFiles { get; private set; }

        public async Task<IdentityGeneratorTemplateModel> ValidateAndBuild()
        {
            ValidateCommandLine(_commandlineModel);
            RootNamespace = string.IsNullOrEmpty(_commandlineModel.RootNamespace)
                ? _projectContext.RootNamespace
                : _commandlineModel.RootNamespace;

            ValidateRequiredDependencies(_commandlineModel.UseSQLite);

            var defaultDbContextNamespace = $"{RootNamespace}.Areas.Identity.Data";

            IsUsingExistingDbContext = false;
            if (IsDbContextSpecified)
            {
                var existingDbContext = await FindExistingType(_commandlineModel.DbContext);
                if (existingDbContext == null)
                {
                    // We need to create one with what the user specified.
                    DbContextClass = GetClassNameFromTypeName(_commandlineModel.DbContext);
                    DbContextNamespace = GetNamespaceFromTypeName(_commandlineModel.DbContext)
                        ?? defaultDbContextNamespace;
                }
                else
                {
                    ValidateExistingDbContext(existingDbContext);
                    IsGenerateCustomUser = false;
                    IsUsingExistingDbContext = true;
                    UserType = FindUserTypeFromDbContext(existingDbContext);
                    DbContextClass = existingDbContext.Name;
                    DbContextNamespace = existingDbContext.Namespace;
                }
            }
            else
            {
                // --dbContext paramter was not specified. So we need to generate one using convention.
                DbContextClass = GetDefaultDbContextName();
                DbContextNamespace = defaultDbContextNamespace;
            }

            // if an existing user class was determined from the DbContext, don't try to get it from here.
            // Identity scaffolding must use the user class tied to the existing DbContext (when there is one).
            if (string.IsNullOrEmpty(UserClass))
            {
                if (string.IsNullOrEmpty(_commandlineModel.UserClass))
                {
                    IsGenerateCustomUser = false;
                    UserClass = "IdentityUser";
                    UserClassNamespace = "Microsoft.AspNetCore.Identity";
                }
                else
                {
                    var existingUser = await FindExistingType(_commandlineModel.UserClass);
                    if (existingUser != null)
                    {
                        ValidateExistingUserType(existingUser);
                        IsGenerateCustomUser = false;
                        UserType = existingUser;
                    }
                    else
                    {
                        IsGenerateCustomUser = true;
                        UserClass = GetClassNameFromTypeName(_commandlineModel.UserClass);
                        UserClassNamespace = GetNamespaceFromTypeName(_commandlineModel.UserClass)
                            ?? defaultDbContextNamespace;
                    }
                }
            }

            if (_commandlineModel.UseDefaultUI)
            {
                ValidateDefaultUIOption();
            }

            if (IsFilesSpecified)
            {
                ValidateFilesOption();
            }

            bool hasExistingLayout = DetermineSupportFileLocation(out string supportFileLocation, out string layout);
            
            var templateModel = new IdentityGeneratorTemplateModel()
            {
                ApplicationName = _applicationInfo.ApplicationName,
                DbContextClass = DbContextClass,
                DbContextNamespace = DbContextNamespace,
                UserClass = UserClass,
                UserClassNamespace = UserClassNamespace,
                UseSQLite = _commandlineModel.UseSQLite,
                IsUsingExistingDbContext = IsUsingExistingDbContext,
                Namespace = RootNamespace,
                IsGenerateCustomUser = IsGenerateCustomUser,
                IsGeneratingIndividualFiles = IsFilesSpecified,
                UseDefaultUI = _commandlineModel.UseDefaultUI,
                GenerateLayout = !hasExistingLayout,
                Layout = layout,
                LayoutPageNoExtension = Path.GetFileNameWithoutExtension(layout),
                SupportFileLocation = supportFileLocation,
                HasExistingNonEmptyWwwRoot = HasExistingNonEmptyWwwRootDirectory
            };

            var filesToGenerate = new List<IdentityGeneratorFile>(IdentityGeneratorFilesConfig.GetFilesToGenerate(NamedFiles, templateModel));

            // Check if we need to add ViewImports and which ones.
            if (!_commandlineModel.UseDefaultUI)
            {
                filesToGenerate.AddRange(IdentityGeneratorFilesConfig.GetViewImports(filesToGenerate, _fileSystem, _applicationInfo.ApplicationBasePath));
            }

            if (IdentityGeneratorFilesConfig.TryGetLayoutPeerFiles(_fileSystem, _applicationInfo.ApplicationBasePath, templateModel, out IReadOnlyList<IdentityGeneratorFile> layoutPeerFiles))
            {
                filesToGenerate.AddRange(layoutPeerFiles);
            }

            if (IdentityGeneratorFilesConfig.TryGetCookieConsentPartialFile(_fileSystem, _applicationInfo.ApplicationBasePath, templateModel, out IdentityGeneratorFile cookieConsentPartialConfig))
            {
                filesToGenerate.Add(cookieConsentPartialConfig);
            }

            templateModel.FilesToGenerate = filesToGenerate.ToArray();

            ValidateFilesToGenerate(templateModel.FilesToGenerate);

            return templateModel;
        }

        private static readonly IReadOnlyList<string> _ExistingLayoutFileCheckLocations = new List<string>()
        {
            "Pages/Shared/",
            "Views/Shared/"
        };

        // If there is no layout file, check the existence of the key directories, and put the support files in the value directory.
        private static readonly IReadOnlyDictionary<string, string> _CheckDirectoryToTargetMapForSupportFiles = new Dictionary<string, string>()
        {
            { "Pages/", "Pages/Shared/" },
            { "Views/", "Views/Shared/" }
        };

        internal static readonly string _DefaultSupportLocation = "Pages/Shared/";

        internal static readonly string _LayoutFileName = "_Layout.cshtml";

        // Checks if there is an existing layout page, and based on its location or lack of existence, determines where to put support pages.
        // Returns true if there is an existing layout page.
        // Note: layoutFile & supportFileLocation will always have a value when this exits.
        //      supportFileLocation is rooted
        internal bool DetermineSupportFileLocation(out string supportFileLocation, out string layoutFile)
        {
            string projectDir = Path.GetDirectoryName(_projectContext.ProjectFullPath);

            if (!string.IsNullOrEmpty(_commandlineModel.Layout))
            {
                if (_commandlineModel.Layout.StartsWith("~"))
                {
                    layoutFile = _commandlineModel.Layout.Substring(1);
                }
                else
                {
                    layoutFile = _commandlineModel.Layout;
                }

                while (!string.IsNullOrEmpty(layoutFile) &&
                    (layoutFile[0] == Path.DirectorySeparatorChar ||
                    layoutFile[0] == Path.AltDirectorySeparatorChar))
                {
                    layoutFile = layoutFile.Substring(1);
                }

                // if the input layout file path consists of only slashes (and possibly a lead ~), it'll be empty at this point.
                // So we'll treat it as if no layout file was specified (handled below).
                if (!string.IsNullOrEmpty(layoutFile))
                {
                    // normalize the path characters sp GetDirectoryName() works.
                    layoutFile = layoutFile.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                    supportFileLocation = Path.GetDirectoryName(layoutFile);

                    // always use forward slashes for the layout file path.
                    layoutFile = layoutFile.Replace("\\", "/");

                    return true;
                }
            }

            bool hasExistingLayoutFile = false;
            supportFileLocation = null;
            layoutFile = null;

            foreach (string checkDirectory in _ExistingLayoutFileCheckLocations)
            {
                string checkFile = Path.Combine(projectDir, checkDirectory, _LayoutFileName);
                if (_fileSystem.FileExists(checkFile))
                {
                    hasExistingLayoutFile = true;
                    supportFileLocation = checkDirectory;
                    layoutFile = Path.Combine(supportFileLocation, _LayoutFileName);
                    break;
                }
            }

            if (string.IsNullOrEmpty(supportFileLocation))
            {
                foreach (KeyValuePair<string, string> checkMapEntry in _CheckDirectoryToTargetMapForSupportFiles)
                {
                    string checkDirectory = Path.Combine(projectDir, checkMapEntry.Key);
                    if (_fileSystem.DirectoryExists(checkDirectory))
                    {
                        supportFileLocation = checkMapEntry.Value;
                        layoutFile = Path.Combine(supportFileLocation, _LayoutFileName);
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(supportFileLocation))
            {
                supportFileLocation = _DefaultSupportLocation;
                layoutFile = Path.Combine(supportFileLocation, _LayoutFileName);
            }

            return hasExistingLayoutFile;
        }

        private void ValidateFilesToGenerate(IdentityGeneratorFile[] filesToGenerate)
        {
            var rootPath = _applicationInfo.ApplicationBasePath;
            var filesToOverWrite = filesToGenerate
                .Where(f => f.ShouldOverWrite == OverWriteCondition.WithForce
                                && _fileSystem.FileExists(Path.Combine(rootPath, f.OutputPath)));

            if (filesToOverWrite.Any() && !_commandlineModel.Force)
            {
                var msg = string.Format(
                        MessageStrings.UseForceOption,
                        string.Join(Environment.NewLine, filesToOverWrite.Select(f => f.OutputPath)));
                throw new InvalidOperationException(msg);
            }
        }

        // returns true if, at the project root, there is a wwwroot directory that contains at least 1 file.
        // return false otherwise.
        private bool HasExistingNonEmptyWwwRootDirectory
        {
            get
            {
                string projectDir = Path.GetDirectoryName(_projectContext.ProjectFullPath);
                string wwwrootCheckLocation = Path.Combine(projectDir, "wwwroot");

                return _fileSystem.DirectoryExists(wwwrootCheckLocation)
                    && _fileSystem.EnumerateFiles(wwwrootCheckLocation, "*", SearchOption.AllDirectories).Any();
            }
        }

        private void ValidateDefaultUIOption()
        {
            var errorStrings = new List<string>();

            if (IsFilesSpecified)
            {
                errorStrings.Add(string.Format(MessageStrings.InvalidOptionCombination,"--files", "--useDefaultUI"));
            }

            if (IsUsingExistingDbContext)
            {
                errorStrings.Add(MessageStrings.ExistingDbContextCannotBeUsedForDefaultUI);
            }

            if (errorStrings.Any())
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, errorStrings));
            }
        }

        private void ValidateFilesOption()
        {
            var errors = new List<string>();

            NamedFiles = _commandlineModel.Files.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var invalidFiles = NamedFiles.Where(f => !IdentityGeneratorFilesConfig.GetFilesToList().Contains(f));

            if (invalidFiles.Any())
            {
                errors.Add(MessageStrings.InvalidFilesListMessage);
                errors.AddRange(invalidFiles);
            }

            if (errors.Any())
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
            }
        }

        private string GetDefaultDbContextName()
        {
            var defaultDbContextName = $"{_applicationInfo.ApplicationName}IdentityDbContext";

            if (!RoslynUtilities.IsValidIdentifier(defaultDbContextName))
            {
                defaultDbContextName = "IdentityDataContext";
            }

            return defaultDbContextName;
        }

        private string GetNamespaceFromTypeName(string dbContext)
        {
            if (dbContext.LastIndexOf('.') == -1)
            {
                return null;
            }

            return dbContext.Substring(0, dbContext.LastIndexOf('.'));
        }

        private string GetClassNameFromTypeName(string dbContext)
        {
            return dbContext.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries).Last();
        }

        private void ValidateExistingDbContext(Type existingDbContext)
        {
            var errorStrings = new List<string>();

            // Validate that the dbContext inherits from IdentityDbContext.
            bool foundValidParentDbContextClass = IsTypeDerivedFromIdentityDbContext(existingDbContext);

            if (!foundValidParentDbContextClass)
            {
                errorStrings.Add(
                    string.Format(MessageStrings.DbContextNeedsToInheritFromIdentityContextMessage,
                        existingDbContext.Name,
                        "Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext"));
            }

            // Validate that the `--userClass` parameter is not passed.
            if (!string.IsNullOrEmpty(_commandlineModel.UserClass))
            {
                errorStrings.Add(MessageStrings.UserClassAndDbContextCannotBeSpecifiedTogether);
            }

            if (errorStrings.Any())
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, errorStrings));
            }
        }

        private void ValidateExistingUserType(Type existingUser)
        {
            var errorStrings = new List<string>();

            // Validate that the user type inherits from IdentityUser
            bool foundValidParentDbContextClass = IsTypeDerivedFromIdentityUser(existingUser);

            if (!foundValidParentDbContextClass)
            {
                errorStrings.Add(
                    string.Format(MessageStrings.DbContextNeedsToInheritFromIdentityContextMessage,
                        existingUser.Name,
                        "Microsoft.AspNetCore.Identity.IdentityUser"));
            }

            if (errorStrings.Any())
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, errorStrings));
            }
        }

        private static bool IsTypeDerivedFromIdentityUser(Type type)
        {
            var parentType = type.BaseType;
            while (parentType != null && parentType != typeof(object))
            {
                if (parentType.FullName == "Microsoft.AspNetCore.Identity.IdentityUser"
                    && parentType.Assembly.GetName().Name == "Microsoft.Extensions.Identity.Stores")
                {
                    return true;
                }

                parentType = parentType.BaseType;
            }

            return false;
        }

        private static bool IsTypeDerivedFromIdentityDbContext(Type type)
        {
            var parentType = type.BaseType;
            while (parentType != null && parentType != typeof(object))
            {
                // There are multiple variations of IdentityDbContext classes.
                // So have to use StartsWith instead of comparing names.
                // 1. IdentityDbContext
                // 2. IdentityDbContext <TUser, TRole, TKey>
                // 3. IdentityDbContext <TUser, TRole, string> 
                // 4. IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> etc.
                if (parentType.Name.StartsWith("IdentityDbContext")
                    && parentType.Namespace == "Microsoft.AspNetCore.Identity.EntityFrameworkCore"
                    && parentType.Assembly.GetName().Name == "Microsoft.AspNetCore.Identity.EntityFrameworkCore")
                {
                    return true;
                }

                parentType = parentType.BaseType;
            }

            return false;
        }

        private Type FindUserTypeFromDbContext(Type existingDbContext)
        {
            var usersProperty = existingDbContext.GetProperties()
                .FirstOrDefault(p => p.Name == "Users");

            if (usersProperty == null 
                || !usersProperty.PropertyType.IsGenericType
                || usersProperty.PropertyType.GetGenericArguments().Count() != 1)
            {
                // The IdentityDbContext has DbSet<UserType> Users property.
                // The only case this would happen is if the user hides the inherited property.
                throw new InvalidOperationException(
                    string.Format(MessageStrings.UserClassCouldNotBeDetermined,
                        existingDbContext.Name));
            }

            return usersProperty.PropertyType.GetGenericArguments().First();
        }

        private async Task<Type> FindExistingType(string type)
        {
            if (_reflectedTypesProvider == null)
            {
                var compilation = await _workspace.CurrentSolution.Projects
                    .Where(p => p.AssemblyName == _projectContext.AssemblyName)
                    .First()
                    .GetCompilationAsync();

                _reflectedTypesProvider = new ReflectedTypesProvider(
                    compilation,
                    null,
                    _projectContext,
                    _loader,
                    _logger);

                if (_reflectedTypesProvider.GetCompilationErrors() != null
                    && _reflectedTypesProvider.GetCompilationErrors().Any())
                {
                    // Failed to build the project.
                    throw new InvalidOperationException(
                        string.Format(MessageStrings.CompilationFailedMessage,
                            Environment.NewLine,
                            string.Join(Environment.NewLine, _reflectedTypesProvider.GetCompilationErrors())));
                }
            }

            var reflectedType = _reflectedTypesProvider.GetReflectedType(type, true);

            return reflectedType;
        }

        private void ValidateCommandLine(IdentityGeneratorCommandLineModel model)
        {
            var errorStrings = new List<string>();;
            if (!string.IsNullOrEmpty(model.UserClass) && !RoslynUtilities.IsValidNamespace(model.UserClass))
            {
                errorStrings.Add(string.Format(MessageStrings.InvalidUserClassName, model.UserClass));;
            }

            if (!string.IsNullOrEmpty(model.DbContext) && !RoslynUtilities.IsValidNamespace(model.DbContext))
            {
                errorStrings.Add(string.Format(MessageStrings.InvalidDbContextClassName, model.DbContext));;
            }

            if (!string.IsNullOrEmpty(model.RootNamespace) && !RoslynUtilities.IsValidNamespace(model.RootNamespace))
            {
                errorStrings.Add(string.Format(MessageStrings.InvalidNamespaceName, model.RootNamespace));
            }

            if (!string.IsNullOrEmpty(model.Layout) && model.GenerateLayout)
            {
                errorStrings.Add(string.Format(MessageStrings.InvalidOptionCombination,"--layout", "--generateLayout"));
            }

            if (errorStrings.Any())
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errorStrings));
            }
        }

        private void ValidateRequiredDependencies(bool useSqlite)
        {

            var dependencies = new HashSet<string>()
            {
                "Microsoft.AspNetCore.Identity.UI",
                "Microsoft.EntityFrameworkCore.Design"
            };

            const string EfDesignPackageName = "Microsoft.EntityFrameworkCore.Design";
            var isEFDesignPackagePresent = _projectContext
                .PackageDependencies
                .Any(package => package.Name.Equals(EfDesignPackageName, StringComparison.OrdinalIgnoreCase));

            if (useSqlite)
            {
                dependencies.Add("Microsoft.EntityFrameworkCore.Sqlite");
            }
            else
            {
                dependencies.Add("Microsoft.EntityFrameworkCore.SqlServer");
            }

            var missingPackages = dependencies.Where(d => !_projectContext.PackageDependencies.Any(p => p.Name.Equals(d, StringComparison.OrdinalIgnoreCase)));

            if (missingPackages.Any())
            {
                throw new InvalidOperationException(
                    string.Format(MessageStrings.InstallPackagesForScaffoldingIdentity, string.Join(",", missingPackages)));
            }
        }
    }
}