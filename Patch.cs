using System;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BreadcrumbTorch
{
    [HarmonyPatch(typeof(Game), "Start")]
    public class GameStartPatch {
        private static void Prefix() {
            ZRoutedRpc.instance.Register("RPC_SpawnBreadcrumbTorch", new Action<long, Vector3>(RPC_SpawnBreadcrumbTorch));
        }
        
        private static void RPC_SpawnBreadcrumbTorch(long sender, Vector3 position)
        {
            if (!ZNet.instance.IsServer()) return;

            var prefab = ZNetScene.instance.GetPrefab(ConfigurationFile.torchPieceName.Value);
            if (prefab == null) return;

            var go = Object.Instantiate(prefab, position, Quaternion.identity);

            var znetView = go.GetComponent<ZNetView>();
            if (znetView != null)
            {
                var zdo = znetView.GetZDO();
                zdo.Set(ZDOVars.s_creator, sender);
                zdo.Set("breadcrumbTorch", true);
            }
        }
    }
    
    [HarmonyPatch(typeof(Player), "Update")]
    public class PlaceTorchPatch
    {
        public static void Postfix(Player __instance)
        {
            if (__instance == null) return;

            if (ZInput.GetKeyDown(ConfigurationFile.torchSpawnKey.Value) && IsPlayerAbleToSpawnTorch(__instance))
            {
                if (ConfigurationFile.torchPlacement.Value == TorchPlacement.OnlyDungeons && !__instance.InInterior())
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, $"{Localization.instance.Localize("$msg_wrongbiome")}");
                    return;
                } 
                if (ConfigurationFile.torchPlacement.Value == TorchPlacement.OnlyOutside && __instance.InInterior())
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, $"{Localization.instance.Localize("$msg_notindungeon")}");
                    return;
                }

                Vector3 position = __instance.transform.position + Vector3.up * ConfigurationFile.torchHeightOffset.Value;
                Logger.Log("Spawning torch");
                ZRoutedRpc.instance.InvokeRoutedRPC("RPC_SpawnBreadcrumbTorch", position);
            }
        }

        private static bool IsPlayerAbleToSpawnTorch(Player player)
        {
            return player.CanMove() &&
                   !player.IsSwimming() &&
                   !InventoryGui.IsVisible() &&
                   !Game.IsPaused();
        }
    }

    [HarmonyPatch(typeof(Player), "CheckCanRemovePiece")]
    public static class CheckCanRemovePiecePatch
    {
        public static bool Prefix(Piece piece, ref bool __result)
        {
            if (piece == null) return true;

            var znetView = piece.GetComponent<ZNetView>();
            if (znetView == null || !znetView.IsValid()) return true;

            var zdo = znetView.GetZDO();
            if (zdo != null && zdo.GetBool("breadcrumbTorch"))
            {
                Logger.Log("breadcrumbTorch piece detected.");
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Piece), "DropResources")]
    public static class Piece_DropResources_Patch
    {
        public static bool Prefix(Piece __instance, HitData hitData)
        {
            if (__instance == null) return true;
            
            var znetView = __instance.gameObject.GetComponent<ZNetView>();
            if (znetView == null || !znetView.IsValid()) return true;

            var zdo = znetView.GetZDO();
            if (zdo != null && zdo.GetBool("breadcrumbTorch"))
            {
                Logger.Log("Skipping drops for breadcrumbTorch");
                var drops = __instance.m_resources;
                if (drops != null)
                {
                    __instance.m_resources = Array.Empty<Piece.Requirement>();
                }
            }

            return true;
        }
    }
}