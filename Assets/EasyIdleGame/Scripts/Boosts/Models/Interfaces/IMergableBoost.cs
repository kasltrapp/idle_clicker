namespace EasyIdleGame
{
    public interface IMergableBoost
    {
        public bool TryMergeBoost(ActiveBoostHolder self, ActiveBoostHolder other)
        {
            if (!CanMergeBoost(other))
            {
                return false;
            }
            MergeBoost(self, other);
            return true;
        }

        public abstract bool CanMergeBoost(ActiveBoostHolder other);

        public abstract void MergeBoost(ActiveBoostHolder self, ActiveBoostHolder other);
    }
}
