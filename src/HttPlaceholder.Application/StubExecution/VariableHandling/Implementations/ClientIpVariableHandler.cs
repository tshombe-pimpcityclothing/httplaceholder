﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HttPlaceholder.Application.Interfaces.Http;

namespace HttPlaceholder.Application.StubExecution.VariableHandling.Implementations
{
    public class ClientIpVariableHandler : IVariableHandler
    {
        private readonly IClientIpResolver _clientIpResolver;

        public ClientIpVariableHandler(IClientIpResolver clientIpResolver)
        {
            _clientIpResolver = clientIpResolver;
        }

        public string Name => "client_ip";

        public string Parse(string input, IEnumerable<Match> matches)
        {
            if (matches.Any())
            {
                var ip = _clientIpResolver.GetClientIp();
                foreach (var match in matches)
                {
                    if (match.Groups.Count >= 2)
                    {
                        input = input.Replace(match.Value, ip);
                    }
                }
            }

            return input;
        }
    }
}
