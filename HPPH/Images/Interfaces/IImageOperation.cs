namespace HPPH;

/// <summary>
/// Represents an operation on an image that works with every color format.
/// </summary>
/// <remarks>
/// Pass it to <see cref="IImage.Execute{TOperation, TResult}(TOperation)"/> to run it on an <see cref="IImage"/>.
/// <para>
/// Implement it as a struct to run it without allocations. A ref struct can be used to pass spans, for example a destination buffer, into the operation.
/// </para>
/// </remarks>
/// <typeparam name="TResult">The type of the result of the operation.</typeparam>
/// <example>
/// <code>
/// readonly struct MaxRed : IImageOperation&lt;byte&gt;
/// {
///     public byte Execute&lt;T&gt;(RefImage&lt;T&gt; image) where T : unmanaged, IColor&lt;T&gt;
///     {
///         byte max = 0;
///         foreach (T color in image)
///             max = Math.Max(max, color.R);
///
///         return max;
///     }
/// }
///
/// byte maxRed = image.Execute&lt;MaxRed, byte&gt;(default);
/// </code>
/// </example>
public interface IImageOperation<out TResult>
{
    /// <summary>
    /// Executes the operation on the given image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <typeparam name="T">The color type of the image.</typeparam>
    /// <returns>The result of the operation.</returns>
    TResult Execute<T>(RefImage<T> image) where T : unmanaged, IColor<T>;
}
