﻿using Ducode.Essentials.Mvc.Interfaces;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace HttPlaceholder.Application.StubExecution.VariableHandling.Implementations
{
    public class EncodedQueryStringVariableHandler : IVariableHandler
    {
        private readonly IHttpContextService _httpContextService;

        public EncodedQueryStringVariableHandler(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        public string Name => "query_encoded";

        public string Parse(string input, IEnumerable<Match> matches)
        {
            var queryDict = _httpContextService.GetQueryStringDictionary();
            foreach (var match in matches)
            {
                if (match.Groups.Count == 3)
                {
                    string queryStringName = match.Groups[2].Value;
                    string replaceValue = string.Empty;
                    queryDict.TryGetValue(queryStringName, out replaceValue);

                    replaceValue = WebUtility.UrlEncode(replaceValue);
                    input = input.Replace(match.Value, replaceValue);
                }
            }

            return input;
        }
    }
}
