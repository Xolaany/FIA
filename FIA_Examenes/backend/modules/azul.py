import cv2
import numpy as np


def destacar_azul(imagen):
    azulBajo = np.array([100, 100, 20], np.uint8)
    azulAlto = np.array([125, 255, 255], np.uint8)

    imgHSV = cv2.cvtColor(imagen, cv2.COLOR_BGR2HSV)

    maskAzul = cv2.inRange(imgHSV, azulBajo, azulAlto)

    resultadoAzul = cv2.bitwise_and(imagen, imagen, mask=maskAzul)

    return resultadoAzul