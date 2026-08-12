using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "newBusinessWrapper", menuName = "EasyIdleGame/Businesses/Business Wrapper", order = 2)]
    public class BusinessWrapper : Business
    {
        [CommentArea("Business Wrapper", "A wrapper that copies all data from its 'underlyingBusiness'. This allows you to easily duplicate businesses with the exact same balancing but different visual representations, without needing to copy everything manually.")]
        [SerializeField] private string _businessWrapperComment;

        [Header("Wrapper Settings")]
        [Tooltip("Base business template copied into this wrapper on enable/validate. Leave null only while authoring; with a value assigned, wrapper-specific self-references are remapped to this wrapper.")]
        public Business underlyingBusiness;

        [Header("Scale Settings")]
        [Tooltip("Multiplier applied to the base cost of the business.")]
        public BigNumber costScale = 1;

        [Tooltip("Multiplier applied to the production cost of the business.")]
        public BigNumber productionCostScale = 1;

        [Tooltip("Multiplier applied to the upgrade cost of the business.")]
        public BigNumber upgradeCostScale = 1;

        [Tooltip("Multiplier applied to the production time of the business.")]
        public BigNumber timeToProduceScale = 1;

        [Tooltip("Multiplier applied to the outputs produced by the business.")]
        public BigNumber outputsScale = 1;

        private void OnEnable()
        {
            SyncData();
        }

        private void OnValidate()
        {
            SyncData();
        }

        [ContextMenu("Force Sync Data")]
        public void SyncData()
        {
            if (underlyingBusiness == null) return;

            string json = JsonUtility.ToJson(underlyingBusiness);
            JsonUtility.FromJsonOverwrite(json, this);

            ReplaceSelfReferences();
            ApplyScale();
        }

        private void ReplaceSelfReferences()
        {
            if (cost != null) cost.ForEach(ReplaceInput);
            if (productionCost != null) productionCost.ForEach(ReplaceInput);
            if (upgradeCost != null) upgradeCost.ForEach(ReplaceInput);

            if (locks != null) locks.ForEach(ReplaceLock);
            if (upperLocks != null) upperLocks.ForEach(ReplaceLock);

            if (outputTables != null)
            {
                foreach (var table in outputTables)
                {
                    if (table.outputs != null)
                    {
                        table.outputs.ForEach(ReplaceOutput);
                    }
                }
            }

            if (levelupData != null)
            {
                if (levelupData.levelRewards != null)
                {
                    foreach (var levelReward in levelupData.levelRewards)
                    {
                        if (levelReward.levelReward != null && levelReward.levelReward.rewardTables != null)
                        {
                            foreach (var table in levelReward.levelReward.rewardTables)
                            {
                                if (table.outputs != null) table.outputs.ForEach(ReplaceOutput);
                            }
                        }
                    }
                }

                if (levelupData.eachLevelReward != null)
                {
                    foreach (var levelReward in levelupData.eachLevelReward)
                    {
                        if (levelReward.levelReward != null && levelReward.levelReward.rewardTables != null)
                        {
                            foreach (var table in levelReward.levelReward.rewardTables)
                            {
                                if (table.outputs != null) table.outputs.ForEach(ReplaceOutput);
                            }
                        }
                    }
                }
            }
        }

        private void ApplyScale()
        {
            if (costScale != 1 && cost != null) InputListExtensionMethods.MultiplyInPlace(cost, costScale);
            if (productionCostScale != 1 && productionCost != null) InputListExtensionMethods.MultiplyInPlace(productionCost, productionCostScale);
            if (upgradeCostScale != 1 && upgradeCost != null) InputListExtensionMethods.MultiplyInPlace(upgradeCost, upgradeCostScale);

            if (timeToProduceScale != 1) timeToProduce = (float)(timeToProduceScale * timeToProduce).ToDouble();

            if (outputsScale != 1)
            {
                DropTableListExtensionMethods.MultiplyInPlace(outputTables, outputsScale);
                levelupData?.MultiplyInPlace(outputsScale);
            }
        }

        private void ReplaceInput(Input input)
        {
            if (input != null && input.businessInput == underlyingBusiness)
            {
                input.businessInput = this;
            }
        }

        private void ReplaceLock(Lock lck)
        {
            if (lck != null && lck.businessLock == underlyingBusiness)
            {
                lck.businessLock = this;
            }
        }

        private void ReplaceOutput(Output output)
        {
            if (output != null && output.businessOutput == underlyingBusiness)
            {
                output.businessOutput = this;
            }
        }
    }
}
