using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using EFT.UI;
using LockableDoors.Common;
using LockableDoors.Fika;
using LockableDoors.Helpers;
using LockableDoors.Patches;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace LockableDoors
{
    [BepInDependency("xyz.drakia.doorrandomizer", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("Jehree.LockableDoors", "LockableDoors", "1.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public const string DataToServerURL = "/jehree/lockabledoors/data_to_server";
        public const string DataToClientURL = "/jehree/lockabledoors/data_to_client";

        public static bool FikaInstalled { get; private set; }
        public static bool IAmDedicatedClient { get; private set; }
        public static ManualLogSource LogSource;


        private void Awake()
        {
            FikaInstalled = Chainloader.PluginInfos.ContainsKey("com.fika.core");
            IAmDedicatedClient = Chainloader.PluginInfos.ContainsKey("com.fika.dedicated");

            LogSource = Logger;
            Settings.Init(Config);
            LogSource.LogWarning("Ebu is cute :3");

            new GetAvailableActionsPatch().Enable();
            new GameStartedPatch().Enable();
            new GameEndedPatch().Enable();

            ConsoleScreen.Processor.RegisterCommandGroup<ConsoleCommands>();
        }

        private void OnEnable()
        {
            FikaInterface.InitOnPluginEnabled();
        }
    }
}