using System.Collections;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace BreadcrumbTorch
{
    [HarmonyPatch(typeof(Fireplace), "Start")]
    public static class Fireplace_Start_Patch
    {
        [UsedImplicitly]
        public static void Postfix(Fireplace __instance)
        {
            if (ZNet.instance != null && ZNet.instance.IsServer())
                return;

            var value = ConfigurationFile.firePlaceVolume.Value / 100f;
            foreach (var audio in __instance.GetComponentsInChildren<AudioSource>())
                audio.minDistance = value;
        }

        public static void UpdateAllFireplaces()
        {
            var pieces = Object.FindObjectsByType<Fireplace>(FindObjectsSortMode.None);
            var value = ConfigurationFile.firePlaceVolume.Value / 100f;
            foreach (var firePlace in pieces)
                foreach (var audio in firePlace.GetComponentsInChildren<AudioSource>())
                    audio.minDistance = value;
        }
    }
    
    [HarmonyPatch(typeof(Player), "OnSpawned")]
    public static class Player_OnSpawned_Sound_Patch
    {
        [UsedImplicitly]
        public static void Postfix(Player __instance)
        {
            __instance.StartCoroutine(ApplyFireplaceVolumeDelayed());
        }

        private static IEnumerator ApplyFireplaceVolumeDelayed()
        {
            yield return new WaitForSeconds(1f);
            Fireplace_Start_Patch.UpdateAllFireplaces();
        }
    }
}