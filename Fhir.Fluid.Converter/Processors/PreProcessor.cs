using Fhir.Fluid.Converter.Converters;
using Fhir.Fluid.Converter.CustomRegex;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Fhir.Fluid.Converter.Processors
{
    public static class PreProcessor
    {
        /// <summary>
        /// Converts a JSON string into a cleaned dictionary object
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static object ParseToObject(string document)
        {
            if (string.IsNullOrWhiteSpace(document))
            {
                throw new ArgumentNullException(nameof(document));
            }

            try
            {
                var xDocument = XDocument.Parse(document);
                // Strip whitepsace from original data to lower memory footprint once GZIP'd
                var originalData = xDocument.ToString(SaveOptions.DisableFormatting);

                XElement root = xDocument.Root ?? throw new InvalidOperationException("Invalid XML structure: Root element is missing.");

                // Remove redundant namespaces to avoid appending namespace prefix before elements
                var defaultNamespace = xDocument.Root?.GetDefaultNamespace().NamespaceName;
                xDocument.Root?.Attributes()
                    .Where(attribute => IsRedundantNamespaceAttribute(attribute, defaultNamespace))
                    .Remove();

                // Normalize non-default namespace prefix in elements
                var namespaces = xDocument.Root?.Attributes()
                    .Where(x => x.IsNamespaceDeclaration && x.Value != defaultNamespace);
                NormalizeNamespacePrefix(root, namespaces);

                // Change XText to XElement with name "_" to avoid serializing depth difference, e.g., given="foo" and given.#text="foo"
                ReplaceTextWithElement(root);

                // Convert to json dictionary
                var jsonString = JsonConvert.SerializeXNode(xDocument);
                //return xDocument.Root;
                var dataDictionary = JsonConvert.DeserializeObject<IDictionary<string, object>>(jsonString, new DictionaryJsonConverter()) ??
                                     new Dictionary<string, object>();

                // Remove line breaks in original data
                dataDictionary["_originalData"] = CCDRegex.InvalidLineBreakRegex().Replace(originalData, string.Empty);

                return dataDictionary;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static bool IsRedundantNamespaceAttribute(XAttribute attribute, string defaultNamespace)
        {
            return attribute != null &&
                   attribute.IsNamespaceDeclaration &&
                   !string.Equals(attribute.Name.LocalName, "xmlns", StringComparison.InvariantCultureIgnoreCase) &&
                   string.Equals(attribute.Value, defaultNamespace, StringComparison.InvariantCultureIgnoreCase);
        }

        private static void NormalizeNamespacePrefix(XElement element, IEnumerable<XAttribute> namespaces)
        {
            if (element == null || namespaces == null)
            {
                return;
            }

            foreach (var ns in namespaces)
            {
                if (string.Equals(ns.Value, element.Name.NamespaceName, StringComparison.InvariantCultureIgnoreCase))
                {
                    element.Name = $"{ns.Name.LocalName}_{element.Name.LocalName}";
                    break;
                }
            }

            foreach (var childElement in element.Elements())
            {
                NormalizeNamespacePrefix(childElement, namespaces);
            }
        }

        private static void ReplaceTextWithElement(XElement element)
        {
            if (element == null)
            {
                return;
            }

            // Iterate reversely as the list itself is updating
            foreach (var node in element.Nodes().ToArray())
            {
                switch (node)
                {
                    case XText textNode:
                        element.Add(new XElement("_", textNode.Value));
                        textNode.Remove();
                        break;
                    case XElement elementNode:
                        ReplaceTextWithElement(elementNode);
                        break;
                }
            }
        }
    }
}
