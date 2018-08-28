﻿using HoNoSoFt.XUnit.Extensions.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Xunit.Sdk;

namespace HoNoSoFt.XUnit.Extensions
{
    /// <summary>
    /// Usage of the XmlFileData attribute gives the opportunity to load xml data
    /// in your tests without polluting your code.
    /// </summary>
    public class XmlFileDataAttribute : DataAttribute
    {
        private readonly string _filePath;
        private readonly Type _type = null;
        private readonly object[] _data;

        /// <inheritdoc />
        public XmlFileDataAttribute(string filePath, params object[] data)
        {
            _filePath = filePath; // Could also look if this is inline json, but it make no sense.
            _data = data;
        }

        /// <inheritdoc />
        public XmlFileDataAttribute(string filePath, Type type, params object[] data)
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
            var fileContent = File.ReadAllText(path);

            var result = new List<object>(_data);
            if (_type != null)
            {
                result.Insert(0, new XmlData(fileContent, _type));
            }
            else
            {
                result.Insert(0, XmlUtility.DeserializeXml(fileContent, type ?? typeof(object)));
            }

            return new[] { result.ToArray() };
        }
    }
}