using Assets._Scripts.Dissonance;
using Dissonance;
using Dissonance.Audio.Capture;
using Dissonance.Config;
using Dissonance.Integrations.MirrorIgnorance;
using Dissonance.Networking;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using Dissonance.Networking.Client;

namespace CommsHack
{
    public class HackMain : Plugin<HackConfig>
    {
        public override string Name => "CommsHack";
        public MirrorIgnoranceCommsNetwork cooms;
        public static FileStream file;
        public static MirrorIgnoranceClient client;
        public static ClientInfo<MirrorConn> clientInfo;
        public static CoroutineHandle handle;
        public static HackMain main;

        public override void OnDisabled()
        {
            base.OnDisabled();
            Exiled.Events.Handlers.Player.Joined -= Events_PlayerJoinEvent;
            Timing.KillCoroutines(handle);
            main = null;
        }

        public override void OnEnabled()
        {
            base.OnEnabled();
            Exiled.Events.Handlers.Player.Joined += Events_PlayerJoinEvent;
            handle = Timing.RunCoroutine(UpdateClient());
            main = this;
        }

        public IEnumerator<float> UpdateClient()
        {
            while (true)
            {
                yield return Timing.WaitForOneFrame;
                if (client != null)
                {
                     client.Update();
                }
            }
        }

        public void Events_PlayerJoinEvent(JoinedEventArgs ev)
        {
            cooms = GameObject.FindObjectOfType<MirrorIgnoranceCommsNetwork>();
            var comms = GameObject.FindObjectOfType<DissonanceComms>();
            //GameObject go = GameObject.Instantiate(NetworkManager.singleton.playerPrefab);
            file = File.OpenRead(Config.CommsFile);
            byte[] bytes = new byte[512];
            file.Read(bytes, 0, bytes.Length);
            /*var bfsc = new BasicFileStreamingCapture();
            bfsc.StartCapture(Config.CommsFile);
            comms._capture.Start(cooms, bfsc);*/
            client = null;
            if (client == null && cooms.Client == null)
            {
                cooms.StartClient(Unit.None);
            }
            {
                client = cooms.Client;//.CreateClient(Unit.None);
                if (comms.TryGetComponent<IMicrophoneCapture>(out var mic))
                {
                    if (mic.IsRecording)
                        mic.StopCapture();
                    UnityEngine.Object.Destroy((Component)mic);
                }
                cooms.Mode = NetworkMode.Host;
                comms._capture.Start(cooms, comms.gameObject.AddComponent<BasicFileStreamingCapture>());
                comms._capture._micName = Config.CommsFile;
                comms._capture.RestartTransmissionPipeline("Dummy");
                client.Connect();
                clientInfo = cooms.Server._clients.GetOrCreateClientInfo(9999, "Dummy", new CodecSettings(Dissonance.Audio.Codecs.Codec.Opus, 48000, 960), new MirrorConn(NetworkServer.localConnection));
                clientInfo.AddRoom("Ghost");
                clientInfo.AddRoom("SCP");
                clientInfo.AddRoom("Null");
                cooms.Server._clients.JoinRoom("Ghost", clientInfo);
                cooms.Server._clients.JoinRoom("SCP", clientInfo);
                cooms.Server._clients.JoinRoom("Null", clientInfo);
                new RoomChannel?(comms.RoomChannels.Open("Ghost", false, ChannelPriority.High, 1f));
                new RoomChannel?(comms.RoomChannels.Open("SCP", false, ChannelPriority.High, 1f));
                new RoomChannel?(comms.RoomChannels.Open("Null", false, ChannelPriority.High, 1f));
                cooms.Server._clients.SendFakeClientState(new MirrorConn(ev.Player.Connection), clientInfo);
                comms.IsMuted = false;
                //new RoomChannel?(comms.RoomChannels.Open("Ghost"));
                //new RoomChannel?(comms.RoomChannels.Open("SCP"));
                //new RoomChannel?(comms.RoomChannels.Open("Null"));
                //new RoomChannel?(comms.RoomChannels.Open("Role"));


                //PacketWriter packetWriter = new PacketWriter(new byte[1024]);
                //packetWriter.WriteClientState(cooms.Server._sessionId, clientInfo.PlayerName, clientInfo.PlayerId, clientInfo.CodecSettings, clientInfo.Rooms);
                //cooms.Server.Send(packetWriter.Written, new MirrorConn(ev.Player.Connection), 0);
            }
            //client.SendVoiceData(new ArraySegment<byte>(bytes));
            //cooms.Server._network.SendVoice();

            /*List<OpenChannel> list = new List<OpenChannel>();
            list.Add(new OpenChannel(ChannelType.Room, 0, new ChannelProperties(comms), false, "Ghost".ToRoomId(), "Ghost"));
            list.Add(new OpenChannel(ChannelType.Room, 0, new ChannelProperties(comms), false, "SCP".ToRoomId(), "SCP"));
            list.Add(new OpenChannel(ChannelType.Room, 0, new ChannelProperties(comms), false, "Null".ToRoomId(), "Null"));
            foreach (var item in cooms.RoomChannels)
            {
            }
            //PacketWriter packet = new PacketWriter();
            client.SendVoiceData(new ArraySegment<byte>(bytes));
            PacketWriter packetV = new PacketWriter(new byte[1024]);
            packetV.WriteVoiceData(cooms.Server.SessionId, 9999, 0, 0, list, new ArraySegment<byte>(bytes));
            //PacketReader p = new PacketReader(packetV.Written);
            //ev.Player.Connection.Send(new DissonanceNetworkMessage(packetV.Written), 1);
            cooms.Server.Send(packetV.Written, new MirrorConn(ev.Player.Connection), 1);*/
            //cooms.Server._relay.ProcessPacketRelay(ref p, false);
        }
    }
}
