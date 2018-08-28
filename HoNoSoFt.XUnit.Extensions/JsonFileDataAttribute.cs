﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Xunit.Sdk;

namespace HoNoSoFt.XUnit.Extensions
{
    /// <summary>
    /// Usage of the JsonFileData attribute gives the opportunity to load json data
    /// in your tests.
    /// </summary>
    public class JsonFileDataAttribute : DataAttribute
    {
        private readonly string _filePath;
        private readonly Type _type = null;
        private readonly object[] _data;

        /// <inheritdoc />
        public JsonFileDataAttribute(string filePath, params object[] data)
        {
            _filePath = filePath; // Could also look if this is inline json, but it make no sense.
            _data = data;
        }

        /// <inheritdoc />
        public JsonFileDataAttribute(string filePath, Type type, params object[] data)
            : this(filePath, data)
        {
            _type = type;
        }

        /// <inheritdoc />
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null) { throw new ArgumentNullException(nameof(testMethod)); }

            // Get the absolute path to the JSON file
            var path = Path.IsPathRooted(_filePath)
                ? _filePath
                : Directory.GetCurrentDirectory() + "/" + _filePath;
            // Original code (Core 2.1, can't work in Standard2.0) :(
            //: Path.GetRelativePath(Directory.GetCurrentDirectory(), _filePath);

            var type = testMethod.GetParameters()[0].ParameterType;

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find file at path: {path}");
            }

            // Load the file
            var fileData = File.ReadAllText(path);
            var result = new List<object>(_data);
            if (_type != null)
            {
                result.Insert(0, new JsonData(fileData, _type));
            }
            else
            {
                if (type == null)
                {
                    // This will return a JObject.
                    result.Insert(0, JsonConvert.DeserializeObject<object>(fileData));
                }
                else
                {
                    result.Insert(0, JsonConvert.DeserializeObject(fileData, type));
                }
            }

            // maybe think of https://stackoverflow.com/questions/17519078/initializing-a-generic-variable-from-a-c-sharp-type-variable...
            // however it's not working well yet.
            return new[] { result.ToArray() };
        }
    }
}
