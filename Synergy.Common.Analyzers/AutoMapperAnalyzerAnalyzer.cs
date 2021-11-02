using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AutoMapperAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AutoMapperAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SYN0001";

        private const string Title = "Call to AutoMapper.Map should have appropriate registration.";
        private const string MessageFormat = "Call {0} has no valid registration.Add CreateMap<{1},{2}>()";
        private const string Description = "Call to AutoMapper.Map should have appropriate registration.";
        private const string Category = "AutoMapper";

        private static readonly DiagnosticDescriptor MapperCallRule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MapperCallRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var mappings = this.FindMappings(compilationContext).ToList();

                compilationContext.RegisterSyntaxNodeAction(node => this.Action(node, mappings), SyntaxKind.InvocationExpression);
            });
        }

        private void Action(SyntaxNodeAnalysisContext obj, IEnumerable<MappingInfo> mappings)
        {
            if (!(obj.Node is InvocationExpressionSyntax invocation))
            {
                return;
            }

            var name = (invocation.Expression as MemberAccessExpressionSyntax)?.Name ?? invocation.Expression as NameSyntax;
            if (name == null)
            {
                return;
            }

            if (!(obj.SemanticModel.GetSymbolInfo(name).Symbol is IMethodSymbol symbolInfo))
            {
                return;
            }
            
            if (string.Equals(symbolInfo.ContainingNamespace.Name, "AutoMapper", StringComparison.CurrentCultureIgnoreCase))
            {
                var isMap = string.Equals(symbolInfo.Name, "Map", StringComparison.CurrentCultureIgnoreCase);
                var isProject = string.Equals(symbolInfo.Name, "ProjectTo", StringComparison.CurrentCultureIgnoreCase);

                if (isMap || isProject)
                {
                    ITypeSymbol from = null, to = null;

                    var typeArgs = symbolInfo.TypeArguments;

                    if (typeArgs.Length == 1)
                    {
                        if (symbolInfo.Parameters.Length == 0)
                        {
                            return;
                        }

                        from = obj.SemanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression).Type;
                        to = typeArgs[0]; ;

                        if (isProject && from?.AllInterfaces.Any(x => x.Name.StartsWith("IQueryable")) == true && from is INamedTypeSymbol named)
                        {
                            if (named.TypeArguments.Length == 0)
                            {
                                return;
                            }
                            from = named.TypeArguments[0];
                        }
                        
                    }
                    else
                    {
                        if (symbolInfo.Parameters.Length < 2)
                        {
                            return;
                        }

                        from = symbolInfo.Parameters[0].Type; 
                        to = symbolInfo.Parameters[1].Type;
                    }

                    if (from == null || to == null)
                    {
                        return;
                    }

                    if (from.AllInterfaces.Any(x => x.ToDisplayString() == "System.Collections.IEnumerable")
                      && to.AllInterfaces.Any(x => x.ToDisplayString() == "System.Collections.IEnumerable"))
                    {
                        from = (from as IArrayTypeSymbol)?.ElementType ?? (from as INamedTypeSymbol)?.TypeArguments[0];
                        to = (to as IArrayTypeSymbol)?.ElementType ?? (to as INamedTypeSymbol)?.TypeArguments[0];

                    }

                    if (mappings.Any(x => x.Src.Type.Equals(@from) && x.Dst.Type.Equals(to)) == false)
                    {
                        var diagnostic = Diagnostic.Create(MapperCallRule, invocation.GetLocation(), invocation.ToString(), from.ToDisplayString(), to.ToDisplayString());

                        obj.ReportDiagnostic(diagnostic);
                    }

                    
                }

                
            }
        }

#pragma warning disable RS1012 // Start action has no registered actions.
        private IEnumerable<MappingInfo> FindMappings(CompilationStartAnalysisContext compilationContext)
#pragma warning restore RS1012 // Start action has no registered actions.
        {
            var types = compilationContext.Compilation.GetSymbolsWithName(x => true, SymbolFilter.Type);

            foreach (var symbol in types.OfType<INamedTypeSymbol>())
            {
                if (symbol.TypeKind == TypeKind.Class && symbol.BaseType.Name == "Profile")
                {
                    foreach (var method in symbol.ConstructedFrom.GetMembers().OfType<IMethodSymbol>())
                    {
                        var s = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

                        var methodBody = (s as MethodDeclarationSyntax)?.Body ?? (s as ConstructorDeclarationSyntax)?.Body;

                        if (methodBody == null)
                            continue;

                        var methods = methodBody.DescendantNodes().OfType<InvocationExpressionSyntax>();
                        var semanticModel = compilationContext.Compilation.GetSemanticModel(methodBody.SyntaxTree);

                        foreach (var invocationExpressionSyntax in methods)
                        {
                            GenericNameSyntax genericName = null;

                            switch (invocationExpressionSyntax.Expression)
                            {
                                case MemberAccessExpressionSyntax exp:
                                {
                                    genericName = exp.Name as GenericNameSyntax;
                                    break;
                                }
                                case GenericNameSyntax name:
                                {
                                    genericName = name;
                                    break;
                                }
                            }
                            
                            if (genericName != null)
                            {
                                if (genericName.Identifier.ValueText == "CreateMap")
                                {
                                    if (genericName.TypeArgumentList.Arguments.Count == 2)
                                    {
                                        yield return new MappingInfo
                                        {
                                            Src = semanticModel.GetTypeInfo(genericName.TypeArgumentList.Arguments[0]),
                                            Dst = semanticModel.GetTypeInfo(genericName.TypeArgumentList.Arguments[1]),
                                        };
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        class MappingInfo
        {
            public TypeInfo Src { get; set; }

            public TypeInfo Dst { get; set; }
        }
    }
}
