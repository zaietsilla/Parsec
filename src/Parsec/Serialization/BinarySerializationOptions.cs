using System.Text;
using Parsec.Common;

namespace Parsec.Serialization;

/// <summary>
/// Defines the options to be used when serializing or deserializing a file
/// <remarks>Although this should be immutable, properties change while reading some formats because that's how the original authors designed them.</remarks>
/// </summary>
public class BinarySerializationOptions
{
    public BinarySerializationOptions()
    {
    }

    public BinarySerializationOptions(Episode episode)
    {
        Episode = episode;
    }

    public BinarySerializationOptions(Encoding encoding)
    {
        Encoding = encoding;
    }

    public BinarySerializationOptions(Episode episode, Encoding? encoding)
    {
        Episode = episode;
        Encoding = encoding ?? Encoding.ASCII;
    }

    public Episode Episode { get; set; } = Episode.Unknown;

    public Encoding Encoding { get; set; } = Encoding.ASCII;

    public object? ExtraOption { get; set; }

    public static BinarySerializationOptions Default => new();
}
