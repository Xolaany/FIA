import cv2
import numpy as np


def destacar_verde(imagen):
    verdeBajo = np.array([35, 100, 20], np.uint8)
    verdeAlto = np.array([85, 255, 255], np.uint8)

    imgHSV = cv2.cvtColor(imagen, cv2.COLOR_BGR2HSV)

    maskVerde = cv2.inRange(imgHSV, verdeBajo, verdeAlto)

    resultadoVerde = cv2.bitwise_and(imagen, imagen, mask=maskVerde)

    return resultadoVerde