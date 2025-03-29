namespace Fhir.Fluid.Converter.Parsers.Options
{
    public class CCDParserOptions
    {
        /// <summary>
        /// Path to directory of liquid templates
        /// </summary>
        public string TemplateDirectoryPath { get; set; }

        /// <summary>
        /// RootTemplate name relative to the TemplateDirectoryPath
        /// </summary>
        public string RootTemplate { get; set; }

        /// <summary>
        /// Controls whether to use the CachedCDAFileProvider
        /// </summary>
        public bool UseCachedFileProvider { get; set; } = true;
    }
}
