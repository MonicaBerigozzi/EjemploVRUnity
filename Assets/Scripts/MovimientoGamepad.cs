using UnityEngine;

public class MovimientoGamepad : MonoBehaviour
{
    private CharacterController controller;
    public Transform camaraVR;

    [Header("Configuración")]
    public float velocidad = 3.0f;
    public float gravedad = -15.0f;
    public float alturaSalto = 1.5f;

    [Header("Controles Táctiles")]
    public bool usarControlesTactiles = true;
    public float sensibilidadTactil = 0.01f;

    [Header("Detección de Techo")]
    public float alturaJugador = 1.8f; // Altura del jugador completo
    public float margenSeguridad = 0.3f; // Espacio extra necesario

    // Variables internas
    private Vector3 velocidadCaida;
    private bool estaEnElSuelo;
    private float tiempoEsperaSalto = 0.0f;

    // Para controles táctiles
    private Vector2 touchInicial;
    private bool estaArrastrando = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller == null) return;

        // --- 1. GESTIÓN DEL SUELO ---
        if (tiempoEsperaSalto > 0)
        {
            tiempoEsperaSalto -= Time.deltaTime;
            estaEnElSuelo = false;
        }
        else
        {
            estaEnElSuelo = controller.isGrounded;
        }

        if (estaEnElSuelo && velocidadCaida.y < 0)
        {
            velocidadCaida.y = -2f;
        }

        // --- 2. INPUT DE MOVIMIENTO ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        bool botonSaltoPresionado = Input.GetButtonDown("Jump");

        // --- 3. MOVIMIENTO TÁCTIL ---
        if (usarControlesTactiles && x == 0 && z == 0 && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            bool esZonaMovimiento = touch.position.x < Screen.width / 2 &&
                                   touch.position.y < Screen.height * 0.7f;

            if (esZonaMovimiento)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    touchInicial = touch.position;
                    estaArrastrando = true;
                }
                else if (touch.phase == TouchPhase.Moved && estaArrastrando)
                {
                    Vector2 delta = touch.position - touchInicial;
                    x = delta.x * sensibilidadTactil;
                    z = delta.y * sensibilidadTactil;

                    x = Mathf.Clamp(x, -1f, 1f);
                    z = Mathf.Clamp(z, -1f, 1f);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    estaArrastrando = false;
                }
            }
            else if (touch.phase == TouchPhase.Began && estaEnElSuelo)
            {
                // Salto táctil ajustado
                IntentarSaltar();
            }
        }

        // --- 4. CALCULO DE DIRECCIÓN ---
        Vector3 forward = camaraVR != null ? camaraVR.forward : transform.forward;
        Vector3 right = camaraVR != null ? camaraVR.right : transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 movimientoHorizontal = forward * z + right * x;

        // --- 5. SALTO CON GAMEPAD ---
        if (botonSaltoPresionado && estaEnElSuelo)
        {
            IntentarSaltar();
        }

        // --- 6. GRAVEDAD ---
        velocidadCaida.y += gravedad * Time.deltaTime;

        // --- 7. DETENER SI COLISIONA ARRIBA ---
        if (velocidadCaida.y > 0)
        {
            RaycastHit hit;
            Vector3 posicionCabeza = transform.position + Vector3.up * alturaJugador;

            if (Physics.Raycast(posicionCabeza, Vector3.up, out hit, 0.3f))
            {
                velocidadCaida.y = 0;
            }
        }

        // --- 8. MOVIMIENTO FINAL ---
        Vector3 movimientoFinal = (movimientoHorizontal * velocidad) + velocidadCaida;
        CollisionFlags flags = controller.Move(movimientoFinal * Time.deltaTime);

        // Si colisionó arriba, detener
        if ((flags & CollisionFlags.Above) != 0)
        {
            velocidadCaida.y = Mathf.Min(velocidadCaida.y, 0);
        }
    }

    // Método mejorado para intentar saltar
    void IntentarSaltar()
    {
        // Calcular desde dónde está la cabeza del jugador
        Vector3 posicionCabeza = transform.position + Vector3.up * alturaJugador;

        // Verificar cuánto espacio hay arriba
        RaycastHit hit;
        float espacioDisponible = alturaSalto + margenSeguridad;

        if (Physics.Raycast(posicionCabeza, Vector3.up, out hit, espacioDisponible))
        {
            // Hay un techo cerca
            float espacioReal = hit.distance;

            if (espacioReal < margenSeguridad)
            {
                // Muy poco espacio, no saltar
                Debug.Log("No hay espacio suficiente para saltar");
                return;
            }
            else
            {
                // Hay espacio pero limitado, ajustar altura del salto
                float alturaPermitida = espacioReal - margenSeguridad;
                float velocidadAjustada = Mathf.Sqrt(alturaPermitida * -2f * gravedad);
                velocidadCaida.y = Mathf.Min(velocidadAjustada, Mathf.Sqrt(alturaSalto * -2f * gravedad));
                tiempoEsperaSalto = 0.2f;

                Debug.Log($"Salto ajustado: {alturaPermitida}m disponible");
            }
        }
        else
        {
            // Espacio libre, salto normal
            velocidadCaida.y = Mathf.Sqrt(alturaSalto * -2f * gravedad);
            tiempoEsperaSalto = 0.2f;
        }
    }
}