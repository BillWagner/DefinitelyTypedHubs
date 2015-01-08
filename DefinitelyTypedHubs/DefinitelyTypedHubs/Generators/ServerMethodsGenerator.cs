using DefinitelyTypedHubs.TypeSystem;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefinitelyTypedHubs.Generators
{
    public class ServerMethodsGenerator
    {
        private readonly IEnumerable<MethodSignature> methods;

        public ServerMethodsGenerator(TypeDeclarationSyntax hubTypeDeclaration, 
            TypeMappingDictionary typeDictionary)
        {
            // Build the tree of method names
            // So that this only traverses the tree once, also build 
            // a list of all the user defined types (UDTs) that are 
            // used in this interface.

            // VERY IMPORTANT: Do not hold onto the TypeDeclarationSyntaxNode. 
            // Process the syntax, and then leave.
            var publicMethods = from member in hubTypeDeclaration.Members
                                let method = member as MethodDeclarationSyntax
                                where member.IsKind(SyntaxKind.MethodDeclaration)
                                && method.Modifiers.Any(SyntaxKind.PublicKeyword)
                                select new MethodSignature(method, typeDictionary);

            methods = publicMethods.ToList();
        }

        public string GenerateInterface(string serverTypeName, TypeMappingDictionary typeDictionary)
        {
            var builder = new StringBuilder("// Hub interfaces:");
            builder.AppendLine();
            builder.Append("interface I");
            builder.Append(serverTypeName);
            builder.AppendLine("{");

            foreach (var member in methods)
            {
                builder.Append("\t");
                builder.Append(member.MethodName);
                builder.Append("(");
                var parms = member.ParameterList
                    .Select(parm => string.Format("{0}: {1}", parm.ParameterName, parm.ParameterType));
                if (parms.Any())
                {
                    var parmString = parms
                        .Aggregate((memo, current) => string.Format("{0}, {1}", memo, current));
                    builder.Append(parmString);
                }
                builder.Append("): IPromise<");
                builder.Append(member.ReturnType.ToString());
                builder.AppendLine(">;");
            }

            builder.AppendLine("}");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}
