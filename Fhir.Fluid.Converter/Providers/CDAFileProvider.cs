using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;

namespace Fhir.Fluid.Converter.Providers
{
    /// <summary>
    /// Custom IFileProvider used to access files for include and render statements
    /// </summary>
    internal class CDAFileProvider : ICDAFileProvider
    {
        private readonly PhysicalFileProvider _innerProvider;

        public CDAFileProvider(string root)
        {
            _innerProvider = new PhysicalFileProvider(root);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var correctedSubpath = GetAbsoluteTemplatePath(subpath);
            return _innerProvider.GetFileInfo(correctedSubpath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _innerProvider.GetDirectoryContents(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return _innerProvider.Watch(filter);
        }

        public string ReadTemplateFile(string templateName)
        {
            try
            {
                var fileInfo = GetFileInfo(templateName);
                string templateContent = null;
                if (fileInfo.Exists)
                {
                    using StreamReader reader = new(fileInfo.CreateReadStream());
                    templateContent = reader.ReadToEnd();
                }
                return templateContent;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetAbsoluteTemplatePath(string templateName)
        {
            // Remove .liquid from end of path before routing logic
            if (templateName.EndsWith(".liquid"))
            {
                templateName = templateName.Substring(0, templateName.LastIndexOf(".liquid"));
            }
            //var result = _innerProvider.Root;
            var pathSegments = templateName.Split(Path.AltDirectorySeparatorChar);

            // Root templates
            if (pathSegments.Length == 1)
            {
                //if (pathSegments[0].EndsWith(".liquid"))
                //    return pathSegments[0];
                //return Path.Join(_innerProvider.Root, $"{pathSegments[0]}.liquid");
                return $"{pathSegments[0]}.liquid";
            }

            // Snippets
            pathSegments[^1] = IsCodeMappingTemplate(templateName) ? $"{pathSegments[^1]}.json" : $"_{pathSegments[^1]}.liquid";

            // return pathSegments.Aggregate(result, Path.Join);
            return string.Join(Path.AltDirectorySeparatorChar, pathSegments);
        }

        private static bool IsCodeMappingTemplate(string templateName)
        {
            return string.Equals("CodeSystem/CodeSystem", templateName, StringComparison.InvariantCultureIgnoreCase) ||
                   string.Equals("ValueSet/ValueSet", templateName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
