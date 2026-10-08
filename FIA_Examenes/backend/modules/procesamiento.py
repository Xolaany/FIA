import cv2
import numpy as np

# Separar en Capas (R, G, B, C, M, Y)
def separar_capas(imagen_bgr):
    img_rgb = cv2.cvtColor(imagen_bgr, cv2.COLOR_BGR2RGB)
    R, G, B = img_rgb[:, :, 0], img_rgb[:, :, 1], img_rgb[:, :, 2]
    cero = np.zeros_like(R)

    capas = {
        "rojo": cv2.merge([cero, cero, R]),
        "verde": cv2.merge([cero, G, cero]),
        "azul": cv2.merge([B, cero, cero]),
        "cian": cv2.merge([B, G, cero]),
        "magenta": cv2.merge([B, cero, R]),
        "amarillo": cv2.merge([cero, G, R])
    }
    return capas

# Escala de Grises
def escala_grises(imagen_bgr):
    img_rgb = cv2.cvtColor(imagen_bgr, cv2.COLOR_BGR2RGB)
    R = img_rgb[:, :, 0].astype(np.float32)
    G = img_rgb[:, :, 1].astype(np.float32)
    B = img_rgb[:, :, 2].astype(np.float32)

    gris = 0.299 * R + 0.587 * G + 0.114 * B
    gris_uint8 = np.clip(gris, 0, 255).astype(np.uint8)

    return cv2.merge([gris_uint8, gris_uint8, gris_uint8])

# Conversión/Visualización HSV
def convertir_hsv(imagen_bgr):
    return cv2.cvtColor(imagen_bgr, cv2.COLOR_BGR2HSV)

# Destacar Rojo (Estilo RojoLive: todo lo demás a negro)
def destacar_rojo(imagen_bgr):
    img_hsv = cv2.cvtColor(imagen_bgr, cv2.COLOR_BGR2HSV)

    # Rangos de rojo ajustados como en RojoLive
    rojo_bajo1 = np.array([0, 100, 20], np.uint8)
    rojo_alto1 = np.array([8, 255, 255], np.uint8)
    rojo_bajo2 = np.array([175, 100, 20], np.uint8)
    rojo_alto2 = np.array([179, 255, 255], np.uint8)

    mask1 = cv2.inRange(img_hsv, rojo_bajo1, rojo_alto1)
    mask2 = cv2.inRange(img_hsv, rojo_bajo2, rojo_alto2)
    mask_roja = cv2.add(mask1, mask2)

    # Aplicar máscara lógica dejando en negro todo lo demás
    return cv2.bitwise_and(imagen_bgr, imagen_bgr, mask=mask_roja)

# Destacar Verde
def destacar_verde(imagen_bgr):
    verde_bajo = np.array([35, 70, 50], np.uint8)
    verde_alto = np.array([85, 255, 255], np.uint8)

    img_hsv = cv2.cvtColor(imagen_bgr, cv2.COLOR_BGR2HSV)
    mask_verde = cv2.inRange(img_hsv, verde_bajo, verde_alto)

    return cv2.bitwise_and(imagen_bgr, imagen_bgr, mask=mask_verde)

# Destacar Azul
def destacar_azul(imagen_bgr):
    azul_bajo = np.array([100, 70, 50], np.uint8)
    azul_alto = np.array([130, 255, 255], np.uint8)

    img_hsv = cv2.cvtColor(imagen_bgr, cv2.COLOR_BGR2HSV)
    mask_azul = cv2.inRange(img_hsv, azul_bajo, azul_alto)

    return cv2.bitwise_and(imagen_bgr, imagen_bgr, mask=mask_azul)

# Imagen Negativa
def negativa(imagen_bgr):
    img_rgb = cv2.cvtColor(imagen_bgr, cv2.COLOR_BGR2RGB)

    R = img_rgb[:, :, 0]
    G = img_rgb[:, :, 1]
    B = img_rgb[:, :, 2]

    R_neg = 255 - R
    G_neg = 255 - G
    B_neg = 255 - B

    img_rgb_neg = cv2.merge((R_neg, G_neg, B_neg))
    return cv2.cvtColor(img_rgb_neg, cv2.COLOR_RGB2BGR)

# Corrección Gamma
def gamma_correction(imagen_bgr, gamma):
    gamma = max(0.01, float(gamma))
    img_norm = imagen_bgr / 255.0
    img_gamma = np.power(img_norm, gamma)
    return np.clip(img_gamma * 255, 0, 255).astype(np.uint8)