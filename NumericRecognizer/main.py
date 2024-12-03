import easyocr
import glob
from pathlib import Path

reader = easyocr.Reader(['en']) # this needs to run only once to load the model into memory

files = glob.glob(r'.\bin\x64\Debug\images\IMG_9134\img\*.png')
for file in files:
    result = reader.readtext(file, allowlist='0123456789')
    if len(result) > 0 :
        row = result[0]
        threshold = row[-1]
        val = None
        if threshold >= 0.9 :
            val = row[-2]
        else :
            val = ""

        timestamp = Path(file).stem

        print("{}\t{}".format(timestamp, val))
