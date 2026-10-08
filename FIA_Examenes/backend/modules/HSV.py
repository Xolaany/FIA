""" HSV"""
import numpy as np
from fastapi import APIRouter, File, UploadFile

from utilidades import a_uint8, leer_imagen, respuesta_png

router = APIRouter(tags=["HSV"])


def rgb_a_hsv(imagen_rgb):
    """RGB -> HSV con las ecuaciones. H en grados (0-360), S y V en 0-1."""
    R = imagen_rgb[:, :, 0].astype(np.float64) / 255.0
    G = imagen_rgb[:, :, 1].astype(np.float64) / 255.0
    B = imagen_rgb[:, :, 2].astype(np.float64) / 255.0

    cmax = np.maximum(np.maximum(R, G), B)
    cmin = np.minimum(np.minimum(R, G), B)
    delta = cmax - cmin
    d = np.where(delta == 0, 1.0, delta)  # evita dividir entre 0

    V = cmax
    S = np.where(cmax == 0, 0.0, delta / np.where(cmax == 0, 1.0, cmax))

    h_r = ((G - B) / d) % 6
    h_g = (B - R) / d + 2
    h_b = (R - G) / d + 4
    H = 60.0 * np.where(cmax == R, h_r, np.where(cmax == G, h_g, h_b))
    H = np.where(delta == 0, 0.0, H)
    return H, S, V


def hsv_como_imagen(imagen_rgb):
    """Imagen para el pictureBox: canales H/2 (0-180), S*255 y V*255."""
    H, S, V = rgb_a_hsv(imagen_rgb)
    h_cv = np.round(H / 2.0) % 180
    return np.stack([a_uint8(h_cv), a_uint8(S * 255), a_uint8(V * 255)], axis=2)


@router.post("/hsv/")
async def hsv(file: UploadFile = File(...)):
    return respuesta_png(hsv_como_imagen(await leer_imagen(file)))