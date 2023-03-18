namespace M64RPFW.Services.Abstractions;

/// <summary>
/// A <see langword="record"/> representing an option in the file picker dropdown.
/// </summary>
/// <param name="Name">The label in the file picker used for this type.</param>
/// <param name="Patterns">A list of glob patterns that will match this option. (e.g. *.png)</param>
/// <param name="AppleTypeIds">
/// A list of <see href="https://developer.apple.com/documentation/uniformtypeidentifiers">uniform type identifiers</see>
/// that will match this option. Only used on MacOS, so can be left empty for the time being.
/// </param>
public record FilePickerOption(string Name, string[]? Patterns = null, string[]? AppleTypeIds = null);