cd /etc/docker
cat >> daemon.json << EOF
{"hosts": ["tcp://0.0.0.0:2375", "unix:///var/run/docker.sock"]}
EOF

mkdir /etc/systemd/system/docker.service.daemon
cd /etc/systemd/system/docker.service.daemon/
cat > override.conf << EOF
 [Service]
 ExecStart=
 ExecStart=/usr/bin/dockerd
EOF

systemctl daemon-reload
systemctl restart docker.service
