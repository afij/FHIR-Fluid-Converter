using Microsoft.Extensions.FileProviders;

namespace FluidCdaTest.Providers
{
    interface ICDAFileProvider : IFileProvider
    {
        string ReadTemplateFile(string templateName);
        string GetAbsoluteTemplatePath(string templateName);
    }
}
