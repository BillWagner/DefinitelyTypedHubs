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
        public MethodParameter(ParameterSyntax parm, TypeMappingDictionary typeMap)
        {
            ParameterType = typeMap.FindOrAdd(parm.Type.ToString());
            ParameterName = parm.Identifier.ToString();
        }
        public string ParameterType { get; }
        public string ParameterName { get; }
    }
}
