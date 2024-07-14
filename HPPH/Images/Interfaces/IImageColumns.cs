namespace HPPH;

/// <summary>
/// Represents a list of columns of an image.
/// </summary>
public interface IImageColumns : IEnumerable<IImageColumn>
{
    /// <summary>
    /// Gets the amount of columns in this list.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets a specific <see cref="IImageColumn"/>.
    /// </summary>
    /// <param name="column">The column to get.</param>
    /// <returns>The requested <see cref="IImageColumn"/>.</returns>
    IImageColumn this[int column] { get; }
}