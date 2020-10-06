//extern alias NAudio;

using Dissonance;
using Dissonance.Audio.Capture;
using Dissonance.Integrations.MirrorIgnorance;
using Dissonance.Networking;
using Exiled.API.Features;
using MEC;
using Mirror;
//using NAudio::NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommsHack
{
    public class AudioAPI
    {
        public static AudioAPI API { get; internal set; }
        private Dictionary<string, Stream> _streams = new Dictionary<string, Stream>();
        private static Process prc;
        private static CoroutineHandle handle;

        public Stream GetNamedStream(string name)
        {
            return _streams[name];
        }

        public void RegisterNamedStream(string name, Stream stream)
        {
            _streams.Add(name, stream);
        }

        public bool ContainsNamedStream(string name)
        {
            return _streams.ContainsKey(name);
        }

        public void UnregisterNamedStream(string name)
        {
            _streams.Remove(name);
        }

        public void PlayFile(string path)
        {
            if (handle.IsValid)
            {
                Timing.KillCoroutines(handle);
            }
            Timing.RunCoroutine(WaitForConvert(path));
        }

        internal IEnumerator<float> WaitForConvert(string path)
        {
            if (File.Exists(path + ".raw"))
            {
                PlayFileRaw(path + ".raw");
                yield break;
                //File.Delete(path + ".raw");
            }
            Exiled.API.Features.Log.Info("Converting " + path);
            if (prc != null && !prc.HasExited)
                prc.Kill();
            prc = Process.Start(HackMain.main.Config.FFMPEG, "-i \"" + path + "\" -f f32le -ar 48000 -ac 1 " + path + ".raw");
            yield return Timing.WaitUntilTrue(() => prc.HasExited);
            Exiled.API.Features.Log.Info("Done!");
            PlayFileRaw(path + ".raw");
        }

        public void PlayFileRaw(string path)
        {
            FileStream reader = File.OpenRead(path);
            PlayStream(reader);
        }

        public void PlayStream(Stream stream)
        {
            PlayWithParams(stream, 9999, 0.5f, false);
        }

        public void PlayWithParams(Stream stream, ushort playerid, float volume, bool _3d)
        {
            var mirrorComms = GameObject.FindObjectOfType<MirrorIgnoranceCommsNetwork>();
            var comms = GameObject.FindObjectOfType<DissonanceComms>();
            mirrorComms.Client = null;
            //if (mirrorComms.Client != null)
            //mirrorComms.StopClient(); // avoid memory leaks?
            mirrorComms.StartClient(Unit.None);
            HackMain.client = mirrorComms.Client;
            if (comms.TryGetComponent<IMicrophoneCapture>(out var mic))
            {
                if (mic.IsRecording)
                    mic.StopCapture();
                UnityEngine.Object.Destroy((Component)mic);
            }
            mirrorComms.Mode = NetworkMode.Host;
            comms.gameObject.AddComponent<FloatArrayCapture>()._file = stream;
            comms._capture.Start(mirrorComms, comms.GetComponent<FloatArrayCapture>());
            comms._capture._micName = "StreamedMic";
            comms._capture.RestartTransmissionPipeline("Dummy");
            HackMain.clientInfo = mirrorComms.Server._clients.GetOrCreateClientInfo(playerid, "Dummy", new CodecSettings(Dissonance.Audio.Codecs.Codec.Opus, 48000, 960), new MirrorConn(NetworkServer.localConnection));
            var clientInfo = HackMain.clientInfo;
            //cooms.Server._clients.JoinRoom("Ghost", clientInfo);
            //cooms.Server._clients.JoinRoom("SCP", clientInfo);
            mirrorComms.Server._clients.JoinRoom("Null", clientInfo);
            mirrorComms.Server._clients.JoinRoom("Intercom", clientInfo);
            //comms.RoomChannels.Open("Ghost", false, ChannelPriority.High, 1f);
            //comms.RoomChannels.Open("SCP", false, ChannelPriority.High, 1f);
            comms.RoomChannels.Open("Null", false, ChannelPriority.High, volume);
            comms.RoomChannels.Open("Intercom", false, ChannelPriority.High, volume);
            foreach (var plr in Player.List)
            {
                mirrorComms.Server._clients.SendFakeClientState(new MirrorConn(plr.Connection), clientInfo);
            }
            comms.IsMuted = false;
        }
    }
}
