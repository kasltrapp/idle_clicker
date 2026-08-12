using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "newBusinessMergeRecipe", menuName = "EasyIdleGame/Businesses/Business Merge Recipe", order = 2)]
    public class BusinessMergeRecipe : ScriptableObject, ISerializationCallbackReceiver, IMetadataProvider
    {
        [CommentArea("Business Merge Recipe", "Defines a conversion rule that consumes inputs and produces outputs. Inputs support businesses and currencies; outputs use DropTables to support weighted or randomized rewards including businesses, currencies, boosts, and managers.", "Create a recipe with two Level 1 Workers as inputs and a Level 2 Worker output table. Call BusinessesManager.TryMergeBusinesses(recipe, out outputs) from your merge UI button.")]
        [SerializeField] private string _recipeComment;

        [Tooltip("Display metadata shown by recipe UI and demo cards.")]
        public Metadata metadata = new Metadata();

        [ConditionalCommentArea("Recipe setup", "No inputs are configured. This merge recipe can be executed without consuming resources if exposed in gameplay.", "inputs", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _inputsInfo;
        [Tooltip("Inputs consumed when this recipe executes, such as business copies or currencies. Empty inputs make the recipe free if exposed by UI/code.")]
        public List<Input> inputs = new List<Input>();

        [ConditionalCommentArea("Recipe setup", "No outputs are configured. This merge recipe can consume inputs without producing anything.", "outputTables", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _outputTablesInfo;
        [Tooltip("Drop tables applied when the recipe succeeds. Empty output tables consume inputs without granting anything.")]
        public List<DropTable> outputTables = new List<DropTable>();

        [Header("Audio")]
        [Tooltip("Sound played when this merge recipe succeeds.")]
        public AudioData mergeSound;
        [Header("Timing")]
        [Min(0)]
        [Tooltip("Base seconds required to complete this recipe when a game uses timed merging. 0 keeps merges immediate.")]
        public float mergeDurationSeconds;

        [Min(0)]
        [Tooltip("Base cooldown seconds after this recipe is used when a game uses merge cooldowns. 0 means no cooldown.")]
        public float mergeCooldownSeconds;

        // ---- Legacy migration fields (do not use) ----

        [HideInInspector]
        [Tooltip("Legacy input data migrated into inputs during deserialization. Hidden and not used for new recipes.")]
        [FormerlySerializedAs("input")]
        public LegacyBusinessMergeHolder[] _legacyBusinessInputs;

        [HideInInspector]
        [Tooltip("Legacy business input data migrated into inputs during deserialization. Hidden and not used for new recipes.")]
        [FormerlySerializedAs("businessInputs")]
        public LegacyBusinessMergeHolder[] _legacyBusinessInputs2;

        [HideInInspector]
        [Tooltip("Legacy output data migrated into outputTables during deserialization. Hidden and not used for new recipes.")]
        [FormerlySerializedAs("output")]
        public LegacyBusinessMergeHolder[] _legacyBusinessOutputs;

        [HideInInspector]
        [Tooltip("Legacy business output data migrated into outputTables during deserialization. Hidden and not used for new recipes.")]
        [FormerlySerializedAs("businessOutputs")]
        public LegacyBusinessMergeHolder[] _legacyBusinessOutputs2;

        public Metadata Metadata => metadata;

        public string GetIconString()
        {
            return metadata.GetIconString();
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            MigrateInputs(_legacyBusinessInputs);
            MigrateInputs(_legacyBusinessInputs2);
            MigrateOutputs(_legacyBusinessOutputs);
            MigrateOutputs(_legacyBusinessOutputs2);

            _legacyBusinessInputs = null;
            _legacyBusinessInputs2 = null;
            _legacyBusinessOutputs = null;
            _legacyBusinessOutputs2 = null;
        }

        private void MigrateInputs(LegacyBusinessMergeHolder[] legacy)
        {
            if (legacy == null || legacy.Length == 0) return;
            if (inputs == null) inputs = new List<Input>();

            foreach (var entry in legacy)
            {
                if (entry.business == null) continue;
                inputs.Add(new Input { businessInput = entry.business, inputAmount = entry.amount });
            }
        }

        private void MigrateOutputs(LegacyBusinessMergeHolder[] legacy)
        {
            if (legacy == null || legacy.Length == 0) return;
            if (outputTables == null) outputTables = new List<DropTable>();

            foreach (var entry in legacy)
            {
                if (entry.business == null) continue;
                outputTables.Add(new DropTable
                {
                    strategy = DropStrategy.All,
                    outputs = new List<Output>
                    {
                        new Output { businessOutput = entry.business, outputAmount = entry.amount }
                    }
                });
            }
        }
    }

    /// <summary> Legacy holder kept only for serialization migration. Do not use. </summary>
    [System.Serializable]
    public class LegacyBusinessMergeHolder
    {
        [FormerlySerializedAs("recipe")]
        [Tooltip("Legacy business target used only to migrate old merge recipe data. Do not edit for new recipes.")]
        public Business business;

        [Tooltip("Legacy business amount used only to migrate old merge recipe data. Do not edit for new recipes.")]
        public int amount;
    }
}

