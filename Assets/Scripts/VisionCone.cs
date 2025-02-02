using UnityEngine;

// Este script define el comportamiento de un cono de visión que detecta objetos
// dentro de un rango y ángulo específicos, y comunica dicha detección al sistema
// de comportamientos de movimiento (SteeringBehaviors).
[RequireComponent(typeof(SteeringBehaviors))]
public class VisionCone : MonoBehaviour
{
    [Header("Parámetros de Visión")]
    [SerializeField] private float visionRange = 10f; // Rango en unidades dentro del cual el agente puede detectar objetos.
    [SerializeField] private float visionAngle = 45f; // Ángulo del cono de visión, medido en grados.
    [SerializeField] private Color detectionColor = Color.red; // Color del Gizmo cuando un objeto ha sido detectado.
    [SerializeField] private Color idleColor = Color.green;    // Color del Gizmo cuando no se detecta ningún objeto.
    [SerializeField] private LayerMask detectableLayers;       // Capas a tomar en cuenta para detección (como "Target").

    [Header("Parámetros de Movimiento")]
    [SerializeField] private SteeringBehaviors steeringBehaviors; // Referencia a los comportamientos de movimiento.

    private GameObject detectedObject = null; // Referencia al objeto detectado actualmente.

    /// <summary>
    /// Dibuja el cono de visión y su estado en el modo de edición y en tiempo de ejecución.
    /// Esto se utiliza únicamente como una ayuda visual en la escena.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Cambia el color del Gizmo dependiendo de si hay un objeto detectado o no.
        Gizmos.color = (detectedObject != null) ? detectionColor : idleColor;

        // Obtén el vector hacia adelante del objeto, escalado por el rango de visión.
        Vector3 forward = transform.forward * visionRange;

        // Calcula los límites derecho e izquierdo del cono según el ángulo especificado.
        Vector3 rightLimit = Quaternion.Euler(0, visionAngle / 2, 0) * forward;
        Vector3 leftLimit = Quaternion.Euler(0, -visionAngle / 2, 0) * forward;

        // Dibuja las líneas visuales del cono.
        Gizmos.DrawLine(transform.position, transform.position + rightLimit);
        Gizmos.DrawLine(transform.position, transform.position + leftLimit);

        // Dibuja una esfera que representa el rango de visión general.
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

    /// <summary>
    /// Se llama en un ciclo fijo (usado para cálculos más precisos relacionados con la física).
    /// Este método verifica continuamente si hay un objeto dentro del cono de visión.
    /// </summary>
    private void FixedUpdate()
    {
        // Detecta el objeto más cercano dentro del rango y ángulo de visión.
        detectedObject = DetectObjectInVision();

        if (detectedObject != null)
        {
            // Si hay un objeto detectado, se pasa al sistema de movimientos.
            steeringBehaviors.SetEnemyReference(detectedObject);
        }
        else
        {
            // Si no hay un objeto en el cono, se resetea la referencia en movimiento.
            steeringBehaviors.SetEnemyReference(null);
        }
    }

    /// <summary>
    /// Detecta los objetos dentro del rango y el ángulo de visión definidos.
    /// Devuelve el objeto más cercano al agente actual.
    /// </summary>
    private GameObject DetectObjectInVision()
    {
        GameObject closestObject = null; // Objeto más cercano detectado.
        float minDistance = visionRange; // Máxima distancia del rango de detección.

        // Busca todos los colliders dentro de la esfera del rango, limitado a las capas configuradas.
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange, detectableLayers);

        foreach (var hit in hits)
        {
            // Calcula la dirección al objetivo desde el agente.
            Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;

            // Calcula el ángulo entre la dirección hacia adelante del agente y la dirección al objetivo.
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            
            if (angleToTarget <= visionAngle / 2) // Verifica si el objetivo está dentro del ángulo del cono.
            {
                // Calcula la distancia entre el agente y el objetivo.
                float distance = Vector3.Distance(transform.position, hit.transform.position);

                // Si está más cerca que el actual "más cercano", actualízalo.
                if (distance < minDistance)
                {
                    closestObject = hit.gameObject;
                    minDistance = distance;
                }
            }
        }

        // Devuelve el objeto más cercano dentro del rango y ángulo de visión.
        return closestObject;
    }
}