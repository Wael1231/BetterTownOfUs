using System.Linq;
using HarmonyLib;
using UnhollowerBaseLib;

namespace BetterTownOfUs
{
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetUnlockedHats))]
    public class UnlockHats
    {
        public static bool Prefix(HatManager __instance, ref Il2CppReferenceArray<HatBehaviour> __result)
        {
            var array = (
                from h in __instance.AllHats.ToArray()
                where h.Free || SaveManager.GetPurchase(h.ProductId, h.BundleId)
                select h
                into o
                orderby o.Order descending, o.name
                select o).ToArray();
            __result = array;
            return false;
        }
    }
}