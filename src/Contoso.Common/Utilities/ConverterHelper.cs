using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Linq;

namespace Contoso.Common.Utilities
{
    /// <summary>
    /// Helper class for converting various things
    /// </summary>
    public class ConverterHelper
    {
        /// <summary>
        /// Converts a JSON string to a XElement, that can be queried with XPath
        /// </summary>
        /// <param name="stringContent">the JSON string</param>
        /// <returns>converted XElement</returns>
        public static XElement ConvertJsonStringToXElement(string stringContent)
        {
            var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(stringContent),
                new System.Xml.XmlDictionaryReaderQuotas());
            var xElement = XElement.Load(jsonReader);
            return xElement;
        }
    }
}
