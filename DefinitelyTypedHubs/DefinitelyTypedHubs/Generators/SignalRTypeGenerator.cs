using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.IO;

namespace DefinitelyTypedHubs.Generators
{
    internal class SignalRTypeGenerator
    {
        internal static Solution CreateSignalRTypeDefIfNeeded(Solution originalSolution, Project project)
        {
            // Check to see if this file already exists, and only create it conditionally.
            // TODO: This isn't working, because this document doesn't appear as part of the
            // project. Go figure.
            // From Srivatsn Narayanan
            // We create Documents only for C# files normally. 
            // However, you can put some annotation in a project to ask for non-C# documents.

            //Say in you csproj file you have this:

            //< Content Include = “somefile.html” />
            //< None Include =”somefile.d.ts” />

            //You can then add this property to the csproj file:

            //< AdditionalFileItemNames >$(AdditionalFileItemNames); Content; None </ AdditionalFileItemNames >

            // This tells the project system to pass all items of name “Content” and “None” 
            // to the compiler as well.Then you can inspect Project.AdditionalDocuments and see TextDocuments for these files.

            // It turns out that there isn't a good way to add the AdditionalFileItemNames node to the 
            // project file. Hmm.
            if (!project.AdditionalDocuments.Any(d => d.Name == "signalR.d.ts"))
            {
                var signalRDoc = DocumentInfo.Create(
                    DocumentId.CreateNewId(project.Id),
                    "signalR.d.ts",
                    new string[] { "Scripts", "typings", "signalR" },
                    SourceCodeKind.Regular,
                    TextLoader.From(TextAndVersion.Create(SourceText.From(signalRdefinitions),
                    VersionStamp.Default)));
                
                var updatedSolution = originalSolution.AddAdditionalDocument(signalRDoc);
                return updatedSolution;
            }
            else
                return originalSolution;
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
interface IPromise<T> {
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

interface HubMethod {
    (callback: (data: string) => void);
}

interface SignalREvents {
    onStart: string;
    onStarting: string;
    onReceived: string;
    onError: string;
    onConnectionSlow: string;
    onReconnect: string;
    onStateChanged: string;
    onDisconnect: string;
}

interface SignalRStateChange {
    oldState: number;
    newState: number;
}

interface SignalR {
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

interface HubProxy {
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

interface HubConnectionSettings {
    queryString?: string;
    logging?: boolean;
    useDefaultPath?: boolean;
}

interface HubConnection extends SignalR {
    //(url?: string, queryString?: any, logging?: boolean): HubConnection;
    proxies;
    received(callback: (data: { Id; Method; Hub; State; Args; }) => void): HubConnection;
    createHubProxy(hubName: string): HubProxy;
}

interface SignalRfn {
    init(url, qs, logging);
}

interface ConnectionSettings {
    transport? ;
    callback? ;
    waitForPageLoad?: boolean;
    jsonp?: boolean;
}

interface JQueryStatic {
    signalR: SignalR;
    connection: SignalR;
    hubConnection(url?: string, queryString?: any, logging?: boolean): HubConnection;
}

interface IPromise<T> {
    done(cb: (result: T) => any): IPromise<T>;
    error(cb: (error: any) => any): IPromise<T>;
}
";
    }
}
