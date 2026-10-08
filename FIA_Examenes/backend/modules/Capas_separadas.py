
import base64

import cv2
import numpy as np
from fastapi import APIRouter, File, HTTPException, UploadFile

router = APIRouter(tags=["Separar en capas"])


def separar_capas(imagen_rgb):
    """Diccionario nombre -> imagen RGB de cada capa de color."""
    R, G, B = imagen_rgb[:, :, 0], imagen_rgb[:, :, 1], imagen_rgb[:, :, 2]
    cero = np.zeros_like(R)
    return {
        "original": imagen_rgb,
        "rojo": np.stack([R, cero, cero], axis=2),
        "verde": np.stack([cero, G, cero], axis=2),
        "azul": np.stack([cero, cero, B], axis=2),
        "cian": np.stack([cero, G, B], axis=2),
        "magenta": np.stack([R, cero, B], axis=2),
        "amarillo": np.stack([R, G, cero], axis=2),
    }


@router.post("/capas/")
async def capas(file: UploadFile = File(...)):
    nparr = np.frombuffer(await file.read(), np.uint8)
    bgr = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    if bgr is None:
        raise HTTPException(status_code=400, detail="Formato de imagen inválido")
    rgb = cv2.cvtColor(bgr, cv2.COLOR_BGR2RGB)

    resultado = {}
    for nombre, capa in separar_capas(rgb).items():
        ok, buffer = cv2.imencode(".png", cv2.cvtColor(capa, cv2.COLOR_RGB2BGR))
        resultado[nombre] = base64.b64encode(buffer.tobytes()).decode("ascii")
    return resultado