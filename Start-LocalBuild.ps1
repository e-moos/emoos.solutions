docker build . --file Dockerfile -t sqeezy/emoos.solutions:latest

$containerId = docker run  -p 80:80 -p 443:443 -d sqeezy/emoos.solutions:latest

Start-Sleep 3

docker logs $containerId

Write-Host -NoNewLine 'Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');

docker logs $containerId

docker rm -fv $containerId