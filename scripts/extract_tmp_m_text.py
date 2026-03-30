from collections import defaultdict

import UnityPy
import csv
import os

# 路径设置
src = 'D:\\SteamLibrary\\steamapps\\common\\Seafrog\\Seafrog_Data'
csv_file = 'D:\\projects\\SeafrogHan\\resources\\tmp_m_text.csv'

TARGET_TYPES = ['TextMeshProUGUI', 'TextMeshPro']

def has_m_text_data(data):
    """检查数据中是否包含 m_text 字段"""
    return isinstance(data, dict) and 'm_text' in data

def get_gameobject_path(mono_obj):
    """
    通过 MonoBehaviour 向上追溯，获取 GameObject 的完整层级路径
    """
    try:
        # 1. 获取挂载的 GameObject
        go_node = mono_obj.m_GameObject.read()
        path = go_node.m_Name
        
        # 2. 获取 GameObject 的 Transform 组件
        # UnityPy 提供了 m_Transform 快捷属性来获取 Transform 或 RectTransform
        if not go_node.m_Transform:
            return path
            
        transform_node = go_node.m_Transform.read()
        
        # 3. 向上遍历父节点
        # m_Father.path_id == 0 表示没有父节点了（到达根节点）
        while transform_node.m_Father.path_id != 0:
            transform_node = transform_node.m_Father.read()
            father_go = transform_node.m_GameObject.read()
            path = f"{father_go.m_Name}/{path}"
            
        return path
    except Exception as e:
        # 如果跨 Bundle 引用断裂，可能无法读取，返回已知部分
        return f"UnknownParent/{path}" if 'path' in locals() else "UnknownPath"

rows = []
count = 0
key_map = defaultdict(list)

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
                        # 读取基础对象以获取指针 (用于获取脚本名和 GameObject)
                        mono_behavior = obj.read()
                        
                        # 检查是否有脚本引用
                        if not mono_behavior.m_Script:
                            print(f"  [SKIP] No script on MonoBehaviour {obj.path_id}")
                            continue
                            
                        script_name = mono_behavior.m_Script.read().m_ClassName
                        print(f"  [MonoBehaviour {obj.path_id}] Script: {script_name}")
                        
                        # 过滤 TMP 组件
                        if script_name in TARGET_TYPES:
                            # 读取具体数据字典
                            data = obj.read_typetree()

                            if has_m_text_data(data):
                                text_content = data['m_text']
                                print(f"  [Found] m_text in {script_name}: {repr(text_content[:50])}")
                                
                                # 跳过空文本
                                if not text_content.strip():
                                    print(f"  [SKIP] Empty/whitespace m_text")
                                    continue
                                
                                # 获取完整层级路径！
                                try:
                                    hierarchy_path = get_gameobject_path(mono_behavior)
                                except Exception as path_err:
                                    print(f"  [ERROR] Failed to get hierarchy path: {path_err}")
                                    hierarchy_path = f"UnknownPath|||{obj.path_id}"

                                print(f"  Found text in: {hierarchy_path}")
                                
                                # 格式: hierarchy_path|||m_text
                                key = f"{hierarchy_path}|||{text_content}"
                                if key in key_map:
                                    print(f"  ⚠️ Duplicate found for key: {key}")
                                    continue
                                key_map[key].append((hierarchy_path, text_content))
                                
                                # 新增一列：Hierarchy Path (语境路径)
                                rows.append([key, text_content, ''])
                                count += 1
                            else:
                                print(f"  [SKIP] No m_text in {script_name}")
                        else:
                            print(f"  [SKIP] Not TMP component: {script_name}")
                                
                    except Exception as e:
                        # 记录所有异常
                        import traceback
                        print(f"  [ERROR] Exception reading MonoBehaviour {obj.path_id}: {e}")
                        traceback.print_exc()

# 导出为 CSV
try:
    with open(csv_file, 'w', newline='', encoding='utf-8-sig') as f:
        writer = csv.writer(f)
        # 写入表头 (强烈建议保留表头，方便查看)
        # writer.writerow(['Context Path (语境路径)', 'Original', 'Translated'])
        # 写入数据行
        writer.writerows(rows)
    print(f"\n✓ CSV exported: {csv_file}")
    print(f"Total TMP texts found: {count}")
except Exception as e:
    print(f"Error writing CSV: {e}")