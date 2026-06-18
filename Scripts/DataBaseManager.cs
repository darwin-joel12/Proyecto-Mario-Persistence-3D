using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class DataBaseManager : MonoBehaviour
{
    private static DataBaseManager _instancia;
    public static DataBaseManager Instancia
    {
        get
        {
            if (_instancia == null)
            {
                _instancia = FindAnyObjectByType<DataBaseManager>();
                if (_instancia == null)
                {
                    GameObject go = new GameObject("DataBaseManager");
                    _instancia = go.AddComponent<DataBaseManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instancia;
        }
    }

    [SerializeField] private string urlAPI = "http://localhost/Juego/game.php";

    private void Awake()
    {
        if (_instancia != null && _instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        _instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GuardarPartida(GameData datos)
    {
        StartCoroutine(CoroutineGuardar(datos));
    }

    private IEnumerator CoroutineGuardar(GameData datos)
    {
        string json = JsonUtility.ToJson(datos);
        using (UnityWebRequest request = new UnityWebRequest(urlAPI, "POST"))
        {
            byte[] cuerpo = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(cuerpo);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
        }
    }

    public IEnumerator CargarPartida(string jugadorId, System.Action<GameData> alCompletar)
    {
        string url = $"{urlAPI}?jugador_id={jugadorId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string respuesta = request.downloadHandler.text;
                Debug.Log("<color=cyan>RESPUESTA DEL SERVIDOR: </color>" + respuesta);

                try
                {
                    GameData datos = JsonUtility.FromJson<GameData>(respuesta);
                    alCompletar?.Invoke(datos);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error al traducir los datos del servidor (JSON incorrecto): " + ex.Message);
                    alCompletar?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError("Error de conexión con el servidor web: " + request.error);
                alCompletar?.Invoke(null);
            }
        }
    }

    // CORRUTINA AÑADIDA PARA LA PREGUNTA 7: Obtener el ranking global de líderes
    public IEnumerator CargarRanking(System.Action<string> alCompletar)
    {
        string url = $"{urlAPI}?action=ranking";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                alCompletar?.Invoke(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error de conexión al cargar ranking: " + request.error);
                alCompletar?.Invoke(null);
            }
        }
    }
}