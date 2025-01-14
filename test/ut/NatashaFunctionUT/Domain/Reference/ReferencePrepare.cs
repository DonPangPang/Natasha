﻿using Microsoft.CodeAnalysis;
using Natasha.CSharp;
using PluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public class ReferencePrepare : DomainPrepare
{
    internal protected static readonly NatashaReferenceCache DefaultReferences;

    static ReferencePrepare()
    {
        DefaultReferences = NatashaReferenceDomain.DefaultDomain.References;
    }
    internal static HashSet<PortableExecutableReference> GetPortableExecutableReferences(LoadBehaviorEnum loadBehavior)
    {
        var domain = DomainManagement.Random();

        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "Domain", "Reference", "Libraries", "DNDV1.dll");
        var assembly = domain.LoadPluginWithHighDependency(path,item=>item.Name!.Contains("PluginBase"));

        var type1 = assembly.GetTypes().Where(item => item.Name == "P1").First();
        IPluginBase plugin1 = (IPluginBase)(Activator.CreateInstance(type1)!);
        //强制加载所有引用
        var result = plugin1!.PluginMethod1();

        var references = domain.References.CombineWithDefaultReferences(DefaultReferences, loadBehavior);
        var sets = new HashSet<PortableExecutableReference>(references);
        sets.ExceptWith(DefaultReferences.GetReferences());
        return sets;
    }
}

