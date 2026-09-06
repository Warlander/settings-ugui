# Warlogic Settings uGUI

uGUI widgets and a unified settings window built on the Warlogic.Settings core registry. Ships a widget factory registry (`SettingsWidgetCatalog`), widget prefabs (slider, toggle, dropdown, keybind row, group header, tab button), and a tabbed settings window view + presenter with per-tab lazy build and a shared Save/Discard footer.

# Installation

## Via Git URL

Open **Window → Package Manager**, click **+**, and choose **Add package from git URL**.

To install the latest version:
```
https://github.com/Warlander/settings-ugui.git
```

To install a specific release, append the tag:
```
https://github.com/Warlander/settings-ugui.git#1.0.0
```

## Via Warlogic registry

Add the scoped registry to `Packages/manifest.json`, then install **Warlogic Settings uGUI** from the Package Manager:

```json
{
  "name": "Warlogic",
  "url": "https://upm.maciejcyranowicz.com",
  "scopes": ["com.warlogic"]
}
```
