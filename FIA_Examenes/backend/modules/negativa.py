import cv2


def negativa(imagen):
    imagenRGB = cv2.cvtColor(imagen, cv2.COLOR_BGR2RGB)

    R = imagenRGB[:, :, 0]
    G = imagenRGB[:, :, 1]
    B = imagenRGB[:, :, 2]

    R_Neg = 255 - R
    G_Neg = 255 - G
    B_Neg = 255 - B

    imgRGBNeg = cv2.merge((R_Neg, G_Neg, B_Neg))

    imgRGBNeg = cv2.cvtColor(imgRGBNeg, cv2.COLOR_RGB2BGR)

    return imgRGBNeg