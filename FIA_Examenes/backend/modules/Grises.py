
import numpy as np
from fastapi import APIRouter, File, UploadFile

from utilidades import a_uint8, leer_imagen, respuesta_png

router = APIRouter(tags=["Gris"])


def convertir_a_gris(imagen_rgb):
    """Ecuación de luminancia (NO cvtColor):  Y = 0.299 R + 0.587 G + 0.114 B"""
    R = imagen_rgb[:, :, 0].astype(np.float64)
    G = imagen_rgb[:, :, 1].astype(np.float64)
    B = imagen_rgb[:, :, 2].astype(np.float64)
    return a_uint8(0.299 * R + 0.587 * G + 0.114 * B)


@router.post("/gris/")
async def gris(file: UploadFile = File(...)):
    return respuesta_png(convertir_a_gris(await leer_imagen(file)))