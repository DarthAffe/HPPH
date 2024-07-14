namespace HPPH;

/// <summary>
/// Represents a list of rows of an image.
/// </summary>
public interface IImageRows : IEnumerable<IImageRow>
{
    /// <summary>
    /// Gets the amount of rows in this list.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets a specific <see cref="IImageRow"/>.
    /// </summary>
    /// <param name="column">The ´row to get.</param>
    /// <returns>The requested <see cref="IImageRow"/>.</returns>
    IImageRow this[int column] { get; }
}
