import numpy as np


def gamma_correction(imagen, gamma):
    imgNorm = imagen / 255.0

    imgGamma = np.power(imgNorm, gamma)

    return np.uint8(imgGamma * 255)