import io
import cv2
import numpy as np
from typing import Optional
from pydantic import BaseModel
from fastapi import Request, APIRouter, UploadFile, File, Form, HTTPException, status
from fastapi.responses import StreamingResponse, Response, JSONResponse

from bd import BaseDatos
from modules.videoCapture import videoPost
import modules.procesamiento as proc

router = APIRouter(tags=["API's"])


MAPA_PREPROCESAMIENTO = {
    "gray": "gris",
    "hsv": "hsv",
    "highlight_red": "rojo",
    "highlight_green": "verde",
    "highlight_blue": "azul",
    "negative": "negativa",
    "gamma": "gamma"
}

# MODELOS
class UsuarioRegistro(BaseModel):
    nombre: str
    apellido_paterno: str
    apellido_materno: str
    usuario: str
    contrasena: str

class UsuarioLogin(BaseModel):
    usuario: str
    contrasena: str


# Login y Registro
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

# Procesamientos

def bytes_a_imagen(file_bytes: bytes):
    np_arr = np.frombuffer(file_bytes, np.uint8)
    return cv2.imdecode(np_arr, cv2.IMREAD_COLOR)

def imagen_a_bytes(img, formato=".png") -> bytes:
    _, buffer = cv2.imencode(formato, img)
    return buffer.tobytes()


@router.post("/api/aplicar-filtro")
async def aplicar_filtro(
    file: UploadFile = File(...),
    accion: str = Form(...),
    param: float = Form(1.0)
):
    try:
        contents = await file.read()
        img = bytes_a_imagen(contents)
        if img is None:
            raise HTTPException(status_code=400, detail="Formato de imagen inválido")

        resultado = img

        if accion == "gray":
            resultado = proc.escala_grises(img)
        elif accion == "hsv":
            resultado = proc.convertir_hsv(img)
        elif accion == "highlight_red":
            resultado = proc.destacar_rojo(img)
        elif accion == "highlight_green":
            resultado = proc.destacar_verde(img)
        elif accion == "highlight_blue":
            resultado = proc.destacar_azul(img)
        elif accion == "negative":
            resultado = proc.negativa(img)
        elif accion == "gamma":
            resultado = proc.gamma_correction(img, param)

        res_bytes = imagen_a_bytes(resultado)
        return Response(content=res_bytes, media_type="image/png")

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.post("/api/separar-capas")
async def separar_capas_api(file: UploadFile = File(...)):
    try:
        contents = await file.read()
        img = bytes_a_imagen(contents)
        if img is None:
            raise HTTPException(status_code=400, detail="Formato de imagen inválido")

        capas_dict = proc.separar_capas(img)
        resultado = {}

        for nombre, img_capa in capas_dict.items():
            resultado[nombre] = imagen_a_bytes(img_capa).hex()

        return JSONResponse(content=resultado)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# Guardado en Base de Datos
@router.post("/api/guardar-imagen-original")
async def guardar_imagen_original(
    id_usuario: int = Form(...),
    nombre: str = Form("imagen.png"),
    file: UploadFile = File(...)
):
    try:
        contents = await file.read()
        img = bytes_a_imagen(contents)
        if img is None:
            raise HTTPException(status_code=400, detail="Formato de imagen inválido")

        alto, ancho, _ = img.shape
        # Convertir a RGB para generar la matriz de píxeles
        img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        matriz_pixels = img_rgb.tolist()

        id_imagen = BaseDatos.guardar_imagen_original(
            id_usuario=id_usuario,
            nombre=nombre,
            ancho=ancho,
            alto=alto,
            matriz_rgb=matriz_pixels,
            bytes_imagen=contents
        )

        if not id_imagen:
            raise HTTPException(status_code=500, detail="Error al insertar en la base de datos")

        return {"estatus": "exito", "id_imagen": id_imagen}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.post("/api/guardar-preprocesamiento")
async def guardar_preprocesamiento(
    id_imagen: int = Form(...),
    tipo_preprocesamiento: str = Form(...),
    parametro: Optional[float] = Form(None),
    file: UploadFile = File(...)
):
    try:
        contents = await file.read()
        img = bytes_a_imagen(contents)
        if img is None:
            raise HTTPException(status_code=400, detail="Formato de imagen inválido")

        resultado = img
        param_val = parametro if parametro is not None else 1.0

        if tipo_preprocesamiento == "gray":
            resultado = proc.escala_grises(img)
        elif tipo_preprocesamiento == "hsv":
            resultado = proc.convertir_hsv(img)
        elif tipo_preprocesamiento == "highlight_red":
            resultado = proc.destacar_rojo(img)
        elif tipo_preprocesamiento == "highlight_green":
            resultado = proc.destacar_verde(img)
        elif tipo_preprocesamiento == "highlight_blue":
            resultado = proc.destacar_azul(img)
        elif tipo_preprocesamiento == "negative":
            resultado = proc.negativa(img)
        elif tipo_preprocesamiento == "gamma":
            resultado = proc.gamma_correction(img, param_val)

        alto, ancho = resultado.shape[:2]
        img_rgb = cv2.cvtColor(resultado, cv2.COLOR_BGR2RGB) if len(resultado.shape) == 3 else resultado
        matriz_pixels = img_rgb.tolist()
        bytes_procesados = imagen_a_bytes(resultado)

        tipo_bd = MAPA_PREPROCESAMIENTO.get(tipo_preprocesamiento, tipo_preprocesamiento)

        id_prep = BaseDatos.guardar_imagen_preprocesada(
            id_imagen=id_imagen,
            tipo_preprocesamiento=tipo_bd,
            ancho=ancho,
            alto=alto,
            matriz_rgb=matriz_pixels,
            parametro=parametro,
            bytes_imagen=bytes_procesados
        )

        if not id_prep:
            raise HTTPException(status_code=500, detail="Error al guardar preprocesamiento en BD")

        return {"estatus": "exito", "id_preprocesamiento": id_prep}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

# STREAMING DE VIDEO
@router.get("/video/")
async def video(request: Request):
    return videoPost(request)