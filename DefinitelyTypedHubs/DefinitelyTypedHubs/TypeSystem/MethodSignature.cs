using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefinitelyTypedHubs.TypeSystem
{
    public class MethodSignature
    {
        public MethodSignature(MethodDeclarationSyntax member, TypeMappingDictionary typeMap)
        {
            MethodName = member.Identifier.ToString();
            var cSharpReturnType = member.ReturnType.ToString();
            var typeScriptName = typeMap.FindOrAdd(cSharpReturnType);
            ReturnType = typeScriptName;
            var parms = member.ParameterList
                .Parameters
                .Select(parm => new MethodParameter(parm, typeMap));
            ParameterList = parms.ToArray();
        }
        public string MethodName { get; }
        public string ReturnType { get; }
        public IEnumerable<MethodParameter> ParameterList { get; }
    }
}
