using DefinitelyTypedHubs.TypeSystem;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefinitelyTypedHubs.Generators
{
    public class ClientMethodsGenerator
    {
        public ClientMethodsGenerator(TypeDeclarationSyntax hubTypeDeclaration, 
            SemanticModel semanticModel,
            TypeMappingDictionary typeMap)
        {
            // TODO: Work to determine the valid client methods.
        }

        public string GenerateInterface(string serverTypeName, TypeMappingDictionary typeMap)
        {
            var builder = new StringBuilder();
            builder.AppendLine("//");
            builder.AppendLine("// Client interfaces:");
            builder.AppendLine("// These are to be implemented by the user.");
            builder.AppendLine("// These are for Hub -> Client calls.");
            builder.AppendLine("// Some dynamic calls may be missing.");
            builder.Append("interface I");
            builder.Append(serverTypeName);
            builder.AppendLine("Client {");
            // More here....
            // TODO: This is the hard part, it's the methods that would 
            // be called at the client.
            builder.AppendLine("}");
            builder.AppendLine();
            return builder.ToString();
        }
    }
}
