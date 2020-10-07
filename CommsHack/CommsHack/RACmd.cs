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
using System.IO;
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
            var arr = arguments.Array.ToList();
            arr.RemoveAt(0);
            arr.RemoveAt(0);
            string str = string.Join(" ", arr);
            /*if (sender is PlayerCommandSender)
            {
                var plr = sender as PlayerCommandSender;
                FileStream reader = File.OpenRead(str);
                AudioAPI.API.PlayWithParams(reader, 9998, 1f, true, plr.CCM.transform.position);
                response = "Playing3D";
                return true;
            }*/
            /*Timing.KillCoroutines(HackMain.handle);
            HackMain.handle = Timing.RunCoroutine(HackMain.main.UpdateClient());*/
            {
                if (str.ToLower().EndsWith(".raw"))
                    AudioAPI.API.PlayFileRaw(str, float.Parse(arguments.Array[1]));
                else
                    AudioAPI.API.PlayFile(str, float.Parse(arguments.Array[1]));
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
