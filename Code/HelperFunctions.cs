using HGEngineHelper.Code.CsvProcessing.Models;
using HGEngineHelper.Code.HGECodeHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Converters;
using static HGEngineHelper.Code.HGEngineExport.HgEngineCodeWriter;

namespace HGEngineHelper
{
    public static class HelperFunctions
    {
        public static int RestrainToMinAndMax(int value, int? min, int? max)
        {
            if (min.HasValue && value < min.Value)
            {
                value = min.Value;
            }
            if (max.HasValue && value > max.Value)
            {
                value = max.Value;
            }
            return value;
        }

        public static string Wrap(this string value, string wrapWith)
        {
            return wrapWith + value + wrapWith;
        }

        public static T GetAtIndexOrDefault<T>(this List<T> list, int index, T defaultValue)
        {
            if (index >= list.Count || index < 0)
            {
                return defaultValue;
            }
            return list[index];
        }

        public static void AddValueToCorrespondingList<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, TValue value)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = new List<TValue>();
            }
            dict[key].Add(value);
        }

        public static Dictionary<TKey,TValue> ConvertListToDictionary<TKey,TValue>(this List<TValue> list, Func<TValue, TKey> getKeyFunc)
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();

            foreach(var val in list)
            {
                result[getKeyFunc(val)] = val;
            }

            return result;
        }

        public static Dictionary<TKey, TValueOut> ConvertListToDictionaryOfValues<TKey, TValueIn, TValueOut>(this List<TValueIn> list, Func<TValueIn, TKey> getKeyFunc, Func<TValueIn, TValueOut> getValueFunc)
        {
            Dictionary<TKey, TValueOut> result = new Dictionary<TKey, TValueOut>();

            foreach (var val in list)
            {
                result[getKeyFunc(val)] = getValueFunc(val) ;
            }

            return result;
        }

        public static Dictionary<TKey, List<TValue>> ConvertListToDictionaryOfLists<TKey,TValue>(List<TValue> list, Func<TValue, TKey> getKeyFunc)
        {
            Dictionary<TKey, List<TValue>> result = new Dictionary<TKey, List<TValue>>();
            foreach(var val in list)
            {
                result.AddValueToCorrespondingList(getKeyFunc(val), val);
            }
            return result;
        }

        public static string FormatStr(this string str, params object[] args)
        {
            return String.Format(str, args);
        }
    }
}
