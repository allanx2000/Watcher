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

        public static SourceOptions CreateFromParameters(bool hasUniqueName = true, bool hasURLField = true)
        {
            SourceOptions so = new SourceOptions();
            so.HasURLField = hasURLField;
            so.HasUniqueName = hasUniqueName;

            return so;
        }
    }
}
