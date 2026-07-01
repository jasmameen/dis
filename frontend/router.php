<?php
/**
 * Router لخادم PHP المدمج — يخدم SPA ويوجّه /api إلى Backend
 * يُستخدم مع: php -S host:port -t dist router.php
 */

declare(strict_types=1);

$uri = parse_url($_SERVER['REQUEST_URI'] ?? '/', PHP_URL_PATH) ?: '/';
$distDir = __DIR__ . '/dist';
$apiBase = rtrim(getenv('API_URL') ?: 'http://localhost:5000', '/');

// توجيه طلبات API إلى ASP.NET Backend
if (str_starts_with($uri, '/api')) {
    $target = $apiBase . $uri;
    if (!empty($_SERVER['QUERY_STRING'])) {
        $target .= '?' . $_SERVER['QUERY_STRING'];
    }

    $method = $_SERVER['REQUEST_METHOD'] ?? 'GET';
    $headers = [];

    if (function_exists('getallheaders')) {
        foreach (getallheaders() as $name => $value) {
            $lower = strtolower($name);
            if (in_array($lower, ['host', 'connection', 'content-length'], true)) {
                continue;
            }
            $headers[] = "{$name}: {$value}";
        }
    } elseif (!empty($_SERVER['HTTP_AUTHORIZATION'])) {
        $headers[] = 'Authorization: ' . $_SERVER['HTTP_AUTHORIZATION'];
    }

    $body = in_array($method, ['POST', 'PUT', 'PATCH', 'DELETE'], true)
        ? file_get_contents('php://input')
        : null;

    $ch = curl_init($target);
    curl_setopt_array($ch, [
        CURLOPT_CUSTOMREQUEST => $method,
        CURLOPT_RETURNTRANSFER => true,
        CURLOPT_HEADER => true,
        CURLOPT_HTTPHEADER => $headers,
        CURLOPT_POSTFIELDS => $body,
        CURLOPT_FOLLOWLOCATION => true,
        CURLOPT_TIMEOUT => 60,
    ]);

    $response = curl_exec($ch);

    if ($response === false) {
        http_response_code(502);
        header('Content-Type: application/json; charset=utf-8');
        echo json_encode([
            'success' => false,
            'message' => 'تعذر الاتصال بالـ API. تأكد من تشغيل Backend على ' . $apiBase,
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }

    $status = (int) curl_getinfo($ch, CURLINFO_HTTP_CODE);
    $headerSize = (int) curl_getinfo($ch, CURLINFO_HEADER_SIZE);
    curl_close($ch);

    $rawHeaders = substr($response, 0, $headerSize);
    $responseBody = substr($response, $headerSize);

    http_response_code($status);

    foreach (explode("\r\n", $rawHeaders) as $headerLine) {
        if ($headerLine === '' || stripos($headerLine, 'HTTP/') === 0) {
            continue;
        }
        if (stripos($headerLine, 'Transfer-Encoding:') === 0) {
            continue;
        }
        header($headerLine, false);
    }

    echo $responseBody;
    exit;
}

// ملفات ثابتة من dist
$filePath = $distDir . $uri;
if ($uri !== '/' && is_file($filePath)) {
    return false;
}

// SPA fallback
$index = $distDir . '/index.html';
if (is_file($index)) {
    header('Content-Type: text/html; charset=utf-8');
    readfile($index);
    exit;
}

http_response_code(404);
echo '404 Not Found';
