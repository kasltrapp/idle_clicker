using TMPro;
using UnityEngine;
using EasyIdleGame;

namespace InfluencerRise.UI
{
    /// <summary>
    /// Minimal Burnout readout for the top bar. The package's displayer set
    /// (CurrencyDisplayer, BusinessDisplayer, ManagerDisplayer, etc. - see
    /// 08-ui-and-displayers.md) only binds to Currency/Business/Manager/Boost/Upgrade
    /// definition assets. There is no built-in displayer for a CustomStat like Burnout,
    /// so this small label fills that one gap rather than a full BaseDisplayer subclass.
    /// </summary>
    public class BurnoutLabel : MonoBehaviour
    {
        [Tooltip("The Burnout CustomStat asset to read.")]
        [SerializeField] private CustomStat burnoutStat;

        [Tooltip("The text element this label writes into.")]
        [SerializeField] private TMP_Text text;

        /// <summary>Wires the fields this label needs. Used by editor/setup tooling as an
        /// alternative to Inspector assignment.</summary>
        public void Initialize(CustomStat stat, TMP_Text targetText)
        {
            burnoutStat = stat;
            text = targetText;
        }

        private void Update()
        {
            if (burnoutStat == null || text == null || PlayerStats.Instance == null) return;

            text.text = $"{PlayerStats.Instance.GetStat(burnoutStat)}/100";
        }
    }
}
