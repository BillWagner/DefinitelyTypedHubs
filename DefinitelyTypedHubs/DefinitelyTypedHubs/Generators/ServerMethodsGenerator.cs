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
        public ServerMethodsGenerator(TypeDeclarationSyntax hubTypeDeclaration)
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
                                select method;

            foreach (var member in publicMethods)
            {
                var returnType = member.ReturnType.ToString();
                var methodName = member.Identifier.ToString();
                var parameters = member.ParameterList.Parameters
                    .Select(parm => new MethodParameter(parm.Type.ToString(), parm.Identifier.ToString()));
            }

        }

        private static void BuildServerInterface(string typeName, TypeDeclarationSyntax typeDecl, StringBuilder hubDefinition)
        {
            // Add the hub
            hubDefinition.AppendLine("// Hub interfaces:");
            hubDefinition.Append("interface I");
            hubDefinition.Append(typeName);
            hubDefinition.AppendLine("{");

            var publicMethods = from member in typeDecl.Members
                                let method = member as MethodDeclarationSyntax
                                where member.IsKind(SyntaxKind.MethodDeclaration)
                                && method.Modifiers.Any(SyntaxKind.PublicKeyword)
                                select method;

            foreach (var member in publicMethods)
            {
                hubDefinition.Append("\t");
                hubDefinition.Append(member.Identifier);
                hubDefinition.Append("(");
                var parms = member.ParameterList.Parameters
                    .Select(parm => string.Format("{0}: {1}", parm.Identifier, parm.Type.ToString()))
                    .Aggregate((memo, current) => string.Format("{0}, {1}", memo, current));
                hubDefinition.Append(parms);

                // TODO: Parameters need TypeScript Names, if not simple types.
                hubDefinition.Append("): IPromise<");
                // TODO: Return Type must be TypeScript proper
                hubDefinition.Append(member.ReturnType.ToString());
                hubDefinition.AppendLine(">;");
            }

            hubDefinition.AppendLine("}");
            hubDefinition.AppendLine();
        }

    }
}
