using OWML.ModHelper;
using System.Collections.Generic;
using UnityEngine;
using Logger = EskerDialogueAddition.Util.Logger;

namespace EskerDialogueAddition
{
    public class EskerDialogueAddition : ModBehaviour
    {
        public static EskerDialogueAddition Instance;

        private TextAsset _eskerText;

        private TextTranslation _translation;

        private void Awake()
        {

        }

        private void Start()
        {
            // for my testing convenience
            Application.runInBackground = true;

            Instance = this;

            ModHelper.HarmonyHelper.AddPostfix<CharacterDialogueTree>(
                nameof(CharacterDialogueTree.LateInitialize),
                typeof(Patches),
                nameof(Patches.UpdateEskerDialogue));

            // Load custom EskerDialogue wrapper from JSON
            string filename = "eskertext.json";
            EskerDialogue eskerDialogue = ModHelper.Storage.Load<EskerDialogue>(filename);
            if (string.IsNullOrEmpty(eskerDialogue.Text))
            {
                Logger.LogError($"Error loading {filename} - text field is null or empty!");
                return;
            }
            else Logger.Log($"Loaded {filename}");

            _translation = FindObjectOfType<TextTranslation>();

            //convert to TextAsset to be used in CharacterDialogueTree.SetTextXml()
            _eskerText = new TextAsset(eskerDialogue.Text);
            Logger.LogSuccess($"Finished loading!");
        }

        class Patches
        {
            public static void UpdateEskerDialogue(CharacterDialogueTree __instance)
            {
                if (__instance._characterName.Equals("Esker"))
                {
                    // recreate character dialogue nodes from text asset
                    __instance.SetTextXml(Instance._eskerText);
                    Logger.LogSuccess("Updated Esker's dialogue with new text.");

                    int counter = 0;
                    foreach (DialogueNode dnode in __instance._mapDialogueNodes.Values)
                    {
                        // update w new page text
                        List<DialogueText.TextBlock> blocks = dnode.DisplayTextData._listTextBlocks;
                        foreach (DialogueText.TextBlock block in blocks)
                        {
                            foreach (string page in block.listPageText)
                            {
                                // ignore if we're not adding a new line
                                if (Instance._translation.m_table.Get(dnode.Name + page) == null)
                                {
                                    Instance._translation.m_table.Insert(dnode.Name + page, page);
                                    counter++;
                                }
                            }
                        }
                        // update w new option text
                        List<DialogueOption> options = dnode.ListDialogueOptions;
                        foreach (DialogueOption option in options)
                        {
                            if (Instance._translation.m_table.Get(option._textID) == null)
                            {
                                Instance._translation.m_table.Insert(option._textID, option._text);
                                counter++;
                            }
                        }
                    }
                    Logger.Log($"Added {counter} strings to {Instance._translation.GetLanguage()} translation table");
                }

            }
        }

        // Wrapper class for exporting from JSON
        private class EskerDialogue
        {
            public string Text { get; set; }
        }

    }
}
