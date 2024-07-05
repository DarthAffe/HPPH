using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HPPH.Generators;

[Generator(LanguageNames.CSharp)]
public class ColorSourceGenerator : IIncrementalGenerator
{
    #region Properties & Fields

    private static readonly Regex COLOR_FORMAT_REGEX = new("^Color(?<colorFormat>[ARGB]{3,4})$", RegexOptions.Compiled);

    private static readonly IGeneratorFeature[] FEATURES =
    [
        new Colors(),
        new Average(),
        new MinMax(),
        new Sum(),
        new Quantize()
    ];

    private const string COLOR_GENERATOR_ATTRIBUTE_SOURCE = """
                                                            namespace HPPH;
                                                            
                                                            [AttributeUsage(AttributeTargets.Struct)]
                                                            internal class ColorGeneratorAttribute : Attribute;
                                                            """;

    #endregion

    #region Methods

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // if (!Debugger.IsAttached)
        //     Debugger.Launch();

        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("ColorGeneratorAttribute.g.cs", SourceText.From(COLOR_GENERATOR_ATTRIBUTE_SOURCE, Encoding.UTF8)));

        IncrementalValueProvider<ImmutableArray<ColorFormatData>> classes = context.SyntaxProvider
                                                                                   .ForAttributeWithMetadataName("HPPH.ColorGeneratorAttribute", static (_, __) => true, Transform)
                                                                                   .Where(type => type.HasValue)
                                                                                   .Select((data, _) => data!.Value)
                                                                                   .Collect();
        context.RegisterSourceOutput(classes, GenerateCode);
    }

    private static ColorFormatData? Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetNode is not StructDeclarationSyntax structDeclaration) return null;
        if (context.SemanticModel.GetDeclaredSymbol(structDeclaration, cancellationToken) is not ITypeSymbol type) return null;

        Match m = COLOR_FORMAT_REGEX.Match(type.Name);
        if (!m.Success) return null;

        string colorFormat = m.Groups["colorFormat"].Value;
        if (colorFormat.Distinct().Count() != colorFormat.Length) return null;

        return new ColorFormatData(type.Name,
                                   colorFormat.Length,
                                   colorFormat.Length > 0 ? colorFormat[0] : ' ',
                                   colorFormat.Length > 1 ? colorFormat[1] : ' ',
                                   colorFormat.Length > 2 ? colorFormat[2] : ' ',
                                   colorFormat.Length > 3 ? colorFormat[3] : ' ');
    }

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<ColorFormatData> colorFormats)
    {
        if (colorFormats.IsDefaultOrEmpty) return;

        foreach (ColorFormatData formatData in colorFormats)
        {
            foreach (IGeneratorFeature feature in FEATURES)
                foreach ((string name, string source) in feature.GenerateFor(formatData))
                    context.AddSource($"{name}.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        foreach (IGeneratorFeature feature in FEATURES)
            foreach ((string name, string source) in feature.GenerateFor(colorFormats))
                context.AddSource($"{name}.g.cs", SourceText.From(source, Encoding.UTF8));
    }
    
    #endregion
}