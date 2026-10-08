create database vision_artificial_2;

\c vision_artificial_2;

create table usuarios (
    id_usuario serial primary key,
    nombre varchar(50) not null,
    apellido_paterno varchar(50) not null,
    apellido_materno varchar(50) not null,
    usuario varchar(50) not null unique,
    contrasena varchar(100) not null
);

create table imagenes (
    id_imagen serial primary key,
    id_usuario integer not null,
    nombre varchar(100) not null,
    ancho integer not null,
    alto integer not null,
    fecha timestamp default current_timestamp,
    tipo varchar(20) default 'original',
    imagen_original bytea,
    matriz_pixeles jsonb not null, -- Reemplaza a la tabla 'pixeles'
    constraint fk_imagen_usuario
        foreign key (id_usuario)
        references usuarios(id_usuario)
);

create table preprocesamientos (
    id_preprocesamiento serial primary key,
    id_imagen integer not null,
    tipo_preprocesamiento varchar(30) not null,
    parametro numeric(4,2),
    fecha timestamp default current_timestamp,
    constraint fk_preprocesamiento_imagen
        foreign key (id_imagen)
        references imagenes(id_imagen),
    constraint chk_tipo_preprocesamiento
        check (
            tipo_preprocesamiento in (
                'gris',
                'hsv',
                'negativa',
                'gamma',
                'rojo',
                'verde',
                'azul'
            )
        ),
    constraint chk_gamma
        check (
            tipo_preprocesamiento <> 'gamma'
            or parametro between 0 and 2
        )
);

create table imagenes_preprocesadas (
    id_imagen_preprocesada serial primary key,
    id_preprocesamiento integer not null,
    ancho integer not null,
    alto integer not null,
    fecha timestamp default current_timestamp,
    imagen bytea,
    matriz_pixeles jsonb not null, -- Reemplaza a la tabla 'pixeles_preprocesados'
    constraint fk_imagen_preprocesada
        foreign key (id_preprocesamiento)
        references preprocesamientos(id_preprocesamiento)
);