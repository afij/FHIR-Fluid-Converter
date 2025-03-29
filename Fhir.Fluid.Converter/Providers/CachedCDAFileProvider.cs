using Fhir.Fluid.Converter.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace Fhir.Fluid.Converter.Providers
{
    /// <summary>
    /// Custom IFileProvider that caches file content to avoid rereading the disk during execution. Used to access files for include and render statements
    /// </summary>
    public class CachedCDAFileProvider : ICDAFileProvider
    {
        private readonly PhysicalFileProvider _innerProvider;
        private readonly ConcurrentDictionary<string, CachedFileEntry> _cache;

        public CachedCDAFileProvider(string root)
        {
            _innerProvider = new PhysicalFileProvider(root);
            _cache = new ConcurrentDictionary<string, CachedFileEntry>();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var correctedSubpath = GetAbsoluteTemplatePath(subpath);
            var filePath = Path.Combine(_innerProvider.Root, correctedSubpath);
            //var fileInfo = _innerProvider.GetFileInfo(correctedSubpath);
            //return fileInfo;

            if (_cache.TryGetValue(filePath, out CachedFileEntry entry))
            {
                return entry.FileInfo;
            }
            else
            {
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    var notFoundInfo = new NotFoundFileInfo(subpath);
                    _cache[filePath] = new CachedFileEntry
                    {
                        FileInfo = notFoundInfo
                    };
                    return notFoundInfo;
                }

                var cachedFileInfo = new CachedFileInfo(fileInfo, this);
                var content = File.ReadAllBytes(filePath);
                _cache[filePath] = new CachedFileEntry
                {
                    FileInfo = cachedFileInfo,
                    Content = content
                };
                return cachedFileInfo;
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _innerProvider.GetDirectoryContents(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return _innerProvider.Watch(filter);
        }

        public byte[] GetFileContent(FileInfo fileInfo)
        {
            if (_cache.TryGetValue(fileInfo.FullName, out CachedFileEntry entry))
            {
                return entry.Content;
            }
            else
            {
                var content = File.ReadAllBytes(fileInfo.FullName);
                var cachedFileInfo = new CachedFileInfo(fileInfo, this);
                _cache[fileInfo.FullName] = new CachedFileEntry
                {
                    FileInfo = cachedFileInfo,
                    Content = content
                };
                return content;
            }
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

        private class CachedFileEntry
        {
            public IFileInfo FileInfo { get; set; }
            public byte[] Content { get; set; }
        }
    }
}
