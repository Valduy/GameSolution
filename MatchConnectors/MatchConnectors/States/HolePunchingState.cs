using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Matches.Messages;
using Network;
using Network.Messages;

namespace Connectors.MatchConnectors.States
{
    public class HolePunchingState : P2PMatchConnectorStateBase
    {
        private readonly byte[] _connectMessage;
        private HashSet<ClientEndPoints> _clients;

        public HolePunchingState(P2PMatchConnector context) : base(context)
        {
            _connectMessage = MessageHelper.GetMessage(NetworkMessages.Connect);
            _clients = new HashSet<ClientEndPoints>(Context.PotentialEndPoints);
            Context.RealEndPoints = new List<ClientEndPoint>();
        }

        public override void ProcessMessage(IPEndPoint ip, byte[] message)
        {
            // TODO: долго не получаю сообщения
            // TODO: долго не получается подключиться

            if (TryGetGetClientByPublicEndPoint(ip, out var client))
            {
                _clients.Remove(client);
                Context.RealEndPoints.Add(client.PublicEndPoint);
            }

            if (TryGetGetClientByPrivateEndPoint(ip, out client))
            {
                _clients.Remove(client);
                Context.RealEndPoints.Add(client.PrivateEndPoint);
            }

            if (!_clients.Any())
            {
                Context.CompleteConnection();
            }
        }

        public override async Task SendMessageAsync()
        {
            foreach (var c in Context.PotentialEndPoints)
            {
                await Context.SendMessageAsync(_connectMessage, c.PublicEndPoint);
                await Context.SendMessageAsync(_connectMessage, c.PrivateEndPoint);
            }

            foreach (var c in Context.RealEndPoints)
            {
                await Context.SendMessageAsync(_connectMessage, c);
            }
        }

        private bool TryGetGetClientByPublicEndPoint(IPEndPoint ip, out ClientEndPoints client)
        {
            client = _clients.FirstOrDefault(o => o.IsClientPublicEndPoint(ip));
            return client != null;
        }

        private bool TryGetGetClientByPrivateEndPoint(IPEndPoint ip, out ClientEndPoints client)
        {
            client = _clients.FirstOrDefault(o => o.IsClientPrivateEndPoint(ip));
            return client != null;
        }
    }
}
