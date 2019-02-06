# vscindex.py -- vlabel script
# This script is invoked by vlabel with the following params:
#  - .vl.json filename
#  - numeric threshold value
import json
import sys
import os
import scenedetect as sc

if len(sys.argv)!=5:
    print("Incorrect # of arguments provided. 4 expected.")
    exit(1)

if not os.path.exists(sys.argv[1]):
    print("vl.json file does not exist. Cannot proceed.")
    exit(2)

fn = sys.argv[1]
thr = int(sys.argv[2])
minlen = int(sys.argv[3])
maxlen = int(sys.argv[4])
categ_name = "Scenes"

print("Analyzig file {} with params:".format(fn))
print(" * Threshold = {}".format(thr))
print(" * Min length = {}".format(minlen))
print(" * Max length = {}".format(maxlen))

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
    l = scene[1].get_frames()-scene[0].get_frames()
    if (l>=minlen and l<=maxlen):
        scenez.append({ "Category" : categ_name, "StartFrame" : scene[0].get_frames()+1, "EndFrame" : scene[1].get_frames()})

VL["Intervals"][categ_name] = scenez

print(" * # of scenes: {}".format(len(scene_list)))
print(" * av scene length:{}".format(VL["VideoFrames"]/len(scene_list)))
print(" * # of scenes taken:{}".format(len(scenez)))

with open(fn,'w') as json_file:
    json.dump(VL,json_file)

input("Press Enter to finish")
exit(0)