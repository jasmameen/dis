<?php
/**
 * تشغيل واجهة نظام الحضور الأكاديمي
 *
 * وضع التطوير (Vite + Hot Reload):
 *   php start.php
 *
 * وضع الإنتاج (ملفات dist جاهزة):
 *   php start.php serve
 *
 * بناء ثم تشغيل الإنتاج:
 *   php start.php build-serve
 */

declare(strict_types=1);

$frontendDir = __DIR__;
$port = (int) (getenv('FRONTEND_PORT') ?: 5173);
$host = getenv('FRONTEND_HOST') ?: '127.0.0.1';
$apiUrl = getenv('API_URL') ?: 'http://localhost:5000';
$mode = $argv[1] ?? 'dev';

chdir($frontendDir);

function line(string $text): void
{
    echo $text . PHP_EOL;
}

function fail(string $message, int $code = 1): never
{
    fwrite(STDERR, "خطأ: {$message}" . PHP_EOL);
    exit($code);
}

function commandExists(string $command): bool
{
    $check = stripos(PHP_OS, 'WIN') === 0
        ? "where {$command} 2>nul"
        : "command -v {$command} 2>/dev/null";

    exec($check, $output, $code);
    return $code === 0;
}

function run(string $command): int
{
    line("> {$command}");
    passthru($command, $code);
    return $code;
}

function isWindows(): bool
{
    return stripos(PHP_OS, 'WIN') === 0;
}

function openBrowser(string $url): void
{
    if (php_sapi_name() === 'cli') {
        line("افتح المتصفح: {$url}");
    }

    if (isWindows()) {
        pclose(popen('start "" "' . $url . '"', 'r'));
    } elseif (PHP_OS === 'Darwin') {
        exec('open ' . escapeshellarg($url));
    } else {
        exec('xdg-open ' . escapeshellarg($url) . ' >/dev/null 2>&1 &');
    }
}

function ensureNode(): void
{
    if (!commandExists('node')) {
        fail('Node.js غير مثبت. حمّله من https://nodejs.org');
    }
}

function ensureDependencies(): void
{
    if (!is_dir(__DIR__ . '/node_modules')) {
        line('جاري تثبيت الحزم (npm install)...');
        if (!commandExists('npm')) {
            fail('npm غير متوفر. ثبّت Node.js أولاً.');
        }
        $code = run('npm install');
        if ($code !== 0) {
            fail('فشل npm install');
        }
    }
}

function startDev(int $port, string $host): void
{
    ensureNode();
    ensureDependencies();

    $vite = __DIR__ . DIRECTORY_SEPARATOR . 'node_modules' . DIRECTORY_SEPARATOR . 'vite' . DIRECTORY_SEPARATOR . 'bin' . DIRECTORY_SEPARATOR . 'vite.js';
    if (!is_file($vite)) {
        fail('لم يُعثر على Vite. نفّذ: npm install');
    }

    $url = "http://{$host}:{$port}";
    line('');
    line('========================================');
    line('  نظام الحضور الأكاديمي - Frontend');
    line('========================================');
    line("  الوضع: تطوير (Vite)");
    line("  الرابط: {$url}");
    line('  API: http://localhost:5000');
    line('  أوقف التشغيل: Ctrl + C');
    line('========================================');
    line('');

    openBrowser($url);

    $node = escapeshellarg('node');
    $vitePath = escapeshellarg($vite);
    $hostArg = escapeshellarg($host);
    $command = "{$node} {$vitePath} --host {$hostArg} --port {$port}";

    $code = run($command);
    exit($code);
}

function buildFrontend(): void
{
    ensureNode();
    ensureDependencies();

    line('جاري بناء الواجهة (npm run build)...');
    $code = run('npm run build');
    if ($code !== 0) {
        fail('فشل البناء');
    }

    if (!is_dir(__DIR__ . '/dist')) {
        fail('مجلد dist غير موجود بعد البناء');
    }
}

function startServe(int $port, string $host, string $apiUrl): void
{
    $distDir = __DIR__ . '/dist';
    if (!is_dir($distDir)) {
        line('مجلد dist غير موجود. سيتم البناء أولاً...');
        buildFrontend();
    }

    $router = __DIR__ . '/router.php';
    if (!is_file($router)) {
        fail('ملف router.php غير موجود');
    }

    putenv('API_URL=' . $apiUrl);

    $url = "http://{$host}:{$port}";
    line('');
    line('========================================');
    line('  نظام الحضور الأكاديمي - Frontend');
    line('========================================');
    line("  الوضع: إنتاج (PHP Built-in Server)");
    line("  الرابط: {$url}");
    line("  API: {$apiUrl}");
    line('  أوقف التشغيل: Ctrl + C');
    line('========================================');
    line('');

    openBrowser($url);

    $routerPath = escapeshellarg($router);
    $address = escapeshellarg("{$host}:{$port}");
    $command = 'php -S ' . $address . ' -t dist ' . $routerPath;

    $code = run($command);
    exit($code);
}

match ($mode) {
    'dev', '--dev', '-d' => startDev($port, $host),
    'serve', '--serve', '-s' => startServe($port, $host, $apiUrl),
    'build-serve', 'build' => (function () use ($port, $host, $apiUrl): void {
        buildFrontend();
        if (($GLOBALS['argv'][1] ?? '') === 'build') {
            line('تم البناء بنجاح.');
            exit(0);
        }
        startServe($port, $host, $apiUrl);
    })(),
    'help', '--help', '-h' => (function (): void {
        line('الاستخدام:');
        line('  php start.php           تشغيل Vite (تطوير)');
        line('  php start.php serve     تشغيل dist عبر PHP');
        line('  php start.php build     بناء الواجهة فقط');
        line('  php start.php build-serve  بناء ثم تشغيل');
        exit(0);
    })(),
    default => fail("وضع غير معروف: {$mode}. استخدم: php start.php help"),
};
