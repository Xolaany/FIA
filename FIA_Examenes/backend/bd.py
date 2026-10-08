import logging
import psycopg
from psycopg.rows import dict_row

logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s")

class BaseDatos:
    _conn = None

    @classmethod
    def conectar(cls, host="localhost", dbname="vision_artificial_2", user="postgres", password="Root_Hash", port=5432):
        """Abre una conexión persistente a la BD."""
        if cls._conn is None or cls._conn.closed:
            try:
                conninfo = f"host={host} dbname={dbname} user={user} password={password} port={port}"
                cls._conn = psycopg.connect(conninfo)
                logging.info("Conexión a PostgreSQL establecida.")
            except psycopg.Error as e:
                logging.error(f"Error al conectar a la base de datos: {e}")
                raise e
        return cls._conn

    @classmethod
    def cerrar_conexion(cls):
        if cls._conn and not cls._conn.closed:
            cls._conn.close()
            logging.info("Conexión a PostgreSQL cerrada.")

    @classmethod
    def registrar_usuario(cls, nombre, apellido_paterno, apellido_materno, usuario, contrasena):
        conn = cls.conectar()
        try:
            with conn.cursor() as cursor:
                query = """
                    INSERT INTO usuarios (nombre, apellido_paterno, apellido_materno, usuario, contrasena)
                    VALUES (%s, %s, %s, %s, %s)
                    RETURNING id_usuario;
                """
                cursor.execute(query, (nombre, apellido_paterno, apellido_materno, usuario, contrasena))
                id_usuario = cursor.fetchone()[0]
                conn.commit()
                return id_usuario
        except psycopg.errors.UniqueViolation:
            conn.rollback()
            logging.warning(f"El usuario '{usuario}' ya existe.")
            return None
        except psycopg.Error as e:
            conn.rollback()
            logging.error(f"Error al registrar usuario: {e}")
            return None

    @classmethod
    def validar_login(cls, usuario, contrasena):
        conn = cls.conectar()
        try:
            with conn.cursor(row_factory=dict_row) as cursor:
                query = """
                    SELECT id_usuario, nombre, usuario 
                    FROM usuarios 
                    WHERE usuario = %s AND contrasena = %s;
                """
                cursor.execute(query, (usuario, contrasena))
                return cursor.fetchone()
        except psycopg.Error as e:
            logging.error(f"Error en login: {e}")
            return None

    @classmethod
    def guardar_imagen_original(cls, id_usuario, nombre, ancho, alto, matriz_rgb, bytes_imagen=None):
        conn = cls.conectar()
        try:
            with conn.cursor() as cursor:
                query = """
                    INSERT INTO imagenes (id_usuario, nombre, ancho, alto, matriz_pixeles, imagen_original)
                    VALUES (%s, %s, %s, %s, %s, %s)
                    RETURNING id_imagen;
                """
                cursor.execute(query, (id_usuario, nombre, ancho, alto, psycopg.types.json.Jsonb(matriz_rgb), bytes_imagen))
                id_imagen = cursor.fetchone()[0]
                conn.commit()
                logging.info(f"Imagen '{nombre}' (ID: {id_imagen}) guardada correctamente.")
                return id_imagen
        except psycopg.Error as e:
            conn.rollback()
            logging.error(f"Error al guardar imagen original: {e}")
            return None

    @classmethod
    def guardar_imagen_preprocesada(cls, id_imagen, tipo_preprocesamiento, ancho, alto, matriz_rgb, parametro=None, bytes_imagen=None):
        conn = cls.conectar()
        try:
            with conn.cursor() as cursor:
                query_prep = """
                    INSERT INTO preprocesamientos (id_imagen, tipo_preprocesamiento, parametro)
                    VALUES (%s, %s, %s)
                    RETURNING id_preprocesamiento;
                """
                cursor.execute(query_prep, (id_imagen, tipo_preprocesamiento, parametro))
                id_prep = cursor.fetchone()[0]

                query_img_prep = """
                    INSERT INTO imagenes_preprocesadas (id_preprocesamiento, ancho, alto, matriz_pixeles, imagen)
                    VALUES (%s, %s, %s, %s, %s)
                    RETURNING id_imagen_preprocesada;
                """
                cursor.execute(query_img_prep, (id_prep, ancho, alto, psycopg.types.json.Jsonb(matriz_rgb), bytes_imagen))
                id_img_proc = cursor.fetchone()[0]

                conn.commit()
                logging.info(f"Preprocesamiento '{tipo_preprocesamiento}' (ID: {id_img_proc}) guardado correctamente.")
                return id_img_proc
        except psycopg.Error as e:
            conn.rollback()
            logging.error(f"Error al guardar imagen preprocesada: {e}")
            return None

    @classmethod
    def obtener_matriz_imagen_original(cls, id_imagen):
        conn = cls.conectar()
        try:
            with conn.cursor() as cursor:
                query = "SELECT matriz_pixeles FROM imagenes WHERE id_imagen = %s;"
                cursor.execute(query, (id_imagen,))
                resultado = cursor.fetchone()
                return resultado[0] if resultado else None
        except psycopg.Error as e:
            logging.error(f"Error al obtener matriz: {e}")
            return None