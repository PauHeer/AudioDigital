using UnityEngine;
using System.Collections.Generic;

public class StepsScript : MonoBehaviour
{
    [System.Serializable]
    public class SurfaceType
    {
        public string tag; // "Sand", "Wood", "Water"
        public AudioClip[] footstepSounds;
        [Range(0.1f, 2f)] public float pitchVariation = 0.3f;
        [Range(0.1f, 1f)] public float volume = 1f;
        public float stepDistance = 0.5f; // Distancia entre pasos para esta superficie
    }

    public List<SurfaceType> surfaceTypes = new List<SurfaceType>();
    public AudioSource audioSource;
    public float raycastDistance = 1.5f;

    private CharacterController characterController;
    private float accumulatedDistance;
    private SurfaceType currentSurface;
    private bool isInWater;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1; // Sonido 3D
            audioSource.minDistance = 1;
            audioSource.maxDistance = 10;
        }
    }

    void Update()
    {
        // Reemplaza la comprobación del CharacterController por:
        if (IsMoving() && IsGrounded())
        {
            CheckCurrentSurface();
            accumulatedDistance += GetMovementSpeed() * Time.deltaTime;

            if (accumulatedDistance > currentSurface.stepDistance)
            {
                PlayFootstep();
                accumulatedDistance = 0f;
            }
        }
    }

    // Añade estos métodos auxiliares:
    private bool IsMoving()
    {
        // Adapta según tu sistema de movimiento:
        return Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, raycastDistance);
    }

    private float GetMovementSpeed()
    {
        // Devuelve la velocidad actual de tu personaje
        return 5f; // Ajusta este valor según tu juego
    }

    void CheckCurrentSurface()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance))
        {
            // Verifica si estamos en agua (puedes usar un collider de trigger para zonas de agua)
            isInWater = hit.collider.CompareTag("Water");

            foreach (SurfaceType surface in surfaceTypes)
            {
                if (hit.collider.CompareTag(surface.tag))
                {
                    currentSurface = surface;
                    return;
                }
            }

            currentSurface = null; // Superficie por defecto
        }
    }

    void PlayFootstep()
    {
        if (currentSurface == null || currentSurface.footstepSounds.Length == 0)
            return;

        // Selección aleatoria del sonido
        AudioClip clip = currentSurface.footstepSounds[Random.Range(0, currentSurface.footstepSounds.Length)];

        // Configuración de audio
        audioSource.clip = clip;
        audioSource.pitch = 1f + Random.Range(-currentSurface.pitchVariation, currentSurface.pitchVariation);
        audioSource.volume = currentSurface.volume;

        // Efecto especial para agua
        if (isInWater)
        {
            audioSource.volume *= 0.7f;
            audioSource.pitch *= 0.9f;
        }

        audioSource.Play();
    }

    // Opcional: Para debuguear la superficie actual
    void OnGUI()
    {
        if (currentSurface != null)
        {
            GUI.Label(new Rect(10, 10, 200, 20), "Superficie: " + currentSurface.tag);
            GUI.Label(new Rect(10, 30, 200, 20), "En agua: " + isInWater);
        }
    }
}