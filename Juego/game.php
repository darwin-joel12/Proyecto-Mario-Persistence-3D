<?php
// Silenciar cualquier advertencia interna de PHP para garantizar que la salida sea JSON puro
error_reporting(0);
ini_set('display_errors', 0);

// Configuración de encabezados HTTP para transferencia JSON segura y soporte CORS
header("Content-Type: application/json");
header("Access-Control-Allow-Origin: *");
header("Access-Control-Allow-Methods: GET, POST");
header("Access-Control-Allow-Headers: Content-Type");

// Parámetros de conexión a la base de datos local de XAMPP
define('DB_HOST', 'localhost');
define('DB_USER', 'root');
define('DB_PASS', ''); 
define('DB_NAME', 'juego_bd');

$conexion = new mysqli(DB_HOST, DB_USER, DB_PASS, DB_NAME);

if ($conexion->connect_error) {
    die(json_encode(["error" => "Error de conexión con la Base de Datos: " . $conexion->connect_error]));
}

$metodo = $_SERVER['REQUEST_METHOD'];

switch ($metodo) {
    case 'GET':
        // 1. EVALUAR SI LA PETICIÓN SOLICITA EL RANKING GLOBAL
        if (isset($_GET["action"]) && $_GET["action"] === "ranking") {
            $sql = "SELECT jugador_nombre, puntuacion FROM partidas_guardadas ORDER BY puntuacion DESC LIMIT 10";
            $resultado = $conexion->query($sql);
            
            $ranking = [];
            if ($resultado && $resultado->num_rows > 0) {
                while ($fila = $resultado->fetch_assoc()) {
                    $ranking[] = [
                        "nombre" => $fila["jugador_nombre"],
                        "puntos" => intval($fila["puntuacion"])
                    ];
                }
            }
            echo json_encode($ranking);
            break;
        }

        // 2. CARGA NORMAL DE JUGADOR (CORREGIDO: Incluye posicion_y y rotacion_y)
        $jugador_id = isset($_GET["jugador_id"]) ? $conexion->real_escape_string($_GET["jugador_id"]) : null;
        
        if (!$jugador_id) {
            echo json_encode(["error" => "Parámetro inválido: Se requiere jugador_id"]);
            break;
        }

        $sql = "SELECT jugador_id, jugador_nombre, puntuacion, posicion_x, posicion_y, posicion_z, rotacion_y, vida, nivel, tiempo_juego, inventario 
                FROM partidas_guardadas WHERE jugador_id = '$jugador_id'";
        $resultado = $conexion->query($sql);

        if ($resultado && $resultado->num_rows > 0) {
            $datos = $resultado->fetch_assoc();
            
            $datos['puntuacion'] = intval($datos['puntuacion']);
            $datos['posicion_x'] = floatval($datos['posicion_x']);
            $datos['posicion_y'] = floatval($datos['posicion_y']); 
            $datos['posicion_z'] = floatval($datos['posicion_z']);
            $datos['rotacion_y'] = floatval($datos['rotacion_y']); 
            $datos['vida'] = intval($datos['vida']);
            $datos['nivel'] = intval($datos['nivel']);
            $datos['tiempo_juego'] = floatval($datos['tiempo_juego']);
            $datos['inventario'] = $datos['inventario'] ? $datos['inventario'] : "Vacio";
            
            echo json_encode($datos);
        } else {
            echo json_encode([
                "jugador_id" => $jugador_id,
                "jugador_nombre" => "Nuevo Jugador",
                "puntuacion" => 0,
                "posicion_x" => 0.0,
                "posicion_y" => 0.0,
                "posicion_z" => 0.0,
                "rotacion_y" => 0.0,
                "vida" => 100,
                "nivel" => 1,
                "tiempo_juego" => 0.0,
                "inventario" => "Vacio"
            ]);
        }
        break;

    case 'POST':
        $entrada = file_get_contents("php://input");
        $datos = json_decode($entrada, true);
        
        if ($datos === null) {
            echo json_encode(["error" => "Estructura JSON corrupta o inválida"]);
            break;
        }

        // CORREGIDO: Extracción de nuevas variables 3D
        $tiempo_juego = floatval($datos["tiempo_juego"]);
        $jugador_id = $conexion->real_escape_string($datos["jugador_id"]);
        $jugador_nombre = $conexion->real_escape_string($datos["jugador_nombre"]);
        $puntuacion = intval($datos["puntuacion"]);
        $posicion_x = floatval($datos["posicion_x"]);
        $posicion_y = floatval($datos["posicion_y"]);
        $posicion_z = floatval($datos["posicion_z"]);
        $rotacion_y = floatval($datos["rotacion_y"]);
        $vida = intval($datos["vida"]);
        $nivel = intval($datos["nivel"]);
        $inventario = $conexion->real_escape_string($datos["inventario"]);

        // CORREGIDO: Inserción que incluye posicion_y y rotacion_y
        $sql = "INSERT INTO partidas_guardadas 
                (jugador_id, jugador_nombre, puntuacion, posicion_x, posicion_y, posicion_z, rotacion_y, vida, nivel, tiempo_juego, inventario) 
                VALUES ('$jugador_id', '$jugador_nombre', $puntuacion, $posicion_x, $posicion_y, $posicion_z, $rotacion_y, $vida, $nivel, $tiempo_juego, '$inventario') 
                ON DUPLICATE KEY UPDATE 
                jugador_nombre = '$jugador_nombre', 
                puntuacion = $puntuacion, 
                posicion_x = $posicion_x, 
                posicion_y = $posicion_y, 
                posicion_z = $posicion_z, 
                rotacion_y = $rotacion_y, 
                vida = $vida, 
                nivel = $nivel,
                tiempo_juego = $tiempo_juego,
                inventario = '$inventario'";

        if ($conexion->query($sql)) {
            echo json_encode(["success" => true, "mensaje" => "Progreso sincronizado correctamente"]);
        } else {
            echo json_encode(["error" => "Error al ejecutar inserción en base de datos: " . $conexion->error]);
        }
        break;
}

$conexion->close();
?>