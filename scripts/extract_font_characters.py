#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
提取 UEBEA 解包的字体文件中的所有 Unicode 字符
"""

import re
from pathlib import Path

def extract_unicode_characters(input_file, output_file):
    """
    从字体资源文件中提取所有 Unicode 字符

    Args:
        input_file: 输入的 .txt 文件路径
        output_file: 输出的字符文件路径
    """
    unicode_values = []

    # 读取文件并提取所有 m_Unicode 值
    with open(input_file, 'r', encoding='utf-8') as f:
        for line in f:
            # 匹配 "unsigned int m_Unicode = <数字>" 的行
            match = re.search(r'unsigned int m_Unicode = (\d+)', line)
            if match:
                unicode_val = int(match.group(1))
                unicode_values.append(unicode_val)

    # 去重并排序
    unicode_values = sorted(set(unicode_values))

    # 转换为字符
    characters = []
    for val in unicode_values:
        try:
            char = chr(val)
            characters.append(char)
        except ValueError:
            print(f"警告: Unicode 值 {val} 无效")

    # 写入输出文件
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(''.join(characters))

    print(f"✓ 成功提取 {len(unicode_values)} 个不同的 Unicode 值")
    print(f"✓ 对应 {len(characters)} 个字符")
    print(f"✓ 已保存到: {output_file}")

    return characters

if __name__ == '__main__':
    # 配置文件路径
    input_file = Path(__file__).parent.parent / 'resources' / 'Grandstander-Bold-resources.assets-1200.txt'
    output_file = Path(__file__).parent.parent / 'resources' / 'Grandstander-Bold-characters.txt'

    if not input_file.exists():
        print(f"错误: 找不到输入文件 {input_file}")
        exit(1)

    extract_unicode_characters(str(input_file), str(output_file))
