using UnityEngine;

public class SincronizarCuerpo : MonoBehaviour
{
    public CharacterController characterController;
    public Transform camaraHeadset;

    // Ajustes mínimos para que no se rompa si la cámara está muy baja
    public float alturaMinima = 0.5f;
    public float alturaMaxima = 2.5f;

    void FixedUpdate()
    {
        SincronizarAltura();
    }

    void SincronizarAltura()
    {
        // Obtenemos la altura de la cámara respecto al suelo del XR Origin
        float alturaActual = Mathf.Clamp(camaraHeadset.localPosition.y, alturaMinima, alturaMaxima);

        // 1. Ajustamos el centro de la cápsula
        // El centro siempre debe estar a la mitad de la altura
        Vector3 centro = characterController.center;
        centro.y = alturaActual / 2;
        characterController.center = centro;

        // 2. Ajustamos la altura total de la cápsula
        characterController.height = alturaActual;
    }
}
