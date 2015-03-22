using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Watcher.Extensions
{
    public struct SourceOptions
    {
        public bool HasUniqueName { get; private set; }
        public bool HasURLField {get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hasUniqueName">Whether multiple types of these sources can be created, each with a unique name</param>
        /// <param name="hasURLField">Has a URL parameter (show the URL input component)</param>
        /// <returns></returns>
        public static SourceOptions CreateFromParameters(bool hasUniqueName = true, bool hasURLField = true)
        {
            SourceOptions so = new SourceOptions();
            so.HasURLField = hasURLField;
            so.HasUniqueName = hasUniqueName;

            return so;
        }
    }
}
