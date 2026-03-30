from PIL import Image

# 配置参数
INPUT_FILE = "KN Maiyuan.png"
OUTPUT_FILE = "KN Maiyuan.png"
# INPUT_FILE = "KN Maiyuan Daydream.png"
# OUTPUT_FILE = "KN Maiyuan Daydream.png"

# Alpha 值阈值（控制字体清晰度）
# 值越大，只保留越不透明的像素（字体越细）
# 值越小，保留更多半透明像素（字体越粗）
# 建议范围 100 ~ 200
ALPHA_THRESHOLD = 125

img = Image.open(INPUT_FILE).convert("RGBA")
pixels = img.load()

width, height = img.size

for y in range(height):
    for x in range(width):
        r, g, b, a = pixels[x, y]

        # 根据 Alpha 值判断
        if a >= ALPHA_THRESHOLD:
            # Alpha 足够高，保留为纯白不透明
            pixels[x, y] = (255, 255, 255, 255)
            # pixels[x, y] = (0, 0, 0, 255)
        else:
            # Alpha 太低，变成完全透明
            pixels[x, y] = (0, 0, 0, 0)

img.save(OUTPUT_FILE)
print(f"处理完成！已保存至 {OUTPUT_FILE}")
print(f"当前 Alpha 阈值: {ALPHA_THRESHOLD}")
print(f"如果字体太细，减小阈值（如100）；如果字体太粗，增大阈值（如200）")
