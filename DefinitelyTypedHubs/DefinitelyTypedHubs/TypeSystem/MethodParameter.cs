using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefinitelyTypedHubs.TypeSystem
{
    public struct MethodParameter
    {
        public MethodParameter(ParameterSyntax parm, SemanticModel semanticModel, TypeMappingDictionary typeMap)
        {
            var symbol = semanticModel.GetSymbolInfo(parm.Type);

            var cSharpType = symbol.Symbol.Name;

            ParameterType = typeMap.FindOrAdd(cSharpType);
            ParameterName = parm.Identifier.ToString();
        }
        public string ParameterType { get; }
        public string ParameterName { get; }
    }
}
