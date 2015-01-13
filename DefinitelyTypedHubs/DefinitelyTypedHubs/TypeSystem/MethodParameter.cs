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
            ParameterType = typeMap.FindOrAddTypeSyntax(parm.Type, semanticModel);
            ParameterName = parm.Identifier.ToString();
        }
        public string ParameterType { get; }
        public string ParameterName { get; }
    }
}
