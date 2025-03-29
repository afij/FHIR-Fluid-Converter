using Microsoft.Extensions.FileProviders;

namespace Fhir.Fluid.Converter.Providers
{
    interface ICDAFileProvider : IFileProvider
    {
        string ReadTemplateFile(string templateName);
        string GetAbsoluteTemplatePath(string templateName);
    }
}
