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

namespace DefinitelyTypedHubs
{
    [ExportCodeRefactoringProvider(DefinitelyTypedHubsCodeRefactoringProvider.RefactoringId, LanguageNames.CSharp), Shared]
    internal class DefinitelyTypedHubsCodeRefactoringProvider : CodeRefactoringProvider
    {
        public const string RefactoringId = "DefinitelyTypedHubs";

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            // TODO: Replace the following code with your own analysis, generating a CodeAction for each refactoring to offer

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

            var parent = typeInfo.BaseType;
            bool found = false;
            // We want to find the string "NamedType Microsoft.AspNet.SignalR.Hub"
            while ((parent.Name != "Hub") && (parent.Name != "Object"))
            {
                parent = parent.BaseType;
                if ((parent.Name == "Hub") && 
                    (parent.ContainingNamespace.ToString() == "Namespace Microsoft.AspNet.SignalR.Hub"))
                {
                    found = true;
                    break;
                }
            }
            if (!found) return;

            // For any type declaration node, create a code action to reverse the identifier text.
            var action = CodeAction.Create("Generate Typescipt Typings", c => GenerateTypings(context.Document, typeDecl, c));

            // Register this code action.
            context.RegisterRefactoring(action);
        }

        private async Task<Solution> GenerateTypings(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            // Produce a reversed version of the type declaration's identifier token.
            var identifierToken = typeDecl.Identifier;
            var newName = new string(identifierToken.Text.ToCharArray().Reverse().ToArray());

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }
    }
}