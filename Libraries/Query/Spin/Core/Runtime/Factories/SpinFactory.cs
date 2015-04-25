﻿/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDS.RDF.Query.Spin.AssemblyDiscovery;
using VDS.RDF.Query.Spin.Model;

[assembly: SpinExtensionsAssembly()]

namespace VDS.RDF.Query.Spin.Core.Runtime.Factories
{
    /// <summary>
    /// A marker interface to register classes that provide Spin extensions implementation
    /// </summary>
    /// TODO check whether we can work with this and that it does not cause any conflict
    ///         then check if we could/should bind this to the automatic Configuration API
    public interface ISpinImplementation
    {
    }

    /// <summary>
    /// A singleton class for managing runtime Spin extensions
    /// </summary>
    public class SpinFactory
    {
        private static SpinFactory _singleton = new SpinFactory();
        private static Dictionary<String, ConstructorInfo> _typesRegistry = new Dictionary<String, ConstructorInfo>();
        private static Dictionary<String, ConstructorInfo> _namespacesRegistry = new Dictionary<String, ConstructorInfo>();

        #region Registry initialisation

        static SpinFactory()
        {
            Type SpinExtension = typeof(ISpinImplementation);
            IEnumerable<Assembly> spinExtensions = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => a.GetCustomAttributes(typeof(SpinExtensionsAssembly), true).Any());
            foreach (Assembly a in spinExtensions)
            {
                try
                {
                    foreach (Type t in a.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract && SpinExtension.IsAssignableFrom(t)))
                    {
                        try
                        {
                            RegisterClass(t);
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        private SpinFactory()
        {
        }

        private static void RegisterClass(Type implementation)
        {
            ImplementsRDFSClass classUriAttribute = (ImplementsRDFSClass)implementation.GetCustomAttributes(typeof(ImplementsRDFSClass), true).FirstOrDefault();
            if (classUriAttribute != null)
            {
                RegisterClass(classUriAttribute.ClassUri, implementation);
            }
            else
            {
                ImplementsRDFSNamespace namespaceUriAttribute = (ImplementsRDFSNamespace)implementation.GetCustomAttributes(typeof(ImplementsRDFSNamespace), true).FirstOrDefault();
                if (namespaceUriAttribute != null)
                {
                    RegisterNamespace(namespaceUriAttribute.NamespaceUri, implementation);
                }
                else
                {
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(implementation.TypeHandle);
                }
            }
        }

        internal static void RegisterClass(Uri rdfsClassUri, Type implementation)
        {
            RegisterClass(rdfsClassUri.ToString(), implementation);
        }

        internal static void RegisterClass(String rdfsClassUri, Type implementation)
        {
            if (_typesRegistry.ContainsKey(rdfsClassUri))
            {
                // is this really useful or is it too restrictive ?
                Type currentType = _typesRegistry[rdfsClassUri].DeclaringType;
                // A sub-implementation is already registered for the controlClassUri
                if (implementation.IsAssignableFrom(currentType)) return;
                // The type conflicts with another declaration
                if (!currentType.IsAssignableFrom(implementation))
                {
                    throw new Exception("Conflicting implementations found for " + rdfsClassUri + " : " + currentType.FullName + " and " + implementation.FullName);
                }
            }
            // Look for any contructor that takes a SpinProcessor as only parameter
            ConstructorInfo classConstructor = implementation.GetConstructor(new Type[] { typeof(SpinModel) });
            if (classConstructor == null)
            {
                if (!_typesRegistry.ContainsKey(rdfsClassUri))
                {
                    throw new ArgumentException("No suitable constructor found for class " + implementation.FullName.ToString());
                }
                // TODO raise a warning that the class is ignored and fall back to the default class
                return;
            }
            _typesRegistry[rdfsClassUri] = classConstructor;
        }

        internal static void RegisterNamespace(Uri namespaceUri, Type implementation)
        {
            RegisterNamespace(namespaceUri.ToString(), implementation);
        }

        internal static void RegisterNamespace(String namespaceUri, Type implementation)
        {
            // Look for any contructor that takes a SpinProcessor as only parameter
            ConstructorInfo classConstructor = implementation.GetConstructor(new Type[] { typeof(SpinModel) });
            if (classConstructor == null)
            {
                if (!_namespacesRegistry.ContainsKey(namespaceUri))
                {
                    throw new ArgumentException("No suitable constructor found for class " + implementation.FullName.ToString());
                }
                // TODO raise a warning that the class is ignored and fall back to the default class
            }
            _namespacesRegistry[namespaceUri] = classConstructor;
        }

        #endregion Registry initialisation

        #region Factory methods

        internal static object TryCreate(Uri rdfsClassUri, SpinModel model)
        {
            return TryCreate(rdfsClassUri.ToString(), model);
        }

        internal static object TryCreate(IResource rdfsClass)
        {
            return TryCreate(rdfsClass.Uri, rdfsClass.GetModel());
        }

        internal static object TryCreate(String rdfsClassUri, SpinModel model)
        {
            if (!_typesRegistry.ContainsKey(rdfsClassUri))
            {
                String key = _namespacesRegistry.Keys.OrderByDescending(ns => ns).Where(ns => rdfsClassUri.StartsWith(ns)).FirstOrDefault();
                if (!String.IsNullOrWhiteSpace(key))
                {
                    ConstructorInfo constructor = _namespacesRegistry[key];
                    _typesRegistry[rdfsClassUri] = constructor;
                    return constructor.Invoke(new Object[] { model });
                }
                return null;
                throw new NotImplementedException("No implementation found for class " + rdfsClassUri.ToString());
            }
            return _typesRegistry[rdfsClassUri].Invoke(new Object[] { model });
        }

        #endregion Factory methods
    }
}