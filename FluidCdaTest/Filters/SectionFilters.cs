using Fluid.Values;
using Fluid;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using Fluid.Ast;

namespace FluidCdaTest.Filters
{
    public static class SectionFilters
    {
        private static readonly Regex NormalizeSectionNameRegex = new Regex("[^A-Za-z0-9]");

        public static void RegisterSectionFilters(this FilterCollection filters)
        {
            filters.AddFilter("get_first_ccda_sections", GetFirstCcdaSections);
            filters.AddFilter("get_first_ccda_sections_by_template_id", GetFirstCcdaSectionsByTemplateId);
        }

        /// <summary>
        /// Returns first instance of the section by name
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValueTask<FluidValue> GetFirstCcdaSections(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            //var sectionLists = GetCcdaSectionLists(input, arguments.At(0).ToString());
            //var result = new Dictionary<string, object>();
            //foreach (var (key, value) in sectionLists)
            //{
            //    result[key] = (value as List<object>)?.First();
            //}

            //return result;
            return new StringValue("test :)");
        }

        public static IDictionary<string, object> GetCcdaSectionLists(IDictionary<string, object> data, string sectionNameContent)
        {
            var result = new Dictionary<string, object>();
            var sectionNames = sectionNameContent.Split("|", StringSplitOptions.RemoveEmptyEntries);
            var components = GetComponents(data);

            if (components == null)
            {
                return result;
            }

            foreach (var sectionName in sectionNames)
            {
                foreach (var component in components)
                {
                    if (component is Dictionary<string, object> componentDict &&
                        componentDict.GetValueOrDefault("section") is Dictionary<string, object> sectionDict &&
                        sectionDict.GetValueOrDefault("title") is Dictionary<string, object> titleDict &&
                        titleDict.GetValueOrDefault("_") is string titleString &&
                        titleString.Contains(sectionName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var normalizedSectionName = NormalizeSectionName(sectionName);
                        if (result.GetValueOrDefault(normalizedSectionName) is List<object> list)
                        {
                            list.Add(sectionDict);
                        }
                        else
                        {
                            result[NormalizeSectionName(sectionName)] = new List<object> { sectionDict };
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns first instance of the section by template ID
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async ValueTask<FluidValue> GetFirstCcdaSectionsByTemplateId(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            //var result = new Dictionary<string, object>();
            //var templateIds = arguments.At(0).ToString().Split("|", StringSplitOptions.RemoveEmptyEntries);
            //var components = GetComponents(data);

            //if (components == null)
            //{
            //    return result;
            //}

            //foreach (var templateId in templateIds)
            //{
            //    foreach (var component in components)
            //    {
            //        if (component is Dictionary<string, object> componentDict &&
            //            componentDict.GetValueOrDefault("section") is Dictionary<string, object> sectionDict &&
            //            sectionDict.GetValueOrDefault("templateId") != null &&
            //            ToJsonString(sectionDict["templateId"]).Contains(templateId, StringComparison.InvariantCultureIgnoreCase))
            //        {
            //            result[NormalizeSectionName(templateId)] = sectionDict;
            //            break;
            //        }
            //    }
            //}

            //return result;

            var result = new Dictionary<string, object>();
            var templateIds = arguments.At(0).ToStringValue().Split("|", StringSplitOptions.RemoveEmptyEntries);
            var templateValue = arguments.At(0);
            var templateIdString = ((StringValue)arguments.At(0)).ToString();
            var components = await GetComponentsAsync(input as DictionaryValue, context);
            if (components == null)
            {
                return NilValue.Empty;
            }
            foreach (var templateId in templateIds)
            {
                foreach (var component in components.Enumerate(context))
                {
                    var testCompDict = component as DictionaryValue;
                    var testSectionDict = await testCompDict.GetValueAsync("section", context) as DictionaryValue;
                    if (testSectionDict != null)
                    {
                        var testTemplateIdDict = await testSectionDict.GetValueAsync("templateId", context);
                        var testEnum = testTemplateIdDict.Enumerate(context);
                        foreach (var testTemplateId in testEnum)
                        {
                            var testStringValue = await testTemplateId.GetValueAsync("root", context);
                            if (testStringValue.ToStringValue().Contains(templateId, StringComparison.InvariantCultureIgnoreCase)) {
                                result[NormalizeSectionName(templateId)] = testSectionDict;
                            }
                        }
                    }
                    // Still required for accurate results - requires investigation
                    if (component is DictionaryValue componentDict &&
                                await component.GetValueAsync("section", context) is DictionaryValue sectionDict &&
                                await sectionDict.GetValueAsync("templateId", context) is DictionaryValue templateIdDict &&
                                await templateIdDict.GetValueAsync("root", context) is StringValue stringValue &&
                                //StringFilters.ToJsonString(stringValue.ToStringValue()).Contains(templateId, StringComparison.InvariantCultureIgnoreCase)
                            stringValue.ToStringValue().Contains(templateId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result[NormalizeSectionName(templateId)] = sectionDict;
                        break;
                    }
                }
            }
            return FluidValue.Create(result, context.Options);
            //return new StringValue("test :)");
        }

        private static async Task<FluidValue> GetComponentsAsync(DictionaryValue data, TemplateContext context)
        {
            //var dataComponents = await data.GetValueAsync("ClinicalDocument.component.structuredBody.component", context);
            var splitInputObject = "msg.ClinicalDocument.component.structuredBody.component".Split('.');
            List<MemberSegment> inputMemberSegments = splitInputObject.Select(x => new IdentifierSegment(x)).ToList().Cast<MemberSegment>().ToList();
            var memberExpression = new MemberExpression(inputMemberSegments);
            var dataComponents = await memberExpression.EvaluateAsync(context);

            if (dataComponents == null)
            {
                return null;
            }

            return dataComponents;
        }

        private static List<object> GetComponents(IDictionary<string, object> data)
        {
            var dataComponents = (((data["ClinicalDocument"] as Dictionary<string, object>)?
                .GetValueOrDefault("component") as Dictionary<string, object>)?
                .GetValueOrDefault("structuredBody") as Dictionary<string, object>)?
                .GetValueOrDefault("component");

            if (dataComponents == null)
            {
                return null;
            }

            return dataComponents is List<object> listComponents
                ? listComponents
                : new List<object> { dataComponents };
        }

        private static string NormalizeSectionName(string input)
        {
            return NormalizeSectionNameRegex.Replace(input, "_");
        }
    }
}
