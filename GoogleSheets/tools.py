import json
import re


default_value = {
    'int': 0,
    'string': "",
    'float': 0,
    'int[]': [],
    'string[]': [],
    'bool': False,
    'float[]': [],
    'bool[]': [],
    'ref' : None,
    'ref[]': []
}

def isGenericType(value):
    match = re.match(r'(ref)<([\s\S]+?)>', value)
    result = False
    if (match):
        result = True
    return result


def getGenericType(type_, value):
    result = None
    isArray = type_.find("[]") != -1
    match = re.match(r'(ref)<([\s\S]+?)>', type_)
    genericType = match.group(2)
    
    if (isArray):
        values = list(json.loads(value))
        result = {"Type": genericType + "StaticData", "IsArray": isArray, "Values" : values}
    else:
        value = int(float(value.replace(",", ".")))
        result = {"Type": genericType + "StaticData", "IsArray": isArray, "Values": [value]}

    return result
   

def getNormalType(type_, value):
    result = None

    # возвращает дефолтное значение для типа, если получена пустая строка
    if value is None or value == '':
        if (type_ in default_value):
            return default_value[type_]
        else:
            return None

    if type_ == "int":
        result = int(float(value.replace(",", ".")))
    if type_ == "float":
        result = float(value.replace(",", "."))
    if type_ == "string":
        result = str(value)
    if type_ == "int[]":
        result = list(json.loads(value))
    if type_ == "string[]":
        result = list(json.loads(value))
    if type_ == "bool":
        result = str(value).lower() in ("true", "1")
    if type_ == "float[]":
        result = list(json.loads(value))
    if type_ == "bool[]":
        result = list(json.loads(value))
    if isGenericType(type_):
        result = getGenericType(type_, value)

    return result

default_value_script = {
    'int': "int",
    'string': "string",
    'float': "float",
    'int[]': "List<int>",
    'string[]': "List<string>",
    'bool': "bool",
    'float[]': "List<float>",
    'bool[]': "List<bool>",
}

def getTypeForScript(type_):
    thisType = ""

    if (type_ in default_value_script):
        thisType = default_value_script[type_]

    if isGenericType(type_):
        thisType = "StaticRef"

    return thisType;


def CreateDataScript(name, header, types):
    script = "//Autogenerated\n"
    script += "using System;\n"
    script += "using System.Collections;\n"
    script += "using System.Collections.Generic;\n"
    script += "\n"
    script += "[Serializable]\n"
    script += "public class " + name + "StaticData\n"
    script += "{\n"
    for i in range(len(header)):
        sType = getTypeForScript(types[i])
        if (sType is not None or sType != ''):
            script += "\t" + "public " + sType + " " + header[i] + ";\n"
    script += "}"
    return script


def CreateModelScript(name, dataName, sheetName):
    script = "//Autogenerated\n"
    script += "using System;\n"
    script += "using System.Collections;\n"
    script += "using System.Collections.Generic;\n"
    script += "\n"
    script += "[Serializable]\n"
    script += "public class " + name + "StaticModel\n"
    script += "{\n"
    script += "\t" + "public List<" + dataName + "StaticData" + "> " + sheetName + ";\n"
    script += "}"
    return script