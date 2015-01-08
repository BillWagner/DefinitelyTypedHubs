using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefinitelyTypedHubs.TypeSystem
{
    public class CSharpToTypeScriptMapping
    {
        public string CSharpName { get; }

        public string TypeScriptName { get; }

        public bool IsUserDefined { get; }

        public CSharpToTypeScriptMapping(string csName, string typeScriptName)
        {
            CSharpName = csName;
            TypeScriptName = typeScriptName;
            IsUserDefined = false;
        }
    }
}
