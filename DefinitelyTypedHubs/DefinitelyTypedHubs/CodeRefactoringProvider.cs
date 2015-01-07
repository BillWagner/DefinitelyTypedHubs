using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Text;
using DefinitelyTypedHubs.Generators;

namespace DefinitelyTypedHubs
{
    /// <summary>
    /// This class implements the refactoring that provides TypeScript 
    /// typing definitions for a SignalR hub.
    /// </summary>
    /// <remarks>
    /// This refactoring is available only when the selected node
    /// is a type definition that represents a SignalR hub.
    /// Open Issue: Should this refactoring only be available on
    /// sealed classes, or should it work on any hub-derived class?
    /// <p>
    /// There are several actions that may be needed as part of the refactoring.
    /// <list type="number">
    /// <item>
    /// <description>
    /// The first task is to create a signalr.d.ts file that stores the basic
    /// interfaces for SignalR. These are hardcoded (at this time), and will
    /// be common for every project. Once generated, they will not be updated 
    /// (at this time).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Next, I create a file that represents the methods that the client 
    /// (TypeScript code) can call at the server. This one is reasonably 
    /// straightforward: code must find all public methods in the hub class.
    /// This must be regenerated whenever the user requests this refactoring.
    /// The user may have added, changed, or removed public methods from the 
    /// hub class.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Third, this refactoring creates a definition for the methods that the 
    /// server code calls on the client. For the moment, this is a dummy file.
    /// I'm still thinking about how to implement that side.
    /// </description>
    /// </item>
    /// </list>
    /// </p>
    /// <p>
    /// During both the second and third phases, this code also builds any 
    /// data types needed for the interfaces.  That's also on the TODO list.
    /// </p>
    /// </remarks>
    [ExportCodeRefactoringProvider(DefinitelyTypedHubsCodeRefactoringProvider.RefactoringId, LanguageNames.CSharp), Shared]
    internal class DefinitelyTypedHubsCodeRefactoringProvider : CodeRefactoringProvider
    {
        /// <summary>
        /// This is the name for my refactoring.
        /// </summary>
        public const string RefactoringId = "DefinitelyTypedHubs";

        /// <summary>
        /// This method computes what action(s) might be valid here. 
        /// </summary>
        /// <param name="context">
        /// The context, which represents where the developer's cursor is.
        /// </param>
        /// <returns>
        /// A task that, when succesfully completed, has registered any valid actions.
        /// </returns>
        /// <remarks>
        /// This method can be whenever the user brings up the refactoring 
        /// lightbulb. For that reason, this method must be well-performing. 
        /// We want to exit as quickly as possible when this refactoring will
        /// not be valid.
        /// If the cursor is on a class definition that is derived from a hub,
        /// this method registers the refactoring action.
        /// </remarks>
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a type declaration node.
            var typeDecl = node as ClassDeclarationSyntax;
            if (typeDecl == null)
            {
                return;
            }

            // look for the base classes:
            // See if any base class is Hub.
            // Full name: Microsoft.AspNet.SignalR.Hub
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            var typeInfo = semanticModel.GetDeclaredSymbol(typeDecl);

            var parent = typeInfo;
            bool found = false;
            // We want to find the string "NamedType Microsoft.AspNet.SignalR.Hub" 
            // as a base c;ass/
            do
            {
                parent = parent.BaseType;
                if ((parent.Name == "Hub") &&
                    (parent.ContainingNamespace.ToString() == "Microsoft.AspNet.SignalR"))
                {
                    found = true;
                    break;
                }
            } while ((parent.Name != "Hub") && (parent.Name != "Object"));

            // This class is not a hub, so return.
            if (!found) return;

            // For any type declaration node that is a hub, create a code action to reverse the identifier text.
            var action = CodeAction.Create("Generate Typescipt Typings", 
                c => GenerateTypings(context.Document, typeDecl, c));

            // Register this code action.
            context.RegisterRefactoring(action);
        }

        private async Task<Solution> GenerateTypings(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            var originalSolution = document.Project.Solution;
            var project = document.Project;
            // This method runs three successive tasks.

            // 1. Generate the signalr.d.ts file, if needed.
            var updatedSolution = SignalRTypeGenerator.CreateSignalRTypeDefIfNeeded(originalSolution, project);

            // 2. Generate the interface that includes all server side methods.
            // Figure out the server and client type names:
            var typeName = typeDecl.Identifier.ToString();

            var serverDefinitions = new ServerMethodsGenerator(typeDecl);


            // This is generated as a string, 
            updatedSolution = GenerateHubFile(document, typeDecl, updatedSolution);

            // 3. Generate the interface that includes all client side methods (later).

            // generate the file:
            return updatedSolution;
        }

        private static Solution GenerateHubFile(Document document, TypeDeclarationSyntax typeDecl, Solution originalSolution)
        {
            var typeName = typeDecl.Identifier.ToString();
            StringBuilder hubDefinition = BuildClientInterfaces(typeName);

            // Data definitions, if any (TODO):

            BuildServerInterface(typeName, typeDecl, hubDefinition);

            // Add the proxy.
            BuildProxyDefinitions(typeName, hubDefinition);

            var docInfo = DocumentInfo.Create(
                    DocumentId.CreateNewId(document.Project.Id),
                    typeName + "Hub.d.ts",
                    new string[] { "Scripts", "typings", "signalR" },
                    SourceCodeKind.Regular,
                    TextLoader.From(TextAndVersion.Create(SourceText.From(hubDefinition.ToString()),
                    VersionStamp.Default)));
            var updatedSolution = originalSolution.AddAdditionalDocument(docInfo);
            return updatedSolution;
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

        private static void BuildProxyDefinitions(string typeName, StringBuilder hubDefinition)
        {
            // Generetated proxies 
            hubDefinition.AppendLine("// Proxy Definition:");

            hubDefinition.Append("interface I");
            hubDefinition.Append(typeName);
            hubDefinition.AppendLine("Proxy {");

            hubDefinition.Append("\tserver: I");
            hubDefinition.Append(typeName);
            hubDefinition.AppendLine(";");

            hubDefinition.Append("\tclient: I");
            hubDefinition.Append(typeName);
            hubDefinition.AppendLine("Client;");

            hubDefinition.AppendLine("}");
            hubDefinition.AppendLine();
        }

        private static StringBuilder BuildClientInterfaces(string typeName)
        {
            var hubDefinition = new StringBuilder();
            hubDefinition.AppendLine("//");
            hubDefinition.AppendLine("// Client interfaces:");
            hubDefinition.AppendLine("// These are to be implemented by the user.");
            hubDefinition.AppendLine("// These are for Hub -> Client calls.");
            hubDefinition.AppendLine("// Some dynamic calls may be missing.");
            hubDefinition.AppendLine();
            hubDefinition.Append("interface I");
            hubDefinition.Append(typeName);
            hubDefinition.AppendLine("Client {");
            // More here....
            // TODO: This is the hard part, it's the methods that would 
            // be called at the client.
            hubDefinition.AppendLine("}");
            return hubDefinition;
        }
    }
}