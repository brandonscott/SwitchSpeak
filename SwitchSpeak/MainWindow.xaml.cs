using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Sharparam.SharpBlade.Native;
using Sharparam.SharpBlade.Razer;
using Sharparam.SharpBlade.Razer.Events;
using TS3QueryLib.Core;
using TS3QueryLib.Core.Client;
using TS3QueryLib.Core.Client.Entities;
using TS3QueryLib.Core.Client.Notification.Enums;
using TS3QueryLib.Core.Client.Notification.EventArgs;
using TS3QueryLib.Core.Client.Responses;
using TS3QueryLib.Core.Common;
using TS3QueryLib.Core.Common.Responses;
using TS3QueryLib.Core.Communication;
using TS3QueryLib.Core.Server.Entities;
using TS3QueryLib.Core.Server.Notification.EventArgs;
using ChannelListEntry = TS3QueryLib.Core.Client.Entities.ChannelListEntry;

namespace SwitchSpeak
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private QueryRunner _asyncClientQueryRunner;
        private AsyncTcpDispatcher _asyncQueryDispatcher;
        private TS3QueryLib.Core.Server.QueryRunner _asyncServerQueryRunner;
        private ObservableCollection<ChannelListEntry> _channels;
        private SyncTcpDispatcher _syncQueryDispatcher;
        private QueryRunner _syncQueryRunner;
        private WhoAmIResponse _me;

        private DragScrollViewerAdaptor _scrollViewerAdaptor;
        private ScrollViewer _scroller;

        public MainWindow()
        {
            InitializeComponent();
            var td = new TextWriterTraceListener("debug.log");
            Debug.Listeners.Add(td);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            RazerProvider.Razer.Touchpad.Gesture += TouchpadOnGesture;
            RazerProvider.Razer.Touchpad.EnableGesture(RazerAPI.GestureType.Tap);
            RazerProvider.Razer.EnableDynamicKey(RazerAPI.DynamicKeyType.DK10, (s, x) => Connect(),
                @"Default\refresh.png", @"Default\refresh.png", true);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.ExceptionObject);
        }

        private void TouchpadOnGesture(object sender, GestureEventArgs args)
        {
            var pos = new Point(args.X, args.Y);

            switch (args.GestureType)
            {
                case RazerAPI.GestureType.Move:
                    _scrollViewerAdaptor.MouseMove(pos);
                    break;
                case RazerAPI.GestureType.Press:
                    _scrollViewerAdaptor.MouseLeftButtonDown(pos);
                    break;
                case RazerAPI.GestureType.Release:
                    _scrollViewerAdaptor.MouseLeftButtonUp();
                    break;
                case RazerAPI.GestureType.Tap:
                    HitTestResult result = VisualTreeHelper.HitTest(treeSpeak, pos);
                    if (result != null)
                    {
                        var tvi = result.VisualHit.FindParent<TreeViewItem>();
                        if (tvi != null)
                        {
                            var context = tvi.DataContext as ClientListEntry;
                            if (context != null)
                            {
                                //we tapped on a client
                                var client = context;
                                Debug.WriteLine(string.Format("Tapped on client : {0}", client.Nickname));
                            }
                            var entry = tvi.DataContext as ChannelListEntry;
                            if (entry != null)
                            {
                                //we tapped on a channel
                                var channel = entry;
                                Debug.WriteLine(string.Format("Tapped on channel : {0}", channel.Name));
                                _asyncServerQueryRunner.MoveClient(_me.ClientId, channel.ChannelId);
                            }
                        }
                    }
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowHelper.ExtendWindowStyleWithTool(this);
            _scroller = (ScrollViewer) treeSpeak.Template.FindName("Scroller", treeSpeak);
            _scrollViewerAdaptor = new DragScrollViewerAdaptor(_scroller);

            RazerProvider.Razer.Touchpad.SetWindow(this, Touchpad.RenderMethod.Polling, new TimeSpan(0, 0, 0, 0, 42));

            Connect();
        }

        private void Connect()
        {
            try
            {
                Disconnect();

                _syncQueryDispatcher = new SyncTcpDispatcher("localhost", 25639);
                _syncQueryRunner = new QueryRunner(_syncQueryDispatcher);

                _me = _syncQueryRunner.SendWhoAmI();
                if (_me.IsErroneous)
                {
                    Debug.WriteLine("Not a TS3 client!");
                    Disconnect();
                    return;
                }

                GetChannelList();
                treeSpeak.ItemsSource = _channels;

                _asyncQueryDispatcher = new AsyncTcpDispatcher("localhost", 25639);
                _asyncQueryDispatcher.BanDetected += QueryDispatcher_BanDetected;
                _asyncQueryDispatcher.ReadyForSendingCommands += QueryDispatcher_ReadyForSendingCommands;
                _asyncQueryDispatcher.ServerClosedConnection += QueryDispatcher_ServerClosedConnection;
                _asyncQueryDispatcher.SocketError += QueryDispatcher_SocketError;
                _asyncQueryDispatcher.NotificationReceived += QueryDispatcher_NotificationReceived;
                _asyncQueryDispatcher.Connect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Disconnect();
            }
        }

        private void QueryDispatcher_ReadyForSendingCommands(object sender, EventArgs e)
        {
            // you can only run commands on the queryrunner when this event has been raised first!
            _asyncClientQueryRunner = new QueryRunner(_asyncQueryDispatcher);
            _asyncClientQueryRunner.Notifications.ChannelTalkStatusChanged += Notifications_ChannelTalkStatusChanged;
            _asyncClientQueryRunner.RegisterForNotifications(ClientNotifyRegisterEvent.Any);

            _asyncServerQueryRunner = new TS3QueryLib.Core.Server.QueryRunner(_asyncQueryDispatcher);
            _asyncServerQueryRunner.Notifications.ClientConnectionLost += Notifications_ClientConnectionLost;
            _asyncServerQueryRunner.Notifications.ClientDisconnect += Notifications_ClientDisconnect;
            _asyncServerQueryRunner.Notifications.ClientJoined += Notifications_ClientJoined;
            _asyncServerQueryRunner.Notifications.ClientKick += Notifications_ClientKick;
            _asyncServerQueryRunner.Notifications.ClientMoved += Notifications_ClientMoved;
            _asyncServerQueryRunner.Notifications.ClientMovedByTemporaryChannelCreate +=
                Notifications_ClientMovedByTemporaryChannelCreate;
            _asyncServerQueryRunner.Notifications.ClientMoveForced += Notifications_ClientMoveForced;
            _asyncServerQueryRunner.RegisterForNotifications(ServerNotifyRegisterEvent.Server |
                                                            ServerNotifyRegisterEvent.Channel);

            GetClientList();
        }

        private void GetChannelList()
        {
            ListResponse<ChannelListEntry> channels = _syncQueryRunner.GetChannelList(true);
            if (channels.IsErroneous)
                throw new Exception("Unable to get the list of channels");

            for (int i = channels.Values.Count - 1; i >= 0; i--)
            {
                if (channels.Values[i].ParentChannelId != 0)
                {
                    for (int j = channels.Values.Count - 1; j >= 0; j--)
                    {
                        if (channels.Values[j].ChannelId == channels.Values[i].ParentChannelId)
                        {
                            //we found the parent, we need to remove the child from list and add it to the parent
                            ChannelListEntry channel = channels.Values[i];
                            channels.Values.RemoveAt(i);

                            //Check we haven't run out of channels
                            if (channels.Count() - 1 < j)
                            {
                                j--;
                            }
                            channels.Values[j].Subchannels.Add(channel);
                            channels.Values[j].Subchannels =
                                new ObservableCollection<ChannelListEntry>(
                                    channels.Values[j].Subchannels.OrderBy(c => c.Order));
                            break;
                        }
                    }
                }
            }

            _channels = new ObservableCollection<ChannelListEntry>(channels.Values.OrderBy(c => c.Order));
        }

        private void GetClientList()
        {
            ListResponse<ClientListEntry> clients = _asyncServerQueryRunner.GetClientList(true);
            if (clients.IsErroneous)
                throw new Exception("Unable to get the list of clients");

            foreach (ChannelListEntry channel in _channels)
            {
                foreach (ClientListEntry client in clients.Values)
                {
                    if (client.ClientId == _me.ClientId)
                    {
                        client.IsMe = true;
                        _me.ChannelId = client.ChannelId;
                    }

                    if (client.ChannelId == channel.ChannelId)
                    {
                        channel.Clients.Add(client);
                        continue;
                    }

                    foreach (ChannelListEntry subchannel in channel.Subchannels)
                    {
                        if (client.ChannelId == subchannel.ChannelId)
                        {
                            subchannel.Clients.Add(client);
                            break;
                        }

                        if (subchannel.Subchannels.Count > 0)
                        {
                            ThroughEachChannel(subchannel, client);
                        }
                    }
                }
            }
        }

        private void ThroughEachChannel(ChannelListEntry channel, ClientListEntry client)
        {
            foreach (ChannelListEntry subchannel in channel.Subchannels)
            {
                if (client.ChannelId == subchannel.ChannelId)
                {
                    subchannel.Clients.Add(client);
                    break;
                }

                if (subchannel.Subchannels.Count > 0)
                {
                    ThroughEachChannel(subchannel, client);
                }
            }
        }

        private void RefreshChannelsAndClients()
        {
            Debug.WriteLine("Refreshing channel and client list");
            try
            {
                Application.Current.Dispatcher.Invoke((Action) delegate
                {
                    GetChannelList();
                    GetClientList();
                    treeSpeak.ItemsSource = _channels;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An error occured while refreshing channels and clients: " + ex.Message +
                                Environment.NewLine + ex.StackTrace);
            }
        }

        private void Notifications_ClientMoveForced(object sender, ClientMovedByClientEventArgs e)
        {
            Debug.WriteLine("Client move forced");
            RefreshChannelsAndClients();
        }

        private void Notifications_ClientMovedByTemporaryChannelCreate(object sender, ClientMovedEventArgs e)
        {
            Debug.WriteLine("Client move to temp channel");
            RefreshChannelsAndClients();
        }

        private void Notifications_ClientMoved(object sender, ClientMovedEventArgs e)
        {
            Debug.WriteLine("Client move");
            RefreshChannelsAndClients();
        }

        private void Notifications_ClientKick(object sender, ClientKickEventArgs e)
        {
            Debug.WriteLine("Client kick");
            if (e.VictimClientId == _me.ClientId)
            {
                Disconnect();
                return;
            }

            RefreshChannelsAndClients();
        }

        private void Notifications_ClientJoined(object sender, ClientJoinedEventArgs e)
        {
            Debug.WriteLine("Client join");
            RefreshChannelsAndClients();
        }

        private void Notifications_ClientDisconnect(object sender, ClientDisconnectEventArgs e)
        {
            Debug.WriteLine("Client disconnect");
            if (e.ClientId == _me.ClientId)
            {
                Disconnect();
                return;
            }

            RefreshChannelsAndClients();
        }

        private void Notifications_ClientConnectionLost(object sender, ClientConnectionLostEventArgs e)
        {
            Debug.WriteLine("Client connection lost");
            if (e.ClientId == _me.ClientId)
            {
                Disconnect();
                return;
            }

            RefreshChannelsAndClients();
        }

        private void QueryDispatcher_NotificationReceived(object sender, EventArgs<string> e)
        {
            Debug.WriteLine(e.Value);
        }

        private void Notifications_ChannelTalkStatusChanged(object sender, TalkStatusEventArgsBase e)
        {
            UpdateClientTalkStatus(_channels, e.ClientId, e.TalkStatus);
        }

        private void QueryDispatcher_ServerClosedConnection(object sender, EventArgs e)
        {
            // this event is raised when the connection to the server is lost.
            Debug.WriteLine("Connection to server closed/lost.");

            // dispose
            Connect();
        }

        private void QueryDispatcher_BanDetected(object sender, EventArgs<SimpleResponse> e)
        {
            Debug.WriteLine("You're account was banned!\nError-Message: {0}\nBan-Message:{1}", e.Value.ErrorMessage,
                e.Value.BanExtraMessage);

            // force disconnect
            Disconnect();
        }

        private void QueryDispatcher_SocketError(object sender, SocketErrorEventArgs e)
        {
            // do not handle connection lost errors because they are already handled by QueryDispatcher_ServerClosedConnection
            if (e.SocketError == SocketError.ConnectionReset)
                return;

            // this event is raised when a socket exception has occured
            Debug.WriteLine("Socket error!! Error Code: " + e.SocketError);

            // force disconnect
            Connect();
        }

        public void Disconnect()
        {
            _channels = new ObservableCollection<ChannelListEntry>();
            Application.Current.Dispatcher.Invoke((Action) delegate { treeSpeak.ItemsSource = null; });
            _me = null;

            if (_syncQueryDispatcher != null)
            {
                _syncQueryDispatcher.Dispose();
                _syncQueryDispatcher = null;
            }

            if (_syncQueryRunner != null)
            {
                _syncQueryRunner.Dispose();
                _syncQueryRunner = null;
            }

            if (_asyncClientQueryRunner != null)
                _asyncClientQueryRunner.Notifications.ChannelTalkStatusChanged -= Notifications_ChannelTalkStatusChanged;
            if (_asyncClientQueryRunner != null)
                _asyncClientQueryRunner.Dispose();
            if (_asyncServerQueryRunner != null)
                _asyncServerQueryRunner.Notifications.ClientConnectionLost -= Notifications_ClientConnectionLost;
            if (_asyncServerQueryRunner != null)
                _asyncServerQueryRunner.Notifications.ClientDisconnect -= Notifications_ClientDisconnect;
            if (_asyncServerQueryRunner != null)
                _asyncServerQueryRunner.Notifications.ClientJoined -= Notifications_ClientJoined;
            if (_asyncServerQueryRunner != null)
                _asyncServerQueryRunner.Notifications.ClientKick -= Notifications_ClientKick;
            if (_asyncServerQueryRunner != null)
                _asyncServerQueryRunner.Notifications.ClientMoved -= Notifications_ClientMoved;
            if (_asyncServerQueryRunner != null)
                _asyncServerQueryRunner.Notifications.ClientMovedByTemporaryChannelCreate -=
                    Notifications_ClientMovedByTemporaryChannelCreate;
            if (_asyncServerQueryRunner != null)
                _asyncServerQueryRunner.Notifications.ClientMoveForced -= Notifications_ClientMoveForced;
            if (_asyncServerQueryRunner != null)
                _asyncServerQueryRunner.Dispose();
            if (_asyncQueryDispatcher != null)
                _asyncQueryDispatcher.BanDetected -= QueryDispatcher_BanDetected;
            if (_asyncQueryDispatcher != null)
                _asyncQueryDispatcher.ReadyForSendingCommands -= QueryDispatcher_ReadyForSendingCommands;
            if (_asyncQueryDispatcher != null)
                _asyncQueryDispatcher.ServerClosedConnection -= QueryDispatcher_ServerClosedConnection;
            if (_asyncQueryDispatcher != null)
                _asyncQueryDispatcher.SocketError -= QueryDispatcher_SocketError;
            if (_asyncQueryDispatcher != null)
                _asyncQueryDispatcher.NotificationReceived -= QueryDispatcher_NotificationReceived;
            if (_asyncQueryDispatcher != null)
                _asyncQueryDispatcher.Dispose();
        }

        private static void UpdateClientTalkStatus(IEnumerable<ChannelListEntry> channels, uint clientId,
            TalkStatus talkStatus)
        {
            foreach (var t1 in channels)
            {
                if (t1.Clients.Any(c => c.ClientId == clientId))
                {
                    //we found our client that we need to change
                    foreach (var t in t1.Clients.Where(t => t.ClientId == clientId))
                    {
                        t.IsClientTalking = talkStatus == TalkStatus.TalkStarted;
                        return;
                    }
                }
                else
                {
                    //keep looking in subchannels until we find what we need
                    UpdateClientTalkStatus(t1.Subchannels, clientId, talkStatus);
                }
            }
        }
    }
}