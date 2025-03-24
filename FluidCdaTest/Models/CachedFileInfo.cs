using FluidCdaTest.Providers;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace FluidCdaTest.Models
{
    public class CachedFileInfo : IFileInfo
    {
        private readonly string _filePath;
        private readonly CachedCDAFileProvider _provider;

        public CachedFileInfo(string filePath, CachedCDAFileProvider provider)
        {
            _filePath = filePath;
            _provider = provider;
        }

        public bool Exists => File.Exists(_filePath);

        public long Length => new FileInfo(_filePath).Length;

        public string PhysicalPath => _filePath;

        public string Name => Path.GetFileName(_filePath);

        public DateTimeOffset LastModified => File.GetLastWriteTimeUtc(_filePath);

        public bool IsDirectory => false;

        public Stream CreateReadStream()
        {
            var content = _provider.GetFileContent(_filePath);
            return new MemoryStream(content);
        }
    }
}
