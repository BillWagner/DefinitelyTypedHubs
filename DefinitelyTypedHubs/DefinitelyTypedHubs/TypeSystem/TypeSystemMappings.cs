using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefinitelyTypedHubs.TypeSystem
{
    class TypeSystemMappings
    {
        static Dictionary<string, TypeScriptTypeDefinition> cache;

        public static Dictionary<string, TypeScriptTypeDefinition> Cache { get { return cache; } }

        public static List<int> TupleTypesByCount { get; set; }
        public static bool HasDictionary { get; set; }

        static TypeSystemMappings() 
        {
            // This is probably the wrong solution.
            // The better way will be to use the different kinds of 
            // tokens to know what the user has typed (predefined type, etc)

            cache = new Dictionary<string, TypeScriptTypeDefinition>();
            TupleTypesByCount = new List<int>();
            HasDictionary = false;

            // Integral types
            cache.Add("System.Object", new TypeScriptTypeDefinition("any", "" ));
            cache.Add("System.Boolean", new TypeScriptTypeDefinition("boolean", "" ));

            cache.Add("System.Byte", new TypeScriptTypeDefinition("number", "" ));
            cache.Add("System.SByte", new TypeScriptTypeDefinition("number", "" ));
            cache.Add("System.Short", new TypeScriptTypeDefinition("number", "" ));
            cache.Add("System.UShort", new TypeScriptTypeDefinition( "number", "" ));
            cache.Add("System.Int32", new TypeScriptTypeDefinition("number", "" ));
            cache.Add("System.UInt32", new TypeScriptTypeDefinition("number", "" ));
            cache.Add("System.Int64", new TypeScriptTypeDefinition("number", "" ));
            cache.Add("System.UInt64", new TypeScriptTypeDefinition("number", "" ));
            cache.Add("System.Single", new TypeScriptTypeDefinition("number", "" ));
            cache.Add("System.Double", new TypeScriptTypeDefinition("number", "" ));
            cache.Add("System.Decimal", new TypeScriptTypeDefinition("number", "" ));

            cache.Add("System.String", new TypeScriptTypeDefinition( "string", "" ));
            cache.Add("System.Char", new TypeScriptTypeDefinition( "string", "" ));
            cache.Add("System.DateTime", new TypeScriptTypeDefinition( "string", "" ));
            cache.Add("System.DateTimeOffset", new TypeScriptTypeDefinition( "string", "" ));
            cache.Add("System.Byte[]", new TypeScriptTypeDefinition("string", "" ));
            cache.Add("System.Type", new TypeScriptTypeDefinition("string", "" ));
            cache.Add("System.Guid", new TypeScriptTypeDefinition("string", "" ));

            cache.Add("System.Exception", new TypeScriptTypeDefinition( "string", ""));
            cache.Add("System.Collections.IDictionary", new TypeScriptTypeDefinition( "Dictionary<string, any>", ""));

        }

        // Types that have implicit declarations 
        public static TypeScriptTypeDefinition MakeFromNullable(Type type)
        {
            if (type.GenericTypeArguments.Count() == 1)
            {
                var name = GetTypeScriptType(type.GenericTypeArguments[0]).Name;
                return new TypeScriptTypeDefinition(name + "?", "");
            }
            throw new ArgumentException("Can't make a Nullable type from: " + type);
        }

        public static TypeScriptTypeDefinition MakeFromTask(Type type)
        {
            var name = "void";

            if (type.GenericTypeArguments.Count() == 1)
            {
                name = GetTypeScriptType(type.GenericTypeArguments[0]).Name;
            }
            return new TypeScriptTypeDefinition(name, "");
        }

        public static TypeScriptTypeDefinition MakeArray(Type type)
        {
            if (type.GenericTypeArguments.Count() != 1)
            {
                throw new ArgumentException("Can't convert " + type.FullName + " to an Array type. Invalid number of generic type arguments");
            }

            var elementType = type.GenericTypeArguments[0];
            var tst = GetTypeScriptType(elementType);

            return new TypeScriptTypeDefinition("Array<" + tst.Name + ">","");
        }

        // TODO: create a generic Dictionary type interface..  such as IDictionary<T> (excepts number or string) in []  
        static TypeScriptTypeDefinition MakeDictionary(Type type)
        {
            if (type.GenericTypeArguments.Count() != 2)
            {
                throw new ArgumentException("Can't convert " + type.FullName + " to a Dictionary type. Invalid number of generic type arguments");
            }

            var keyType = type.GenericTypeArguments[0];
            var valueType = type.GenericTypeArguments[1];
            var keyTypeScriptType = GetTypeScriptType(keyType);
            if (keyTypeScriptType.Name != "string" && keyTypeScriptType.Name != "number")
            {
                throw new ArgumentException("Can't convert " + type.FullName
                    + " to a collection type. TypeScript type of the key is "
                    + keyTypeScriptType.Name + ". Should be number or string");
            }

            var valueTypeScriptType = GetTypeScriptType(valueType);
            HasDictionary = true;

            return new TypeScriptTypeDefinition("IDictionary<" + valueTypeScriptType.Name + ">","");
        }

        // TODO: for each count of tuples, create a corresponding interface with number of Tuples..
        static TypeScriptTypeDefinition MakeTuple(Type type)
        {
            var count = type.GenericTypeArguments.Count();
            var name = "Tuple" + count + "<";

            TupleTypesByCount.Add(count);

            // TODO: Use Aggregate
            var list = type.GenericTypeArguments.ToList();
            foreach (var item in list)
                name += GetTypeScriptType(item).Name + ", ";

            name = name.Substring(0, name.Length - ", ".Length);
            // END TODO
            name += ">";

            return new TypeScriptTypeDefinition(name, "");
        }

        // Types that have exlplicit declarations

        static TypeScriptTypeDefinition MakeEnum(Type type)
        {
            var name = type.Name;
            var declaration = "enum " + name + " {";
            var count = 0;

            // TODO:  USE ROSLYN
            //type.GetEnumNames().ToList().ForEach(n =>
            //{
            //    var sep = (count != 0 ? ", " : "") + "\n    ";
            //    count++;
            //    declaration += sep + n;
            //});
            // END TODO:

            declaration += "\n}";

            return new TypeScriptTypeDefinition(name,declaration);
        }

        static TypeScriptTypeDefinition MakeClassOrInterface(Type type)
        {
            var declaration = "";
            var name = type.Name;
            // TODO:  USE ROSLYN:
            var members = new List<Tuple<string, TypeScriptTypeDefinition>>();// type.GetMembers().ToList()
                //.Select((mi, x) => Tuple.Create(mi.MemberType.ToString(), mi))
                //.Where(mt => (mt.Item1 == "Property" || mt.Item1 == "Field"))
                //.ToList();
            // END TODO:

            // TODO: Consider differentiating between classes and interfaces...
            declaration += "interface " + name + " {\n";

            foreach(var mt in members)
            { 
                var mem = mt.Item2;

                var fieldName = mem.Name;
                var typeName = "";

                if (mt.Item1 == "Property")
                {
                    // TODO: USE ROSLYN var prop = type.GetProperty(fieldName);
                    // TODO: USE ROSLYN typeName = GetTypeScriptType(prop.PropertyType).Name;
                }
                else if (mt.Item1 == "Field")
                {
                    // TODO:  USE ROSLYN var prop = type.GetField(fieldName);
                    // TODO:  USE ROSLYN typeName = GetTypeScriptType(prop.FieldType).Name;
                }
                else
                {
                    // Shouldn't happen
                    throw new ArgumentException("Unknown kind of class or interface element: " + fieldName + " is not expected....");
                }

                declaration += "    " + mem.Name + ": " + typeName + ";\n";
            };

            declaration += "}";

            return new TypeScriptTypeDefinition(name, declaration);
        }

        public static TypeScriptTypeDefinition GetTypeScriptType(Type type)
        {
            TypeScriptTypeDefinition value;
            var typeName = type.FullName;

            if (cache.TryGetValue(typeName, out value))
            {
                return value;
            }


            // Nullables..
            if (type.FullName.Contains("Nullable"))
            {
                value = MakeFromNullable(type);
            }
            // Tasks..
            else if (type.FullName.Contains("Task") /* TODO USE ROSLYN || type.GetInterfaces().Any(X => X.FullName.Contains("Task")) */)
            {
                value = MakeFromTask(type);
            }
            // Dictionaries -- these should come before IEnumerables, because they also implement IEnumerable
            // TODO: USE ROSLYN
            //else if (type.GetInterfaces().Any(X => X.FullName.Contains("IDictionary")))
            //{
            //    value = MakeDictionary(type);
            //}
            // Arrays
            else if (typeName.Contains("[]"))
            {
                value = MakeArray(typeof(List<Object>));
            }
            // TODO: USE ROSLYN
            //else if (type.GetInterfaces().Any(X => X.FullName.Contains("IList") || X.FullName.Contains("IEnumerable")))
            //{
            //    value = MakeArray(type);
            //}
            //else if (type.GetInterfaces().Any(X => X.FullName.Contains("Tuple")))
            //{
            //    value = MakeTuple(type);
            //}
            //else if (type.IsEnum)
            //{
            //    value = MakeEnum(type);
            //}
            //else if (type.IsClass || type.IsInterface)
            //{
            //    value = MakeClassOrInterface(type);
            //}
            else if (type.FullName == "System.Void")
            {
                value = new TypeScriptTypeDefinition("void", "");
            }
            else
            {
                //Console.WriteLine("Warning:" + type);
                value = new TypeScriptTypeDefinition("UNKNOWN TYPE " + type, "");
            }


            cache.Add(type.FullName, value);

            return value;
        }
    }
}
