using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;

namespace M64RPFW.Misc;

public static class PrettyPrint
{
    private static readonly ImmutableDictionary<Type, string> SpecialNames = new Dictionary<Type, string>
    {
        {typeof(bool), "bool"},
        {typeof(byte), "byte"},
        {typeof(sbyte), "sbyte"},
        {typeof(short), "short"},
        {typeof(ushort), "ushort"},
        {typeof(int), "int"},
        {typeof(uint), "uint"},
        {typeof(long), "long"},
        {typeof(ulong), "ulong"},
        {typeof(float), "float"},
        {typeof(double), "double"},
        {typeof(decimal), "decimal"},
        {typeof(object), "object"},
        {typeof(string), "string"},
    }.ToImmutableDictionary();

    private static void TypeNameBuild(Type t, StringBuilder sb)
    {
        // "primitive" types, object, string
        if (SpecialNames.TryGetValue(t, out var specialName))
        {
            sb.Append(specialName);
            return;
        }

        if (t.IsGenericType)
        {
            // Nullable<T> becomes T?
            if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // underlying type of nullable
                Type ut = t.GetGenericArguments()[0];
                TypeNameBuild(ut, sb);
                sb.Append("?");
                return;
            }

            bool pastFirst = false;
            
            // Generic case (not Nullable)
            if (t.Namespace != null)
                sb.Append($"{t.Namespace}.");
            sb.Append($"{t.Name}<");
            foreach (Type arg in t.GetGenericArguments())
            {
                if (!pastFirst)
                {
                    pastFirst = true;
                    sb.Append(", ");
                }
                TypeNameBuild(arg, sb);
            }
            sb.Append(">");
            return;
        }

        if (t.IsGenericTypeDefinition)
        {
            bool pastFirst = false;
            
            // Generic case (not Nullable)
            if (t.Namespace != null)
                sb.Append($"{t.Namespace}.");
            sb.Append($"{t.Name}<");
            foreach (Type arg in t.GetGenericArguments())
            {
                if (!pastFirst)
                    pastFirst = true;
                else
                    sb.Append(", ");
                sb.Append(arg.Name);
            }
            sb.Append(">");
            return;
        }

        if (t.IsGenericTypeParameter)
        {
            sb.Append($"{t.Name} from ");
            TypeNameBuild(t.DeclaringType!, sb);
            return;
        }

        if (t.IsGenericMethodParameter)
        {
            bool pastFirst;
            
            MethodBase m = t.DeclaringMethod!;
            Type? mt = m.DeclaringType;
            
            sb.Append($"{t.Name} from ");
            if (mt != null)
                TypeNameBuild(mt, sb);
            sb.Append($".{m.Name}<");
            
            pastFirst = false;
            foreach (Type gmp in m.GetGenericArguments())
            {
                if (!pastFirst)
                    pastFirst = true;
                else
                    sb.Append(", ");
                sb.Append(gmp.Name);
            }
            
            sb.Append(">(");

            pastFirst = false;
            foreach (var param in m.GetParameters())
            {
                if (!pastFirst)
                    pastFirst = true;
                else
                    sb.Append(", ");

                Type pt = param.ParameterType;
                if (pt.IsGenericMethodParameter)
                    sb.Append(pt.Name);
                else
                    TypeNameBuild(pt, sb);
                sb.Append($" {param.Name}");
            }

            sb.Append(")");
        }
        
        if (t.IsArray)
        {
            TypeNameBuild(t.GetElementType()!, sb);
            sb.Append("[]");
            return;
        }
        
        if (t.IsPointer)
        {
            TypeNameBuild(t.GetElementType()!, sb);
            sb.Append("*");
            return;
        }
        
        if (t.IsByRef)
        {
            sb.Append("ref ");
            TypeNameBuild(t.GetElementType()!, sb);
            
            return;
        }

        sb.Append(t.FullName);
    }

    public static string GetTypeName(Type t)
    {
        StringBuilder sb = new();
        TypeNameBuild(t, sb);
        return sb.ToString();
    }
}