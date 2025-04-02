using Microsoft.Extensions.FileProviders;

namespace Fhir.Fluid.Converter.Providers
{
    internal interface ICDAFileProvider : IFileProvider
    {
        string ReadTemplateFile(string templateName);
        string GetAbsoluteTemplatePath(string templateName);
    }
}
