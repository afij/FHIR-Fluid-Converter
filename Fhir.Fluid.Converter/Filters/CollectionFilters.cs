﻿using Fhir.Fluid.Converter.Parsers;
using Fluid;
using Fluid.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Fluid.Converter.Filters
{
    internal static class CollectionFilters
    {
        private static CCDParser _parser;

        public static void RegisterCollectionFilters(this FilterCollection filters, CCDParser parser)
        {
            _parser = parser;
            filters.AddFilter("to_array", ToArray);
            filters.AddFilter("batch_render", BatchRender);
        }

        /// <summary>
        /// Return an array like object created from a given object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValueTask<FluidValue> ToArray(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input == null || input is NilValue)
            {
                return FluidValue.Create(new List<object>(), context.Options);
            }
            else if (input is ArrayValue)
            {
                return FluidValue.Create(input.Enumerate(context), context.Options);
                //return FluidValue.Create(new List<object>() { input.Enumerate(context) }, context.Options);
            }
            else if (input is DictionaryValue)
            {
                return FluidValue.Create(new List<object>() { input }, context.Options);
            }
            else
            {
                //return FluidValue.Create(input.Enumerate(context), context.Options);
                return FluidValue.Create(input.Enumerate(context).ToList(), context.Options);
            }
        }

        /// <summary>
        /// Render every entry in a collection with a snippet and a variable name set in snippet
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async ValueTask<FluidValue> BatchRender(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            context.EnterChildScope();
            try
            {
                var inputArray = input as ArrayValue;

                var templateFileSystem = context.Options.FileProvider;
                var templateInfo = templateFileSystem.GetFileInfo($"{arguments.At(0).ToStringValue()}.liquid");
                string templateContent = null;
                if (templateInfo.Exists)
                {
                    using StreamReader reader = new(templateInfo.CreateReadStream());
                    templateContent = reader.ReadToEnd();
                }
                if (string.IsNullOrEmpty(templateContent))
                {
                    throw new Exception();
                }

                if (_parser.TryParse(templateContent, out var template, out var errors))
                {
                    StringBuilder batchOutputBuilder = new();
                    if (input is ArrayValue)
                    {
                        foreach (var value in input.Enumerate(context))
                        {
                            //var output = await template.RenderAsync(new TemplateContext(new Dictionary<string, object> { { arguments.At(1).ToStringValue(), value } }, context.Options));
                            context.SetValue(arguments.At(1).ToStringValue(), value);
                            var output = await template.RenderAsync(context);
                            if (!string.IsNullOrEmpty(output))
                            {
                                batchOutputBuilder.Append(output);
                            }
                        }
                    }
                    else
                    {
                        //var output = await template.RenderAsync(new TemplateContext(new Dictionary<string, object> { { arguments.At(1).ToStringValue(), input } }, context.Options));
                        context.SetValue(arguments.At(1).ToStringValue(), input);
                        var output = await template.RenderAsync(context);
                        if (!string.IsNullOrEmpty(output))
                        {
                            batchOutputBuilder.Append(output);
                        }
                    }
                    return new StringValue(batchOutputBuilder.ToString());
                }
                return NilValue.Empty;
            }
            finally
            {
                context.ReleaseScope();
            }
        }
    }
}
