using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Warlogic.Settings.Ugui.Tests.Editor
{
    public class SettingsWindowPresenterTests
    {
        private sealed class TestView : ISettingsWindowView, ISettingsNavigationView
        {
            public event Action SaveClicked { add { } remove { } }
            public event Action DiscardClicked { add { } remove { } }

            public RectTransform TabButtonRoot { get; }
            public RectTransform ContentRoot { get; }
            public RectTransform RevealedTarget { get; private set; }
            public int RevealCount { get; private set; }
            public int ScrollToTopCount { get; private set; }

            public TestView()
            {
                TabButtonRoot = new GameObject("Tabs", typeof(RectTransform)).GetComponent<RectTransform>();
                ContentRoot = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            }

            public void SetSaveInteractable(bool interactable) { }

            public void SetDiscardInteractable(bool interactable) { }

            public void ScrollToTop()
            {
                ScrollToTopCount++;
            }

            public void Reveal(RectTransform target)
            {
                RevealedTarget = target;
                RevealCount++;
            }

            public void Destroy()
            {
                UnityEngine.Object.DestroyImmediate(TabButtonRoot.gameObject);
                UnityEngine.Object.DestroyImmediate(ContentRoot.gameObject);
            }
        }

        private sealed class GroupedBoolSetting : Setting<bool>, IGroupedSetting
        {
            public string Group { get; }

            public GroupedBoolSetting(string key, string group)
                : base(key, key, false)
            {
                Group = group;
            }

            protected override string Serialize(bool value)
            {
                return value.ToString();
            }

            protected override bool Deserialize(string raw)
            {
                return bool.Parse(raw);
            }
        }

        private sealed class TestWidget : ISettingWidget
        {
            public event Action ValueEdited { add { } remove { } }

            public RectTransform Root { get; }

            public TestWidget(RectTransform root)
            {
                Root = root;
            }

            public void Refresh() { }
        }

        private sealed class TestWidgetFactory : ISettingWidgetFactory
        {
            public ISettingWidget Create(ISetting setting, Transform parent)
            {
                var root = new GameObject(setting.Key, typeof(RectTransform));
                root.transform.SetParent(parent, false);
                return new TestWidget(root.GetComponent<RectTransform>());
            }
        }

        private GameObject _catalogObject;
        private SettingsWidgetCatalog _catalog;
        private TestView _view;

        [SetUp]
        public void SetUp()
        {
            _catalogObject = new GameObject("Catalog");
            _catalog = _catalogObject.AddComponent<SettingsWidgetCatalog>();
            _view = new TestView();

            var serializedCatalog = new SerializedObject(_catalog);
            serializedCatalog.FindProperty("tabButtonPrefab").objectReferenceValue = LoadPrefab<SettingsTabButton>("SettingsTabButton");
            serializedCatalog.FindProperty("groupHeaderPrefab").objectReferenceValue = LoadPrefab<SettingsGroupHeader>("SettingsGroupHeader");
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            _catalog.RegisterFactory(typeof(GroupedBoolSetting), new TestWidgetFactory());
        }

        [TearDown]
        public void TearDown()
        {
            _view.Destroy();
            UnityEngine.Object.DestroyImmediate(_catalogObject);
        }

        [Test]
        public void InitialSettingDestination_SelectsOwningTabAndRevealsGroupHeader()
        {
            var registry = new SettingsRegistry();
            registry.AddTab("general", "General");
            registry.AddTab("keybinds", "Keybinds").Add(new GroupedBoolSetting("keybind.symmetry", "Symmetry"));

            var presenter = new SettingsWindowPresenter(registry, _catalog, _view, _view,
                SettingsDestination.ForSetting("keybind.symmetry"));

            Assert.AreEqual("keybinds", presenter.ActiveTabId);
            Assert.AreEqual(1, _view.RevealCount);
            Assert.IsNotNull(_view.RevealedTarget.GetComponent<SettingsGroupHeader>());
        }

        [Test]
        public void TryNavigate_ActiveTab_RevealsAgain()
        {
            var registry = new SettingsRegistry();
            registry.AddTab("keybinds", "Keybinds").Add(new GroupedBoolSetting("keybind.symmetry", "Symmetry"));
            var presenter = new SettingsWindowPresenter(registry, _catalog, _view, _view,
                SettingsDestination.ForSetting("keybind.symmetry"));

            bool navigated = presenter.TryNavigate(SettingsDestination.ForSetting("keybind.symmetry"));

            Assert.IsTrue(navigated);
            Assert.AreEqual(2, _view.RevealCount);
        }

        [Test]
        public void TryNavigate_MissingDestination_DoesNotChangeActiveTab()
        {
            var registry = new SettingsRegistry();
            registry.AddTab("general", "General");
            var presenter = new SettingsWindowPresenter(registry, _catalog, _view, _view);

            bool navigated = presenter.TryNavigate(SettingsDestination.ForSetting("missing"));

            Assert.IsFalse(navigated);
            Assert.AreEqual("general", presenter.ActiveTabId);
            Assert.AreEqual(0, _view.RevealCount);
        }

        [Test]
        public void TryNavigate_TabDestination_ScrollsToTop()
        {
            var registry = new SettingsRegistry();
            registry.AddTab("general", "General");
            var presenter = new SettingsWindowPresenter(registry, _catalog, _view, _view);

            bool navigated = presenter.TryNavigate(SettingsDestination.ForTab("general"));

            Assert.IsTrue(navigated);
            Assert.AreEqual(1, _view.ScrollToTopCount);
        }

        private static T LoadPrefab<T>(string name) where T : Component
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(
                $"Packages/com.warlogic.settings.ugui/Runtime/Prefabs/{name}.prefab").GetComponent<T>();
        }
    }
}
