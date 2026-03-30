# SeafrogHan

> [English](README.md)

## 项目简介
SeafrogHan 是《Seafrog》的完整中文本地化模组。由于游戏对资产包（asset bundles）强制执行 CRC 校验，直接修改文件是不可行的。为了绕过这一限制，本项目依靠运行时补丁（runtime patching），直接在 Unity 的 TextMeshPro 组件中拦截并替换文本。本项目仓库包含 C# 注入代码、翻译映射字典以及生成像素级完美自定义字体所需的 Python 工具链。

## 技术亮点
- **责任链模式 (Chain of Responsibility Pattern)**：翻译匹配依赖于一系列处理器的组合。由于游戏具有复杂且动态的运行时文本语境，我们将字符串评估逻辑交给 [`Handler/`](Handler/) 目录中的一系列处理器按顺序进行处理，直到满足翻译条件为止。
- **运行时 Hook (Runtime Hooking)**：核心逻辑在恰当的时机拦截 Unity `TextMeshPro` 的渲染事件。这就像是一个运行时代理（Proxy），在不触碰原始游戏程序集的情况下，捕获并重新定义文本对象的基础操作。
- **像素完美的图像后处理 (Pixel-Perfect Image Post-Processing)**：为了还原原版游戏锯齿状的字体风格，需要消除自动抗锯齿。我们运行 Python 脚本（[`scripts/fix.py`](scripts/fix.py) 和 [`scripts/normalize.py`](scripts/normalize.py)）将模糊的 alpha 边缘转换为纯粹的二值化颜色。

## 涉及技术与库
- [BepInEx](https://github.com/BepInEx/BepInEx): Unity 模组的标准化注入框架，作为插件的入口点和模块加载器。
- [HarmonyLib](https://github.com/pardeike/Harmony): 用于在运行时修补、替换和装饰 .NET 方法的库。
- [UnityPy](https://github.com/K0lb3/UnityPy): 一个强大的 Python 库，用于通过代码提取和操作 Unity 资产包。
- [fnt2TMPro](https://github.com/napbla/fnt2TMPro): 一个专业工具，用于将 `.fnt` 文件转换为标准的 Unity TextMeshPro 资产。
- [ParaTranz](https://paratranz.cn/projects/18454): 协助管理翻译字符串数据的协作本地化平台。

## 自定义字体工作流
本项目使用了五种主要字体：*KN Maiyuan*、*KN Maiyuan BlueWhiteStroke*、*KN Maiyuan WhiteStroke*、*AaHuanMengKongJianXiangSuTi SDF* 和 *ZhanKuKuHei SDF*。生成锯齿边缘变体需要经过严格的流水线：

1. 使用 Python 提取实际需要显示的字符。
2. 使用 [Snowb](https://snowb.org/) 生成 `.fnt` 配置文件。对于 *KN Maiyuan BlueWhiteStroke* 这样的描边字体变体，需要细致的配置调整：设置 Font Size 为 `32`，Line Height 设为 `28`，Padding 和 Spacing 均设为 `3`，设置 Stroke Width 为 `2` （设成 Outer、并搭配 Round 的 Line Cap/Join），并且要采用 Fixed Size（Width / Height 均为 `2048`）来进行打包以支持多字符。
3. 运行我们的 Python 处理脚本，将输出的 `.png` 颜色二值化为清晰的锯齿边缘。
4. 使用 Unity Editor `2020.3.49f1c1`，通过 `fnt2TMPro` 将字体处理为 TextMeshPro 格式。
   - *设置要求*：Unity 图像设置需要 `RGBA 32 bit`、`Full Rect` 以及无压缩的 `Point` 过滤。Shader 必须设置为 `TextMeshPro/Sprite` 以保留原始图像中的颜色。间距和 bundle 尺寸（例如尺寸 `2048x2048`，padding 为 `5`）经过特别调整以保证清晰度。
5. 通过 Asset Bundle Browser 将字体打包，并放入游戏的 `Bepinex/plugins/resources` 目录中进行加载。

## 项目结构

```text
SeafrogHan/
├── Handler/                        # 责任链模式的翻译处理器模块
│   ├── ITranslationHandler.cs      # 定义处理器契约的接口
│   ├── TranslationContext.cs       # 在责任链中传递的评估状态
│   ├── TranslationHandlerBase.cs   # 共享处理器逻辑的基类
│   └── [...Other Handlers].cs      # 特定的匹配逻辑 (例如: MapEntityHandler)
├── resources/                      # 语言数据和参考文件
│   ├── cur_characters.txt          # 导出的用于生成字体的字符列表
│   └── *.csv                       # 从 ParaTranz 拉取的原始翻译字典
├── scripts/                        # 自动化打包与处理管道
│   ├── extract_*.py                # 从 Unity 资产中提取文本和元数据的脚本
│   ├── fix.py                      # 移除基础字体的抗锯齿
│   └── normalize.py                # 对带描边字体的颜色输出进行二值化处理
├── LICENSE                         # 项目开源协议 (GNU LGPLv2.1)
├── MissingStringTracker.cs         # 工具组件：用于在测试期间记录未翻译的文本
├── Plugin.cs                       # BepInEx 入口点和 Harmony 补丁协调器
├── ResourceLoader.cs               # 自定义资产包加载器，用于注入外部字体
├── SeafrogHan.csproj               # .NET 6.0 构建配置
└── TranslationManager.cs           # 核心模块：初始化和查询 CSV 字典
```

## 开源协议
本项目采用 [GNU Lesser General Public License v2.1](LICENSE) 协议进行开源。

> 注意：本项目仅供学习交流使用。请支持正版游戏。