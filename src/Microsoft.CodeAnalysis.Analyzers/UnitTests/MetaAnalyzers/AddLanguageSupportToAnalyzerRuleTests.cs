﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Analyzers.UnitTests.MetaAnalyzers
{
    public class AddLanguageSupportToAnalyzerRuleTests : CodeFixTestBase
    {
        [Fact]
        public void CSharp_VerifyDiagnostic()
        {
            var source = @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp, ""MyLanguage"")]
class MyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}";
            DiagnosticResult expected = GetCSharpExpectedDiagnostic(7, 2, "MyAnalyzer", missingLanguageName: LanguageNames.VisualBasic);

            // Verify diagnostic if analyzer assembly doesn't reference C# code analysis assembly.
            VerifyCSharp(source, referenceFlags: ReferenceFlags.RemoveCodeAnalysis, expected: expected);

            // Verify no diagnostic if analyzer assembly references C# code analysis assembly.
            VerifyCSharp(source, referenceFlags: ReferenceFlags.None);
        }

        [Fact]
        public void VisualBasic_VerifyDiagnostic()
        {
            var source = @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(LanguageNames.VisualBasic, ""MyLanguage"")>
Class MyAnalyzer
	Inherits DiagnosticAnalyzer
	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Throw New NotImplementedException()
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class
";
            DiagnosticResult expected = GetBasicExpectedDiagnostic(7, 2, "MyAnalyzer", missingLanguageName: LanguageNames.CSharp);

            // Verify diagnostic if analyzer assembly doesn't reference VB code analysis assembly.
            VerifyBasic(source, referenceFlags: ReferenceFlags.RemoveCodeAnalysis, expected: expected);

            // Verify no diagnostic if analyzer assembly references VB code analysis assembly.
            VerifyBasic(source, referenceFlags: ReferenceFlags.None);
        }

        [Fact]
        public void CSharp_NoDiagnosticCases()
        {
            var source = @"
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(""MyLanguage"")]
class MyAnalyzerWithCustomLanguageAttribute : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
class MyAnalyzerWithBothLanguages : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public override void Initialize(AnalysisContext context)
    {
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public abstract class MyAbstractAnalyzer : DiagnosticAnalyzer
{
}
";
            VerifyCSharp(source, referenceFlags: ReferenceFlags.RemoveCodeAnalysis);
            VerifyCSharp(source, referenceFlags: ReferenceFlags.None);
        }

        [Fact]
        public void VisualBasic_NoDiagnosticCases()
        {
            var source = @"
Imports System
Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics

<DiagnosticAnalyzer(""MyLanguage"")>
Class MyAnalyzerWithCustomLanguageAttribute
	Inherits DiagnosticAnalyzer
	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Throw New NotImplementedException()
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class

<DiagnosticAnalyzer(LanguageNames.VisualBasic, LanguageNames.CSharp)>
Class MyAnalyzerWithBothLanguages
	Inherits DiagnosticAnalyzer
	Public Overrides ReadOnly Property SupportedDiagnostics() As ImmutableArray(Of DiagnosticDescriptor)
		Get
			Throw New NotImplementedException()
		End Get
	End Property

	Public Overrides Sub Initialize(context As AnalysisContext)
	End Sub
End Class

<DiagnosticAnalyzer(LanguageNames.VisualBasic)>
Public MustInherit Class MyAbstractAnalyzer
	Inherits DiagnosticAnalyzer
End Class
";
            VerifyBasic(source, referenceFlags: ReferenceFlags.RemoveCodeAnalysis);
            VerifyBasic(source, referenceFlags: ReferenceFlags.None);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return null;
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return null;
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DiagnosticAnalyzerAttributeAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DiagnosticAnalyzerAttributeAnalyzer();
        }

        private static DiagnosticResult GetCSharpExpectedDiagnostic(int line, int column, string analyzerTypeName, string missingLanguageName)
        {
            return GetExpectedDiagnostic(LanguageNames.CSharp, line, column, analyzerTypeName, missingLanguageName);
        }

        private static DiagnosticResult GetBasicExpectedDiagnostic(int line, int column, string analyzerTypeName, string missingLanguageName)
        {
            return GetExpectedDiagnostic(LanguageNames.VisualBasic, line, column, analyzerTypeName, missingLanguageName);
        }

        private static DiagnosticResult GetExpectedDiagnostic(string language, int line, int column, string analyzerTypeName, string missingLanguageName)
        {
            string fileName = language == LanguageNames.CSharp ? "Test0.cs" : "Test0.vb";
            return new DiagnosticResult
            {
                Id = DiagnosticIds.AddLanguageSupportToAnalyzerRuleId,
                Message = string.Format(CodeAnalysisDiagnosticsResources.AddLanguageSupportToAnalyzerMessage, analyzerTypeName, missingLanguageName),
                Severity = DiagnosticHelpers.DefaultDiagnosticSeverity,
                Locations = new[]
                {
                    new DiagnosticResultLocation(fileName, line, column)
                }
            };
        }
    }
}
