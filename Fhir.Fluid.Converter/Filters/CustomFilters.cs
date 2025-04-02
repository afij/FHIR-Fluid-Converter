using Fhir.Fluid.Converter.Parsers;
using Fluid;

namespace Fhir.Fluid.Converter.Filters
{
    internal static class CustomFilters
    {
        public static void RegisterCustomFilters(this FilterCollection filters, CCDParser parser)
        {
            filters.RegisterGeneralFilters();
            filters.RegisterStringFilters();
            filters.RegisterSectionFilters();
            filters.RegisterCollectionFilters(parser);
            filters.RegisterDateFilters();
            filters.RegisterMiscFilters();
        }
    }
}
