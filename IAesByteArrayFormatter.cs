using System.IO;

namespace Forge.Formatters
{

    /// <summary>
    /// Service interface for the formatter
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    public interface IAesByteArrayFormatter : IDataFormatter<Stream>
    {
    }

}
