# Stage 1: Build
# node:20-alpine compiles TypeScript and bundles assets; discarded after this stage.
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build -- --configuration production

# Stage 2: Serve
# nginx:alpine (~23 MB) serves the static output with SPA routing support.
FROM nginx:alpine AS runtime
COPY --from=build /app/dist/skyroute-frontend/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
