using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        // TODO:  More must happen here.
        // Make sure we can find:   
        // 1. Nullable<T>
        // 2. sequence (Array, List, IEnumerable,)
        // 4. Task<T> to Promise<T>
        // 5. Associative containers to IDictionary (where key is string or number)
        // 6. Tuples

        // User defined:
        // 7. enums
        // General UDTs

        public string FindOrAddTypeSyntax(TypeSyntax cSharpType, SemanticModel semanticModel)
        {
            string typeScriptName = "MoreParsingNeeded";

            // There's a special case here (well, actually several)
            // 1. If the return type is a nullable<T>, The returnType is 
            // either a GenericNameSyntax (Nullable<int>), or a NullableTypeSyntax (int?)
            // In both cases, we want to find the type that is the nullable, and 
            // use that type instead, given that it is a nullable)
            // TODO: This will likely need to be refactored again, when I process
            // other generic types that aren't nullables.
            if (cSharpType is GenericNameSyntax)
            {
                var genericType = cSharpType as GenericNameSyntax;
                var symbol = semanticModel.GetSymbolInfo(genericType);

                var genericTypeArg = symbol.Symbol.Name;

                // Nullable<T>:
                if (genericTypeArg == "Nullable")
                {
                    var typeArg = genericType.TypeArgumentList.Arguments.First();
                    var csharpName = semanticModel.GetSymbolInfo(typeArg).Symbol.Name;
                    typeScriptName = FindOrAddNullable(csharpName);
                }
            }
            else if (cSharpType is NullableTypeSyntax)
            {
                var nullableReturnType = cSharpType as NullableTypeSyntax;
                var actualType = nullableReturnType.ElementType;
                var symbol = semanticModel.GetSymbolInfo(actualType);
                var cSharpReturnType = symbol.Symbol.Name;
                typeScriptName = FindOrAddNullable(cSharpReturnType);
            }
            else
            {
                var symbol = semanticModel.GetSymbolInfo(cSharpType);
                var cSharpReturnType = symbol.Symbol.Name;
                typeScriptName = FindOrAdd(cSharpReturnType);
            }
            return typeScriptName;
        }


        private string FindOrAddNullable(string cSharpTypeName)
        {
            // look to see if it's already created:
            if (typeMappings.ContainsKey(cSharpTypeName))
                return typeMappings[cSharpTypeName].TypeScriptName + "?";
            else
                return "StillMoreParsingToDo?";
        }

        private string FindOrAdd(string cSharpTypeName)
        {
            // look to see if it's already created:
            if (typeMappings.ContainsKey(cSharpTypeName))
                return typeMappings[cSharpTypeName].TypeScriptName;
            return "StillMoreParsingToDo";
        }
    }
}
