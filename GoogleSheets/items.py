import json
import sys
from py_linq import Enumerable
from sheets import sheets
import tools


def main(out_path: str = "", out_path_scripts: str = ""):
    sheet = sheets()

    sheetName = "Items"
    dataClassName = "Item"

    itemHeader, itemTypes = sheet.GetHeaderAndTypes(sheetName)
    items = sheet.GetTableDic(sheetName, itemHeader, itemTypes)

    with open(out_path + sheetName +'.json', 'wt', encoding='utf-8') as file:
        to = {sheetName: list(items[id] for id in sorted(items.keys()))}
        json.dump(to, file, ensure_ascii=False, indent=2, sort_keys=True)

    with open(out_path_scripts + dataClassName +'StaticData.cs', 'wt', encoding='utf-8') as file:
        to = tools.CreateDataScript(dataClassName, itemHeader, itemTypes)
        file.write(to)
    
    with open(out_path_scripts + sheetName +'StaticModel.cs', 'wt', encoding='utf-8') as file:
        to = tools.CreateModelScript(sheetName, dataClassName, sheetName)
        file.write(to)


if __name__ == '__main__':
    path = sys.argv[1] if len(sys.argv) > 1 else ""
    pathScripts = sys.argv[2] if len(sys.argv) > 2 else ""
    main(path, pathScripts)
