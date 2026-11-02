using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Core.Content
{
    /// <summary>Parses a JSON list file item by item, so an error names the item it is in.</summary>
    internal static class JsonItems
    {
        /// <summary>The file as a JSON array, or throws a ContentException naming the file.</summary>
        public static JArray ParseArray(string file, string json)
        {
            JToken root;
            try
            {
                root = string.IsNullOrWhiteSpace(json) ? null : JToken.Parse(json);
            }
            catch (JsonException e)
            {
                throw new ContentException(file, e.Message, e);
            }

            if (root == null) throw new ContentException(file, "file is empty");
            if (!(root is JArray array)) throw new ContentException(file, "must be a list");
            return array;
        }

        /// <summary>The file as a JSON object, or throws a ContentException naming the file.</summary>
        public static JObject ParseObject(string file, string json)
        {
            JToken root;
            try
            {
                root = string.IsNullOrWhiteSpace(json) ? null : JToken.Parse(json);
            }
            catch (JsonException e)
            {
                throw new ContentException(file, e.Message, e);
            }

            if (root == null) throw new ContentException(file, "file is empty");
            if (!(root is JObject obj)) throw new ContentException(file, "must be an object");
            return obj;
        }

        /// <summary>
        /// Each item converted to T. An item that does not convert (a missing required field, a wrong type) is reported
        /// under its id (or #index when it has none) and left out.
        /// </summary>
        public static List<(T Item, JObject Json, string Id)> Convert<T>(JArray array, ContentErrors errors) where T : class
        {
            var list = new List<(T, JObject, string)>();
            for (int i = 0; i < array.Count; i++)
            {
                var obj = array[i] as JObject;
                string id = IdOf(obj, i);
                if (obj == null)
                {
                    errors.Add(id, "must be an object");
                    continue;
                }

                try
                {
                    var item = obj.ToObject<T>();
                    if (item == null) errors.Add(id, "must be an object");
                    else list.Add((item, obj, id));
                }
                catch (JsonException e)
                {
                    errors.Add(id, e.Message);
                }
            }

            return list;
        }

        public static string IdOf(JObject obj, int index)
        {
            var token = obj?["id"];
            if (token == null || token.Type == JTokenType.Null) return "#" + index;
            return token.ToString();
        }
    }
}
