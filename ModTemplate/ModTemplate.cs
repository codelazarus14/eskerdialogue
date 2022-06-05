using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace ModTemplate
{
    public class ModTemplate : ModBehaviour
    {
        private TextAsset _textAsset;
        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(ModTemplate)} is loaded!", MessageType.Success);

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                var playerBody = FindObjectOfType<PlayerBody>();
                ModHelper.Console.WriteLine($"Found player body, and it's called {playerBody.name}!",
                    MessageType.Success);

                TravelerController tc = null;
                // Look for Esker's controller
                TravelerController[] travelerControllers = FindObjectsOfType<TravelerController>();
                for (int i = 0; i < travelerControllers.Length; i++)
                {
                    if (travelerControllers[i].name.Equals("Villager_HEA_Esker"))
                        tc = travelerControllers[i];
                }
                if (tc == null)
                    ModHelper.Console.WriteLine("Failed to find Esker's controller!", MessageType.Error);
                _textAsset = tc._dialogueSystem._xmlCharacterDialogueAsset;
                ModHelper.Console.WriteLine($"Found raw xml text asset: {_textAsset}", MessageType.Success);
                // TODO: somehow modify/edit text asset in-transit with new dialogue so I don't have to create my own
                // or figure out another way to modify the character's dialogue with CharacterDialogueTree
            };
        }
    }
}
