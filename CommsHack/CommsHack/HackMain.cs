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
using Dissonance.Networking.Client;
using HarmonyLib;

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
        public static Harmony inst;

        public override void OnDisabled()
        {
            base.OnDisabled();
            //Exiled.Events.Handlers.Player.Joined -= Events_PlayerJoinEvent2;
            inst.UnpatchAll();
            inst = null;
            main = null;
            AudioAPI.API = null;
            Timing.KillCoroutines(handle);
        }

        public override void OnEnabled()
        {
            //Exiled.Events.Events.DisabledPatches.Add(typeof(PlayerPositionManager).GetMethod(nameof(PlayerPositionManager.TransmitData)));
            base.OnEnabled();
            //Exiled.Events.Handlers.Player.Joined += Events_PlayerJoinEvent2;
            handle = Timing.RunCoroutine(UpdateClient());
            main = this;
            inst = new Harmony("virtualbrightplayz.commhack.scpsl");
            inst.PatchAll();
            AudioAPI.API = new AudioAPI();
        }

        public void Events_PlayerJoinEvent2(JoinedEventArgs ev)
        {
            string str = Config.CommsFile;
            if (str.ToLower().EndsWith(".raw"))
                AudioAPI.API.PlayFileRaw(str, 0.5f);
            else
                AudioAPI.API.PlayFile(str, 0.5f);
        }

        public IEnumerator<float> UpdateClient()
        {
            while (true)
            {
                yield return Timing.WaitForOneFrame;
                if (client != null && !client._disconnected)
                {
                    for (int i = 0; i < DebugSettings.Instance._levels.Count; i++)
                    {
                        DebugSettings.Instance._levels[i] = LogLevel.Trace;
                    }
                    if (client.Update() == ClientStatus.Error)
                    {
                        Exiled.API.Features.Log.Error("Client error!!!");
                    }
                }
            }
        }

        public void Events_PlayerJoinEvent(JoinedEventArgs ev)
        {
            cooms = GameObject.FindObjectOfType<MirrorIgnoranceCommsNetwork>();
            var comms = GameObject.FindObjectOfType<DissonanceComms>();
            file = File.OpenRead(Config.CommsFile);
            byte[] bytes = new byte[512];
            file.Read(bytes, 0, bytes.Length);
            client = null;
            if (client == null && cooms.Client == null)
            {
                cooms.StartClient(Unit.None);
            }
            {
                client = cooms.Client;
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
                cooms.Server._clients.JoinRoom("Null", clientInfo);
                cooms.Server._clients.JoinRoom("Intercom", clientInfo);
                comms.RoomChannels.Open("Null", false, ChannelPriority.High, 1f);
                comms.RoomChannels.Open("Intercom", false, ChannelPriority.High, 1f);
                cooms.Server._clients.SendFakeClientState(new MirrorConn(ev.Player.Connection), clientInfo);
                comms.IsMuted = false;
            }
        }
    }
}
