"""
从 resources/ 下所有 CSV 文件中提取包含已知字符集中的额外字符，
输出到 resources/cur_characters.txt，每行一个字符。
"""

import csv
import os
import sys

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_DIR = os.path.dirname(SCRIPT_DIR)
RESOURCES_DIR = os.path.join(PROJECT_DIR, "resources")
OUTPUT_FILE = os.path.join(RESOURCES_DIR, "cur_characters.txt")

# 已知字符集（characters.txt 第一行）
KNOWN_CHARS = set(
    ' !"#$%&\'()*+,-./0123456789:;<=>?@'
    'ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`'
    'abcdefghijklmnopqrstuvwxyz{|}~'
    '\xa0¡¢£¤¥¦§¨©ª«¬\xad®¯°±²³´µ¶·¸¹º»¼½¾¿'
    'ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß'
    'àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ'
    '\u2002\u2003\u2004\u2005\u2006'  # 各种空格
    '–—''‚""„†‡•…\u2009‰‹›⁄€™'
)

def main():
    extra_chars = set()

    for known_char in KNOWN_CHARS:
        extra_chars.add(known_char)

    for filename in os.listdir(RESOURCES_DIR):
        if not filename.lower().endswith('.csv'):
            continue
        filepath = os.path.join(RESOURCES_DIR, filename)
        with open(filepath, 'r', encoding='utf-8-sig') as f:
            reader = csv.reader(f)
            for row in reader:
                for cell in row:
                    for ch in cell:
                        extra_chars.add(ch)

    sorted_chars = sorted(extra_chars)

    with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
        for ch in sorted_chars:
            f.write(ch + '\n')

    print(f"共提取 {len(sorted_chars)} 个额外字符，已写入 {OUTPUT_FILE}")


if __name__ == '__main__':
    main()
