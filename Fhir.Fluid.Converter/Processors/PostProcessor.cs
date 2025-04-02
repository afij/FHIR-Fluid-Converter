using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Fhir.Fluid.Converter.Processors
{
    internal static class PostProcessor
    {
        /// <summary>
        /// Performs post processing on converted string 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Process(string input)
        {
            var jObj = JsonConvert.DeserializeObject<JObject>(input, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, NullValueHandling = NullValueHandling.Ignore });
            RemoveEmptyStringsAndObjects(jObj);
            var mergedObj = MergeJson(jObj);
            RemoveEmptyStringsAndObjects(mergedObj);
            var mergedJsonString = mergedObj.ToString(Formatting.Indented);
            return mergedJsonString;
        }

        /// <summary>
        /// Removes empty string and object properties from JObject
        /// </summary>
        /// <param name="jObject"></param>
        public static void RemoveEmptyStringsAndObjects(JObject jObject)
        {
            var propertiesToRemove = new List<JProperty>();

            foreach (var property in jObject.Properties())
            {
                if (property.Value.Type == JTokenType.String && string.IsNullOrEmpty(property.Value.ToString()))
                {
                    propertiesToRemove.Add(property);
                }
                else if (property.Value.Type == JTokenType.Object)
                {
                    RemoveEmptyStringsAndObjects((JObject)property.Value);
                    if (!((JObject)property.Value).HasValues)
                    {
                        propertiesToRemove.Add(property);
                    }
                }
                else if (property.Value.Type == JTokenType.Array)
                {
                    RemoveEmptyStringsAndObjects((JArray)property.Value);
                    if (!((JArray)property.Value).HasValues)
                    {
                        propertiesToRemove.Add(property);
                    }
                }
            }

            foreach (var property in propertiesToRemove)
            {
                property.Remove();
            }
        }

        /// <summary>
        /// Removes empty string and object properties from JArray
        /// </summary>
        /// <param name="jObject"></param>
        private static void RemoveEmptyStringsAndObjects(JArray jArray)
        {
            for (int i = jArray.Count - 1; i >= 0; i--)
            {
                var item = jArray[i];

                if (item.Type == JTokenType.String && string.IsNullOrEmpty(item.ToString()))
                {
                    jArray.RemoveAt(i);
                }
                else if (item.Type == JTokenType.Object)
                {
                    RemoveEmptyStringsAndObjects((JObject)item);
                    if (!((JObject)item).HasValues)
                    {
                        jArray.RemoveAt(i);
                    }
                }
                else if (item.Type == JTokenType.Array)
                {
                    RemoveEmptyStringsAndObjects((JArray)item);
                    if (!((JArray)item).HasValues)
                    {
                        jArray.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Merges JObjects together based on their resource key
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static JObject MergeJson(JObject obj)
        {
            try
            {
                var mergedEntity = new JArray();
                var resourceKeyToIndexMap = new Dictionary<string, int>();

                if (obj.TryGetValue("entry", out var entries))
                {
                    foreach (var entry in entries)
                    {
                        var resourceKey = GetKey(entry);
                        if (resourceKeyToIndexMap.TryGetValue(resourceKey, out int index))
                        {
                            mergedEntity[index] = Merge((JObject)mergedEntity[index], (JObject)entry);
                        }
                        else
                        {
                            mergedEntity.Add(entry);
                            resourceKeyToIndexMap[resourceKey] = mergedEntity.Count - 1;
                        }
                    }

                    obj["entry"] = mergedEntity;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return obj;
        }

        /// <summary>
        /// Merge to JObjects together
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        private static JObject Merge(JObject obj1, JObject obj2)
        {
            var setting = new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union,
            };

            obj1.Merge(obj2, setting);

            return obj1;
        }

        /// <summary>
        /// Determines a JToken's 'key' based on their resource type/versionId/Id
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string GetKey(JToken obj)
        {
            var resourceType = obj.SelectToken("$.resource.resourceType")?.Value<string>();
            if (resourceType != null)
            {
                var key = resourceType;
                var versionId = obj.SelectToken("$.resource.meta.versionId")?.Value<string>();
                key += versionId != null ? $"_{versionId}" : string.Empty;

                var resourceId = obj.SelectToken("$.resource.id")?.Value<string>();
                key += resourceId != null ? $"_{resourceId}" : string.Empty;

                return key;
            }

            return JsonConvert.SerializeObject(obj);
        }
    }
}
