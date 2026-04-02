using HarmonyLib;
using UnityEngine;

namespace BreadcrumbTorch
{
    [HarmonyPatch(typeof(Fireplace), "Start")]
    public static class Fireplace_Start_Patch
    {
        public static void Postfix(Fireplace __instance)
        {
            if (ZNet.instance != null && ZNet.instance.IsServer())
                return;

            float value = ConfigurationFile.fireVolume.Value / 100f;
            foreach (var audio in __instance.GetComponentsInChildren<AudioSource>())
                audio.minDistance = value;
        }

        public static void UpdateAllFireplaces()
        {
            var pieces = Object.FindObjectsByType<Fireplace>(FindObjectsSortMode.None);
            float value = ConfigurationFile.fireVolume.Value / 100f;
            foreach (var firePlace in pieces)
                foreach (var audio in firePlace.GetComponentsInChildren<AudioSource>())
                    audio.minDistance = value;
        }
    }
}