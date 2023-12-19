﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using MSTest.Analyzers.Helpers;

namespace MSTest.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseParallelizeAttributeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableResourceString Title = new(nameof(Resources.UseParallelizeAttributeAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableResourceString MessageFormat = new(nameof(Resources.UseParallelizeAttributeAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableResourceString Description = new(nameof(Resources.UseParallelizeAttributeAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    internal static readonly DiagnosticDescriptor Rule = new(
        DiagnosticIds.UseParallelizedAttributeRuleId,
        Title,
        MessageFormat,
        Categories.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        Description,
        $"https://github.com/microsoft/testfx/blob/main/docs/analyzers/{DiagnosticIds.UseParallelizedAttributeRuleId}.md",
        WellKnownCustomTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var parallelizeAttributeSymbol = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftVisualStudioTestToolsUnitTestingParallelizeAttribute);
        var doNotParallelizeAttributeSymbol = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftVisualStudioTestToolsUnitTestingDoNotParallelizeAttribute);

        bool hasParallelizeAttribute = false;
        bool hasDoNotParallelizeAttribute = false;
        foreach (var attribute in context.Compilation.Assembly.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, parallelizeAttributeSymbol))
            {
                hasParallelizeAttribute = true;
            }

            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, doNotParallelizeAttributeSymbol))
            {
                hasDoNotParallelizeAttribute = true;
            }
        }

        if (!hasParallelizeAttribute && !hasDoNotParallelizeAttribute)
        {
            // We cannot provide any good location for assembly level missing attributes
            context.ReportDiagnostic(Diagnostic.Create(Rule, Location.None));
        }
    }
}