using Fluid;
using FluidCdaTest.Parsers;

namespace FluidCdaTest.Filters
{
    public static class CustomFilters
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
