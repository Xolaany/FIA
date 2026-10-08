import io
import cv2
import numpy as np
from typing import Optional
from pydantic import BaseModel
from fastapi import Request, APIRouter, UploadFile, File, HTTPException, status
from fastapi.responses import StreamingResponse, Response

from bd import BaseDatos
from modules.videoCapture import videoPost

router = APIRouter(tags=["API's"])

# MODELOS PYDANTIC
class UsuarioRegistro(BaseModel):
    nombre: str
    apellido_paterno: str
    apellido_materno: str
    usuario: str
    contrasena: str

class UsuarioLogin(BaseModel):
    usuario: str
    contrasena: str


# ENDPOINTS DE AUTENTICACIÓN
@router.post("/api/registro", status_code=status.HTTP_201_CREATED)
async def registrar_usuario(datos: UsuarioRegistro):
    if not all([datos.nombre.strip(), datos.apellido_paterno.strip(), 
                datos.apellido_materno.strip(), datos.usuario.strip(), datos.contrasena.strip()]):
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST, 
            detail="Todos los campos son obligatorios"
        )
    
    id_usuario = BaseDatos.registrar_usuario(
        nombre=datos.nombre.strip(),
        apellido_paterno=datos.apellido_paterno.strip(),
        apellido_materno=datos.apellido_materno.strip(),
        usuario=datos.usuario.strip(),
        contrasena=datos.contrasena.strip()
    )
    
    if id_usuario is None:
        raise HTTPException(
            status_code=status.HTTP_409_CONFLICT, 
            detail="El nombre de usuario ya existe en la base de datos"
        )
        
    return {
        "estatus": "exito",
        "mensaje": "Usuario registrado exitosamente",
        "id_usuario": id_usuario
    }


@router.post("/api/login")
async def login(datos: UsuarioLogin):
    if not datos.usuario.strip() or not datos.contrasena.strip():
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST, 
            detail="Campos vacíos: Se requiere usuario y contraseña"
        )
        
    usuario = BaseDatos.validar_login(datos.usuario.strip(), datos.contrasena.strip())
    
    if not usuario:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED, 
            detail="Error de autenticación: Usuario o contraseña incorrectos"
        )
        
    return {
        "estatus": "exito",
        "mensaje": f"Bienvenido {usuario['nombre']}",
        "usuario": usuario
    }


# ==========================================
# ENDPOINTS DE VISIÓN / IMÁGENES Y VIDEO
# ==========================================
@router.post("/img-procesada/")
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


@router.get("/video/")
async def video(request: Request):
    return videoPost(request)