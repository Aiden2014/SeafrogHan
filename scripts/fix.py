from PIL import Image

# ================= 配置区 =================
INPUT_FILE = "KN Maiyuan WhiteStroke.png"  # 输入图片

# 图片格式（输入输出保持一致）
# FORMAT = "BLUE_WHITE"  # "BLUE_WHITE" 或 "WHITE_BLACK"
FORMAT = "WHITE_BLACK"

# 标准 RGB 定义（不要修改这些常量）
RGB_WHITE = (255, 255, 255)
RGB_BLACK = (0, 0, 0)
RGB_BLUE  = (70, 139, 248)

# 根据FORMAT设置目标颜色
COLORS = {
    "BLUE_WHITE": {
        "fill": RGB_BLUE,      # 文字颜色
        "stroke": RGB_WHITE    # 描边颜色
    },
    "WHITE_BLACK": {
        "fill": RGB_WHITE,     # 文字颜色
        "stroke": RGB_BLACK    # 描边颜色
    }
}

TARGET_FILL = COLORS[FORMAT]["fill"]
TARGET_STROKE = COLORS[FORMAT]["stroke"]

# 亮度阈值
BRIGHTNESS_THRESHOLD = 170 
ALPHA_THRESHOLD = 30
# ==========================================

img = Image.open(INPUT_FILE).convert("RGBA")
pixels = img.load()
width, height = img.size

for y in range(height):
    for x in range(width):
        r, g, b, a = pixels[x, y]

        if a < ALPHA_THRESHOLD:
            pixels[x, y] = (0, 0, 0, 0)
            continue

        # 计算亮度
        brightness = r * 0.299 + g * 0.587 + b * 0.114

        # BLUE_WHITE: 深色是文字，浅色是描边
        # WHITE_BLACK: 浅色是文字，深色是描边（亮度判断需要反过来）
        if FORMAT == "BLUE_WHITE":
            is_fill = (brightness < BRIGHTNESS_THRESHOLD)
        else:  # WHITE_BLACK
            is_fill = (brightness >= BRIGHTNESS_THRESHOLD)

        if is_fill:
            # 填充文字颜色
            pixels[x, y] = (*TARGET_FILL, 255)
        else:
            # 填充描边颜色
            pixels[x, y] = (*TARGET_STROKE, 255)

output_name = f"{INPUT_FILE}"
img.save(output_name)
print(f"处理完成！已生成标准预览图：{output_name}")