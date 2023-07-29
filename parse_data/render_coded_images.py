import numpy as np
from PIL import Image
import os

# load the


dataset = "viz-calcium"


def render_single_attribute(attribute):
    # load the parsed numpy array for this attribute
    array = np.load("./parsed_data/%s/monitors/%s.npy" % (dataset, attribute))
    array = np.asarray(array, dtype=np.single)
    save_path = "./parsed_data/%s/rendered_images/%s" % (dataset, attribute)

    size = (2048, 2048)  # size of the images

    n_neurons = array.shape[0]
    n_steps = array.shape[1]

    steps_per_image = int((size[0] * size[1]) / n_neurons)
    n_total_images = int(np.math.ceil(n_steps / steps_per_image))

    print(steps_per_image)
    print(n_total_images)

    intervals = np.arange(steps_per_image, n_steps, steps_per_image)
    if intervals.shape[0] == 0 or intervals[-1] < n_steps:
        intervals = np.append(intervals, n_steps)

    interval_start = 0

    os.makedirs(save_path, exist_ok=True)

    for interval_end in intervals:
        image_array = np.zeros(size[0] * size[1] * 4, dtype=np.uint8)

        data = array[:, interval_start:interval_end].transpose().flatten()
        # data = np.linspace(0,int(image_array.shape[0]/4)-1,int(image_array.shape[0]/4),dtype=np.single)

        image_array[0:data.shape[0] * 4] = np.frombuffer(data.tobytes(), dtype=image_array.dtype)
        # image_array[0:data.shape[0]*4]=np.frombuffer(data.tobytes(),dtype=image_array.dtype)

        image_array = image_array.reshape((size[0], size[1], 4))
        im = Image.fromarray(image_array)
        im.save(save_path + "/%05d_%05d.png" % (interval_start, interval_end))
        # im.save(save_path+"/test.png")
        interval_start = interval_end
    print(intervals)
if __name__=="__main__":
    attributes = ["fired", "fired fraction", "activity", "dampening", "current calcium", "target calcium",
                  "synaptic input", "background input", "grown axons", "connected axons", "grown dendrites",
                  "connected dendrites"]

    for attribute in attributes:
        print(attribute)
        render_single_attribute(attribute)