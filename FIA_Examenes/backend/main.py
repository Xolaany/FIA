from fastapi import FastAPI
from API import router as api_router
from bd import BaseDatos

app = FastAPI(title="ExamenAPI")


@app.on_event("startup")
def startup_event():
    # Inicializa la conexión con PostgreSQL
    BaseDatos.conectar()

@app.on_event("shutdown")
def shutdown_event():
    # Cierra la conexión de forma limpia
    BaseDatos.cerrar_conexion()

# Incluir todas las rutas definidas en API.py
app.include_router(api_router)


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000)