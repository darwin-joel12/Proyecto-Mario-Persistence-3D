using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ControladorMario : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidadMovimiento = 5f;
    public float velocidadRotacion = 360f;

    [Header("Configuración de Base de Datos")]
    [SerializeField] private bool autoGuardar = true;
    [SerializeField] private float intervaloAutoGuardado = 10f;
    [SerializeField] private float distanciaAutoGuardado = 5f;

    [Header("Estadísticas del Jugador")]
    private DataBaseManager dbManager;
    private string jugadorId;
    private string jugadorNombre = "Mario";
    private int puntuacion = 0;
    private int vida = 100;
    private int nivel = 1;
    private float tiempoTranscurrido = 0f;
    private string inventario = "Vacio";

    [Header("Configuración del Ranking (Pregunta 7)")]
    private string rankingTexto = "Presiona T para ver el Ranking";
    private bool mostrandoRanking = false;

    private Vector3 ultimaPosicionGuardada;
    private float temporizadorAutoGuardado = 0f;

    private Animator animator;
    private Rigidbody rb;
    private float inputX, inputZ;
    private Vector3 direccionMovimiento;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        Debug.Log("=== INICIANDO JUEGO ===");
        dbManager = DataBaseManager.Instancia;
        CargarOCrearIdJugador();
        CargarPartida();
        ultimaPosicionGuardada = transform.position;
    }

    void Update()
    {
        // Acumular tiempo de juego transcurrido (Pregunta 5)
        tiempoTranscurrido += Time.deltaTime;

        // Capturar entrada exacta del Ingeniero
        Vector2 entrada = Vector2.zero;
        var teclado = Keyboard.current;
        if (teclado != null)
        {
            if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed) entrada.y = 1f;
            if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed) entrada.y = -1f;
            if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) entrada.x = 1f;
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) entrada.x = -1f;
        }

        inputX = entrada.x;
        inputZ = entrada.y;

        // Calcular dirección de movimiento original del Ingeniero (Movimiento local)
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        direccionMovimiento = (forward * inputZ + right * inputX).normalized;

        // Animaciones originales del Ingeniero (Mapeo directo de entrada para el Blend Tree)
        if (animator != null)
        {
            float suavizado = Time.deltaTime * 10f;
            float xVal = Mathf.Lerp(animator.GetFloat("xVal"), inputX, suavizado);
            float yVal = Mathf.Lerp(animator.GetFloat("yVal"), inputZ, suavizado);
            animator.SetFloat("xVal", xVal);
            animator.SetFloat("yVal", yVal);
        }

        // Auto guardado utilizando las variables correctas
        if (autoGuardar && dbManager != null)
        {
            temporizadorAutoGuardado += Time.deltaTime;
            if (temporizadorAutoGuardado >= intervaloAutoGuardado)
            {
                temporizadorAutoGuardado = 0f;
                GuardarPartida("Auto-guardado por tiempo");
            }

            float distanciaMovida = Vector3.Distance(transform.position, ultimaPosicionGuardada);
            if (distanciaMovida >= distanciaAutoGuardado)
            {
                ultimaPosicionGuardada = transform.position;
                GuardarPartida("Auto-guardado por distancia");
            }
        }

        // Controles de prueba
        if (teclado != null)
        {
            if (teclado.gKey.wasPressedThisFrame) GuardarPartida("Manual");
            if (teclado.lKey.wasPressedThisFrame) CargarPartida();
            if (teclado.cKey.wasPressedThisFrame) MostrarEstado();
            if (teclado.xKey.wasPressedThisFrame) SumarPuntos(10);
            if (teclado.zKey.wasPressedThisFrame) RecibirDanio(10);
            if (teclado.rKey.wasPressedThisFrame) Curar(10);
            if (teclado.iKey.wasPressedThisFrame) AgregarAlInventario("Moneda");

            // DETECCIÓN DE LA TECLA T PARA CARGAR EL RANKING GLOBAL (PREGUNTA 7)
            if (teclado.tKey.wasPressedThisFrame)
            {
                mostrandoRanking = !mostrandoRanking;
                if (mostrandoRanking)
                {
                    rankingTexto = "Cargando Ranking...";
                    StartCoroutine(dbManager.CargarRanking((json) =>
                    {
                        if (!string.IsNullOrEmpty(json))
                        {
                            rankingTexto = "=== TOP 10 JUGADORES ===\n\n";
                            string limpiador = json.Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Replace("\"", "");
                            string[] entradas = limpiador.Split(',');
                            int puesto = 1;
                            for (int i = 0; i < entradas.Length - 1; i += 2)
                            {
                                if (entradas[i].Contains("nombre") && entradas[i + 1].Contains("puntos"))
                                {
                                    string nombre = entradas[i].Split(':')[1];
                                    string puntos = entradas[i + 1].Split(':')[1];
                                    rankingTexto += $"{puesto}. {nombre} - {puntos} pts\n";
                                    puesto++;
                                }
                            }
                        }
                        else
                        {
                            rankingTexto = "Error al conectar con el servidor.";
                        }
                    }));
                }
            }
        }
    }

    void FixedUpdate()
    {
        // Lógica física de traslación y rotación original del Ingeniero
        if (direccionMovimiento.magnitude > 0.01f)
        {
            float dt = Time.fixedDeltaTime;
            Vector3 desplazamiento = direccionMovimiento * velocidadMovimiento * dt;
            if (rb != null)
            {
                rb.MovePosition(rb.position + desplazamiento);

                if (inputZ >= 0)
                {
                    Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionMovimiento);
                    rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, rotacionObjetivo, velocidadRotacion * dt));
                }
                else if (Mathf.Abs(inputX) > 0.01f)
                {
                    float giro = inputX * velocidadRotacion * dt;
                    rb.MoveRotation(rb.rotation * Quaternion.Euler(0, giro, 0));
                }
            }
        }
    }

    private void CargarOCrearIdJugador()
    {
        if (PlayerPrefs.HasKey("JugadorId"))
        {
            jugadorId = PlayerPrefs.GetString("JugadorId");
            Debug.Log($"ID del jugador cargado localmente: {jugadorId}");
        }
        else
        {
            jugadorId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("JugadorId", jugadorId);
            PlayerPrefs.Save();
            Debug.Log($"Nuevo ID único generado y persistido localmente: {jugadorId}");
        }
    }

    private void GuardarPartida(string motivo)
    {
        if (dbManager == null) return;

        GameData datos = new GameData();
        datos.jugador_id = jugadorId;
        datos.jugador_nombre = jugadorNombre;
        datos.puntuacion = puntuacion;

        // CORREGIDO: Guardado de coordenadas 3D completas y rotación Y
        datos.posicion_x = transform.position.x;
        datos.posicion_y = transform.position.y; // Nueva altura Y
        datos.posicion_z = transform.position.z;
        datos.rotacion_y = transform.eulerAngles.y; // Nueva rotación Y

        datos.vida = vida;
        datos.nivel = nivel;
        datos.tiempo_juego = tiempoTranscurrido;
        datos.inventario = inventario;

        dbManager.GuardarPartida(datos);
        Debug.Log($"Partida guardada ({motivo}) - Pos: ({datos.posicion_x:F1}, {datos.posicion_y:F1}, {datos.posicion_z:F1}) | Rot Y: {datos.rotacion_y:F0}°");
    }

    private void CargarPartida()
    {
        if (dbManager == null) return;

        StartCoroutine(dbManager.CargarPartida(jugadorId, (datos) =>
        {
            if (datos != null && !string.IsNullOrEmpty(datos.jugador_id))
            {
                jugadorNombre = datos.jugador_nombre;
                puntuacion = datos.puntuacion;
                vida = datos.vida;
                nivel = datos.nivel;
                tiempoTranscurrido = datos.tiempo_juego;
                inventario = string.IsNullOrEmpty(datos.inventario) ? "Vacio" : datos.inventario;

                // --- CAMBIO 1: Ajuste de suelo ---
                // Sumamos un pequeño valor (ej. 0.1f) para evitar que se hunda en el colisionador del suelo
                float ajusteSuelo = 0.1f;
                Vector3 posicion = new Vector3(datos.posicion_x, datos.posicion_y + ajusteSuelo, datos.posicion_z);

                // --- CAMBIO 2: Reset de físicas antes de mover ---
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;        // Detiene cualquier caída o movimiento previo
                    rb.angularVelocity = Vector3.zero; // Detiene cualquier rotación previa
                    rb.isKinematic = true;             // Desactiva físicas momentáneamente para teletransportar
                }

                transform.position = posicion;
                transform.rotation = Quaternion.Euler(0f, datos.rotacion_y, 0f);

                ultimaPosicionGuardada = posicion;

                // Reactivamos físicas después de posicionar
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.position = posicion;
                    rb.rotation = Quaternion.Euler(0f, datos.rotacion_y, 0f);
                }

                Debug.Log($"Partida cargada correctamente. Pos: {posicion} | Rot: {datos.rotacion_y:F0}°");
            }
            else
            {
                Debug.Log("Nueva partida detectada en el Servidor");
            }
        }));
    }
    public void SumarPuntos(int puntos)
    {
        puntuacion += puntos;
        Debug.Log($"+{puntos} puntos! Total: {puntuacion}");
        GuardarPartida("Suma de puntos");
    }

    public void RecibirDanio(int danio)
    {
        vida = Mathf.Max(0, vida - danio);
        Debug.Log($"Daño: -{danio} HP. Vida restante: {vida}");
        GuardarPartida("Daño recibido");
    }

    public void Curar(int cantidad)
    {
        vida = Mathf.Min(100, vida + cantidad);
        Debug.Log($"Curación: +{cantidad} HP. Vida actual: {vida}");
        GuardarPartida("Curación");
    }

    public void AgregarAlInventario(string item)
    {
        if (inventario == "Vacio" || string.IsNullOrEmpty(inventario))
        {
            inventario = item;
        }
        else
        {
            inventario += "," + item;
        }
        Debug.Log($"Item agregado al inventario: {item}");
        GuardarPartida("Item recolectado");
    }

    private void MostrarEstado()
    {
        Debug.Log($"ID: {jugadorId} | Nombre: {jugadorNombre} | Puntos: {puntuacion} | Vida: {vida} | Nivel: {nivel} | Tiempo: {tiempoTranscurrido:F2} | Inv: {inventario}");
    }

    void OnGUI()
    {
        GUIStyle estiloBox = new GUIStyle(GUI.skin.box);
        estiloBox.fontSize = 20;
        estiloBox.fontStyle = FontStyle.Bold;

        GUIStyle estiloLabel = new GUIStyle(GUI.skin.label);
        estiloLabel.fontSize = 18;

        // Cuadrícula de estadísticas grande y legible
        GUI.Box(new Rect(10, 10, 260, 210), "=== MARIO ===", estiloBox);
        GUI.Label(new Rect(25, 45, 230, 28), $"PUNTOS: {puntuacion}", estiloLabel);
        GUI.Label(new Rect(25, 75, 230, 28), $"VIDA: {vida}", estiloLabel);
        GUI.Label(new Rect(25, 105, 230, 28), $"NIVEL: {nivel}", estiloLabel);
        GUI.Label(new Rect(25, 135, 230, 28), $"TIEMPO: {tiempoTranscurrido:F1}s", estiloLabel);
        GUI.Label(new Rect(25, 165, 230, 28), $"INV: {inventario}", estiloLabel);

        // Cuadrícula de controles
        GUI.Box(new Rect(10, Screen.height - 215, 390, 205), "=== CONTROLES ===", estiloBox);
        GUI.Label(new Rect(25, Screen.height - 175, 360, 30), "WASD/Flechas: Movimiento", estiloLabel);
        GUI.Label(new Rect(25, Screen.height - 140, 360, 30), "G: Guardar | L: Cargar | T: Ranking", estiloLabel);
        GUI.Label(new Rect(25, Screen.height - 105, 360, 30), "X: Puntos | Z: Dano", estiloLabel);
        GUI.Label(new Rect(25, Screen.height - 70, 360, 30), "I: Agregar Moneda al Inv", estiloLabel);

        // RENDEREADO DEL RANKING GLOBAL EN EL EXTREMO DERECHO (LETRA GRANDE)
        if (mostrandoRanking)
        {
            GUI.Box(new Rect(Screen.width - 310, 10, 300, 310), "=== RANKING GLOBAL ===", estiloBox);
            GUI.Label(new Rect(Screen.width - 295, 45, 270, 250), rankingTexto, estiloLabel);
        }
    }
}