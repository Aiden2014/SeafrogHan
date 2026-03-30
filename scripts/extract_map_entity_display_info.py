import json
import csv
from pathlib import Path

# 定义路径
monobehaviour_dir = Path(r"d:\SteamLibrary\steamapps\common\Seafrog\tmp\MonoBehaviour")
resources_dir = Path(r"d:\projects\SeafrogHan\resources")
output_file = resources_dir / "map_entity_display_info.csv"
output_file_name = resources_dir / "map_enity_display_name.csv"

# 确保resources目录存在
resources_dir.mkdir(parents=True, exist_ok=True)

# 收集所有数据（用集合去重）
data = []
seen = set()
data_name = []
seen_name = set()

# 遍历MonoBehaviour文件夹，找所有MapEntityDisplayInfo_开头的json文件
for json_file in sorted(monobehaviour_dir.glob("MapEntityDisplayInfo_*.json")):
    try:
        with open(json_file, 'r', encoding='utf-8') as f:
            content = json.load(f)
            name = content.get("name", "").strip()
            description = content.get("description", "").strip()
            
            if name or description:
                # 第一列：name|||description格式
                first_col = f"{name}|||{description}"
                
                # 检查是否已经存在（去重）
                if first_col not in seen:
                    seen.add(first_col)
                    # 第二列：description内容
                    second_col = description
                    # 第三列：空（用于翻译）
                    third_col = ""
                    
                    data.append([first_col, second_col, third_col])
                    print(f"✓ Extracted: {json_file.name}")
                else:
                    print(f"⊘ Skipped (duplicate): {json_file.name}")
                
                # 也处理name到map_enity_display_name.csv
                if name and name not in seen_name:
                    seen_name.add(name)
                    data_name.append([name, name, ""])
                    print(f"✓ Name extracted: {name}")
    except Exception as e:
        print(f"✗ Error processing {json_file.name}: {e}")

# 写入CSV文件（不添加表头）
with open(output_file, 'w', newline='', encoding='utf-8-sig') as f:
    writer = csv.writer(f)
    writer.writerows(data)

# 写入name CSV文件（不添加表头）
with open(output_file_name, 'w', newline='', encoding='utf-8-sig') as f:
    writer = csv.writer(f)
    writer.writerows(data_name)

print(f"\n✓ 成功提取 {len(data)} 条记录到: {output_file}")
print(f"✓ 成功提取 {len(data_name)} 条name记录到: {output_file_name}")
