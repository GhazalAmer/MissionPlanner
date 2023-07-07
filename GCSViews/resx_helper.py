import xml.etree.ElementTree as ET

new_tree = ET.parse('FlightPlannerNew.resx')
old_tree = ET.parse('FlightPlannerOld.resx')
new_root = new_tree.getroot()
old_root = old_tree.getroot()

oldV = {}
oldEl = {}
for el in old_root.findall('data'):
    name = el.get('name')
    value = el[0].text
    oldV[name] = value
    oldEl[name] = el

newNames = []
for el in new_root.findall('data'):
    newNames.append(el.get('name'))

for el in new_root.findall('data'):
    name = el.get('name')
    if name in oldV:
        el[0].text = oldV[name]

toAdd = [el for (name, el) in oldEl.items() if name is not in newNames]

new_root.extend(toAdd)
new_tree.write("newMreged.resx")