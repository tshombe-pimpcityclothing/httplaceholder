﻿using System.IO;
using YamlDotNet.Serialization;

namespace HttPlaceholder.Services.Implementations
{
   internal class YamlService : IYamlService
   {
      public TObject Parse<TObject>(string input)
      {
         var reader = new StringReader(input);
         var deserializer = new Deserializer();
         var yamlObject = deserializer.Deserialize<TObject>(reader);
         return yamlObject;
      }
   }
}