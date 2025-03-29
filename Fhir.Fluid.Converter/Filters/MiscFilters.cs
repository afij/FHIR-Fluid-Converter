using Fluid;
using Fluid.Values;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Fhir.Fluid.Converter.Filters
{
    public static class MiscFilters
    {
        public static void RegisterMiscFilters(this FilterCollection filters)
        {
            filters.AddFilter("json", Json);
        }

        // Overrides Fluid's own json filter
        //        public static async ValueTask<FluidValue> Json(FluidValue input, FilterArguments arguments, TemplateContext context)
        //        {
        //            using var ms = new MemoryStream();
        //            var encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        //            await using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions
        //            {
        //                Indented = arguments.At(0).ToBooleanValue(),
        //                // Unsafe Relaxed encoding so that symbols like + are output
        //                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        //            }))
        //            {
        //                await WriteJson(writer, input, context);
        //            }

        //            ms.Seek(0, SeekOrigin.Begin);
        //            using var sr = new StreamReader(ms, Encoding.UTF8);
        //            var json = await sr.ReadToEndAsync();
        //            return new StringValue(json);
        //        }

        //        private static async ValueTask WriteJson(Utf8JsonWriter writer, FluidValue input, TemplateContext ctx, HashSet<object> stack = null)
        //        {
        //            switch (input.Type)
        //            {
        //                case FluidValues.Array:
        //                    writer.WriteStartArray();
        //                    foreach (var item in input.Enumerate(ctx))
        //                    {
        //                        await WriteJson(writer, item, ctx);
        //                    }
        //                    writer.WriteEndArray();
        //                    break;
        //                case FluidValues.Boolean:
        //                    writer.WriteBooleanValue(input.ToBooleanValue());
        //                    break;
        //                case FluidValues.Nil:
        //                    writer.WriteNullValue();
        //                    break;
        //                case FluidValues.Number:
        //                    writer.WriteNumberValue(input.ToNumberValue());
        //                    break;
        //                case FluidValues.Dictionary:
        //                    if (input.ToObjectValue() is IFluidIndexable dic)
        //                    {
        //                        writer.WriteStartObject();
        //                        foreach (var key in dic.Keys)
        //                        {
        //                            writer.WritePropertyName(key);
        //                            if (dic.TryGetValue(key, out var value))
        //                            {
        //                                await WriteJson(writer, value, ctx);
        //                            }
        //                            else
        //                            {
        //                                await WriteJson(writer, NilValue.Instance, ctx);
        //                            }
        //                        }

        //                        writer.WriteEndObject();
        //                    }
        //                    else
        //                    {
        //                        writer.WriteNullValue();
        //                    }
        //                    break;
        //                case FluidValues.Object:
        //                    var obj = input.ToObjectValue();
        //                    if (obj != null)
        //                    {
        //                        writer.WriteStartObject();
        //                        var type = obj.GetType();
        //                        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        //                        var strategy = ctx.Options.MemberAccessStrategy;

        //                        var conv = strategy.MemberNameStrategy;
        //                        foreach (var property in properties)
        //                        {
        //                            var name = conv(property);
        //#pragma warning disable CA1859 // Change type of variable 'fluidValue' from 'Fluid.Values.FluidValue' to 'Fluid.Values.StringValue' for improved performance
        //                            var fluidValue = await input.GetValueAsync(name, ctx);
        //#pragma warning restore CA1859
        //                            if (fluidValue.IsNil())
        //                            {
        //                                continue;
        //                            }

        //                            stack ??= new HashSet<object>();
        //                            if (fluidValue is ObjectValue)
        //                            {
        //                                var value = fluidValue.ToObjectValue();
        //                                if (stack.Contains(value))
        //                                {
        //                                    fluidValue = StringValue.Create("Circular reference has been detected.");
        //                                }
        //                            }

        //                            writer.WritePropertyName(name);
        //                            stack.Add(obj);
        //                            await WriteJson(writer, fluidValue, ctx, stack);
        //                            stack.Remove(obj);
        //                        }

        //                        writer.WriteEndObject();
        //                    }
        //                    else
        //                    {
        //                        writer.WriteNullValue();
        //                    }
        //                    break;
        //                case FluidValues.DateTime:
        //                    var objValue = input.ToObjectValue();
        //                    if (objValue is DateTime dateTime)
        //                    {
        //                        writer.WriteStringValue(dateTime);
        //                    }
        //                    else if (objValue is DateTimeOffset dateTimeOffset)
        //                    {
        //                        writer.WriteStringValue(dateTimeOffset);
        //                    }
        //                    else
        //                    {
        //                        writer.WriteStringValue(Convert.ToDateTime(objValue));
        //                    }
        //                    break;
        //                case FluidValues.String:
        //                    writer.WriteStringValue(input.ToStringValue());
        //                    break;
        //                case FluidValues.Blank:
        //                    writer.WriteStringValue(string.Empty);
        //                    break;
        //                case FluidValues.Empty:
        //                    writer.WriteStringValue(string.Empty);
        //                    break;
        //                default:
        //                    throw new NotSupportedException("Unrecognized FluidValue");
        //            }
        //        }

        /// <summary>
        /// Converts input to a JSON string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="arguments"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async ValueTask<FluidValue> Json(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var indented = arguments.At(0).ToBooleanValue();
            var settings = new JsonSerializerSettings
            {
                Formatting = indented ? Formatting.Indented : Formatting.None,
                StringEscapeHandling = StringEscapeHandling.Default
            };

            var json = JsonConvert.SerializeObject(await ConvertFluidValueToJsonAsync(input, context), settings);
            return new StringValue(json);
        }

        private static async Task<object> ConvertFluidValueToJsonAsync(FluidValue input, TemplateContext ctx, HashSet<object> stack = null)
        {
            switch (input.Type)
            {
                case FluidValues.Array:
                    var array = new List<object>();
                    foreach (var item in input.Enumerate(ctx))
                    {
                        array.Add(await ConvertFluidValueToJsonAsync(item, ctx));
                    }
                    return array;
                case FluidValues.Boolean:
                    return input.ToBooleanValue();
                case FluidValues.Nil:
                    return null;
                case FluidValues.Number:
                    return input.ToNumberValue();
                case FluidValues.Dictionary:
                    if (input.ToObjectValue() is IFluidIndexable dic)
                    {
                        var dictionary = new Dictionary<string, object>();
                        foreach (var key in dic.Keys)
                        {
                            if (dic.TryGetValue(key, out var value))
                            {
                                dictionary[key] = await ConvertFluidValueToJsonAsync(value, ctx);
                            }
                            else
                            {
                                dictionary[key] = null;
                            }
                        }
                        return dictionary;
                    }
                    return null;
                case FluidValues.Object:
                    var obj = input.ToObjectValue();
                    if (obj != null)
                    {
                        var dictionary = new Dictionary<string, object>();
                        var type = obj.GetType();
                        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                        var strategy = ctx.Options.MemberAccessStrategy;

                        var conv = strategy.MemberNameStrategy;
                        foreach (var property in properties)
                        {
                            var name = conv(property);
                            var fluidValue = await input.GetValueAsync(name, ctx);
                            if (fluidValue.IsNil())
                            {
                                continue;
                            }

                            stack ??= new HashSet<object>();
                            if (fluidValue is ObjectValue)
                            {
                                var value = fluidValue.ToObjectValue();
                                if (stack.Contains(value))
                                {
                                    fluidValue = StringValue.Create("Circular reference has been detected.");
                                }
                            }

                            dictionary[name] = await ConvertFluidValueToJsonAsync(fluidValue, ctx, stack);
                        }
                        return dictionary;
                    }
                    return null;
                case FluidValues.DateTime:
                    var objValue = input.ToObjectValue();
                    if (objValue is DateTime dateTime)
                    {
                        return dateTime;
                    }
                    else if (objValue is DateTimeOffset dateTimeOffset)
                    {
                        return dateTimeOffset;
                    }
                    return Convert.ToDateTime(objValue);
                case FluidValues.String:
                    return input.ToStringValue();
                case FluidValues.Blank:
                case FluidValues.Empty:
                    return string.Empty;
                default:
                    throw new NotSupportedException("Unrecognized FluidValue");
            }
        }
    }
}
