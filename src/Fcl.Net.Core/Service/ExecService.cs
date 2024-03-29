﻿using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Service
{
    public class ExecService
    {
        private readonly Dictionary<FclServiceMethod, IStrategy> _strategies;

        public ExecService(Dictionary<FclServiceMethod, IStrategy> strategies)
        {
            _strategies = strategies;
        }

        public async Task<T> ExecuteAsync<T>(FclService service, FclServiceConfig config = null, object msg = null)
            where T : class
        {
            var message = CreateMessage(service, msg);
            var response = await _strategies[service.Method].ExecuteAsync<T>(service, config, message).ConfigureAwait(false);

            if(response == null)
                return response;

            if(response is FclAuthResponse authResponse && authResponse.Status == ResponseStatus.Redirect)
                return await ExecuteAsync<T>(authResponse.Data, config, msg).ConfigureAwait(false);

            return response;
        }

        private static object CreateMessage(FclService service, object msg = null)
        {
            var message = new Dictionary<string, object>();            

            if (msg != null)
            {
                var dataDict = msg.ToDictionary<string, object>();

                foreach (var item in dataDict)
                    message.Add(item.Key, item.Value);
            }

            if (service.Data != null && service.Data.Any())
                message["data"] = service.Data;

            return message.Any() ? message : null;
        }
    }
}
