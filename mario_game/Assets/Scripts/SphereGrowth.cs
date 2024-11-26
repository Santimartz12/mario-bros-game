using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereGrowth : MonoBehaviour
{
    public GameObject spherePrefab; // Prefab de la esfera
    public Transform spawnPoint;   // Punto de generación
    public float growthRate = 0.1f; // Velocidad de crecimiento
    public float growthMultiplier = 5f; // Multiplicador de crecimiento
    public float maxScale = 5f; // Tamaño máximo
    public float shootForce = 500f; // Fuerza de disparo
    public float sphereLifetime = 5f; // Tiempo antes de desaparecer
    public Vector3 initialScale = Vector3.one * 0.1f; // Tamaño inicial

    private GameObject currentSphere;  // La esfera activa
    private GameObject baseSphere;     // La esfera inmutable (molde)
    private Rigidbody sphereRb;
    private Vector3 lastPosition;

    void Start()
    {
        // Crear la esfera base inmutable
        baseSphere = Instantiate(spherePrefab, spawnPoint.position, Quaternion.identity);
        baseSphere.transform.localScale = initialScale;
        baseSphere.name = "BaseSphere";
        baseSphere.SetActive(false); // Ocultar la esfera base para no interferir en la escena

        // Crear la primera esfera activa a partir de la base
        CreateSphere();
        lastPosition = transform.position;
    }

    void Update()
    {
        // Si no hay una esfera activa, generar una nueva
        if (currentSphere == null && Vector3.Distance(transform.position, lastPosition) > 0)
        {
            CreateSphere();
        }

        // Si hay una esfera activa, manejar su crecimiento y disparo
        if (currentSphere != null)
        {
            HandleSphereGrowth();
            HandleShoot();
        }

        lastPosition = transform.position;
    }

    void HandleSphereGrowth()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        if (distanceMoved > 0)
        {
            // Calcular el crecimiento de la esfera
            Vector3 growth = Vector3.one * growthRate * growthMultiplier * Time.deltaTime;
            Vector3 newScale = currentSphere.transform.localScale + growth;

            if (newScale.x <= maxScale && newScale.y <= maxScale && newScale.z <= maxScale)
            {
                // Ajustar la escala y mantener la base en el suelo
                float previousScale = currentSphere.transform.localScale.y;
                currentSphere.transform.localScale = newScale;

                float scaleDifference = (newScale.y - previousScale) / 2;
                currentSphere.transform.position += new Vector3(0, scaleDifference, 0);
            }
        }
    }

    void HandleShoot()
    {
        if (Input.GetKeyDown(KeyCode.F) && currentSphere != null)
        {
            // Disparar la esfera activa
            sphereRb.isKinematic = false;
            sphereRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            currentSphere.transform.SetParent(null);
            sphereRb.AddForce(transform.forward * shootForce);

            Debug.Log("Lanzando esfera: " + currentSphere.name);
            Destroy(currentSphere, sphereLifetime);
            currentSphere = null;
        }
    }

    void CreateSphere()
    {
        // Crear una nueva esfera a partir de la base
        currentSphere = Instantiate(baseSphere, spawnPoint.position, Quaternion.identity);
        currentSphere.SetActive(true);
        currentSphere.name = "ActiveSphere";
        SetupSphere(currentSphere);
        Debug.Log("Nueva esfera creada.");
    }

    void SetupSphere(GameObject sphere)
    {
        // Configurar las propiedades de la nueva esfera
        sphere.transform.localScale = initialScale;
        sphere.tag = "Sphere";
        sphere.transform.SetParent(spawnPoint);
        sphere.transform.localPosition = Vector3.zero;

        sphereRb = sphere.GetComponent<Rigidbody>();
        if (sphereRb == null)
        {
            sphereRb = sphere.AddComponent<Rigidbody>();
        }

        sphereRb.isKinematic = true;
        sphereRb.useGravity = true;
        sphereRb.constraints = RigidbodyConstraints.FreezeRotation;

        SphereCollider collider = sphere.GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = sphere.AddComponent<SphereCollider>();
        }

        collider.isTrigger = false;
    }
}
