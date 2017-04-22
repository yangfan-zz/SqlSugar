﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace SqlSugar
{
    public class RewritableMethods : IRewritableMethods
    {
        
        /// <summary>
        ///DataReader to Dynamic
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public ExpandoObject DataReaderToExpandoObject(IDataReader reader)
        {
            ExpandoObject result = new ExpandoObject();
            var dic = ((IDictionary<string, object>)result);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                try
                {
                    dic.Add(reader.GetName(i), reader.GetValue(i));
                }
                catch
                {
                    dic.Add(reader.GetName(i), null);
                }
            }
            return result;
        }
        /// <summary>
        ///DataReader to DataReaderToDictionary
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public Dictionary<string,object> DataReaderToDictionary(IDataReader reader)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                try
                {
                    result.Add(reader.GetName(i), reader.GetValue(i));
                }
                catch
                {
                    result.Add(reader.GetName(i), null);
                }
            }
            return result;
        }

        public List<T> DataReaderToDynamicList<T>(IDataReader reader)
        {
            var tType = typeof(T);
            var classProperties = tType.GetProperties().ToList();
            var reval = new List<T>();
            if (reader != null && !reader.IsClosed)
            {
                while (reader.Read())
                {
                    var readerValues = DataReaderToDictionary(reader);
                    var result = new Dictionary<string, object>();
                    foreach (var item in classProperties)
                    {
                        var name = item.Name;
                        var typeName = tType.Name;
                        if (item.PropertyType.IsClass())
                        {
                            result.Add(name,DataReaderToDynamicList_Part(readerValues, item, reval));
                        }
                        else
                        {
                            if (readerValues.ContainsKey(name))
                            {
                                result.Add(name, readerValues[name]);
                            }
                        }
                    }
                    var stringValue = SerializeObject(result);
                    reval.Add((T)DeserializeObject<T>(stringValue));
                }
                reader.Close();
            }
            return reval;
        }
        private Dictionary<string,object> DataReaderToDynamicList_Part<T>(Dictionary<string, object> readerValues,PropertyInfo item, List<T> reval)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            var type = item.PropertyType;
            var classProperties = type.GetProperties().ToList();
            foreach (var prop in classProperties)
            {
                var name = prop.Name;
                var typeName = type.Name;
                if (prop.PropertyType.IsClass())
                {
                    result.Add(name, DataReaderToDynamicList_Part(readerValues, prop, reval));
                }
                else
                {
                    var key = typeName + "." + name;
                    if (readerValues.ContainsKey(key))
                    {
                        result.Add(name, readerValues[key]);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Serialize Object
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// Serialize Object
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
