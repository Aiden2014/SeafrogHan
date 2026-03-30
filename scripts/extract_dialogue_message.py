import UnityPy
import csv
import os

# 路径设置
src = 'D:\\SteamLibrary\\steamapps\\common\\Seafrog\\Seafrog_Data'
dst = 'D:\\projects\\SeafrogHan\\resources\\dialogue_message'
csv_file = 'D:\\projects\\SeafrogHan\\resources\\dialogue_message.csv'

# 确保输出目录存在
os.makedirs(dst, exist_ok=True)

def has_dialogue_data(data):
    """检查数据中是否包含对话相关的字段"""
    if isinstance(data, dict):
        keys = data.keys()
        return any(key in keys for key in [
            'DialogueEntry', 'dialogueEntry', 'entries', 'dialogue',
            'messages', 'message', 'characterDialogue', 'dialogueText'
        ])
    return False

rows = []
count = 0

for root, dirs, files in os.walk(src):
    for name in files:
        if name.endswith(('.assets', '.bundle')):
            path = os.path.join(root, name)
            print(f"Processing: {path}")
            try:
                env = UnityPy.load(path)
            except Exception as e:
                print(f"  Error loading: {e}")
                continue

            for obj in env.objects:
                if obj.type.name == "MonoBehaviour":
                    try:
                        data = obj.read_typetree()

                        # 检查是否包含对话数据
                        if has_dialogue_data(data) and 'entries' in data:
                            m_name = data.get('m_Name', '')
                            # 提取 message 字段
                            for entry in data['entries']:
                                if isinstance(entry, dict) and 'message' in entry:
                                    message = entry['message']
                                    # 使用 ||| 作为分隔符，避免与负数混淆
                                    # 格式: bundle|||path_id|||m_Name|||message
                                    key = f"{name}|||{obj.path_id}|||{m_name}|||{message}"
                                    # 第二列：原始 message
                                    # 第三列：空（用于翻译）
                                    rows.append([key, message, ''])
                                    count += 1

                            print(f"  ✓ Found {len([e for e in data['entries'] if isinstance(e, dict) and 'message' in e])} messages")
                    except Exception as e:
                        # 跳过无法读取的对象
                        pass

# 导出为 CSV
try:
    with open(csv_file, 'w', newline='', encoding='utf-8-sig') as f:
        writer = csv.writer(f)
        # 写入表头
        writer.writerow(['Key (bundle|||path_id|||m_Name|||message)', 'Original', 'Translated'])
        # 写入数据行
        writer.writerows(rows)
    print(f"\n✓ CSV exported: {csv_file}")
    print(f"Total messages: {count}")
except Exception as e:
    print(f"Error writing CSV: {e}")