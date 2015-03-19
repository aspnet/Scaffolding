﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator;

namespace Microsoft.Framework.CodeGeneration.Templating
{
    internal class RazorTemplatingHost : RazorEngineHost
    {
        private static readonly string[] _defaultNamespaces = new[]
        {
            "System",
            "System.Linq",
            "System.Collections.Generic",
            "System.Dynamic",
            "Microsoft.Framework.CodeGeneration.Templating",
        };

        public RazorTemplatingHost([NotNull]Type baseType)
            : base(new CSharpRazorCodeLanguage())
        {
            DefaultBaseClass = baseType.FullName;

            //ToDo: Why Do I need templateTypeName? Do I need other parameters?
            GeneratedClassContext = new GeneratedClassContext(
                executeMethodName: "ExecuteAsync",
                writeMethodName: "Write",
                writeLiteralMethodName: "WriteLiteral",
                writeToMethodName: "WriteTo",
                writeLiteralToMethodName: "WriteLiteralTo",
                templateTypeName: "",
                generatedTagHelperContext: new GeneratedTagHelperContext())
            {
            };

            foreach (var ns in _defaultNamespaces)
            {
                NamespaceImports.Add(ns);
            }
        }
    }
}