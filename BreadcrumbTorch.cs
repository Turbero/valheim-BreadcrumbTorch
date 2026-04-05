using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace BreadcrumbTorch
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class BreadcrumbTorch : BaseUnityPlugin
    {
        public const string GUID = "Turbero.BreadcrumbTorch";
        public const string NAME = "Breadcrumb Torch";
        public const string VERSION = "1.0.3";

        private readonly Harmony harmony = new Harmony(GUID);

        void Awake()
        {
            ConfigurationFile.LoadConfig(this);

            harmony.PatchAll();
        }

        void onDestroy()
        {
            harmony.UnpatchSelf();
        }
        
        private void Start()
        {
            StartCoroutine(WaitForNetworking());
        }

        private System.Collections.IEnumerator WaitForNetworking()
        {
            // Wait until full networking initialization
            while (ZRoutedRpc.instance == null || ZNet.instance == null)
                yield return new WaitForSeconds(1f);
            
            // Commands registration
            Commands.RegisterConsoleCommand();
        }
    }
    
    internal static class Commands
    {
        public static void RegisterConsoleCommand()
        {
            new Terminal.ConsoleCommand("force_ghost_off_from_all_pieces", "Force -ghost effect off- in all world pieces", args =>
            {
                if (ZNet.instance != null && ZNet.instance.IsServer())
                {
                    Logger.Log("Running force_ghost_off_from_all_pieces...");
                    var pieces = Object.FindObjectsByType<Piece>(FindObjectsSortMode.None);
                    foreach (var piece in pieces)
                    foreach (var col in piece.GetComponentsInChildren<Collider>())
                        Physics.IgnoreLayerCollision(col.gameObject.layer, LayerMask.NameToLayer("character"), false);
                }
                else
                {
                    Logger.Log("Not admin");
                }
            });
        }
    }
}
