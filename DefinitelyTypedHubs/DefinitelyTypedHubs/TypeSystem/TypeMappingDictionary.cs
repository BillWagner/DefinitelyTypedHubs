using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefinitelyTypedHubs.TypeSystem
{
    public class TypeMappingDictionary
    {
        private Dictionary<string, CSharpToTypeScriptMapping> typeMappings = new Dictionary<string, CSharpToTypeScriptMapping>();

        public TypeMappingDictionary()
        {
            addStandardMappings();
        }


        private void addStandardMappings()
        {
            typeMappings.Add("object", new CSharpToTypeScriptMapping("object", "any"));
        }

        public string FindOrAdd(string cSharpReturnType)
        {
            // look to see if it's already created:
            if (typeMappings.ContainsKey(cSharpReturnType))
                return typeMappings[cSharpReturnType].TypeScriptName;

            // TODO:  More must happen here.
            return cSharpReturnType;
        }
    }
}
