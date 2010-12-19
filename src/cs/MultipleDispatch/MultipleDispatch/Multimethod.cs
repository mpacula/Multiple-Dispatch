using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MultipleDispatch
{
    /// <summary>
    /// Implements multiple dispatch, i.e. provides the functionality of dispatching to a method
    /// based on the runtime type of all arguments.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Multimethod
    {
        /// <summary>
        /// Caches results of method lookups for the given target type and method signature.
        /// Speeds up method invocation.
        /// </summary>
        private static Dictionary<Signature, MethodInfo> dispatchCache = new Dictionary<Signature, MethodInfo>();

        /// <summary>
        /// Invokes a method on the target object based on the runtime type of all arguments.
        /// </summary>
        /// <typeparam name="TRet">return type of the method to invoke</typeparam>
        /// <param name="target">target object on which the method will be invoked</param>
        /// <param name="methodName">case-sensitive name of the method to invoke</param>
        /// <param name="arguments">arguments to invoke the method with</param>
        /// <returns>result of the method invocation</returns>
        public static TRet Call<TRet>(this object target, string methodName, params object[] arguments)
        {
            return (TRet)Dispatch(target, methodName, typeof(TRet), arguments);
        }
        
        /// <summary>
        /// Invokes a void method on the target object based on the runtime type of all arguments.
        /// </summary>
        /// <param name="target">target object on which the method will be invoked</param>
        /// <param name="methodName">case-sensitive name of the method to invoke</param>
        /// <param name="arguments">arguments to invoke the method with</param>
        public static void Call(this object target, string methodName, params object[] arguments)
        {
            Dispatch(target, methodName, typeof(void), arguments);
        }

        private static object Dispatch(object target, string methodName, Type returnType, params object[] arguments)
        {
            MethodInfo bestMatch = null; // will contain the method to call if one exists
            var argTypes = from arg in arguments select arg == null ? null : arg.GetType();

            // Is the call cached?
            Signature sig = new Signature() { 
                Name = methodName,
                ParameterTypes = argTypes, 
                ReturnType = returnType, 
                TargetType = target.GetType() };

            if (dispatchCache.ContainsKey(sig))
            {
                bestMatch = dispatchCache[sig];
                goto invoke;
            }       

            // Find all type-matching methods and store them
            var methods = target.GetType().GetMethods();
            var matchingMethods = new List<MethodInfo>();
            foreach (var method in methods)
            {
                if (!method.Name.Equals(methodName)) continue;

                // Check for return type compatibility
                if (!returnType.IsAssignableFrom(method.ReturnType)) continue;
                
                // Check for argument compatibility
                var parameters = method.GetParameters();
                if (arguments.Length != parameters.Length) continue;

                bool paramsCompatible = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if(arguments[i] == null)
                        paramsCompatible = !parameters[i].ParameterType.IsValueType;  // null can be assigned to any reference type
                    else
                        paramsCompatible = parameters[i].ParameterType.IsAssignableFrom(arguments[i].GetType());

                    if (!paramsCompatible) break;
                }

                if (!paramsCompatible)
                    continue;

                // we have a match!
                matchingMethods.Add(method);
            }

            if (matchingMethods.Count == 0)
                throw new NoMatchingMethodException(sig);

            // Is there a *single* method that best matches the arguments?
            foreach (MethodInfo m1 in matchingMethods)
            {
                foreach (MethodInfo m2 in matchingMethods)
                {
                    if (m1 == m2) continue;
                    if (!IsMoreSpecific(m1, m2))
                        goto next_m1;
                }
                bestMatch = m1; break;
            next_m1: continue;
            }

            if (bestMatch == null)
                throw new AmbiguousCallException(from mi in GetAmbiguousMethods(matchingMethods) select new Signature(mi));

            dispatchCache[sig] = bestMatch;

        invoke:
            return bestMatch.Invoke(target, arguments);
        }

        /// <summary>
        /// True if src is at least as specific as target, i.e. its type-safe uses are
        /// a subset of the type-safe uses of target.
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static bool IsMoreSpecific(MethodInfo src, MethodInfo target)
        {
            // is return type at least as general?
            if (!target.ReturnType.IsAssignableFrom(src.ReturnType))
                return false;

            var targetParams = target.GetParameters();
            var srcParams = src.GetParameters();

            // same number of parameters?
            if (srcParams.Length != targetParams.Length)
                return false;

            // are all parameters at least as specific?
            for (int i = 0; i < srcParams.Length; i++)
            {
                if (!targetParams[i].ParameterType.IsAssignableFrom(srcParams[i].ParameterType))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the methods between which a call is ambiguous by finding all most type-specific methods.
        /// If the call is unambiguous, only one method is returned.
        /// </summary>
        /// <param name="methods"></param>
        /// <returns></returns>
        private static IEnumerable<MethodInfo> GetAmbiguousMethods(IEnumerable<MethodInfo> methods)
        {
            IList<MethodInfo> mostSpecific = new List<MethodInfo>();
            int maxSpecificity = -1; // number of methods less specific than the current most specific method
                                     // as determined by isMoreSpecific()


            foreach (MethodInfo m1 in methods)
            {
                int specificity = 0;
                foreach (MethodInfo m2 in methods)
                {
                    if (m1 == m2) continue;

                    specificity += IsMoreSpecific(m1, m2) ? 1 : 0;
                }

                if (specificity > maxSpecificity)
                {
                    mostSpecific.Clear();
                    mostSpecific.Add(m1);
                    maxSpecificity = specificity;
                }
                else if (specificity == maxSpecificity)
                {
                    mostSpecific.Add(m1);
                }
            }

            return mostSpecific;
        }
    }

    /// <summary>
    /// Uniquely identifies a method.
    /// </summary>
    public class Signature
    {
        public String Name { get; set; }
        public Type TargetType { get; set; }
        public IEnumerable<Type> ParameterTypes { get; set; }
        public Type ReturnType { get; set; }

        public Signature()
        {
        }

        public Signature(MethodInfo mi)
        {
            Name = mi.Name;
            TargetType = mi.DeclaringType;
            ReturnType = mi.ReturnType;
            ParameterTypes = from p in mi.GetParameters() select p.ParameterType;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Signature))
                return false;

            Signature o = (Signature)obj;
            return TargetType.Equals(o.TargetType) 
                && Name.Equals(o.Name)
                && Enumerable.SequenceEqual(ParameterTypes, o.ParameterTypes) 
                && ReturnType.Equals(o.ReturnType);
        }

        public override int GetHashCode()
        {
            int hash = 17 * ReturnType.GetHashCode();
            hash = 19 * hash + TargetType.GetHashCode();
            hash = 307 * hash + Name.GetHashCode();

            int elt_hash = 1;
            foreach (var t in ParameterTypes)
            {
                elt_hash = 31 * elt_hash + (t == null ? 0 : t.GetHashCode());
            }

            return hash + elt_hash;
        }

        public override string ToString()
        {
            String msg = String.Format("{0} (", ReturnType.Name);
            bool first = true;
            foreach (var param in ParameterTypes)
            {
                if (!first)
                    msg += ", ";
                msg += (param == null ? "null" : param.Name);
                if (first) first = false;
            }
            msg += ")";
            return msg;
        }
    }

    public class NoMatchingMethodException : Exception
    {
        public Signature Signature { get; private set; }

        public NoMatchingMethodException(Signature sig)
            : base(ComposeMessage(sig))
        {
            Signature = sig;    
        }

        private static string ComposeMessage(Signature sig)
        {
            return String.Format("No matching method for type signature {0}", sig);
        }
    }

    public class AmbiguousCallException : Exception
    {
        public IEnumerable<Signature> Signatures { get; private set; }

        public AmbiguousCallException(IEnumerable<Signature> signatures)
            : base(ComposeMessage(signatures))
        {
            Signatures = signatures;
        }

        private static string ComposeMessage(IEnumerable<Signature> signatures)
        {
            String msg = String.Format("Ambiguous call. Matching methods: ");
            
            foreach (var sig in signatures)
            {
                msg += "\n" + sig;
            }

            return msg;
        }
    }
}
