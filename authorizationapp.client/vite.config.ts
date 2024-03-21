import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';

const baseFolder =
    process.env.APPDATA !== undefined && process.env.APPDATA !== ''
        ? `${process.env.APPDATA}/ASP.NET/https`
        : `${process.env.HOME}/.aspnet/https`;

const certificateArg = process.argv.map(arg => arg.match(/--name=(?<value>.+)/i)).filter(Boolean)[0];
const certificateName = certificateArg ? certificateArg.groups.value : "authorizationapp.client";

if (!certificateName) {
    console.error('Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly.')
    process.exit(-1);
}

const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (0 !== child_process.spawnSync('dotnet', [
        'dev-certs',
        'https',
        '--export-path',
        certFilePath,
        '--format',
        'Pem',
        '--no-password',
    ], { stdio: 'inherit', }).status) {
        throw new Error("Could not create certificate.");
    }
}

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        proxy: {
            '^/weatherforecast': {
                target: 'https://localhost:7028/',
                secure: false
            },
            '^/users/create': {
                target: 'https://localhost:7028/',
                secure: false
            },
            '^/pingauth': {
                target: 'https://localhost:7028/',
                secure: false
            },
            '^/register': {
                target: 'https://localhost:7028/',
                secure: false
            },
            '^/login': {
                target: 'https://localhost:7028/',
                secure: false
            },
            '^/logout': {
                target: 'https://localhost:7028/',
                secure: false
            },
            '^/users': {
                target: 'https://localhost:7028/',
                secure: false
            },
            '^/deleteUser': {
                target: 'https://localhost:7028/',
                secure: false
            },
            '^/setAdminRights': {
                target: 'https://localhost:7028/',
                secure: false
            },
            '^/updatePhoto': {
                target: 'https://localhost:7028/',
                secure: false
            },
            '/currentUser': {
                target: 'https://localhost:7028/',
                secure: false
            }
            
        },
        port: 5173,
        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        }
    }
})
