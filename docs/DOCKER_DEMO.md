# Demostración de Docker

Este documento contiene los comandos básicos para demostrar la ejecución de la API del proyecto dentro de Docker.

## 1. Construir la imagen

Desde la carpeta raíz del proyecto `ClinicaFinal`:

```powershell
docker build -f .\Clinica.API\Dockerfile -t clinica-api:latest .
```

Este comando construye la imagen de la API .NET 10 utilizando el archivo:

```text
Clinica.API/Dockerfile
```

## 2. Iniciar el contenedor

Si el contenedor `clinica-api-docker` ya fue creado previamente:

```powershell
docker start clinica-api-docker
```

## 3. Verificar que el contenedor esté ejecutándose

```powershell
docker ps
```

Debe aparecer el contenedor:

```text
clinica-api-docker
```

## 4. Detener el contenedor

```powershell
docker stop clinica-api-docker
```

## Nota

La API REST del proyecto está desarrollada con ASP.NET Core sobre .NET 10 y puede ejecutarse dentro de un contenedor Docker. Durante la presentación se puede demostrar la construcción de la imagen y posteriormente iniciar el contenedor ya configurado.
