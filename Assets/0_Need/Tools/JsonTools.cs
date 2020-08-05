using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace BanSupport
{

    public static partial class Tools
    {
        private static JsonSerializerSettings _JsonTypeSettings = null;

        private static JsonSerializerSettings JsonTypeSettings
        {
            get
            {
                if (_JsonTypeSettings == null)
                {
                    _JsonTypeSettings = new JsonSerializerSettings();
                    _JsonTypeSettings.TypeNameHandling = TypeNameHandling.All;
                }
                return _JsonTypeSettings;
            }
        }

		/// <summary>
		/// Json序列化
		/// </summary>
		public static string JsonSerialize(System.Object aObject, bool includeType = false)
		{
			try
			{
				if (includeType)
				{
					return JsonConvert.SerializeObject(aObject, JsonTypeSettings);
				}
				else
				{
					return JsonConvert.SerializeObject(aObject);
				}
			}
			catch (Exception e)
			{
				Debug.Log("serialize error:" + e.ToString());
				return string.Empty;
			}
		}

		/// <summary>
		/// Json反序列化
		/// </summary>
		public static T JsonDeserialize<T>(string str, bool includeType = false)
		{
			try
			{
				if (includeType)
				{
					return JsonConvert.DeserializeObject<T>(str, JsonTypeSettings);
				}
				else
				{
					return JsonConvert.DeserializeObject<T>(str);
				}
			}
			catch (Exception e)
			{
				Debug.Log("Deserialize error:" + e.ToString());
				return default(T);
			}
		}
	}
}