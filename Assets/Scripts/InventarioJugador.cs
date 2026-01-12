using UnityEngine;
using TMPro; // NECESARIO para usar texto nuevo
using System.Collections; // Necesario para la cuenta atrás (borrar texto)
//using UnityEngine.UI;

public class InventarioJugador : MonoBehaviour
{
    public bool tieneLlave = false;
    public float duracionMensaje = 5.0f;
    // Aquí arrastraremos tu texto del Canvas
    //public TextMeshProUGUI textoNotificaciones;
    //public Text textoNotificaciones;
    public TMP_Text textoNotificaciones;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Llave"))
        {
            tieneLlave = true;

            // Mostrar mensaje
            MostrarMensaje("¡Llave recogida!");

            // Sonido (Opcional)
            // GetComponent<AudioSource>().Play();

            Destroy(other.gameObject);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Puerta"))
        {
            if (tieneLlave)
            {
                MostrarMensaje("¡Abriendo puerta!");
                hit.gameObject.SetActive(false);
            }
            else
            {
                MostrarMensaje("Está cerrada. Busca la llave.");
            }
        }
    }

    // Función auxiliar para escribir y borrar a los 3 segundos
    void MostrarMensaje(string mensaje)
    {
        if (textoNotificaciones != null)
        {
            textoNotificaciones.text = mensaje;
            StopAllCoroutines(); // Reinicia el contador si ya había uno
            StartCoroutine(BorrarTextoDespuesDe(duracionMensaje));
        }
    }

    // Rutina de espera
    IEnumerator BorrarTextoDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        textoNotificaciones.text = ""; // Borra el texto
    }
}