import re
import json


def isGenericType(value):
    match = re.match(r'(ref)<([\s\S]+?)>', value)
    print(match.group)
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
        result = {"Type": genericType , "IsArray": isArray, "Values" : values}
    else:
        value = int(float(value.replace(",", ".")))
        result = {"Type": genericType, "IsArray": isArray, "Value": value}

    return result

value = 'ref<Items>'
gen = getGenericType(value, "1")
print(gen)