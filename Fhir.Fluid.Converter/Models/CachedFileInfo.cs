using Fhir.Fluid.Converter.Providers;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace Fhir.Fluid.Converter.Models
{
    internal class CachedFileInfo : IFileInfo
    {
        private readonly FileInfo _fileInfo;
        private readonly CachedCDAFileProvider _provider;

        public CachedFileInfo(FileInfo fileInfo, CachedCDAFileProvider provider)
        {
            _fileInfo = fileInfo;
            _provider = provider;
        }

        public bool Exists => _fileInfo.Exists;

        public long Length => _fileInfo.Length;

        public string PhysicalPath => _fileInfo.FullName;

        public string Name => _fileInfo.Name;

        public DateTimeOffset LastModified => _fileInfo.LastWriteTimeUtc;

        public bool IsDirectory => false;

        public Stream CreateReadStream()
        {
            var content = _provider.GetFileContent(_fileInfo);
            return new MemoryStream(content);
        }
    }
}
