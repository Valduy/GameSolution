﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Network;
using Network.Messages;

namespace Matches.States
{
    public class WaitClientState : ListenSessionStateBase
    {
        private readonly int _playersCount;

        public WaitClientState(ListenSessionMatch context)
            : base(context) 
            => _playersCount = context.ExpectedPlayers.Count();

        public override async Task ProcessMessageAsync(IPEndPoint ip, byte[] received)
        {
            if (MessageHelper.GetMessageType(received) == NetworkMessages.Hello)
            {
                if (IsClient(ip))
                {
                    await Context.SendMessageAsync(MessageHelper.GetMessage(NetworkMessages.Hello), ip);
                }
                else
                {
                    try
                    {
                        var clientEndPoints = CreateClientEndPoints(ip, received);

                        if (IsExpected(clientEndPoints))
                        {
                            Context.AddClient(clientEndPoints);
                            TryChangeState();
                            await Context.SendMessageAsync(MessageHelper.GetMessage(NetworkMessages.Hello), ip);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is JsonException || ex is ArgumentException)
                        {
                            return; // TODO: лог о неправильном JSON'е?
                        }
                    }
                }
            }
        }

        private ClientEndPoints CreateClientEndPoints(IPEndPoint ip, byte[] received)
        {
            var data = MessageHelper.ToString(received);
            var privateEndPoint = JsonSerializer.Deserialize<ClientEndPoint>(data) 
                                  ?? throw new ArgumentException("При десериализации конечной точки был получен null.");

            return new ClientEndPoints(
                ip.Address.ToString(),
                ip.Port,
                privateEndPoint.Ip,
                privateEndPoint.Port
            );
        }

        private bool IsExpected(ClientEndPoints endPoints)
            => Context.ExpectedPlayers.Any(o => o.IsSameSocket(endPoints));

        private void TryChangeState()
        {
            if (Context.Clients.Count >= _playersCount)
            {
                Context.State = new ChooseHostState(Context);
                Context.NotifyThatStarted();
            }
        }
    }
}
