using System;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    [Obsolete("Use TestUtilities instead for non-inheriting test setups")]
    public abstract class GeneralTestsBase : TestsBase
    {
        protected Business TestBusiness => Resources.Load<Business>("__testBusiness");
        protected Business TestBusinessWithPriceMultiplier = Resources.Load<Business>("__testBusinessWith1.8PriceMult");

        protected Currency TestCurrency => Resources.Load<Currency>("__testCurrency");

        protected GenericMultiplierBoost TestBoost1 => Resources.Load<GenericMultiplierBoost>("__test0.65PriceBoost");
        protected GenericMultiplierBoost TestBoost2 => Resources.Load<GenericMultiplierBoost>("__test0.33PriceBoost");

        // make sure autoupgrade is disable and autoupgradeonfirstlevel is enabled on the test manager
        protected Manager TestManager => Resources.Load<Manager>("__testManager");
        protected Business TestBusinessWithManager => Resources.Load<Business>("__testBusinessWithManager");

        protected Upgrade TestUpgrade => Resources.Load<Upgrade>("__testUpgrade");

    }
}