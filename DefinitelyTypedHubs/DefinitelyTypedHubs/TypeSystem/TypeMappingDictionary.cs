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
            // Because I use the semantic model, I don't need to worry
            // about the different 'spellings' of types (e.g. 'int', 'Int32', and so on).

            // Void to void mapping needed
            typeMappings.Add(typeof(void).Name, new CSharpToTypeScriptMapping(typeof(void).Name, "void"));
            typeMappings.Add(typeof(object).Name, new CSharpToTypeScriptMapping(typeof(object).Name, "any"));

            typeMappings.Add(typeof(bool).Name, new CSharpToTypeScriptMapping(typeof(bool).Name, "boolean"));

            typeMappings.Add(typeof(byte).Name, new CSharpToTypeScriptMapping(typeof(byte).Name, "number"));
            typeMappings.Add(typeof(SByte).Name, new CSharpToTypeScriptMapping(typeof(SByte).Name, "number"));
            typeMappings.Add(typeof(short).Name, new CSharpToTypeScriptMapping(typeof(short).Name, "number"));
            typeMappings.Add(typeof(ushort).Name, new CSharpToTypeScriptMapping(typeof(ushort).Name, "number"));
            typeMappings.Add(typeof(int).Name, new CSharpToTypeScriptMapping(typeof(int).Name, "number"));
            typeMappings.Add(typeof(uint).Name, new CSharpToTypeScriptMapping(typeof(uint).Name, "number"));
            typeMappings.Add(typeof(long).Name, new CSharpToTypeScriptMapping(typeof(long).Name, "number"));
            typeMappings.Add(typeof(ulong).Name, new CSharpToTypeScriptMapping(typeof(ulong).Name, "number"));
            typeMappings.Add(typeof(float).Name, new CSharpToTypeScriptMapping(typeof(float).Name, "number"));
            typeMappings.Add(typeof(double).Name, new CSharpToTypeScriptMapping(typeof(double).Name, "number"));
            typeMappings.Add(typeof(decimal).Name, new CSharpToTypeScriptMapping(typeof(decimal).Name, "number"));

            // lots of C# Types map to TypeScript Strings:
            typeMappings.Add(typeof(string).Name, new CSharpToTypeScriptMapping(typeof(string).Name, "string"));
            typeMappings.Add(typeof(char).Name, new CSharpToTypeScriptMapping(typeof(char).Name, "char"));
            typeMappings.Add(typeof(DateTime).Name, new CSharpToTypeScriptMapping(typeof(DateTime).Name, "string"));
            typeMappings.Add(typeof(DateTimeOffset).Name, new CSharpToTypeScriptMapping(typeof(DateTimeOffset).Name, "string"));
        }

        public string FindOrAdd(string cSharpTypeName)
        {
            // look to see if it's already created:
            if (typeMappings.ContainsKey(cSharpTypeName))
                return typeMappings[cSharpTypeName].TypeScriptName;

            // TODO:  More must happen here.
            return cSharpTypeName;
        }
    }
}
