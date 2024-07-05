using System.Collections.Generic;
using System.Collections.Immutable;

namespace HPPH.Generators;

internal interface IGeneratorFeature
{
    public IEnumerable<(string name, string source)> GenerateFor(ColorFormatData colorFormat);
    public IEnumerable<(string name, string source)> GenerateFor(ImmutableArray<ColorFormatData> colorFormats);
}