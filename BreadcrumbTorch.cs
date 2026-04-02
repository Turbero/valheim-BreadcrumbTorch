using BepInEx;
using HarmonyLib;

namespace BreadcrumbTorch
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class BreadcrumbTorch : BaseUnityPlugin
    {
        public const string GUID = "Turbero.BreadcrumbTorch";
        public const string NAME = "Breadcrumb Torch";
        public const string VERSION = "1.0.2";

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
    }
}
