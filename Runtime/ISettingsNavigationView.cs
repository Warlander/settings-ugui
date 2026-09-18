using UnityEngine;

namespace Warlogic.Settings.Ugui
{
    public interface ISettingsNavigationView
    {
        void ScrollToTop();
        void Reveal(RectTransform target);
    }
}
