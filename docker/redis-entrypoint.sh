#!/bin/sh
set -e

PASSWORD=$(tr -d '[:space:]' < /run/secrets/redis_password)
sed "s|generate_a_strong_long_password_here|${PASSWORD}|g" /etc/redis/redis.conf > /tmp/redis-final.conf

exec redis-server /tmp/redis-final.conf
