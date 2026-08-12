using UnityEngine;

namespace EasyIdleGame.UI
{
    public class DemoManager : SimpleDemoManager
    {
        [CommentArea("Demo Page Manager", "Extends SimpleDemoManager with page navigation for the included demo scenes.", "Assign root page GameObjects in display order. Wire navigation buttons to OpenPage(page) or TogglePage(page).")]
        [SerializeField] private string _demoManagerComment;

        [Tooltip("Root page GameObjects controlled by this demo manager. The first page opens on Start; leave empty for single-page demos.")]
        public GameObject[] pages;

        private GameObject openedPage;

        new void Start()
        {
            base.Start();

            if (pages.Length > 0)
                OpenPage(pages[0]);

            foreach (var v in FindObjectsOfType<BaseDisplayer>(true))
            {
                v.Redraw();
            }
        }

        public void DisplayProfit_Boost(ProfitData data) => DisplayProfit(data, true, 0);

        public override void OpenBusinessDetail(Business business)
        {
            FindObjectOfType<BusinessDetailDisplayer>(true).OpenAndRedraw(business);
        }

        public void PrestigeGame() => PrestigeManager.Instance.TryPrestigeGame(out _);

        public void OnPrestige()
        {
            startProps.ApplyOuputs(1);
        }

        public void OpenPage(GameObject page_)
        {
            foreach (GameObject page in pages)
                page.SetActive(page == page_);

            openedPage = page_;
        }

        public void TogglePage(GameObject page_)
        {
            if (openedPage == page_) OpenPage(pages[0]);
            else OpenPage(page_);
        }
    }
}
