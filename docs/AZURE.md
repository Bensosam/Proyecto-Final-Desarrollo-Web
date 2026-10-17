# Publicación en Microsoft Azure

Este documento resume las URLs públicas y la arquitectura desplegada del proyecto final de Clínica.

## Frontend

Aplicación web publicada en Microsoft Azure:

https://clinica-web-20261016193538.azurewebsites.net

## API REST

API desarrollada en ASP.NET Core .NET 10 y publicada en Microsoft Azure:

https://clinica-api-20261016192458.azurewebsites.net

## Arquitectura

La arquitectura utilizada por la solución es:

```text
Usuario
   ↓
Frontend Web / PWA
   ↓
API REST .NET 10
   ↓
Entity Framework Core
   ↓
Azure SQL Database
```

El frontend no se conecta directamente a la base de datos.

## Aplicación móvil / PWA

La aplicación móvil de farmacia está implementada como una PWA y consume la misma API REST .NET 10 publicada en Azure.

Permite consultar medicamentos de farmacia mostrando información como:

- Nombre.
- Precio.
- Marca.
- Descripción.
- Existencia.
- Imagen.

También permite consultar el detalle de cada medicamento y aplicar filtros por nombre, marca y categoría.

## Nota

Tanto el frontend como la API están accesibles públicamente mediante HTTPS y utilizan Azure SQL Database como almacenamiento de datos.
