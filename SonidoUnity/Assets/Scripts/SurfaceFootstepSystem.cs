using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceFootstepSystem : MonoBehaviour
{
    [System.Serializable]
    public class SurfaceSettings
    {
        public string tag;
        public AudioClip[] sounds;
        [Range(0.5f, 1.5f)] public float minPitch = 0.9f;
        [Range(0.5f, 1.5f)] public float maxPitch = 1.1f;
        [Range(0.1f, 1f)] public float volume = 0.8f;
        [Tooltip("Distancia en metros entre pasos")] public float stepLength = 0.5f;
        [Tooltip("Ajusta velocidad de reproducción")] public float speedMultiplier = 1f;
    }

    [Header("Surface Configuration")]
    public SurfaceSettings[] surfaceSettings;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    [Tooltip("Distancia para detectar superficie")] public float raycastDistance = 1.5f;

    private Vector3 lastPosition;
    private float distanceMoved;
    private SurfaceSettings currentSurface;
    private bool isGrounded;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // Sonido 3D completo
            audioSource.playOnAwake = false;
        }

        lastPosition = transform.position;
    }

    void Update()
    {
        CheckGroundAndSurface();
        HandleFootsteps();
    }

    void CheckGroundAndSurface()
    {
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance);

        if (isGrounded)
        {
            foreach (var surface in surfaceSettings)
            {
                if (hit.collider.CompareTag(surface.tag))
                {
                    currentSurface = surface;
                    return;
                }
            }
        }
        currentSurface = null;
    }

    void HandleFootsteps()
    {
        if (!isGrounded || currentSurface == null) return;

        float actualMovement = Vector3.Distance(transform.position, lastPosition);
        distanceMoved += actualMovement * currentSurface.speedMultiplier;
        lastPosition = transform.position;

        if (distanceMoved >= currentSurface.stepLength)
        {
            PlayFootstep();
            distanceMoved = 0f;
        }
    }

    void PlayFootstep()
    {
        if (currentSurface.sounds.Length == 0) return;

        AudioClip clip = currentSurface.sounds[Random.Range(0, currentSurface.sounds.Length)];
        audioSource.pitch = Random.Range(currentSurface.minPitch, currentSurface.maxPitch);
        audioSource.volume = currentSurface.volume;

        // Efectos especiales por superficie
        if (currentSurface.tag == "Water")
        {
            audioSource.pitch *= 0.9f;
            audioSource.volume *= 0.7f;
        }
        else if (currentSurface.tag == "Wood")
        {
            audioSource.pitch *= 1.1f;
        }

        audioSource.PlayOneShot(clip);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? (currentSurface != null ? Color.green : Color.yellow) : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * raycastDistance);
    }
}