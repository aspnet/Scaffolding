﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.VisualStudio.Web.CodeGenerators.Mvc {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class MessageStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal MessageStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.VisualStudio.Web.CodeGenerators.Mvc.MessageStrings", typeof(MessageStrings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Added Controller : &apos;{0}&apos;..
        /// </summary>
        internal static string AddedController {
            get {
                return ResourceManager.GetString("AddedController", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There are probably still some manual steps required..
        /// </summary>
        internal static string AdditionalStepsRequired {
            get {
                return ResourceManager.GetString("AdditionalStepsRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Checkout the &apos;{0}&apos; file that got generated..
        /// </summary>
        internal static string CheckoutReadMe {
            get {
                return ResourceManager.GetString("CheckoutReadMe", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to compile the project in memory{0}{1}.
        /// </summary>
        internal static string CompilationFailedMessage {
            get {
                return ResourceManager.GetString("CompilationFailedMessage", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Controller Name is required.
        /// </summary>
        internal static string ControllerNameRequired {
            get {
                return ResourceManager.GetString("ControllerNameRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DbContext type '{0}' is found but it does not inherit from '{1}'.
        /// </summary>
        internal static string DbContextNeedsToInheritFromIdentityContextMessage {
            get {
                return ResourceManager.GetString("DbContextNeedsToInheritFromIdentityContextMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DbContext class to use..
        /// </summary>
        internal static string DbContextOptionDesc {
            get {
                return ResourceManager.GetString("DbContextOptionDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Switch to specify that default layout should be used for the views..
        /// </summary>
        internal static string DefaultLayoutSwitchDesc {
            get {
                return ResourceManager.GetString("DefaultLayoutSwitchDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Controller name is required for an Empty Controller.
        /// </summary>
        internal static string EmptyControllerNameRequired {
            get {
                return ResourceManager.GetString("EmptyControllerNameRequired", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Cannot use an existing DbContext with the '--useDefaultUI' option..
        /// </summary>
        internal static string ExistingDbContextCannotBeUsedForDefaultUI {
            get {
                return ResourceManager.GetString("ExistingDbContextCannotBeUsedForDefaultUI", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The file {0} exists, use -f option to overwrite.
        /// </summary>
        internal static string FileExists_useforce {
            get {
                return ResourceManager.GetString("FileExists_useforce", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Specify whether to overwrite existing files..
        /// </summary>
        internal static string ForceOptionDesc {
            get {
                return ResourceManager.GetString("ForceOptionDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To scaffold controllers and views using models please install Entity Framework core packages and try again: {0}.
        /// </summary>
        internal static string InstallEfPackages {
            get {
                return ResourceManager.GetString("InstallEfPackages", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please install the following packages to your project for scaffolding identity: {0}.
        /// </summary>
        internal static string InstallPackagesForScaffoldingIdentity {
            get {
                return ResourceManager.GetString("InstallPackagesForScaffoldingIdentity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please install the below packages to your project:.
        /// </summary>
        internal static string InstallPackageMessage {
            get {
                return ResourceManager.GetString("InstallPackageMessage", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Value of --dbContext '{0}' is not a valid class name..
        /// </summary>
        internal static string InvalidDbContextClassName {
            get {
                return ResourceManager.GetString("InvalidDbContextClassName", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Could not find the files below. (Please use '--listFiles' to check the list of available files).
        /// </summary>
        internal static string InvalidFilesListMessage {
            get {
                return ResourceManager.GetString("InvalidFilesListMessage", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Option '{0}' cannot be used with '{1}'..
        /// </summary>
        internal static string InvalidOptionCombination {
            get {
                return ResourceManager.GetString("InvalidOptionCombination", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Value of --userClass '{0}' is not a valid class name..
        /// </summary>
        internal static string InvalidUserClassName {
            get {
                return ResourceManager.GetString("InvalidUserClassName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The namespace name &apos;{0}&apos; is not valid..
        /// </summary>
        internal static string InvalidNamespaceName {
            get {
                return ResourceManager.GetString("InvalidNamespaceName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The template name &apos;{0}&apos; is not valid. Supported view templates: &apos;Empty|Create|Edit|Delete|Details|List&apos;&quot;.
        /// </summary>
        internal static string InvalidViewTemplateName {
            get {
                return ResourceManager.GetString("InvalidViewTemplateName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Custom Layout page to use..
        /// </summary>
        internal static string LayoutOptionsDesc {
            get {
                return ResourceManager.GetString("LayoutOptionsDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Model class to use..
        /// </summary>
        internal static string ModelClassOptionDesc {
            get {
                return ResourceManager.GetString("ModelClassOptionDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;{0}&apos; template cannot be used without specifying a model class. Please provide a model using &apos;--model&apos; option..
        /// </summary>
        internal static string ModelClassRequiredForTemplate {
            get {
                return ResourceManager.GetString("ModelClassRequiredForTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Multiple types matching the name {0} exist:{1}, please use a fully qualified name&quot;.
        /// </summary>
        internal static string MultipleTypesMatchingName {
            get {
                return ResourceManager.GetString("MultipleTypesMatchingName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Specify the name of the namespace to use for the generated controller..
        /// </summary>
        internal static string NamespaceOptionDesc {
            get {
                return ResourceManager.GetString("NamespaceOptionDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Specify the relative output folder path from project where the file needs to be generated, if not specified, file will be generated in the project folder..
        /// </summary>
        internal static string OutDirOptionDesc {
            get {
                return ResourceManager.GetString("OutDirOptionDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;--noPageModel&apos; flag cannot be used for scaffolding model based Razor Pages..
        /// </summary>
        internal static string PageModelFlagNotSupported {
            get {
                return ResourceManager.GetString("PageModelFlagNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please provide a valid {0}.
        /// </summary>
        internal static string ProvideValidArgument {
            get {
                return ResourceManager.GetString("ProvideValidArgument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Razor Page Name is required..
        /// </summary>
        internal static string RazorPageNameRequired {
            get {
                return ResourceManager.GetString("RazorPageNameRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to generate a readme: {0}.
        /// </summary>
        internal static string ReadmeGenerationFailed {
            get {
                return ResourceManager.GetString("ReadmeGenerationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There may be additional steps required for the generated code to work. Refer to &lt;forward-link&gt;.
        /// </summary>
        internal static string Scaffolding_additionalSteps {
            get {
                return ResourceManager.GetString("Scaffolding_additionalSteps", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Scaffolding generated all the code but the new context created could not be registered using dependency injection..
        /// </summary>
        internal static string ScaffoldingSuccessful_unregistered {
            get {
                return ResourceManager.GetString("ScaffoldingSuccessful_unregistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Switch to specify whether to reference script libraries in the generated views..
        /// </summary>
        internal static string ScriptsOptionDesc {
            get {
                return ResourceManager.GetString("ScriptsOptionDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to generate readme file at &apos;{0}&apos;..
        /// </summary>
        internal static string String1 {
            get {
                return ResourceManager.GetString("String1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The TemplateName cannot be empty.
        /// </summary>
        internal static string TemplateNameRequired {
            get {
                return ResourceManager.GetString("TemplateNameRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A type with the name {0} does not exist.
        /// </summary>
        internal static string TypeDoesNotExist {
            get {
                return ResourceManager.GetString("TypeDoesNotExist", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The below files exist. Use '--force' to overwrite: {0}.
        /// </summary>
        internal static string UseForceOption {
            get {
                return ResourceManager.GetString("UseForceOption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to '--userClass' cannot be used to specify a user class when using an existing DbContext..
        /// </summary>
        internal static string UserClassAndDbContextCannotBeSpecifiedTogether {
            get {
                return ResourceManager.GetString("UserClassAndDbContextCannotBeSpecifiedTogether", resourceCulture);
            }
        }
        
         /// <summary>
        ///   Looks up a localized string similar to Could not determine the user class from the DbContext class '{0}'.
        /// </summary>
        internal static string UserClassCouldNotBeDetermined {
            get {
                return ResourceManager.GetString("UserClassCouldNotBeDetermined", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Validation succeded but model type not set.
        /// </summary>
        internal static string ValidationSuccessfull_modelUnset {
            get {
                return ResourceManager.GetString("ValidationSuccessfull_modelUnset", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The ViewName cannot be empty..
        /// </summary>
        internal static string ViewNameRequired {
            get {
                return ResourceManager.GetString("ViewNameRequired", resourceCulture);
            }
        }
    }
}
