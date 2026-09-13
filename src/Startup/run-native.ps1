# Runs the OpenMU server natively (no Docker), against the shared Postgres
# container (openmu-season1-smoke-db) published on host port 5432.
#
# Before running this:
#   docker start openmu-season1-smoke-db
#   docker stop openmu-season1-smoke-app   # frees the game ports for this process
#
# Usage:
#   .\run-native.ps1                        # local-only (127.127.127.127), admin panel on :8081
#   .\run-native.ps1 100.125.169.104        # exposes to Tailscale, like the Docker setup
#   .\run-native.ps1 loopback -reinit       # re-seeds the DB from scratch (careful: shared DB!)
#
# The first argument (if not "-reinit" or another dash-prefixed flag) sets RESOLVE_IP:
# "loopback" (default), "local", "public", or a literal IP/hostname. Anything else you pass
# is forwarded to the server as extra command-line args.
#
# We use the RESOLVE_IP environment variable instead of the -resolveIP: CLI argument on
# purpose: passing "-resolveIP:loopback" through `dotnet run` on this machine gets the
# argument split into "-resolveIP:" and "loopback" as two separate argv entries, which the
# server reads as an empty custom resolver and crashes with "a parameter with an IP or host
# name is required". The environment variable is read directly, with no argv parsing involved.

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$env:DB_HOST = "127.0.0.1"
$env:DB_ADMIN_USER = "postgres"
$env:DB_ADMIN_PW = "admin"
$env:ASPNETCORE_URLS = "http://+:8081"

$resolveIp = "loopback"
$extraArgs = $args

if ($args.Count -gt 0 -and $args[0] -notmatch "^-") {
    $resolveIp = $args[0]
    $extraArgs = $args[1..($args.Count - 1)]
}

$env:RESOLVE_IP = $resolveIp

dotnet run -c Release --no-launch-profile -- -autostart -version:season1-classic @extraArgs
