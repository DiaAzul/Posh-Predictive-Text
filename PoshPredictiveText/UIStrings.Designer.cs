﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PoshPredictiveText {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class UIStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UIStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PoshPredictiveText.UIStrings", typeof(UIStrings).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to List commands supported with tab-expansion of arguments.
        /// </summary>
        public static string HELP_LIST_COMMANDS {
            get {
                return ResourceManager.GetString("HELP_LIST_COMMANDS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This is the help text.
        /// </summary>
        public static string HELP_TEXT {
            get {
                return ResourceManager.GetString("HELP_TEXT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///Resolve-Argument provides tab-completion for the following commands:
        ///
        ///    Conda
        ///    Git
        ///    Mamba
        ///    Pip
        ///    Python
        ///.
        /// </summary>
        public static string LIST_OF_COMMANDS {
            get {
                return ResourceManager.GetString("LIST_OF_COMMANDS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resolve-Argument logile created..
        /// </summary>
        public static string LOGFILE_CREATED_HEADER {
            get {
                return ResourceManager.GetString("LOGFILE_CREATED_HEADER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Directory for the log file does not exist..
        /// </summary>
        public static string LOGGER_NO_DIRECTORY {
            get {
                return ResourceManager.GetString("LOGGER_NO_DIRECTORY", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The log level parameter must be INFO, WARN, ERROR. Use Get-Error for more detail..
        /// </summary>
        public static string LOGGER_NOT_VALID_LEVEL {
            get {
                return ResourceManager.GetString("LOGGER_NOT_VALID_LEVEL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The path for the log file is not valid. Use Get-Error for more detail..
        /// </summary>
        public static string LOGGER_NOT_VALID_PATH {
            get {
                return ResourceManager.GetString("LOGGER_NOT_VALID_PATH", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to create log file. Use Get-Error for more detail..
        /// </summary>
        public static string LOGGER_UNABLE_TO_CREATE_LOGFILE {
            get {
                return ResourceManager.GetString("LOGGER_UNABLE_TO_CREATE_LOGFILE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # Remove conda tab-expansion if installed.
        ///if (Test-Path Function:\TabExpansion) {
        ///    $testForConda = Get-Item Function:\TabExpansion
        ///    if ($testForConda.Source -eq &quot;conda&quot;) {
        ///        Remove-Item Function:\TabExpansion
        ///        if (Test-Path Function:\CondaTabExpansionBackup) {
        ///            Rename-Item Function:\CondaTabExpansionBackup Function:\TabExpansion
        ///        }
        ///    }
        ///}
        ///Register-ArgumentCompleter -CommandName $cmdNames -Native -ScriptBlock {
        ///    param(
        ///        [string]$wordToComplete, 
        ///  [rest of string was truncated]&quot;;.
        /// </summary>
        public static string REGISTER_COMMAND_SCRIPT {
            get {
                return ResourceManager.GetString("REGISTER_COMMAND_SCRIPT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0.1.0.
        /// </summary>
        public static string VERSION {
            get {
                return ResourceManager.GetString("VERSION", resourceCulture);
            }
        }
    }
}