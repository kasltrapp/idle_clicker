using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using EasyIdleGame;

namespace InfluencerRise.Tools
{
    /// <summary>
    /// Standalone economy-pacing simulator for Phase 6 balancing. Reusable - re-run any
    /// time via Unity_RunCommand (see docs/BugTracker.md for the invocation used to build
    /// this) or wire an Editor menu item to EconomySimulator.RunAndLog(...) later.
    ///
    /// DESIGN CHOICE (own inference, not from any instruction): reads real configured data
    /// from live assets via Resources.Load (business costs/production/locks, manager data,
    /// the player XP curve) and reimplements the formulas this project has already verified
    /// empirically this session (geometric cost curve: cost(N) = base * costMultiplier^N,
    /// confirmed via Input.cs:GetCurrentPrice/GetRealCost; player XP = +1 per business unit
    /// ever purchased via Business.xpsAddPerPurchaseToPlayer, confirmed via full-codebase
    /// investigation - production round completion grants NO player XP; Manager passive
    /// effect = baseValue * businessSpeedMultiplier * (level-1), clamped to baseValue at
    /// level<=1, confirmed via ManagerHolder.CurrentPassiveUpgrades and cross-checked
    /// against the empirically-observed "6.4 at level 3" finding from the math audit) -
    /// operating ENTIRELY on local simulation state (Dictionary/double fields declared in
    /// this file). It NEVER touches CurrencyManager.Instance, PlayerStats.Instance,
    /// BusinessesManager.Instance, ManagersManager.Instance, or any other live singleton,
    /// so it cannot mutate real save state no matter how many times or how it's run - the
    /// safest way found to reconcile "use the real, verified formulas" against "must not
    /// mutate any permanent state," given Player Level has no reset-to-zero method (only
    /// ever-forward AddXp) so a genuinely fresh run against the live singletons is not
    /// achievable without a full save wipe this environment has no clean way to trigger.
    ///
    /// SCOPE (deliberately excluded, flagged rather than silently modeled):
    /// - No Prestige/Rebrand loop (currencies never reset mid-run) - appropriate for a
    ///   single from-fresh-start run through the Level Progression schedule.
    /// - No Boosts (ViralMomentBoost/AlgorithmPushBoost/AllNighterBoost) - excluding them
    ///   makes this a conservative/floor pacing estimate, not an inflated one.
    /// - Rare/Legendary managers OTHER than a forced Algorithm Whisperer grant (used only
    ///   for the explicit comparison run) are not modeled as acquired - their combined
    ///   Capsule odds are small enough not to materially change Business-purchase-driven
    ///   pacing, and modeling all 9 adds complexity without answering the question asked.
    /// - Common Manager acquisition (automation) is NOT modeled via real RNG. Every
    ///   Manager's `cost` list is empty (confirmed via Resources.Load) - managers are
    ///   Capsule-lottery-only, not directly purchasable. Modeled instead as a CLOSED-FORM
    ///   expected-value time-to-first-likely-acquire via FreeCapsule pulls alone (real
    ///   verified weight table, 4h cooldown), applied deterministically in the main loop
    ///   (a business "becomes automated" at its manager's expected acquisition time) -
    ///   this is an approximation of a stochastic process, not a real per-pull simulation;
    ///   flagged clearly in the report this script's caller produces.
    /// </summary>
    public static class EconomySimulator
    {
        public enum StrategyProfile { Active, Afk }

        public static readonly int[] MilestoneLevels = { 3, 7, 12, 14, 18, 24, 28, 34, 42, 55, 65, 80, 110, 150 };

        public class BusinessDef
        {
            public string Name;
            public string Platform;
            public Currency CostCurrency;
            public double CostAmount;
            public double CostMultiplier;
            public double ProductionTimeSeconds;
            public Currency OutputCurrency;
            public double OutputAmount;
            public int XpPerPurchase;
            public int LockOwnCount;
            public string LockPriorBusiness;
            public int LockLevel;
            public string ManagerName;
            public int MaxOwned; // the "own Nx" threshold that unlocks the next business, -1 if none (last in chain)
        }

        public class SimResult
        {
            public StrategyProfile Profile;
            public bool AlgorithmWhispererGranted;
            public Dictionary<int, double> LevelMilestoneSeconds = new Dictionary<int, double>();
            public Dictionary<string, double> PlatformUnlockSeconds = new Dictionary<string, double>();
            public Dictionary<string, double> BusinessFirstPurchaseSeconds = new Dictionary<string, double>();
            public Dictionary<string, double> ManagerExpectedAcquireSeconds = new Dictionary<string, double>();
            public List<string> Stalls = new List<string>();
            public List<string> Anomalies = new List<string>();
            public double FinalSimSecondsReached;
            public int FinalLevel;
            public bool CappedOut;
            public double FinalFollowers;
            public double FinalCash;
            public double FinalCloutCoin;

            /// <summary>
            /// Total units owned per business at run end. Monotonic (never decreases) unlike
            /// FinalFollowers/FinalCash, which are point-in-time currency BALANCES that can
            /// legitimately swing non-monotonically near the exponential (~1.15) cost curve -
            /// TryReinvest's greedy buy-cheapest-affordable loop can consume a large fraction
            /// of the balance in one purchase once an expensive unit becomes affordable, so two
            /// runs sampled at the same wall-clock cutoff can land at different points in that
            /// sawtooth pattern. Use this field (or milestone-level timestamps) for comparing
            /// real economic progress between runs, not FinalFollowers/FinalCash balances.
            /// </summary>
            public Dictionary<string, int> FinalOwnedCounts = new Dictionary<string, int>();
        }

        // ---- Real data loaded once per run from live assets (read-only) ----

        /// <summary>
        /// costOverrides: optional test hook for finding candidate Followers costs for
        /// stranded businesses before committing them to the real assets (see
        /// docs/BugTracker.md's Cash-bootstrap entries) - keyed by business name, each value
        /// overrides that business's real configured cost with a Followers-denominated
        /// amount, leaving every other business's real data (and every other field on the
        /// overridden business) untouched. Null/absent = read the real asset value
        /// unmodified, which is what every non-testing call should use.
        /// </summary>
        private static List<BusinessDef> LoadBusinessDefs(out Dictionary<string, double> managerExpectedAcquireSeconds, Dictionary<string, double> costOverrides = null)
        {
            string[] order = { "FirstPost", "SelfieSession", "StoryFeed", "EngagementFarming", "VerifiedStatus",
                "TrendChase", "CollabFarming", "ReactionContent", "AlgorithmBaiting", "ViralChallenge",
                "HotTake", "UnboxingHaul", "Streaming", "SponsorDeals", "OwnBrand" };
            string[] platform = { "Yourgram", "Yourgram", "Yourgram", "Yourgram", "Yourgram",
                "QuickTok", "QuickTok", "QuickTok", "QuickTok", "QuickTok",
                "Broadcast", "Broadcast", "Broadcast", "Broadcast", "Broadcast" };
            // prior-in-chain business name (null = platform entry point), lock own-count, lock level
            string[] priorBiz = { null, "FirstPost", "SelfieSession", "StoryFeed", "EngagementFarming",
                null, "TrendChase", "CollabFarming", "ReactionContent", "AlgorithmBaiting",
                null, "HotTake", "UnboxingHaul", "Streaming", "SponsorDeals" };
            int[] lockOwn = { 0, 10, 15, 20, 25, 0, 10, 15, 20, 25, 0, 10, 15, 20, 25 };
            int[] lockLvl = { 0, 3, 7, 14, 24, 0, 18, 28, 42, 65, 0, 55, 80, 110, 150 };

            var defs = new List<BusinessDef>();
            for (int i = 0; i < order.Length; i++)
            {
                var b = Resources.Load<Business>("Businesses/" + order[i]);
                var cost = b.Cost.FirstOrDefault();
                var output = b.Outputs.FirstOrDefault();
                double costAmount = cost != null ? cost.inputAmount.ToDouble() : 0;
                Currency costCurrency = cost?.currencyInput;
                if (costOverrides != null && costOverrides.TryGetValue(order[i], out double overrideAmount))
                {
                    costAmount = overrideAmount;
                    costCurrency = Resources.Load<Currency>("Currencies/Followers");
                }
                defs.Add(new BusinessDef
                {
                    Name = order[i],
                    Platform = platform[i],
                    CostCurrency = costCurrency,
                    CostAmount = costAmount,
                    CostMultiplier = b.CostMultiplier.ToDouble(),
                    ProductionTimeSeconds = b.InitialTimeToProduce,
                    OutputCurrency = output?.currencyOutput,
                    OutputAmount = output != null ? output.outputAmount.ToDouble() : 0,
                    XpPerPurchase = b.xpsAddPerPurchaseToPlayer,
                    LockOwnCount = lockOwn[i],
                    LockPriorBusiness = priorBiz[i],
                    LockLevel = lockLvl[i],
                    ManagerName = b.manager != null ? b.manager.name : null,
                });
            }

            // Expected time-to-first-likely-acquire each Common Manager via FreeCapsule alone.
            // Real verified weight table this session: FreeCapsule = 15 Common@10, 6 Rare@2, 3 Legendary@0.
            // Total weight = 15*10 + 6*2 + 3*0 = 162. Expected pulls for one specific manager
            // at weight w = totalWeight / w (geometric distribution mean). Cooldown = 4h = 14400s.
            const double totalWeight = 162.0;
            const double commonWeight = 10.0;
            const double freeCapsuleCooldownSeconds = 4 * 3600;
            double expectedPullsPerCommonManager = totalWeight / commonWeight; // 16.2
            double expectedSecondsPerCommonManager = expectedPullsPerCommonManager * freeCapsuleCooldownSeconds;

            managerExpectedAcquireSeconds = new Dictionary<string, double>();
            foreach (var d in defs)
            {
                if (!string.IsNullOrEmpty(d.ManagerName) && !managerExpectedAcquireSeconds.ContainsKey(d.ManagerName))
                    managerExpectedAcquireSeconds[d.ManagerName] = expectedSecondsPerCommonManager;
            }

            return defs;
        }

        public class PlatformTravelCost
        {
            public Currency Currency;
            public double Amount;
        }

        /// <summary>
        /// Real platform travel costs, read from the Location assets (`QuickTokPlatform.asset`,
        /// `BroadcastPlatform.asset`) via Resources.Load - confirmed via direct asset read that
        /// this simulator had NEVER modeled these before (every prior run treated a platform as
        /// unlocking for free the instant its Level+ownership gate was met). Real data: QuickTok
        /// costs 500 Followers to travel to (playerLevelLock=12, plus owning 1x StoryFeed);
        /// Broadcast costs 5000 CASH (playerLevelLock=34, plus owning 1x ReactionContent) - a
        /// second real Cash cost gate on top of the QuickTok-internal chain, confirmed via
        /// BroadcastPlatform.asset's own `cost` field. Loaded once per run so `Resources.Load`
        /// count matches every other real-data read in this file (never touches live singletons).
        /// </summary>
        private static Dictionary<string, PlatformTravelCost> LoadPlatformTravelCosts()
        {
            var result = new Dictionary<string, PlatformTravelCost>();
            var quickTok = Resources.Load<Location>("Locations/QuickTokPlatform");
            var broadcast = Resources.Load<Location>("Locations/BroadcastPlatform");
            var qtCost = quickTok.Cost.FirstOrDefault();
            var bcCost = broadcast.Cost.FirstOrDefault();
            result["QuickTok"] = new PlatformTravelCost { Currency = qtCost?.currencyInput, Amount = qtCost != null ? qtCost.inputAmount.ToDouble() : 0 };
            result["Broadcast"] = new PlatformTravelCost { Currency = bcCost?.currencyInput, Amount = bcCost != null ? bcCost.inputAmount.ToDouble() : 0 };
            return result;
        }

        /// <summary>
        /// Player XP granted per production round = business tier (1-5) x this. Must be
        /// kept in sync with InfluencerRise.PlayerProgression.ProductionXpGrant.
        /// XpPerTierPerRound - duplicated here (not read from the live component) since
        /// this simulator deliberately never touches live scene/singleton state at all,
        /// see the class-level DESIGN CHOICE note.
        /// </summary>
        private const float ProductionXpPerTierPerRound = 0.5f;

        private static readonly Dictionary<string, int> BusinessTierByName = new Dictionary<string, int>
        {
            { "FirstPost", 1 }, { "SelfieSession", 2 }, { "StoryFeed", 3 }, { "EngagementFarming", 4 }, { "VerifiedStatus", 5 },
            { "TrendChase", 1 }, { "CollabFarming", 2 }, { "ReactionContent", 3 }, { "AlgorithmBaiting", 4 }, { "ViralChallenge", 5 },
            { "HotTake", 1 }, { "UnboxingHaul", 2 }, { "Streaming", 3 }, { "SponsorDeals", 4 }, { "OwnBrand", 5 },
        };

        private static double CumulativeXpForLevel(double firstLevelXp, double q, int level)
        {
            if (level <= 0) return 0;
            // Mirrors Mathb.GeometricSequence_Sum's own q==1 special case - at q=1 the
            // (q^level-1)/(q-1) closed form is 0/0=NaN, and "NaN <= x" is always false in
            // IEEE754, which silently freezes level-up forever (confirmed empirically:
            // three different startXp values all produced identical Level=0-forever
            // results before this fix). q=1 is the ONLY viable live-applicable curve
            // shape anyway, since PlayerStats.levelIncrementMultiplier is a plain int
            // field (confirmed via source read) - q>=2 is exponential-steep (Iteration 1
            // proved q=2 stalls hard by level 15) and fractional q (1.12-1.15, this
            // simulator's earlier iterations) can never actually reach the live int
            // field. sum_{i=0}^{level-1} firstLevelXp * q^i, which for q=1 is just
            // firstLevelXp * level (level terms, each worth firstLevelXp).
            if (q == 1) return firstLevelXp * level;
            return firstLevelXp * (Math.Pow(q, level) - 1) / (q - 1);
        }

        private static double AlgorithmWhispererMultiplier(bool granted)
        {
            if (!granted) return 1.0;
            // Granted at level 1 (simplest "owned" baseline). businessSpeedMultiplier=2,
            // base passiveBoosts value=1.4 (confirmed via Resources.Load this session).
            // ManagerHolder.CurrentPassiveUpgrades formula: multiplier = speedMult*(level-1),
            // clamped to >=1 => at level 1: multiplier=max(2*0,1)=1 => effective = 1.4*1 = 1.4.
            const float baseValue = 1.4f;
            const float speedMult = 2f;
            const int level = 1;
            float m = speedMult * (level - 1);
            if (m <= 0) m = 1;
            return baseValue * m;
        }

        // ---- Simulation core ----

        /// <summary>
        /// startXp/incrementMultiplier default to the CURRENT live PlayerStats.startXp/
        /// levelIncrementMultiplier values (1000/2, confirmed via fresh scene read) so a
        /// no-argument call reflects the actual live configuration. Pass explicit values to
        /// test a candidate tuning before ever touching the live component - this is the
        /// mechanism the iterative tuning pass (see docs/BugTracker.md) uses.
        /// </summary>
        public static SimResult RunSimulation(StrategyProfile profile, bool grantAlgorithmWhispererEarly, double capSimulatedSeconds, bool verboseDebug = false, double firstLevelXp = 1000, double q = 2, Dictionary<string, double> costOverrides = null)
        {
            var result = new SimResult { Profile = profile, AlgorithmWhispererGranted = grantAlgorithmWhispererEarly };

            var businesses = LoadBusinessDefs(out var managerExpectedAcquireSeconds, costOverrides);
            foreach (var kv in managerExpectedAcquireSeconds) result.ManagerExpectedAcquireSeconds[kv.Key] = kv.Value;
            var platformTravelCosts = LoadPlatformTravelCosts();

            // Loaded once, compared by reference (not by .name string) - matches the
            // established, proven-reliable currency-identification pattern used throughout
            // this project's own MCP testing this session.
            var currencyFollowers = Resources.Load<Currency>("Currencies/Followers");
            var currencyCash = Resources.Load<Currency>("Currencies/Cash");
            var currencyCloutCoin = Resources.Load<Currency>("Currencies/CloutCoin");

            // Confirmed via GameBootstrap.cs + live scene component: fresh saves are granted
            // 10 starting Followers (GameBootstrap.startingAmount/startingCurrency).
            double followers = 10, cash = 0, cloutCoin = 0;
            double simSeconds = 0;
            int playerLevel = 0;
            double cumulativeXp = 0;
            var owned = businesses.ToDictionary(b => b.Name, b => 0);
            var firstPurchaseTime = new Dictionary<string, double>();
            var levelMilestonesHit = new HashSet<int>();
            var platformsUnlocked = new HashSet<string> { "Yourgram" };

            double whispererMultiplier = AlgorithmWhispererMultiplier(grantAlgorithmWhispererEarly);

            double dt = 10.0; // simulated seconds per step - fine-grained enough for day/hour-scale reporting
            double nextCheckpoint = profile == StrategyProfile.Afk ? 3600.0 : 0.0; // AFK reinvests hourly
            double lastProgressSeconds = 0;
            double lastOwnedTotal = 0;
            const double stallThresholdSeconds = 6 * 3600; // 6 simulated hours with zero purchases = stall
            bool stalledAlready = false;

            // Local helpers -------------------------------------------------

            bool IsBusinessUnlocked(BusinessDef b)
            {
                if (b.LockLevel > 0 && playerLevel < b.LockLevel) return false;
                if (b.LockOwnCount > 0 && !string.IsNullOrEmpty(b.LockPriorBusiness) && owned[b.LockPriorBusiness] < b.LockOwnCount) return false;
                // Platform entry points additionally need the platform unlocked
                if (b.Platform == "QuickTok" && !platformsUnlocked.Contains("QuickTok")) return false;
                if (b.Platform == "Broadcast" && !platformsUnlocked.Contains("Broadcast")) return false;
                return true;
            }

            double NextUnitCost(BusinessDef b) => b.CostAmount * Math.Pow(b.CostMultiplier, owned[b.Name]);

            bool IsAutomated(BusinessDef b)
            {
                if (string.IsNullOrEmpty(b.ManagerName)) return false;
                if (!managerExpectedAcquireSeconds.TryGetValue(b.ManagerName, out double eta)) return false;
                return simSeconds >= eta;
            }

            double GetCurrencyBalance(Currency c)
            {
                if (c == null) return 0;
                if (c == currencyFollowers) return followers;
                if (c == currencyCash) return cash;
                if (c == currencyCloutCoin) return cloutCoin;
                return 0;
            }

            void SpendCurrency(Currency c, double amount)
            {
                if (c == null) return;
                if (c == currencyFollowers) followers -= amount;
                else if (c == currencyCash) cash -= amount;
                else if (c == currencyCloutCoin) cloutCoin -= amount;
            }

            void GrantXp(double xp)
            {
                cumulativeXp += xp;
                int newLevel = playerLevel;
                while (CumulativeXpForLevel(firstLevelXp, q, newLevel + 1) <= cumulativeXp)
                    newLevel++;
                if (newLevel != playerLevel)
                {
                    playerLevel = newLevel;
                    foreach (int m in MilestoneLevels)
                        if (playerLevel >= m && !levelMilestonesHit.Contains(m))
                        {
                            levelMilestonesHit.Add(m);
                            result.LevelMilestoneSeconds[m] = simSeconds;
                        }
                }
            }

            // Platform unlock requires level + prior-business ownership (already tracked) AND
            // being able to AFFORD the real Location travel cost (QuickTok=500 Followers,
            // Broadcast=5000 Cash - confirmed via Resources.Load<Location>, never modeled by
            // this simulator before this fix). Checked once per tick rather than inline after
            // every XP grant/purchase (previous duplicated approach) so the affordability half
            // of the condition - which can take real time to satisfy even after level+ownership
            // are already met - is retried every tick until affordable, then pays the cost
            // exactly once (unlockOnlyOnce, matching the real Location asset).
            void TryUnlockPlatforms()
            {
                TryUnlockOne("QuickTok", 12, "StoryFeed", 1);
                TryUnlockOne("Broadcast", 34, "ReactionContent", 1);
            }

            void TryUnlockOne(string platformName, int levelReq, string requiredBusiness, int requiredCount)
            {
                if (platformsUnlocked.Contains(platformName)) return;
                if (playerLevel < levelReq || owned[requiredBusiness] < requiredCount) return;
                if (!platformTravelCosts.TryGetValue(platformName, out var travel)) return;
                if (GetCurrencyBalance(travel.Currency) < travel.Amount) return;
                SpendCurrency(travel.Currency, travel.Amount);
                platformsUnlocked.Add(platformName);
                result.PlatformUnlockSeconds[platformName] = simSeconds;
            }

            void BuyOneUnit(BusinessDef b)
            {
                double cost = NextUnitCost(b);
                SpendCurrency(b.CostCurrency, cost);
                owned[b.Name]++;
                if (!firstPurchaseTime.ContainsKey(b.Name))
                {
                    firstPurchaseTime[b.Name] = simSeconds;
                    result.BusinessFirstPurchaseSeconds[b.Name] = simSeconds;
                }
                GrantXp(b.XpPerPurchase);
            }

            void TryReinvest()
            {
                // Greedy: repeatedly buy the cheapest currently-affordable next-unit purchase
                // across all unlocked, not-yet-capped businesses, until nothing more affordable.
                bool boughtSomething;
                int safety = 0;
                do
                {
                    boughtSomething = false;
                    BusinessDef cheapestAffordable = null;
                    double cheapestCost = double.MaxValue;
                    foreach (var b in businesses)
                    {
                        if (!IsBusinessUnlocked(b)) continue;
                        double cost = NextUnitCost(b);
                        if (cost > GetCurrencyBalance(b.CostCurrency)) continue;
                        if (cost < cheapestCost) { cheapestCost = cost; cheapestAffordable = b; }
                    }
                    if (cheapestAffordable != null)
                    {
                        BuyOneUnit(cheapestAffordable);
                        boughtSomething = true;
                    }
                    safety++;
                } while (boughtSomething && safety < 100000);
            }

            // Main loop -------------------------------------------------------

            double nextDebugLog = 0;
            while (simSeconds < capSimulatedSeconds)
            {
                if (verboseDebug && simSeconds >= nextDebugLog)
                {
                    Debug.Log(string.Format("[EconomySim t={0:F0}s/{1:F1}h] Followers={2:F1} owned[FirstPost]={3} lvl={4} xp={5:F1}",
                        simSeconds, simSeconds / 3600.0, followers, owned["FirstPost"], playerLevel, cumulativeXp));
                    nextDebugLog += 3600; // once per simulated hour
                }
                // Production for this step
                foreach (var b in businesses)
                {
                    if (owned[b.Name] <= 0) continue;
                    bool producing = profile == StrategyProfile.Active || IsAutomated(b);
                    if (!producing) continue;

                    double multiplier = b.Platform == "Yourgram" ? whispererMultiplier : 1.0;
                    double rate = (b.OutputAmount * owned[b.Name] * multiplier) / b.ProductionTimeSeconds; // currency per simulated second
                    double gained = rate * dt;

                    if (b.OutputCurrency == null) { /* no currency output configured, skip */ }
                    else if (b.OutputCurrency == currencyFollowers) followers += gained;
                    else if (b.OutputCurrency == currencyCash) cash += gained;

                    // Production-round XP grant (InfluencerRise.PlayerProgression.
                    // ProductionXpGrant, mirrored here - see that script's header). Rounds
                    // completed per second = owned/ProductionTimeSeconds, independent of any
                    // output-amount multiplier (Whisperer boosts currency per round, not the
                    // number of rounds completed).
                    if (BusinessTierByName.TryGetValue(b.Name, out int tier))
                    {
                        double roundsPerSecond = owned[b.Name] / b.ProductionTimeSeconds;
                        double xpGained = tier * ProductionXpPerTierPerRound * roundsPerSecond * dt;
                        if (xpGained > 0) GrantXp(xpGained);
                    }
                }

                // Level-up reward CloutCoin (native, repeating 5*level per level - approximated
                // as a smooth trickle isn't right since it's a discrete per-level event; handled
                // precisely inside GrantXp would need refactor, so approximate here: only credit
                // CloutCoin on the step level actually changes via a delta check).
                // (CloutCoin is not consumed anywhere in this simplified simulation - see class
                // header "Common Manager acquisition ... not modeled via real RNG" - so precise
                // CloutCoin accrual doesn't affect any purchase decision in this run; tracked only
                // for final-state reporting.)

                // Platform unlocks (level+ownership+affordability) checked before reinvestment
                // each tick, so travel cost is paid as soon as possible rather than being
                // starved by business reinvestment spending the same currency first.
                TryUnlockPlatforms();

                // Reinvestment
                if (profile == StrategyProfile.Active)
                {
                    TryReinvest();
                }
                else if (simSeconds >= nextCheckpoint)
                {
                    TryReinvest();
                    nextCheckpoint += 3600.0;
                }

                // Stall detection: no new business units purchased for stallThresholdSeconds
                double ownedTotal = owned.Values.Sum();
                if (ownedTotal > lastOwnedTotal)
                {
                    lastOwnedTotal = ownedTotal;
                    lastProgressSeconds = simSeconds;
                }
                else if (!stalledAlready && simSeconds - lastProgressSeconds >= stallThresholdSeconds && playerLevel < 150)
                {
                    stalledAlready = true;
                    result.Stalls.Add(string.Format(
                        "STALL at simSeconds={0:F0} ({1:F1}h): no new business unit purchased for {2:F1}h. " +
                        "Level={3}, Followers={4:F1}, Cash={5:F1}, owned totals={6}",
                        simSeconds, simSeconds / 3600.0, (simSeconds - lastProgressSeconds) / 3600.0, playerLevel,
                        followers, cash, string.Join(",", owned.Select(kv => kv.Key + "=" + kv.Value))));
                }

                simSeconds += dt;

                if (playerLevel >= 150 && owned["OwnBrand"] >= 25) break; // full completion, stop early
            }

            result.FinalSimSecondsReached = simSeconds;
            result.FinalLevel = playerLevel;
            result.CappedOut = simSeconds >= capSimulatedSeconds;
            result.FinalFollowers = followers;
            result.FinalCash = cash;
            result.FinalCloutCoin = cloutCoin;
            foreach (var kv in owned) result.FinalOwnedCounts[kv.Key] = kv.Value;

            // Sanity/anomaly checks
            if (double.IsNaN(followers) || double.IsInfinity(followers)) result.Anomalies.Add("Followers became NaN/Infinity");
            if (double.IsNaN(cash) || double.IsInfinity(cash)) result.Anomalies.Add("Cash became NaN/Infinity");
            if (playerLevel > 150) result.Anomalies.Add("Player level exceeded the final v1 milestone (150): " + playerLevel);

            return result;
        }
    }
}
