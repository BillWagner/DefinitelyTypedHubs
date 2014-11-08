using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefinitelyTypedHubs.TypeSystem
{
    internal class TypeScriptTypeDefinition
    {
        public string Name { get; }
        public string Declaration { get; }

        public TypeScriptTypeDefinition(string name, string declaration)
        {
            this.Name = name;
            this.Declaration = declaration;
        }
    }
}
