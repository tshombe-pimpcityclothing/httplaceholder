﻿using System;
using System.Text.RegularExpressions;
using HttPlaceholder.Application.StubExecution.Utilities;
using HttPlaceholder.Common.Utilities;

namespace HttPlaceholder.Application.StubExecution.Implementations;

/// <inheritdoc />
internal class StringChecker : IStringChecker
{
    /// <inheritdoc />
    public bool CheckString(string input, object condition, out string outputForLogging)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (condition == null)
        {
            throw new ArgumentNullException(nameof(condition));
        }

        if (condition is string stringCondition)
        {
            var regexResult = StringHelper.IsRegexMatchOrSubstring(input, stringCondition);
            outputForLogging = !regexResult ? stringCondition : string.Empty;
            return regexResult;
        }

        var checkingModel = StringConditionUtilities.ConvertCondition(condition);
        var result = true;
        if (!string.IsNullOrWhiteSpace(checkingModel.StringEquals))
        {
            result &= string.Equals(input, checkingModel.StringEquals);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.StringEqualsCi))
        {
            result &= string.Equals(input, checkingModel.StringEqualsCi, StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.StringNotEquals))
        {
            result &= !string.Equals(input, checkingModel.StringNotEquals);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.StringNotEqualsCi))
        {
            result &= !string.Equals(input, checkingModel.StringNotEqualsCi, StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.Contains))
        {
            result &= input.Contains(checkingModel.Contains);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.ContainsCi))
        {
            result &= input.Contains(checkingModel.ContainsCi, StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.NotContains))
        {
            result &= !input.Contains(checkingModel.NotContains);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.NotContainsCi))
        {
            result &= !input.Contains(checkingModel.NotContainsCi, StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.StartsWith))
        {
            result &= input.StartsWith(checkingModel.StartsWith);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.StartsWithCi))
        {
            result &= input.StartsWith(checkingModel.StartsWithCi, StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.DoesNotStartWith))
        {
            result &= !input.StartsWith(checkingModel.DoesNotStartWith);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.DoesNotStartWithCi))
        {
            result &= !input.StartsWith(checkingModel.DoesNotStartWithCi, StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.EndsWith))
        {
            result &= input.EndsWith(checkingModel.EndsWith);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.EndsWithCi))
        {
            result &= input.EndsWith(checkingModel.EndsWithCi, StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.DoesNotEndWith))
        {
            result &= !input.EndsWith(checkingModel.DoesNotEndWith);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.DoesNotEndWithCi))
        {
            result &= !input.EndsWith(checkingModel.DoesNotEndWithCi, StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.Regex))
        {
            var regex = new Regex(checkingModel.Regex);
            result &= regex.IsMatch(input);
        }

        if (!string.IsNullOrWhiteSpace(checkingModel.RegexNoMatches))
        {
            var regex = new Regex(checkingModel.RegexNoMatches);
            result &= !regex.IsMatch(input);
        }

        outputForLogging = result ? string.Empty : checkingModel.ToString();
        return result;
    }
}