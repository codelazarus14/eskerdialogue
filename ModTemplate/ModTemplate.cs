using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using Logger = ModTemplate.Util.Logger;

namespace ModTemplate
{
    public class ModTemplate : ModBehaviour
    {
        public static ModTemplate Instance;

        private EskerDialogue _eskerDialogue;

        private TravelerController _eskerController;

        private TextAsset _eskerText;

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.

            // for my testing convenience
            Application.runInBackground = true;

            Instance = this;

            ModHelper.HarmonyHelper.AddPostfix<CharacterDialogueTree>(
                nameof(CharacterDialogueTree.LateInitialize), 
                typeof(Patches),
                nameof(Patches.OnDialogueTreeInitialized));

            // Load custom EskerDialogue wrapper from JSON
            _eskerDialogue = ModHelper.Storage.Load<EskerDialogue>("eskertext.json");
            if (string.IsNullOrEmpty(_eskerDialogue.text))
                Logger.LogError("Error loading eskertext.json!");
            else
                Logger.Log("Loaded eskertext.json");

            //convert to TextAsset to be used in SetTextXml
            _eskerText = new TextAsset(_eskerDialogue.text);
            Logger.LogSuccess($"Finished loading new dialogue");

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;

                _eskerController = null;
                // Find Esker's controller
                TravelerController[] travelerControllers = FindObjectsOfType<TravelerController>();
                for (int i = 0; i < travelerControllers.Length; i++)
                {
                    //Logger.Log($"Found traveler controller {travelerControllers[i]}");
                    if (travelerControllers[i].name.Equals("Villager_HEA_Esker"))
                        _eskerController = travelerControllers[i];
                }
            };
        }

        class Patches
        {
            //TODO: fix this.. beginning to feel like I shouldn't be patching LateInitialize
            // there's an NRE generated somewhere along the way but not by any of these fields
            // so i'm guessing it's either 1. i don't understand patching or 2. something is actually null
            // bc of late init when i'm going to access it even tho it shouldn't be
            public static void OnDialogueTreeInitialized()
            {
                if (Instance._eskerController._dialogueSystem._characterName.Equals("Esker"))
                {
                    Instance._eskerController._dialogueSystem.SetTextXml(Instance._eskerText);
                    Logger.LogSuccess("Updated Esker's dialogue with new xml");
                }

            }
        }

        // Wrapper class for exporting from JSON
        private class EskerDialogue
        {
            public string text { get; set; }
        }

    }
}
