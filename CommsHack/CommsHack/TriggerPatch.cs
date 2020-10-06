using HarmonyLib;
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
            if (__instance.GetComponent<QueryProcessor>().PlayerId == 9999)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
