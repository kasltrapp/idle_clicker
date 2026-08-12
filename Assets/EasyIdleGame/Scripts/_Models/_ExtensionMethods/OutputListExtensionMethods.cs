using System.Collections.Generic;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that extends Output menthods for lists
    /// </summary>
    public static class OutputListExtensionMethods
    {
        public static void ApplyOuputs(this List<Output> outputs, BigNumber amount)
        {
            outputs.ForEach(x => x.OnProductionRoundFinished_Apply(amount));
        }

        public static List<Output> MultiplyOutputs(this List<Output> outputs, BigNumber multiplier)
        {
            List<Output> multipliedOutputs = new List<Output>();
            outputs.ForEach(x => multipliedOutputs.Add(x.Multiply(multiplier)));
            return multipliedOutputs;
        }

        public static List<Output> GetCurrenciesOutputs(this List<Output> outputs)
        {
            return outputs.FindAll(x => x.currencyOutput != null);
        }

        public static List<Output> GetBusinessesOutputs(this List<Output> outputs)
        {
            return outputs.FindAll(x => x.businessOutput != null);
        }

        public static List<Output> Squash(this List<Output> outputs)
        {
            List<Output> squashedOutputs = new List<Output>();
            foreach (var output in outputs)
            {
                if (output.businessOutput != null || output.currencyOutput != null || output.boostOutput != null || output.managerOutput != null)
                {
                    var existingOutput = squashedOutputs.Find(x =>
                        x.businessOutput == output.businessOutput &&
                        x.currencyOutput == output.currencyOutput &&
                        x.boostOutput == output.boostOutput &&
                        x.managerOutput == output.managerOutput);

                    if (existingOutput != null)
                    {
                        existingOutput.outputAmount += output.outputAmount;
                        existingOutput.outputMaxAmount += output.outputMaxAmount;
                    }
                    else
                    {
                        squashedOutputs.Add(new Output
                        {
                            businessOutput = output.businessOutput,
                            currencyOutput = output.currencyOutput,
                            boostOutput = output.boostOutput,
                            managerOutput = output.managerOutput,
                            outputAmount = output.outputAmount,
                            outputMaxAmount = output.outputMaxAmount
                        });
                    }
                }
            }
            return squashedOutputs;
        }

        public static string CustomToString(this List<Output> outputs)
        {
            string str = "";
            foreach (var output in outputs)
            {
                str += output.ToString() + "\n";
            }
            return str.TrimEnd('\n');

        }
    }
}