using CommandSystem;
using Dissonance;
using Dissonance.Audio.Capture;
using Dissonance.Integrations.MirrorIgnorance;
using Dissonance.Networking;
using Exiled.API.Features;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommsHack
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class RACmd : ICommand
    {
        public string Command => "audio";

        public string[] Aliases => new string[0];

        public string Description => "Destroy their eardrums lol";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            /*Timing.KillCoroutines(HackMain.handle);
            HackMain.handle = Timing.RunCoroutine(HackMain.main.UpdateClient());*/
            {
                var cooms = GameObject.FindObjectOfType<MirrorIgnoranceCommsNetwork>();
                var comms = GameObject.FindObjectOfType<DissonanceComms>();
                //cooms.Client = null;
                /*if (cooms.Client != null)
                    cooms.StopClient();*/
                HackMain.client = null;
                if (HackMain.client == null && cooms.Client == null)
                {
                    cooms.StartClient(Unit.None);
                }
                HackMain.client = cooms.Client;
                if (comms.TryGetComponent<IMicrophoneCapture>(out var mic))
                {
                    if (mic.IsRecording)
                        mic.StopCapture();
                    UnityEngine.Object.Destroy((Component)mic);
                }
                cooms.Mode = NetworkMode.Host;
                comms._capture.Start(cooms, comms.gameObject.AddComponent<BasicFileStreamingCapture>());
                var arr = arguments.Array.ToList();
                arr.RemoveAt(0);
                comms._capture._micName = string.Join(" ", arr.ToArray());
                comms._capture.RestartTransmissionPipeline("Dummy");
                var clientInfo = HackMain.clientInfo;
                HackMain.clientInfo = cooms.Server._clients.GetOrCreateClientInfo(9999, "Dummy", new CodecSettings(Dissonance.Audio.Codecs.Codec.Opus, 48000, 960), new MirrorConn(NetworkServer.localConnection));
                clientInfo.AddRoom("Ghost");
                clientInfo.AddRoom("SCP");
                clientInfo.AddRoom("Null");
                cooms.Server._clients.JoinRoom("Ghost", clientInfo);
                cooms.Server._clients.JoinRoom("SCP", clientInfo);
                cooms.Server._clients.JoinRoom("Null", clientInfo);
                new RoomChannel?(comms.RoomChannels.Open("Ghost", false, ChannelPriority.High, 1f));
                new RoomChannel?(comms.RoomChannels.Open("SCP", false, ChannelPriority.High, 1f));
                new RoomChannel?(comms.RoomChannels.Open("Null", false, ChannelPriority.High, 1f));
                foreach (var plr in Player.List)
                {
                    cooms.Server._clients.SendFakeClientState(new MirrorConn(plr.Connection), clientInfo);
                }
                comms.IsMuted = false;
            }
            response = "Playing";
            return true;
        }
    }
}
