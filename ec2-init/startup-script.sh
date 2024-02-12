#!/bin/bash
aws s3 cp --recursive s3://shtannikov-api apps/
cd apps

mv web-api.service /etc/systemd/system/web-api.service
mv appsettings.json /
chmod +x web-api

systemctl start web-api.service
systemctl enable web-api.service