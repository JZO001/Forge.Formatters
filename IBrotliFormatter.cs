#if NETSTANDARD2_1 || NETCOREAPP3_1_OR_GREATER

namespace Forge.Formatters
{

    /// <summary>GZip formatter</summary>
    public interface IBrotliFormatter : IDataFormatter<byte[]>
    {
    }

}

#endif
