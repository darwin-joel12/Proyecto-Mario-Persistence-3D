using UnityEngine;

public class Moneda : MonoBehaviour
{
    [Header("Configuración")]
    public int puntosValor = 10;
    public GameObject efectoRecoger;

    [Header("Animación")]
    public float velocidadRotacion = 100f;
    public float velocidadFlotacion = 2f;
    public float alturaFlotacion = 0.3f;

    private Vector3 posicionInicial;
    private bool recogida = false;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime);

        float nuevaY = posicionInicial.y + Mathf.Sin(Time.time * velocidadFlotacion) * alturaFlotacion;
        transform.position = new Vector3(posicionInicial.x, nuevaY, posicionInicial.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (recogida) return;

        ControladorMario mario = other.GetComponent<ControladorMario>();
        if (mario != null)
        {
            recogida = true;
            mario.SumarPuntos(puntosValor);

            if (efectoRecoger != null)
            {
                Instantiate(efectoRecoger, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}