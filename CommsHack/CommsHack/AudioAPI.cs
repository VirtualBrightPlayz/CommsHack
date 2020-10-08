//extern alias NAudio;

using Assets._Scripts.Dissonance;
using Dissonance;
using Dissonance.Audio.Capture;
using Dissonance.Audio.Playback;
using Dissonance.Integrations.MirrorIgnorance;
using Dissonance.Networking;
using Exiled.API.Features;
using MEC;
using Mirror;
using RemoteAdmin;
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

        public void PlayFile(string path, float volume)
        {
            if (handle.IsValid)
            {
                Timing.KillCoroutines(handle);
            }
            Timing.RunCoroutine(WaitForConvert(path, volume));
        }

        internal IEnumerator<float> WaitForConvert(string path, float volume)
        {
            if (File.Exists(path + ".raw"))
            {
                PlayFileRaw(path + ".raw", volume);
                yield break;
                //File.Delete(path + ".raw");
            }
            Exiled.API.Features.Log.Info("Converting " + path);
            if (prc != null && !prc.HasExited)
                prc.Kill();
            prc = Process.Start(HackMain.main.Config.FFMPEG, "-i \"" + path + "\" -f f32le -ar 48000 -ac 1 " + path + ".raw");
            yield return Timing.WaitUntilTrue(() => prc.HasExited);
            Exiled.API.Features.Log.Info("Done!");
            PlayFileRaw(path + ".raw", volume);
        }

        public void PlayFileRaw(string path, float volume)
        {
            FileStream reader = File.OpenRead(path);
            PlayStream(reader, volume);
        }

        public void PlayStream(Stream stream, float volume)
        {
            PlayWithParams(stream, 9999, volume, false, Vector3.zero);
        }

        public void PlayWithParams(Stream stream, ushort playerid, float volume, bool _3d, Vector3 position)
        {
            var mirrorComms = GameObject.FindObjectOfType<MirrorIgnoranceCommsNetwork>();
            var comms = GameObject.FindObjectOfType<DissonanceComms>();
            //mirrorComms.Client = null;
            /*if (mirrorComms.Client != null)
                mirrorComms.StopClient();*/ // avoid memory leaks?
            if (mirrorComms.Client == null)
                mirrorComms.StartClient(Unit.None);
            HackMain.client = mirrorComms.Client;
            if (comms.TryGetComponent<IMicrophoneCapture>(out var mic))
            {
                if (mic.IsRecording)
                    mic.StopCapture();
                UnityEngine.Object.Destroy((Component)mic);
            }
            mirrorComms.Mode = NetworkMode.Host;
            var capt = comms.gameObject.AddComponent<FloatArrayCapture>();
            capt._file = stream;
            comms._capture.Start(mirrorComms, capt);
            comms._capture._micName = "StreamedMic";
            comms._capture.RestartTransmissionPipeline("Dummy");
            HackMain.clientInfo = mirrorComms.Server._clients.GetOrCreateClientInfo(playerid, "Dummy", new CodecSettings(Dissonance.Audio.Codecs.Codec.Opus, 48000, 960), new MirrorConn(NetworkServer.localConnection));
            var clientInfo = HackMain.clientInfo;
            comms.IsMuted = false;
            if (_3d)
            {
                //mirrorComms.Server._clients.JoinRoom("Null", clientInfo);
                //comms.RoomChannels.Open("Null", true, ChannelPriority.High, 1f);
                GameObject go = GameObject.Instantiate(NetworkManager.singleton.playerPrefab);
                go.transform.position = position;
                go.GetComponent<CharacterClassManager>().CurClass = RoleType.Tutorial;
                go.GetComponent<CharacterClassManager>().GodMode = true;
                //go.GetComponent<CharacterClassManager>().RefreshPlyModel();
                go.GetComponent<NicknameSync>().Network_myNickSync = clientInfo.PlayerName;
                go.GetComponent<QueryProcessor>().PlayerId = playerid;
                go.GetComponent<QueryProcessor>().NetworkPlayerId = playerid;
                //GameObject.Destroy(go.GetComponent<CustomBroadcastTrigger>());
                go.GetComponent<CustomBroadcastTrigger>().enabled = true;
                go.GetComponent<CustomBroadcastTrigger>().BroadcastPosition = true;
                go.GetComponent<CustomBroadcastTrigger>().PlayerId = playerid.ToString();
                go.GetComponent<CustomBroadcastTrigger>().ChannelType = CommTriggerTarget.Room;
                //go.GetComponent<CustomBroadcastTrigger>().Priority = ChannelPriority.High;
                go.GetComponent<CustomBroadcastTrigger>().RoomName = "Proximity";
                //go.GetComponent<CustomBroadcastTrigger>().OpenChannel();
                go.GetComponent<Radio>().isVoiceChatting = true;
                go.GetComponent<Radio>().NetworkisVoiceChatting = true;
                go.GetComponent<MirrorIgnorancePlayer>()._playerId = clientInfo.PlayerName;
                go.GetComponent<MirrorIgnorancePlayer>().Network_playerId = clientInfo.PlayerName;
                go.GetComponent<DisableUselessComponents>()._added = true;
                go.GetComponent<CharacterClassManager>().IsVerified = true;
                go.GetComponent<CharacterClassManager>().NetworkIsVerified = true;
                NetworkServer.Spawn(go);
                mirrorComms.Server._clients.JoinRoom("Proximity", clientInfo);
                comms.RoomChannels.Open("Proximity", true, ChannelPriority.None, volume);
                //PlayerManager.RemovePlayer(go);
                Timing.RunCoroutine(SpawnLate(go, clientInfo, playerid));
                //GameObject.Destroy(go.GetComponent<CustomBroadcastTrigger>());
                //go.GetComponent<DissonanceUserSetup>().enabled = false;
            }
            else
            {
                var items = comms.RoomChannels._openChannelsBySubId.ToArray();
                foreach (var item in items)
                {
                    comms.RoomChannels.Close(item.Value);
                }
                mirrorComms.Server._clients.LeaveRoom("Null", clientInfo);
                mirrorComms.Server._clients.LeaveRoom("Intercom", clientInfo);
                mirrorComms.Server._clients.JoinRoom("Null", clientInfo);
                mirrorComms.Server._clients.JoinRoom("Intercom", clientInfo);
                comms.RoomChannels.Open("Null", false, ChannelPriority.None, volume);
                comms.RoomChannels.Open("Intercom", false, ChannelPriority.None, volume);
            }
            //mirrorComms.Server._clients.OnAddedClient(clientInfo);
            //comms._players.Add(new LocalVoicePlayerState(playerid.ToString(), comms._capture, comms.Rooms, comms.RoomChannels, comms.PlayerChannels, comms._capture, comms.GetComponent<ICommsNetwork>()));
            /*foreach (var plr in Player.List)
            {
                mirrorComms.Server._clients.SendFakeClientState(new MirrorConn(plr.ReferenceHub.characterClassManager.connectionToClient), clientInfo);
            }*/
        }

        internal IEnumerator<float> SpawnLate(GameObject go, ClientInfo<MirrorConn> clientInfo, ushort playerid)
        {
            /*go.GetComponent<MirrorIgnorancePlayer>().SetPlayerName(clientInfo.PlayerName);
            go.GetComponent<MirrorIgnorancePlayer>().CmdSetPlayerName(clientInfo.PlayerName);*/
            yield return Timing.WaitForOneFrame;
            go.GetComponent<QueryProcessor>().PlayerId = playerid;
            go.GetComponent<QueryProcessor>().NetworkPlayerId = playerid;
            //go.GetComponent<DissonanceUserSetup>().currentProfile = go.GetComponent<DissonanceUserSetup>().voice[Team.TUT];
            go.GetComponent<DissonanceUserSetup>().SetSpeakingFlags((SpeakingFlags)0);
            go.GetComponent<CustomBroadcastTrigger>().RoomName = "Proximity";
            //go.GetComponent<MirrorIgnorancePlayer>().CmdSetPlayerName(clientInfo.PlayerName);
            go.GetComponent<MirrorIgnorancePlayer>().RpcSetPlayerName(clientInfo.PlayerName);
            yield return Timing.WaitForOneFrame;
            Exiled.API.Features.Log.Info(go.GetComponent<Radio>().state == null);
            try
            {
                ((IVoicePlaybackInternal)go.GetComponent<Radio>().unityPlayback).StartPlayback();
            }
            catch (System.Exception e)
            {
                Exiled.API.Features.Log.Error(e);
            }
            var ccm = go.GetComponent<CharacterClassManager>();
            yield return Timing.WaitForOneFrame;
            //go.GetComponent<PlayerMovementSync>().OverridePosition(go.transform.position, 0f);
            /*var posdata = new PlayerPositionData(go.GetComponent<ReferenceHub>());
            NetworkServer.SendToAll(new PlayerPositionManager.PositionMessage(new PlayerPositionData[] { posdata }, 1, 0));*/
            Exiled.API.Features.Log.Info(go.GetComponent<CustomBroadcastTrigger>().IsTransmitting);
            Exiled.API.Features.Log.Info(go.GetComponent<CustomBroadcastTrigger>().ChannelType);
            Exiled.API.Features.Log.Info(go.GetComponent<CustomBroadcastTrigger>().RoomName);
            Exiled.API.Features.Log.Info(go.GetComponent<CustomBroadcastTrigger>().ShouldActivate(go.GetComponent<CustomBroadcastTrigger>().IsUserActivated()));
            //Exiled.API.Features.Log.Info(Radio.comms.FindPlayer(clientInfo.PlayerName) == null);
            //Exiled.API.Features.Log.Info(Radio.comms.FindPlayer(clientInfo.PlayerName).Playback == null);
            // fuck the exiled ghostmode patch
            //PlayerManager.RemovePlayer(go);
            yield return Timing.WaitForOneFrame;
            Exiled.API.Features.Log.Info(go.GetComponent<CustomBroadcastTrigger>().RoomName);
            Exiled.API.Features.Log.Info(go.GetComponent<Radio>().unityPlayback.IsSpeaking);
        }
    }
}
