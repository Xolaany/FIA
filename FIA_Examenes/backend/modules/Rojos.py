
import numpy as np
from fastapi import APIRouter, File, UploadFile

from boton_gris import convertir_a_gris
from boton_hsv import rgb_a_hsv
from utilidades import leer_imagen, respuesta_png

router = APIRouter(tags=["Destacar rojo"])

# Matiz del rojo en escala 0-180 (OpenCV): está en ambos extremos del círculo
RANGOS_ROJO = [(0, 10), (170, 180)]
S_MIN = 50  # saturación mínima (0-255): descarta píxeles casi grises
V_MIN = 50  # valor mínimo (0-255): descarta píxeles muy oscuros


def destacar_rojo(imagen_rgb):
    """Deja en color los píxeles rojos; el resto queda en gris."""
    H, S, V = rgb_a_hsv(imagen_rgb)
    h = H / 2.0
    mascara = np.zeros(h.shape, dtype=bool)
    for h_min, h_max in RANGOS_ROJO:
        mascara |= (h >= h_min) & (h <= h_max)
    mascara &= (S * 255 >= S_MIN) & (V * 255 >= V_MIN)

    gris = convertir_a_gris(imagen_rgb)
    gris_rgb = np.stack([gris, gris, gris], axis=2)
    return np.where(mascara[:, :, None], imagen_rgb, gris_rgb)


@router.post("/destacar/rojo")
async def destacar_color_rojo(file: UploadFile = File(...)):
    return respuesta_png(destacar_rojo(await leer_imagen(file)))