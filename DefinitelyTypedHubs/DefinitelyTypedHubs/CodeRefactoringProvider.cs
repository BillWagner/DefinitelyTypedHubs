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

            var parent = typeInfo;
            bool found = false;
            // We want to find the string "NamedType Microsoft.AspNet.SignalR.Hub"
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
            if (!found) return;

            // For any type declaration node, create a code action to reverse the identifier text.
            var action = CodeAction.Create("Generate Typescipt Typings", c => GenerateTypings(context.Document, typeDecl, c));

            // Register this code action.
            context.RegisterRefactoring(action);
        }

        private async Task<Solution> GenerateTypings(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            // TODO: Create the specific code to generate the typings from the hub 
            // definition

            // generate the file:
            var originalSolution = document.Project.Solution;
            var docInfo = DocumentInfo.Create(
                DocumentId.CreateNewId(document.Project.Id),
                "signalR.d.ts",
                new string[] { "Scripts", "typings", "signalR" },
                SourceCodeKind.Regular,
                TextLoader.From(TextAndVersion.Create(SourceText.From(signalRdefinitions),
                VersionStamp.Default)));
            var updatedSolution = originalSolution.AddAdditionalDocument(docInfo);
            return updatedSolution;
        }

    const string signalRdefinitions =
@"// Code from DefinitelyTyped Project. https://github.com/borisyankov/DefinitelyTyped (MIT license)
//   JQueryPromise & JQueryDeferred definitions (c) Microsoft
//   SignalR definitions (c) Boris Yankov https://github.com/borisyankov/
//   Updated by Murat Girgin
//   Roslyn-ized by Bill Wagner

//   
//    Copyrights are respective of each contributor listed at the beginning of each definition file.//
//
//    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the 'Software'), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
//    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
//    THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

// Simplified JQueryPromise and JQueryDefered, renamed to prevent name potential clashes with jquery
    interface IPromise<T>
    {
    always(...alwaysCallbacks: any[]): IPromise<T>;
    done(...doneCallbacks: any[]): IPromise<T>;
    fail(...failCallbacks: any[]): IPromise<T>;
    progress(...progressCallbacks: any[]): IPromise<T>;
    then<U>(onFulfill: (...values: any[]) => U, onReject?: (...reasons: any[]) => U, onProgress?: (...progression: any[]) => any): IPromise<U>;
}
    interface IDeferred<T> extends IPromise<T> {
        always(...alwaysCallbacks: any[]): IDeferred<T>;
        done(...doneCallbacks: any[]): IDeferred<T>;
        fail(...failCallbacks: any[]): IDeferred<T>;
        progress(...progressCallbacks: any[]): IDeferred<T>;
        notify(...args: any[]): IDeferred<T>;
        notifyWith(context: any, ...args: any[]): IDeferred<T>;
        reject(...args: any[]): IDeferred<T>;
        rejectWith(context: any, ...args: any[]): IDeferred<T>;
        resolve(val: T): IDeferred<T>;
        resolve(...args: any[]): IDeferred<T>;
        resolveWith(context: any, ...args: any[]): IDeferred<T>;
        state(): string;
        promise(target?: any): IPromise<T>;
    }

    interface HubMethod
    {
    (callback: (data: string) => void);
}

    interface SignalREvents
    {
        onStart: string;
    onStarting: string;
    onReceived: string;
    onError: string;
    onConnectionSlow: string;
    onReconnect: string;
    onStateChanged: string;
    onDisconnect: string;
}

    interface SignalRStateChange
    {
        oldState: number;
    newState: number;
}

    interface SignalR
    {
        events: SignalREvents;
    connectionState: any;
    transports: any;

    hub: HubConnection;
    id: string;
    logging: boolean;
    messageId: string;
    url: string;

    (url: string, queryString?: any, logging?: boolean): SignalR;
    hubConnection(url?: string): SignalR;

    log(msg: string, logging: boolean): void;
    isCrossDomain(url: string): boolean;
    changeState(connection: SignalR, expectedState: number, newState: number): boolean;
    isDisconnecting(connection: SignalR): boolean;

    // createHubProxy(hubName: string): SignalR;

    start(): IPromise<any>;
    start(callback: () => void): IPromise<any>;
    start(settings: ConnectionSettings): IPromise<any>;
    start(settings: ConnectionSettings, callback: () => void): IPromise<any>;

    send(data: string): void;
    stop(async?: boolean, notifyServer?: boolean): void;

    starting(handler: () => void): SignalR;
    received(handler: (data: any) => void): SignalR;
    error(handler: (error: string) => void): SignalR;
    stateChanged(handler: (change: SignalRStateChange) => void): SignalR;
    disconnected(handler: () => void): SignalR;
    connectionSlow(handler: () => void): SignalR;
    sending(handler: () => void): SignalR;
    reconnecting(handler: () => void): SignalR;
    reconnected(handler: () => void): SignalR;
}

    interface HubProxy
    {
    (connection: HubConnection, hubName: string): HubProxy;
    state: any;
    connection: HubConnection;
    hubName: string;
    init(connection: HubConnection, hubName: string): void;
    hasSubscriptions(): boolean;
    on(eventName: string, callback: (...msg) => void): HubProxy;
    off(eventName: string, callback: (msg) => void): HubProxy;
    invoke(methodName: string, ...args: any[]): any; // IDeferred<any>;
}

    interface HubConnectionSettings
    {
        queryString?: string;
    logging?: boolean;
    useDefaultPath?: boolean;
}

    interface HubConnection extends SignalR
    {
        //(url?: string, queryString?: any, logging?: boolean): HubConnection;
        proxies;
        received(callback: (data: { Id; Method; Hub; State; Args; }) => void): HubConnection;
        createHubProxy(hubName: string): HubProxy;
    }

    interface SignalRfn
    {
    init(url, qs, logging);
    }

    interface ConnectionSettings
    {
        transport? ;
    callback? ;
    waitForPageLoad?: boolean;
    jsonp?: boolean;
}

    interface JQueryStatic
    {
        signalR: SignalR;
    connection: SignalR;
    hubConnection(url?: string, queryString?: any, logging?: boolean): HubConnection;
}
";
    }
}