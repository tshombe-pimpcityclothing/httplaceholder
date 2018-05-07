﻿using System.Linq;
using Microsoft.Extensions.Logging;
using Placeholder.Implementation.Services;
using Placeholder.Models;
using Placeholder.Models.Enums;
using Placeholder.Utilities;

namespace Placeholder.Implementation.Implementations.ConditionCheckers
{
   public class QueryStringConditionChecker : IConditionChecker
   {
      private readonly ILogger<QueryStringConditionChecker> _logger;
      private readonly IHttpContextService _httpContextService;

      public QueryStringConditionChecker(
         ILogger<QueryStringConditionChecker> logger,
         IHttpContextService httpContextService)
      {
         _logger = logger;
         _httpContextService = httpContextService;
      }

      public ConditionValidationType Validate(StubModel stub)
      {
         var result = ConditionValidationType.NotExecuted;
         var queryStringConditions = stub.Conditions?.Url?.Query;
         if (queryStringConditions != null)
         {
            _logger.LogInformation($"Method condition found for stub '{stub.Id}': '{string.Join(", ", queryStringConditions.Select(c => $"{c.Key}: {c.Value}"))}'");
            int validQueryStrings = 0;
            var queryString = _httpContextService.GetQueryStringDictionary();
            foreach (var condition in queryStringConditions)
            {
               // Check whether the condition query is available in the actual query string.
               _logger.LogInformation($"Checking request query string against query string condition '{condition.Key}: {condition.Value}'");
               if (queryString.TryGetValue(condition.Key, out string queryValue))
               {
                  // Check whether the condition query value is available in the actual query string.
                  string value = condition.Value ?? string.Empty;
                  if (!StringHelper.IsRegexMatchOrSubstring(queryValue, value))
                  {
                     // If the check failed, it means the query string is incorrect and the condition should fail.
                     _logger.LogInformation($"Query string condition '{condition.Key}: {condition.Value}' failed.");
                     break;
                  }

                  validQueryStrings++;
               }
            }

            // If the number of succeeded conditions is equal to the actual number of conditions,
            // the query string condition is passed and the stub ID is passed to the result.
            if (validQueryStrings == queryStringConditions.Count)
            {
               _logger.LogInformation($"Query string condition check succeeded for stub '{stub.Id}'.");
               result = ConditionValidationType.Valid;
            }
            else
            {
               result = ConditionValidationType.Invalid;
            }
         }

         return result;
      }
   }
}