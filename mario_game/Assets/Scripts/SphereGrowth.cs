using System.Collections;
using UnityEngine;

public class SphereGrowth : MonoBehaviour
{
    public GameObject spherePrefab;
    public Transform spawnPoint;
    public float growthRate = 0.1f; // Velocidad
    public float growthMultiplier = 5f; // Crecimiento
    public float maxScale = 5f; // Tamaño máximo
    public float shootForce = 500f; // Fuerza
    public float respawnDelay = 2f; // Tiempo de espera
    public Vector3 initialScale = Vector3.one * 0.1f; // Tamaño inicial

    private GameObject currentSphere;
    private Rigidbody sphereRb;
    private Vector3 lastPosition;
    private bool isRespawning = false;

    void Start()
    {
        lastPosition = transform.position;
        currentSphere = GameObject.FindGameObjectWithTag("Sphere");
        if (currentSphere != null)
        {
            SetupSphere(currentSphere);
        }
        else
        {
            Debug.LogWarning("No se encontró una esfera inicial. Será creada cuando el personaje se mueva.");
        }
    }

    void Update()
    {
        if (currentSphere == null && !isRespawning && Vector3.Distance(transform.position, lastPosition) > 0)
        {
            StartCoroutine(RespawnSphere());
        }

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
            Vector3 growth = Vector3.one * growthRate * growthMultiplier * Time.deltaTime;
            Vector3 newScale = currentSphere.transform.localScale + growth;

            if (newScale.x <= maxScale && newScale.y <= maxScale && newScale.z <= maxScale)
            {
                currentSphere.transform.localScale = newScale;
            }
        }
    }

    void HandleShoot()
    {
        if (Input.GetKeyDown(KeyCode.F) && currentSphere != null)
        {
            sphereRb.isKinematic = false;
            sphereRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            currentSphere.transform.SetParent(null);
            sphereRb.AddForce(transform.forward * shootForce);

            Debug.Log("Lanzando y destruyendo esfera: " + currentSphere.name);
            Destroy(currentSphere, 3f);
            currentSphere = null;
        }
    }

    IEnumerator RespawnSphere()
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);

        if (currentSphere == null)
        {
            Debug.Log("Creando nueva esfera en el spawn point.");
            currentSphere = Instantiate(spherePrefab, spawnPoint.position, Quaternion.identity);
            SetupSphere(currentSphere);
        }

        isRespawning = false;
    }

    void SetupSphere(GameObject sphere)
    {
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
