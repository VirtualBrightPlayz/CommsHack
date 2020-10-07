using Dissonance;
using HarmonyLib;
using Mirror;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommsHack
{
    [HarmonyPatch(typeof(CustomBroadcastTrigger), nameof(CustomBroadcastTrigger.IsUserActivated))]
    public class TriggerPatch
    {
        public static bool Prefix(CustomBroadcastTrigger __instance, ref bool __result)
        {
            if (__instance.GetComponent<QueryProcessor>().PlayerId >= 9000 || __instance.GetComponent<NetworkIdentity>().connectionToClient == null)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(VoiceBroadcastTrigger), "get_CanTrigger")]
    public class TriggerPatch2
    {
        public static bool Prefix(VoiceBroadcastTrigger __instance, ref bool __result)
        {
            if (__instance.GetComponent<QueryProcessor>().PlayerId >= 9000 || __instance.GetComponent<NetworkIdentity>().connectionToClient == null)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BaseCommsTrigger), "get_TokenActivationState")]
    public class TriggerPatch3
    {
        public static bool Prefix(BaseCommsTrigger __instance, ref bool __result)
        {
            if (__instance.GetComponent<QueryProcessor>().PlayerId >= 9000 || __instance.GetComponent<NetworkIdentity>().connectionToClient == null)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(VoiceBroadcastTrigger), nameof(VoiceBroadcastTrigger.ShouldActivate))]
    public class TriggerPatch4
    {
        public static bool Prefix(VoiceBroadcastTrigger __instance, ref bool __result, bool intent)
        {
            if (__instance.GetComponent<QueryProcessor>().PlayerId >= 9000 || __instance.GetComponent<NetworkIdentity>().connectionToClient == null)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
