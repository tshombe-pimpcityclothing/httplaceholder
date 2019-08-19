﻿using System.Collections.Generic;
using System.Threading.Tasks;
using HttPlaceholder.Application.Stubs.Commands.AddStub;
using HttPlaceholder.Application.Stubs.Commands.DeleteStub;
using HttPlaceholder.Application.Stubs.Commands.UpdateStubCommand;
using HttPlaceholder.Application.Stubs.Queries.GetAllStubs;
using HttPlaceholder.Application.Stubs.Queries.GetStub;
using HttPlaceholder.Domain;
using HttPlaceholder.Dto.Stubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HttPlaceholder.Controllers
{
    /// <summary>
    /// Stub Controller
    /// </summary>
    [Route("ph-api/stubs")]
    public class StubController : BaseApiController
    {
        /// <summary>
        /// Adds a new stub.
        /// </summary>
        /// <param name="stub"></param>
        /// <returns>OK, but no content returned</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Add([FromBody] StubDto stub)
        {
            await Mediator.Send(new AddStubCommand {Stub = Mapper.Map<StubModel>(stub)});
            return NoContent();
        }

        /// <summary>
        /// Updates a given stub.
        /// </summary>
        /// <param name="stub">The posted stub.</param>
        /// <param name="StubId">The stub ID.</param>
        /// <returns>OK, but no content returned</returns>
        [HttpPut("{StubId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Update([FromBody] StubDto stub, string StubId)
        {
            var command = new UpdateStubCommand {StubId = StubId, Stub = Mapper.Map<StubModel>(stub)};
            await Mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Get all stubs.
        /// </summary>
        /// <returns>All stubs.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FullStubDto>>> GetAll() =>
            Ok(Mapper.Map<IEnumerable<FullStubDto>>(await Mediator.Send(new GetAllStubsQuery())));

        /// <summary>
        /// Get a specific stub by stub identifier.
        /// </summary>
        /// <returns>The stub.</returns>
        [HttpGet]
        [Route("{StubId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<FullStubDto>> Get([FromRoute] GetStubQuery query) =>
            Ok(Mapper.Map<FullStubDto>(await Mediator.Send(query)));

        /// <summary>
        /// Delete a specific stub by stub identifier.
        /// </summary>
        /// <returns>OK, but not content</returns>
        [HttpDelete]
        [Route("{StubId}")]
        public async Task<ActionResult> Delete([FromRoute] DeleteStubCommand command)
        {
            await Mediator.Send(command);
            return NoContent();
        }
    }
}
