using Assets._Scripts.Dissonance;
using CommandSystem;
using Dissonance;
using Dissonance.Audio.Capture;
using Dissonance.Integrations.MirrorIgnorance;
using Dissonance.Networking;
using Exiled.API.Features;
using MEC;
using Mirror;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommsHack
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
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
                HackMain.clientInfo = cooms.Server._clients.GetOrCreateClientInfo(9999, "Dummy", new CodecSettings(Dissonance.Audio.Codecs.Codec.Opus, 48000, 960), new MirrorConn(NetworkServer.localConnection));
                var clientInfo = HackMain.clientInfo;
                /*if (!(sender is PlayerCommandSender))
                {*/
                    //cooms.Server._clients.JoinRoom("Ghost", clientInfo);
                    //cooms.Server._clients.JoinRoom("SCP", clientInfo);
                    cooms.Server._clients.JoinRoom("Null", clientInfo);
                    cooms.Server._clients.JoinRoom("Intercom", clientInfo);
                    //comms.RoomChannels.Open("Ghost", false, ChannelPriority.High, 1f);
                    //comms.RoomChannels.Open("SCP", false, ChannelPriority.High, 1f);
                    comms.RoomChannels.Open("Null", false, ChannelPriority.High, 1f);
                    comms.RoomChannels.Open("Intercom", false, ChannelPriority.High, 1f);
                /*}*/
                response = "Playing";
                /*if (sender is PlayerCommandSender)
                {
                    cooms.Server._clients.JoinRoom("Proximity", clientInfo);
                    cooms.Server._clients.JoinRoom("Null", clientInfo);
                    comms.RoomChannels.Open("Proximity", true, ChannelPriority.High, 1f);
                    comms.RoomChannels.Open("Null", true, ChannelPriority.High, 1f);
                    var plr = sender as PlayerCommandSender;
                    GameObject go = GameObject.Instantiate(NetworkManager.singleton.playerPrefab);
                    go.transform.position = plr.CCM.transform.position;
                    go.GetComponent<CharacterClassManager>().CurClass = RoleType.Tutorial;
                    go.GetComponent<CharacterClassManager>().GodMode = true;
                    //go.GetComponent<CharacterClassManager>().RefreshPlyModel();
                    go.GetComponent<NicknameSync>().Network_myNickSync = clientInfo.PlayerName;
                    go.GetComponent<QueryProcessor>().PlayerId = 9999;
                    go.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
                    go.GetComponent<MirrorIgnorancePlayer>().SetPlayerName(clientInfo.PlayerName);
                    //GameObject.Destroy(go.GetComponent<CustomBroadcastTrigger>());
                    go.AddComponent<CustomBroadcastTrigger>().Mode = CommActivationMode.VoiceActivation;
                    //go.GetComponent<DissonanceUserSetup>().enabled = false;
                    NetworkServer.SpawnWithClientAuthority(go, NetworkServer.localConnection);
                    Timing.RunCoroutine(SpawnLate(go, plr));
                    response += "3d";
                }*/
                foreach (var plr in Player.List)
                {
                    cooms.Server._clients.SendFakeClientState(new MirrorConn(plr.Connection), clientInfo);
                }
                comms.IsMuted = false;
                return true;
            }
        }

        public IEnumerator<float> SpawnLate(GameObject go, PlayerCommandSender plr)
        {
            yield return Timing.WaitForOneFrame;
            yield return Timing.WaitForOneFrame;
            //go.GetComponent<PlayerMovementSync>().OverridePosition(plr.CCM.transform.position, plr.CCM.transform.rotation.eulerAngles.y, false);
            go.GetComponent<DissonanceUserSetup>().voice[Team.TUT].Apply();
            go.GetComponent<CustomBroadcastTrigger>().OpenChannel();
        }
    }
}
