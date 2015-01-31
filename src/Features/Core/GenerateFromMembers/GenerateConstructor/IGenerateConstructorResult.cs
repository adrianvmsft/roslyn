﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace Microsoft.CodeAnalysis.GenerateFromMembers.GenerateConstructor
{
    internal interface IGenerateConstructorResult
    {
        bool ContainsChanges { get; }
        CodeRefactoring GetCodeRefactoring(CancellationToken cancellationToken);
    }
}
