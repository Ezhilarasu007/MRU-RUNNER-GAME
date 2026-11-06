"""
MRU INFINITE RUNNER 3D — AUTOMATED ZIP BUNDLE GENERATOR
"""

import os
import zipfile

PROJECT_ROOT = os.path.dirname(os.path.abspath(__file__))
OUTPUT_DIR = os.path.join(PROJECT_ROOT, "Build", "Release_Packages")
os.makedirs(OUTPUT_DIR, exist_ok=True)

def create_zip(zip_name, file_list):
    zip_path = os.path.join(OUTPUT_DIR, zip_name)
    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
        for file_path, arcname in file_list:
            if os.path.exists(file_path):
                zipf.write(file_path, arcname)
    print(f"SUCCESS: Created package -> {zip_path}")

print("=== PACKAGING MRU INFINITE RUNNER 3D ===")

# 1. Web Portal Zip
web_files = []
for root, _, files in os.walk(PROJECT_ROOT):
    if "Build" in root or ".git" in root: continue
    for f in files:
        if f.endswith(('.html', '.css', '.js')) or 'www' in root:
            full_p = os.path.join(root, f)
            rel_p = os.path.relpath(full_p, PROJECT_ROOT)
            web_files.append((full_p, os.path.join("MRU_Web_Portal", rel_p)))

create_zip("MRU_Runner_Web_Portal_v1.0.0.zip", web_files)

# 2. Complete Project Source Scaffold Zip
source_files = []
ignore_dirs = {'.git', 'Library', 'Temp', 'obj', 'Logs', 'Build'}
for root, dirs, files in os.walk(PROJECT_ROOT):
    dirs[:] = [d for d in dirs if d not in ignore_dirs]
    for f in files:
        full_p = os.path.join(root, f)
        rel_p = os.path.relpath(full_p, PROJECT_ROOT)
        source_files.append((full_p, os.path.join("MRU_Runner_Game_1.0.0.0", rel_p)))

create_zip("MRU_Runner_Full_Project_v1.0.0.zip", source_files)
print("Done!")
