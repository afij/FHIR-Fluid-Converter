using Fluid;

namespace FluidCdaTest.Filters
{
    public static class CustomFilters
    {
        public static void RegisterCustomFilters(this FilterCollection filters)
        {
            filters.RegisterGeneralFilters();
            filters.RegisterStringFilters();
            filters.RegisterSectionFilters();
            filters.RegisterCollectionFilters();
            filters.RegisterDateFilters();
            filters.RegisterMiscFilters();
        }
    }
}
