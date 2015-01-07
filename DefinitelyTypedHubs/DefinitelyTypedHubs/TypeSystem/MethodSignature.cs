using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefinitelyTypedHubs.TypeSystem
{
    public class MethodSignature
    {
        public MethodSignature(string name, string returnType, IEnumerable<MethodParameter> parms)
        {
            MethodName = name;
            ReturnType = returnType;
            // Eagerly execute the query and provide local storage:
            ParameterList = parms.ToArray();
        }
        public string MethodName { get; }
        public string ReturnType { get; }
        public IEnumerable<MethodParameter> ParameterList { get; }
    }
}
