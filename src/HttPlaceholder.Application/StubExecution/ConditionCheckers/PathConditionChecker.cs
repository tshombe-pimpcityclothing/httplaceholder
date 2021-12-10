﻿using HttPlaceholder.Application.Interfaces.Http;
using HttPlaceholder.Common.Utilities;
using HttPlaceholder.Domain;
using HttPlaceholder.Domain.Enums;

namespace HttPlaceholder.Application.StubExecution.ConditionCheckers
{
    public class PathConditionChecker : IConditionChecker
    {
        private readonly IHttpContextService _httpContextService;

        public PathConditionChecker(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        public ConditionCheckResultModel Validate(StubModel stub)
        {
            var result = new ConditionCheckResultModel();
            var pathCondition = stub.Conditions?.Url?.Path;
            if (string.IsNullOrEmpty(pathCondition))
            {
                return result;
            }

            var path = _httpContextService.Path;
            if (StringHelper.IsRegexMatchOrSubstring(path, pathCondition))
            {
                // The path matches the provided regex. Add the stub ID to the resulting list.
                result.ConditionValidation = ConditionValidationType.Valid;
            }
            else
            {
                result.Log = $"Condition '{pathCondition}' did not pass for request.";
                result.ConditionValidation = ConditionValidationType.Invalid;
            }

            return result;
        }

        public int Priority => 8;
    }
}