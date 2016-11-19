using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NeUWP.Utilities
{
    public static class JsonUtil
    {
        /// <summary>
        /// Convert the json string to object
        /// </summary>
        public static T Deserialize<T>(string jsonStr)
        {
            return (T)Deserialize(jsonStr, typeof(T));
        }

        /// <summary>
        /// Convert the json string to object
        /// </summary>
        public static object Deserialize(string jsonStr, Type type)
        {
            object obj = null;

            if (string.IsNullOrWhiteSpace(jsonStr))
            {
                return obj;
            }

            JsonSerializerSettings JsonSetting = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Error = new EventHandler<ErrorEventArgs>(DeserializeHandler)
            };

            try
            {
                obj = JsonConvert.DeserializeObject(jsonStr, type, JsonSetting);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("JsonConvert.DeserializeObject: " + ex.Message);
            }

            return obj;
        }

        private static void DeserializeHandler(object sender, ErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("JsonConvert.DeserializeHandler: " + e.ErrorContext.Error);
            e.ErrorContext.Handled = true;
        }

        /// <summary>
        /// Convert the object to json string
        /// </summary>
        public static string Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            Type type = null;

            try
            {
                type = obj.GetType();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("JsonConvert.Serialize: " + ex.Message);

                return null;
            }

            if (type != null)
            {
                return Serialize(obj, type);
            }

            return null;
        }

        /// <summary>
        /// Convert the object to json string
        /// </summary>
        public static string Serialize(object obj, Type type)
        {
            string ret = "";

            if (obj == null)
            {
                return null;
            }

            JsonSerializerSettings JsonSetting = new JsonSerializerSettings()
            {
                //DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Error = new EventHandler<ErrorEventArgs>(SerializeHandler),
            };

            try
            {
                ret = JsonConvert.SerializeObject(obj, type, Formatting.None, JsonSetting);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return ret;
        }

        private static void SerializeHandler(object sender, ErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("JsonConvert.SerializeHandler: " + e.ErrorContext.Error);
            e.ErrorContext.Handled = true;
        }
    }
}
