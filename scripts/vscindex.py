# vscindex.py -- vlabel script
# This script is invoked by vlabel with the following params:
#  - .vl.json filename
#  - numeric threshold value
import json
import sys
import os
import scenedetect as sc

if len(sys.argv)!=4:
    print("Incorrect # of arguments provided. 3 expected.")
    exit(1)

if not os.path.exists(sys.argv[1]):
    print("vl.json file does not exist. Cannot proceed.")
    exit(2)

fn = sys.argv[1]
thr = int(sys.argv[2])
minlen = int(sys.argv[3])
categ_name = "Scenes"

#fn = 'd:\\video\\heedbook_5.vl.json'
#thr = 32
#minlen = 64

with open(fn) as json_file:
    VL = json.load(json_file)

vfile = VL["Filename"]

print("Processing {}".format(vfile))

video_manager = sc.VideoManager([vfile])
stats_manager = sc.stats_manager.StatsManager()
scene_manager = sc.SceneManager(stats_manager)
scene_manager.add_detector(sc.ContentDetector(threshold=thr))
base_timecode = video_manager.get_base_timecode()

video_manager.set_downscale_factor()

video_manager.start()

scene_manager.detect_scenes(frame_source=video_manager)

scene_list = scene_manager.get_scene_list(base_timecode)

if categ_name not in VL["Categories"]:
    VL["Categories"].append(categ_name)

scenez = []

for i, scene in enumerate(scene_list):
    if (scene[1].get_frames()-scene[0].get_frames()>=minlen):
        scenez.append({ "Category" : categ_name, "StartFrame" : scene[0].get_frames(), "EndFrame" : scene[1].get_frames()-1})

VL["Intervals"][categ_name] = scenez

print(" * # of scenes: {}".format(len(scene_list)))
print(" * av scene length:{}".format(VL["VideoFrames"]/len(scene_list)))
print(" * # of scenes taken:{}".format(len(scenez)))

with open(fn,'w') as json_file:
    json.dump(VL,json_file)

input("Press Enter to finish")
exit(0)