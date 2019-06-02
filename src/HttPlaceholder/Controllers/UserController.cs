﻿using System.Threading.Tasks;
using HttPlaceholder.Application.Users.Queries.GetUserData;
using HttPlaceholder.Dto.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HttPlaceholder.Controllers
{
    /// <summary>
    /// User controller
    /// </summary>
    [Route("ph-api/users")]
    public class UserController : BaseApiController
    {
        /// <summary>
        /// Get the user for the given username.
        /// </summary>
        /// <returns>The User.</returns>
        [HttpGet]
        [Route("{Username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserDto>> Get([FromRoute]GetUserDataQuery query) =>
            Ok(Mapper.Map<UserDto>(await Mediator.Send(query)));
    }
}
