using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HPPH;

public static unsafe partial class PixelHelper
{
    #region Methods

    public static Span<TTarget> Convert<TSource, TTarget>(ReadOnlySpan<TSource> data)
        where TSource : struct, IColor
        where TTarget : struct, IColor
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        Span<TSource> dataCopy = new TSource[data.Length];
        data.CopyTo(dataCopy);

        return Convert<TSource, TTarget>(dataCopy);
    }

    public static Span<TTarget> Convert<TSource, TTarget>(Span<TSource> data)
        where TSource : struct, IColor
        where TTarget : struct, IColor
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        Convert(MemoryMarshal.AsBytes(data), TSource.ColorFormat, TTarget.ColorFormat);

        return MemoryMarshal.Cast<TSource, TTarget>(data);
    }

    internal static void Convert(Span<byte> data, IColorFormat sourceFormat, IColorFormat targetFormat)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        ArgumentNullException.ThrowIfNull(sourceFormat);
        ArgumentNullException.ThrowIfNull(targetFormat);

        if (sourceFormat == targetFormat) return;

        if (sourceFormat.BytesPerPixel == targetFormat.BytesPerPixel)
            ConvertEqualBpp(data, sourceFormat, targetFormat);
        else if ((sourceFormat.BytesPerPixel == 3) && (targetFormat.BytesPerPixel == 4))
            ConvertWiden3To4Bytes(data, sourceFormat, targetFormat);
        else if ((sourceFormat.BytesPerPixel == 4) && (targetFormat.BytesPerPixel == 3))
            ConvertNarrow4To3Bytes(data, sourceFormat, targetFormat);
        else
            throw new NotSupportedException("Data is not of a supported valid color-type.");
    }

    private static void ConvertEqualBpp(Span<byte> data, IColorFormat sourceFormat, IColorFormat targetFormat)
    {
        ReadOnlySpan<byte> sourceMapping = sourceFormat.ByteMapping;
        ReadOnlySpan<byte> targetMapping = targetFormat.ByteMapping;

        switch (sourceFormat.BytesPerPixel)
        {
            case 3:
                ReadOnlySpan<byte> mapping3 = [targetMapping[sourceMapping[0]], targetMapping[sourceMapping[1]], targetMapping[sourceMapping[2]]];
                ReadOnlySpan<byte> mask3 =
                [
                    mapping3[0],
                    mapping3[1],
                    mapping3[2],

                    (byte)(mapping3[0] + 3),
                    (byte)(mapping3[1] + 3),
                    (byte)(mapping3[2] + 3),

                    (byte)(mapping3[0] + 6),
                    (byte)(mapping3[1] + 6),
                    (byte)(mapping3[2] + 6),

                    (byte)(mapping3[0] + 9),
                    (byte)(mapping3[1] + 9),
                    (byte)(mapping3[2] + 9),

                    (byte)(mapping3[0] + 12),
                    (byte)(mapping3[1] + 12),
                    (byte)(mapping3[2] + 12),

                    15
                ];

                ConvertEqualBpp(data, mask3, 3);
                break;

            case 4:
                ReadOnlySpan<byte> mapping4 = [targetMapping[sourceMapping[0]], targetMapping[sourceMapping[1]], targetMapping[sourceMapping[2]], targetMapping[sourceMapping[3]]];
                ReadOnlySpan<byte> mask4 =
                [
                    mapping4[0],
                    mapping4[1],
                    mapping4[2],
                    mapping4[3],

                    (byte)(mapping4[0] + 4),
                    (byte)(mapping4[1] + 4),
                    (byte)(mapping4[2] + 4),
                    (byte)(mapping4[3] + 4),

                    (byte)(mapping4[0] + 8),
                    (byte)(mapping4[1] + 8),
                    (byte)(mapping4[2] + 8),
                    (byte)(mapping4[3] + 8),

                    (byte)(mapping4[0] + 12),
                    (byte)(mapping4[1] + 12),
                    (byte)(mapping4[2] + 12),
                    (byte)(mapping4[3] + 12),
                ];

                ConvertEqualBpp(data, mask4, 4);
                break;

            default:
                throw new NotSupportedException("Data is not of a supported valid color-type.");
        }
    }

    // DarthAffe 07.07.2024: No fallback-implementation here. Shuffle Requires only Ssse3 which should be supported nearly anywhere and if not the fallback of Vector128.Shuffle is perfectly fine.
    private static void ConvertEqualBpp(Span<byte> data, ReadOnlySpan<byte> mask, int bpp)
    {
        int elementsPerVector = Vector128<byte>.Count / bpp;
        int bytesPerVector = elementsPerVector * bpp;

        int chunks = data.Length / bytesPerVector;
        Vector128<byte> maskVector = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(mask));

        int missingElements = (data.Length - (chunks * bytesPerVector)) / bpp;

        fixed (byte* dataPtr = data)
        {
            byte* ptr = dataPtr;

            for (int i = 0; i < chunks; i++)
            {
                Vector128<byte> vector = Vector128.Load(ptr);
                Vector128.Shuffle(vector, maskVector).Store(ptr);

                ptr += bytesPerVector;
            }

            Span<byte> buffer = stackalloc byte[missingElements * bpp]; // DarthAffe 07.07.2024: This is fine as it's always < 16 bytes
            for (int i = 0; i < missingElements; i++)
            {
                int elementIndex = i * buffer.Length;
                for (int j = 0; j < buffer.Length; j++)
                    buffer[elementIndex + j] = ptr[elementIndex + mask[j]];
            }

            buffer.CopyTo(new Span<byte>(ptr, buffer.Length));
        }
    }

    private static void ConvertWiden3To4Bytes(Span<byte> data, IColorFormat sourceFormat, IColorFormat targetFormat)
    {
        throw new NotImplementedException();
    }

    private static void ConvertNarrow4To3Bytes(Span<byte> data, IColorFormat sourceFormat, IColorFormat targetFormat)
    {
        throw new NotImplementedException();
    }

    #endregion
}
