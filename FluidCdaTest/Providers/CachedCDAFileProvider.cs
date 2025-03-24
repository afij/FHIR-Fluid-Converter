using FluidCdaTest.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace FluidCdaTest.Providers
{
    public class CachedCDAFileProvider : IFileProvider
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
                if (!File.Exists(filePath))
                {
                    var notFoundInfo = new NotFoundFileInfo(subpath);
                    _cache[filePath] = new CachedFileEntry
                    {
                        FileInfo = notFoundInfo
                    };
                    return notFoundInfo;
                }

                var fileInfo = new CachedFileInfo(filePath, this);
                var content = File.ReadAllBytes(filePath);
                _cache[filePath] = new CachedFileEntry
                {
                    FileInfo = fileInfo,
                    Content = content
                };
                return fileInfo;
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

        public byte[] GetFileContent(string filePath)
        {
            if (_cache.TryGetValue(filePath, out CachedFileEntry entry))
            {
                return entry.Content;
            }
            else
            {
                var content = File.ReadAllBytes(filePath);
                var fileInfo = new CachedFileInfo(filePath, this);
                _cache[filePath] = new CachedFileEntry
                {
                    FileInfo = fileInfo,
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
                    using (StreamReader reader = new StreamReader(fileInfo.CreateReadStream()))
                    {
                        templateContent = reader.ReadToEnd();
                    }
                }
                return templateContent;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetAbsoluteTemplatePath(string templateName)
        {
            // Remove .liquid from end of path before routing logic
            if (templateName.EndsWith(".liquid"))
            {
                templateName = templateName.Substring(0, templateName.LastIndexOf(".liquid"));
            }
            var result = _innerProvider.Root;
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
