from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import Response
from API import router as api_router
import cv2
import numpy as np
import io

app = FastAPI(title="ExamenAPI")

app.include_router(api_router)

@app.post("/img-procesada/")
async def img_procesada(file: UploadFile = File(...)):
    contenido = await file.read()
    nparr = np.frombuffer(contenido, np.uint8)
    img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

    if img is None:
        raise HTTPException(status_code=400, detail="Formato de imagen inválido")

    b = img[:, :, 0].astype(np.float32)
    g = img[:, :, 1].astype(np.float32)
    r = img[:, :, 2].astype(np.float32)

    gray_array = 0.299 * r + 0.587 * g + 0.114 * b

    gray_custom = np.clip(gray_array, 0, 255).astype(np.uint8)

    _, empaquetar_img = cv2.imencode('.png', gray_custom)

    return Response(content=empaquetar_img.tobytes(), media_type='image/png')

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000)