FROM alpine:latest AS generate
WORKDIR /certificates
RUN apk update && \
	apk add --no-cache openssl && \
	rm -rf /var/cache/apk/*
RUN openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout todolistproxy.key -out todolistproxy.crt -subj "/C=GB"

FROM nginx:latest
EXPOSE 443
COPY nginx.conf /etc/nginx/nginx.conf
COPY --from=generate /certificates/todolistproxy.key /certificates/todolistproxy.crt /etc/nginx/ssl/
