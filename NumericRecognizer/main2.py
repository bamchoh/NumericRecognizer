import easyocr
import glob
from pathlib import Path
import subprocess

with open('out.txt', 'w') as fp:
    pass

files = glob.glob(r'.\images\IMG_9134\img\*.png')
for i in range(len(files)):
    if i == 0:
        continue

    file1 = files[i-1]
    file2 = files[i]

    with open('out.txt', 'a') as fp:
        fp.write("{}\t{}\t".format(file1, file2))

    with open('out.txt', 'a') as fp:
        subprocess.run(['compare.exe', '-metric', 'SSIM', '-fuzz', '10%', file1, file2, "images\\IMG_9134\\diff\\{}.png".format(i)], encoding='utf-8', stderr=fp)

    with open('out.txt', 'a') as fp:
        fp.write("\n")
