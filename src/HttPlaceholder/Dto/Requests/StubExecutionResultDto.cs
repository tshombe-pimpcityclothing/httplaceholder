﻿using System.Collections.Generic;
using HttPlaceholder.Application.Interfaces.Mappings;
using HttPlaceholder.Domain;

namespace HttPlaceholder.Dto.Requests
{
    /// <summary>
    /// A model for storing all execution related data for a given stub.
    /// </summary>
    public class StubExecutionResultDto : IMapFrom<StubExecutionResultModel>
    {
        /// <summary>
        /// Gets or sets the stub identifier.
        /// </summary>
        public string StubId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="StubExecutionResultDto"/> is passed.
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// Gets or sets the conditions.
        /// </summary>
        public IEnumerable<ConditionCheckResultDto> Conditions { get; set; }

        /// <summary>
        /// Gets or sets the negative conditions.
        /// </summary>
        public IEnumerable<ConditionCheckResultDto> NegativeConditions { get; set; }
    }
}