using Microsoft.CodeAnalysis;
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
        public MethodSignature(MethodDeclarationSyntax member, SemanticModel semanticModel, TypeMappingDictionary typeMap)
        {
            MethodName = member.Identifier.ToString();
            var returnType = member.ReturnType;

            // TODO:  TypeScript return types for nullables are simply the non-nullable counterpar.
            ReturnType = typeMap.FindOrAddTypeSyntax(returnType, semanticModel);
            var parms = member.ParameterList
                .Parameters
                .Select(parm => new MethodParameter(parm, semanticModel, typeMap));
            ParameterList = parms.ToArray();
        }
        public string MethodName { get; }
        public string ReturnType { get; }
        public IEnumerable<MethodParameter> ParameterList { get; }
    }
}
