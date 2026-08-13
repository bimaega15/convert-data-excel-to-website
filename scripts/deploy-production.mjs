#!/usr/bin/env node
// Build Vue client + publish .NET server locally, then ship to the production
// server over SSH and swap it into place with an automatic rollback if the
// health check after restart fails.
import { execFileSync } from "node:child_process";
import { existsSync, rmSync, cpSync, mkdirSync } from "node:fs";
import { fileURLToPath } from "node:url";
import os from "node:os";
import path from "node:path";

const ROOT = path.dirname(path.dirname(fileURLToPath(import.meta.url)));
const CLIENT_DIR = path.join(ROOT, "sifp_vue.client");
const SERVER_PROJECT = path.join(ROOT, "Sifp_Vue.Server", "Sifp_Vue.Server.csproj");
const WWWROOT_DIR = path.join(ROOT, "Sifp_Vue.Server", "wwwroot");
const PUBLISH_DIR = path.join(ROOT, "build", "publish");
const TARBALL = path.join(ROOT, "build", "deploy.tar.gz");

const REMOTE_HOST = process.env.DEPLOY_HOST ?? "103.253.244.37";
const REMOTE_PORT = process.env.DEPLOY_PORT ?? "22";
const REMOTE_USER = process.env.DEPLOY_USER ?? "root";
const REMOTE_PATH = "/home/region4pep.com";
const SERVICE_NAME = "sifp.service";
const SSH_KEY = process.env.DEPLOY_SSH_KEY ?? path.join(os.homedir(), ".ssh", "id_ed25519_sifp_deploy");

function run(cmd, args, opts = {}) {
  console.log(`\n$ ${cmd} ${args.join(" ")}`);
  execFileSync(cmd, args, { stdio: "inherit", ...opts });
}

function sshExec(remoteCommand) {
  run("ssh", ["-i", SSH_KEY, "-o", "BatchMode=yes", "-p", REMOTE_PORT, `${REMOTE_USER}@${REMOTE_HOST}`, remoteCommand]);
}

if (!existsSync(SSH_KEY)) {
  console.error(`SSH deploy key not found at ${SSH_KEY}. Set DEPLOY_SSH_KEY or generate the key first.`);
  process.exit(1);
}

console.log("== 1/5 Building Vue client ==");
run("npm", ["run", "build"], { cwd: CLIENT_DIR, shell: process.platform === "win32" });

console.log("== 2/5 Refreshing wwwroot with new build ==");
// Only wipe the Vue-owned assets/ subfolder (see .gitignore) — wwwroot also
// holds Razor /admin static files (css/, js/, lib/, favicon.ico) that must
// survive a frontend rebuild, so this merges rather than replacing wwwroot wholesale.
rmSync(path.join(WWWROOT_DIR, "assets"), { recursive: true, force: true });
mkdirSync(WWWROOT_DIR, { recursive: true });
cpSync(path.join(CLIENT_DIR, "dist"), WWWROOT_DIR, { recursive: true });

console.log("== 3/5 Publishing .NET server (linux-x64) ==");
rmSync(PUBLISH_DIR, { recursive: true, force: true });
run("dotnet", [
  "publish", SERVER_PROJECT,
  "-c", "Release",
  "-r", "linux-x64",
  "--self-contained", "false",
  "-o", PUBLISH_DIR,
]);

console.log("== 4/5 Packaging and uploading ==");
const BUILD_DIR = path.dirname(TARBALL);
mkdirSync(BUILD_DIR, { recursive: true });
rmSync(TARBALL, { force: true });
// Use relative paths (cwd = build/) — GNU tar on Windows misreads "C:\..." as a remote host spec.
run("tar", ["-czf", path.basename(TARBALL), "-C", "publish", "."], { cwd: BUILD_DIR });
run("scp", ["-i", SSH_KEY, "-P", REMOTE_PORT, TARBALL, `${REMOTE_USER}@${REMOTE_HOST}:/tmp/sifp-deploy.tar.gz`]);

console.log("== 5/5 Swapping into place on server (with auto-rollback) ==");
const remoteScript = `set -e
rm -rf '${REMOTE_PATH}.new'
mkdir -p '${REMOTE_PATH}.new'
tar xzf /tmp/sifp-deploy.tar.gz -C '${REMOTE_PATH}.new'
chmod +x '${REMOTE_PATH}.new/Sifp_Vue.Server'
chown -R root:root '${REMOTE_PATH}.new'
systemctl stop ${SERVICE_NAME}
rm -rf '${REMOTE_PATH}.prev'
mv '${REMOTE_PATH}' '${REMOTE_PATH}.prev'
mv '${REMOTE_PATH}.new' '${REMOTE_PATH}'
systemctl start ${SERVICE_NAME}
HEALTHY=0
for i in 1 2 3 4 5; do
  sleep 3
  if curl -sf -o /dev/null http://localhost:5000/api/dashboard; then
    HEALTHY=1
    break
  fi
done
if [ "$HEALTHY" = "1" ]; then
  echo DEPLOY_OK
  rm -rf '${REMOTE_PATH}.prev'
  rm -f /tmp/sifp-deploy.tar.gz
else
  echo 'Health check failed, rolling back' >&2
  systemctl stop ${SERVICE_NAME}
  rm -rf '${REMOTE_PATH}'
  mv '${REMOTE_PATH}.prev' '${REMOTE_PATH}'
  systemctl start ${SERVICE_NAME}
  exit 1
fi`;

sshExec(remoteScript);

console.log("\nDeploy selesai: https://region4pep.com");
