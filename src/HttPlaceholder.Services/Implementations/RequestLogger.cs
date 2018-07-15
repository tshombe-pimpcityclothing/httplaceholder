﻿using System;
using System.Collections.Generic;
using System.Linq;
using HttPlaceholder.Models;

namespace HttPlaceholder.Services.Implementations
{
   internal class RequestLogger : IRequestLogger
   {
      private readonly RequestResultModel _result;

      public RequestLogger()
      {
         _result = new RequestResultModel
         {
            RequestBeginTime = DateTime.Now
         };
      }

      public RequestResultModel GetResult()
      {
         _result.RequestEndTime = DateTime.Now;
         return _result;
      }

      public void LogRequestParameters(string method, string url, string body, IDictionary<string, string> headers)
      {
         string headerString = string.Join(", ", headers.Select(h => $"{h.Key} = {h.Value}"));
         _result.RequestParameters = new
         {
            method,
            url,
            body,
            headers = headerString
         };
      }

      public void SetCorrelationId(string correlationId)
      {
         _result.CorrelationId = correlationId;
      }

      public void SetStubExecutionResult(string stubId, bool passed, IEnumerable<ConditionCheckResultModel> conditions, IEnumerable<ConditionCheckResultModel> negativeConditions)
      {
         _result.StubExecutionResults.Add(new StubExecutionResultModel
         {
            Passed = passed,
            StubId = stubId,
            Conditions = conditions,
            NegativeConditions = negativeConditions
         });
      }

      public void SetExecutingStubId(string stubId)
      {
         _result.ExecutingStubId = stubId;
      }

      public void SetResponseWriterResult(string writerName, bool executed)
      {
         _result.StubResponseWriterResults.Add(new StubResponseWriterResultModel
         {
            Executed = executed,
            ResponseWriterName = writerName
         });
      }
   }
}