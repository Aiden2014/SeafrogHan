# SeafrogHan

> [中文文档](README.zh.md)

## Description
SeafrogHan is a complete Chinese localization mod for Seafrog. The game enforces CRC checks on its asset bundles, making direct file modification impossible. To bypass this limitation, the project relies on runtime patching to intercept and replace text directly within Unity's TextMeshPro components. The repository includes the C# injection code, the translation maps, and the Python toolchain required to generate pixel-perfect custom fonts. 

## Interesting Techniques
- **Chain of Responsibility Pattern**: Translation matching relies on a series of distinct handlers. Because the game features complex and dynamic runtime text contexts, we pass string evaluation through a structured sequence in the [`Handler/`](Handler/) directory until a translation condition is successfully met.
- **Runtime Hooking**: The core logic intercepts Unity `TextMeshPro` rendering events just in time. This functions essentially like a runtime proxy, trapping and redefining fundamental operations on text objects without ever touching the original game assemblies.
- **Pixel-Perfect Image Post-Processing**: Replicating the vanilla game's jagged font style requires stripping out automatic anti-aliasing. We run Python scripts ([`scripts/fix.py`](scripts/fix.py) and [`scripts/normalize.py`](scripts/normalize.py)) to convert blurred alpha edges into pure binary colors.

## Technologies & Libraries
- [BepInEx](https://github.com/BepInEx/BepInEx): The standard injection framework for Unity modding. Serves as the plugin entry point and module loader.
- [HarmonyLib](https://github.com/pardeike/Harmony): A library for patching, replacing, and decorating .NET methods during runtime.
- [UnityPy](https://github.com/K0lb3/UnityPy): A powerful Python library for extracting and manipulating Unity asset bundles programmatically. 
- [fnt2TMPro](https://github.com/napbla/fnt2TMPro): A specialized utility for turning `.fnt` files into standard Unity TextMeshPro assets. 
- [ParaTranz](https://paratranz.cn/projects/18454): The collaborative localization platform managing the translated string data. 

## Custom Font Workflow
We use five primary typefaces: *KN Maiyuan*, *KN Maiyuan BlueWhiteStroke*, *KN Maiyuan WhiteStroke*, *AaHuanMengKongJianXiangSuTi SDF*, and *ZhanKuKuHei SDF*. Generating the jagged-edge variants requires a strict pipeline:

1. We extract the required display characters using Python.
2. We generate the `.fnt` configuration using [Snowb](https://snowb.org/). For stroke variants like *KN Maiyuan BlueWhiteStroke*, must set properties accurately: Font Size `32`, Line Height `28`, Padding `3`, Spacing `3`, Stroke Width `2` (outer with Round cap/join), and adjusting fixed canvas size (Width/Height: `2048x2048`).
3. We run our Python processing scripts to binarize the `.png` output colors into jagged, clear edges. 
4. Using Unity Editor `2020.3.49f1c1`, we process the fonts into TextMeshPro format via `fnt2TMPro`. 
   - *Requirements*: The Unity image settings demand `RGBA 32 bit`, `Full Rect`, and a `Point` filter without compression. The shaders must be set to `TextMeshPro/Sprite` to preserve the original image colors. Both padding and bundle sizing (e.g., `2048x2048`, padding `5`) are adjusted to maintain clarity across device resolutions. 
5. We package the fonts into bundles via the Asset Bundle Browser and load them out of the game's `Bepinex/plugins/resources` directory. 

## Project Structure

```text
SeafrogHan/
├── Handler/                        # Chain of Responsibility translation modules
│   ├── ITranslationHandler.cs      # Interface defining the handler contract
│   ├── TranslationContext.cs       # Evaluation state passed through the chain
│   ├── TranslationHandlerBase.cs   # Base class for shared handler logic
│   └── [...Other Handlers].cs      # Specific matching logic (e.g., MapEntityHandler)
├── resources/                      # Language data and reference files
│   ├── cur_characters.txt          # Exported character list for font mapping
│   └── *.csv                       # Raw translation dictionaries pulled from ParaTranz
├── scripts/                        # Automation pipeline for packaging and manipulation
│   ├── extract_*.py                # Scripts to scrape text and metadata from Unity assets
│   ├── fix.py                      # Removes anti-aliasing from base font imagery
│   └── normalize.py                # Binarizes color output for the stroke font variants
├── LICENSE                         # Project license (GNU LGPLv2.1)
├── MissingStringTracker.cs         # Utility component for logging untranslated text during testing
├── Plugin.cs                       # BepInEx entry point and Harmony patching orchestrator
├── ResourceLoader.cs               # Custom asset bundle loader for injecting external fonts
├── SeafrogHan.csproj               # .NET 6.0 build configuration
└── TranslationManager.cs           # Core module initializing and querying CSV dictionaries
```

## License
This project is licensed under the [GNU Lesser General Public License v2.1](LICENSE).

> Note: This project is for educational purposes only. Please support the original game.
