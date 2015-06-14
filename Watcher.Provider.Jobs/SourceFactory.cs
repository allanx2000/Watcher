using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions.V2;

namespace Watcher.Provider.Jobs
{
    /// <summary>
    /// Logic for selecting the specific Providers
    /// </summary>
    public static class SourceFactory
    {
        /// <summary>
        /// Passes the call to the actual Provider that creates it
        /// </summary>
        /// <param name="name"></param>
        /// <param name="metaData"></param>
        /// <returns></returns>
        public static AbstractSource CreateSource(string name, List<MetaDataObject> metaData)
        {
            var type = metaData.FirstOrDefault(x => x.ID == JobsProvider.META_SOURCE);

            if (type == null)
                throw new Exception("Source is was not specified");

            string typeString = type.Value.ToString();

            switch (typeString)
            {
                case JobsProvider.SourceNames.Dice:
                    return DiceProvider.DoCreateNewSource(name, metaData);
                case JobsProvider.SourceNames.Monster:
                    return MonsterProvider.DoCreateNewSource(name, metaData);
                default:
                    throw new NotSupportedException(typeString);
            }
        }
    }
}
