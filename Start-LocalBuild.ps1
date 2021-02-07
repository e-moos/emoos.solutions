docker build . --file Dockerfile -t sqeezy/emoos.solutions:latest

$containerId = docker run  -p 80:80 -p 443:443 -d sqeezy/emoos.solutions:latest

docker logs -f $containerId

docker rm -fv $containerId