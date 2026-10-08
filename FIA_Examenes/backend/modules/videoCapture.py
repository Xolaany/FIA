from datetime import datetime
from fastapi.responses import StreamingResponse
from fastapi import Request
import cv2
import asyncio
import os

#Funcion Generar Video(frames/fotogramas)
async def videoGen(request: Request):
    img = cv2.VideoCapture(0, cv2.CAP_V4L2)
    try:
        while img.isOpened():

            if await request.is_disconnected():
                break

            success, frame = img.read()
            if not success:
                break
            
            frame = cv2.flip(frame, 1)

            _, buffer = cv2.imencode('.jpg', frame)
            frame_bytes = buffer.tobytes()

            yield (
                b'--frame\r\n'
                b'Content-Type: image/jpeg\r\n\r\n' + frame_bytes + b'\r\n'
            )

            await asyncio.sleep(0.03)
    finally:
        img.release()
        cv2.destroyAllWindows()

#Funcion Enviar video
def videoPost(request: Request):
    return StreamingResponse(videoGen(request), media_type="multipart/x-mixed-replace; boundary=frame")

