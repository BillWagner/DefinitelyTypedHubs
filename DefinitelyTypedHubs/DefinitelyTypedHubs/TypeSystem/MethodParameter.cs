using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefinitelyTypedHubs.TypeSystem
{
    public struct MethodParameter
    {
        public MethodParameter(string typeName, string argName)
        {
            ParameterType = typeName;
            ParameterName = argName;
        }
        public string ParameterType { get; }
        public string ParameterName { get; }
    }
}
