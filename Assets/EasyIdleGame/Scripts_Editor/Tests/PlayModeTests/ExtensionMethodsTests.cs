using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame.Tests
{
    public class ExtensionMethodsTests
    {
        [Test]
        public void InputListExtensionMethods_Squash_CombinesIdenticalInputs()
        {
            var currency = ScriptableObject.CreateInstance<Currency>();

            var inputs = new List<Input>
            {
                new Input { currencyInput = currency, inputAmount = 10 },
                new Input { currencyInput = currency, inputAmount = 20 }
            };

            var squashed = inputs.Squash();

            Assert.AreEqual(1, squashed.Count, "Should squash to single element.");
            Assert.AreEqual(new BigNumber(30), squashed[0].inputAmount, "Amounts should be summed.");

            Object.DestroyImmediate(currency);
        }

        [Test]
        public void OutputListExtensionMethods_Squash_CombinesIdenticalOutputs()
        {
            var currency = ScriptableObject.CreateInstance<Currency>();

            var outputs = new List<Output>
            {
                new Output { currencyOutput = currency, outputAmount = 15 },
                new Output { currencyOutput = currency, outputAmount = 25 }
            };

            var squashed = outputs.Squash();

            Assert.AreEqual(1, squashed.Count, "Should squash to single element.");
            Assert.AreEqual(new BigNumber(40), squashed[0].outputAmount, "Amounts should be summed.");

            Object.DestroyImmediate(currency);
        }

        [Test]
        public void LockListExtensionMethods_Multiply_ScalesAllLocks()
        {
            var lck = new Lock { inputAmount = 5 };
            var list = new List<Lock> { lck };

            var multiplied = list.Multiply(4, false);

            Assert.AreEqual(1, multiplied.Count, "Should maintain count.");
            Assert.AreEqual(new BigNumber(20), multiplied[0].inputAmount, "Amount should be multiplied.");
        }
    }
}
