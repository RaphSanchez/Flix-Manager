﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BlazorMovies.Shared.Resources {
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
    public class MovieFormResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal MovieFormResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BlazorMovies.Shared.Resources.MovieFormResources", typeof(MovieFormResources).Assembly);
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
        ///   Looks up a localized string similar to Actor Not Found.
        /// </summary>
        public static string Actor_Not_Found {
            get {
                return ResourceManager.GetString("Actor Not Found", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Actors.
        /// </summary>
        public static string Actors {
            get {
                return ResourceManager.GetString("Actors", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Genres.
        /// </summary>
        public static string Genres {
            get {
                return ResourceManager.GetString("Genres", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to In Theathers.
        /// </summary>
        public static string In_Theaters {
            get {
                return ResourceManager.GetString("In Theaters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot be in theaters if release date is in the future..
        /// </summary>
        public static string InTheaters_Validation {
            get {
                return ResourceManager.GetString("InTheaters_Validation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Poster.
        /// </summary>
        public static string Poster {
            get {
                return ResourceManager.GetString("Poster", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Release Date.
        /// </summary>
        public static string Release_Date {
            get {
                return ResourceManager.GetString("Release Date", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value for {0} must be between {1} and {2}..
        /// </summary>
        public static string ReleaseDate_Validation {
            get {
                return ResourceManager.GetString("ReleaseDate_Validation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Summary.
        /// </summary>
        public static string Summary {
            get {
                return ResourceManager.GetString("Summary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The {0} field must be between {2} and {1}..
        /// </summary>
        public static string Summary_Validation {
            get {
                return ResourceManager.GetString("Summary_Validation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Title.
        /// </summary>
        public static string Title {
            get {
                return ResourceManager.GetString("Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The {0} field must be between {2} and {1}..
        /// </summary>
        public static string Title_Validation {
            get {
                return ResourceManager.GetString("Title_Validation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Trailer.
        /// </summary>
        public static string Trailer {
            get {
                return ResourceManager.GetString("Trailer", resourceCulture);
            }
        }
    }
}