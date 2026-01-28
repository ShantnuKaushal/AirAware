FROM node:20-alpine
WORKDIR /app
COPY src/airaware.web/package*.json ./
RUN npm install
COPY src/airaware.web/ .
RUN npm run build
EXPOSE 3000
CMD ["npm", "run", "start"]