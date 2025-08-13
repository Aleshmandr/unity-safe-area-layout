# Safe Area Layout

Unity GUI [layout group](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/UIAutoLayout.html#layout-groups)
that makes children respect the [Safe Area](https://docs.unity3d.com/ScriptReference/Screen-safeArea.html).
It drives direct children's anchors while in Play Mode and supports [`LayoutElement.ignoreLayout`](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.UI.ILayoutIgnorer.html).

![Demonstration video](Extras~/demo.gif)

## Features
- Fully integrated with Unity GUI's layout system: only rebuilds the layout when needed, no `Update` method or coroutines attached
- Does not resize it's own `RectTransform`, so it can be used in objects with a `Canvas` component directly
- Does not demand a full screen `RectTransform`: the script detects where your rect overlaps with the Safe Area and updates accordingly
- Ignore children using a `IgnoreSafeArea` component or `LayoutElement` with `Ignore Layout` marked as true.
  Useful for background images, for example.
- Preview Safe Area adjustments in Editor using any of the Preview Modes in `SafeAreaLayoutGroup`'s inspector while hovering the `Hover to Preview Layout` button or while in Play Mode.
  All Preview Modes support both portrait and landscape resolutions.
  `Screen.safeArea` Preview Mode is only applied when using Unity's [Device Simulator](https://docs.unity3d.com/Manual/device-simulator-introduction.html) (in Unity 2020 and older, available as an [UPM package](https://docs.unity3d.com/Packages/com.unity.device-simulator@latest/index.html))
- Only affects canvases in either `Screen Space - Overlay` or `Screen Space - Camera` modes, so `World Space` canvases are ignored
- Project-wide margin multipliers configurable in Project Settings at `Edit → Project Settings → Safe Area Layout`
- Per-group override support by assigning a custom `SafeAreaLayoutConfig` to a `SafeAreaLayoutGroup`'s `OverrideGloblalLayoutConfig` field
- Extensible additional margins via Margins Providers: plug in components deriving from `MarginsProviderBase` in the `Additional Margins` list to reserve extra space (e.g., banner ads)

## Installing

- Install via [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html)
using the following git URL:
  ```
  https://github.com/Aleshmandr/unity-safe-area-layout.git
  ```

- Clone this repository directly to your `Packages` folder or anywhere inside your project's `Assets`.


## Sample
A sample scene is available at  [Samples~/SimpleSample](Samples~/SimpleSample).


## How to use
1. Add the [SafeAreaLayoutGroup](Runtime/SafeAreaLayoutGroup.cs) script anywhere in your UI hierarchy, even objects with `Canvas` components are supported.
   Direct children will have their anchors driven while the script is enabled.
2. (optional) Configure global margins in Project Settings: go to `Edit → Project Settings → Safe Area Layout` and adjust Top/Bottom/Left/Right margin multipliers.
3. (optional) Create a custom `SafeAreaLayoutConfig` asset and assign it to the `OverrideGloblalLayoutConfig` field on a specific `SafeAreaLayoutGroup` to override the global settings for that group.
4. (optional) Make specific children be ignored by the layout group by adding the `IgnoreSafeArea` component to them.
   Alternatively, use `LayoutElement` components with the `Ignore Layout` flag marked as true.
5. (optional) Add additional margins via Margins Providers: assign one or more components implementing `MarginsProviderBase` to the `SafeAreaLayoutGroup`'s `Additional Margins` list to provide extra top/bottom/left/right spacing.
    This is useful for reserving space for elements like a persistent banner ad (you can create a custom provider, e.g. `AdBannerMarginsProvider`, that reports the banner height).
6. (optional) Use one of the Preview Modes while in editor to preview the adjustments.
    Preview is applied on Play Mode and while hovering the `Hover to Preview Layout` button in the `SafeAreaLayoutGroup`'s inspector.
7. Play the game
8. Enjoy 🍾
