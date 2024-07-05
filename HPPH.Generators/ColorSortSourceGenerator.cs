using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HPPH.Generators;

[Generator(LanguageNames.CSharp)]
public class ColorSortSourceGenerator : IIncrementalGenerator
{
    #region Properties & Fields

    private const string SORT_GENERATOR_ATTRIBUTE_SOURCE = """
                                                            namespace HPPH;
                                                            
                                                            [AttributeUsage(AttributeTargets.Method)]
                                                            internal class ColorSortGeneratorAttribute(string dataTypeName, string sortValueName) : Attribute
                                                            {
                                                                public string DataTypeName { get; } = dataTypeName;
                                                                public string SortValueName { get; } = sortValueName;
                                                            }
                                                            """;

    #endregion

    #region Methods

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //if (!Debugger.IsAttached)
        //    Debugger.Launch();

        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("ColorSortGeneratorAttribute.g.cs", SourceText.From(SORT_GENERATOR_ATTRIBUTE_SOURCE, Encoding.UTF8)));

        IncrementalValueProvider<ImmutableArray<ColorSortData>> classes = context.SyntaxProvider
                                                                                   .ForAttributeWithMetadataName("HPPH.ColorSortGeneratorAttribute", static (_, __) => true, Transform)
                                                                                   .Where(type => type.HasValue)
                                                                                   .Select((data, _) => data!.Value)
                                                                                   .Collect();
        context.RegisterSourceOutput(classes, GenerateCode);
    }

    private static ColorSortData? Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetNode is not MethodDeclarationSyntax methodDeclaration) return null;
        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken) is not IMethodSymbol method) return null;
        if (!method.IsPartialDefinition) return null;

        foreach (AttributeData attribute in method.GetAttributes())
        {
            if (attribute.AttributeClass?.Name != "ColorSortGeneratorAttribute")
                continue;

            if (!attribute.ConstructorArguments.IsEmpty)
            {
                ImmutableArray<TypedConstant> args = attribute.ConstructorArguments;
                if (args.Any(arg => arg.Kind == TypedConstantKind.Error)) break;

                return new ColorSortData(method.ContainingNamespace.Name, method.ContainingType.Name, ((ClassDeclarationSyntax)methodDeclaration.Parent!).Modifiers.ToString(), $"{methodDeclaration.Modifiers} {methodDeclaration.ReturnType} {methodDeclaration.Identifier}{methodDeclaration.TypeParameterList}{methodDeclaration.ParameterList} {methodDeclaration.ConstraintClauses}", args[0].Value!.ToString(), args[1].Value!.ToString());
            }
        }

        return null;
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<ColorSortData> colorSorts)
    {
        if (colorSorts.IsDefaultOrEmpty) return;

        Dictionary<(string, string, string), string> sourceMapping = colorSorts.Select(x => (x.Namespace, x.Class, x.ClassModifiers)).Distinct().ToDictionary(x => x, _ => string.Empty);

        foreach (ColorSortData colorSort in colorSorts)
            sourceMapping[(colorSort.Namespace, colorSort.Class, colorSort.ClassModifiers)] += GenerateSortMethodSource(colorSort);

        foreach (KeyValuePair<(string @namespace, string @class, string classModifier), string> data in sourceMapping)
        {
            context.AddSource($"{data.Key.@class}.g.cs", SourceText.From($$"""
                                                                           using System.Buffers;
                                                                           
                                                                           namespace {{data.Key.@namespace}};
                                                                           
                                                                           {{data.Key.classModifier}} class {{data.Key.@class}}
                                                                           {
                                                                           {{data.Value}}
                                                                           }
                                                                           """, Encoding.UTF8));
        }
    }

    private static string GenerateSortMethodSource(ColorSortData colorSort)
    => $$"""
             {{colorSort.Signature}}
             {
                 fixed ({{colorSort.DataTypeName}}* ptr = colors)
                 {
                     {{colorSort.DataTypeName}}* end = ptr + colors.Length;
             
                     Span<int> histogram = stackalloc int[256];
                     histogram.Clear();
                     for ({{colorSort.DataTypeName}}* color = ptr; color < end; color++)
                         histogram[(*color).{{colorSort.SortValueName}}]++;
             
                     {{colorSort.DataTypeName}}[] bucketsArray = ArrayPool<{{colorSort.DataTypeName}}>.Shared.Rent(colors.Length);
                     try
                     {
                         Span<{{colorSort.DataTypeName}}> buckets = bucketsArray.AsSpan()[..colors.Length];
                         Span<int> currentBucketIndex = stackalloc int[256];
             
                         int offset = 0;
                         for (int i = 0; i < histogram.Length; i++)
                         {
                             currentBucketIndex[i] = offset;
                             offset += histogram[i];
                         }
             
                         for ({{colorSort.DataTypeName}}* color = ptr; color < end; color++)
                             buckets[currentBucketIndex[(*color).{{colorSort.SortValueName}}]++] = (*color);
             
                         buckets.CopyTo(colors);
                     }
                     finally
                     {
                         ArrayPool<{{colorSort.DataTypeName}}>.Shared.Return(bucketsArray);
                     }
                 }
             }
         
         """;

    #endregion
}