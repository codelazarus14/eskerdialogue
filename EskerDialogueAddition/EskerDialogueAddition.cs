using OWML.ModHelper;
using UnityEngine;
using Logger = EskerDialogueAddition.Util.Logger;

namespace EskerDialogueAddition
{
    public class EskerDialogueAddition : ModBehaviour
    {
        public static EskerDialogueAddition Instance;

        private TravelerController _eskerController;

        private EskerDialogue _eskerDialogue;

        private TextAsset _eskerText;

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            // for my testing convenience
            Application.runInBackground = true;

            Instance = this;

            ModHelper.HarmonyHelper.AddPrefix<TextTranslation>(
                nameof(TextTranslation._Translate),
                typeof(Patches),
                nameof(Patches.HideTranslationError));
            ModHelper.HarmonyHelper.AddPostfix<CharacterDialogueTree>(
                nameof(CharacterDialogueTree.LateInitialize),
                typeof(Patches),
                nameof(Patches.SetNewXml));
            ModHelper.HarmonyHelper.AddPostfix<DialogueNode>(
                nameof(DialogueNode.GetNextPage),
                typeof(Patches),
                nameof(Patches.SetPageText));
            ModHelper.HarmonyHelper.AddPostfix<DialogueOption>(
                nameof(DialogueOption.SetNodeId),
                typeof(Patches),
                nameof(Patches.SetOptionText));

            // Load custom EskerDialogue wrapper from JSON
            string filename = "eskertext.json";
            _eskerDialogue = ModHelper.Storage.Load<EskerDialogue>(filename);
            if (string.IsNullOrEmpty(_eskerDialogue.Text))
                Logger.LogError($"Error loading {filename} - text field is null or empty!");
            else
                Logger.Log($"Loaded {filename}");

            //convert to TextAsset to be used in SetTextXml
            _eskerText = new TextAsset(_eskerDialogue.Text);
            Logger.LogSuccess($"Finished loading!");

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;

                _eskerController = null;
                // Find Esker's controller
                TravelerController[] travelerControllers = FindObjectsOfType<TravelerController>();
                for (int i = 0; i < travelerControllers.Length; i++)
                {
                    if (travelerControllers[i].name.Equals("Villager_HEA_Esker"))
                        _eskerController = travelerControllers[i];
                }
            };
        }

        class Patches
        {
            public static void SetNewXml(CharacterDialogueTree __instance)
            {
                if (__instance._characterName.Equals("Esker"))
                {
                    Instance._eskerController._dialogueSystem.SetTextXml(Instance._eskerText);
                    Logger.LogSuccess("Updated Esker's dialogue with new text.");
                }

            }

            public static void SetPageText(DialogueNode __instance, out string mainText)
            {

                if (__instance._name.StartsWith("Esker"))
                {
                    // add to mainText directly w/o call to Translate()
                    mainText = __instance._listPagesToDisplay[__instance._currentPage].Trim();
                }
                else
                {
                    // default behavior in GetNextPage() - required(?) for an out parameter
                    string key = __instance._name + __instance._listPagesToDisplay[__instance._currentPage];
                    mainText = TextTranslation.Translate(key).Trim();
                }
            }

            public static void SetOptionText(DialogueOption __instance)
            {
                if (__instance._textID.StartsWith("Esker"))
                    // ruins all translatable strings of Esker dialogue but wtv   
                    __instance._textID = __instance._text;
            }

            public static bool HideTranslationError(TextTranslation __instance, ref string __result, string key)
            {
                // should only affect strings related to Esker
                string text = __instance.m_table.Get(key);
                if (text == null)
                {
                    Logger.Log("Suppressing translation error - someone's been hard-coding strings.");
                    __result = key;
                    // skip translation method
                    return false;
                }
                return true;
            }
        }

        // Wrapper class for exporting from JSON
        private class EskerDialogue
        {
            public string Text { get; set; }
        }

    }
}
