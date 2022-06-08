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
            // the if and else if are both executed one after the other - so at some point this is called before Esker's initialized..
            // but it does overwrite their dialogue.. just not perfectly (displays the Name field for the option for some reason)
            public static void OnDialogueTreeInitialized()
            {

                if (Instance._eskerController._dialogueSystem._characterName is null)
                    Logger.LogError($"Name is null for {Instance._eskerController.name}!");
                else if (Instance._eskerController._dialogueSystem._characterName.Equals("Esker"))
                {
                    Instance._eskerController._dialogueSystem.SetTextXml(Instance._eskerText);
                    Logger.Log($"Esker's name is {Instance._eskerController._dialogueSystem._characterName}");
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
